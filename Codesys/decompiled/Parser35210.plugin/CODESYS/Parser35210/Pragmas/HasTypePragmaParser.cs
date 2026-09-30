using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using CODESYS.Parser;
using CODESYS.Parser35210.Utilities;

namespace CODESYS.Parser35210.Pragmas
{
	internal readonly struct HasTypePragmaParser
	{
		private ParserContext Context { get; }

		private _ILanguageModelBuilder6 LMItemFactory => Context.LMItemFactory;

		private PragmaStatementParser PragmaParser => Context.PragmaStatementParser;

		private IPragmaScanner PragmaScanner => PragmaParser.PragmaScanner;

		private IScanner9 Scanner => Context.Scanner;

		private IErrorHandler ErrorHandler => Context.ErrorHandler;

		private ITokenFactory TokenFactory => Context.TokenFactory;

		private HasTypePragmaParser(ParserContext context)
		{
			Context = context;
		}

		internal static _IExpression ParseHasTypePragma(ParserContext context, out bool bError, IToken tokenPragma)
		{
			return new HasTypePragmaParser(context).ParseHasTypePragma(out bError, tokenPragma);
		}

		internal static _IExpression ParseIsEnumTypePragma(ParserContext context, IToken tokenPragma)
		{
			return new HasTypePragmaParser(context).ParseIsEnumTypePragma(tokenPragma);
		}

		private void AddErrorST(_IExprement exp, MessageId nErrorId, params object[] args)
		{
			ErrorHandler.AddErrorST(exp, nErrorId, args);
		}

		private _IExpression ParseHasTypePragma(out bool bError, IToken tokenPragma)
		{
			_IHasTypeExpression iHasTypeExpression = LMItemFactory.CreateHasTypeExpression(tokenPragma);
			if (PragmaScanner.GetNext(out var token) != PragmaTokenType.Operator || token.Operator != PragmaOperator.LeftParenthesis)
			{
				AddErrorST(iHasTypeExpression, MessageId.Err_OperatorExpected, "(", PragmaScanner.GetTokenText(token));
			}
			iHasTypeExpression.Variable = ParseItemReference(out bError, tokenPragma) as _IVariableReference;
			if (PragmaScanner.GetNext(out token) != PragmaTokenType.Operator || token.Operator != PragmaOperator.Comma)
			{
				AddErrorST(iHasTypeExpression, MessageId.Err_OperatorExpected, ",", PragmaScanner.GetTokenText(token));
			}
			ICompiledType compiledType = ParseType();
			if (compiledType is _IUserdefType iUserdefType)
			{
				new PragmaSourcePositionAdjuster(Scanner.CurrentToken.Position, PragmaScanner.PositionOffset, TokenFactory).AdjustSourcePositions(iUserdefType.NameExpression as _IExpression);
			}
			if (PragmaScanner.GetNext(out token) != PragmaTokenType.Operator)
			{
				AddErrorST(iHasTypeExpression, MessageId.Err_OperatorExpected, ")", PragmaScanner.GetTokenText(token));
				iHasTypeExpression.ReferencedType = compiledType;
				return iHasTypeExpression;
			}
			if (token.Operator == PragmaOperator.Comma)
			{
				if (PragmaScanner.GetNext(out token) != PragmaTokenType.Operator || (token.Operator != PragmaOperator.True && token.Operator != PragmaOperator.False))
				{
					AddErrorST(iHasTypeExpression, MessageId.Err_OperatorExpected, "FALSE", PragmaScanner.GetTokenText(token));
				}
				else if (token.Operator == PragmaOperator.False)
				{
					_IHasCompatibleTypeExpression iHasCompatibleTypeExpression = LMItemFactory.CreateHasCompatibleTypeExpression();
					iHasCompatibleTypeExpression.AssignFrom(iHasTypeExpression);
					iHasTypeExpression = iHasCompatibleTypeExpression;
				}
				PragmaScanner.GetNext(out token);
			}
			if (token.Type != PragmaTokenType.Operator || token.Operator != PragmaOperator.RightParenthesis)
			{
				AddErrorST(iHasTypeExpression, MessageId.Err_OperatorExpected, ")", PragmaScanner.GetTokenText(token));
			}
			iHasTypeExpression.ReferencedType = compiledType;
			return iHasTypeExpression;
		}

		private ICompiledType ParseType()
		{
			IScanner9 scanner = Context.Scanner;
			Context.Scanner = PragmaScanner.OrgScanner;
			_IType result = Context.TypeParser.ParseType();
			Context.Scanner = scanner;
			return result;
		}

		private _IExpression ParseIsEnumTypePragma(IToken tokenPragma)
		{
			_IIsEnumTypeExpression iIsEnumTypeExpression = LMItemFactory.CreateIsEnumTypeExpression(tokenPragma);
			if (PragmaScanner.GetNext(out var token) != PragmaTokenType.Operator || token.Operator != PragmaOperator.LeftParenthesis)
			{
				AddErrorST(iIsEnumTypeExpression, MessageId.Err_OperatorExpected, "(", PragmaScanner.GetTokenText(token));
			}
			iIsEnumTypeExpression.ReferencedType = ParseType();
			if (PragmaScanner.GetNext(out token) != PragmaTokenType.Operator || token.Operator != PragmaOperator.RightParenthesis)
			{
				AddErrorST(iIsEnumTypeExpression, MessageId.Err_OperatorExpected, ")", PragmaScanner.GetTokenText(token));
			}
			return iIsEnumTypeExpression;
		}

		private _IItemReference ParseItemReference(out bool bError, IToken tokenPragma)
		{
			return ItemReferenceParser.ParseItemReference(Context, out bError, tokenPragma);
		}
	}
}
