using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using \u0007;
using \u000E;
using \u000F;
using _3S.CoDeSys.Compiler35220;
using _3S.CoDeSys.Compiler35220.Scopes;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using \u007F;

namespace \u0081
{
	// Token: 0x0200014C RID: 332
	internal sealed class \u0008 : global::\u0007.\u0005
	{
		// Token: 0x0600175F RID: 5983 RVA: 0x00047B84 File Offset: 0x00045D84
		internal new static \u0081.\u0008 \u0001(_ICompileContext \u0002, string \u0003, _ISignature \u0004, _IPreCompileContext \u0005, _ICompileContext \u0006)
		{
			_IPreCompileContext contextByLibraryPath = \u0002.GetContextByLibraryPath(\u0003);
			return \u0081.\u0008.\u0001(\u0002, \u0004, true, \u0005, contextByLibraryPath, \u0006);
		}

		// Token: 0x06001760 RID: 5984 RVA: 0x00047BA8 File Offset: 0x00045DA8
		internal new static \u0081.\u0008 \u0001(_ICompileContext \u0002, _ISignature \u0003, _IPreCompileContext \u0004, _ICompileContext \u0005)
		{
			return \u0081.\u0008.\u0001(\u0002, \u0003, true, \u0004, \u0005);
		}

		// Token: 0x06001761 RID: 5985 RVA: 0x00047BB4 File Offset: 0x00045DB4
		internal new static \u0081.\u0008 \u0001(_ICompileContext \u0002, _ISignature \u0003, bool \u0004, _IPreCompileContext \u0005, _IPreCompileContext \u0006, _ICompileContext \u0007)
		{
			\u0081.\u0008 u = new \u0081.\u0008(\u0002, \u0003, \u0004, \u0005, \u0006, \u0007);
			u.\u0001();
			return u;
		}

		// Token: 0x06001762 RID: 5986 RVA: 0x00047BCC File Offset: 0x00045DCC
		internal new static \u0081.\u0008 \u0001(_ICompileContext \u0002, _ISignature \u0003, bool \u0004, _IPreCompileContext \u0005, _ICompileContext \u0006)
		{
			\u0081.\u0008 u = new \u0081.\u0008(\u0002, \u0003, \u0004, \u0005, \u0006);
			u.\u0001();
			return u;
		}

		// Token: 0x06001763 RID: 5987 RVA: 0x00047BE0 File Offset: 0x00045DE0
		private new \u0081.\u0008 \u0001(_IPreCompileContext \u0002)
		{
			\u0081.\u0008 u = \u0081.\u0008.\u0001(this.PrimaryContext, null, true, this.\u0001, \u0002, this.\u0001);
			u.TypifierLateParseTreeLoader = this.TypifierLateParseTreeLoader;
			return u;
		}

		// Token: 0x06001764 RID: 5988 RVA: 0x00047C08 File Offset: 0x00045E08
		private new \u0081.\u0008 \u0001()
		{
			\u0081.\u0008 u = \u0081.\u0008.\u0001(this.PrimaryContext, null, true, this.\u0001, this.\u0001, this.\u0001);
			u.TypifierLateParseTreeLoader = this.TypifierLateParseTreeLoader;
			return u;
		}

		// Token: 0x06001765 RID: 5989 RVA: 0x00047C38 File Offset: 0x00045E38
		internal \u0008(_ICompileContext \u0001\u0002, _ISignature \u0018\u0003, bool \u0013\u0003, _IPreCompileContext \u0019\u0003, _IPreCompileContext \u001A\u0003, _ICompileContext \u001B\u0003) : base(\u0001\u0002, \u0018\u0003, \u0013\u0003)
		{
			this.\u0001 = \u001A\u0003;
			this.\u0001 = \u0019\u0003;
			this.\u0001 = \u001B\u0003;
		}

		// Token: 0x06001766 RID: 5990 RVA: 0x00047C5C File Offset: 0x00045E5C
		internal \u0008(_ICompileContext \u0001\u0002, _ISignature \u0018\u0003, bool \u0013\u0003, _IPreCompileContext \u0019\u0003, _ICompileContext \u001B\u0003) : base(\u0001\u0002, \u0018\u0003, \u0013\u0003)
		{
			this.\u0001 = \u0019\u0003;
			this.\u0001 = \u001B\u0003;
		}

