using System;
using _3S.CoDeSys.Compiler35220;
using _3S.CoDeSys.Compiler35220.Phase3_Location;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0084
{
	// Token: 0x02000100 RID: 256
	internal static class \u0004
	{
		// Token: 0x06001313 RID: 4883 RVA: 0x00034BFC File Offset: 0x00032DFC
		public static bool \u0001(ICompiledType \u0002, IScope \u0003, ICodegenerator \u0004)
		{
			if (!TypeTable.IsBlock(\u0002.Class))
			{
				return true;
			}
			if (!(\u0004 is ICodegenerator3))
			{
				return false;
			}
			int num = \u0002.Size(\u0003);
			return \u0002.Class == TypeClass.Userdef && num == (\u0004 as ICodegenerator3).RegisterSize && num == Locator.\u0002(\u0002, 0, \u0003 as IScope5);
		}

		// Token: 0x06001314 RID: 4884 RVA: 0x00034C58 File Offset: 0x00032E58
		public static bool \u0001(TypeClass \u0002, TypeClass \u0003, ILiteralValue \u0004)
		{
			return \u0004.\u0001(\u0002, \u0003, Helper.\u0001(\u0004));
		}

		// Token: 0x06001315 RID: 4885 RVA: 0x00034C68 File Offset: 0x00032E68
		private static bool \u0001(bool \u0002, ulong \u0003, long \u0004, long \u0005)
		{
			return (\u0002 && \u0003 > (ulong)(-(ulong)\u0004)) || (!\u0002 && \u0003 > (ulong)\u0005);
		}

		// Token: 0x06001316 RID: 4886 RVA: 0x00034C80 File Offset: 0x00032E80
		private static bool \u0001(TypeClass \u0002, TypeClass \u0003, long \u0004)
		{
			if (\u0004 < 0L && TypeTable.IsSigned(\u0003))
			{
				return \u0004.\u0001(\u0002, true, (ulong)(-(ulong)\u0004));
			}
			return \u0004.\u0001(\u0002, false, (ulong)\u0004);
		}

		// Token: 0x06001317 RID: 4887 RVA: 0x00034CA4 File Offset: 0x00032EA4
		public static bool \u0001(TypeClass \u0002, bool \u0003, ulong \u0004)
		{
			switch (\u0002)
			{
			case TypeClass.Byte:
			case TypeClass.USInt:
				return \u0004.\u0001(\u0003, \u0004, 0L, 255L);
			case TypeClass.Word:
			case TypeClass.UInt:
				return \u0004.\u0001(\u0003, \u0004, 0L, 65535L);
			case TypeClass.DWord:
			case TypeClass.UDInt:
			case TypeClass.Time:
			case TypeClass.Date:
			case TypeClass.DateAndTime:
			case TypeClass.TimeOfDay:
			case TypeClass.Pointer:
				return \u0004.\u0001(\u0003, \u0004, 0L, (long)((ulong)-1));
			case TypeClass.LWord:
			case TypeClass.ULInt:
			case TypeClass.LTime:
				return \u0003;
			case TypeClass.SInt:
				return \u0004.\u0001(\u0003, \u0004, -128L, 127L);
			case TypeClass.Int:
				return \u0004.\u0001(\u0003, \u0004, -32768L, 32767L);
			case TypeClass.DInt:
				return \u0004.\u0001(\u0003, \u0004, -2147483648L, 2147483647L);
			case TypeClass.LInt:
			case TypeClass.LDate:
			case TypeClass.LDateAndTime:
			case TypeClass.LTimeOfDay:
				return \u0004.\u0001(\u0003, \u0004, long.MinValue, long.MaxValue);
			case TypeClass.Real:
			case TypeClass.LReal:
			case TypeClass.None:
			case TypeClass.AnyInt:
				return false;
			}
			return false;
		}

		// Token: 0x06001318 RID: 4888 RVA: 0x00034E14 File Offset: 0x00033014
		public static int \u0001(string \u0002, out bool \u0003)
		{
			\u0003 = false;
			int num = 0;
			IScanner scanner = APEnvironmentFacade.Instance.LanguageModelMgr.CreateScanner();
			scanner.Initialize(\u0002);
			IToken token;
			if (scanner.GetNext(out token) == TokenType.Integer)
			{
				Operator @operator = Operator.None;
				ulong num2;
				bool flag;
				bool flag2;
				scanner.GetInteger(token, out num2, out flag, out @operator, out flag2);
				if (@operator == Operator.None && !flag2 && num2 < (ulong)-1)
				{
					\u0003 = true;
					checked
					{
						try
						{
							num = (int)num2;
							if (flag)
							{
								num = 0 - num;
							}
						}
						catch
						{
							num = 0;
							\u0003 = false;
						}
					}
				}
			}
			return num;
		}

		// Token: 0x06001319 RID: 4889 RVA: 0x00034E94 File Offset: 0x00033094
		public static uint \u0001(string \u0002, out bool \u0003)
		{
			\u0003 = false;
			uint result = 0U;
			IScanner scanner = APEnvironmentFacade.Instance.LanguageModelMgr.CreateScanner();
			scanner.Initialize(\u0002);
			IToken token;
			if (scanner.GetNext(out token) == TokenType.Integer)
			{
				Operator @operator = Operator.None;
				ulong num;
				bool flag;
				bool flag2;
				scanner.GetInteger(token, out num, out flag, out @operator, out flag2);
				if (!flag && @operator == Operator.None && !flag2 && num < (ulong)-1)
				{
					\u0003 = true;
					try
					{
						result = checked((uint)num);
					}
					catch
					{
						result = 0U;
						\u0003 = false;
					}
				}
			}
			return result;
		}

		// Token: 0x0600131A RID: 4890 RVA: 0x00034F10 File Offset: 0x00033110
		public static int \u0001(IExpression \u0002, IScope \u0003, out bool \u0004)
		{
			return \u0004.\u0001(\u0002, \u0003, false, out \u0004);
		}

		// Token: 0x0600131B RID: 4891 RVA: 0x00034F1C File Offset: 0x0003311C
		public static int \u0001(IExpression \u0002, IScope \u0003, bool \u0004, out bool \u0005)
		{
			\u0005 = false;
			if (\u0002 == null)
			{
				return -1;
			}
			ILiteralValue literalValue = (\u0002 as _IExpression).Literal(\u0003, \u0004);
			if (literalValue == null)
			{
				return -1;
			}
			return literalValue.GetInt(out \u0005);
		}

		// Token: 0x0600131C RID: 4892 RVA: 0x00034F4C File Offset: 0x0003314C
		internal static ICompiledType \u0001(ICompiledType \u0002)
		{
			if (\u0002 == null)
			{
				return null;
			}
			if (\u0002.Class != TypeClass.Array)
			{
				return \u0002;
			}
			return \u0004.\u0001((\u0002 as _IArrayType).BaseType);
		}

		// Token: 0x0600131D RID: 4893 RVA: 0x00034F70 File Offset: 0x00033170
		internal static ICompiledType \u0002(ICompiledType \u0002)
		{
			if (\u0002 == null)
			{
				return null;
			}
			if (\u0002.Class != TypeClass.Array)
			{
				return \u0002;
			}
			return \u0004.\u0002((\u0002 as _IArrayType).OriginalBaseType);
		}

		// Token: 0x0600131E RID: 4894 RVA: 0x00034F94 File Offset: 0x00033194
		internal static IType \u0001(IType \u0002)
		{
			if (\u0002 == null)
			{
				return null;
			}
			if (\u0002.Class == TypeClass.Pointer)
			{
				return \u0004.\u0001((\u0002 as _IPointerType).BaseType);
			}
			if (\u0002.Class == TypeClass.Reference)
			{
				return \u0004.\u0001((\u0002 as _IReferenceType).BaseType);
			}
			return \u0002;
		}
	}
}
