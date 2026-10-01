using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using CODESYS.Parser;
using CODESYS.Parser35210.Statements;
using CODESYS.Parser35210.Utilities;

namespace CODESYS.Parser35210.Pragmas
{
	internal readonly struct PragmaIfStatementParser
	{
		private readonly _IPragmaIfStatement _ifstatement;

		private ParserContext Context { get; }

		private _ILanguageModelBuilder6 LMItemFactory => Context.LMItemFactory;

		private IScanner9 Scanner => Context.Scanner;

		private StatementParser StatementParser => Context.StatementParser;

		private PragmaStatementParser PragmaParser => Context.PragmaStatementParser;

		private IPragmaScanner PragmaScanner => PragmaParser.PragmaScanner;

		private IErrorHandler ErrorHandler => Context.ErrorHandler;

		private ITokenFactory TokenFactory => Context.TokenFactory;

		private PragmaIfStatementParser(ParserContext context, IToken tokenPragma)
		{
			Context = context;
			_ifstatement = context.LMItemFactory.CreatePragmaIfStatement(null, tokenPragma);
		}

		internal static _IPragmaIfStatement ParsePragmaIfStatement(ParserContext context, out bool bError, IToken tokenPragma)
		{
			PragmaIfStatementParser pragmaIfStatementParser = new PragmaIfStatementParser(context, tokenPragma);
			pragmaIfStatementParser.ParsePragmaIf(out bError, tokenPragma);
			return pragmaIfStatementParser._ifstatement;
		}

		private void Next(out IToken token, bool bWithPragma, bool bWithComment)
		{
			Scanner.Next(out token, bWithPragma, bWithComment);
		}

		private void AddErrorST(_IExprement exp, MessageId nErrorId, params object[] args)
		{
			ErrorHandler.AddErrorST(exp, nErrorId, args);
		}

		private void ParsePragmaIf(out bool bError, IToken tokenPragma)
		{
			bError = false;
			bool bError2;
			_IExpression iExpression = PragmaParser.ParsePragmaORExp(out bError2, tokenPragma) ?? LMItemFactory.CreateErrorExpression(tokenPragma);
			_ifstatement.Condition = iExpression;
			bool bElseFound = false;
			PragmaOperator opLastFound = PragmaOperator.If;
			_IPragmaElseIf elseifCurrent = null;
			if (PragmaScanner.GetNext(out var token) != PragmaTokenType.End && (iExpression.MessagesList == null || iExpression.MessagesList.Count == 0))
			{
				AddErrorST(_ifstatement, MessageId.Err_UnexpectedTokenFound, PragmaScanner.GetTokenText(token));
			}
			IToken token2 = TokenFactory.CreateEmptyToken();
			while (true)
			{
				bError = ParseSequenceAndReturnError(bElseFound, out var seq, ref token2, out var op);
				if (bError)
				{
					break;
				}
				if (token2.Type == TokenType.End)
				{
					AddErrorST(_ifstatement, MessageId.Err_Operator1of3ExpectedInsteadofEOF, Scanner.GetOperatorText(Operator.Elsif), Scanner.GetOperatorText(Operator.Else), Scanner.GetOperatorText(Operator.EndIf));
					break;
				}
				SetSequenceStatement(opLastFound, seq, elseifCurrent);
				if (op != PragmaOperator.EndIf)
				{
					bElseFound = CheckForElseOrElseif(tokenPragma, op, bElseFound, ref elseifCurrent);
					opLastFound = op;
					continue;
				}
				break;
			}
		}

		private bool ParseSequenceAndReturnError(bool bElseFound, out _ISequenceStatement seq, ref IToken token, out PragmaOperator op)
		{
			seq = LMItemFactory.CreateSequenceStatement(token);
			while (true)
			{
				Next(out token, bWithPragma: true, bWithComment: true);
				if (CheckForPragmaReturnError(bElseFound, token, out op))
				{
					break;
				}
				Scanner.SetPosition(token);
				if (token.Type == TokenType.End)
				{
					break;
				}
				bool bError;
				_IStatement sm = StatementParser.ParseSTStatement(out bError, bTopLevel: false);
				seq.Add(sm);
				if (!bError)
				{
					continue;
				}
				IToken tokenPos;
				Operator @operator = StatementParser.ParseReSyncST(out tokenPos);
				if (@operator != Operator.Semicolon)
				{
					op = MapPragmaOperator(@operator);
					if (CheckForFittingOperator(bElseFound, op))
					{
						break;
					}
					Scanner.SetPosition(tokenPos);
					return true;
				}
			}
			return false;
		}

		private static bool CheckForFittingOperator(bool bElseFound, PragmaOperator op)
		{
			if (!bElseFound && (op == PragmaOperator.EndIf || op == PragmaOperator.Elsif || op == PragmaOperator.Else))
			{
				return true;
			}
			if (bElseFound && op == PragmaOperator.EndIf)
			{
				return true;
			}
			return false;
		}

		private static PragmaOperator MapPragmaOperator(Operator opHelp)
		{
			switch (opHelp)
			{
			case Operator.EndIf:
				return PragmaOperator.EndIf;
			case Operator.Elsif:
				return PragmaOperator.Elsif;
			case Operator.Else:
				return PragmaOperator.Else;
			default:
				return PragmaOperator.None;
			}
		}

		private bool CheckForPragmaReturnError(bool bElseFound, IToken token, out PragmaOperator op)
		{
			op = PragmaOperator.None;
			if (token.Type == TokenType.Pragma)
			{
				PragmaScanner.Reset(Scanner.GetPragma(token), token);
				PragmaScanner.GetNext(out var token2);
				op = token2.Operator;
				if (!bElseFound && (op == PragmaOperator.EndIf || op == PragmaOperator.Elsif || op == PragmaOperator.Else))
				{
					return true;
				}
				if (bElseFound && op == PragmaOperator.EndIf)
				{
					return true;
				}
			}
			return false;
		}

		private bool CheckForElseOrElseif(IToken tokenPragma, PragmaOperator op, bool bElseFound, ref _IPragmaElseIf elseifCurrent)
		{
			switch (op)
			{
			case PragmaOperator.Else:
				bElseFound = true;
				break;
			case PragmaOperator.Elsif:
			{
				elseifCurrent = LMItemFactory.CreatePragmaElseIf();
				bool bError;
				_IExpression condition = PragmaParser.ParsePragmaORExp(out bError, tokenPragma) ?? LMItemFactory.CreateErrorExpression(Scanner.CurrentToken);
				if (PragmaScanner.GetNext(out var _) != PragmaTokenType.End)
				{
					AddErrorST(_ifstatement, MessageId.Err_UnexpectedTokenFound, "TODO");
				}
				elseifCurrent.Condition = condition;
				break;
			}
			}
			return bElseFound;
		}

		private void SetSequenceStatement(PragmaOperator opLastFound, _ISequenceStatement seq, _IPragmaElseIf elseifCurrent)
		{
			switch (opLastFound)
			{
			case PragmaOperator.If:
				_ifstatement.IfThen = seq;
				break;
			case PragmaOperator.Elsif:
				if (elseifCurrent != null)
				{
					elseifCurrent.Controlled = seq;
					_ifstatement.AddElseIf(elseifCurrent);
				}
				break;
			case PragmaOperator.Else:
				_ifstatement.IfElse = seq;
				break;
			}
		}
	}
}
