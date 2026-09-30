using System;
using System.Linq;
using \u0006;
using \u0007;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using \u0084;

namespace \u0016
{
	// Token: 0x020002F5 RID: 757
	internal static class \u0011
	{
		// Token: 0x06002E7C RID: 11900 RVA: 0x000AD72C File Offset: 0x000AB92C
		public static bool \u0001(_IVariable \u0002, _IVariable \u0003, _ICompileContext \u0004, _ICompileContext \u0005, bool \u0006)
		{
			if (\u0003 == null || \u0005 == null)
			{
				if (\u0005 != null)
				{
					\u0002.SetFlag(VarFlag.OnlChangeInit, true);
				}
				return true;
			}
			\u0016.\u0011.\u0001(\u0003);
			\u0016.\u0011.\u0002(\u0002, \u0003);
			if (\u0016.\u0011.\u0002(\u0002, \u0003))
			{
				return true;
			}
			bool flag = \u0016.\u0011.\u0001(\u0002, \u0003, \u0006);
			bool flag2 = false;
			flag = \u0016.\u0011.\u0001(\u0002, flag, ref flag2);
			IScope5 scope = global::\u0007.\u0005.\u0001(\u0004);
			IScope5 scope2 = global::\u0007.\u0005.\u0001(\u0005);
			int num = \u0003.CompiledType.Size(scope2);
			int num2 = \u0002.CompiledType.Size(scope);
			if (num == num2 && global::\u0006.\u0011.\u0001(\u0002.CompiledType, \u0003.CompiledType, scope as ICommonScope, scope2 as ICommonScope, false, true))
			{
				return \u0016.\u0011.\u0001(\u0002, \u0003, scope, flag);
			}
			if (\u0016.\u0011.\u0001(\u0002, \u0003, scope))
			{
				return true;
			}
			if (num < num2)
			{
				flag = !flag2;
			}
			bool result = false;
			if (\u0016.\u0011.\u0001(\u0002, \u0003, scope, scope2, ref result, flag))
			{
				return result;
			}
			if (!global::\u0006.\u0011.\u0004(\u0003.CompiledType, \u0002.CompiledType, scope2 as ICommonScope, scope as ICommonScope))
			{
				\u0016.\u0011.\u0001(\u0002, \u0003);
				return flag;
			}
			if (\u0002.HasAttribute(CompileAttributes.ATTRIBUTE_NO_COPY))
			{
				\u0002.SetFlag(VarFlag.OnlChangeInit, true);
				\u0002.SetFlag(VarFlag.OnlChangeCopy, false);
				\u0003.SetFlag(VarFlag.OnlChangeExit, true);
				\u0003.SetFlag(VarFlag.OnlChangeNoExit, false);
				return flag;
			}
			if (\u0002.CompiledType.Class == TypeClass.Userdef || \u0002.CompiledType.Class == TypeClass.Array)
			{
				\u0002.SetFlag(VarFlag.OnlChangeInit, true);
				flag = !flag2;
			}
			\u0002.SetFlag(VarFlag.OnlChangeCopy, flag);
			\u0003.SetFlag(VarFlag.OnlChangeCopy, flag);
			return flag;
		}

		// Token: 0x06002E7D RID: 11901 RVA: 0x000AD8C4 File Offset: 0x000ABAC4
		private static bool \u0001(_IVariable \u0002, _IVariable \u0003, bool \u0004)
		{
			bool u = \u0016.\u0011.\u0001(\u0002, \u0003) || \u0004;
			u = \u0016.\u0011.\u0003(\u0002, \u0003, u);
			return \u0016.\u0011.\u0002(\u0002, \u0003, u);
		}

