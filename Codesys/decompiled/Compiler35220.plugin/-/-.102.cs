using System;
using System.Collections.Generic;
using \u0002;
using \u0019;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0004
{
	// Token: 0x02000133 RID: 307
	internal static class \u0003
	{
		// Token: 0x060015C0 RID: 5568 RVA: 0x0003F960 File Offset: 0x0003DB60
		private static bool \u0001(IArrayDimension \u0002, out int \u0003, out int \u0004, IScope \u0005)
		{
			\u0004 = -1;
			bool flag;
			\u0003 = \u0002.LowerBorderInt(out flag, \u0005);
			if (!flag)
			{
				return false;
			}
			\u0004 = \u0002.UpperBorderInt(out flag, \u0005);
			return flag;
		}

		// Token: 0x060015C1 RID: 5569 RVA: 0x0003F98C File Offset: 0x0003DB8C
		private static ulong? \u0001(IVariable \u0002, _IIndexAccessExpression \u0003, IScope \u0004)
		{
			IArrayType arrayType = \u0002.CompiledType as IArrayType;
			if (arrayType == null)
			{
				return null;
			}
			if (\u0003.Accesses.Length != arrayType.Dimensions.Length)
			{
				return null;
			}
			ICompiledType compiledType;
			ulong? num = global::\u0004.\u0003.\u0001(arrayType, \u0003, \u0004, out compiledType);
			if (num == null || compiledType == null)
			{
				return null;
			}
			int? num2 = global::\u0002.\u0002.\u0001(compiledType, \u0004, new RecursionGuard());
			if (num2 == null || num2.Value < 0)
			{
				return null;
			}
			ulong? num3 = num;
			ulong num4 = (ulong)((long)num2.Value);
			if (num3 == null)
			{
				return null;
			}
			return new ulong?(num3.GetValueOrDefault() * num4);
		}

		// Token: 0x060015C2 RID: 5570 RVA: 0x0003FA50 File Offset: 0x0003DC50
		private static IList<_IIndexAccessExpression> \u0001(_IIndexAccessExpression \u0002)
		{
			IList<_IIndexAccessExpression> list = new List<_IIndexAccessExpression>();
			do
			{
				list.Insert(0, \u0002);
				\u0002 = (\u0002.Var as _IIndexAccessExpression);
			}
			while (\u0002 != null);
			return list;
		}

		// Token: 0x060015C3 RID: 5571 RVA: 0x0003FA7C File Offset: 0x0003DC7C
		private static IList<IArrayType> \u0001(IArrayType \u0002, out ICompiledType \u0003)
		{
			IList<IArrayType> list = new List<IArrayType>();
			do
			{
				list.Add(\u0002);
				\u0003 = (\u0002.Base as ICompiledType);
				\u0002 = (\u0002.Base as IArrayType);
			}
			while (\u0002 != null);
			return list;
		}

		// Token: 0x060015C4 RID: 5572 RVA: 0x0003FAB4 File Offset: 0x0003DCB4
		private static bool \u0001(IArrayDimension \u0002, _IExpression \u0003, IScope \u0004, out int \u0005, out int \u0006)
		{
			\u0005 = (\u0006 = -1);
			int num;
			int num2;
			if (!global::\u0004.\u0003.\u0001(\u0002, out num, out num2, \u0004))
			{
				return false;
			}
			\u0006 = num2 - num + 1;
			\u0005 = 0;
			if (\u0003 != null)
			{
				if (!\u0003.IsConstant(\u0004, true))
				{
					return false;
				}
				int num3;
				if (!\u0003.Literal(\u0004, true).GetInt(out num3))
				{
					return false;
				}
				\u0005 = num3 - num;
				if (\u0005 < 0)
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x060015C5 RID: 5573 RVA: 0x0003FB14 File Offset: 0x0003DD14
		private static bool \u0001(IList<IArrayType> \u0002, IList<_IIndexAccessExpression> \u0003, IScope \u0004, IList<int> \u0005, IList<int> \u0006)
		{
			for (int i = 0; i < \u0002.Count; i++)
			{
				IArrayType arrayType = \u0002[i];
				_IIndexAccessExpression iindexAccessExpression = (i < \u0003.Count) ? \u0003[i] : null;
				for (int j = 0; j < arrayType.Dimensions.Length; j++)
				{
					IArrayDimension u = arrayType.Dimensions[j];
					_IExpression u2 = (iindexAccessExpression != null) ? iindexAccessExpression.GetAccess(j) : null;
					int item;
					int item2;
					if (!global::\u0004.\u0003.\u0001(u, u2, \u0004, out item, out item2))
					{
						return false;
					}
					\u0005.Add(item);
					\u0006.Add(item2);
				}
			}
			return true;
		}

		// Token: 0x060015C6 RID: 5574 RVA: 0x0003FBA0 File Offset: 0x0003DDA0
		private static ulong? \u0001(IArrayType \u0002, _IIndexAccessExpression \u0003, IScope \u0004, out ICompiledType \u0005)
		{
			IList<_IIndexAccessExpression> u = global::\u0004.\u0003.\u0001(\u0003);
			IList<IArrayType> u2 = global::\u0004.\u0003.\u0001(\u0002, out \u0005);
			IList<int> list = new List<int>();
			IList<int> list2 = new List<int>();
			if (!global::\u0004.\u0003.\u0001(u2, u, \u0004, list, list2))
			{
				return null;
			}
			ulong num = 1UL;
			ulong num2 = 0UL;
			for (int i = list.Count - 1; i >= 0; i--)
			{
				num2 += (ulong)((long)list[i] * (long)num);
				num *= (ulong)((long)list2[i]);
			}
			return new ulong?(num2);
		}

		// Token: 0x060015C7 RID: 5575 RVA: 0x0003FC20 File Offset: 0x0003DE20
		private static ulong? \u0001(IExpression \u0002, IScope \u0003)
		{
			_IIndexAccessExpression iindexAccessExpression = \u0002 as _IIndexAccessExpression;
			if (iindexAccessExpression != null)
			{
				ulong? num = global::\u0004.\u0003.\u0001(iindexAccessExpression.GetVariable(\u0003), iindexAccessExpression, \u0003);
				if (num != null)
				{
					return new ulong?(num.Value);
				}
			}
			return null;
		}

		// Token: 0x060015C8 RID: 5576 RVA: 0x0003FC68 File Offset: 0x0003DE68
		private static bool \u0001(_ICompoAccessExpression \u0002, IScope \u0003, out int \u0004)
		{
			\u0004 = 0;
			if (\u0002._Right.GetVariable(\u0003) != null)
			{
				return false;
			}
			ILiteralValue literalValue = \u0002._Right.Literal(\u0003);
			if (literalValue == null)
			{
				return false;
			}
			if (!literalValue.GetInt(out \u0004))
			{
				\u0004 = 0;
			}
			return true;
		}

		// Token: 0x060015C9 RID: 5577 RVA: 0x0003FCA8 File Offset: 0x0003DEA8
		private static int? \u0001(_ICompoAccessExpression \u0002, IScope \u0003)
		{
			if (\u0002 == null)
			{
				return null;
			}
			int num;
			if (global::\u0004.\u0003.\u0001(\u0002, \u0003, out num))
			{
				if (!(\u0002.Left is ICompoAccessExpression))
				{
					return new int?(num);
				}
				\u0002 = (\u0002._Left as _ICompoAccessExpression);
			}
			int num2 = 0;
			checked
			{
				for (;;)
				{
					IVariable variable = \u0002.GetVariable(\u0003);
					IDataLocation dataLocation = (variable != null) ? variable.DataLocation : null;
					if (dataLocation == null)
					{
						break;
					}
					_IExpression iexpression = \u0002._Left;
					_IIndexAccessExpression iindexAccessExpression = iexpression as _IIndexAccessExpression;
					if (iindexAccessExpression != null)
					{
						ulong? num3 = global::\u0004.\u0003.\u0001(iindexAccessExpression, \u0003);
						if (num3 != null)
						{
							ulong valueOrDefault = num3.GetValueOrDefault();
							num2 += (int)valueOrDefault;
						}
						iexpression = iindexAccessExpression._Var;
					}
					num2 += dataLocation.Offset;
					if (!dataLocation.IsRelativ)
					{
						goto IL_BE;
					}
					\u0002 = (iexpression as _ICompoAccessExpression);
					if (\u0002 == null)
					{
						goto IL_BE;
					}
				}
				return null;
				IL_BE:;
			}
			return new int?(checked(num2 * 8) + num);
		}

		// Token: 0x060015CA RID: 5578 RVA: 0x0003FD80 File Offset: 0x0003DF80
		private static int? \u0001(_IExpression \u0002, IVariable \u0003, IScope \u0004)
		{
			int? num;
			if (\u0003.DataLocation == null)
			{
				num = null;
			}
			else
			{
				_ICompoAccessExpression icompoAccessExpression = \u0002 as _ICompoAccessExpression;
				if (icompoAccessExpression != null)
				{
					num = global::\u0004.\u0003.\u0001(icompoAccessExpression, \u0004);
				}
				else if (\u0002 is IIndexAccessExpression && !\u0003.DataLocation.IsRelativ)
				{
					num = new int?(0);
				}
				else
				{
					IDataLocation dataLocation = \u0003.DataLocation;
					num = ((dataLocation != null) ? new int?(dataLocation.Offset * 8) : null);
				}
			}
			if (num == null)
			{
				return null;
			}
			ulong? num2 = global::\u0004.\u0003.\u0001(\u0002, \u0004);
			checked
			{
				if (num2 != null)
				{
					ulong valueOrDefault = num2.GetValueOrDefault();
					num += (int)(valueOrDefault * 8UL);
				}
				return num;
			}
		}

		// Token: 0x060015CB RID: 5579 RVA: 0x0003FE50 File Offset: 0x0003E050
		public static ILiteralValue \u0001(IList<_IExpression> \u0002, IScope \u0003)
		{
			if (\u0002.Count != 1)
			{
				return null;
			}
			_IExpression iexpression = \u0002[0];
			IVariable variable = iexpression.GetVariable(\u0003);
			if (variable == null)
			{
				return null;
			}
			int? num = global::\u0004.\u0003.\u0001(iexpression, variable, \u0003);
			if (num == null)
			{
				return null;
			}
			if (!variable.GetFlag(VarFlag.RelativeStack))
			{
				return \u0019.\u0003.\u0001((long)num.Value);
			}
			bool flag = false;
			IScope5 scope = (IScope5)\u0003;
			if (scope != null && scope.Codegenerator != null)
			{
				ICodegenerator3 codegenerator = scope.Codegenerator as ICodegenerator3;
				if (codegenerator != null)
				{
					flag = codegenerator.GetProperty(CodegeneratorProperties.PositiveStackGrow);
				}
			}
			long num2 = (long)num.Value;
			int? num3;
			int num4;
			if (!flag)
			{
				num3 = num;
				num4 = 0;
				if (num3.GetValueOrDefault() > num4 & num3 != null)
				{
					goto IL_C8;
				}
			}
			if (!flag)
			{
				goto IL_E0;
			}
			num3 = num;
			num4 = 0;
			if (!(num3.GetValueOrDefault() < num4 & num3 != null))
			{
				goto IL_E0;
			}
			IL_C8:
			num2 += (long)(((IScope5)\u0003).Codegenerator.StackDisplacement * 8);
			IL_E0:
			return \u0019.\u0003.\u0001(num2);
		}
	}
}
