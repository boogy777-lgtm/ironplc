using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using CODESYS.Parser;
using CODESYS.Parser35210.Declaration;
using CODESYS.Parser35210.Utilities;

namespace CODESYS.Parser35210.Expressions
{
	internal readonly struct ImplicitCastOperatorParser
	{
		private ParserContext Context { get; }

		private IScanner9 Scanner => Context.Scanner;

		private IErrorHandler ErrorHandler => Context.ErrorHandler;

		private _ILanguageModelBuilder6 LMItemFactory => Context.LMItemFactory;

		private ExpressionParser ExpressionParser => Context.ExpressionParser;

		private TypeParser TypeParser => Context.TypeParser;

		private ImplicitCastOperatorParser(ParserContext context)
		{
			Context = context;
		}

		private TokenType Next(out IToken token)
		{
			return Scanner.Next(out token);
		}

		public static _IExpression ParseStatic(ParserContext context, out bool bError, Operator op, _IToken token, _IToken startToken, out _IToken endToken)
		{
			return new ImplicitCastOperatorParser(context).Parse(out bError, op, token, startToken, out endToken);
		}

		public _IExpression Parse(out bool bError, Operator op, _IToken token, _IToken startToken, out _IToken endToken)
		{
			return ParseImplicitCastOperator(out bError, token, startToken, out endToken);
		}

		private _IExpression ParseImplicitCastOperator(out bool bError, _IToken token, _IToken startToken, out _IToken endToken)
		{
			bError = false;
			endToken = token;
			_ICastExpression iCastExpression = LMItemFactory.CreateCastExpression();
			if (Next(out var token2) != TokenType.Operator || Scanner.GetOperator(token2) != Operator.LeftParenthesis)
			{
				ErrorHandler.AddErrorSTWithToken(iCastExpression, token2, MessageId.Err_OperatorExpected, Scanner.GetOperatorText(Operator.LeftParenthesis), Scanner.GetTokenText(token2));
			}
			_IExpression baseExpression = ExpressionParser.ParseSTOperand(out bError);
			if (Next(out token2) != TokenType.Operator || Scanner.GetOperator(token2) != Operator.Comma)
			{
				ErrorHandler.AddErrorSTWithToken(iCastExpression, token2, MessageId.Err_OperatorExpected, Scanner.GetOperatorText(Operator.Comma), Scanner.GetTokenText(token2));
			}
			_IExpression expWithType = ExpressionParser.ParseSTOperand(out bError);
			if (bError)
			{
				ICompiledType compiledType2 = (iCastExpression.ExplicitelySpecifiedType = TypeParser.ParseType());
				if (compiledType2 != null)
				{
					expWithType = null;
					bError = false;
				}
			}
			if (Next(out token2) != TokenType.Operator || Scanner.GetOperator(token2) != Operator.RightParenthesis)
			{
				ErrorHandler.AddErrorSTWithToken(iCastExpression, token2, MessageId.Err_OperatorExpected, Scanner.GetOperatorText(Operator.RightParenthesis), Scanner.GetTokenText(token2));
			}
			endToken = token2 as _IToken;
			iCastExpression.ExpWithType = expWithType;
			iCastExpression.BaseExpression = baseExpression;
			return ExpressionParser.ParseVarAccess(iCastExpression, out bError, startToken, ref endToken);
		}
	}
}
