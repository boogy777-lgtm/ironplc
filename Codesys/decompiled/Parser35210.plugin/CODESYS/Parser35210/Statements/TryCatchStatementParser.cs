using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using CODESYS.Parser;
using CODESYS.Parser35210.Expressions;
using CODESYS.Parser35210.Utilities;

namespace CODESYS.Parser35210.Statements
{
	internal readonly struct TryCatchStatementParser
	{
		private ParserContext Context { get; }

		private StatementParser StatementParser => Context.StatementParser;

		private _ILanguageModelBuilder6 LMItemFactory => Context.LMItemFactory;

		private IScanner9 Scanner => Context.Scanner;

		private ExpressionParser ExpressionParser => Context.ExpressionParser;

		private IErrorHandler ErrorHandler => Context.ErrorHandler;

		private TryCatchStatementParser(ParserContext context)
		{
			Context = context;
		}

		internal static _ITryCatchStatement Parse(ParserContext context, out bool bError, IToken tokenTry)
		{
			return new TryCatchStatementParser(context).ParseTryCatchStatement(out bError, tokenTry);
		}

		private _IExpression ParseSTOperand(out bool bError)
		{
			return ExpressionParser.ParseSTOperand(out bError);
		}

		private void AddErrorST(_IExprement exp, MessageId nErrorId, params object[] args)
		{
			ErrorHandler.AddErrorST(exp, nErrorId, args);
		}

		private _ITryCatchStatement ParseTryCatchStatement(out bool bError, IToken tokenTry)
		{
			bError = false;
			_ITryCatchStatement iTryCatchStatement = LMItemFactory.CreateTryCatchStatement(tokenTry);
			bool bErrorLocal = false;
			Operator opLastFound = Operator.__Try;
			IToken token = tokenTry;
			while (true)
			{
				Operator op = Operator.__EndTry;
				_ISequenceStatement seq = LMItemFactory.CreateSequenceStatement(token);
				bError = ParseSequenceOutError(opLastFound, seq, iTryCatchStatement, out token, ref op, ref bErrorLocal);
				if (bError)
				{
					return iTryCatchStatement;
				}
				bError = ParseOptionalOperatorOutError(opLastFound, op, iTryCatchStatement, bErrorLocal);
				InsertSequence(opLastFound, iTryCatchStatement, seq);
				if (CheckForUnexpectedEnd(token, iTryCatchStatement) || op == Operator.__EndTry)
				{
					break;
				}
				opLastFound = op;
			}
			StatementParser.SetStatementFlags(iTryCatchStatement);
			return iTryCatchStatement;
		}

		private bool ParseSequenceOutError(Operator opLastFound, _ISequenceStatement seq, _ITryCatchStatement trycatch, out IToken token, ref Operator op, ref bool bErrorLocal)
		{
			while (true)
			{
				Scanner.Next(out token, bWithPragma: true, bWithComment: true);
				if (CheckForBreak(token, opLastFound, ref op))
				{
					break;
				}
				bErrorLocal = false;
				Scanner.SetPosition(token);
				if (token.Type == TokenType.End)
				{
					break;
				}
				_IStatement sm = Context.InternalParser.ParseSTStatement(out bErrorLocal);
				seq.Add(sm);
				if (!bErrorLocal)
				{
					continue;
				}
				IToken tokenPos;
				Operator @operator = StatementParser.ParseReSyncST(out tokenPos);
				if (@operator != Operator.Semicolon)
				{
					if (CheckForBreak(@operator, opLastFound, out op))
					{
						break;
					}
					ResynchOutsideStatement(tokenPos, opLastFound, trycatch, seq);
					return true;
				}
			}
			return false;
		}

		private bool CheckForUnexpectedEnd(IToken token, _ITryCatchStatement trycatch)
		{
			if (token.Type == TokenType.End)
			{
				AddErrorST(trycatch, MessageId.Err_Operator1of3ExpectedInsteadofEOF, Scanner.GetOperatorText(Operator.__Catch), Scanner.GetOperatorText(Operator.__Finally), Scanner.GetOperatorText(Operator.__EndTry));
				return true;
			}
			return false;
		}

		private static void InsertSequence(Operator opLastFound, _ITryCatchStatement trycatch, _ISequenceStatement seq)
		{
			switch (opLastFound)
			{
			case Operator.__Try:
				trycatch._Try = seq;
				break;
			case Operator.__Catch:
				trycatch._Catch = seq;
				break;
			case Operator.__Finally:
				trycatch._Finally = seq;
				break;
			case Operator.__EndTry:
				break;
			}
		}

		private bool ParseOptionalOperatorOutError(Operator opLastFound, Operator op, _ITryCatchStatement trycatch, bool bErrorLocal)
		{
			bool bError = false;
			if (opLastFound == Operator.__Try && op == Operator.__Catch && StatementParser.CheckOptionalOperator(Operator.LeftParenthesis))
			{
				if (!StatementParser.CheckOptionalOperator(Operator.RightParenthesis))
				{
					trycatch._Exception = ParseSTOperand(out bError);
				}
				_IErrorExpression exprError = null;
				StatementParser.CheckForOperator(trycatch, Operator.RightParenthesis, bErrorLocal, out exprError);
			}
			return bError;
		}

		private void ResynchOutsideStatement(IToken tokenPos, Operator opLastFound, _ITryCatchStatement trycatch, _ISequenceStatement seq)
		{
			Scanner.SetPosition(tokenPos);
			switch (opLastFound)
			{
			case Operator.__Try:
				trycatch._Try = seq;
				break;
			case Operator.__Catch:
				trycatch._Catch = seq;
				break;
			case Operator.__Finally:
				trycatch._Finally = seq;
				break;
			case Operator.__EndTry:
				break;
			}
		}

		private static bool CheckForBreak(Operator opHelp, Operator opLastFound, out Operator op)
		{
			op = opHelp;
			if (opLastFound == Operator.__Try && (op == Operator.__Catch || op == Operator.__Finally || op == Operator.__EndTry))
			{
				return true;
			}
			if (opLastFound == Operator.__Catch && (op == Operator.__Finally || op == Operator.__EndTry))
			{
				return true;
			}
			if (opLastFound == Operator.__Finally && op == Operator.__EndTry)
			{
				return true;
			}
			return false;
		}

		private bool CheckForBreak(IToken token, Operator opLastFound, ref Operator op)
		{
			if (token.Type == TokenType.Operator)
			{
				return CheckForBreak(Scanner.GetOperator(token), opLastFound, out op);
			}
			return false;
		}
	}
}
