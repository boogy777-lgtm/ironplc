using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using CODESYS.Parser35210.Utilities;

namespace CODESYS.Parser35210.Expressions
{
	internal readonly struct UnaryNotParser
	{
		private ParserContext Context { get; }

		private _ILanguageModelBuilder6 LMItemFactory => Context.LMItemFactory;

		private ExpressionParser ExpressionParser => Context.ExpressionParser;

		private UnaryNotParser(ParserContext context)
		{
			Context = context;
		}

		public static _IExpression Parse(ParserContext context, out bool bError, Operator op, _IToken token, _IToken startToken, out _IToken endToken)
		{
			return new UnaryNotParser(context).Parse(out bError, token, startToken, out endToken);
		}

		private _IExpression Parse(out bool bError, _IToken token, _IToken startToken, out _IToken endToken)
		{
			return ParseUnaryNotOperator(out bError, token, startToken, out endToken);
		}

		private _IExpression ParseUnaryNotOperator(out bool bError, _IToken token, _IToken startToken, out _IToken endToken)
		{
			bError = false;
			endToken = token;
			_IExpression exp = ExpressionParser.ParseSTOperandWithPosition(out bError, null, out endToken) ?? LMItemFactory.CreateErrorExpression(token);
			short positionLength = Helper.CalculateLength(startToken, endToken);
			_IOperatorExpression iOperatorExpression = LMItemFactory.CreateOperatorExpression(Operator.Not, token);
			iOperatorExpression.AddOperand(exp);
			iOperatorExpression.PositionLength = positionLength;
			return iOperatorExpression;
		}
	}
}
