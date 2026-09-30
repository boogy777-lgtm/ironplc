using System;
using System.Collections.Generic;
using System.Linq;
using \u0007;
using \u0010;
using \u0011;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using \u0081;

namespace _3S.CoDeSys.Compiler35220.Scopes
{
	// Token: 0x02000155 RID: 341
	internal sealed class SymbolTable
	{
		// Token: 0x060017B1 RID: 6065 RVA: 0x000495B0 File Offset: 0x000477B0
		public SymbolTable(_IPreCompileContext precom, _IPreCompileContext comconPool, _ICompileContext comcon, global::\u0010.\u0002 symbolOverrides)
		{
			this.\u0001 = precom;
			this.\u0002 = comconPool;
			this.\u0001 = comcon;
			this.\u0001 = symbolOverrides;
			this.\u0001(precom, comconPool, comcon, symbolOverrides);
		}

		// Token: 0x060017B2 RID: 6066 RVA: 0x0004960C File Offset: 0x0004780C
		public void \u0001()
		{
			this.\u0002 = new CaseInsensitiveDictionary<global::\u0007.\u0006>();
			this.\u0001 = new CaseInsensitiveDictionary<global::\u0007.\u0006>();
			this.\u0001 = new CaseInsensitiveDictionary<List<_ISignature>>();
			this.\u0001(this.\u0001, this.\u0002, this.\u0001, this.\u0001);
		}

