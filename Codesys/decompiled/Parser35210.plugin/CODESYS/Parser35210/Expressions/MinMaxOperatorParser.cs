using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using CODESYS.Parser;
using CODESYS.Parser35210.Utilities;

namespace CODESYS.Parser35210.Expressions
{
	internal readonly struct MinMaxOperatorParser
	{
		private ParserContext Context { get; }

		private ExpressionParser ExpressionParser => Context.ExpressionParser;

		private _ILanguageModelBuilder6 LMItemFactory => Context.LMItemFactory;

		private IScanner9 Scanner => Context.Scanner;

		private IErrorHandler ErrorHandler => Context.ErrorHandler;

		private MinMaxOperatorParser(ParserContext context)
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

		public static _IExpression ParseStatic(ParserContext context, out bool bError, Operator op, _IToken token, _IToken startToken, out _IToken endToken)
		{
			return new MinMaxOperatorParser(context).Parse(out bError, op, token, startToken, out endToken);
		}

		public _IExpression Parse(out bool bError, Operator op, _IToken token, _IToken startToken, out _IToken endToken)
		{
			return ParseMinMaxOperator(out bError, op, token, out endToken);
		}

		private _IExpression ParseMinMaxOperator(out bool bError, Operator op, _IToken token, out _IToken endToken)
		{
			bError = false;
			endToken = token;
			if (Next(out var token2) != TokenType.Operator || Scanner.GetOperator(token2) != Operator.LeftParenthesis)
			{
				_IOperatorExpression result = CreateOpExpressionWithError(op, token, token2);
				if (AttemptResync(out token2) == Operator.RightParenthesis)
				{
					return result;
				}
				ResyncOutside(token2);
				bError = true;
				return result;
			}
			Next(out token2);
			if (token2.Type == TokenType.Operator && Scanner.GetOperator(token2) == Operator.RightParenthesis)
			{
				endToken = token2 as _IToken;
				return null;
			}
			Scanner.SetPosition(token2);
			return ParseOperands(ref bError, op, token, ref endToken);
		}

		private _IOperatorExpression ParseOperands(ref bool bError, Operator op, _IToken token, ref _IToken endToken)
		{
			_IOperatorExpression opexp = null;
			bool bError2;
			_IExpression expOperand = ExpressionParser.ParseAssignment(out bError2);
			while (true)
			{
				if (!Scanner.TryNextOperator(out var op2, out var token2) || NotExpectedOperatorsFound(token2, ref opexp, op, token))
				{
					op2 = AttemptResync(out var tokenHelp);
					if (op2 != Operator.Comma && op2 != Operator.RightParenthesis)
					{
						ResyncOutside(tokenHelp);
						bError = true;
						break;
					}
				}
				if (op2 == Operator.RightParenthesis)
				{
					endToken = token2 as _IToken;
					break;
				}
				ProcessOperands(ref opexp, token2, expOperand, op, token);
			}
			return opexp;
		}

		private bool NotExpectedOperatorsFound(IToken tokenHelp, ref _IOperatorExpression opexp, Operator op, IToken token)
		{
			Operator @operator = Scanner.GetOperator(tokenHelp);
			if (@operator != Operator.Comma && @operator != Operator.RightParenthesis)
			{
				if (opexp == null)
				{
					opexp = LMItemFactory.CreateOperatorExpression(op, token);
				}
				ErrorHandler.AddErrorSTWithToken(opexp, tokenHelp, MessageId.Err_Operator1of2Expected, Scanner.GetOperatorText(Operator.Comma), Scanner.GetOperatorText(Operator.RightParenthesis), Scanner.GetTokenText(tokenHelp));
				return true;
			}
			return false;
		}

		private Operator AttemptResync(out IToken tokenHelp)
		{
			return ParseReSyncST(out tokenHelp, Operator.LeftParenthesis, Operator.Comma, Operator.RightParenthesis);
		}

		private void ProcessOperands(ref _IOperatorExpression opexp, IToken tokenHelp, _IExpression expOperand1, Operator op, IToken token)
		{
			bool bError;
			_IExpression exp = ExpressionParser.ParseAssignment(out bError) ?? LMItemFactory.CreateErrorExpression(tokenHelp);
			LMItemFactory.AddOperandHelp(ref opexp, expOperand1, exp, op, token);
		}

		private _IOperatorExpression CreateOpExpressionWithError(Operator op, IToken token, IToken tokenHelp)
		{
			_IOperatorExpression iOperatorExpression = LMItemFactory.CreateOperatorExpression(op, token);
			ErrorHandler.AddErrorSTWithToken(iOperatorExpression, tokenHelp, MessageId.Err_OperatorExpected, Scanner.GetOperatorText(Operator.LeftParenthesis), Scanner.GetTokenText(tokenHelp));
			return iOperatorExpression;
		}

		private void ResyncOutside(IToken tokenHelp)
		{
			Scanner.SetPosition(tokenHelp);
		}
	}
}
