using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;
using CODESYS.Parser;
using CODESYS.Parser35210.Pragmas;
using CODESYS.Parser35210.Utilities;

namespace CODESYS.Parser35210.Statements
{
	public class StatementParser
	{
		private int _stackDepth;

		private readonly LStack<Tuple<bool, int>> m_BpStack = new LStack<Tuple<bool, int>>();

		private ParserContext Context { get; }

		private PragmaStatementParser PragmaStatementParser => Context.PragmaStatementParser;

		private _ILanguageModelBuilder6 LMItemFactory => Context.LMItemFactory;

		private IScanner9 Scanner => Context.Scanner;

		private IErrorHandler ErrorHandler => Context.ErrorHandler;

		internal bool Flow { get; set; } = true;


		internal bool Bp { get; set; } = true;


		internal bool InCase { get; set; }

		internal bool InDeclaration { get; set; }

		internal bool Implicit { get; set; }

		internal bool ImplicitAnyway { get; set; }

		internal bool ParseInitializationExpression
		{
			get
			{
				if (!Implicit)
				{
					return ImplicitAnyway;
				}
				return true;
			}
		}

		internal bool InsideDeclarationVarDecl { get; set; }

		internal StatementParser(ParserContext context)
		{
			Context = context;
		}

		private TokenType Next(out IToken token)
		{
			return Scanner.Next(out token);
		}

		private TokenType Next(out IToken token, bool bWithPragma, bool bWithComment)
		{
			return Scanner.Next(out token, bWithPragma, bWithComment);
		}

		private void MatchOperator(_IExprement exp, params Operator[] ops)
		{
			Scanner.MatchOperator(ErrorHandler, exp, ops);
		}

		private void AddErrorST(_IExprement exp, MessageId nErrorId, params object[] args)
		{
			ErrorHandler.AddErrorST(exp, nErrorId, args);
		}

		private void AddErrorSTWithToken(_IExprement exp, IToken token, MessageId nErrorId, params object[] args)
		{
			ErrorHandler.AddErrorSTWithToken(exp, token, nErrorId, args);
		}

		internal bool CheckOptionalOperator(Operator op)
		{
			if (Scanner.Next(out var token) != TokenType.Operator || Scanner.GetOperator(token) != op)
			{
				Scanner.SetPosition(token);
				return false;
			}
			return true;
		}

		internal void CheckForOperator(_IExprement exprement, Operator op, bool bErrorLocal, out _IErrorExpression exprError)
		{
			exprError = null;
			if (Scanner.Next(out var token) == TokenType.Operator && Scanner.GetOperator(token) == op)
			{
				return;
			}
			if (!bErrorLocal)
			{
				exprError = LMItemFactory.CreateErrorExpression(token);
				AddErrorSTWithToken(exprement, token, MessageId.Err_OperatorExpected, Scanner.GetOperatorText(op), Scanner.GetTokenText(token));
				Scanner.SetPosition(token);
			}
			if (ParseReSyncST(op) != op)
			{
				if (bErrorLocal)
				{
					AddErrorSTWithToken(exprement, token, MessageId.Err_OperatorExpected, Scanner.GetOperatorText(op), Scanner.GetTokenText(token));
				}
				Scanner.SetPosition(token);
			}
		}

		private Operator ParseReSyncST(params Operator[] ops)
		{
			return Scanner.ParseReSyncST(ops);
		}

		internal Operator ParseReSyncST(out IToken tokenPos, params Operator[] ops)
		{
			Scanner.Next(out tokenPos);
			Scanner.SetPosition(tokenPos);
			return Scanner.ParseReSyncST(ops);
		}

		internal void SetStatementFlags(_IStatement statement)
		{
			statement.SetFlag(StatementFlag.GenerateFlow, Flow);
			statement.SetFlag(StatementFlag.GenerateBP, Bp);
		}

		internal _IStatement ParseSTStatement(out bool bError, bool bTopLevel)
		{
			_stackDepth++;
			Scanner.GetNext(out var token);
			Scanner.SetPosition(token);
			int sourceOffset = Scanner.SourceOffset;
			_IStatement iStatement = ParseSTStatementHelp(out bError, bTopLevel);
			int sourceOffset2 = Scanner.SourceOffset;
			if (iStatement != null)
			{
				_IToken iToken = (_IToken)token;
				long num = ((_IToken)Scanner.CurrentToken).CharactersToSkipSeen - iToken.CharactersToSkipSeen;
				iStatement.PositionLength = (short)(sourceOffset2 - sourceOffset - num);
			}
			_stackDepth--;
			return iStatement;
		}

