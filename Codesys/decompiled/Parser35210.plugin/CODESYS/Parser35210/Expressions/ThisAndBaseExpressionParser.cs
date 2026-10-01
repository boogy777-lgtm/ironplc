using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using CODESYS.Parser35210.Utilities;

namespace CODESYS.Parser35210.Expressions
{
	internal readonly struct ThisAndBaseExpressionParser
	{
		private ParserContext Context { get; }

		private _ILanguageModelBuilder6 LMItemFactory => Context.LMItemFactory;

		private ExpressionParser ExpressionParser => Context.ExpressionParser;

		private ThisAndBaseExpressionParser(ParserContext context)
		{
			Context = context;
		}

		public static _IExpression Parse(ParserContext context, out bool bError, Operator op, _IToken token, _IToken startToken, out _IToken endToken)
		{
			return new ThisAndBaseExpressionParser(context).Parse(out bError, op, token, startToken, out endToken);
		}

		private _IExpression Parse(out bool bError, Operator op, _IToken token, _IToken startToken, out _IToken endToken)
		{
			if (op == Operator.Super)
			{
				return ParseSuperExpression(out bError, token, startToken, out endToken);
			}
			return ParseThisExpression(out bError, token, startToken, out endToken);
		}

		private _IExpression ParseSuperExpression(out bool bError, _IToken token, _IToken startToken, out _IToken endToken)
		{
			endToken = token;
			_IBaseExpression exp = LMItemFactory.CreateBaseExpression(token);
			return ExpressionParser.ParseVarAccess(exp, out bError, startToken, ref endToken);
		}

		private _IExpression ParseThisExpression(out bool bError, _IToken token, _IToken startToken, out _IToken endToken)
		{
			endToken = token;
			_IThisExpression exp = LMItemFactory.CreateThisExpression(token);
			return ExpressionParser.ParseVarAccess(exp, out bError, startToken, ref endToken);
		}
	}
}
