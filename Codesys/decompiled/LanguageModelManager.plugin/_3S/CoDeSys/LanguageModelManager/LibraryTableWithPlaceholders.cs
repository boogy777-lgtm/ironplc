using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Device;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.Core.TargetSettings;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x020000B6 RID: 182
	[TypeGuid("{5BD4521A-4CD8-4B36-8E20-66F5156CF8DB}")]
	[StorageVersion("3.5.3.0")]
	public class LibraryTableWithPlaceholders : GenericObject2, _ILibraryTable
	{
		// Token: 0x1700027D RID: 637
		// (get) Token: 0x06000A84 RID: 2692 RVA: 0x00017FF4 File Offset: 0x00016FF4
		// (set) Token: 0x06000A85 RID: 2693 RVA: 0x00018278 File Offset: 0x00017278
		[DefaultSerialization("libinfosave")]
		[StorageVersion("3.5.3.0")]
		[StorageIgnorable]
		private LibInfoNew[] LibInfoSave
		{
			get
			{
				LList<LibInfoNew> llist = new LList<LibInfoNew>();
				foreach (string text in this._applicationLibList)
				{
					LibInfoNew libInfoNew = new LibInfoNew();
					libInfoNew.bOutOfPool = false;
					libInfoNew.strLibraryId = text;
					libInfoNew.strNamespace = this._applicationLibNamespaces[text];
					libInfoNew.nId = this._htIdLibraryTable[libInfoNew.strNamespace + "@" + text];
					libInfoNew.strReferencingLibrary = string.Empty;
					libInfoNew.qualifiedOnly = this._applicationLibQOnly[text];
					llist.Add(libInfoNew);
				}
				foreach (string text2 in this._poolLibList)
				{
					LibInfoNew libInfoNew2 = new LibInfoNew();
					libInfoNew2.bOutOfPool = true;
					libInfoNew2.strLibraryId = text2;
					libInfoNew2.strNamespace = this._poolLibNamespaces[text2];
					libInfoNew2.nId = this._htIdLibraryTable[libInfoNew2.strNamespace + "@" + text2];
					libInfoNew2.strReferencingLibrary = string.Empty;
					libInfoNew2.qualifiedOnly = this._poolLibQOnly[text2];
					llist.Add(libInfoNew2);
				}
				foreach (string text3 in this._visibleLibrariesList.Keys)
				{
					foreach (string text4 in this._visibleLibrariesList[text3])
					{
						LibInfoNew libInfoNew3 = new LibInfoNew();
						libInfoNew3.bOutOfPool = false;
						libInfoNew3.strLibraryId = text4;
						libInfoNew3.strNamespace = this._visibleLibrariesNamespaces[text3][text4];
						libInfoNew3.nId = this._htIdLibraryTable[libInfoNew3.strNamespace + "@" + text4];
						libInfoNew3.strReferencingLibrary = text3;
						libInfoNew3.qualifiedOnly = this._visibleLibrariesQOnly[text3][text4];
						llist.Add(libInfoNew3);
					}
				}
				return llist.ToArray();
			}
			set
			{
				for (int i = 0; i < value.Length; i++)
				{
					LibInfoNew libInfoNew = value[i];
					if (libInfoNew.strLibraryId != string.Empty)
					{
						if (!this._visibleLibrariesList.ContainsKey(libInfoNew.strLibraryId))
						{
							this._visibleLibrariesList[libInfoNew.strLibraryId] = new LList<string>();
						}
						if (!this._visibleLibrariesNamespaces.ContainsKey(libInfoNew.strLibraryId))
						{
							this._visibleLibrariesNamespaces[libInfoNew.strLibraryId] = new CaseInsensitiveDictionary<string>();
						}
						if (!this._namespaceToLibraries_Libs.ContainsKey(libInfoNew.strLibraryId))
						{
							this._namespaceToLibraries_Libs[libInfoNew.strLibraryId] = new CaseInsensitiveDictionary<string>();
						}
						if (!this._visibleLibrariesQOnly.ContainsKey(libInfoNew.strLibraryId))
						{
							this._visibleLibrariesQOnly[libInfoNew.strLibraryId] = new CaseInsensitiveDictionary<bool>();
						}
						this.AddLibraryInVersionFreeLibraryIdTable(libInfoNew.strLibraryId);
					}
					if (libInfoNew.strReferencingLibrary != string.Empty)
					{
						if (!this._visibleLibrariesList.ContainsKey(libInfoNew.strReferencingLibrary))
						{
							this._visibleLibrariesList[libInfoNew.strReferencingLibrary] = new LList<string>();
						}
						if (!this._visibleLibrariesNamespaces.ContainsKey(libInfoNew.strReferencingLibrary))
						{
							this._visibleLibrariesNamespaces[libInfoNew.strReferencingLibrary] = new CaseInsensitiveDictionary<string>();
						}
						if (!this._visibleLibrariesQOnly.ContainsKey(libInfoNew.strReferencingLibrary))
						{
							this._visibleLibrariesQOnly[libInfoNew.strReferencingLibrary] = new CaseInsensitiveDictionary<bool>();
						}
						if (!this._namespaceToLibraries_Libs.ContainsKey(libInfoNew.strReferencingLibrary))
						{
							this._namespaceToLibraries_Libs[libInfoNew.strReferencingLibrary] = new CaseInsensitiveDictionary<string>();
						}
						this._visibleLibrariesList[libInfoNew.strReferencingLibrary].Add(libInfoNew.strLibraryId);
						this._visibleLibrariesNamespaces[libInfoNew.strReferencingLibrary][libInfoNew.strLibraryId] = libInfoNew.strNamespace;
						this._visibleLibrariesQOnly[libInfoNew.strReferencingLibrary][libInfoNew.strLibraryId] = libInfoNew.qualifiedOnly;
						this._namespaceToLibraries_Libs[libInfoNew.strReferencingLibrary][libInfoNew.strNamespace] = libInfoNew.strLibraryId;
					}
					else if (libInfoNew.bOutOfPool)
					{
						this._poolLibList.Add(libInfoNew.strLibraryId);
						this._poolLibNamespaces[libInfoNew.strLibraryId] = libInfoNew.strNamespace;
						this._poolLibQOnly[libInfoNew.strLibraryId] = libInfoNew.qualifiedOnly;
						this._namespaceToLibraries_Pool[libInfoNew.strNamespace] = libInfoNew.strLibraryId;
					}
					else
					{
						this._applicationLibList.Add(libInfoNew.strLibraryId);
						this._applicationLibNamespaces[libInfoNew.strLibraryId] = libInfoNew.strNamespace;
						this._applicationLibQOnly[libInfoNew.strLibraryId] = libInfoNew.qualifiedOnly;
						this._namespaceToLibraries_App[libInfoNew.strNamespace] = libInfoNew.strLibraryId;
					}
					if (!this._htLibraryIdTable.ContainsKey(libInfoNew.nId))
					{
						this._htLibraryIdTable.Add(libInfoNew.nId, libInfoNew.strLibraryId);
						this._htIdLibraryTable.Add(libInfoNew.strNamespace + "@" + libInfoNew.strLibraryId, libInfoNew.nId);
					}
				}
			}
		}

		// Token: 0x1700027E RID: 638
		// (get) Token: 0x06000A86 RID: 2694 RVA: 0x000185AC File Offset: 0x000175AC
		// (set) Token: 0x06000A87 RID: 2695 RVA: 0x00018620 File Offset: 0x00017620
		[DefaultSerialization("placholdertabl")]
		[StorageVersion("3.5.3.0")]
		[StorageIgnorable]
		private stringtuple[] placeholders
		{
			get
			{
				stringtuple[] array = new stringtuple[this._placeholderTable.Keys.Count];
				LList<string> llist = new LList<string>(this._placeholderTable.Keys);
				for (int i = 0; i < llist.Count; i++)
				{
					stringtuple stringtuple = new stringtuple();
					stringtuple.str1 = llist[i];
					stringtuple.str2 = this._placeholderTable[stringtuple.str1];
					array[i] = stringtuple;
				}
				return array;
			}
			set
			{
				for (int i = 0; i < value.Length; i++)
				{
					stringtuple stringtuple = value[i];
					this._placeholderTable.Add(stringtuple.str1, stringtuple.str2);
				}
			}
		}

		// Token: 0x1700027F RID: 639
		// (get) Token: 0x06000A88 RID: 2696 RVA: 0x00018658 File Offset: 0x00017658
		public IList<_ICompilerMessage> Messages
		{
			get
			{
				if (this._messages == null)
				{
					return new List<_ICompilerMessage>(0);
				}
				return this._messages;
			}
		}

		// Token: 0x06000A89 RID: 2697 RVA: 0x00018670 File Offset: 0x00017670
		public LibraryTableWithPlaceholders()
		{
		}

		// Token: 0x06000A8A RID: 2698 RVA: 0x0001874C File Offset: 0x0001774C
		public LibraryTableWithPlaceholders(CompileContext comcon, CompileContext comconRef)
		{
			this._libidman = comcon.LibraryIdManager;
			for (CompileContext compileContext = comcon; compileContext != null; compileContext = (compileContext.ParentContext as CompileContext))
			{
				_IPreCompileContext precom = APEnvironmentFacade.Instance.LanguageModelMgr._GetPrecompileContext(compileContext.ApplicationGuid);
				LDictionary<string, string> libRecursionCheck = new LDictionary<string, string>();
				if (!APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351300)
				{
					this.AddLibraryPlaceholderResolutionsRecursive(libRecursionCheck, compileContext.GetTargetSettings(), compileContext.ApplicationGuid, compileContext.GetDeviceIdentification(), precom);
					this.AddLibraryPlaceholderResolutionsRecursive(libRecursionCheck, compileContext.GetTargetSettings(), compileContext.ApplicationGuid, compileContext.GetDeviceIdentification(), APEnvironmentFacade.Instance.LanguageModelMgr.Pool);
				}
				if (!APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35500 && comcon.ApplicationGuid != Guid.Empty)
				{
					this.AddVisibleLibrariesRecursive(APEnvironmentFacade.Instance.LanguageModelMgr.Pool, comcon, comconRef);
				}
				this.AddVisibleLibrariesRecursive(precom, compileContext, comconRef);
			}
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35500 && comcon.ApplicationGuid != Guid.Empty)
			{
				this.AddVisibleLibrariesRecursive(APEnvironmentFacade.Instance.LanguageModelMgr.Pool, comcon, comconRef);
			}
		}

		// Token: 0x06000A8B RID: 2699 RVA: 0x00018938 File Offset: 0x00017938
		public LibraryTableWithPlaceholders(_IPreCompileContext precom)
		{
			this._placeholderTable = (precom.PlaceholderTable as CaseInsensitiveDictionary<string>);
			while (precom != null)
			{
				LDictionary<string, string> libRecursionCheck = new LDictionary<string, string>();
				this.AddLibraryPlaceholderResolutionsRecursive(libRecursionCheck, precom.GetTargetSettings(), precom.ApplicationGuid, CompileContext.GetDeviceIdentification(precom.ApplicationGuid), precom);
				this.AddLibraryPlaceholderResolutionsRecursive(libRecursionCheck, precom.GetTargetSettings(), precom.ApplicationGuid, CompileContext.GetDeviceIdentification(precom.ApplicationGuid), APEnvironmentFacade.Instance.LanguageModelMgr.Pool);
				this.AddVisibleLibrariesRecursive(precom);
				if (precom.ApplicationGuid != Guid.Empty)
				{
					this.AddVisibleLibrariesRecursive(APEnvironmentFacade.Instance.LanguageModelMgr.Pool);
				}
				Guid parentApplicationGuid = APEnvironmentFacade.Instance.LMServiceProvider.LanguageModelProviderService.GetParentApplicationGuid(precom.ApplicationGuid);
				if (!(parentApplicationGuid != Guid.Empty))
				{
					break;
				}
				precom = APEnvironmentFacade.Instance.LanguageModelMgr._GetPrecompileContext(parentApplicationGuid);
			}
		}

		// Token: 0x06000A8C RID: 2700 RVA: 0x00018AE7 File Offset: 0x00017AE7
		public IList<string> AllReferencedLibraries()
		{
			return new LList<string>(this._dicAllReferencedLibraries.Keys);
		}

		// Token: 0x06000A8D RID: 2701 RVA: 0x00018AFC File Offset: 0x00017AFC
		public bool LibraryIsUnique(string stLibraryPath)
		{
			string text = LibraryHelper.VersionFreeLibraryPath(stLibraryPath);
			return this._dicLibraryVersions.ContainsKey(text) && this._dicLibraryVersions[text].Count == 1;
		}

		// Token: 0x06000A8E RID: 2702 RVA: 0x00018B34 File Offset: 0x00017B34
		public string GetLibraryOfPlaceholder(string stPlaceholder)
		{
			CaseInsensitiveDictionary<string> placeholderTable = this._placeholderTable;
			lock (placeholderTable)
			{
				string result;
				if (this._placeholderTable.TryGetValue(stPlaceholder, ref result))
				{
					return result;
				}
			}
			return null;
		}

		// Token: 0x06000A8F RID: 2703 RVA: 0x00018B88 File Offset: 0x00017B88
		public string VersionFreeLibraryPath(string stLibraryPath)
		{
			string empty = string.Empty;
			if (this._dicAllReferencedLibraries.TryGetValue(stLibraryPath, ref empty))
			{
				return empty;
			}
			return stLibraryPath;
		}

		// Token: 0x06000A90 RID: 2704 RVA: 0x00018BB0 File Offset: 0x00017BB0
		private void AddLibraryInVersionFreeLibraryIdTable(string stLibraryId)
		{
			string text = LibraryHelper.VersionFreeLibraryPath(stLibraryId);
			this._dicAllReferencedLibraries[stLibraryId] = text;
			string text2;
			string text3;
			Version version;
			if (LibraryHelper.ParseLibraryId(stLibraryId, out text2, out text3, out version))
			{
				LDictionary<Version, Version> ldictionary;
				if (this._dicLibraryVersions.ContainsKey(text))
				{
					ldictionary = this._dicLibraryVersions[text];
				}
				else
				{
					ldictionary = new LDictionary<Version, Version>();
					this._dicLibraryVersions[text] = ldictionary;
				}
				ldictionary[version] = version;
			}
		}

		// Token: 0x06000A91 RID: 2705 RVA: 0x00018C20 File Offset: 0x00017C20
		private void AddLibraryInIdTable(CompileContext comcon, CompileContext comconRef, string stLibraryId, string stNamespace)
		{
			int num = this.GetIdOfLibraryReference(stLibraryId, stNamespace);
			if (num == Common.InvalidID)
			{
				if (comconRef != null)
				{
					num = comconRef.GetIdOfLibraryReference(stLibraryId, stNamespace);
				}
				if (num == Common.InvalidID)
				{
					if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35430)
					{
						num = this._libidman.GetNext();
					}
					else
					{
						num = comcon.LibraryIdManager.GetNext();
					}
				}
				string text = stNamespace + "@" + stLibraryId;
				this._htIdLibraryTable[text] = num;
				this._htLibraryIdTable[num] = stLibraryId;
			}
		}

		// Token: 0x06000A92 RID: 2706 RVA: 0x00018CA8 File Offset: 0x00017CA8
		public int GetIdOfLibraryReference(string stLibraryId, string stNamespace)
		{
			string text = stNamespace + "@" + stLibraryId;
			int result;
			if (this._htIdLibraryTable.TryGetValue(text, ref result))
			{
				return result;
			}
			return Common.InvalidID;
		}

		// Token: 0x06000A93 RID: 2707 RVA: 0x00018CDC File Offset: 0x00017CDC
		public string GetLibraryById(int nId)
		{
			string result = null;
			if (this._htLibraryIdTable.TryGetValue(nId, ref result))
			{
				return result;
			}
			return null;
		}

		// Token: 0x06000A94 RID: 2708 RVA: 0x00018D00 File Offset: 0x00017D00
		public IList<IPreCompileContext> GetLocalVisibleILibraries()
		{
			LList<IPreCompileContext> llist = new LList<IPreCompileContext>();
			foreach (string stLibraryId in this._applicationLibList)
			{
				_IPreCompileContext libraryContext = APEnvironmentFacade.Instance.LanguageModelMgr.GetLibraryContext(stLibraryId);
				if (libraryContext != null)
				{
					llist.Add(libraryContext);
				}
			}
			return llist;
		}

		// Token: 0x06000A95 RID: 2709 RVA: 0x00018D68 File Offset: 0x00017D68
		public IEnumerable<IPreCompileContext> GetVisibleILibraries(string stLibraryId)
		{
			LHashSet<IPreCompileContext> lhashSet = new LHashSet<IPreCompileContext>();
			if (string.IsNullOrEmpty(stLibraryId))
			{
				foreach (string stLibraryId2 in this._applicationLibList)
				{
					lhashSet.Add(APEnvironmentFacade.Instance.LanguageModelMgr.GetLibraryContext(stLibraryId2));
				}
				using (IEnumerator<string> enumerator = this._poolLibList.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						string stLibraryId3 = enumerator.Current;
						lhashSet.Add(APEnvironmentFacade.Instance.LanguageModelMgr.GetLibraryContext(stLibraryId3));
					}
					goto IL_DF;
				}
			}
			LList<string> llist;
			if (this._visibleLibrariesList.TryGetValue(stLibraryId, ref llist))
			{
				foreach (string stLibraryId4 in llist)
				{
					lhashSet.Add(APEnvironmentFacade.Instance.LanguageModelMgr.GetLibraryContext(stLibraryId4));
				}
			}
			IL_DF:
			lhashSet.Remove(null);
			return lhashSet;
		}

		// Token: 0x06000A96 RID: 2710 RVA: 0x00018E88 File Offset: 0x00017E88
		public IList<IPreCompileContext> GetVisibleILibraries(_IPreCompileContext precom)
		{
			IEnumerable<string> visibleLibraryPaths = this.GetVisibleLibraryPaths(precom);
			LList<IPreCompileContext> llist = new LList<IPreCompileContext>();
			foreach (string stLibraryId in visibleLibraryPaths)
			{
				_IPreCompileContext libraryContext = APEnvironmentFacade.Instance.LanguageModelMgr.GetLibraryContext(stLibraryId);
				if (libraryContext != null)
				{
					llist.Add(libraryContext);
				}
			}
			return llist;
		}

		// Token: 0x06000A97 RID: 2711 RVA: 0x00018EF4 File Offset: 0x00017EF4
		public IList<_IPreCompileContext> GetVisibleLibraries(_IPreCompileContext precom)
		{
			IEnumerable<string> visibleLibraryPaths = this.GetVisibleLibraryPaths(precom);
			LList<_IPreCompileContext> llist = new LList<_IPreCompileContext>();
			foreach (string stLibraryId in visibleLibraryPaths)
			{
				_IPreCompileContext libraryContext = APEnvironmentFacade.Instance.LanguageModelMgr.GetLibraryContext(stLibraryId);
				if (libraryContext != null)
				{
					llist.Add(libraryContext);
				}
			}
			return llist;
		}

		// Token: 0x06000A98 RID: 2712 RVA: 0x00018F60 File Offset: 0x00017F60
		public ICaseInsensitiveDictionary<string> GetNamespacesOfLibrary(_IPreCompileContext precomLocal)
		{
			if (precomLocal == null || precomLocal.ApplicationGuid != Guid.Empty)
			{
				return this._applicationLibNamespaces;
			}
			if (string.IsNullOrEmpty(precomLocal.LibraryPath))
			{
				return this._poolLibNamespaces;
			}
			return this._visibleLibrariesNamespaces[precomLocal.LibraryPath];
		}

		// Token: 0x06000A99 RID: 2713 RVA: 0x00018FB0 File Offset: 0x00017FB0
		public string GetLocalLibraryNamespaceRecursiveExtChecked(_IPreCompileContext precomLocal, string stLibraryToFind, Dictionary<_IPreCompileContext, _IPreCompileContext> recursionTest)
		{
			if (recursionTest.ContainsKey(precomLocal))
			{
				return null;
			}
			recursionTest.Add(precomLocal, precomLocal);
			string namespaceOfLibrary = this.GetNamespaceOfLibrary(precomLocal, stLibraryToFind);
			if (namespaceOfLibrary != null)
			{
				return namespaceOfLibrary;
			}
			IList<IPreCompileContext> visibleILibraries = this.GetVisibleILibraries(precomLocal);
			for (int i = 0; i < visibleILibraries.Count; i++)
			{
				_IPreCompileContext ipreCompileContext = (_IPreCompileContext)visibleILibraries[i];
				string localLibraryNamespaceRecursiveExtChecked = this.GetLocalLibraryNamespaceRecursiveExtChecked(ipreCompileContext, stLibraryToFind, recursionTest);
				if (localLibraryNamespaceRecursiveExtChecked != null)
				{
					return this.GetNamespaceOfLibrary(precomLocal, ipreCompileContext.LibraryId) + "." + localLibraryNamespaceRecursiveExtChecked;
				}
			}
			return null;
		}

		// Token: 0x06000A9A RID: 2714 RVA: 0x0001902E File Offset: 0x0001802E
		public string GetLocalLibraryNamespaceRecursiveExt(_IPreCompileContext precomLocal, string stLibraryToFind)
		{
			return this.GetLocalLibraryNamespaceRecursiveExtChecked(precomLocal, stLibraryToFind, new Dictionary<_IPreCompileContext, _IPreCompileContext>());
		}

		// Token: 0x06000A9B RID: 2715 RVA: 0x00019040 File Offset: 0x00018040
		public string GetLocalLibraryNamespaceRecursive(_IPreCompileContext precomLocal, string stLibraryToFind)
		{
			string namespaceOfLibrary = this.GetNamespaceOfLibrary(precomLocal, stLibraryToFind);
			if (namespaceOfLibrary != null)
			{
				return namespaceOfLibrary;
			}
			IList<IPreCompileContext> visibleILibraries = this.GetVisibleILibraries(precomLocal);
			for (int i = 0; i < visibleILibraries.Count; i++)
			{
				_IPreCompileContext ipreCompileContext = (_IPreCompileContext)visibleILibraries[i];
				string localLibraryNamespaceRecursive = this.GetLocalLibraryNamespaceRecursive(precomLocal, ipreCompileContext, new Dictionary<_IPreCompileContext, _IPreCompileContext>());
				if (localLibraryNamespaceRecursive != null)
				{
					return this.GetNamespaceOfLibrary(precomLocal, ipreCompileContext.LibraryId) + "." + localLibraryNamespaceRecursive;
				}
			}
			return null;
		}

		// Token: 0x06000A9C RID: 2716 RVA: 0x000190B0 File Offset: 0x000180B0
		public string GetLocalLibraryNamespaceRecursive(_IPreCompileContext precomToFind, _IPreCompileContext precomToSearchIn, Dictionary<_IPreCompileContext, _IPreCompileContext> recursionTest)
		{
			if (recursionTest.ContainsKey(precomToSearchIn))
			{
				return null;
			}
			recursionTest.Add(precomToSearchIn, precomToSearchIn);
			IList<_IPreCompileContext> visibleLibraries = this.GetVisibleLibraries(precomToSearchIn);
			string namespaceOfLibrary = this.GetNamespaceOfLibrary(precomToSearchIn, precomToFind.LibraryPath);
			if (namespaceOfLibrary != null)
			{
				return namespaceOfLibrary;
			}
			foreach (_IPreCompileContext ipreCompileContext in visibleLibraries)
			{
				string localLibraryNamespaceRecursive = this.GetLocalLibraryNamespaceRecursive(precomToFind, ipreCompileContext, recursionTest);
				if (localLibraryNamespaceRecursive != null)
				{
					return this.GetNamespaceOfLibrary(precomToSearchIn, ipreCompileContext.LibraryPath) + "." + localLibraryNamespaceRecursive;
				}
			}
			return null;
		}

		// Token: 0x06000A9D RID: 2717 RVA: 0x00019150 File Offset: 0x00018150
		private bool? GetQualifiedOnlyRecursiveChecked(_IPreCompileContext precomToSearchIn, string stLibraryIdToLookup, Dictionary<_IPreCompileContext, _IPreCompileContext> recursionTest)
		{
			if (recursionTest.ContainsKey(precomToSearchIn))
			{
				return null;
			}
			recursionTest.Add(precomToSearchIn, precomToSearchIn);
			if (APEnvironmentFacade.Instance.LanguageModelMgr.GetLibraryContext(stLibraryIdToLookup).QualifiedAccessOnly)
			{
				return new bool?(true);
			}
			bool? flag = this.GetQualifiedOnlyExt(precomToSearchIn, stLibraryIdToLookup);
			if (flag != null)
			{
				return new bool?(flag.Value);
			}
			foreach (_IPreCompileContext ipreCompileContext in this.GetVisibleLibraries(precomToSearchIn))
			{
				flag = this.GetQualifiedOnlyRecursiveChecked(ipreCompileContext, stLibraryIdToLookup, recursionTest);
				if (flag != null)
				{
					if (flag.Value)
					{
						return new bool?(true);
					}
					if (ipreCompileContext.QualifiedAccessOnly)
					{
						return new bool?(true);
					}
					bool? qualifiedOnlyExt = this.GetQualifiedOnlyExt(precomToSearchIn, ipreCompileContext.LibraryPath);
					if (qualifiedOnlyExt != null)
					{
						return new bool?(flag.Value || qualifiedOnlyExt.Value);
					}
					return new bool?(flag.Value);
				}
			}
			return null;
		}

		// Token: 0x06000A9E RID: 2718 RVA: 0x00019278 File Offset: 0x00018278
		public bool? GetQualifiedOnlyRecursive(_IPreCompileContext precomLocal, string stLibraryIdToLookup)
		{
			Dictionary<_IPreCompileContext, _IPreCompileContext> recursionTest = new Dictionary<_IPreCompileContext, _IPreCompileContext>();
			return this.GetQualifiedOnlyRecursiveChecked(precomLocal, stLibraryIdToLookup, recursionTest);
		}

		// Token: 0x06000A9F RID: 2719 RVA: 0x00019294 File Offset: 0x00018294
		public string GetNamespaceOfLocalLibrary(string stLibraryIdToLookup)
		{
			string result = null;
			bool flag = this._applicationLibNamespaces.TryGetValue(stLibraryIdToLookup, ref result);
			if (!flag)
			{
				flag = this._poolLibNamespaces.TryGetValue(stLibraryIdToLookup, ref result);
			}
			if (flag)
			{
				return result;
			}
			return null;
		}

		// Token: 0x06000AA0 RID: 2720 RVA: 0x000192CC File Offset: 0x000182CC
		public bool? GetQualifiedOnlyExt(_IPreCompileContext precomLocal, string stLibraryIdToLookup)
		{
			bool value = false;
			bool flag = false;
			CaseInsensitiveDictionary<bool> caseInsensitiveDictionary;
			if (precomLocal.ApplicationGuid != Guid.Empty)
			{
				flag = this._applicationLibQOnly.TryGetValue(stLibraryIdToLookup, ref value);
			}
			else if (string.IsNullOrEmpty(precomLocal.LibraryPath))
			{
				flag = this._poolLibQOnly.TryGetValue(stLibraryIdToLookup, ref value);
			}
			else if (this._visibleLibrariesQOnly.TryGetValue(precomLocal.LibraryPath, ref caseInsensitiveDictionary))
			{
				flag = caseInsensitiveDictionary.TryGetValue(stLibraryIdToLookup, ref value);
			}
			if (flag)
			{
				return new bool?(value);
			}
			return null;
		}

		// Token: 0x06000AA1 RID: 2721 RVA: 0x00019350 File Offset: 0x00018350
		public bool GetQualifiedOnly(_IPreCompileContext precomLocal, string stLibraryIdToLookup)
		{
			bool? qualifiedOnlyExt = this.GetQualifiedOnlyExt(precomLocal, stLibraryIdToLookup);
			return qualifiedOnlyExt != null && qualifiedOnlyExt.Value;
		}

		// Token: 0x06000AA2 RID: 2722 RVA: 0x00019378 File Offset: 0x00018378
		public IEnumerable Get32BitOnly()
		{
			return this._libraries32BitOnlyList.Values;
		}

		// Token: 0x06000AA3 RID: 2723 RVA: 0x00019388 File Offset: 0x00018388
		public string GetNamespaceOfLibrary(_IPreCompileContext precomLocal, string stLibraryIdToLookup)
		{
			string result = null;
			bool flag = false;
			CaseInsensitiveDictionary<string> caseInsensitiveDictionary;
			if (precomLocal.ApplicationGuid != Guid.Empty)
			{
				flag = this._applicationLibNamespaces.TryGetValue(stLibraryIdToLookup, ref result);
			}
			else if (string.IsNullOrEmpty(precomLocal.LibraryPath))
			{
				flag = this._poolLibNamespaces.TryGetValue(stLibraryIdToLookup, ref result);
			}
			else if (this._visibleLibrariesNamespaces.TryGetValue(precomLocal.LibraryPath, ref caseInsensitiveDictionary))
			{
				flag = caseInsensitiveDictionary.TryGetValue(stLibraryIdToLookup, ref result);
			}
			if (flag)
			{
				return result;
			}
			return null;
		}

		// Token: 0x06000AA4 RID: 2724 RVA: 0x00019400 File Offset: 0x00018400
		public _IPreCompileContext GetLibraryContextByNamespace(string stNamespace)
		{
			string libraryIdByNamespace = this.GetLibraryIdByNamespace(stNamespace);
			if (string.IsNullOrEmpty(libraryIdByNamespace))
			{
				return null;
			}
			return APEnvironmentFacade.Instance.LanguageModelMgr.GetLibraryContext(libraryIdByNamespace);
		}

		// Token: 0x06000AA5 RID: 2725 RVA: 0x00019430 File Offset: 0x00018430
		public _IPreCompileContext GetLibraryContextByNamespace(string stNamespace, _IPreCompileContext precomLocal)
		{
			string libraryIdByNamespace = this.GetLibraryIdByNamespace(stNamespace, precomLocal);
			if (string.IsNullOrEmpty(libraryIdByNamespace))
			{
				return null;
			}
			return APEnvironmentFacade.Instance.LanguageModelMgr.GetLibraryContext(libraryIdByNamespace);
		}

		// Token: 0x06000AA6 RID: 2726 RVA: 0x00019460 File Offset: 0x00018460
		public string GetLibraryIdByNamespace(string stNamespace)
		{
			string result = null;
			bool flag = this._namespaceToLibraries_App.TryGetValue(stNamespace, ref result);
			if (!flag)
			{
				flag = this._namespaceToLibraries_Pool.TryGetValue(stNamespace, ref result);
			}
			if (flag)
			{
				return result;
			}
			return null;
		}

		// Token: 0x06000AA7 RID: 2727 RVA: 0x00019498 File Offset: 0x00018498
		public string GetLibraryIdByNamespace(string stNamespace, _IPreCompileContext precomLocal)
		{
			string result = null;
			bool flag = false;
			CaseInsensitiveDictionary<string> caseInsensitiveDictionary;
			if (precomLocal.ApplicationGuid != Guid.Empty)
			{
				flag = this._namespaceToLibraries_App.TryGetValue(stNamespace, ref result);
			}
			else if (string.IsNullOrEmpty(precomLocal.LibraryPath))
			{
				flag = this._namespaceToLibraries_Pool.TryGetValue(stNamespace, ref result);
			}
			else if (this._namespaceToLibraries_Libs.TryGetValue(precomLocal.LibraryPath, ref caseInsensitiveDictionary))
			{
				flag = caseInsensitiveDictionary.TryGetValue(stNamespace, ref result);
			}
			if (flag)
			{
				return result;
			}
			return null;
		}

		// Token: 0x06000AA8 RID: 2728 RVA: 0x00019510 File Offset: 0x00018510
		public IList<string> GetVisibleLibraryPaths(_IPreCompileContext precom)
		{
			if (precom.ApplicationGuid != Guid.Empty)
			{
				return this._applicationLibList;
			}
			if (string.IsNullOrEmpty(precom.LibraryPath))
			{
				return this._poolLibList;
			}
			if (!this._visibleLibrariesList.ContainsKey(precom.LibraryPath))
			{
				return new LList<string>(0);
			}
			return this._visibleLibrariesList[precom.LibraryPath];
		}

		// Token: 0x06000AA9 RID: 2729 RVA: 0x00019578 File Offset: 0x00018578
		private void AddLibraryConflictMessage(string stNamespace)
		{
			if (this._messages == null)
			{
				this._messages = new List<_ICompilerMessage>();
			}
			INamespaceConflictChecker namespaceConflictCheckerOrNull = APEnvironmentFacade.Instance.NamespaceConflictCheckerOrNull;
			IEnumerable<INamespaceConflictIssue> issues = (namespaceConflictCheckerOrNull != null) ? namespaceConflictCheckerOrNull.CheckConflictingNamespace(stNamespace) : null;
			CompilerProxy._CompilerMessageCreator.AddLibraryConflictIssues(this._messages, issues);
		}

		// Token: 0x06000AAA RID: 2730 RVA: 0x000195C1 File Offset: 0x000185C1
		private void AddVisibleLibrariesRecursive(_IPreCompileContext precom)
		{
			this.AddVisibleLibrariesRecursive(precom, null, null);
		}

		// Token: 0x06000AAB RID: 2731 RVA: 0x000195CC File Offset: 0x000185CC
		private void AddVisibleLibrariesRecursive(_IPreCompileContext precom, CompileContext comcon, CompileContext comconRef)
		{
			LList<string> llist = null;
			ICaseInsensitiveDictionary<string> caseInsensitiveDictionary = null;
			if (precom == null)
			{
				return;
			}
			this.GetAllVisibleLibraries(precom as PreCompileContext, out llist, out caseInsensitiveDictionary, this);
			CaseInsensitiveDictionary<bool> caseInsensitiveDictionary2 = null;
			if (precom.ApplicationGuid != Guid.Empty)
			{
				caseInsensitiveDictionary2 = this._applicationLibQOnly;
				if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35600)
				{
					this._applicationLibList.AddRange(from lib in llist
					where !this._applicationLibList.Contains(lib)
					select lib);
				}
				else
				{
					this._applicationLibList = llist;
					this._applicationLibNamespaces = (caseInsensitiveDictionary as CaseInsensitiveDictionary<string>);
				}
				using (IEnumerator<string> enumerator = caseInsensitiveDictionary.Keys.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						string text = enumerator.Current;
						string text2 = caseInsensitiveDictionary[text];
						if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35600 && !this._applicationLibNamespaces.ContainsKey(text))
						{
							this._applicationLibNamespaces.Add(text, text2);
						}
						if (!this._namespaceToLibraries_App.ContainsKey(text2))
						{
							this._namespaceToLibraries_App[text2] = text;
						}
					}
					goto IL_25D;
				}
			}
			if (string.IsNullOrEmpty(precom.LibraryPath))
			{
				caseInsensitiveDictionary2 = this._poolLibQOnly;
				this._poolLibList = llist;
				this._poolLibNamespaces = (caseInsensitiveDictionary as CaseInsensitiveDictionary<string>);
				using (IEnumerator<string> enumerator = caseInsensitiveDictionary.Keys.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						string text3 = enumerator.Current;
						string text4 = caseInsensitiveDictionary[text3];
						if (!this._namespaceToLibraries_Pool.ContainsKey(text4))
						{
							this._namespaceToLibraries_Pool[text4] = text3;
						}
					}
					goto IL_25D;
				}
			}
			if (!this._visibleLibrariesQOnly.ContainsKey(precom.LibraryPath))
			{
				caseInsensitiveDictionary2 = new CaseInsensitiveDictionary<bool>();
				this._visibleLibrariesQOnly.Add(precom.LibraryPath, caseInsensitiveDictionary2);
			}
			else
			{
				caseInsensitiveDictionary2 = this._visibleLibrariesQOnly[precom.LibraryPath];
			}
			this._visibleLibrariesList[precom.LibraryPath] = llist;
			this._visibleLibrariesNamespaces[precom.LibraryPath] = (caseInsensitiveDictionary as CaseInsensitiveDictionary<string>);
			CaseInsensitiveDictionary<string> caseInsensitiveDictionary3 = new CaseInsensitiveDictionary<string>();
			foreach (string text5 in caseInsensitiveDictionary.Keys)
			{
				string text6 = caseInsensitiveDictionary[text5];
				if (!caseInsensitiveDictionary3.ContainsKey(text6))
				{
					caseInsensitiveDictionary3[text6] = text5;
				}
				if (precom.Namespace == text6)
				{
					this.AddLibraryConflictMessage(text6);
				}
			}
			this._namespaceToLibraries_Libs[precom.LibraryPath] = caseInsensitiveDictionary3;
			IL_25D:
			if (precom.Support32BitOnly && !this._libraries32BitOnlyList.ContainsKey(precom.LibraryId))
			{
				this._libraries32BitOnlyList[precom.LibraryId] = precom;
			}
			foreach (string text7 in llist)
			{
				string stNamespace = caseInsensitiveDictionary[text7];
				this.AddLibraryInVersionFreeLibraryIdTable(text7);
				if (comcon != null)
				{
					this.AddLibraryInIdTable(comcon, comconRef, text7, stNamespace);
				}
				_IPreCompileContext libraryContext = APEnvironmentFacade.Instance.LanguageModelMgr.GetLibraryContext(text7);
				if (libraryContext != null)
				{
					if (!this._visibleLibrariesList.ContainsKey(text7))
					{
						this.AddVisibleLibrariesRecursive(libraryContext, comcon, comconRef);
					}
					if (!caseInsensitiveDictionary2.ContainsKey(text7))
					{
						caseInsensitiveDictionary2.Add(text7, precom.QualifiedAccessOnlyLocal(libraryContext));
					}
					if (libraryContext.Support32BitOnly && !this._libraries32BitOnlyList.ContainsKey(libraryContext.LibraryId))
					{
						this._libraries32BitOnlyList[libraryContext.LibraryId] = libraryContext;
					}
				}
			}
		}

		// Token: 0x06000AAC RID: 2732 RVA: 0x00019960 File Offset: 0x00018960
		internal void GetAllVisibleLibraries(PreCompileContext precom, out LList<string> llibs, out ICaseInsensitiveDictionary<string> llibNamespaces, _ILibraryTable libtable)
		{
			llibs = new LList<string>();
			llibNamespaces = new CaseInsensitiveDictionary<string>();
			ICaseInsensitiveDictionary<string> caseInsensitiveDictionary = new CaseInsensitiveDictionary<string>();
			foreach (string text in precom.GetAllLocalLibraries())
			{
				if (!caseInsensitiveDictionary.ContainsKey(text))
				{
					llibs.Add(text);
					llibNamespaces.Add(text, precom.NameLibraryTable[text]);
					caseInsensitiveDictionary[text] = text;
				}
			}
			_ILibraryPlaceholder[] array;
			if (!APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35700)
			{
				Hashtable hashtable = new Hashtable();
				foreach (string key in precom.PlaceholderMap.Keys)
				{
					hashtable.Add(key, precom.PlaceholderMap[key]);
				}
				array = new _ILibraryPlaceholder[hashtable.Values.Count];
				hashtable.Values.CopyTo(array, 0);
			}
			else
			{
				array = precom.Placeholders;
			}
			foreach (_ILibraryPlaceholder ilibraryPlaceholder in array)
			{
				string libraryOfPlaceholder = libtable.GetLibraryOfPlaceholder(ilibraryPlaceholder.Name);
				if (!string.IsNullOrEmpty(libraryOfPlaceholder) && !caseInsensitiveDictionary.ContainsKey(libraryOfPlaceholder))
				{
					caseInsensitiveDictionary[libraryOfPlaceholder] = libraryOfPlaceholder;
					llibNamespaces.Add(libraryOfPlaceholder, ilibraryPlaceholder.Namespace);
					llibs.Add(libraryOfPlaceholder);
				}
			}
			this.AddVisibleSubLibraries(precom, llibs, llibNamespaces, caseInsensitiveDictionary, libtable);
		}

		// Token: 0x06000AAD RID: 2733 RVA: 0x00019AEC File Offset: 0x00018AEC
		private void AddVisibleSubLibraries(PreCompileContext precom, LList<string> llibs, ICaseInsensitiveDictionary<string> llibNamespaces, ICaseInsensitiveDictionary<string> dicLibraries, _ILibraryTable libtable)
		{
			foreach (string stLibraryId in precom.GetAllLocalLibraries())
			{
				_IPreCompileContext libraryContext = APEnvironmentFacade.Instance.LanguageModelMgr.LibList.GetLibraryContext(stLibraryId);
				if (libraryContext != null && !precom.QualifiedAccessOnlyLocal(libraryContext))
				{
					this.AddVisibleLibraries(libraryContext as PreCompileContext, llibs, llibNamespaces, dicLibraries, libtable);
				}
			}
			foreach (_ILibraryPlaceholder ilibraryPlaceholder in precom.Placeholders)
			{
				string libraryOfPlaceholder = libtable.GetLibraryOfPlaceholder(ilibraryPlaceholder.Name);
				if (!string.IsNullOrEmpty(libraryOfPlaceholder))
				{
					_IPreCompileContext libraryContext2 = APEnvironmentFacade.Instance.LanguageModelMgr.GetLibraryContext(libraryOfPlaceholder);
					if (libraryContext2 != null && !precom.QualifiedAccessOnlyLocal(libraryContext2))
					{
						this.AddVisibleLibraries(libraryContext2 as PreCompileContext, llibs, llibNamespaces, dicLibraries, libtable);
					}
				}
			}
		}

		// Token: 0x06000AAE RID: 2734 RVA: 0x00019BD4 File Offset: 0x00018BD4
		private void AddVisibleLibraries(PreCompileContext precom, LList<string> llibs, ICaseInsensitiveDictionary<string> llibNamespaces, ICaseInsensitiveDictionary<string> dicLibraries, _ILibraryTable libtable)
		{
			foreach (string text in precom.GetAllLocalLibraries())
			{
				_IPreCompileContext libraryContext = APEnvironmentFacade.Instance.LanguageModelMgr.LibList.GetLibraryContext(text);
				if (precom.PublishSymbols(libraryContext) && !dicLibraries.ContainsKey(text) && precom.NameLibraryTable.ContainsKey(text))
				{
					dicLibraries[text] = text;
					llibNamespaces.Add(text, precom.NameLibraryTable[text]);
					llibs.Add(text);
				}
			}
			foreach (_ILibraryPlaceholder ilibraryPlaceholder in precom.Placeholders)
			{
				if (ilibraryPlaceholder.PublishSymbols)
				{
					string libraryOfPlaceholder = libtable.GetLibraryOfPlaceholder(ilibraryPlaceholder.Name);
					if (!string.IsNullOrEmpty(libraryOfPlaceholder) && ilibraryPlaceholder.PublishSymbols && !dicLibraries.ContainsKey(libraryOfPlaceholder))
					{
						dicLibraries[libraryOfPlaceholder] = libraryOfPlaceholder;
						llibNamespaces.Add(libraryOfPlaceholder, ilibraryPlaceholder.Namespace);
						llibs.Add(libraryOfPlaceholder);
					}
				}
			}
		}

		// Token: 0x06000AAF RID: 2735 RVA: 0x00019CEC File Offset: 0x00018CEC
		private void AddLibraryPlaceholderResolutionsRecursive(LDictionary<string, string> libRecursionCheck, ITargetSettings tarset, Guid appObjectGuid, IDeviceIdentification devid, _IPreCompileContext precom)
		{
			if (precom == null)
			{
				return;
			}
			if (!string.IsNullOrEmpty(precom.LibraryPath))
			{
				if (libRecursionCheck.ContainsKey(precom.LibraryPath))
				{
					return;
				}
				libRecursionCheck.Add(precom.LibraryPath, precom.LibraryPath);
			}
			_ILibraryPlaceholder[] placeholders = precom.Placeholders;
			int i = 0;
			while (i < placeholders.Length)
			{
				_ILibraryPlaceholder ilibraryPlaceholder = placeholders[i];
				_IPreCompileContext ipreCompileContext = null;
				bool flag = true;
				CaseInsensitiveDictionary<string> placeholderTable = this._placeholderTable;
				lock (placeholderTable)
				{
					string text;
					if (this._placeholderTable.TryGetValue(ilibraryPlaceholder.Name, ref text))
					{
						if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35500)
						{
							if (text == null)
							{
								if (Debugger.IsAttached)
								{
									Debugger.Break();
								}
								goto IL_2A4;
							}
							ipreCompileContext = APEnvironmentFacade.Instance.LanguageModelMgr.GetLibraryContext(text);
							if (ipreCompileContext == null)
							{
								goto IL_2A4;
							}
							if (!ipreCompileContext.LinkAll)
							{
								ipreCompileContext.LinkAll = ilibraryPlaceholder.LinkAllContent;
							}
							if (!ipreCompileContext.LinkInSimulation)
							{
								ipreCompileContext.LinkInSimulation = ilibraryPlaceholder.LinkInSimulation;
							}
							if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35600)
							{
								flag = false;
							}
						}
					}
					else
					{
						flag = false;
						string libraryPath;
						ipreCompileContext = PreCompileContext._ResolveLibraryPlaceholder(tarset, appObjectGuid, ilibraryPlaceholder, devid, out libraryPath);
						if (ipreCompileContext != null)
						{
							libraryPath = ipreCompileContext.LibraryPath;
						}
						if (ipreCompileContext != null && !string.IsNullOrEmpty(libraryPath))
						{
							if (ilibraryPlaceholder.Name == null)
							{
								if (Debugger.IsAttached)
								{
									Debugger.Break();
								}
								goto IL_2A4;
							}
							if (!this._placeholderTable.ContainsKey(ilibraryPlaceholder.Name))
							{
								this._placeholderTable.Add(ilibraryPlaceholder.Name, libraryPath);
							}
						}
						if (ipreCompileContext == null)
						{
							goto IL_2A4;
						}
					}
				}
				goto IL_178;
				IL_2A4:
				i++;
				continue;
				IL_178:
				if (flag)
				{
					goto IL_2A4;
				}
				if (!string.IsNullOrEmpty(precom.LibraryPath))
				{
					CaseInsensitiveDictionary<bool> caseInsensitiveDictionary;
					if (this._visibleLibrariesQOnly.ContainsKey(precom.LibraryPath))
					{
						caseInsensitiveDictionary = this._visibleLibrariesQOnly[precom.LibraryPath];
					}
					else
					{
						caseInsensitiveDictionary = new CaseInsensitiveDictionary<bool>();
						this._visibleLibrariesQOnly.Add(precom.LibraryPath, caseInsensitiveDictionary);
					}
					if (!caseInsensitiveDictionary.ContainsKey(ipreCompileContext.LibraryPath))
					{
						caseInsensitiveDictionary.Add(ipreCompileContext.LibraryPath, ilibraryPlaceholder.QualifiedOnlyLocal);
					}
				}
				else if (ipreCompileContext.LibraryPath == null)
				{
					if (Debugger.IsAttached)
					{
						Debugger.Break();
						goto IL_2A4;
					}
					goto IL_2A4;
				}
				else if (precom.ApplicationGuid != Guid.Empty)
				{
					this._applicationLibQOnly[ipreCompileContext.LibraryPath] = ilibraryPlaceholder.QualifiedOnlyLocal;
				}
				else
				{
					this._poolLibQOnly[ipreCompileContext.LibraryPath] = ilibraryPlaceholder.QualifiedOnlyLocal;
				}
				if (ipreCompileContext.LibraryId != null)
				{
					if (ipreCompileContext.Support32BitOnly && !this._libraries32BitOnlyList.ContainsKey(ipreCompileContext.LibraryId))
					{
						this._libraries32BitOnlyList[ipreCompileContext.LibraryId] = ipreCompileContext;
					}
					this.AddLibraryPlaceholderResolutionsRecursive(libRecursionCheck, tarset, appObjectGuid, devid, ipreCompileContext);
					goto IL_2A4;
				}
				if (Debugger.IsAttached)
				{
					Debugger.Break();
					goto IL_2A4;
				}
				goto IL_2A4;
			}
			foreach (string text2 in precom.GetAllLocalLibraries().ToArray<string>())
			{
				if (text2 == null)
				{
					if (Debugger.IsAttached)
					{
						Debugger.Break();
					}
				}
				else
				{
					_IPreCompileContext libraryContext = APEnvironmentFacade.Instance.LanguageModelMgr.GetLibraryContext(text2);
					this.AddLibraryPlaceholderResolutionsRecursive(libRecursionCheck, tarset, appObjectGuid, devid, libraryContext);
				}
			}
		}

		// Token: 0x06000AB0 RID: 2736 RVA: 0x0001A01C File Offset: 0x0001901C
		public static string GetNameOfLibrary(_IPreCompileContext containerPrecom, _IPreCompileContext precom, ITargetSettings tarset, Guid appObjectGuid, IDeviceIdentification devid)
		{
			IList<_IPreCompileContext> list;
			ICaseInsensitiveDictionary<string> caseInsensitiveDictionary;
			containerPrecom.GetAllVisibleLibraries(out list, out caseInsensitiveDictionary, true);
			string result;
			if (caseInsensitiveDictionary.TryGetValue(precom.LibraryPath, out result))
			{
				return result;
			}
			foreach (_ILibraryPlaceholder ilibraryPlaceholder in containerPrecom.Placeholders)
			{
				if (containerPrecom.ResolveLibraryPlaceholder(tarset, appObjectGuid, ilibraryPlaceholder, devid) == precom)
				{
					return ilibraryPlaceholder.Namespace;
				}
			}
			return null;
		}

		// Token: 0x06000AB1 RID: 2737 RVA: 0x0001A080 File Offset: 0x00019080
		public static ICollection<IPreCompileContext> LibraryContextsWithResolvedPlaceholders(PreCompileContext targetPrecom, Guid guidApplication, bool bWithPublishedSymbols)
		{
			List<IPreCompileContext> list = new List<IPreCompileContext>();
			list.AddRange(targetPrecom.LibraryContexts);
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV3300)
			{
				CaseInsensitiveHashtable caseInsensitiveHashtable = new CaseInsensitiveHashtable();
				foreach (IPreCompileContext preCompileContext in list)
				{
					caseInsensitiveHashtable[preCompileContext.LibraryPath] = null;
				}
				foreach (_ILibraryPlaceholder placeholder in targetPrecom.Placeholders)
				{
					_IPreCompileContext ipreCompileContext = targetPrecom.ResolveLibraryPlaceholder(CompileContext.GetTargetSettings(guidApplication), guidApplication, placeholder, CompileContext.GetDeviceIdentification(guidApplication));
					if (ipreCompileContext != null)
					{
						if (!caseInsensitiveHashtable.Contains(ipreCompileContext.LibraryPath))
						{
							list.Add(ipreCompileContext);
						}
						if (bWithPublishedSymbols)
						{
							foreach (IPreCompileContext preCompileContext2 in ipreCompileContext.PublishedLibraries(guidApplication))
							{
								if (!caseInsensitiveHashtable.Contains(preCompileContext2.LibraryPath))
								{
									list.Add(preCompileContext2);
								}
							}
						}
					}
				}
			}
			return list;
		}

		// Token: 0x06000AB2 RID: 2738 RVA: 0x00004E6B File Offset: 0x00003E6B
		public uint CalculateChecksum(_IPreCompileContext precom)
		{
			return 0U;
		}

		// Token: 0x0400018B RID: 395
		private readonly CaseInsensitiveDictionary<string> _placeholderTable = new CaseInsensitiveDictionary<string>();

		// Token: 0x0400018C RID: 396
		private readonly CaseInsensitiveDictionary<LList<string>> _visibleLibrariesList = new CaseInsensitiveDictionary<LList<string>>();

		// Token: 0x0400018D RID: 397
		private LList<string> _applicationLibList = new LList<string>();

		// Token: 0x0400018E RID: 398
		private LList<string> _poolLibList = new LList<string>();

		// Token: 0x0400018F RID: 399
		private readonly CaseInsensitiveDictionary<CaseInsensitiveDictionary<string>> _visibleLibrariesNamespaces = new CaseInsensitiveDictionary<CaseInsensitiveDictionary<string>>();

		// Token: 0x04000190 RID: 400
		private CaseInsensitiveDictionary<string> _applicationLibNamespaces = new CaseInsensitiveDictionary<string>();

		// Token: 0x04000191 RID: 401
		private CaseInsensitiveDictionary<string> _poolLibNamespaces = new CaseInsensitiveDictionary<string>();

		// Token: 0x04000192 RID: 402
		private readonly CaseInsensitiveDictionary<CaseInsensitiveDictionary<bool>> _visibleLibrariesQOnly = new CaseInsensitiveDictionary<CaseInsensitiveDictionary<bool>>();

		// Token: 0x04000193 RID: 403
		private readonly CaseInsensitiveDictionary<bool> _applicationLibQOnly = new CaseInsensitiveDictionary<bool>();

		// Token: 0x04000194 RID: 404
		private readonly CaseInsensitiveDictionary<bool> _poolLibQOnly = new CaseInsensitiveDictionary<bool>();

		// Token: 0x04000195 RID: 405
		private readonly CaseInsensitiveHashtable _libraries32BitOnlyList = new CaseInsensitiveHashtable();

		// Token: 0x04000196 RID: 406
		private readonly CaseInsensitiveDictionary<CaseInsensitiveDictionary<string>> _namespaceToLibraries_Libs = new CaseInsensitiveDictionary<CaseInsensitiveDictionary<string>>();

		// Token: 0x04000197 RID: 407
		private readonly CaseInsensitiveDictionary<string> _namespaceToLibraries_App = new CaseInsensitiveDictionary<string>();

		// Token: 0x04000198 RID: 408
		private readonly CaseInsensitiveDictionary<string> _namespaceToLibraries_Pool = new CaseInsensitiveDictionary<string>();

		// Token: 0x04000199 RID: 409
		private readonly LDictionary<int, string> _htLibraryIdTable = new LDictionary<int, string>();

		// Token: 0x0400019A RID: 410
		private readonly CaseInsensitiveDictionary<int> _htIdLibraryTable = new CaseInsensitiveDictionary<int>();

		// Token: 0x0400019B RID: 411
		private readonly CaseInsensitiveDictionary<LDictionary<Version, Version>> _dicLibraryVersions = new CaseInsensitiveDictionary<LDictionary<Version, Version>>();

		// Token: 0x0400019C RID: 412
		private readonly CaseInsensitiveDictionary<string> _dicAllReferencedLibraries = new CaseInsensitiveDictionary<string>();

		// Token: 0x0400019D RID: 413
		private List<_ICompilerMessage> _messages;

		// Token: 0x0400019E RID: 414
		private readonly IdMan _libidman;
	}
}