		private _IStatement ParseSTStatementHelp(out bool bError, bool bTopLevel)
		{
			bool bSemicolonNecessary = true;
			IToken token;
			TokenType tokenType = Next(out token, bWithPragma: true, bWithComment: true);
			bError = false;
			_IStatement iStatement;
			switch (tokenType)
			{
			case TokenType.DocComment:
			{
				string docComment = Scanner.GetDocComment(token);
				_ICommentStatement iCommentStatement = LMItemFactory.CreateCommentStatement(docComment, token);
				iCommentStatement.DocComment = true;
				iStatement = iCommentStatement;
				bSemicolonNecessary = false;
				break;
			}
			case TokenType.Comment:
			{
				string comment = Scanner.GetComment(token);
				iStatement = LMItemFactory.CreateCommentStatement(comment, token);
				bSemicolonNecessary = false;
				break;
			}
			case TokenType.Pragma:
				iStatement = ParseSTStatementHelpPragma(out bError, token);
				bSemicolonNecessary = false;
				break;
			case TokenType.Integer:
				iStatement = TryParseCaseLabelStatement(token, ref bSemicolonNecessary);
				break;
			case TokenType.Identifier:
				iStatement = TryParseDeclarationOrLabel(ref bError, token, ref bSemicolonNecessary);
				break;
			case TokenType.DirectVariable:
			{
				Scanner.SetPosition(token);
				_IExpression exp = ParseAssignExp(out bError);
				iStatement = LMItemFactory.CreateExpressionStatement(exp, token);
				break;
			}
			case TokenType.Operator:
				iStatement = ParseOperatorStatement(ref bError, token, ref bSemicolonNecessary);
				break;
			case TokenType.End:
				if (bTopLevel)
				{
					return null;
				}
				iStatement = LMItemFactory.CreateErrorStatement(token);
				AddErrorSTWithToken(iStatement, token, MessageId.Err_UnexpectedTokenFound, ((_IScanner)Scanner).GetTokenText(token, ETokenTextFlags.TruncateAtLinebreak));
				break;
			default:
				iStatement = LMItemFactory.CreateErrorStatement(token);
				AddErrorSTWithToken(iStatement, token, MessageId.Err_UnexpectedTokenFound, ((_IScanner)Scanner).GetTokenText(token, ETokenTextFlags.TruncateAtLinebreak));
				break;
			}
			ParseSemicolonIfNecessary(bSemicolonNecessary, iStatement);
			iStatement?.SetFlag(StatementFlag.GenerateFlow, Flow);
			iStatement?.SetFlag(StatementFlag.GenerateBP, Bp);
			iStatement?.SetFlag(StatementFlag.GenerateBP2, ShouldCreateBpForCurrentStatement());
			if (iStatement != null && !(iStatement is IPragmaStatement))
			{
				RestoreBp();
			}
			return iStatement;
		}

		internal _IStatement TryParseVariableDeclarationList()
		{
			_IStatement iStatement = null;
			IToken token;
			switch (Next(out token, bWithPragma: true, bWithComment: true))
			{
			case TokenType.Operator:
				iStatement = _TryParseVariableDeclarationList(token);
				break;
			case TokenType.DocComment:
			{
				string docComment = Scanner.GetDocComment(token);
				_ICommentStatement iCommentStatement = LMItemFactory.CreateCommentStatement(docComment, token);
				iCommentStatement.DocComment = true;
				iStatement = iCommentStatement;
				break;
			}
			case TokenType.Comment:
			{
				string comment = Scanner.GetComment(token);
				iStatement = LMItemFactory.CreateCommentStatement(comment, token);
				break;
			}
			case TokenType.Pragma:
			{
				iStatement = ParseSTStatementHelpPragma(out var _, token);
				break;
			}
			}
			if (iStatement == null)
			{
				Scanner.SetPosition(token);
			}
			else
			{
				iStatement.SetFlag(StatementFlag.GenerateFlow, Flow);
				iStatement.SetFlag(StatementFlag.GenerateBP, Bp);
				iStatement.SetFlag(StatementFlag.GenerateBP2, ShouldCreateBpForCurrentStatement());
				if (!(iStatement is IPragmaStatement))
				{
					RestoreBp();
				}
			}
			return iStatement;
		}

