using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using \u0002;
using \u0005;
using \u0017;
using \u0019;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler35220.Services
{
	// Token: 0x020000A5 RID: 165
	internal static class ConstantFoldingHelper
	{
		// Token: 0x06000D6C RID: 3436 RVA: 0x00023544 File Offset: 0x00021744
		internal static _ILiteralExpression \u0001(ILiteralValue \u0002)
		{
			_ILiteralExpression result = null;
			switch (\u0002.KindOf)
			{
			case KindOfLiteral.SignedInteger:
				result = \u0019.\u0003.\u0001(\u0002.SignedLong);
				break;
			case KindOfLiteral.UnsignedInteger:
				result = \u0019.\u0003.\u0001(\u0002.UnsignedLong);
				break;
			case KindOfLiteral.Float:
				result = \u0019.\u0003.\u0001(\u0002.Float);
				break;
			case KindOfLiteral.Bool:
			{
				bool @bool = \u0002.Bool;
				long u = 0L;
				if (@bool)
				{
					u = 1L;
				}
				result = \u0019.\u0003.\u0001(u, TypeClass.Bool);
				break;
			}
			}
			return result;
		}

		// Token: 0x06000D6D RID: 3437 RVA: 0x000235B8 File Offset: 0x000217B8
		internal static global::\u0005.\u0001 \u0001(_IOperatorExpression \u0002, ILiteralValue[] \u0003, bool \u0004, ICompiledType \u0005, Operator \u0006)
		{
			if (\u0005 == null)
			{
				return ConstantFoldingHelper.\u0001(\u0003, \u0004, \u0006);
			}
			return ConstantFoldingHelper.\u0001(\u0002, \u0005);
		}

		// Token: 0x06000D6E RID: 3438 RVA: 0x000235D0 File Offset: 0x000217D0
		private static global::\u0005.\u0001 \u0001(ILiteralValue[] \u0002, bool \u0003, Operator \u0004)
		{
			global::\u0005.\u0001 result;
			if (!\u0003)
			{
				result = global::\u0005.\u0001.\u0008;
			}
			else if (\u0002.Length == 0 || \u0002[0].KindOf == KindOfLiteral.None)
			{
				result = global::\u0005.\u0001.\u0008;
			}
			else if (\u0004 - Operator.Expt <= 9)
			{
				result = global::\u0005.\u0001.\u0003;
			}
			else
			{
				result = ConstantFoldingHelper.\u0001(\u0002[0].KindOf);
			}
			return result;
		}

		// Token: 0x06000D6F RID: 3439 RVA: 0x00023614 File Offset: 0x00021814
		private static global::\u0005.\u0001 \u0001(_IOperatorExpression \u0002, ICompiledType \u0003)
		{
			global::\u0005.\u0001 result;
			if (TypeTable.IsInteger(\u0003.Class))
			{
				result = (TypeTable.IsSigned(\u0003.Class) ? global::\u0005.\u0001.\u0001 : global::\u0005.\u0001.\u0002);
			}
			else if (TypeTable.IsReal(\u0003.Class))
			{
				result = global::\u0005.\u0001.\u0003;
			}
			else if (\u0003.Class == TypeClass.Bool)
			{
				result = global::\u0005.\u0001.\u0004;
			}
			else if (\u0003.Class == TypeClass.String)
			{
				result = global::\u0005.\u0001.\u0005;
			}
			else if (\u0003.Class == TypeClass.WString)
			{
				result = global::\u0005.\u0001.\u0005;
			}
			else
			{
				result = ConstantFoldingHelper.\u0001(\u0002);
			}
			return result;
		}

		// Token: 0x06000D70 RID: 3440 RVA: 0x00023684 File Offset: 0x00021884
		private static global::\u0005.\u0001 \u0001(KindOfLiteral \u0002)
		{
			switch (\u0002)
			{
			case KindOfLiteral.SignedInteger:
				return global::\u0005.\u0001.\u0001;
			case KindOfLiteral.UnsignedInteger:
				return global::\u0005.\u0001.\u0002;
			case KindOfLiteral.Float:
				return global::\u0005.\u0001.\u0003;
			case KindOfLiteral.String:
				return global::\u0005.\u0001.\u0005;
			case KindOfLiteral.Bool:
				return global::\u0005.\u0001.\u0004;
			default:
				return global::\u0005.\u0001.\u0008;
			}
		}

		// Token: 0x06000D71 RID: 3441 RVA: 0x000236B0 File Offset: 0x000218B0
		private static global::\u0005.\u0001 \u0001(_IOperatorExpression \u0002)
		{
			if (\u0002._OperandsList.Any(new Func<_IExpression, bool>(ConstantFoldingHelper.<>c.<>9.\u0001)))
			{
				return global::\u0005.\u0001.\u0008;
			}
			if (\u0002.Type.Class == TypeClass.Time)
			{
				if (!\u0002._OperandsList.Any(new Func<_IExpression, bool>(ConstantFoldingHelper.<>c.<>9.\u0002)))
				{
					return global::\u0005.\u0001.\u0006;
				}
			}
			if (\u0002.Type.Class == TypeClass.LTime)
			{
				if (!\u0002._OperandsList.Any(new Func<_IExpression, bool>(ConstantFoldingHelper.<>c.<>9.\u0003)))
				{
					return global::\u0005.\u0001.\u0006;
				}
			}
			if (TypeClass.DateAndTime == \u0002.Type.Class || TypeClass.Date == \u0002.Type.Class)
			{
				return ConstantFoldingHelper.\u0001(\u0002, \u0002.Type.Class, TypeClass.Time);
			}
			if (TypeClass.LDate == \u0002.Type.Class || TypeClass.LDateAndTime == \u0002.Type.Class)
			{
				return ConstantFoldingHelper.\u0001(\u0002, \u0002.Type.Class, TypeClass.LTime);
			}
			return global::\u0005.\u0001.\u0008;
		}

		// Token: 0x06000D72 RID: 3442 RVA: 0x000237CC File Offset: 0x000219CC
		private static global::\u0005.\u0001 \u0001(_IOperatorExpression \u0002, TypeClass \u0003, TypeClass \u0004)
		{
			ConstantFoldingHelper.\u0001 u = new ConstantFoldingHelper.\u0001();
			u.\u0001 = \u0003;
			u.\u0002 = \u0004;
			int num = \u0002._OperandsList.Count(new Func<_IExpression, bool>(u.\u0001));
			if (1 != num)
			{
				return global::\u0005.\u0001.\u0008;
			}
			if (!\u0002._OperandsList.Where(new Func<_IExpression, bool>(u.\u0002)).Any(new Func<_IExpression, bool>(u.\u0003)))
			{
				return global::\u0005.\u0001.\u0007;
			}
			return global::\u0005.\u0001.\u0008;
		}

		// Token: 0x06000D73 RID: 3443 RVA: 0x00023838 File Offset: 0x00021A38
		internal static _IStructureInitialization \u0001(global::\u0002.\u0002 \u0002, _IExpression \u0003, \u0017.\u0006 \u0004, IRecursionGuard \u0005)
		{
			IVariable variable = \u0004.\u0001(\u0003);
			if (variable == null || variable.Initial == null)
			{
				return null;
			}
			_IStructureInitialization istructureInitialization = variable.Initial as _IStructureInitialization;
			if (istructureInitialization != null)
			{
				return istructureInitialization;
			}
			_IArrayInitialization iarrayInitialization = variable.Initial as _IArrayInitialization;
			if (iarrayInitialization != null)
			{
				_IIndexAccessExpression iindexAccessExpression = \u0003 as _IIndexAccessExpression;
				int num;
				if (iindexAccessExpression != null && ConstantFoldingHelper.\u0001(\u0002, iindexAccessExpression, \u0004, \u0005, out num))
				{
					bool flag;
					IList<_IExpression> list = ConstantFoldingHelper.\u0001(\u0002, iarrayInitialization, \u0004, \u0005, out flag);
					if (list != null && flag && num >= 0 && num < list.Count)
					{
						return list[num] as _IStructureInitialization;
					}
				}
			}
			return null;
		}

		// Token: 0x06000D74 RID: 3444 RVA: 0x000238C8 File Offset: 0x00021AC8
		private static bool \u0001(global::\u0002.\u0002 \u0002, _IIndexAccessExpression \u0003, \u0017.\u0006 \u0004, IRecursionGuard \u0005, out int \u0006)
		{
			\u0006 = 0;
			int num = 1;
			bool flag = false;
			IVariable variable = \u0004.\u0001(\u0003._Var);
			if (variable != null)
			{
				_IArrayType iarrayType = variable.CompiledType as _IArrayType;
				if (iarrayType != null && variable.GetFlag(VarFlag.Constant))
				{
					for (int i = iarrayType.Dimensions.Length - 1; i >= 0; i--)
					{
						ILiteralValue literalValue = \u0002.\u0001((_IExpression2)\u0003.GetAccess(i), \u0004, \u0005);
						if (literalValue == null)
						{
							return false;
						}
						int num2;
						if (!\u0002.\u0001(iarrayType._Dimensions[i]._LowerBorder, \u0004, \u0005, out num2))
						{
							return false;
						}
						int num3;
						if (!\u0002.\u0001(iarrayType._Dimensions[i]._UpperBorder, \u0004, \u0005, out num3))
						{
							return false;
						}
						int @int = literalValue.GetInt(out flag);
						if (!flag)
						{
							return false;
						}
						if (@int < num2 || @int > num3)
						{
							return false;
						}
						int num4 = num3 - num2 + 1;
						\u0006 += (@int - num2) * num;
						num *= num4;
					}
				}
			}
			return true;
		}

		// Token: 0x06000D75 RID: 3445 RVA: 0x000239C8 File Offset: 0x00021BC8
		internal static IList<_IExpression> \u0001(global::\u0002.\u0002 \u0002, _IArrayInitialization \u0003, \u0017.\u0006 \u0004, IRecursionGuard \u0005, out bool \u0006)
		{
			List<_IExpression> list = new List<_IExpression>();
			\u0006 = true;
			foreach (_IExpression iexpression in \u0003._InitValues)
			{
				_IMultipleIndexInitialization imultipleIndexInitialization = iexpression as _IMultipleIndexInitialization;
				if (imultipleIndexInitialization != null)
				{
					int num;
					if (!\u0002.\u0001(imultipleIndexInitialization._Number, \u0004, \u0005, out num))
					{
						\u0006 = false;
						return null;
					}
					for (int i = 0; i < num; i++)
					{
						list.Add(imultipleIndexInitialization._Value);
					}
				}
				else
				{
					list.Add(iexpression);
				}
			}
			return list;
		}

		// Token: 0x020000A7 RID: 167
		[CompilerGenerated]
		private sealed class \u0001
		{
			// Token: 0x06000D7C RID: 3452 RVA: 0x00023AE0 File Offset: 0x00021CE0
			internal bool \u0001(_IExpression \u0002)
			{
				return this.\u0001 == \u0002.Type.Class;
			}

			// Token: 0x06000D7D RID: 3453 RVA: 0x00023AF8 File Offset: 0x00021CF8
			internal bool \u0002(_IExpression \u0002)
			{
				return this.\u0001 != \u0002.Type.Class;
			}

			// Token: 0x06000D7E RID: 3454 RVA: 0x00023B10 File Offset: 0x00021D10
			internal bool \u0003(_IExpression \u0002)
			{
				return this.\u0002 != \u0002.Type.Class && !TypeTable.IsInteger(\u0002.Type.Class);
			}

			// Token: 0x0400023C RID: 572
			public TypeClass \u0001;

			// Token: 0x0400023D RID: 573
			public TypeClass \u0002;
		}
	}
}
