using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using CODESYS.Parser;
using CODESYS.Parser35210.Utilities;

namespace CODESYS.Parser35210.Expressions
{
	internal readonly struct ScopeExpressionParser
	{
		private ParserContext Context { get; }

		private _ILanguageModelBuilder6 LMItemFactory => Context.LMItemFactory;

		private ExpressionParser ExpressionParser => Context.ExpressionParser;

		private IScanner9 Scanner => Context.Scanner;

		private IErrorHandler ErrorHandler => Context.ErrorHandler;

		private ScopeExpressionParser(ParserContext context)
		{
			Context = context;
		}

		private TokenType Next(out IToken token)
		{
			return Scanner.Next(out token);
		}

		public static _IExpression ParseStatic(ParserContext context, out bool bError, Operator op, _IToken token, _IToken startToken, out _IToken endToken)
		{
			return new ScopeExpressionParser(context).Parse(out bError, op, token, startToken, out endToken);
		}

		public _IExpression Parse(out bool bError, Operator op, _IToken token, _IToken startToken, out _IToken endToken)
		{
			switch (op)
			{
			case Operator.Period:
				return ParseGlobalScopeExpression(out bError, token, startToken, out endToken);
			case Operator.__SystemScope:
				Context.Scanner.MatchOperator(Context.ErrorHandler, Operator.Period);
				return ParseSystemScopeExpression(out bError, token, startToken, out endToken);
			case Operator.__Copy:
				Context.Scanner.MatchOperator(Context.ErrorHandler, Operator.Period);
				return ParseCopyScopeExpression(out bError, token, startToken, out endToken);
			case Operator.__PoolScope:
				Context.Scanner.MatchOperator(Context.ErrorHandler, Operator.Period);
				return ParsePoolScopeExpression(out bError, token, startToken, out endToken);
			default:
				bError = true;
				endToken = token;
				return null;
			}
		}

		private _IExpression ParsePoolScopeExpression(out bool bError, _IToken token, _IToken startToken, out _IToken endToken)
		{
			endToken = token;
			_IExpression result;
			if (Next(out var token2) != TokenType.Identifier)
			{
				_IPoolScopeExpression iPoolScopeExpression = LMItemFactory.CreatePoolScopeExpression(LMItemFactory.CreateErrorExpression(), token);
				ErrorHandler.AddErrorSTWithToken(iPoolScopeExpression, token2, MessageId.Err_IdentifierExpected, Scanner.GetTokenText(token2));
				result = iPoolScopeExpression;
				bError = true;
			}
			else
			{
				string identifier = Scanner.GetIdentifier(token2);
				_IVariableExpression expBase = LMItemFactory.CreateVariableExpression(identifier, token2);
				_IPoolScopeExpression exp = LMItemFactory.CreatePoolScopeExpression(expBase, token);
				result = ExpressionParser.ParseVarAccess(exp, out bError, startToken, ref endToken);
			}
			return result;
		}

		private _IExpression ParseGlobalScopeExpression(out bool bError, _IToken token, _IToken startToken, out _IToken endToken)
		{
			endToken = token;
			_IExpression result;
			if (Next(out var token2) != TokenType.Identifier)
			{
				_IGlobalScopeExpression iGlobalScopeExpression = LMItemFactory.CreateGlobalScopeExpression(LMItemFactory.CreateErrorExpression(), token);
				ErrorHandler.AddErrorSTWithToken(iGlobalScopeExpression, token2, MessageId.Err_IdentifierExpected, Scanner.GetTokenText(token2));
				result = iGlobalScopeExpression;
				bError = true;
			}
			else
			{
				string identifier = Scanner.GetIdentifier(token2);
				_IVariableExpression expBase = LMItemFactory.CreateVariableExpression(identifier, token2);
				_IGlobalScopeExpression exp = LMItemFactory.CreateGlobalScopeExpression(expBase, token);
				result = ExpressionParser.ParseVarAccess(exp, out bError, startToken, ref endToken);
			}
			return result;
		}

		private _IExpression ParseSystemScopeExpression(out bool bError, _IToken token, _IToken startToken, out _IToken endToken)
		{
			_IExpression expBase = ExpressionParser.ParseSTOperandWithPosition(out bError, startToken, out endToken);
			return LMItemFactory.CreateSystemScopeExpression(expBase, token);
		}

		private _IExpression ParseCopyScopeExpression(out bool bError, _IToken token, _IToken startToken, out _IToken endToken)
		{
			endToken = token;
			bError = false;
			_ICopyScopeExpression iCopyScopeExpression = LMItemFactory.CreateCopyScopeExpression(null, token);
			iCopyScopeExpression._Base = ExpressionParser.ParseSTOperandWithPosition(out bError, startToken, out endToken);
			return iCopyScopeExpression;
		}
	}
}
