using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using \u0003;
using \u0006;
using \u000E;
using \u0012;
using \u0018;
using \u0019;
using \u001C;
using _3S.CoDeSys.Compiler35220;
using _3S.CoDeSys.Compiler35220.Phase1_Typification;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using \u0080;
using \u0084;

namespace \u001F
{
	// Token: 0x0200030F RID: 783
	internal static class \u0010
	{
		// Token: 0x06002F47 RID: 12103 RVA: 0x000B1D70 File Offset: 0x000AFF70
		public static IList<_ISignature> \u0001(ICommonScope \u0002, _IPreCompileContext \u0003, _ICallExpression \u0004, IList<ICompiledType> \u0005, _ISignature \u0006, _ISignature4 \u0007)
		{
			\u001C.\u0011 u = new global::\u0018.\u000F(\u0003, \u0005);
			List<_ISignature> list = new List<_ISignature>();
			IList<_ISignature> list2 = u.\u0001(\u0007, \u0006.Name);
			if (list2 != null && list2.Count == 1)
			{
				return list2;
			}
			if (list2 != null && list2.Count == 0 && \u0006 != null)
			{
				return new List<_ISignature>
				{
					\u0006
				};
			}
			\u001F.\u000F u2 = new \u001F.\u000F(\u0004);
			if (!\u001F.\u0010.\u0001(\u0002, u, u2, \u0006, \u0007, list))
			{
				\u001F.\u0010.\u0001(\u0002, u, u2, \u0006, \u0007, list);
			}
			return list;
		}

		// Token: 0x06002F48 RID: 12104 RVA: 0x000B1DEC File Offset: 0x000AFFEC
		public static IList<_ISignature> \u0001(ICommonScope \u0002, _ICompileContext \u0003, _ICallExpression \u0004, _ISignature \u0005, _ISignature4 \u0006)
		{
			\u001F.\u000F u = new \u001F.\u000F(\u0004);
			return \u001F.\u0010.\u0001(\u0002, \u0003, u, \u0005, \u0006);
		}

		// Token: 0x06002F49 RID: 12105 RVA: 0x000B1E0C File Offset: 0x000B000C
		public static IList<_ISignature> \u0001(ICommonScope \u0002, _ICompileContext \u0003, IAssignmentExpression[] \u0004, _ISignature \u0005, _ISignature4 \u0006)
		{
			FBInitParameterService u = new FBInitParameterService(\u0004);
			return \u001F.\u0010.\u0001(\u0002, \u0003, u, \u0005, \u0006);
		}

		// Token: 0x06002F4A RID: 12106 RVA: 0x000B1E2C File Offset: 0x000B002C
		public static IList<_ISignature> \u0001(ICommonScope \u0002, _IPreCompileContext \u0003, IAssignmentExpression[] \u0004, IList<ICompiledType> \u0005, _ISignature \u0006, _ISignature4 \u0007)
		{
			\u001C.\u0011 u = new global::\u0018.\u000F(\u0003, \u0005);
			List<_ISignature> list = new List<_ISignature>();
			IList<_ISignature> list2 = u.\u0001(\u0007, \u0006.Name);
			if (list2 != null && list2.Count == 1)
			{
				return list2;
			}
			FBInitParameterService u2 = new FBInitParameterService(\u0004);
			if (!\u001F.\u0010.\u0001(\u0002, u, u2, \u0006, \u0007, list))
			{
				\u001F.\u0010.\u0001(\u0002, u, u2, \u0006, \u0007, list);
			}
			return list;
		}

		// Token: 0x06002F4B RID: 12107 RVA: 0x000B1E8C File Offset: 0x000B008C
		private static IList<_ISignature> \u0001(ICommonScope \u0002, _ICompileContext \u0003, \u0084.\u001A \u0004, _ISignature \u0005, _ISignature4 \u0006)
		{
			\u001C.\u0011 u = new \u0080.\u0014(\u0003);
			List<_ISignature> list = new List<_ISignature>();
			if (!\u001F.\u0010.\u0001(\u0002, u, \u0004, \u0005, \u0006, list))
			{
				\u001F.\u0010.\u0001(\u0002, u, \u0004, \u0005, \u0006, list);
			}
			return list;
		}

		// Token: 0x06002F4C RID: 12108 RVA: 0x000B1EC4 File Offset: 0x000B00C4
		public static IList<_ISignature> \u0001(_ICompileContext \u0002, _ISignature4 \u0003, string \u0004)
		{
			return ((\u001C.\u0011)new \u0080.\u0014(\u0002)).\u0001(\u0003, \u0004);
		}

		// Token: 0x06002F4D RID: 12109 RVA: 0x000B1ED4 File Offset: 0x000B00D4
		private static IVariable \u0001(\u0084.\u001A \u0002, _ISignature \u0003, int \u0004)
		{
			return \u0002.\u0001(\u0003, \u0004);
		}

