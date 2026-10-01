using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using CODESYS.Parser35210.Utilities;

namespace CODESYS.Parser35210
{
	internal static class ForLoopExtender
	{
		internal static void ExtendForLoop(_IForStatement forstatement, _ILanguageModelBuilder7 builder)
		{
			_IAssignmentExpression iAssignmentExpression = forstatement._CounterStart as _IAssignmentExpression;
			_IExpression iExpression = null;
			forstatement._Counter = null;
			forstatement._Condition = null;
			if (iAssignmentExpression != null)
			{
				iExpression = ExtendCondition(forstatement, iAssignmentExpression, out var opCondition, builder);
				forstatement._Condition = opCondition;
			}
			if (iExpression != null && forstatement._By != null)
			{
				_IAssignmentExpression iAssignmentExpression2 = (_IAssignmentExpression)(forstatement._Counter = ExtendCounter(forstatement, iExpression, builder));
			}
		}

		private static _IAssignmentExpression ExtendCounter(_IForStatement forstatement, _IExpression expCounter, _ILanguageModelBuilder7 builder)
		{
			_IExpression iExpression = forstatement._By.Duplicate() as _IExpression;
			_IOperatorExpression iOperatorExpression;
			if (forstatement._By is _ILiteralExpression iLiteralExpression)
			{
				if (iLiteralExpression.Negative)
				{
					iOperatorExpression = builder.CreateOperatorExpression(Operator.Minus);
					iExpression = builder.CreateLiteralExpression(Math.Abs(iLiteralExpression.LongValue));
					iExpression._Position = forstatement._By._Position;
				}
				else
				{
					iOperatorExpression = builder.CreateOperatorExpression(Operator.Plus);
				}
			}
			else
			{
				iOperatorExpression = builder.CreateOperatorExpression(Operator.Plus);
			}
			iOperatorExpression.AddOperand(expCounter.Duplicate() as _IExpression);
			iOperatorExpression.AddOperand(iExpression);
			_IAssignmentExpression iAssignmentExpression = builder.CreateAssignmentExpression(expCounter.Duplicate() as _IExpression);
			iAssignmentExpression._RValue = iOperatorExpression;
			return iAssignmentExpression;
		}

		private static _IExpression ExtendCondition(_IForStatement forstatement, _IAssignmentExpression assignexp, out _IOperatorExpression opCondition, _ILanguageModelBuilder7 builder)
		{
			_IExpression lValue = assignexp._LValue;
			_IExpression by = forstatement._By;
			opCondition = null;
			if (by is _ILiteralExpression iLiteralExpression)
			{
				opCondition = builder.CreateOperatorExpression(iLiteralExpression.Negative ? Operator.Ge : Operator.Le);
				opCondition.AddOperand(lValue.Duplicate() as _IExpression);
				opCondition.AddOperand(forstatement._UpperBound.Duplicate() as _IExpression);
			}
			else
			{
				opCondition = builder.CreateOperatorExpression(Operator.Or);
				_IOperatorExpression iOperatorExpression = builder.CreateOperatorExpression(Operator.And);
				_IOperatorExpression iOperatorExpression2 = builder.CreateOperatorExpression(Operator.Ge);
				iOperatorExpression2.AddOperand(by.Duplicate() as _IExpression);
				iOperatorExpression2.AddOperand(builder.CreateLiteralExpression(0L));
				_IOperatorExpression iOperatorExpression3 = builder.CreateOperatorExpression(Operator.Le);
				iOperatorExpression3.AddOperand(lValue.Duplicate() as _IExpression);
				iOperatorExpression3.AddOperand(forstatement._UpperBound.Duplicate() as _IExpression);
				iOperatorExpression.AddOperand(iOperatorExpression2);
				iOperatorExpression.AddOperand(iOperatorExpression3);
				_IOperatorExpression iOperatorExpression4 = builder.CreateOperatorExpression(Operator.And);
				_IOperatorExpression iOperatorExpression5 = builder.CreateOperatorExpression(Operator.Lt);
				iOperatorExpression5.AddOperand(by.Duplicate() as _IExpression);
				iOperatorExpression5.AddOperand(builder.CreateLiteralExpression(0L));
				_IOperatorExpression iOperatorExpression6 = builder.CreateOperatorExpression(Operator.Ge);
				iOperatorExpression6.AddOperand(lValue.Duplicate() as _IExpression);
				iOperatorExpression6.AddOperand(forstatement._UpperBound.Duplicate() as _IExpression);
				iOperatorExpression4.AddOperand(iOperatorExpression5);
				iOperatorExpression4.AddOperand(iOperatorExpression6);
				opCondition.AddOperand(iOperatorExpression);
				opCondition.AddOperand(iOperatorExpression4);
			}
			opCondition._Position = forstatement._UpperBound._Position;
			opCondition.LengthIntern = forstatement._UpperBound.LengthIntern;
			return lValue;
		}
	}
}
