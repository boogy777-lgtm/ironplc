using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using CODESYS.Parser35210.Utilities;

namespace CODESYS.Parser35210.Expressions
{
	internal readonly struct InfixOperationParser
	{
		private ParserContext Context { get; }

		private _ILanguageModelBuilder6 LMItemFactory => Context.LMItemFactory;

		private IScanner9 Scanner => Context.Scanner;

		private OperandParser OperandParser => Context.OperandParser;

		private InfixOperationParser(ParserContext context)
		{
			Context = context;
		}

		private TokenType Next(out IToken token)
		{
			return Scanner.Next(out token);
		}

		private static bool IsMulOperator(Operator op)
		{
			if (op != Operator.Times && op != Operator.Divide && op != Operator.Mod && op != Operator.__vcMul && op != Operator.__vcDiv)
			{
				return op == Operator.__vcDot;
			}
			return true;
		}

		private static bool IsAddOperator(Operator op)
		{
			if (op != Operator.Plus && op != Operator.Minus && op != Operator.__vcAdd)
			{
				return op == Operator.__vcSub;
			}
			return true;
		}

		private _IExpression ParseMULExp(out bool bError)
		{
			_IOperatorExpression extop = null;
			_IExpression iExpression = OperandParser.ParseSTOperand(out bError);
			if (iExpression == null)
			{
				return null;
			}
			Operator op;
			IToken token;
			for (bool flag = Scanner.TryNextOperator(out op, out token); flag && IsMulOperator(op); flag = Scanner.TryNextOperator(out op, out token))
			{
				_IExpression iExpression2 = OperandParser.ParseSTOperand(out bError);
				if (iExpression2 == null)
				{
					return null;
				}
				AddOperandHelp(ref extop, iExpression, iExpression2, op, token);
			}
			Scanner.SetPosition(token);
			if (extop == null)
			{
				return iExpression;
			}
			return extop;
		}

		private _IExpression ParseADDExp(out bool bError)
		{
			_IOperatorExpression extop = null;
			_IExpression iExpression = ParseMULExp(out bError);
			if (iExpression == null)
			{
				return null;
			}
			Operator op;
			IToken token;
			for (bool flag = Scanner.TryNextOperator(out op, out token); flag && IsAddOperator(op); flag = Scanner.TryNextOperator(out op, out token))
			{
				_IExpression iExpression2 = ParseMULExp(out bError);
				if (iExpression2 == null)
				{
					return null;
				}
				AddOperandHelp(ref extop, iExpression, iExpression2, op, token);
			}
			Scanner.SetPosition(token);
			if (extop == null)
			{
				return iExpression;
			}
			return extop;
		}

		private _IExpression ParseCompareExp(out bool bError)
		{
			_IOperatorExpression extop = null;
			_IExpression iExpression = ParseADDExp(out bError);
			if (iExpression == null)
			{
				return null;
			}
			IToken token;
			TokenType tokenType = Next(out token);
			Operator @operator;
			while (tokenType == TokenType.Operator && ((@operator = Scanner.GetOperator(token)) == Operator.Equal || @operator == Operator.NotEqual || @operator == Operator.Less || @operator == Operator.LessEqual || @operator == Operator.Greater || @operator == Operator.GreaterEqual))
			{
				_IExpression iExpression2 = ParseADDExp(out bError);
				if (iExpression2 == null)
				{
					return null;
				}
				AddOperandHelp(ref extop, iExpression, iExpression2, @operator, token);
				tokenType = Next(out token);
			}
			Scanner.SetPosition(token);
			if (extop == null)
			{
				return iExpression;
			}
			return extop;
		}

		private _IExpression ParseANDExp(out bool bError)
		{
			_IOperatorExpression extop = null;
			_IExpression iExpression = ParseCompareExp(out bError);
			if (iExpression == null)
			{
				return null;
			}
			IToken token;
			TokenType tokenType = Next(out token);
			Operator @operator;
			while (tokenType == TokenType.Operator && ((@operator = Scanner.GetOperator(token)) == Operator.And || @operator == Operator.And_Then))
			{
				_IExpression iExpression2 = ParseCompareExp(out bError);
				if (iExpression2 == null)
				{
					return null;
				}
				AddOperandHelp(ref extop, iExpression, iExpression2, @operator, token);
				tokenType = Next(out token);
			}
			Scanner.SetPosition(token);
			if (extop == null)
			{
				return iExpression;
			}
			return extop;
		}

		internal static _IExpression ParseORExp(ParserContext context, out bool bError)
		{
			return new InfixOperationParser(context).ParseORExp(out bError);
		}

		private _IExpression ParseORExp(out bool bError)
		{
			_IOperatorExpression extop = null;
			_IExpression iExpression = ParseANDExp(out bError);
			if (iExpression == null)
			{
				return null;
			}
			IToken token;
			TokenType tokenType = Next(out token);
			Operator @operator;
			while (tokenType == TokenType.Operator && ((@operator = Scanner.GetOperator(token)) == Operator.Or || @operator == Operator.Or_Else || @operator == Operator.Xor))
			{
				_IExpression iExpression2 = ParseANDExp(out bError);
				if (iExpression2 == null)
				{
					return null;
				}
				AddOperandHelp(ref extop, iExpression, iExpression2, @operator, token);
				tokenType = Next(out token);
			}
			Scanner.SetPosition(token);
			if (extop == null)
			{
				return iExpression;
			}
			return extop;
		}

		private void AddOperandHelp(ref _IOperatorExpression extop, _IExpression exp1, _IExpression exp2, Operator op, IToken token)
		{
			LMItemFactory.AddOperandHelp(ref extop, exp1, exp2, op, token);
		}
	}
}
