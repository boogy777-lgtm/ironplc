using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using CODESYS.Parser;
using CODESYS.Parser35210.Declaration;
using CODESYS.Parser35210.Utilities;

namespace CODESYS.Parser35210.Expressions
{
	internal readonly struct NewExpressionParser
	{
		private ParserContext Context { get; }

		private _ILanguageModelBuilder6 LMItemFactory => Context.LMItemFactory;

		private IScanner9 Scanner => Context.Scanner;

		private IErrorHandler ErrorHandler => Context.ErrorHandler;

		private ExpressionParser ExpressionParser => Context.ExpressionParser;

		private TypeParser TypeParser => Context.TypeParser;

		private NewExpressionParser(ParserContext context)
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

		public static _IExpression Parse(ParserContext context, out bool bError, _IToken token, out _IToken endToken)
		{
			return new NewExpressionParser(context).Parse(out bError, token, out endToken);
		}

		private _IExpression Parse(out bool bError, _IToken token, out _IToken endToken)
		{
			return ParseNewOperator(out bError, token, out endToken);
		}

		private _IExpression ParseNewOperator(out bool bError, _IToken token, out _IToken endToken)
		{
			bError = false;
			endToken = token;
			_INewExpression iNewExpression = LMItemFactory.CreateNewExpression(null, null, token);
			if (!Scanner.TryNextOperator(Operator.LeftParenthesis, out var token2))
			{
				ErrorHandler.AddErrorSTWithToken(iNewExpression, token2, MessageId.Err_OperatorExpected, Scanner.GetOperatorText(Operator.LeftParenthesis), Scanner.GetTokenText(token2));
			}
			_IType iType = TypeParser.ParseType();
			if (iType == null)
			{
				ErrorHandler.AddErrorSTWithToken(iNewExpression, token2, MessageId.Err_NewNeedsType);
			}
			if (Scanner.TryNextOperator(Operator.LeftParenthesis, out token2))
			{
				ParseInputsForIniCall(bError, ref endToken, iNewExpression);
			}
			else
			{
				Scanner.SetPosition(token2);
			}
			_IExpression count;
			if (Scanner.TryNextOperator(Operator.Comma, out token2))
			{
				count = ExpressionParser.ParseSTOperand(out bError);
			}
			else
			{
				Scanner.SetPosition(token2);
				count = LMItemFactory.CreateLiteralExpression(1L);
			}
			if (!Scanner.TryNextOperator(Operator.RightParenthesis, out token2))
			{
				ErrorHandler.AddErrorSTWithToken(iNewExpression, token2, MessageId.Err_OperatorExpected, Scanner.GetOperatorText(Operator.RightParenthesis), Scanner.GetTokenText(token2));
			}
			endToken = token2 as _IToken;
			iNewExpression._Count = count;
			iNewExpression._TypeToCast = iType;
			return iNewExpression;
		}

		private void ParseInputsForIniCall(bool bError, ref _IToken endToken, _INewExpression newexp)
		{
			if (Scanner.TryNextOperator(Operator.RightParenthesis, out var token))
			{
				return;
			}
			Scanner.SetPosition(token);
			IToken tokenTest;
			string stIdent;
			while (ParseIdentifier(newexp, out tokenTest, out stIdent))
			{
				_IErrorExpression exprError = null;
				CheckForAssignmentOperator(newexp, bError, out exprError);
				bool bErrorLocal;
				_IExpression rValue = ParseInitialValue(out bErrorLocal);
				_IVariableExpression expLValue = LMItemFactory.CreateVariableExpression(stIdent, tokenTest);
				_IAssignmentExpression iAssignmentExpression = LMItemFactory.CreateAssignmentExpression(expLValue);
				iAssignmentExpression._RValue = rValue;
				newexp.AddFBInitParam(iAssignmentExpression);
				switch (GetNextOperator(bErrorLocal, ref tokenTest))
				{
				case Operator.Comma:
					continue;
				case Operator.RightParenthesis:
					endToken = tokenTest as _IToken;
					return;
				}
				if (!HandleError(newexp, bErrorLocal, tokenTest))
				{
					return;
				}
			}
		}

		private Operator GetNextOperator(bool bErrorLocal, ref IToken tokenTest)
		{
			Operator result = Operator.None;
			if (bErrorLocal)
			{
				result = ParseReSyncST(Operator.Comma, Operator.RightParenthesis);
			}
			else if (Next(out tokenTest) == TokenType.Operator)
			{
				result = Scanner.GetOperator(tokenTest);
			}
			return result;
		}

		private bool HandleError(_INewExpression newexp, bool bErrorLocal, IToken tokenTest)
		{
			if (!bErrorLocal)
			{
				ErrorHandler.AddErrorSTWithToken(newexp, tokenTest, MessageId.Err_Operator1of2Expected, Scanner.GetOperatorText(Operator.Comma), Scanner.GetOperatorText(Operator.RightParenthesis), Scanner.GetTokenText(tokenTest));
				switch (ParseReSyncST(Operator.Comma, Operator.RightParenthesis))
				{
				case Operator.Comma:
					return true;
				case Operator.RightParenthesis:
					return false;
				}
			}
			Scanner.SetPosition(tokenTest);
			return false;
		}

		private _IExpression ParseInitialValue(out bool bErrorLocal)
		{
			_IExpression iExpression = ExpressionParser.ParseInitialisation();
			bErrorLocal = false;
			if (iExpression is IStructureInitialization)
			{
				ErrorHandler.AddErrorST(iExpression, MessageId.Err_StructureInitialisationNotPossible);
				bErrorLocal = true;
			}
			else if (iExpression is IArrayInitialization)
			{
				ErrorHandler.AddErrorST(iExpression, MessageId.Err_ArrayInitialisationNotPossible);
				bErrorLocal = true;
			}
			return iExpression;
		}

		private bool ParseIdentifier(_INewExpression newexp, out IToken tokenTest, out string stIdent)
		{
			tokenTest = null;
			stIdent = null;
			if (Next(out tokenTest) != TokenType.Identifier)
			{
				ErrorHandler.AddErrorSTWithToken(newexp, tokenTest, MessageId.Err_IdentifierExpected, Scanner.GetTokenText(tokenTest));
				ParseReSyncST(Operator.RightParenthesis);
				return false;
			}
			stIdent = Scanner.GetIdentifier(tokenTest);
			return true;
		}

		private void CheckForAssignmentOperator(_IExprement exprement, bool bErrorLocal, out _IErrorExpression exprError)
		{
			exprError = null;
			Operator @operator = Operator.Assign;
			if (Scanner.TryNextOperator(@operator, out var token))
			{
				return;
			}
			if (!bErrorLocal)
			{
				exprError = LMItemFactory.CreateErrorExpression(token);
				ErrorHandler.AddErrorSTWithToken(exprement, token, MessageId.Err_OperatorExpected, Scanner.GetOperatorText(@operator), Scanner.GetTokenText(token));
				Scanner.SetPosition(token);
			}
			if (ParseReSyncST(@operator) != @operator)
			{
				if (bErrorLocal)
				{
					ErrorHandler.AddErrorSTWithToken(exprement, token, MessageId.Err_OperatorExpected, Scanner.GetOperatorText(@operator), Scanner.GetTokenText(token));
				}
				Scanner.SetPosition(token);
			}
		}
	}
}