		private _IStatement _TryParseVariableDeclarationList(IToken token)
		{
			_IStatement iStatement = null;
			switch (Scanner.GetOperator(token))
			{
			case Operator.Var:
			case Operator.VarConfig:
			case Operator.VarExternal:
			case Operator.VarGlobal:
			case Operator.VarInput:
			case Operator.VarInOut:
			case Operator.VarOutput:
			case Operator.VarTemp:
			case Operator.VarStat:
			case Operator.VarInst:
				iStatement = ParseVariableList(token);
				MatchOperator(iStatement, Operator.EndVar);
				break;
			case Operator.Semicolon:
				iStatement = LMItemFactory.CreateEmptyStatement(token);
				break;
			}
			return iStatement;
		}

		private _IStatement ParseOperatorStatement(ref bool bError, IToken token, ref bool bSemicolonNecessary)
		{
			_IStatement iStatement;
			switch (Scanner.GetOperator(token))
			{
			case Operator.__Copy:
			case Operator.This:
			case Operator.Super:
			case Operator.__Init:
			case Operator.__QueryInterface:
			case Operator.__QueryPointer:
			case Operator.__Delete:
			case Operator.__Cast:
			case Operator.__FCall:
			case Operator.__PropertyInfo:
			case Operator.__Throw:
			case Operator.__CallInitFunction:
			case Operator.__LateCompiledExpr:
			case Operator.__MemoryBarrier:
			case Operator.__vcStore:
				iStatement = ParseExpressionStatement(out bError, token);
				break;
			case Operator.__Wait:
				iStatement = ParseWaitStatement(out bError, token);
				break;
			case Operator.__PoolScope:
			case Operator.__CurrentTask:
				iStatement = ParseCaseLabelOrExpressionStatement(out bError, token, ref bSemicolonNecessary);
				break;
			case Operator.Plus:
			case Operator.Minus:
			case Operator.__SystemScope:
				return TryParseCaseLabelStatement(token, ref bSemicolonNecessary);
			case Operator.Type:
				if (InsideDeclarationVarDecl)
				{
					return CreateUnexpectedTokenErrorStatement(token);
				}
				iStatement = ParseTypeDeclaration(token);
				MatchOperator(iStatement, Operator.EndType);
				bSemicolonNecessary = false;
				break;
			case Operator.Var:
			case Operator.VarConfig:
			case Operator.VarExternal:
			case Operator.VarGlobal:
			case Operator.VarInput:
			case Operator.VarInOut:
			case Operator.VarOutput:
			case Operator.VarTemp:
			case Operator.VarStat:
			case Operator.VarInst:
				iStatement = ParseVariableList(token);
				MatchOperator(iStatement, Operator.EndVar);
				bSemicolonNecessary = false;
				break;
			case Operator.Function:
			case Operator.FunctionBlock:
			case Operator.Program:
			case Operator.Method:
			case Operator.Interface:
				if (InsideDeclarationVarDecl)
				{
					return CreateUnexpectedTokenErrorStatement(token);
				}
				iStatement = ParsePOUDeclaration(token, Scanner.GetOperator(token));
				bSemicolonNecessary = false;
				break;
			case Operator.Semicolon:
				iStatement = LMItemFactory.CreateEmptyStatement(token);
				bSemicolonNecessary = false;
				break;
			case Operator.Return:
				iStatement = ParseReturn(out bError, token);
				break;
			case Operator.Exit:
				iStatement = LMItemFactory.CreateExitStatement(token);
				break;
			case Operator.Continue:
				iStatement = LMItemFactory.CreateContinueStatement(token);
				break;
			case Operator.__Try:
				iStatement = ParseTryCatchStatement(out bError, token);
				bSemicolonNecessary = false;
				break;
			case Operator.If:
				iStatement = ParseIf(out bError, token);
				bSemicolonNecessary = false;
				break;
			case Operator.While:
				iStatement = ParseWhile(out bError, token);
				bSemicolonNecessary = false;
				break;
			case Operator.Repeat:
				iStatement = ParseRepeat(out bError, token);
				bSemicolonNecessary = false;
				break;
			case Operator.For:
				iStatement = ParseFor(out bError, token);
				bSemicolonNecessary = false;
				break;
			case Operator.Case:
				iStatement = ParseCase(out bError, token);
				bSemicolonNecessary = false;
				break;
			case Operator.Period:
				if (!InDeclaration)
				{
					return ParseExpressionStatement(out bError, token);
				}
				iStatement = ParseVariableDeclaration(token);
				bSemicolonNecessary = false;
				break;
			case Operator.Jmp:
				iStatement = ParseJump(out bError, token);
				break;
			case Operator.CalC:
				iStatement = ParseConditionalCall(out bError, token);
				break;
			default:
				return CreateUnexpectedTokenErrorStatement(token);
			}
			return iStatement;
		}

