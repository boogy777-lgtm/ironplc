using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using CODESYS.Parser;
using CODESYS.Parser35210.Utilities;

namespace CODESYS.Parser35210.Expressions
{
	internal readonly struct ConversionExpressionParser
	{
		private ParserContext Context { get; }

		private _ILanguageModelBuilder6 LMItemFactory => Context.LMItemFactory;

		private IScanner9 Scanner => Context.Scanner;

		private ITypeTable3 TypeTable => Context.TypeTable;

		private IErrorHandler ErrorHandler => Context.ErrorHandler;

		private ExpressionParser ExpressionParser => Context.ExpressionParser;

		private ConversionExpressionParser(ParserContext context)
		{
			Context = context;
		}

		private TokenType Next(out IToken token)
		{
			return Scanner.Next(out token);
		}

		private Operator ParseReSyncST(params Operator[] ops)
		{
			return Scanner.ParseReSyncST(ops);
		}

		private Operator ParseReSyncST(out IToken tokenPos, params Operator[] ops)
		{
			Next(out tokenPos);
			Scanner.SetPosition(tokenPos);
			return Scanner.ParseReSyncST(ops);
		}

		public static _IExpression ParseStatic(ParserContext context, out bool bError, Operator op, _IToken token, _IToken startToken, out _IToken endToken)
		{
			return new ConversionExpressionParser(context).Parse(out bError, op, token, startToken, out endToken);
		}

		public _IExpression Parse(out bool bError, Operator op, _IToken token, _IToken startToken, out _IToken endToken)
		{
			return ParseConversionExpression(out bError, token, out endToken);
		}

		private _IExpression ParseConversionExpression(out bool bError, _IToken token, out _IToken endToken)
		{
			bError = false;
			endToken = token;
			Scanner.GetConversion(token, out var sourceType, out var destType);
			_IConversionExpression iConversionExpression = LMItemFactory.CreateConversionExpression(TypeTable.GetTypeByOperator(sourceType), TypeTable.GetTypeByOperator(destType), token);
			if (Next(out var token2) != TokenType.Operator || Scanner.GetOperator(token2) != Operator.LeftParenthesis)
			{
				ErrorHandler.AddErrorSTWithToken(iConversionExpression, token2, MessageId.Err_OperatorExpected, Scanner.GetOperatorText(Operator.LeftParenthesis), Scanner.GetTokenText(token2));
				switch (ParseReSyncST(Operator.LeftParenthesis, Operator.RightParenthesis))
				{
				case Operator.RightParenthesis:
					return iConversionExpression;
				default:
					Scanner.SetPosition(token2);
					bError = true;
					return iConversionExpression;
				case Operator.LeftParenthesis:
					break;
				}
			}
			bool bError2;
			_IExpression iExpression2 = (iConversionExpression._Exp = ExpressionParser.ParseAssignment(out bError2));
			if (Next(out token2) != TokenType.Operator || Scanner.GetOperator(token2) != Operator.RightParenthesis)
			{
				ErrorHandler.AddErrorSTWithToken(iConversionExpression, token2, MessageId.Err_OperatorExpected, Scanner.GetOperatorText(Operator.RightParenthesis), Scanner.GetTokenText(token2));
				bError2 = true;
			}
			else
			{
				endToken = token2 as _IToken;
			}
			if ((iExpression2 == null || bError2) && ParseReSyncST(out var tokenPos, Operator.RightParenthesis) != Operator.RightParenthesis)
			{
				Scanner.SetPosition(tokenPos);
				bError = true;
			}
			return iConversionExpression;
		}
	}
}