		// Token: 0x1700056C RID: 1388
		// (get) Token: 0x06001767 RID: 5991 RVA: 0x00047C78 File Offset: 0x00045E78
		// (set) Token: 0x06001768 RID: 5992 RVA: 0x00047C80 File Offset: 0x00045E80
		internal global::\u000F.\u0015 TypifierLateParseTreeLoader { get; set; }

		// Token: 0x06001769 RID: 5993 RVA: 0x00047C8C File Offset: 0x00045E8C
		public new static _IPreCompileContext[] \u0001(_IPreCompileContext \u0002, _ICompileContext \u0003)
		{
			IList<_IPreCompileContext> visibleLibraries = \u0003._LibraryTable.GetVisibleLibraries(\u0002);
			_IPreCompileContext[] array = new _IPreCompileContext[visibleLibraries.Count];
			visibleLibraries.CopyTo(array, 0);
			return array;
		}

		// Token: 0x1700056D RID: 1389
		// (get) Token: 0x0600176A RID: 5994 RVA: 0x00047CBC File Offset: 0x00045EBC
		public _ICompileContext PrimaryContext
		{
			get
			{
				return this.\u0001[0];
			}
		}

		// Token: 0x0600176B RID: 5995 RVA: 0x00047CC8 File Offset: 0x00045EC8
		internal new _ICompileContext \u0001(_IPreCompileContext \u0002)
		{
			if (\u0002 == null || \u0002.LibraryPath == null || \u0002.LibraryPath == string.Empty)
			{
				return null;
			}
			string text = null;
			bool flag = false;
			bool bOutOfLibrary = false;
			bool bOutOfPool = false;
			_ICompileContext result = this.PrimaryContext;
			if (this.\u0001 != null && this.\u0001 != this.\u0001)
			{
				if (this.\u0001.LibraryPath.ToUpperInvariant() == \u0002.LibraryPath.ToUpperInvariant())
				{
					text = this.\u0001.Namespace;
				}
				else
				{
					text = this.PrimaryContext._LibraryTable.GetNamespaceOfLibrary(this.\u0001, \u0002.LibraryPath);
				}
				bOutOfLibrary = true;
			}
			else
			{
				for (int i = 0; i < this.\u0001.Length; i++)
				{
					text = this.\u0001[i].GetLocalLibraryNamespace(\u0002);
					if (text != null)
					{
						flag = true;
						result = this.\u0001[i];
						break;
					}
				}
				if (text == null)
				{
					text = this.PrimaryContext._LibraryTable.GetNamespaceOfLibrary(this.\u0001, \u0002.LibraryPath);
					bOutOfLibrary = false;
					bOutOfPool = true;
				}
			}
			if (!flag)
			{
				for (int j = 0; j < this.\u0001.Length; j++)
				{
					if (this.\u0001[j].ContainsLibraryReference(\u0002, text, bOutOfLibrary, bOutOfPool))
					{
						flag = true;
						result = this.\u0001[j];
						break;
					}
				}
			}
			if (!flag)
			{
				this.PrimaryContext.AddLibrary(\u0002, this.\u0001, text, bOutOfLibrary, bOutOfPool);
			}
			return result;
		}

		// Token: 0x0600176C RID: 5996 RVA: 0x00047E20 File Offset: 0x00046020
		public override IScope \u0001(int \u0002)
		{
			_IPreCompileContext libraryById = this.PrimaryContext.GetLibraryById(\u0002);
			if (libraryById != null)
			{
				return this.\u0001(libraryById);
			}
			for (int i = 1; i < this.\u0001.Length; i++)
			{
				libraryById = this.\u0001[i].GetLibraryById(\u0002);
				if (libraryById != null)
				{
					return this.\u0001(libraryById);
				}
			}
			return null;
		}

