using System;
using System.Collections.Generic;
using System.Linq;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler35220.PreCompile.Typification
{
	// Token: 0x020001A2 RID: 418
	internal static class TimeOperationTypeCalculator
	{
		// Token: 0x06001DBD RID: 7613 RVA: 0x000603F0 File Offset: 0x0005E5F0
		private static ICompiledType \u0001(IList<_IExpression> \u0002)
		{
			TypeClass typeClass = TypeClass.None;
			bool flag = false;
			bool flag2 = false;
			bool flag3 = false;
			_IPointerType ipointerType = null;
			int num = 0;
			foreach (_IExpression iexpression in \u0002)
			{
				TypeClass @class = iexpression.Type.DeRefType.Class;
				switch (@class)
				{
				case TypeClass.Time:
					if (typeClass == TypeClass.None)
					{
						typeClass = TypeClass.Time;
					}
					flag = true;
					break;
				case TypeClass.Date:
				case TypeClass.DateAndTime:
				case TypeClass.TimeOfDay:
					typeClass = iexpression.Type.DeRefType.Class;
					num++;
					break;
				case TypeClass.Pointer:
					ipointerType = (iexpression.Type.DeRefType as _IPointerType);
					num++;
					break;
				default:
					if (@class == TypeClass.LTime)
					{
						if (typeClass == TypeClass.None)
						{
							typeClass = TypeClass.LTime;
						}
						flag2 = true;
					}
					else
					{
						flag3 = true;
					}
					break;
				}
			}
			if (num == 1 && flag && !flag2 && !flag3)
			{
				return TypeTable.Get(typeClass);
			}
			if (num == 0 && flag && !flag2 && !flag3)
			{
				return TypeTable.Get(TypeClass.Time);
			}
			if (num == 0 && !flag && flag2 && !flag3)
			{
				return TypeTable.Get(TypeClass.LTime);
			}
			if (ipointerType != null && num == 1)
			{
				return ipointerType;
			}
			return null;
		}

		// Token: 0x06001DBE RID: 7614 RVA: 0x00060528 File Offset: 0x0005E728
		private static ICompiledType \u0002(IList<_IExpression> \u0002)
		{
			if (\u0002.Count == 2)
			{
				TypeClass @class = \u0002[0].Type.DeRefType.Class;
				TypeClass class2 = \u0002[1].Type.DeRefType.Class;
				if (@class == TypeClass.Date || @class == TypeClass.DateAndTime || @class == TypeClass.TimeOfDay || @class == TypeClass.Time)
				{
					if (class2 == TypeClass.Time)
					{
						return TypeTable.Get(@class);
					}
					if (class2 == @class)
					{
						return TypeTable.Time;
					}
				}
				else
				{
					if (@class == TypeClass.LTime && class2 == TypeClass.LTime)
					{
						return TypeTable.LTime;
					}
					if (@class == TypeClass.Pointer && TypeTable.IsInteger(class2))
					{
						return \u0002[0].Type.DeRefType;
					}
					if (@class == TypeClass.Pointer && class2 == TypeClass.Pointer)
					{
						return TypeTable.DWord;
					}
				}
			}
			return null;
		}

		// Token: 0x06001DBF RID: 7615 RVA: 0x000605DC File Offset: 0x0005E7DC
		private static ICompiledType \u0001(IList<_IExpression> \u0002, ICommonScope \u0003)
		{
			if (\u0002.Count >= 2)
			{
				bool flag = false;
				bool flag2 = false;
				bool flag3 = false;
				bool flag4 = false;
				int num = 0;
				foreach (_IExpression iexpression in \u0002)
				{
					TypeClass @class = iexpression.Type.DeRefType.Class;
					if (@class != TypeClass.Time)
					{
						if (@class == TypeClass.LTime)
						{
							flag2 = true;
							num++;
						}
						else
						{
							if (!TypeTable.IsInteger(iexpression.Type.DeRefType.Class))
							{
								flag3 = true;
							}
							if (TypeTable.IsLInteger2(iexpression.Type.DeRefType.Class, \u0003))
							{
								flag4 = true;
							}
						}
					}
					else
					{
						flag = true;
						num++;
					}
				}
				if (num == 1 && flag2 && !flag3)
				{
					return TypeTable.LTime;
				}
				if (num == 1 && flag && !flag3 && !flag4)
				{
					return TypeTable.Time;
				}
			}
			return null;
		}

		// Token: 0x06001DC0 RID: 7616 RVA: 0x000606D0 File Offset: 0x0005E8D0
		private static ICompiledType \u0002(IList<_IExpression> \u0002, ICommonScope \u0003)
		{
			if (\u0002.Count == 2)
			{
				TypeClass @class = \u0002[0].Type.DeRefType.Class;
				TypeClass class2 = \u0002[1].Type.DeRefType.Class;
				if (@class == TypeClass.Time && TypeTable.IsInteger(class2) && !TypeTable.IsLInteger2(class2, \u0003))
				{
					return TypeTable.Time;
				}
				if (@class == TypeClass.LTime && TypeTable.IsInteger(class2))
				{
					return TypeTable.LTime;
				}
			}
			return null;
		}

		// Token: 0x06001DC1 RID: 7617 RVA: 0x00060744 File Offset: 0x0005E944
		public static bool \u0001(_IOperatorExpression \u0002, ICommonScope \u0003, out ICompiledType \u0004)
		{
			IList<_IExpression> operandsList = \u0002._OperandsList;
			ICompiledType[] array = operandsList.Select(new Func<_IExpression, ICompiledType>(TimeOperationTypeCalculator.<>c.<>9.\u0001)).ToArray<ICompiledType>();
			IType[] u = array;
			IType type = UnknownIdentVisitor.\u0001(\u0002, u);
			if (type != null)
			{
				\u0004 = (type as ICompiledType);
				return true;
			}
			\u0004 = null;
			Operator code = \u0002.Code;
			switch (code)
			{
			case Operator.Add:
				break;
			case Operator.Sub:
				goto IL_9B;
			case Operator.Mul:
				goto IL_A5;
			case Operator.Div:
				goto IL_B0;
			default:
				switch (code)
				{
				case Operator.Plus:
					break;
				case Operator.Minus:
					goto IL_9B;
				case Operator.Times:
					goto IL_A5;
				case Operator.Power:
					goto IL_B9;
				case Operator.Divide:
					goto IL_B0;
				default:
					goto IL_B9;
				}
				break;
			}
			\u0004 = TimeOperationTypeCalculator.\u0001(operandsList);
			goto IL_B9;
			IL_9B:
			\u0004 = TimeOperationTypeCalculator.\u0002(operandsList);
			goto IL_B9;
			IL_A5:
			\u0004 = TimeOperationTypeCalculator.\u0001(operandsList, \u0003);
			goto IL_B9;
			IL_B0:
			\u0004 = TimeOperationTypeCalculator.\u0002(operandsList, \u0003);
			IL_B9:
			return \u0004 != null;
		}
	}
}
