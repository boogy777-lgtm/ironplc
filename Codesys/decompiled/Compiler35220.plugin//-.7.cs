using System;
using \u000E;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0082
{
	// Token: 0x0200019E RID: 414
	internal static class \u0007
	{
		// Token: 0x06001DAE RID: 7598 RVA: 0x0005FFD4 File Offset: 0x0005E1D4
		public static bool \u0001(ISignature \u0002, out int \u0003, out int \u0004)
		{
			bool result = false;
			\u0003 = 0;
			\u0004 = 0;
			if (Operator.FunctionBlock == \u0002.POUType || Operator.Program == \u0002.POUType)
			{
				return false;
			}
			foreach (IVariable variable in \u0002.AllInputs)
			{
				if (!variable.GetFlag(VarFlag.Implicit))
				{
					\u0004++;
					if (variable.Initial == null)
					{
						\u0003++;
					}
					else
					{
						result = true;
					}
				}
			}
			return result;
		}

		// Token: 0x06001DAF RID: 7599 RVA: 0x00060040 File Offset: 0x0005E240
		private static bool \u0001(_ICallExpression \u0002, ISignature \u0003, int \u0004)
		{
			int num = \u0004;
			IVariable variable = \u0003[IdentifierConstants.InstancePointer];
			if (variable != null)
			{
				num--;
			}
			bool flag = false;
			int num2 = 0;
			while (!flag && num2 < \u0002.InputAssigns.Length)
			{
				flag = (\u0002.InputAssigns[num2].LValue is INullExpression);
				if (!flag)
				{
					num2++;
				}
			}
			bool flag2 = true;
			if (flag)
			{
				num2 = num;
				while (flag2)
				{
					if (num2 >= \u0003.AllInputs.Length)
					{
						break;
					}
					IVariable variable2 = \u0003.AllInputs[num2];
					if (!variable2.GetFlag(VarFlag.Implicit))
					{
						flag2 = (variable2.Initial != null);
					}
					if (flag2)
					{
						num2++;
					}
				}
			}
			else
			{
				ICaseInsensitiveDictionary<IExpression> caseInsensitiveDictionary = \u000F.\u0001(\u0002, \u0003);
				flag2 = (0 < caseInsensitiveDictionary.Count);
			}
			return flag2;
		}

		// Token: 0x06001DB0 RID: 7600 RVA: 0x000600F4 File Offset: 0x0005E2F4
		internal static bool \u0001(_ICallExpression \u0002, int \u0003, int \u0004, _ISignature \u0005, \u0007.\u0001 \u0006, \u0007.\u0002 \u0007 = null)
		{
			int num;
			int num2;
			bool flag = \u0007.\u0001(\u0005, out num, out num2);
			bool flag2 = \u0007.\u0001(\u0002, \u0005, \u0003);
			if (\u0004 < \u0003)
			{
				\u0006(\u0002, MessageId.Err_FunNeedsNInputs, new object[]
				{
					\u0005.OrgName,
					\u0004
				});
				return false;
			}
			if (flag && flag2)
			{
				if (\u0007 != null)
				{
					\u0007(\u0005);
				}
				return true;
			}
			if (flag)
			{
				\u0006(\u0002, MessageId.Err_FunNeedsAtLeastNInputs, new object[]
				{
					\u0005.OrgName,
					num,
					num2
				});
			}
			else
			{
				\u0006(\u0002, MessageId.Err_FunNeedsNInputs, new object[]
				{
					\u0005.OrgName,
					\u0004
				});
			}
			return false;
		}

		// Token: 0x06001DB1 RID: 7601 RVA: 0x000601AC File Offset: 0x0005E3AC
		public static ICompiledType \u0001(_IPartialAccessExpression \u0002, ICompiledType \u0003, \u0007.\u0001 \u0004, ICommonScope \u0005)
		{
			ICompiledType deRefType = \u0003.DeRefType;
			if (deRefType != null)
			{
				int num;
				if (!TypeTable.IsPartialAccessSupportedType(deRefType.Class, out num, \u0005))
				{
					\u0004(\u0002, MessageId.Err_PartialAccess_OnlyOnBitTypes, new object[]
					{
						deRefType
					});
				}
				else
				{
					int directVariableSizeInBits = TypeTable.GetDirectVariableSizeInBits(\u0002.PartSize);
					if (directVariableSizeInBits > num || \u0002.PartOffset < 0 || \u0002.PartOffset * directVariableSizeInBits >= num)
					{
						\u0004(\u0002, MessageId.Err_PartialAccess_NotAValidComponent, new object[]
						{
							deRefType,
							\u0002.PartSize,
							\u0002.PartOffset
						});
					}
				}
			}
			return TypeTable.GetDirectVariableSizeType(\u0002.PartSize);
		}

		// Token: 0x0200019F RID: 415
		// (Invoke) Token: 0x06001DB3 RID: 7603
		public delegate void \u0001(_IExprement exp, MessageId mid, params object[] args);

		// Token: 0x020001A0 RID: 416
		// (Invoke) Token: 0x06001DB7 RID: 7607
		public delegate void \u0002(_ISignature sign);
	}
}