		// Token: 0x060017B3 RID: 6067 RVA: 0x00049658 File Offset: 0x00047858
		private void \u0001(_IPreCompileContext \u0002, _IPreCompileContext \u0003, _ICompileContext \u0004, global::\u0010.\u0002 \u0005)
		{
			bool flag = \u0002.KindOf == KindOfContext.Target;
			flag = (flag && \u0002 != APEnvironmentFacade.Instance.LanguageModelMgr._SystemContext);
			this.\u0001(\u0002, global::\u0011.\u0005.\u0001);
			if (flag)
			{
				Guid guid = \u0002.ApplicationGuid;
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
					this.\u0001(ipreCompileContext, global::\u0011.\u0005.\u0002);
				}
			}
			_IPreCompileContext[] array = this.\u0001(\u0002, \u0004);
			_IPreCompileContext[] array2 = new _IPreCompileContext[0];
			if (flag)
			{
				array2 = \u0081.\u0008.\u0001(\u0003, \u0004);
			}
			foreach (_IPreCompileContext ipreCompileContext2 in array)
			{
				if (!\u0004._LibraryTable.GetQualifiedOnly(\u0002, ipreCompileContext2.LibraryPath))
				{
					this.\u0001(ipreCompileContext2, global::\u0011.\u0005.\u0003);
				}
			}
			if (flag)
			{
				this.\u0001(\u0003, global::\u0011.\u0005.\u0004);
				foreach (_IPreCompileContext ipreCompileContext3 in array2)
				{
					if (!\u0004._LibraryTable.GetQualifiedOnly(\u0002, ipreCompileContext3.LibraryPath))
					{
						this.\u0001(ipreCompileContext3, global::\u0011.\u0005.\u0005);
					}
				}
			}
			this.\u0001(\u0002, \u0005, global::\u0011.\u0005.\u0006);
			this.\u0002(\u0002, global::\u0011.\u0005.\u0006);
			if (flag)
			{
				Guid guid2 = \u0002.ApplicationGuid;
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
					this.\u0002(ipreCompileContext4, global::\u0011.\u0005.\u0007);
				}
			}
			foreach (_IPreCompileContext ipreCompileContext5 in array)
			{
				if (!\u0004._LibraryTable.GetQualifiedOnly(\u0002, ipreCompileContext5.LibraryPath))
				{
					this.\u0001(ipreCompileContext5, \u0005, global::\u0011.\u0005.\u0008);
					this.\u0002(ipreCompileContext5, global::\u0011.\u0005.\u0008);
				}
			}
			if (flag)
			{
				this.\u0002(\u0003, global::\u0011.\u0005.\u000E);
				foreach (_IPreCompileContext ipreCompileContext6 in array2)
				{
					if (!\u0004._LibraryTable.GetQualifiedOnly(\u0002, ipreCompileContext6.LibraryPath))
					{
						this.\u0001(ipreCompileContext6, \u0005, global::\u0011.\u0005.\u000F);
						this.\u0002(ipreCompileContext6, global::\u0011.\u0005.\u000F);
					}
				}
			}
			if (APEnvironmentFacade.Instance.LanguageModelMgr._SystemContext != \u0002)
			{
				this.\u0001(APEnvironmentFacade.Instance.LanguageModelMgr._SystemContext, global::\u0011.\u0005.\u0001);
				this.\u0002(APEnvironmentFacade.Instance.LanguageModelMgr._SystemContext, global::\u0011.\u0005.\u000E);
			}
		}

		// Token: 0x060017B4 RID: 6068 RVA: 0x000498D4 File Offset: 0x00047AD4
		private void \u0001(_IPreCompileContext \u0002, global::\u0010.\u0002 \u0003, global::\u0011.\u0005 \u0004)
		{
			foreach (KeyValuePair<_ISignature, string> keyValuePair in \u0003.\u0001(\u0002.LibraryPath))
			{
				this.\u0001(keyValuePair.Value, null, keyValuePair.Key, \u0003.OverrideSignaturesPrecompileContext, \u0004, true);
			}
		}

		// Token: 0x17000579 RID: 1401
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

		// Token: 0x060017B6 RID: 6070 RVA: 0x00049974 File Offset: 0x00047B74
		public global::\u0007.\u0006 \u0002(string \u0002)
		{
			global::\u0007.\u0006 result = null;
			this.\u0002.TryGetValue(\u0002, out result);
			return result;
		}

		// Token: 0x060017B7 RID: 6071 RVA: 0x00049994 File Offset: 0x00047B94
		internal IReadOnlyList<_ISignature> \u0001(string \u0002)
		{
			List<_ISignature> result;
			this.\u0001.TryGetValue(\u0002, out result);
			return result;
		}

		// Token: 0x060017B8 RID: 6072 RVA: 0x000499B4 File Offset: 0x00047BB4
		private void \u0001(string \u0002, _IVariable \u0003, _ISignature \u0004, _IPreCompileContext \u0005, global::\u0011.\u0005 \u0006, bool \u0007)
		{
			if (\u0002 != null)
			{
				global::\u0007.\u0006 u;
				if (this.\u0002.TryGetValue(\u0002, out u))
				{
					u.\u0001(\u0003, \u0004, null, \u0006, \u0005);
					return;
				}
				u = new global::\u0007.\u0006(\u0003, \u0004, null, \u0006, \u0005)
				{
					\u0002 = \u0007
				};
				this.\u0002.Add(\u0002, u);
			}
		}

		// Token: 0x060017B9 RID: 6073 RVA: 0x00049A04 File Offset: 0x00047C04
		private void \u0001(string \u0002, _IVariable \u0003, _ISignature \u0004, _IPreCompileContext \u0005, global::\u0011.\u0005 \u0006)
		{
			global::\u0007.\u0006 u;
			if (this.\u0001.TryGetValue(\u0002, out u))
			{
				u.\u0001(\u0003, \u0004, null, \u0006, \u0005);
				return;
			}
			u = new global::\u0007.\u0006(\u0003, \u0004, null, \u0006, \u0005);
			this.\u0001.Add(\u0002, u);
		}

		// Token: 0x060017BA RID: 6074 RVA: 0x00049A4C File Offset: 0x00047C4C
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

		// Token: 0x060017BB RID: 6075 RVA: 0x00049B14 File Offset: 0x00047D14
		private void \u0001(_IPreCompileContext \u0002, global::\u0011.\u0005 \u0003)
		{
			Dictionary<Guid, _ISignature> dictionary = new Dictionary<Guid, _ISignature>();
			foreach (_ISignature isignature in \u0002._GVLSignatures)
			{
				if (!isignature.HasAttribute(CompileAttributes.ATTRIBUTE_QUALIFIED_ONLY) && (!isignature.GetFlag(SignatureFlag.Internal) || (\u0003 != global::\u0011.\u0005.\u0003 && \u0003 != global::\u0011.\u0005.\u0005)))
				{
					foreach (_IVariable ivariable in isignature.AllVariables)
					{
						this.\u0001(ivariable.VersionedName, ivariable, isignature, \u0002, \u0003);
					}
					dictionary[isignature.ObjectGuid] = isignature;
				}
			}
			this.\u0001(\u0002, dictionary);
		}

		// Token: 0x060017BC RID: 6076 RVA: 0x00049BE8 File Offset: 0x00047DE8
		private void \u0001(_IPreCompileContext \u0002, Dictionary<Guid, _ISignature> \u0003)
		{
			foreach (_ISignature isignature in \u0002.AllFlat.Where(new Func<_ISignature, bool>(SymbolTable.<>c.<>9.\u0001)))
			{
				_ISignature item;
				if (\u0003.TryGetValue(isignature.ParentObjectGuid, out item))
				{
					string propertyName = IdentifierConstants.GetPropertyName(isignature.Name);
					if (!string.IsNullOrEmpty(propertyName))
					{
						List<_ISignature> list;
						if (!this.\u0001.TryGetValue(propertyName, out list))
						{
							list = (this.\u0001[propertyName] = new List<_ISignature>());
						}
						if (!list.Contains(item))
						{
							list.Add(item);
						}
					}
				}
			}
		}

		// Token: 0x060017BD RID: 6077 RVA: 0x00049CB0 File Offset: 0x00047EB0
		private void \u0002(_IPreCompileContext \u0002, global::\u0011.\u0005 \u0003)
		{
			bool flag = this.\u0001 == APEnvironmentFacade.Instance.LanguageModelMgr._SystemContext;
			foreach (_ISignature isignature in \u0002._AllSignatures)
			{
				if ((!isignature.GetFlag(SignatureFlag.Internal) || (\u0003 != global::\u0011.\u0005.\u0008 && \u0003 != global::\u0011.\u0005.\u000F)) && (!isignature.GetFlag(SignatureFlag.SystemNamespaceForced) || flag))
				{
					this.\u0001(isignature.Name, null, isignature, \u0002, \u0003, false);
				}
			}
		}

		// Token: 0x04000438 RID: 1080
		private ICaseInsensitiveDictionary<global::\u0007.\u0006> \u0001 = new CaseInsensitiveDictionary<global::\u0007.\u0006>();

		// Token: 0x04000439 RID: 1081
		private ICaseInsensitiveDictionary<global::\u0007.\u0006> \u0002 = new CaseInsensitiveDictionary<global::\u0007.\u0006>();

		// Token: 0x0400043A RID: 1082
		private ICaseInsensitiveDictionary<List<_ISignature>> \u0001 = new CaseInsensitiveDictionary<List<_ISignature>>();

		// Token: 0x0400043B RID: 1083
		private readonly _IPreCompileContext \u0001;

		// Token: 0x0400043C RID: 1084
		private readonly _IPreCompileContext \u0002;

		// Token: 0x0400043D RID: 1085
		private readonly _ICompileContext \u0001;

		// Token: 0x0400043E RID: 1086
		private readonly global::\u0010.\u0002 \u0001;
	}
}