		// Token: 0x06002F4E RID: 12110 RVA: 0x000B1EE0 File Offset: 0x000B00E0
		private static void \u0001(ICommonScope \u0002, \u001C.\u0011 \u0003, \u0084.\u001A \u0004, _ISignature \u0005, _ISignature4 \u0006, List<_ISignature> \u0007)
		{
			\u001F.\u0010.\u0001(\u0002, \u0003, \u0004, \u0005, \u0006, \u0007, new Func<ICompiledType, ICompiledType, ICommonScope, bool>(global::\u0006.\u0011.\u0002));
		}

		// Token: 0x06002F4F RID: 12111 RVA: 0x000B1EFC File Offset: 0x000B00FC
		private static bool \u0001(ICommonScope \u0002, \u001C.\u0011 \u0003, \u0084.\u001A \u0004, _ISignature \u0005, _ISignature4 \u0006, List<_ISignature> \u0007)
		{
			\u0084.\u001B @object = new \u0084.\u001B(\u0003);
			return \u001F.\u0010.\u0001(\u0002, \u0003, \u0004, \u0005, \u0006, \u0007, new Func<ICompiledType, ICompiledType, ICommonScope, bool>(@object.\u0001));
		}

		// Token: 0x06002F50 RID: 12112 RVA: 0x000B1F2C File Offset: 0x000B012C
		private static bool \u0001(_ISignature \u0002, _ISignature \u0003, ICommonScope \u0004)
		{
			if (\u0002.AllInputs.Length != \u0003.AllInputs.Length)
			{
				return false;
			}
			int num = 0;
			foreach (IVariable variable in \u0002.AllInputs)
			{
				IVariable variable2 = \u0003.AllInputs[num++];
				if (!variable.GetFlag(VarFlag.Implicit) && !global::\u0006.\u0011.\u0001((ICompiledType)variable.Type, (ICompiledType)variable2.Type, \u0004))
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x06002F51 RID: 12113 RVA: 0x000B1FA8 File Offset: 0x000B01A8
		private static bool \u0001(ICommonScope \u0002, \u001C.\u0011 \u0003, \u0084.\u001A \u0004, _ISignature \u0005, _ISignature4 \u0006, List<_ISignature> \u0007, Func<ICompiledType, ICompiledType, ICommonScope, bool> \u0008)
		{
			\u001F.\u0010.\u0001 u = new \u001F.\u0010.\u0001();
			u.\u0001 = \u0002;
			using (IEnumerator<_ISignature> enumerator = \u0003.\u0001(\u0006, \u0005.Name).GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					\u001F.\u0010.\u0002 u2 = new \u001F.\u0010.\u0002();
					u2.\u0001 = u;
					u2.\u0001 = enumerator.Current;
					CaseInsensitiveDictionary<IVariable> caseInsensitiveDictionary = new CaseInsensitiveDictionary<IVariable>();
					if (!\u001F.\u0010.\u0001(\u0003, u2.\u0001.\u0001, \u0004, \u0008, u2.\u0001, caseInsensitiveDictionary) && !\u0004.\u0001(caseInsensitiveDictionary, u2.\u0001) && \u0007.All(new Func<_ISignature, bool>(u2.\u0001)))
					{
						\u0007.Add(u2.\u0001);
					}
				}
			}
			_ISignature4 isignature = \u0003.\u0001(\u0006);
			if (isignature != null)
			{
				return \u001F.\u0010.\u0001(u.\u0001, \u0003, \u0004, \u0005, isignature, \u0007, \u0008);
			}
			return \u0007.Any<_ISignature>();
		}

		// Token: 0x06002F52 RID: 12114 RVA: 0x000B209C File Offset: 0x000B029C
		private static bool \u0001(\u001C.\u0011 \u0002, ICommonScope \u0003, \u0084.\u001A \u0004, Func<ICompiledType, ICompiledType, ICommonScope, bool> \u0005, _ISignature \u0006, CaseInsensitiveDictionary<IVariable> \u0007)
		{
			bool result = false;
			_IExpression[] array = \u0004.\u0001().ToArray<_IExpression>();
			for (int i = 0; i < array.Length; i++)
			{
				IVariable variable = \u001F.\u0010.\u0001(\u0004, \u0006, i);
				ICompiledType u = \u0002.\u0001(array[i], i);
				if (variable == null || !\u001F.\u0010.\u0001(\u0005, u, variable, \u0003))
				{
					result = true;
				}
				if (variable != null)
				{
					\u0007.Add(variable.Name, variable);
				}
			}
			return result;
		}

		// Token: 0x06002F53 RID: 12115 RVA: 0x000B2100 File Offset: 0x000B0300
		public static void \u0001(_IExpression \u0002, int \u0003)
		{
			_INamespaceAccessExpression inamespaceAccessExpression = \u0002 as _INamespaceAccessExpression;
			if (inamespaceAccessExpression != null)
			{
				\u001F.\u0010.\u0001(inamespaceAccessExpression._Access, \u0003);
				return;
			}
			_ICompoAccessExpression icompoAccessExpression = \u0002 as _ICompoAccessExpression;
			if (icompoAccessExpression != null)
			{
				\u001F.\u0010.\u0001(icompoAccessExpression._Right, \u0003);
				return;
			}
			_IVariableExpression ivariableExpression = \u0002 as _IVariableExpression;
			if (ivariableExpression != null)
			{
				ivariableExpression.SignatureId = \u0003;
				return;
			}
			_IGlobalScopeExpression iglobalScopeExpression = \u0002 as _IGlobalScopeExpression;
			if (iglobalScopeExpression != null)
			{
				\u001F.\u0010.\u0001(iglobalScopeExpression._Base, \u0003);
				return;
			}
			_ISystemScopeExpression isystemScopeExpression = \u0002 as _ISystemScopeExpression;
			if (isystemScopeExpression == null)
			{
				return;
			}
			\u001F.\u0010.\u0001(isystemScopeExpression._Base, \u0003);
		}

		// Token: 0x06002F54 RID: 12116 RVA: 0x000B2180 File Offset: 0x000B0380
		private static bool \u0001(Func<ICompiledType, ICompiledType, ICommonScope, bool> \u0002, ICompiledType \u0003, IVariable \u0004, ICommonScope \u0005)
		{
			ICompiledType deRefType = \u0004.OriginalType.DeRefType;
			_IArrayType iarrayType = \u0003 as _IArrayType;
			if (iarrayType != null && \u0004.HasAttribute(CompileAttributes.ATTRIBUTE_VARIABLE_LENGTH_ARRAY))
			{
				_IPointerType ipointerType = \u0004.OriginalType.DeRefType as _IPointerType;
				if (ipointerType != null)
				{
					return \u0002(iarrayType.BaseType, ipointerType.BaseType, \u0005);
				}
			}
			return \u0002(\u0003, deRefType, \u0005);
		}

		// Token: 0x06002F55 RID: 12117 RVA: 0x000B21E4 File Offset: 0x000B03E4
		internal static bool \u0001(_ICallExpression \u0002, IList<_ISignature> \u0003, global::\u000E.\u0016 \u0004, global::\u0012.\u0013 \u0005)
		{
			return \u001F.\u0010.\u0001(\u0002, \u0002.Callee.ToString(), \u0003, \u0004, \u0005);
		}

		// Token: 0x06002F56 RID: 12118 RVA: 0x000B21FC File Offset: 0x000B03FC
		internal static bool \u0001(object \u0002, string \u0003, IList<_ISignature> \u0004, global::\u000E.\u0016 \u0005, global::\u0012.\u0013 \u0006)
		{
			if (\u0004.Count == 0)
			{
				\u0005(\u0002, MessageId.Err_NoMatchingOverload, new object[]
				{
					\u0003
				});
				return true;
			}
			if (\u0004.Count >= 2)
			{
				\u0005(\u0002, MessageId.Err_Ambiguity, new object[]
				{
					\u0003
				});
				foreach (_ISignature isignature in \u0004)
				{
					_ISourcePosition isourcePosition = isignature._NameExpression.Position as _ISourcePosition;
					if (isourcePosition != null && isourcePosition.ObjectGuid == Guid.Empty)
					{
						isourcePosition.SetObjectIdentification(APEnvironmentFacade.Instance.LanguageModelMgr.LibList.GetProjectHandle(isignature.LibraryPath), isignature.ObjectGuid);
					}
					if (isourcePosition != null && isignature.HasAttribute("overloads"))
					{
						isourcePosition.Length = (short)isignature.GetAttributeValue("overloads").Length;
					}
					string u = global::\u0003.\u0006.\u0001(MessageId.Inf_RelatedPosition, Array.Empty<object>());
					\u0006(\u0002, global::\u0019.\u0003.\u0001(isourcePosition, u, Severity.Information, MessageId.Inf_RelatedPosition));
				}
				return true;
			}
			return false;
		}

		// Token: 0x02000310 RID: 784
		[CompilerGenerated]
		private sealed class \u0001
		{
			// Token: 0x040008FF RID: 2303
			public ICommonScope \u0001;
		}

		// Token: 0x02000311 RID: 785
		[CompilerGenerated]
		private sealed class \u0002
		{
			// Token: 0x06002F59 RID: 12121 RVA: 0x000B2330 File Offset: 0x000B0530
			internal bool \u0001(_ISignature \u0002)
			{
				return !\u001F.\u0010.\u0001(\u0002, this.\u0001, this.\u0001.\u0001);
			}

			// Token: 0x04000900 RID: 2304
			public _ISignature \u0001;

			// Token: 0x04000901 RID: 2305
			public \u001F.\u0010.\u0001 \u0001;
		}
	}
}
