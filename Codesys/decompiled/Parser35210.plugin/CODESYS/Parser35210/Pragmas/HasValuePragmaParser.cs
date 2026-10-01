using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using CODESYS.Parser;
using CODESYS.Parser35210.Utilities;

namespace CODESYS.Parser35210.Pragmas
{
	internal readonly struct HasValuePragmaParser
	{
		private ParserContext Context { get; }

		private _ILanguageModelBuilder6 LMItemFactory => Context.LMItemFactory;

		private PragmaStatementParser PragmaParser => Context.PragmaStatementParser;

		private IPragmaScanner PragmaScanner => PragmaParser.PragmaScanner;

		private IErrorHandler ErrorHandler => Context.ErrorHandler;

		private HasValuePragmaParser(ParserContext context)
		{
			Context = context;
		}

		internal static _IExpression ParseHasValuePragma(ParserContext context, IToken tokenPragma)
		{
			return new HasValuePragmaParser(context).ParseHasValuePragma(tokenPragma);
		}

		private void AddErrorST(_IExprement exp, MessageId nErrorId, params object[] args)
		{
			ErrorHandler.AddErrorST(exp, nErrorId, args);
		}

		private _IExpression ParseHasValuePragma(IToken tokenPragma)
		{
			_IHasValueExpression iHasValueExpression = LMItemFactory.CreateHasValueExpression(tokenPragma);
			if (PragmaScanner.GetNext(out var token) != PragmaTokenType.Operator || token.Operator != PragmaOperator.LeftParenthesis)
			{
				AddErrorST(iHasValueExpression, MessageId.Err_OperatorExpected, "(", PragmaScanner.GetTokenText(token));
			}
			if (PragmaScanner.GetNext(out token) != PragmaTokenType.Identifier)
			{
				AddErrorST(iHasValueExpression, MessageId.Err_IdentifierExpected, PragmaScanner.GetTokenText(token));
			}
			else
			{
				iHasValueExpression.Define = token.Identifier;
			}
			if (PragmaScanner.GetNext(out token) != PragmaTokenType.Operator || token.Operator != PragmaOperator.Comma)
			{
				AddErrorST(iHasValueExpression, MessageId.Err_OperatorExpected, ",", PragmaScanner.GetTokenText(token));
			}
			if (PragmaScanner.GetNext(out token) != PragmaTokenType.SingleByteString)
			{
				AddErrorST(iHasValueExpression, MessageId.Err_DefineValueExpected, PragmaScanner.GetTokenText(token));
			}
			else
			{
				iHasValueExpression.DefineValue = token.String;
			}
			if (PragmaScanner.GetNext(out token) != PragmaTokenType.Operator || token.Operator != PragmaOperator.RightParenthesis)
			{
				AddErrorST(iHasValueExpression, MessageId.Err_OperatorExpected, ")", PragmaScanner.GetTokenText(token));
			}
			return iHasValueExpression;
		}
	}
}