		// Token: 0x06002E7E RID: 11902 RVA: 0x000AD8F0 File Offset: 0x000ABAF0
		public static bool \u0001(_IVariable \u0002, _IVariable \u0003, _ICompileContext \u0004, _ICompileContext \u0005)
		{
			if (\u0003 == null || \u0005 == null)
			{
				if (\u0005 != null)
				{
					\u0002.SetFlag(VarFlag.OnlChangeInit, true);
				}
				return true;
			}
			\u0016.\u0011.\u0001(\u0003);
			if (\u0016.\u0011.\u0002(\u0002, \u0003))
			{
				return true;
			}
			bool flag = \u0016.\u0011.\u0003(\u0002, \u0003);
			flag = \u0016.\u0011.\u0003(\u0002, \u0003, flag);
			flag = \u0016.\u0011.\u0002(\u0002, \u0003, flag);
			if (flag)
			{
				return true;
			}
			IScope5 scope = global::\u0007.\u0005.\u0001(\u0004);
			IScope5 scope2 = global::\u0007.\u0005.\u0001(\u0005);
			int num = \u0003.CompiledType.Size(scope2);
			int num2 = \u0002.CompiledType.Size(scope);
			return (num != num2 || !global::\u0006.\u0011.\u0001(\u0002.CompiledType, \u0003.CompiledType, scope as ICommonScope, scope2 as ICommonScope)) && (!global::\u0006.\u0011.\u0001(\u0002.CompiledType, \u0003.CompiledType, scope as ICommonScope, scope as ICommonScope) || num != num2 || (\u0016.\u0011.\u0001(\u0002, scope) && \u0016.\u0011.\u0002(\u0002, \u0003, scope)));
		}

		// Token: 0x06002E7F RID: 11903 RVA: 0x000AD9D4 File Offset: 0x000ABBD4
		private static bool \u0001(_IVariable \u0002, bool \u0003, ref bool \u0004)
		{
			if (\u0002.Address != null && !\u0002.Address.Incomplete && \u0002.Address.Location == DirectVariableLocation.Input)
			{
				\u0003 = false;
				\u0004 = true;
			}
			return \u0003;
		}

		// Token: 0x06002E80 RID: 11904 RVA: 0x000ADA00 File Offset: 0x000ABC00
		private static void \u0001(_IVariable \u0002, _IVariable \u0003)
		{
			\u0002.SetFlag(VarFlag.OnlChangeInit, true);
			\u0002.SetFlag(VarFlag.OnlChangeReInit, true);
			if (\u0003.CompiledType.Class == TypeClass.Userdef)
			{
				\u0003.SetFlag(VarFlag.OnlChangeExit, true);
				\u0003.SetFlag(VarFlag.OnlChangeNoExit, false);
			}
		}

		// Token: 0x06002E81 RID: 11905 RVA: 0x000ADA58 File Offset: 0x000ABC58
		private static bool \u0001(_IVariable \u0002, _IVariable \u0003, IScope5 \u0004, IScope5 \u0005, ref bool \u0006, bool \u0007)
		{
			if (\u0002.CompiledType.Class == TypeClass.Userdef || \u0002.CompiledType.Class == TypeClass.Array)
			{
				ICompiledType compiledType = \u0002.CompiledType;
				bool flag = false;
				if (compiledType.Class == TypeClass.Array && \u0003.CompiledType.Class == TypeClass.Array)
				{
					_IArrayType iarrayType = (_IArrayType)\u0002.CompiledType;
					_IArrayType u = (_IArrayType)\u0003.CompiledType;
					compiledType = \u0084.\u0004.\u0001(iarrayType);
					flag = \u0016.\u0011.\u0001(\u0004, \u0005, iarrayType, u);
				}
				_IUserdefType iuserdefType = compiledType as _IUserdefType;
				_ISignature isignature = ((iuserdefType != null) ? iuserdefType.GetSignature(\u0004) : null) as _ISignature;
				if (!flag && isignature != null && global::\u0006.\u0011.\u0001(\u0002.CompiledType, \u0003.CompiledType, \u0004 as ICommonScope, \u0004 as ICommonScope) && !isignature.GetFlagInternal(SignatureFlagInternal.ForceOnlineChangeCopy))
				{
					\u0006 = \u0016.\u0011.\u0001(\u0002, \u0003, \u0004, \u0007);
					return true;
				}
			}
			return false;
		}

		// Token: 0x06002E82 RID: 11906 RVA: 0x000ADB30 File Offset: 0x000ABD30
		private static bool \u0001(_IVariable \u0002, _IVariable \u0003, IScope5 \u0004)
		{
			if (\u0003.CompiledType.Class == TypeClass.Userdef)
			{
				_IUserdefType iuserdefType = \u0003.CompiledType as _IUserdefType;
				if (((iuserdefType != null) ? iuserdefType.GetSignature(\u0004) : null) == null)
				{
					\u0002.SetFlag(VarFlag.OnlChangeCopy, false);
					\u0003.SetFlag(VarFlag.OnlChangeCopy, false);
					\u0002.SetFlag(VarFlag.OnlChangeInit, true);
					return true;
				}
			}
			return false;
		}

