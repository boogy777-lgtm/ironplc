using System;
using CODESYS.Parser35220.Utilities;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace CODESYS.Parser35220
{
	// Token: 0x02000006 RID: 6
	internal static class ForLoopExtender
	{
		// Token: 0x06000006 RID: 6 RVA: 0x00002080 File Offset: 0x00000280
		internal static void ExtendForLoop(_IForStatement forstatement, _ILanguageModelBuilder7 builder)
		{
			_IAssignmentExpression iassignmentExpression = forstatement._CounterStart as _IAssignmentExpression;
			_IExpression iexpression = null;
			forstatement._Counter = null;
			forstatement._Condition = null;
			if (iassignmentExpression != null)
			{
				_IOperatorExpression condition;
				iexpression = ForLoopExtender.ExtendCondition(forstatement, iassignmentExpression, out condition, builder);
				forstatement._Condition = condition;
			}
			if (iexpression != null && forstatement._By != null)
			{
				_IAssignmentExpression counter = ForLoopExtender.ExtendCounter(forstatement, iexpression, builder);
				forstatement._Counter = counter;
			}
		}

		// Token: 0x06000007 RID: 7 RVA: 0x000020DC File Offset: 0x000002DC
		private static _IAssignmentExpression ExtendCounter(_IForStatement forstatement, _IExpression expCounter, _ILanguageModelBuilder7 builder)
		{
			_IExpression iexpression = forstatement._By.Duplicate() as _IExpression;
			_ILiteralExpression iliteralExpression = forstatement._By as _ILiteralExpression;
			_IOperatorExpression ioperatorExpression;
			if (iliteralExpression != null)
			{
				if (iliteralExpression.Negative)
				{
					ioperatorExpression = builder.CreateOperatorExpression(158);
					iexpression = builder.CreateLiteralExpression(Math.Abs(iliteralExpression.LongValue));
					iexpression._Position = forstatement._By._Position;
				}
				else
				{
					ioperatorExpression = builder.CreateOperatorExpression(157);
				}
			}
			else
			{
				ioperatorExpression = builder.CreateOperatorExpression(157);
			}
			ioperatorExpression.AddOperand(expCounter.Duplicate() as _IExpression);
			ioperatorExpression.AddOperand(iexpression);
			_IAssignmentExpression iassignmentExpression = builder.CreateAssignmentExpression(expCounter.Duplicate() as _IExpression);
			iassignmentExpression._RValue = ioperatorExpression;
			return iassignmentExpression;
		}

		// Token: 0x06000008 RID: 8 RVA: 0x0000218C File Offset: 0x0000038C
		private static _IExpression ExtendCondition(_IForStatement forstatement, _IAssignmentExpression assignexp, out _IOperatorExpression opCondition, _ILanguageModelBuilder7 builder)
		{
			_IExpression lvalue = assignexp._LValue;
			_IExpression by = forstatement._By;
			opCondition = null;
			_ILiteralExpression iliteralExpression = by as _ILiteralExpression;
			if (iliteralExpression != null)
			{
				opCondition = builder.CreateOperatorExpression(iliteralExpression.Negative ? 136 : 138);
				opCondition.AddOperand(lvalue.Duplicate() as _IExpression);
				opCondition.AddOperand(forstatement._UpperBound.Duplicate() as _IExpression);
			}
			else
			{
				opCondition = builder.CreateOperatorExpression(129);
				_IOperatorExpression ioperatorExpression = builder.CreateOperatorExpression(127);
				_IOperatorExpression ioperatorExpression2 = builder.CreateOperatorExpression(136);
				ioperatorExpression2.AddOperand(by.Duplicate() as _IExpression);
				ioperatorExpression2.AddOperand(builder.CreateLiteralExpression(0L));
				_IOperatorExpression ioperatorExpression3 = builder.CreateOperatorExpression(138);
				ioperatorExpression3.AddOperand(lvalue.Duplicate() as _IExpression);
				ioperatorExpression3.AddOperand(forstatement._UpperBound.Duplicate() as _IExpression);
				ioperatorExpression.AddOperand(ioperatorExpression2);
				ioperatorExpression.AddOperand(ioperatorExpression3);
				_IOperatorExpression ioperatorExpression4 = builder.CreateOperatorExpression(127);
				_IOperatorExpression ioperatorExpression5 = builder.CreateOperatorExpression(139);
				ioperatorExpression5.AddOperand(by.Duplicate() as _IExpression);
				ioperatorExpression5.AddOperand(builder.CreateLiteralExpression(0L));
				_IOperatorExpression ioperatorExpression6 = builder.CreateOperatorExpression(136);
				ioperatorExpression6.AddOperand(lvalue.Duplicate() as _IExpression);
				ioperatorExpression6.AddOperand(forstatement._UpperBound.Duplicate() as _IExpression);
				ioperatorExpression4.AddOperand(ioperatorExpression5);
				ioperatorExpression4.AddOperand(ioperatorExpression6);
				opCondition.AddOperand(ioperatorExpression);
				opCondition.AddOperand(ioperatorExpression4);
			}
			opCondition._Position = forstatement._UpperBound._Position;
			opCondition.LengthIntern = forstatement._UpperBound.LengthIntern;
			return lvalue;
		}
	}
}
