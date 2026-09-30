using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using CODESYS.Parser;
using CODESYS.Parser35210.Resources;
using CODESYS.Parser35210.Utilities;

namespace CODESYS.Parser35210.Pragmas
{
	internal readonly struct HasConstantValueOrTypePragmaParser
	{
		private ParserContext Context { get; }

		private _ILanguageModelBuilder6 LMItemFactory => Context.LMItemFactory;

		private PragmaStatementParser PragmaParser => Context.PragmaStatementParser;

		private IScanner9 Scanner => Context.Scanner;

		private IPragmaScanner PragmaScanner => PragmaParser.PragmaScanner;

		private IErrorHandler ErrorHandler => Context.ErrorHandler;

		private ITokenFactory TokenFactory => Context.TokenFactory;

		private HasConstantValueOrTypePragmaParser(ParserContext context)
		{
			Context = context;
		}

		internal static _IExpression ParseHasConstantValuePragma(ParserContext context, IToken tokenPragma)
		{
			return new HasConstantValueOrTypePragmaParser(context).ParseHasConstantValuePragma(tokenPragma);
		}

		internal static _IExpression ParseHasConstantTypePragma(ParserContext context, IToken tokenPragma)
		{
			return new HasConstantValueOrTypePragmaParser(context).ParseHasConstantTypePragma(tokenPragma);
		}

		private void AddErrorST(_IExprement exp, MessageId nErrorId, params object[] args)
		{
			ErrorHandler.AddErrorST(exp, nErrorId, args);
		}

		private _IExpression ParseHasConstantValuePragma(IToken tokenPragma)
		{
			_IHasConstantValueExpression2 iHasConstantValueExpression = (_IHasConstantValueExpression2)LMItemFactory.CreateHasConstantValueExpression(tokenPragma);
			if (PragmaScanner.GetNext(out var token) != PragmaTokenType.Operator || token.Operator != PragmaOperator.LeftParenthesis)
			{
				AddErrorST(iHasConstantValueExpression, MessageId.Err_OperatorExpected, "(", PragmaScanner.GetTokenText(token));
			}
			iHasConstantValueExpression._Constant = ParsePragmaExpression(tokenPragma, out var stParsedSourceSnippet);
			if (iHasConstantValueExpression._Constant == null)
			{
				AddErrorST(iHasConstantValueExpression, MessageId.Err_ExpressionExpectedInstead, stParsedSourceSnippet, PragmaScanner.GetTokenText(token));
			}
			else
			{
				new PragmaSourcePositionAdjuster(Scanner.CurrentToken.Position, PragmaScanner.PositionOffset, TokenFactory).AdjustSourcePositions(iHasConstantValueExpression._Constant);
			}
			if (PragmaScanner.GetNext(out token) != PragmaTokenType.Operator || token.Operator != PragmaOperator.Comma)
			{
				AddErrorST(iHasConstantValueExpression, MessageId.Err_OperatorExpected, ",", PragmaScanner.GetTokenText(token));
			}
			iHasConstantValueExpression._ConstantValue = ParsePragmaExpression(tokenPragma, out stParsedSourceSnippet);
			if (iHasConstantValueExpression._ConstantValue == null)
			{
				AddErrorST(iHasConstantValueExpression, MessageId.Err_Operator1of2Expected, Strings.Literal, Strings.Constant, stParsedSourceSnippet);
			}
			if (PragmaScanner.GetNext(out token) != PragmaTokenType.Operator || (token.Operator != PragmaOperator.Comma && token.Operator != PragmaOperator.RightParenthesis))
			{
				AddErrorST(iHasConstantValueExpression, MessageId.Err_Operator1of2Expected, ")", ",", PragmaScanner.GetTokenText(token));
			}
			else
			{
				ParseHasConstantValuePragmaComparison(token, iHasConstantValueExpression);
			}
			return iHasConstantValueExpression;
		}

		private _IExpression ParseHasConstantTypePragma(IToken tokenPragma)
		{
			_IHasConstantTypeExpression iHasConstantTypeExpression = LMItemFactory.CreateHasConstantTypeExpression(tokenPragma);
			if (PragmaScanner.GetNext(out var token) != PragmaTokenType.Operator || token.Operator != PragmaOperator.LeftParenthesis)
			{
				AddErrorST(iHasConstantTypeExpression, MessageId.Err_OperatorExpected, "(", PragmaScanner.GetTokenText(token));
			}
			iHasConstantTypeExpression._Constant = ParsePragmaExpression(tokenPragma, out var stParsedSourceSnippet);
			if (iHasConstantTypeExpression._Constant == null)
			{
				AddErrorST(iHasConstantTypeExpression, MessageId.Err_ExpressionExpectedInstead, stParsedSourceSnippet, PragmaScanner.GetTokenText(token));
			}
			else
			{
				new PragmaSourcePositionAdjuster(Scanner.CurrentToken.Position, PragmaScanner.PositionOffset, TokenFactory).AdjustSourcePositions(iHasConstantTypeExpression._Constant);
			}
			if (PragmaScanner.GetNext(out token) != PragmaTokenType.Operator || token.Operator != PragmaOperator.Comma)
			{
				AddErrorST(iHasConstantTypeExpression, MessageId.Err_OperatorExpected, ",", PragmaScanner.GetTokenText(token));
			}
			if (!(ParsePragmaExpression(tokenPragma, out stParsedSourceSnippet) is _ILiteralExpression iLiteralExpression) || iLiteralExpression.ConstantType != 0)
			{
				AddErrorST(iHasConstantTypeExpression, MessageId.Err_LiteralExpected, stParsedSourceSnippet);
			}
			else
			{
				iHasConstantTypeExpression._ConstantTypeReplaced = iLiteralExpression.LiteralValue.Bool;
			}
			if (PragmaScanner.GetNext(out token) != PragmaTokenType.Operator || token.Operator != PragmaOperator.RightParenthesis)
			{
				AddErrorST(iHasConstantTypeExpression, MessageId.Err_OperatorExpected, ")", PragmaScanner.GetTokenText(token));
			}
			return iHasConstantTypeExpression;
		}

		private _IExpression ParsePragmaExpression(IToken tokenPragma, out string stParsedSourceSnippet)
		{
			IScanner9 scanner = Context.Scanner;
			Context.Scanner = PragmaScanner.OrgScanner;
			int sourceOffset = Scanner.SourceOffset;
			IExpression expression = Context.ExpressionParser.ParseExpression();
			int sourceOffset2 = Scanner.SourceOffset;
			_IExpression iExpression = expression as _IExpression;
			if (iExpression != null)
			{
				IMinimalPosition minimalPosition2 = (iExpression.PositionIntern = LMItemFactory.CreateMinimalPosition(tokenPragma.Position, (short)(iExpression.PositionIntern.PositionOffset + 1)));
			}
			stParsedSourceSnippet = ((_IScanner)Scanner).GetInputSubString(sourceOffset, sourceOffset2 - sourceOffset);
			Context.Scanner = scanner;
			return iExpression;
		}

		private void ParseHasConstantValuePragmaComparison(IPragmaToken token, _IHasConstantValueExpression2 expHelp)
		{
			Operator opComparison = Operator.Equal;
			if (token.Operator == PragmaOperator.Comma)
			{
				if (PragmaScanner.OrgScanner.GetNext(out var token2) != TokenType.Operator)
				{
					AddErrorST(expHelp, MessageId.Err_IllegalOperator, PragmaScanner.OrgScanner.GetTokenText(token2));
				}
				else
				{
					opComparison = PragmaScanner.OrgScanner.GetOperator(token2);
					if (PragmaScanner.GetNext(out token) != PragmaTokenType.Operator || token.Operator != PragmaOperator.RightParenthesis)
					{
						AddErrorST(expHelp, MessageId.Err_OperatorExpected, ")", PragmaScanner.GetTokenText(token));
					}
				}
			}
			expHelp._OpComparison = opComparison;
		}
	}
}