		// Token: 0x0600176D RID: 5997 RVA: 0x00047E74 File Offset: 0x00046074
		public new _ISignature \u0001(string \u0002, IList \u0003, IList \u0004)
		{
			_ISignature isignature = this.PrimaryContext.FindSuperGlobalSignature(\u0002) as _ISignature;
			if (isignature == null)
			{
				_ISignature isignature2 = (APEnvironmentFacade.Instance.LanguageModelMgr.GetPrecompileContext(this.PrimaryContext.ApplicationGuid) as _IPreCompileContext)[\u0002];
				if (isignature2 != null && isignature2.GetFlag(SignatureFlag.SuperGlobal))
				{
					isignature = isignature2;
				}
			}
			if (isignature == null)
			{
				_ISignature isignature3 = this.\u0001[\u0002];
				if (isignature3 != null && isignature3.GetFlag(SignatureFlag.SuperGlobal))
				{
					isignature = isignature3;
					\u0003.Add(isignature);
					\u0004.Add(this.\u0001);
				}
			}
			else
			{
				\u0003.Add(isignature);
				\u0004.Add(APEnvironmentFacade.Instance.LanguageModelMgr.GetPrecompileContext(this.PrimaryContext.ApplicationGuid) as _IPreCompileContext);
			}
			return isignature;
		}

		// Token: 0x0600176E RID: 5998 RVA: 0x00047F38 File Offset: 0x00046138
		private new IEnumerable<_IPreCompileContext> \u0001()
		{
			_IPreCompileContext u = this.\u0001;
			Guid guid;
			if (u != null)
			{
				guid = u.ApplicationGuid;
				yield return u;
			}
			else
			{
				guid = this.PrimaryContext.ApplicationGuid;
				if (guid == Guid.Empty)
				{
					yield return APEnvironmentFacade.Instance.LanguageModelMgr.Pool;
				}
			}
			while (guid != Guid.Empty)
			{
				yield return APEnvironmentFacade.Instance.LanguageModelMgr.GetPrecompileContext(guid) as _IPreCompileContext;
				guid = APEnvironmentFacade.Instance.LMServiceProvider.LanguageModelProviderService.GetParentApplicationGuid(guid);
			}
			yield break;
		}

		// Token: 0x1700056E RID: 1390
		public override IList<ISignature> this[string \u0002]
		{
			get
			{
				ArrayList arrayList = new ArrayList();
				ArrayList arrayList2 = new ArrayList();
				if (this.\u0001(\u0002, arrayList, arrayList2) == null)
				{
					foreach (_IPreCompileContext u in this.\u0001())
					{
						global::\u0007.\u0006 u2 = (this.PrimaryContext.SymbolTables as \u007F.\u0004).\u0001(u, this.\u0001).\u0002(\u0002);
						if (u2 != null && u2.\u0003())
						{
							if (u2.\u0001)
							{
								return u2.\u0001();
							}
							return this.\u0001(u2);
						}
						else
						{
							IList<ISignature> list = base[\u0002];
							if (list != null)
							{
								return list;
							}
						}
					}
				}
				if (arrayList.Count == 0)
				{
					return base[\u0002];
				}
				ISignature[] array = new ISignature[arrayList.Count];
				for (int i = 0; i < arrayList.Count; i++)
				{
					_ISignature u3 = arrayList[i] as _ISignature;
					_IPreCompileContext u4 = arrayList2[i] as _IPreCompileContext;
					_ISignature isignature = this.\u0001(u3, u4);
					array[i] = isignature;
				}
				return array;
			}
		}

		// Token: 0x06001770 RID: 6000 RVA: 0x00048080 File Offset: 0x00046280
		internal new global::\u0007.\u0005 \u0001(_ISignature \u0002, _IPreCompileContext \u0003)
		{
			\u0081.\u0008 u;
			if (string.IsNullOrEmpty(\u0003.LibraryPath))
			{
				u = \u0081.\u0008.\u0001(this.PrimaryContext, \u0002, true, this.\u0001, this.\u0001);
			}
			else
			{
				u = \u0081.\u0008.\u0001(this.PrimaryContext, \u0002, true, this.\u0001, \u0003, this.\u0001);
			}
			u.TypifierLateParseTreeLoader = this.TypifierLateParseTreeLoader;
			return u;
		}

		// Token: 0x06001771 RID: 6001 RVA: 0x000480E0 File Offset: 0x000462E0
		public override _IScope \u0001(string \u0002)
		{
			return \u0081.\u0008.\u0001(this.PrimaryContext, \u0002, this.\u0001, this.\u0001, this.\u0001);
		}

