using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using CODESYS.Parser35210.Utilities;

namespace CODESYS.Parser35210.Expressions
{
	internal readonly struct CurrentTaskExpressionParser
	{
		private ParserContext Context { get; }

		private _ILanguageModelBuilder6 LMItemFactory => Context.LMItemFactory;

		private ExpressionParser ExpressionParser => Context.ExpressionParser;

		private CurrentTaskExpressionParser(ParserContext context)
		{
			Context = context;
		}

		public static _IExpression ParseStatic(ParserContext context, out bool bError, Operator op, _IToken token, _IToken startToken, out _IToken endToken)
		{
			return new CurrentTaskExpressionParser(context).Parse(out bError, op, token, startToken, out endToken);
		}

		public _IExpression Parse(out bool bError, Operator op, _IToken token, _IToken startToken, out _IToken endToken)
		{
			return ParseCurrentTaskExpression(out bError, token, startToken, out endToken);
		}

		private _IExpression ParseCurrentTaskExpression(out bool bError, _IToken token, _IToken startToken, out _IToken endToken)
		{
			Context.Scanner.MatchOperator(Context.ErrorHandler, Operator.Period);
			_IExpression expBase = ExpressionParser.ParseSTOperandWithPosition(out bError, startToken, out endToken);
			return LMItemFactory.CreateCurrentTaskExpression(expBase, token);
		}
	}
}
