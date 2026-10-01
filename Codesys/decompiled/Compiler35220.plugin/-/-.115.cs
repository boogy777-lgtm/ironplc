using System;
using \u0007;
using \u0011;
using \u001F;
using _3S.CoDeSys.Compiler35220;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;
using \u0081;

namespace \u0014
{
	// Token: 0x02000153 RID: 339
	internal sealed class \u0005
	{
		// Token: 0x060017A4 RID: 6052 RVA: 0x00048E08 File Offset: 0x00047008
		public \u0005(_IPreCompileContext \u0097\u0002, _ICompileContext \u0001\u0002)
		{
			bool flag = \u0097\u0002.KindOf == KindOfContext.Target;
			flag = (flag && \u0097\u0002 != APEnvironmentFacade.Instance.LanguageModelMgr._SystemContext);
			this.\u0001(\u0097\u0002, \u0001\u0002, global::\u0011.\u0005.\u0001);
			IPreCompileContext[] array;
			if (flag)
			{
				Guid guid = \u0097\u0002.ApplicationGuid;
				while (guid != Guid.Empty)
				{
					guid = APEnvironmentFacade.Instance.LanguageModelMgr.ApplicationDeviceTable.GetParentApplication(guid);
					if (guid == Guid.Empty)
					{
						break;
					}
					_IPreCompileContext ipreCompileContext = APEnvironmentFacade.Instance.LanguageModelMgr._GetPrecompileContext(guid);
					if (ipreCompileContext == null)
					{
						break;
					}
					_ICompileContext u = APEnvironmentFacade.Instance.LanguageModelMgr.GetCompileContext(guid) as _ICompileContext;
					this.\u0001(ipreCompileContext, u, global::\u0011.\u0005.\u0002);
				}
				array = \u0001\u0002.LibraryContexts;
				foreach (_IPreCompileContext ipreCompileContext2 in array)
				{
					if (!\u0001\u0002._LibraryTable.GetQualifiedOnly(\u0097\u0002, ipreCompileContext2.LibraryPath))
					{
						this.\u0001(ipreCompileContext2, \u0001\u0002, global::\u0011.\u0005.\u0003);
					}
				}
			}
			else
			{
				IPreCompileContext[] array2 = this.\u0001(\u0097\u0002, \u0001\u0002);
				array = array2;
				foreach (_IPreCompileContext ipreCompileContext3 in array)
				{
					if (!\u0001\u0002._LibraryTable.GetQualifiedOnly(\u0097\u0002, ipreCompileContext3.LibraryPath))
					{
						this.\u0001(ipreCompileContext3, \u0001\u0002, global::\u0011.\u0005.\u0003);
					}
				}
			}
			this.\u0002(\u0097\u0002, \u0001\u0002, global::\u0011.\u0005.\u0006);
			if (flag)
			{
				Guid guid2 = \u0097\u0002.ApplicationGuid;
				while (guid2 != Guid.Empty)
				{
					guid2 = APEnvironmentFacade.Instance.LanguageModelMgr.ApplicationDeviceTable.GetParentApplication(guid2);
					if (guid2 == Guid.Empty)
					{
						break;
					}
					_IPreCompileContext ipreCompileContext4 = APEnvironmentFacade.Instance.LanguageModelMgr._GetPrecompileContext(guid2);
					if (ipreCompileContext4 == null)
					{
						break;
					}
					this.\u0002(ipreCompileContext4, \u0001\u0002, global::\u0011.\u0005.\u0007);
				}
			}
			foreach (_IPreCompileContext ipreCompileContext5 in array)
			{
				if (!\u0001\u0002._LibraryTable.GetQualifiedOnly(\u0097\u0002, ipreCompileContext5.LibraryPath))
				{
					this.\u0002(ipreCompileContext5, \u0001\u0002, global::\u0011.\u0005.\u0008);
				}
			}
			if (flag)
			{
				this.\u0001(APEnvironmentFacade.Instance.LanguageModelMgr.Pool, \u0001\u0002, global::\u0011.\u0005.\u0004);
				this.\u0002(APEnvironmentFacade.Instance.LanguageModelMgr.Pool, \u0001\u0002, global::\u0011.\u0005.\u000E);
				foreach (IPreCompileContext preCompileContext in \u0001\u0002._LibraryTable.GetVisibleILibraries(APEnvironmentFacade.Instance.LanguageModelMgr.Pool))
				{
					_IPreCompileContext ipreCompileContext6 = (_IPreCompileContext)preCompileContext;
					if (!\u0001\u0002._LibraryTable.GetQualifiedOnly(\u0097\u0002, ipreCompileContext6.LibraryPath))
					{
						this.\u0001(ipreCompileContext6, \u0001\u0002, global::\u0011.\u0005.\u0005);
						this.\u0002(ipreCompileContext6, \u0001\u0002, global::\u0011.\u0005.\u000F);
					}
				}
			}
		}

