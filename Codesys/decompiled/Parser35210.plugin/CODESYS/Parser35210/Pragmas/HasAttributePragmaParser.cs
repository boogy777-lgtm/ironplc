using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using CODESYS.Parser;
using CODESYS.Parser35210.Utilities;

namespace CODESYS.Parser35210.Pragmas
{
	internal readonly struct HasAttributePragmaParser
	{
		private ParserContext Context { get; }

		private _ILanguageModelBuilder6 LMItemFactory => Context.LMItemFactory;

		private PragmaStatementParser PragmaParser => Context.PragmaStatementParser;

		private IPragmaScanner PragmaScanner => PragmaParser.PragmaScanner;

		private IErrorHandler ErrorHandler => Context.ErrorHandler;

		private HasAttributePragmaParser(ParserContext context)
		{
			Context = context;
		}

		internal static _IExpression ParseHasAttributePragma(ParserContext context, out bool bError, IToken tokenPragma)
		{
			return new HasAttributePragmaParser(context).ParseHasAttributePragma(out bError, tokenPragma);
		}

		private void AddErrorST(_IExprement exp, MessageId nErrorId, params object[] args)
		{
			ErrorHandler.AddErrorST(exp, nErrorId, args);
		}

		private _IItemReference ParseItemReference(out bool bError, IToken tokenPragma)
		{
			return ItemReferenceParser.ParseItemReference(Context, out bError, tokenPragma);
		}

		private _IExpression ParseHasAttributePragma(out bool bError, IToken tokenPragma)
		{
			_IHasAttributeExpression iHasAttributeExpression = LMItemFactory.CreateHasAttributeExpression(tokenPragma);
			if (PragmaScanner.GetNext(out var token) != PragmaTokenType.Operator || token.Operator != PragmaOperator.LeftParenthesis)
			{
				AddErrorST(iHasAttributeExpression, MessageId.Err_OperatorExpected, "(", PragmaScanner.GetTokenText(token));
			}
			iHasAttributeExpression.ItemReference = ParseItemReference(out bError, tokenPragma);
			if (PragmaScanner.GetNext(out token) != PragmaTokenType.Operator || token.Operator != PragmaOperator.Comma)
			{
				AddErrorST(iHasAttributeExpression, MessageId.Err_OperatorExpected, ",", PragmaScanner.GetTokenText(token));
			}
			if (PragmaScanner.GetNext(out token) != PragmaTokenType.SingleByteString)
			{
				AddErrorST(iHasAttributeExpression, MessageId.Err_AttributeNameExpected, PragmaScanner.GetTokenText(token));
			}
			else
			{
				iHasAttributeExpression.Attribute = token.String;
			}
			if (PragmaScanner.GetNext(out token) != PragmaTokenType.Operator || token.Operator != PragmaOperator.RightParenthesis)
			{
				AddErrorST(iHasAttributeExpression, MessageId.Err_OperatorExpected, ")", PragmaScanner.GetTokenText(token));
			}
			return iHasAttributeExpression;
		}
	}
}