		// Token: 0x06002E83 RID: 11907 RVA: 0x000ADB90 File Offset: 0x000ABD90
		private static bool \u0002(_IVariable \u0002, _IVariable \u0003, bool \u0004)
		{
			if (\u0002.Address == null && \u0003.Address != null)
			{
				\u0004 = true;
			}
			else if (\u0002.Address != null && \u0003.Address == null)
			{
				\u0004 = true;
			}
			else if (\u0002.Address != null && !\u0002.Address.IsEqual(\u0003.Address))
			{
				\u0004 = true;
			}
			return \u0004;
		}

		// Token: 0x06002E84 RID: 11908 RVA: 0x000ADBE8 File Offset: 0x000ABDE8
		private static bool \u0003(_IVariable \u0002, _IVariable \u0003, bool \u0004)
		{
			if (\u0002.IsProperty != \u0003.IsProperty)
			{
				\u0004 = true;
			}
			if (\u0002.IsPropertyMonitor != \u0003.IsPropertyMonitor)
			{
				\u0004 = true;
			}
			return \u0004;
		}

		// Token: 0x06002E85 RID: 11909 RVA: 0x000ADC10 File Offset: 0x000ABE10
		private static bool \u0001(_IVariable \u0002, _IVariable \u0003)
		{
			VarFlag varFlag = VarFlag.ReplacedConstant | VarFlag.Constant | VarFlag.Enum | VarFlag.Alias | VarFlag.Retain | VarFlag.Persistent | VarFlag.VarConfig | VarFlag.Global | VarFlag.VarAccess | VarFlag.Temp;
			return (\u0003.Flags & varFlag) != (\u0002.Flags & varFlag);
		}

		// Token: 0x06002E86 RID: 11910 RVA: 0x000ADC3C File Offset: 0x000ABE3C
		private static bool \u0002(_IVariable \u0002, _IVariable \u0003)
		{
			if (\u0002.GetFlag(VarFlag.Static) != \u0003.GetFlag(VarFlag.Static))
			{
				\u0002.SetFlag(VarFlag.OnlChangeCopy, false);
				\u0003.SetFlag(VarFlag.OnlChangeCopy, false);
				\u0002.SetFlag(VarFlag.OnlChangeInit, true);
				return true;
			}
			return false;
		}

		// Token: 0x06002E87 RID: 11911 RVA: 0x000ADC90 File Offset: 0x000ABE90
		private static void \u0002(_IVariable \u0002, _IVariable \u0003)
		{
			if (\u0002.HasAttribute(CompileAttributes.ATTRIBUTE_INIT_ON_ONLCHANGE))
			{
				\u0002.SetFlag(VarFlag.OnlChangeInit, true);
				\u0003.SetFlag(VarFlag.OnlChangeNoExit, false);
			}
		}

		// Token: 0x06002E88 RID: 11912 RVA: 0x000ADCBC File Offset: 0x000ABEBC
		private static void \u0001(_IVariable \u0002)
		{
			\u0002.SetFlag(VarFlag.OnlChangeNoExit, true);
			\u0002.SetFlag(VarFlag.OnlChangeCopy, false);
		}