		// Token: 0x17000578 RID: 1400
		public global::\u0007.\u0006 this[string \u0002]
		{
			get
			{
				global::\u0007.\u0006 result = null;
				if (this.\u0001.TryGetValue(\u0002, out result))
				{
					return result;
				}
				this.\u0002.TryGetValue(\u0002, out result);
				return result;
			}
		}

		// Token: 0x060017A6 RID: 6054 RVA: 0x00049108 File Offset: 0x00047308
		public global::\u0007.\u0006 \u0002(string \u0002)
		{
			global::\u0007.\u0006 result = null;
			this.\u0002.TryGetValue(\u0002, out result);
			return result;
		}

		// Token: 0x060017A7 RID: 6055 RVA: 0x00049128 File Offset: 0x00047328
		internal void \u0001(string \u0002, _IVariable \u0003, _ISignature \u0004, _IPreCompileContext \u0005, global::\u0011.\u0005 \u0006, bool \u0007)
		{
			global::\u0007.\u0006 u = null;
			if (this.\u0002.TryGetValue(\u0002, out u))
			{
				if (\u001F.\u0006.\u0001(\u0006, u.\u0001))
				{
					u = null;
					this.\u0002.Remove(\u0002);
				}
				else
				{
					u.\u0001(\u0003, \u0004, null, \u0006, \u0005);
				}
			}
			if (u == null)
			{
				u = new global::\u0007.\u0006(\u0003, \u0004, null, \u0006, \u0005)
				{
					\u0002 = \u0007
				};
				this.\u0002.Add(\u0002, u);
			}
		}

		// Token: 0x060017A8 RID: 6056 RVA: 0x0004919C File Offset: 0x0004739C
		internal void \u0001(string \u0002, _IVariable \u0003, _ISignature \u0004, _IPreCompileContext \u0005, global::\u0011.\u0005 \u0006)
		{
			global::\u0007.\u0006 u = null;
			if (this.\u0001.TryGetValue(\u0002, out u))
			{
				if (\u001F.\u0006.\u0001(\u0006, u.\u0001))
				{
					u = null;
					this.\u0001.Remove(\u0002);
				}
				else
				{
					u.\u0001(\u0003, \u0004, null, \u0006, \u0005);
				}
			}
			if (u == null)
			{
				u = new global::\u0007.\u0006(\u0003, \u0004, null, \u0006, \u0005);
				this.\u0001.Add(\u0002, u);
			}
		}

		// Token: 0x060017A9 RID: 6057 RVA: 0x00049208 File Offset: 0x00047408
		public _IPreCompileContext[] \u0001(_IPreCompileContext \u0002, _ICompileContext \u0003)
		{
			if (\u0002.KindOf != KindOfContext.Target)
			{
				return \u0081.\u0008.\u0001(\u0002, \u0003);
			}
			ICaseInsensitiveDictionary<_IPreCompileContext> caseInsensitiveDictionary = new CaseInsensitiveDictionary<_IPreCompileContext>();
			Guid guid = \u0002.ApplicationGuid;
			while (guid != Guid.Empty)
			{
				_IPreCompileContext ipreCompileContext = APEnvironmentFacade.Instance.LanguageModelMgr._GetPrecompileContext(guid);
				if (ipreCompileContext == null)
				{
					break;
				}
				foreach (_IPreCompileContext ipreCompileContext2 in \u0081.\u0008.\u0001(ipreCompileContext, \u0003))
				{
					if (!caseInsensitiveDictionary.ContainsKey(ipreCompileContext2.LibraryPath))
					{
						caseInsensitiveDictionary.Add(ipreCompileContext2.LibraryPath, ipreCompileContext2);
					}
				}
				guid = APEnvironmentFacade.Instance.LanguageModelMgr.ApplicationDeviceTable.GetParentApplication(guid);
			}
			_IPreCompileContext[] array2 = new _IPreCompileContext[caseInsensitiveDictionary.Values.Count];
			caseInsensitiveDictionary.Values.CopyTo(array2, 0);
			return array2;
		}