		// Token: 0x1700056F RID: 1391
		// (get) Token: 0x06001772 RID: 6002 RVA: 0x00048100 File Offset: 0x00046300
		public override global::\u0007.\u0005 _GlobalScope
		{
			get
			{
				return this.\u0001();
			}
		}

		// Token: 0x17000570 RID: 1392
		// (get) Token: 0x06001773 RID: 6003 RVA: 0x00048108 File Offset: 0x00046308
		public override _IScope PoolScope
		{
			get
			{
				\u0081.\u0008 u = \u0081.\u0008.\u0001(this.PrimaryContext, null, true, this.\u0001, this.\u0001, this.\u0001);
				u.TypifierLateParseTreeLoader = this.TypifierLateParseTreeLoader;
				return u;
			}
		}

		// Token: 0x17000571 RID: 1393
		// (get) Token: 0x06001774 RID: 6004 RVA: 0x00048138 File Offset: 0x00046338
		public override IScope5 SystemScope
		{
			get
			{
				\u0081.\u0008 u = \u0081.\u0008.\u0001(this.PrimaryContext, null, true, this.\u0001, APEnvironmentFacade.Instance.LanguageModelMgr._SystemContext, this.\u0001);
				u.TypifierLateParseTreeLoader = this.TypifierLateParseTreeLoader;
				return u;
			}
		}

		// Token: 0x06001775 RID: 6005 RVA: 0x00048170 File Offset: 0x00046370
		internal new _ISignature \u0001(_ISignature \u0002, _IPreCompileContext \u0003)
		{
			return global::\u000E.\u0007.\u0001(this, \u0002, \u0003, this.PrimaryContext, this.\u0001);
		}

		// Token: 0x06001776 RID: 6006 RVA: 0x00048188 File Offset: 0x00046388
		internal new _ISignature[] \u0001(global::\u0007.\u0006 \u0002)
		{
			_ISignature[] array;
			if (\u0002.\u0001())
			{
				array = new _ISignature[]
				{
					this.\u0001(\u0002.SimpleSign, \u0002.SimplePreCompileContext)
				};
				\u0002.SimpleSign = array[0];
			}
			else
			{
				array = \u0002.\u0001();
				_IPreCompileContext[] array2 = \u0002.\u0001();
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = this.\u0001(array[i], array2[i]);
				}
				\u0002.\u0001(array);
			}
			\u0002.\u0001 = true;
			return array;
		}

		// Token: 0x06001777 RID: 6007 RVA: 0x00048200 File Offset: 0x00046400
		public new static void \u0001()
		{
		}

		// Token: 0x06001778 RID: 6008 RVA: 0x00048204 File Offset: 0x00046404
		public override IVariable[] \u0001(string \u0002, out ISignature[] \u0003)
		{
			\u0003 = null;
			_IPreCompileContext ipreCompileContext = this.\u0001;
			if (ipreCompileContext == null)
			{
				ipreCompileContext = (APEnvironmentFacade.Instance.LanguageModelMgr.GetPrecompileContext(this.PrimaryContext.ApplicationGuid) as _IPreCompileContext);
			}
			SymbolTable symbolTable = (this.PrimaryContext.SymbolTables as \u007F.\u0004).\u0001(ipreCompileContext, this.\u0001);
			global::\u0007.\u0006 u = symbolTable[\u0002];
			if (u != null && u.\u0002())
			{
				IVariable[] result;
				if (!u.\u0001)
				{
					global::\u000E.\u0007.\u0001(this, u, this.PrimaryContext, this.\u0001, out result, out \u0003);
				}
				else
				{
					IVariable[] array = u.\u0001();
					result = array;
					ISignature[] array2 = u.\u0001();
					\u0003 = array2;
				}
				return result;
			}
			if (u == null)
			{
				return this.\u0001(symbolTable, \u0002, ipreCompileContext, ref \u0003);
			}
			return null;
		}

		// Token: 0x06001779 RID: 6009 RVA: 0x000482B8 File Offset: 0x000464B8
		private new IVariable[] \u0001(SymbolTable \u0002, string \u0003, _IPreCompileContext \u0004, ref ISignature[] \u0005)
		{
			IReadOnlyList<_ISignature> readOnlyList = \u0002.\u0001(\u0003);
			if (readOnlyList == null || readOnlyList.Count == 0)
			{
				return null;
			}
			return global::\u000E.\u0007.\u0001(this, readOnlyList, \u0003, this.PrimaryContext, this.\u0001, \u0004, out \u0005);
		}

