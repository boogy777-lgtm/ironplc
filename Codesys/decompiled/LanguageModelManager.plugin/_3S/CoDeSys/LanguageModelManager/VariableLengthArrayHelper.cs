using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000105 RID: 261
	internal static class VariableLengthArrayHelper
	{
		// Token: 0x0600133B RID: 4923 RVA: 0x00035414 File Offset: 0x00034414
		internal static bool IsVariableLengthArray(_IIndexAccessExpression indexaccess, IScope5 scope, out IVariable arrayVar)
		{
			arrayVar = (indexaccess.Var as _IExpression).GetVariable(scope);
			return arrayVar != null && arrayVar.HasAttribute(CompileAttributes.ATTRIBUTE_VARIABLE_LENGTH_ARRAY);
		}

		// Token: 0x0600133C RID: 4924 RVA: 0x0003543C File Offset: 0x0003443C
		private static IExpression CreateNewBaseExpression(_IIndexAccessExpression indexaccess, IVariable arrayVar, ILanguageModelBuilder12 lmBuilder, out IExprementPosition pos)
		{
			IExpression result = null;
			_ICompoAccessExpression icompoAccessExpression = indexaccess.Var as _ICompoAccessExpression;
			_IVariableExpression ivariableExpression = indexaccess.Var as _IVariableExpression;
			pos = null;
			if (icompoAccessExpression != null || ivariableExpression != null)
			{
				IExpression expression = indexaccess.Accesses[0];
				pos = lmBuilder.CreateExprementPosition(expression.Position.PositionCombination);
				IVariableExpression2 variableExpression = lmBuilder.CreateVariableExpression(pos, arrayVar.OrgName + "__Array__Info");
				if (icompoAccessExpression == null)
				{
					result = variableExpression;
				}
				else
				{
					_IExpression iexpression = icompoAccessExpression.Left as _IExpression;
					iexpression = (_IExpression)iexpression.Duplicate();
					result = lmBuilder.CreateCompoAccessExpression(pos, iexpression, variableExpression);
				}
			}
			return result;
		}

		// Token: 0x0600133D RID: 4925 RVA: 0x000354D3 File Offset: 0x000344D3
		private static bool IsInnerMostIndexAccess(_IIndexAccessExpression indexaccess)
		{
			return !(indexaccess.Var is _IIndexAccessExpression);
		}

		// Token: 0x0600133E RID: 4926 RVA: 0x000354E8 File Offset: 0x000344E8
		private static IExpression CreateArrayInfoAccess(_IIndexAccessExpression indexaccess, IVariable arrayVar, int i, string stComponent, ILanguageModelBuilder12 lmBuilder, out IExprementPosition pos)
		{
			IExpression expBase = VariableLengthArrayHelper.CreateNewBaseExpression(indexaccess, arrayVar, lmBuilder, out pos);
			IExpression expAccess = lmBuilder.CreateLiteralExpression(pos, (long)(i + 1));
			IExpression expLeft = lmBuilder.CreateIndexAccessExpression(pos, expBase, expAccess);
			IVariableExpression2 expRight = lmBuilder.CreateVariableExpression(pos, stComponent);
			return lmBuilder.CreateCompoAccessExpression(pos, expLeft, expRight);
		}

		// Token: 0x0600133F RID: 4927 RVA: 0x00035538 File Offset: 0x00034538
		private static _IExpression CreateZeroRelativeIndexAccess(_IIndexAccessExpression indexaccess, IVariable arrayVar, int i, ILanguageModelBuilder12 lmBuilder)
		{
			IExprementPosition pos;
			IExpression expOp = VariableLengthArrayHelper.CreateArrayInfoAccess(indexaccess, arrayVar, i, "diLower", lmBuilder, out pos);
			return (_IExpression)lmBuilder.CreateOperatorExpression(pos, Operator.Minus, indexaccess.Accesses[i], expOp);
		}

		// Token: 0x06001340 RID: 4928 RVA: 0x00035570 File Offset: 0x00034570
		private static IExpression CreateArrayDimRangeExpression(_IIndexAccessExpression indexaccess, IVariable arrayVar, int i, ILanguageModelBuilder12 lmBuilder)
		{
			IExprementPosition pos;
			IExpression expOp = VariableLengthArrayHelper.CreateArrayInfoAccess(indexaccess, arrayVar, i, "diUpper", lmBuilder, out pos);
			IExpression expOp2 = VariableLengthArrayHelper.CreateArrayInfoAccess(indexaccess, arrayVar, i, "diLower", lmBuilder, out pos);
			IExpression expOp3 = lmBuilder.CreateOperatorExpression(pos, Operator.Minus, expOp, expOp2);
			IExpression expOp4 = lmBuilder.CreateLiteralExpression(pos, 1L);
			return lmBuilder.CreateOperatorExpression(pos, Operator.Plus, expOp4, expOp3);
		}

		// Token: 0x06001341 RID: 4929 RVA: 0x000355C8 File Offset: 0x000345C8
		private static IExpression CreateArrayDimRangeProductExpression(_IIndexAccessExpression indexaccess, IVariable arrayVar, int iZeroBasedIndex, int iNumAccesses, ILanguageModelBuilder12 lmBuilder)
		{
			int num = iZeroBasedIndex + 1;
			if (num == iNumAccesses)
			{
				return null;
			}
			List<IExpression> list = new List<IExpression>();
			for (int i = num; i < iNumAccesses; i++)
			{
				list.Add(VariableLengthArrayHelper.CreateArrayDimRangeExpression(indexaccess, arrayVar, i, lmBuilder));
			}
			if (num + 1 == iNumAccesses)
			{
				return list[0];
			}
			IExpression expression = lmBuilder.CreateOperatorExpression(null, Operator.Times, list[0], list[1]);
			if (2 < num)
			{
				for (int j = 2; j < num; j++)
				{
					expression = lmBuilder.CreateOperatorExpression(null, Operator.Times, expression, list[j]);
				}
			}
			return expression;
		}

		// Token: 0x06001342 RID: 4930 RVA: 0x00035658 File Offset: 0x00034658
		private static List<IExpression> DetermineSingleOffsetsForLinearisedIndexAccess(_IIndexAccessExpression indexaccess, IVariable arrayVar, ILanguageModelBuilder12 lmBuilder)
		{
			List<IExpression> list = new List<IExpression>();
			for (int i = 0; i < indexaccess.NumAccesses; i++)
			{
				IExpression expression = VariableLengthArrayHelper.CreateZeroRelativeIndexAccess(indexaccess, arrayVar, i, lmBuilder);
				IExpression expression2 = VariableLengthArrayHelper.CreateArrayDimRangeProductExpression(indexaccess, arrayVar, i, indexaccess.NumAccesses, lmBuilder);
				if (expression2 == null)
				{
					list.Add(expression);
				}
				else
				{
					IExpression item = lmBuilder.CreateOperatorExpression(null, Operator.Times, expression, expression2);
					list.Add(item);
				}
			}
			return list;
		}

		// Token: 0x06001343 RID: 4931 RVA: 0x000356BC File Offset: 0x000346BC
		private static _IExpression CreateLinearisedIndexAccess(_IIndexAccessExpression indexaccess, IVariable arrayVar, ILanguageModelBuilder12 lmBuilder)
		{
			List<IExpression> list = VariableLengthArrayHelper.DetermineSingleOffsetsForLinearisedIndexAccess(indexaccess, arrayVar, lmBuilder);
			if (1 == indexaccess.NumAccesses)
			{
				return (_IExpression)list[0];
			}
			IExpression expression = lmBuilder.CreateOperatorExpression(null, Operator.Plus, list[0], list[1]);
			if (2 < indexaccess.NumAccesses)
			{
				for (int i = 2; i < indexaccess.NumAccesses; i++)
				{
					expression = lmBuilder.CreateOperatorExpression(null, Operator.Plus, expression, list[i]);
				}
			}
			return (_IExpression)expression;
		}

		// Token: 0x06001344 RID: 4932 RVA: 0x00035738 File Offset: 0x00034738
		private static void RemoveAccessesExceptFirstOne(_IIndexAccessExpression indexaccess)
		{
			List<_IExpression> list = new List<_IExpression>();
			for (int i = 1; i < indexaccess.NumAccesses; i++)
			{
				list.Add(indexaccess[i]);
			}
			ICollection<_IExpression> accesses = indexaccess._Accesses;
			foreach (_IExpression item in list)
			{
				accesses.Remove(item);
			}
		}

		// Token: 0x06001345 RID: 4933 RVA: 0x000357B4 File Offset: 0x000347B4
		internal static void HandleVariableLengthArray(_IIndexAccessExpression indexaccess, IScope5 scope)
		{
			IVariable arrayVar;
			if (VariableLengthArrayHelper.IsVariableLengthArray(indexaccess, scope, out arrayVar) && VariableLengthArrayHelper.IsInnerMostIndexAccess(indexaccess))
			{
				ILanguageModelBuilder12 lmBuilder = APEnvironmentFacade.Instance.LanguageModelMgr.CreateLanguageModelBuilder() as ILanguageModelBuilder12;
				_IExpression iexpression;
				if (1 == indexaccess.NumAccesses)
				{
					iexpression = VariableLengthArrayHelper.CreateZeroRelativeIndexAccess(indexaccess, arrayVar, 0, lmBuilder);
				}
				else
				{
					iexpression = VariableLengthArrayHelper.CreateLinearisedIndexAccess(indexaccess, arrayVar, lmBuilder);
				}
				CompilerProxy.TypifyExprement(iexpression, scope, scope.ApplicationContext as _ICompileContext, null, false, false, null);
				indexaccess[0] = iexpression;
				if (1 < indexaccess.NumAccesses)
				{
					VariableLengthArrayHelper.RemoveAccessesExceptFirstOne(indexaccess);
				}
			}
		}
	}
}
