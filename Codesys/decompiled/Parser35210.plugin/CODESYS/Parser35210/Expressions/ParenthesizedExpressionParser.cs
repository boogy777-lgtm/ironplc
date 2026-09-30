using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using CODESYS.Parser;
using CODESYS.Parser35210.Utilities;

namespace CODESYS.Parser35210.Expressions
{
	internal readonly struct ParenthesizedExpressionParser
	{
		private ParserContext Context { get; }

		private _ILanguageModelBuilder6 LMItemFactory => Context.LMItemFactory;

		private IScanner9 Scanner => Context.Scanner;

		private IErrorHandler ErrorHandler => Context.ErrorHandler;

		private ExpressionParser ExpressionParser => Context.ExpressionParser;

		private ParenthesizedExpressionParser(ParserContext context)
		{
			Context = context;
		}

		private TokenType Next(out IToken token)
		{
			return Scanner.Next(out token);
		}

		private Operator ParseReSyncST(out IToken tokenPos, params Operator[] ops)
		{
			Next(out tokenPos);
			Scanner.SetPosition(tokenPos);
			return Scanner.ParseReSyncST(ops);
		}

		internal static _IExpression ParseParenthesizedExpressionStatic(ParserContext context, out bool bError, _IToken token, out _IToken endToken, out short lenghtOfExpWithoutParanthesis)
		{
			return new ParenthesizedExpressionParser(context).ParseParenthesizedExpression(out bError, token, out endToken, out lenghtOfExpWithoutParanthesis);
		}

		private _IExpression ParseParenthesizedExpression(out bool bError, _IToken token, out _IToken endToken, out short lenghtOfExpWithoutParanthesis)
		{
			bError = false;
			endToken = token;
			lenghtOfExpWithoutParanthesis = 0;
			_IExpression iExpression = ExpressionParser.ParseAssignment(out bError);
			if (iExpression == null)
			{
				iExpression = LMItemFactory.CreateErrorExpression(token);
			}
			else if (iExpression is IOperatorExpression operatorExpression && !Helper.IsPrefixOperator(operatorExpression.Code))
			{
				iExpression.SetPosition(token);
			}
			IToken tokenPos;
			if (bError)
			{
				if (ParseReSyncST(out tokenPos, Operator.RightParenthesis) != Operator.RightParenthesis)
				{
					bError = true;
					Scanner.SetPosition(tokenPos);
				}
			}
			else if (Next(out tokenPos) != TokenType.Operator || Scanner.GetOperator(tokenPos) != Operator.RightParenthesis)
			{
				ErrorHandler.AddErrorSTWithToken(iExpression, tokenPos, MessageId.Err_OperatorExpected, Scanner.GetOperatorText(Operator.RightParenthesis), Scanner.GetTokenText(tokenPos));
				bError = true;
				Scanner.SetPosition(tokenPos);
			}
			endToken = tokenPos as _IToken;
			lenghtOfExpWithoutParanthesis = iExpression.LengthIntern;
			return iExpression;
		}
	}
}
