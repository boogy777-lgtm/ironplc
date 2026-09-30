using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using CODESYS.Parser;
using CODESYS.Parser35210.Utilities;

namespace CODESYS.Parser35210.Pragmas
{
	internal readonly struct DefinedPragmaOperandParser
	{
		private ParserContext Context { get; }

		private _ILanguageModelBuilder6 LMItemFactory => Context.LMItemFactory;

		private PragmaStatementParser PragmaParser => Context.PragmaStatementParser;

		private IPragmaScanner PragmaScanner => PragmaParser.PragmaScanner;

		private IErrorHandler ErrorHandler => Context.ErrorHandler;

		private DefinedPragmaOperandParser(ParserContext context)
		{
			Context = context;
		}

		internal static _IExpression ParseDefinedPragma(ParserContext context, out bool bError, IToken tokenPragma)
		{
			return new DefinedPragmaOperandParser(context).ParseDefinedPragma(out bError, tokenPragma);
		}

		internal static _IExpression ParseProjectDefinedPragma(ParserContext context, out bool bError, IToken tokenPragma)
		{
			return new DefinedPragmaOperandParser(context).ParseProjectDefinedPragma(out bError, tokenPragma);
		}

		private void AddErrorST(_IExprement exp, MessageId nErrorId, params object[] args)
		{
			ErrorHandler.AddErrorST(exp, nErrorId, args);
		}

		private _IItemReference ParseItemReference(out bool bError, IToken tokenPragma)
		{
			return ItemReferenceParser.ParseItemReference(Context, out bError, tokenPragma);
		}

		private _IExpression ParseDefinedPragma(out bool bError, IToken tokenPragma)
		{
			_IDefinedExpression iDefinedExpression = LMItemFactory.CreateDefinedExpression(tokenPragma);
			if (PragmaScanner.GetNext(out var token) != PragmaTokenType.Operator || token.Operator != PragmaOperator.LeftParenthesis)
			{
				AddErrorST(iDefinedExpression, MessageId.Err_OperatorExpected, "(", PragmaScanner.GetTokenText(token));
			}
			iDefinedExpression.ItemReference = ParseItemReference(out bError, tokenPragma);
			if (PragmaScanner.GetNext(out token) != PragmaTokenType.Operator || token.Operator != PragmaOperator.RightParenthesis)
			{
				AddErrorST(iDefinedExpression, MessageId.Err_OperatorExpected, ")", PragmaScanner.GetTokenText(token));
			}
			return iDefinedExpression;
		}

		private _IExpression ParseProjectDefinedPragma(out bool bError, IToken tokenPragma)
		{
			_IProjectDefinedExpression iProjectDefinedExpression = LMItemFactory.CreateProjectDefinedExpression(tokenPragma);
			if (PragmaScanner.GetNext(out var token) != PragmaTokenType.Operator || token.Operator != PragmaOperator.LeftParenthesis)
			{
				AddErrorST(iProjectDefinedExpression, MessageId.Err_OperatorExpected, "(", PragmaScanner.GetTokenText(token));
				bError = true;
				return iProjectDefinedExpression;
			}
			if (PragmaScanner.GetNext(out token) != PragmaTokenType.Identifier)
			{
				AddErrorST(iProjectDefinedExpression, MessageId.Err_IdentifierExpected, PragmaScanner.GetTokenText(token));
				bError = true;
				return iProjectDefinedExpression;
			}
			iProjectDefinedExpression.DefineReference = LMItemFactory.CreateDefineReference(tokenPragma, token.Identifier);
			if (PragmaScanner.GetNext(out token) != PragmaTokenType.Operator || token.Operator != PragmaOperator.RightParenthesis)
			{
				AddErrorST(iProjectDefinedExpression, MessageId.Err_OperatorExpected, ")", PragmaScanner.GetTokenText(token));
				bError = true;
			}
			bError = false;
			return iProjectDefinedExpression;
		}
	}
}