		// Token: 0x060017AA RID: 6058 RVA: 0x000492D0 File Offset: 0x000474D0
		internal void \u0001(_IPreCompileContext \u0002, _ISignature \u0003, _ICompileContext \u0004, global::\u0011.\u0005 \u0005)
		{
			if (\u0003.HasAttribute(CompileAttributes.ATTRIBUTE_QUALIFIED_ONLY))
			{
				return;
			}
			if (\u0003.GetFlag(SignatureFlag.Internal) && (\u0005 == global::\u0011.\u0005.\u0003 || \u0005 == global::\u0011.\u0005.\u0005))
			{
				return;
			}
			_ISignature isignature = global::\u0014.\u0005.\u0001(\u0004, \u0003.GetSearchName(\u0004));
			if (isignature == null)
			{
				return;
			}
			foreach (_IVariable ivariable in isignature.AllVariables)
			{
				this.\u0001(ivariable.VersionedName, ivariable, isignature, \u0002, \u0005);
			}
		}

		// Token: 0x060017AB RID: 6059 RVA: 0x00049364 File Offset: 0x00047564
		private void \u0001(_IPreCompileContext \u0002, _ICompileContext \u0003, global::\u0011.\u0005 \u0004)
		{
			LList<_ISignature> llist = new LList<_ISignature>();
			if (\u0002.KindOf == KindOfContext.Library)
			{
				llist.AddRange(\u0002._GVLSignatures);
				llist.AddRange(\u0003.SuperGlobalSignatures);
			}
			else
			{
				llist.AddRange(\u0003.AllGlobalSignatures);
			}
			foreach (_ISignature isignature in llist)
			{
				if (!isignature.HasAttribute(CompileAttributes.ATTRIBUTE_QUALIFIED_ONLY) && (!isignature.GetFlag(SignatureFlag.Internal) || (\u0004 != global::\u0011.\u0005.\u0003 && \u0004 != global::\u0011.\u0005.\u0005)))
				{
					_ISignature isignature2 = global::\u0014.\u0005.\u0001(\u0003, isignature.GetSearchName(\u0003));
					if (isignature2 != null)
					{
						foreach (_IVariable ivariable in isignature2.AllVariables)
						{
							this.\u0001(ivariable.VersionedName, ivariable, isignature2, \u0002, \u0004);
						}
					}
				}
			}
		}

		// Token: 0x060017AC RID: 6060 RVA: 0x00049464 File Offset: 0x00047664
		private void \u0002(_IPreCompileContext \u0002, _ICompileContext \u0003, global::\u0011.\u0005 \u0004)
		{
			foreach (_ISignature isignature in \u0002._AllSignatures)
			{
				if (!isignature.GetFlag(SignatureFlag.Internal) || (\u0004 != global::\u0011.\u0005.\u0008 && \u0004 != global::\u0011.\u0005.\u000F))
				{
					_ISignature isignature2 = global::\u0014.\u0005.\u0001(\u0003, isignature.GetSearchName(\u0003));
					if (isignature2 != null)
					{
						this.\u0001(isignature2.Name, null, isignature2, \u0002, \u0004, false);
					}
				}
			}
		}

		// Token: 0x060017AD RID: 6061 RVA: 0x000494E8 File Offset: 0x000476E8
		public static _ISignature \u0001(_ICompileContext \u0002, string \u0003)
		{
			for (_ICompileContext icompileContext = \u0002; icompileContext != null; icompileContext = icompileContext.ParentContext)
			{
				_ISignature isignature = icompileContext[\u0003];
				if (isignature != null)
				{
					return isignature;
				}
			}
			return null;
		}

		// Token: 0x04000433 RID: 1075
		private readonly ICaseInsensitiveDictionary<global::\u0007.\u0006> \u0001 = new CaseInsensitiveDictionary<global::\u0007.\u0006>();

		// Token: 0x04000434 RID: 1076
		private readonly ICaseInsensitiveDictionary<global::\u0007.\u0006> \u0002 = new CaseInsensitiveDictionary<global::\u0007.\u0006>();
	}
}
