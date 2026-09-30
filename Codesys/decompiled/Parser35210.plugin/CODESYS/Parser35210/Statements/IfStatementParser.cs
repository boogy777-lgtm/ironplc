using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using CODESYS.Parser35210.Expressions;
using CODESYS.Parser35210.Utilities;

namespace CODESYS.Parser35210.Statements
{
	internal readonly struct IfStatementParser
	{
		private ParserContext Context { get; }

		private StatementParser StatementParser => Context.StatementParser;

		private _ILanguageModelBuilder6 LMItemFactory => Context.LMItemFactory;

		private IScanner9 Scanner => Context.Scanner;

		private ExpressionParser ExpressionParser => Context.ExpressionParser;

		private IfStatementParser(ParserContext context)
		{
			Context = context;
		}

		internal static _IIfStatement Parse(ParserContext context, out bool bError, IToken tokenIf)
		{
			return new IfStatementParser(context).ParseIf(out bError, tokenIf);
		}

		private _IStatement ParseSTStatement(out bool bErrorLocal)
		{
			return StatementParser.ParseSTStatement(out bErrorLocal, bTopLevel: false);
		}

		private Operator ParseReSyncST(out IToken tokenPos)
		{
			return StatementParser.ParseReSyncST(out tokenPos);
		}

		private void CheckForOperator(_IExprement exprement, Operator op, bool bErrorLocal, out _IErrorExpression exprError)
		{
			StatementParser.CheckForOperator(exprement, op, bErrorLocal, out exprError);
		}

		private void Next(out IToken token, bool bWithPragma, bool bWithComment)
		{
			Scanner.Next(out token, bWithPragma, bWithComment);
		}

		private _IExpression ParseAssignExp(out bool bErrorLocal)
		{
			return ExpressionParser.ParseAssignExp(out bErrorLocal);
		}

		private void AddErrorST(_IExprement exp, MessageId nErrorId, params object[] args)
		{
			Context.ErrorHandler.AddErrorST(exp, nErrorId, args);
		}

		private _IIfStatement ParseIf(out bool bError, IToken tokenIf)
		{
			bool bErrorLocal;
			_IExpression expCond = ParseAssignExp(out bErrorLocal) ?? LMItemFactory.CreateErrorExpression(tokenIf);
			_IIfStatement iIfStatement = LMItemFactory.CreateIfStatement(expCond, tokenIf);
			Operator opLastFound = Operator.Then;
			CheckForOperator(iIfStatement, Operator.Then, bErrorLocal, out var exprError);
			if (exprError != null)
			{
				iIfStatement._Condition = exprError;
			}
			bError = ParseSequenceReturnError(tokenIf, opLastFound, iIfStatement);
			if (bError)
			{
				return iIfStatement;
			}
			RemoveElseIfs(iIfStatement);
			StatementParser.RestoreBp();
			return iIfStatement;
		}

		private bool ParseSequenceReturnError(IToken token, Operator opLastFound, _IIfStatement ifstatement)
		{
			bool bElseFound = false;
			_IElseIf elseifCurrent = null;
			while (true)
			{
				Operator op = Operator.EndIf;
				_ISequenceStatement seq = LMItemFactory.CreateSequenceStatement(token);
				if (ParseStatement(out token, opLastFound, ifstatement, bElseFound, seq, elseifCurrent, ref op))
				{
					return true;
				}
				InsertSequenceStatement(opLastFound, ifstatement, seq, elseifCurrent);
				if (token.Type == TokenType.End)
				{
					AddErrorST(ifstatement, MessageId.Err_Operator1of3ExpectedInsteadofEOF, Scanner.GetOperatorText(Operator.Elsif), Scanner.GetOperatorText(Operator.Else), Scanner.GetOperatorText(Operator.EndIf));
					break;
				}
				if (op == Operator.EndIf)
				{
					break;
				}
				bElseFound = HandleElseAndElseIf(op, bElseFound, ifstatement, ref elseifCurrent);
				opLastFound = op;
			}
			return false;
		}

		private bool ParseStatement(out IToken token, Operator opLastFound, _IIfStatement ifstatement, bool bElseFound, _ISequenceStatement seq, _IElseIf elseifCurrent, ref Operator op)
		{
			while (!CheckForNextOperator(bElseFound, out token, ref op))
			{
				bool bErrorLocal;
				_IStatement sm = ParseSTStatement(out bErrorLocal);
				seq.Add(sm);
				if (bErrorLocal)
				{
					bool bResynchoutside;
					bool flag = HandleLocalError(bElseFound, opLastFound, ifstatement, seq, elseifCurrent, out bResynchoutside, ref op);
					if (bResynchoutside)
					{
						return true;
					}
					if (flag)
					{
						break;
					}
				}
			}
			return false;
		}

		private bool HandleElseAndElseIf(Operator op, bool bElseFound, _IIfStatement ifstatement, ref _IElseIf elseifCurrent)
		{
			switch (op)
			{
			case Operator.Else:
				bElseFound = true;
				break;
			case Operator.Elsif:
			{
				elseifCurrent = LMItemFactory.CreateElseIf();
				bool bErrorLocal;
				_IExpression condition = ParseAssignExp(out bErrorLocal) ?? LMItemFactory.CreateErrorExpression(Scanner.CurrentToken);
				_IErrorExpression exprError = null;
				CheckForOperator(ifstatement, Operator.Then, bErrorLocal, out exprError);
				elseifCurrent._Condition = condition;
				break;
			}
			}
			return bElseFound;
		}

		private bool CheckForNextOperator(bool bElseFound, out IToken token, ref Operator op)
		{
			Next(out token, bWithPragma: true, bWithComment: true);
			if (token.Type == TokenType.Operator)
			{
				op = Scanner.GetOperator(token);
				if (!bElseFound && (op == Operator.EndIf || op == Operator.Elsif || op == Operator.Else))
				{
					return true;
				}
				if (bElseFound && op == Operator.EndIf)
				{
					return true;
				}
			}
			Scanner.SetPosition(token);
			if (token.Type == TokenType.End)
			{
				return true;
			}
			return false;
		}

		private bool HandleLocalError(bool bElseFound, Operator opLastFound, _IIfStatement ifstatement, _ISequenceStatement seq, _IElseIf elseifCurrent, out bool bResynchoutside, ref Operator op)
		{
			bResynchoutside = false;
			IToken tokenPos;
			Operator @operator = ParseReSyncST(out tokenPos);
			if (@operator == Operator.Semicolon)
			{
				return false;
			}
			op = @operator;
			if (!bElseFound && (op == Operator.EndIf || op == Operator.Elsif || op == Operator.Else))
			{
				return false;
			}
			if (bElseFound && op == Operator.EndIf)
			{
				return true;
			}
			bResynchoutside = true;
			Scanner.SetPosition(tokenPos);
			switch (opLastFound)
			{
			case Operator.Then:
				ifstatement._IfThen = seq;
				break;
			case Operator.Elsif:
				elseifCurrent._Controlled = seq;
				ifstatement.AddElseIf(elseifCurrent);
				break;
			case Operator.Else:
				ifstatement._IfElse = seq;
				break;
			}
			return false;
		}

		private static void InsertSequenceStatement(Operator opLastFound, _IIfStatement ifstatement, _ISequenceStatement seq, _IElseIf elseifCurrent)
		{
			switch (opLastFound)
			{
			case Operator.Then:
				ifstatement._IfThen = seq;
				break;
			case Operator.Elsif:
				if (elseifCurrent != null)
				{
					elseifCurrent._Controlled = seq;
					ifstatement.AddElseIf(elseifCurrent);
				}
				break;
			case Operator.Else:
				ifstatement._IfElse = seq;
				break;
			}
		}

		private void RemoveElseIfs(_IIfStatement ifstatement)
		{
			if (ifstatement._ElseIf.Count > 0)
			{
				_IStatement ifElse = ifstatement._IfElse;
				for (int num = ifstatement._ElseIf.Count - 1; num >= 0; num--)
				{
					_IElseIf iElseIf = ifstatement._ElseIf[num];
					_IIfStatement iIfStatement = LMItemFactory.CreateIfStatement();
					iIfStatement._Condition = iElseIf._Condition;
					iIfStatement._IfThen = iElseIf._Controlled;
					iIfStatement._IfElse = ifElse;
					iIfStatement._Position = iElseIf._Position;
					iIfStatement.SetFlag(StatementFlag.GenerateFlow | StatementFlag.GenerateBP, bSetTrue: true);
					_ISequenceStatement iSequenceStatement = LMItemFactory.CreateSequenceStatement(1);
					iSequenceStatement._Position = iElseIf._Position;
					iSequenceStatement.Add(iIfStatement);
					ifElse = iSequenceStatement;
				}
				ifstatement.ClearElseIf();
				ifstatement._IfElse = ifElse;
			}
		}
	}
}
