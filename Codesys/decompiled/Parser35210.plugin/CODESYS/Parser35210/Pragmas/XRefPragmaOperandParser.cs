using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using CODESYS.Parser;
using CODESYS.Parser35210.Utilities;

namespace CODESYS.Parser35210.Pragmas
{
	internal readonly struct XRefPragmaOperandParser
	{
		private ParserContext Context { get; }

		private _ILanguageModelBuilder6 LMItemFactory => Context.LMItemFactory;

		private PragmaStatementParser PragmaParser => Context.PragmaStatementParser;

		private IPragmaScanner PragmaScanner => PragmaParser.PragmaScanner;

		private IErrorHandler ErrorHandler => Context.ErrorHandler;

		private XRefPragmaOperandParser(ParserContext context)
		{
			Context = context;
		}

		internal static _IExpression ParseXRefPragma(ParserContext context, out bool bError, IToken tokenPragma)
		{
			return new XRefPragmaOperandParser(context).ParseXRefPragma(out bError, tokenPragma);
		}

		private void AddErrorST(_IExprement exp, MessageId nErrorId, params object[] args)
		{
			ErrorHandler.AddErrorST(exp, nErrorId, args);
		}

		private _IItemReference ParseItemReference(out bool bError, IToken tokenPragma)
		{
			return ItemReferenceParser.ParseItemReference(Context, out bError, tokenPragma);
		}

		private _IExpression ParseXRefPragma(out bool bError, IToken tokenPragma)
		{
			_IXRefExpression iXRefExpression = LMItemFactory.CreateXRefExpression(tokenPragma);
			if (PragmaScanner.GetNext(out var token) != PragmaTokenType.Operator || token.Operator != PragmaOperator.LeftParenthesis)
			{
				AddErrorST(iXRefExpression, MessageId.Err_OperatorExpected, "(", PragmaScanner.GetTokenText(token));
			}
			iXRefExpression.XRef = ParseItemReference(out bError, tokenPragma);
			if (PragmaScanner.GetNext(out token) == PragmaTokenType.Operator && token.Operator == PragmaOperator.from)
			{
				iXRefExpression.XRefFrom = ParseItemReference(out bError, tokenPragma);
			}
			if (PragmaScanner.GetNext(out token) != PragmaTokenType.Operator || token.Operator != PragmaOperator.RightParenthesis)
			{
				AddErrorST(iXRefExpression, MessageId.Err_OperatorExpected, ")", PragmaScanner.GetTokenText(token));
			}
			return iXRefExpression;
		}
	}
}