		private _IStatement ParseExpressionStatement(out bool bError, IToken token)
		{
			Scanner.SetPosition(token);
			_IExpression exp = ParseAssignExp(out bError);
			return LMItemFactory.CreateExpressionStatement(exp, token);
		}

		private _IStatement TryParseCaseLabelStatement(IToken token, ref bool bSemicolonNecessary)
		{
			if (InCase)
			{
				Scanner.SetPosition(token);
				_ICaseLabelStatement iCaseLabelStatement = ParseCaseLabel(token);
				if (iCaseLabelStatement != null)
				{
					bSemicolonNecessary = false;
					return iCaseLabelStatement;
				}
			}
			return CreateUnexpectedTokenErrorStatement(token);
		}

		private _IStatement CreateUnexpectedTokenErrorStatement(IToken token)
		{
			_IStatement iStatement = LMItemFactory.CreateErrorStatement(token);
			AddErrorSTWithToken(iStatement, token, MessageId.Err_UnexpectedTokenFound, ((_IScanner)Scanner).GetTokenText(token, ETokenTextFlags.TruncateAtLinebreak));
			return iStatement;
		}

		private _IStatement ParseCaseLabelOrExpressionStatement(out bool bError, IToken token, ref bool bSemicolonNecessary)
		{
			Scanner.SetPosition(token);
			bError = false;
			_IStatement result;
			if (InCase)
			{
				_ICaseLabelStatement iCaseLabelStatement = ParseCaseLabel(token);
				if (iCaseLabelStatement != null)
				{
					result = iCaseLabelStatement;
					bSemicolonNecessary = false;
					return result;
				}
				Scanner.SetPosition(token);
			}
			_IExpression exp = ParseAssignExp(out bError);
			if (InDeclaration)
			{
				result = ParseVariableDeclaration(token);
				bSemicolonNecessary = false;
			}
			else
			{
				result = LMItemFactory.CreateExpressionStatement(exp, token);
			}
			return result;
		}

		private _IStatement ParseWaitStatement(out bool bError, IToken token)
		{
			_IStatement result = null;
			Scanner.SetPosition(token);
			if (ParseAssignExp(out bError) is _IOperatorExpression iOperatorExpression)
			{
				_IWhileStatement iWhileStatement = LMItemFactory.CreateWhileStatement();
				if (iOperatorExpression._OperandsList.Count == 1)
				{
					_IOperatorExpression iOperatorExpression2 = LMItemFactory.CreateOperatorExpression(Operator.Not);
					iOperatorExpression2.AddOperand(iOperatorExpression[0]);
					iWhileStatement._Condition = iOperatorExpression2;
				}
				else
				{
					_IErrorExpression iErrorExpression = LMItemFactory.CreateErrorExpression(token);
					AddErrorST(iErrorExpression, MessageId.Err_OpNeedsExactInputs, Scanner.GetOperatorText(Operator.__Wait), 1);
					iWhileStatement._Condition = iErrorExpression;
				}
				_ISequenceStatement iSequenceStatement = LMItemFactory.CreateSequenceStatement();
				_ICallExpression exp = LMItemFactory.CreateCallExpression(LMItemFactory.CreateVariableExpression("SynchWait", token), token);
				iSequenceStatement.Add(LMItemFactory.CreateExpressionStatement(exp, token));
				iWhileStatement._Controlled = iSequenceStatement;
				result = iWhileStatement;
			}
			return result;
		}

		private void ParseSemicolonIfNecessary(bool bSemicolonNecessary, _IStatement statement)
		{
			if (bSemicolonNecessary && (Next(out var token) != TokenType.Operator || Scanner.GetOperator(token) != Operator.Semicolon))
			{
				if (token.Type == TokenType.End)
				{
					AddErrorSTWithToken(statement, token, MessageId.Err_SemicolonExpectedInsteadOfEnd, Scanner.GetTokenText(token));
				}
				else
				{
					AddErrorSTWithToken(statement, token, MessageId.Err_SemicolonExpected, Scanner.GetTokenText(token));
					Scanner.SetPosition(token);
				}
			}
		}

