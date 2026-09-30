using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using CODESYS.Parser;
using CODESYS.Parser35210.Utilities;

namespace CODESYS.Parser35210.Expressions
{
	internal readonly struct FunctionCallParser
	{
		private ParserContext Context { get; }

		private ExpressionParser ExpressionParser => Context.ExpressionParser;

		private _ILanguageModelBuilder6 LMItemFactory => Context.LMItemFactory;

		private IScanner9 Scanner => Context.Scanner;

		private IErrorHandler ErrorHandler => Context.ErrorHandler;

		private FunctionCallParser(ParserContext context)
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

		internal static _IExpression ParseFunctionCall(ParserContext context, _IExpression exp, out bool bError, _IToken currentToken, ref _IToken endToken)
		{
			return new FunctionCallParser(context).ParseFunctionCall(exp, out bError, currentToken, ref endToken);
		}

		private _IExpression ParseFunctionCall(_IExpression exp, out bool bError, _IToken currentToken, ref _IToken endToken)
		{
			bError = false;
			_ICallExpression iCallExpression = LMItemFactory.CreateCallExpression(exp, currentToken);
			if (Scanner.TryNextOperator(Operator.RightParenthesis, out var token))
			{
				return iCallExpression;
			}
			Scanner.SetPosition(token);
			while (!ParseOperand(ref bError, ref endToken, iCallExpression))
			{
			}
			return iCallExpression;
		}

		private bool ParseOperand(ref bool bError, ref _IToken endToken, _ICallExpression call)
		{
			string stIdent = string.Empty;
			bool bOutput = false;
			if (Next(out var token) == TokenType.Identifier)
			{
				if (Next(out var token2) == TokenType.Operator)
				{
					Operator @operator = Scanner.GetOperator(token2);
					if (ParseWeirdAssignment(call, @operator, token, ref stIdent, ref bOutput, out var bDone))
					{
						return bDone;
					}
				}
				else
				{
					Scanner.SetPosition(token);
				}
			}
			else
			{
				Scanner.SetPosition(token);
			}
			bool bError2;
			_IExpression iExpression = ExpressionParser.ParseAssignment(out bError2);
			_IVariableExpression iVariableExpression = null;
			if (stIdent != string.Empty)
			{
				iVariableExpression = LMItemFactory.CreateVariableExpression(stIdent, token);
			}
			if (bOutput)
			{
				call.AddOutput(iVariableExpression, iExpression);
			}
			else
			{
				call.AddParam(iExpression, iVariableExpression);
			}
			return CheckForBreak(ref bError, ref endToken, call, bError2, token);
		}

		private bool ParseWeirdAssignment(_ICallExpression call, Operator opHelp, IToken tokenTest, ref string stIdent, ref bool bOutput, out bool bDone)
		{
			if (opHelp == Operator.Assign || opHelp == Operator.AssignOut)
			{
				stIdent = Scanner.GetIdentifier(tokenTest);
				bOutput = opHelp == Operator.AssignOut;
				if (Scanner.TryNextOperator(out opHelp, out var token))
				{
					if (CheckForFinalComma(call, opHelp, tokenTest, stIdent, out bDone))
					{
						return true;
					}
					if (CheckForEmptyAssignment(call, opHelp, tokenTest, stIdent))
					{
						bDone = true;
						return true;
					}
					bDone = false;
					Scanner.SetPosition(token);
				}
				else
				{
					Scanner.SetPosition(token);
				}
			}
			else
			{
				Scanner.SetPosition(tokenTest);
			}
			bDone = false;
			return false;
		}

		private bool CheckForEmptyAssignment(_ICallExpression call, Operator opHelp, IToken tokenTest, string stIdent)
		{
			if (opHelp == Operator.RightParenthesis)
			{
				call.AddEmptyAssign(LMItemFactory.CreateVariableExpression(stIdent, tokenTest));
				return true;
			}
			return false;
		}

		private bool CheckForFinalComma(_ICallExpression call, Operator opHelp, IToken tokenTest, string stIdent, out bool bDone)
		{
			if (opHelp == Operator.Comma)
			{
				if (Scanner.TryNextOperator(Operator.RightParenthesis, out var token))
				{
					bDone = true;
					return true;
				}
				Scanner.SetPosition(token);
				call.AddEmptyAssign(LMItemFactory.CreateVariableExpression(stIdent, tokenTest));
				bDone = false;
				return true;
			}
			bDone = false;
			return false;
		}

		private bool CheckForBreak(ref bool bError, ref _IToken endToken, _ICallExpression call, bool bErrorLocal, IToken tokenTest)
		{
			Operator op;
			if (bErrorLocal)
			{
				op = Scanner.ParseReSyncST(Operator.Comma, Operator.RightParenthesis);
			}
			else
			{
				Scanner.TryNextOperator(out op, out tokenTest);
			}
			switch (op)
			{
			case Operator.Comma:
			{
				if (Next(out var token) == TokenType.Operator && Scanner.GetOperator(token) == Operator.RightParenthesis)
				{
					return true;
				}
				Scanner.SetPosition(token);
				return false;
			}
			case Operator.RightParenthesis:
				endToken = tokenTest as _IToken;
				return true;
			default:
				if (!bErrorLocal)
				{
					ErrorHandler.AddErrorSTWithToken(call, tokenTest, MessageId.Err_Operator1of2Expected, Scanner.GetOperatorText(Operator.Comma), Scanner.GetOperatorText(Operator.RightParenthesis), Scanner.GetTokenText(tokenTest));
					switch (ParseReSyncST(Operator.Comma, Operator.RightParenthesis))
					{
					case Operator.Comma:
						return false;
					case Operator.RightParenthesis:
						return true;
					}
				}
				Scanner.SetPosition(tokenTest);
				bError = true;
				return true;
			}
		}
	}
}
