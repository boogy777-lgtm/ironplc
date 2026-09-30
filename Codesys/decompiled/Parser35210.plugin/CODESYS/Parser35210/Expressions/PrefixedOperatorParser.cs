using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using CODESYS.Parser;
using CODESYS.Parser35210.Declaration;
using CODESYS.Parser35210.Utilities;

namespace CODESYS.Parser35210.Expressions
{
	internal readonly struct PrefixedOperatorParser
	{
		private ParserContext Context { get; }

		private _ILanguageModelBuilder6 LMItemFactory => Context.LMItemFactory;

		private IScanner9 Scanner => Context.Scanner;

		private IErrorHandler ErrorHandler => Context.ErrorHandler;

		private ExpressionParser ExpressionParser => Context.ExpressionParser;

		private TypeParser TypeParser => Context.TypeParser;

		private PrefixedOperatorParser(ParserContext context)
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

		public static _IExpression Parse(ParserContext context, out bool bError, Operator op, _IToken token, _IToken startToken, out _IToken endToken)
		{
			return new PrefixedOperatorParser(context).Parse(out bError, op, token, out endToken);
		}

		private _IExpression Parse(out bool bError, Operator op, _IToken token, out _IToken endToken)
		{
			return ParsePrefixedOperator(out bError, op, token, out endToken);
		}

		private _IExpression ParsePrefixedOperator(out bool bError, Operator op, _IToken currentToken, out _IToken endToken)
		{
			bError = false;
			endToken = currentToken;
			_IOperatorExpression iOperatorExpression = LMItemFactory.CreateOperatorExpression(op, currentToken);
			if (!CheckForLeftParenthesis(ref bError, iOperatorExpression, out var expression))
			{
				return expression;
			}
			if (Scanner.TryNextOperator(Operator.RightParenthesis, out var token))
			{
				endToken = token as _IToken;
				return iOperatorExpression;
			}
			Scanner.SetPosition(token);
			if (HandleSizeOfOperator(op, ref endToken, token, iOperatorExpression, out var sizeofexp))
			{
				return sizeofexp;
			}
			ParseOperands(ref bError, ref endToken, token, iOperatorExpression);
			return iOperatorExpression;
		}

		private void ParseOperands(ref bool bError, ref _IToken endToken, IToken tokenHelp, _IOperatorExpression opexp)
		{
			Operator @operator;
			do
			{
				bool bError2;
				_IExpression exp = ExpressionParser.ParseAssignment(out bError2) ?? LMItemFactory.CreateErrorExpression(tokenHelp);
				opexp.AddOperand(exp);
				if (Next(out tokenHelp) != TokenType.Operator || ((@operator = Scanner.GetOperator(tokenHelp)) != Operator.Comma && @operator != Operator.RightParenthesis))
				{
					ErrorHandler.AddErrorSTWithToken(opexp, tokenHelp, MessageId.Err_Operator1of2Expected, Scanner.GetOperatorText(Operator.Comma), Scanner.GetOperatorText(Operator.RightParenthesis), Scanner.GetTokenText(tokenHelp));
					@operator = ParseReSyncST(out var tokenPos, Operator.Comma, Operator.RightParenthesis);
					if (@operator != Operator.Comma && @operator != Operator.RightParenthesis)
					{
						Scanner.SetPosition(tokenPos);
						bError = true;
						return;
					}
				}
			}
			while (@operator == Operator.Comma || @operator != Operator.RightParenthesis);
			endToken = tokenHelp as _IToken;
		}

		private bool HandleSizeOfOperator(Operator op, ref _IToken endToken, IToken tokenHelp, _IOperatorExpression opexp, out _IExpression sizeofexp)
		{
			sizeofexp = null;
			if (Helper.IsSizeOfOperator(op))
			{
				_IType iType = TypeParser.TryParseType();
				if (iType == null || iType.Class == TypeClass.Userdef)
				{
					Scanner.SetPosition(tokenHelp);
				}
				else
				{
					if (Next(out var token) == TokenType.Operator && Scanner.GetOperator(token) == Operator.RightParenthesis)
					{
						_ITypeExpression exp = LMItemFactory.CreateTypeExpression(iType, tokenHelp);
						opexp.AddOperand(exp);
						sizeofexp = opexp;
						endToken = token as _IToken;
						return true;
					}
					Scanner.SetPosition(tokenHelp);
				}
			}
			return false;
		}

		private bool CheckForLeftParenthesis(ref bool bError, _IOperatorExpression opexp, out _IExpression expression)
		{
			expression = null;
			if (!Scanner.TryNextOperator(Operator.LeftParenthesis, out var token))
			{
				ErrorHandler.AddErrorSTWithToken(opexp, token, MessageId.Err_OperatorExpected, Scanner.GetOperatorText(Operator.LeftParenthesis), Scanner.GetTokenText(token));
				if (ParseReSyncST(out token, Operator.LeftParenthesis, Operator.Comma, Operator.RightParenthesis) == Operator.RightParenthesis)
				{
					expression = opexp;
					return true;
				}
				Scanner.SetPosition(token);
				bError = true;
				expression = opexp;
				return false;
			}
			return true;
		}
	}
}