		// Token: 0x0600177A RID: 6010 RVA: 0x000482F4 File Offset: 0x000464F4
		internal override bool \u0001(string \u0002, out IVariable[] \u0003, out ISignature[] \u0004)
		{
			\u0004 = null;
			ISignature signature = null;
			\u0003 = null;
			if (this.\u0004)
			{
				IVariable variable = this.\u0002(\u0002, out signature);
				if (variable != null && !variable.HasFlag(VarFlag.External))
				{
					\u0003 = new IVariable[1];
					\u0003[0] = variable;
					\u0004 = new ISignature[1];
					\u0004[0] = signature;
					return true;
				}
				signature = base.\u0001(\u0002);
				if (signature != null)
				{
					\u0004 = new ISignature[1];
					\u0004[0] = signature;
					return true;
				}
			}
			if (!base.LocalScope)
			{
				\u0003 = this.\u0001(\u0002, out \u0004);
				if (\u0003 != null)
				{
					return true;
				}
				if (this.\u0002 && this[\u0002] != null)
				{
					\u0004 = this.\u0002(\u0002);
				}
			}
			return \u0004 != null;
		}

		// Token: 0x0600177B RID: 6011 RVA: 0x00048398 File Offset: 0x00046598
		public override IScope \u0001(string \u0002)
		{
			int u = -1;
			string text = string.Empty;
			_IPreCompileContext ipreCompileContext;
			if (this.\u0001 != null && !string.IsNullOrEmpty(this.\u0001.LibraryPath))
			{
				if (this.\u0001.Namespace != null && \u0002.ToUpperInvariant() == this.\u0001.Namespace.ToUpperInvariant())
				{
					ipreCompileContext = this.\u0001;
					_ICompileContext icompileContext = this.\u0001(ipreCompileContext);
					base.Id = icompileContext.GetIdOfLibraryReference(ipreCompileContext, \u0002);
					return this;
				}
				ipreCompileContext = this.PrimaryContext._LibraryTable.GetLibraryContextByNamespace(\u0002, this.\u0001);
				if (ipreCompileContext != null)
				{
					_ICompileContext icompileContext2 = this.\u0001(ipreCompileContext);
					text = \u0002;
					u = icompileContext2.GetIdOfLibraryReference(ipreCompileContext, text);
				}
			}
			else
			{
				ipreCompileContext = this.PrimaryContext.GetLibraryByName(\u0002);
				if (ipreCompileContext == null)
				{
					ipreCompileContext = this.PrimaryContext._LibraryTable.GetLibraryContextByNamespace(\u0002, this.\u0001);
				}
				if (ipreCompileContext != null)
				{
					_ICompileContext icompileContext3 = this.\u0001(ipreCompileContext);
					text = \u0002;
					u = icompileContext3.GetIdOfLibraryReference(ipreCompileContext, text);
				}
				else
				{
					for (int i = 1; i < this.\u0001.Length; i++)
					{
						ipreCompileContext = this.\u0001[i].GetLibraryByName(\u0002);
						if (ipreCompileContext != null)
						{
							text = this.\u0001[i].GetLocalLibraryNamespace(ipreCompileContext);
							u = this.\u0001[i].GetIdOfLibraryReference(ipreCompileContext, text);
						}
					}
				}
			}
			if (ipreCompileContext != null)
			{
				\u0081.\u0008 u2 = \u0081.\u0008.\u0001(this.PrimaryContext, null, false, this.\u0001, ipreCompileContext, this.\u0001);
				u2.Id = u;
				u2.Name = text;
				u2.SearchLocalScope = false;
				u2.\u0002 = true;
				u2.TypifierLateParseTreeLoader = this.TypifierLateParseTreeLoader;
				return u2;
			}
			return null;
		}

		// Token: 0x04000410 RID: 1040
		private new readonly _IPreCompileContext \u0001;

		// Token: 0x04000411 RID: 1041
		private new readonly _ICompileContext \u0001;

		// Token: 0x04000412 RID: 1042
		[CompilerGenerated]
		private new global::\u000F.\u0015 \u0001;
	}
}