		private _IStatement TryParseDeclarationOrLabel(ref bool bError, IToken token, ref bool bSemicolonNecessary)
		{
			if (InDeclaration)
			{
				_IStatement iStatement;
				if (Scanner.GetIdentifier(token) == "FUNCTIONBLOCK")
				{
					iStatement = ParsePOUDeclaration(token, Operator.FunctionBlock);
					AddErrorSTWithToken(iStatement, token, MessageId.Err_FunctionBlockNoLongerValid);
				}
				else
				{
					iStatement = ParseVariableDeclaration(token);
					bSemicolonNecessary = false;
				}
				return iStatement;
			}
			if (!InCase && Next(out var token2) == TokenType.Operator && Scanner.GetOperator(token2) == Operator.Colon)
			{
				string identifier = Scanner.GetIdentifier(token);
				_IStatement iStatement = LMItemFactory.CreateLabelStatement(identifier, token);
				bSemicolonNecessary = false;
				return iStatement;
			}
			Scanner.SetPosition(token);
			if (InCase)
			{
				_ICaseLabelStatement iCaseLabelStatement = ParseCaseLabel(token);
				if (iCaseLabelStatement != null)
				{
					_IStatement iStatement = iCaseLabelStatement;
					bSemicolonNecessary = false;
					return iStatement;
				}
				Scanner.SetPosition(token);
			}
			_IExpression exp = ParseAssignExp(out bError);
			return LMItemFactory.CreateExpressionStatement(exp, token);
		}

		private _IStatement ParseSTStatementHelpPragma(out bool bError, IToken token)
		{
			return PragmaStatementParser.ParsePragma(out bError, token);
		}

		private _ICaseLabelStatement ParseCaseLabel(IToken tokenCaseLabel)
		{
			return CaseStatementParser.ParseCaseLabel(Context, tokenCaseLabel);
		}

		private _IStatement ParseCase(out bool bError, IToken tokenCase)
		{
			return CaseStatementParser.ParseCaseStatement(Context, out bError, tokenCase);
		}

		private _IStatement ParsePOUDeclaration(IToken tokenPOUType, Operator opParam)
		{
			return Context.DeclarationParser.ParsePOUDeclaration(tokenPOUType, opParam);
		}

		private _IStatement ParseVariableDeclaration(IToken tokenIdent)
		{
			return Context.DeclarationParser.ParseVariableDeclaration(tokenIdent);
		}

		private _IStatement ParseTypeDeclaration(IToken tokenType)
		{
			return Context.DeclarationParser.ParseTypeDeclaration(tokenType);
		}

		private _IStatement ParseVariableList(IToken tokenVar)
		{
			return Context.DeclarationParser.ParseVariableList(tokenVar);
		}

		private _IReturnStatement ParseReturn(out bool bError, IToken tokenRet)
		{
			return ReturnStatementParser.Parse(Context, out bError, tokenRet);
		}

		private _ITryCatchStatement ParseTryCatchStatement(out bool bError, IToken tokenRet)
		{
			return TryCatchStatementParser.Parse(Context, out bError, tokenRet);
		}

		private _IStatement ParseIf(out bool bError, IToken tokenIf)
		{
			return IfStatementParser.Parse(Context, out bError, tokenIf);
		}

		private _IStatement ParseWhile(out bool bError, IToken tokenWhile)
		{
			return WhileStatementParser.Parse(Context, out bError, tokenWhile);
		}

		private _IStatement ParseFor(out bool bError, IToken tokenFor)
		{
			return ForStatementParser.Parse(Context, out bError, tokenFor);
		}

		private _IStatement ParseRepeat(out bool bError, IToken tokenRepeat)
		{
			return RepeatStatementParser.Parse(Context, out bError, tokenRepeat);
		}

		private _IStatement ParseConditionalCall(out bool bError, IToken tokenCalc)
		{
			return ConditionalCallParser.Parse(Context, out bError, tokenCalc);
		}

		private _IStatement ParseJump(out bool bError, IToken tokenJump)
		{
			return JumpStatementParser.Parse(Context, out bError, tokenJump);
		}

		private bool ShouldCreateBpForCurrentStatement()
		{
			if (m_BpStack.get_Count() > 0)
			{
				return m_BpStack.Peek().Item1;
			}
			return true;
		}

		internal void EnableBp()
		{
			m_BpStack.Push(new Tuple<bool, int>(item1: true, _stackDepth - 1));
		}

		internal void DisableBp()
		{
			m_BpStack.Push(new Tuple<bool, int>(item1: false, _stackDepth - 1));
		}

		internal void RestoreBp()
		{
			if (m_BpStack.get_Count() > 0 && m_BpStack.Peek().Item2 >= _stackDepth)
			{
				m_BpStack.Pop();
			}
		}

		private _IExpression ParseAssignExp(out bool bError)
		{
			if (Implicit || ImplicitAnyway)
			{
				return Context.ExpressionParser.ParseInitialisationExp(out bError);
			}
			return Context.ExpressionParser.ParseAssignment(out bError);
		}
	}
}