		// Token: 0x06002E89 RID: 11913 RVA: 0x000ADCD8 File Offset: 0x000ABED8
		private static bool \u0001(IScope5 \u0002, IScope5 \u0003, _IArrayType \u0004, _IArrayType \u0005)
		{
			if (\u0004._Dimensions.Count<_IArrayDimension>() != \u0005._Dimensions.Count<_IArrayDimension>())
			{
				return true;
			}
			for (int i = 0; i < \u0004._Dimensions.Count<_IArrayDimension>(); i++)
			{
				bool flag;
				int num = \u0004._Dimensions[i].Range(out flag, \u0002);
				bool flag2;
				int num2 = \u0005._Dimensions[i].Range(out flag2, \u0003);
				if (!flag || !flag2 || num != num2)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06002E8A RID: 11914 RVA: 0x000ADD50 File Offset: 0x000ABF50
		private static bool \u0001(_IVariable \u0002, _IVariable \u0003, IScope5 \u0004, bool \u0005)
		{
			_IUserdefType iuserdefType = \u0002.CompiledType as _IUserdefType;
			if (\u0002.CompiledType.Class == TypeClass.Array)
			{
				iuserdefType = (\u0084.\u0004.\u0001(\u0002.CompiledType as _IArrayType) as _IUserdefType);
			}
			ISignature signature = null;
			if (iuserdefType != null)
			{
				signature = iuserdefType.GetSignature(\u0004);
			}
			if (signature != null && signature.GetFlag(SignatureFlag.FunctionTableChanged))
			{
				\u0002.SetFlag((VarFlag)((ulong)int.MinValue), true);
			}
			\u0016.\u0011.\u0001(\u0002, \u0003, \u0005);
			\u0016.\u0011.\u0003(\u0002, \u0003);
			return \u0005;
		}

		// Token: 0x06002E8B RID: 11915 RVA: 0x000ADDCC File Offset: 0x000ABFCC
		private static void \u0003(_IVariable \u0002, _IVariable \u0003)
		{
			if (\u0002.GetFlag(VarFlag.Constant))
			{
				IExpression initial = \u0002.Initial;
				if (initial != null)
				{
					IExpression initial2 = \u0003.Initial;
					if (initial2 == null)
					{
						\u0002.SetFlag(VarFlag.OnlChangeInit, true);
						return;
					}
					if (!((_IExpression)initial2).IsEqual(initial))
					{
						\u0002.SetFlag(VarFlag.OnlChangeInit, true);
					}
				}
			}
		}

		// Token: 0x06002E8C RID: 11916 RVA: 0x000ADE24 File Offset: 0x000AC024
		private static void \u0001(_IVariable \u0002, _IVariable \u0003, bool \u0004)
		{
			if (\u0004)
			{
				if (\u0002.CompiledType.Class == TypeClass.Array && \u0084.\u0004.\u0001(\u0002.CompiledType as _IArrayType).Class != TypeClass.Userdef)
				{
					\u0002.SetFlag(VarFlag.OnlChangeInit, true);
				}
				if (!\u0002.GetFlag(VarFlag.Temp) && !\u0003.GetFlag(VarFlag.Temp))
				{
					\u0002.SetFlag(VarFlag.OnlChangeCopy, true);
					\u0003.SetFlag(VarFlag.OnlChangeCopy, true);
					return;
				}
				if (!\u0002.GetFlag(VarFlag.Temp))
				{
					\u0002.SetFlag(VarFlag.OnlChangeInit, true);
				}
			}
		}

		// Token: 0x06002E8D RID: 11917 RVA: 0x000ADEC0 File Offset: 0x000AC0C0
		private static bool \u0002(_IVariable \u0002, _IVariable \u0003, IScope5 \u0004)
		{
			return (\u0002.CompiledType.Class != TypeClass.Pointer && \u0002.CompiledType.Class != TypeClass.Reference) || \u0002.CompiledType.Class != \u0003.CompiledType.Class || !global::\u0006.\u0011.\u0001(\u0002.CompiledType.BaseType, \u0003.CompiledType.BaseType, \u0004 as ICommonScope, \u0004 as ICommonScope);
		}

		// Token: 0x06002E8E RID: 11918 RVA: 0x000ADF30 File Offset: 0x000AC130
		private static bool \u0001(_IVariable \u0002, IScope5 \u0003)
		{
			if (\u0002.CompiledType.Class == TypeClass.Userdef || \u0002.CompiledType.Class == TypeClass.Array)
			{
				ICompiledType compiledType = \u0002.CompiledType;
				if (compiledType.Class == TypeClass.Array)
				{
					compiledType = \u0084.\u0004.\u0001(compiledType as _IArrayType);
				}
				_IUserdefType iuserdefType = compiledType as _IUserdefType;
				_ISignature isignature = ((iuserdefType != null) ? iuserdefType.GetSignature(\u0003) : null) as _ISignature;
				if (isignature != null && !isignature.GetFlagInternal(SignatureFlagInternal.ForceOnlineChangeCopy))
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x06002E8F RID: 11919 RVA: 0x000ADFA8 File Offset: 0x000AC1A8
		private static bool \u0003(_IVariable \u0002, _IVariable \u0003)
		{
			VarFlag varFlag = VarFlag.ReplacedConstant | VarFlag.Constant | VarFlag.Enum | VarFlag.Alias | VarFlag.Retain | VarFlag.Persistent | VarFlag.VarAccess | VarFlag.Temp;
			return (\u0003.Flags & varFlag) != (\u0002.Flags & varFlag);
		}
	}
}
