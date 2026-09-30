using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using _3S.CoDeSys.Compiler.Serialization;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Device;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.Core.TargetSettings;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.LanguageModelManager.LibraryTable;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x020000B8 RID: 184
	[TypeGuid("{D22AC189-3D1C-422D-B11C-0CA8BF22A579}")]
	[StorageVersion("3.5.13.0")]
	public class LibraryTableWithoutPlaceholders : GenericObject2, _ILibraryTable2, _ILibraryTable, ILibraryTableSerializable
	{
		// Token: 0x17000280 RID: 640
		// (get) Token: 0x06000ABC RID: 2748 RVA: 0x0001AB58 File Offset: 0x00019B58
		// (set) Token: 0x06000ABD RID: 2749 RVA: 0x0001AB6A File Offset: 0x00019B6A
		[DefaultSerialization("libinfosave")]
		[StorageVersion("3.5.13.0")]
		[StorageIgnorable]
		private LibInfoNew[] LibInfoSave
		{
			get
			{
				return this.LibInfoSerializable.Cast<LibInfoNew>().ToArray<LibInfoNew>();
			}
			set
			{
				this.LibInfoSerializable = value;
			}
		}

		// Token: 0x17000281 RID: 641
		// (get) Token: 0x06000ABE RID: 2750 RVA: 0x0001AB74 File Offset: 0x00019B74
		// (set) Token: 0x06000ABF RID: 2751 RVA: 0x0001AE18 File Offset: 0x00019E18
		public IEnumerable<ILibInfoSerializable> LibInfoSerializable
		{
			get
			{
				LList<LibInfoNew> llist = new LList<LibInfoNew>();
				foreach (_IPreCompileContext ipreCompileContext in this._applicationLibContextList)
				{
					string libraryPath = ipreCompileContext.LibraryPath;
					LibInfoNew libInfoNew = new LibInfoNew
					{
						bOutOfPool = false,
						strLibraryId = libraryPath,
						strNamespace = this._applicationLibNamespaces[libraryPath]
					};
					libInfoNew.nId = this._htIdLibraryTable[libInfoNew.strNamespace + "@" + libraryPath];
					libInfoNew.strReferencingLibrary = string.Empty;
					libInfoNew.qualifiedOnly = this._applicationLibQOnly[libraryPath];
					llist.Add(libInfoNew);
				}
				foreach (_IPreCompileContext ipreCompileContext2 in this._poolLibContextList)
				{
					string libraryPath2 = ipreCompileContext2.LibraryPath;
					LibInfoNew libInfoNew2 = new LibInfoNew
					{
						bOutOfPool = true,
						strLibraryId = libraryPath2,
						strNamespace = this._poolLibNamespaces[libraryPath2]
					};
					libInfoNew2.nId = this._htIdLibraryTable[libInfoNew2.strNamespace + "@" + libraryPath2];
					libInfoNew2.strReferencingLibrary = string.Empty;
					libInfoNew2.qualifiedOnly = this._applicationLibQOnly[libraryPath2];
					llist.Add(libInfoNew2);
				}
				foreach (string text in this._visibleLibrariesContextList.Keys)
				{
					foreach (_IPreCompileContext ipreCompileContext3 in this._visibleLibrariesContextList[text])
					{
						string libraryPath3 = ipreCompileContext3.LibraryPath;
						LibInfoNew libInfoNew3 = new LibInfoNew
						{
							bOutOfPool = false,
							strLibraryId = libraryPath3,
							strNamespace = this._visibleLibrariesNamespaces[text][libraryPath3]
						};
						libInfoNew3.nId = this._htIdLibraryTable[libInfoNew3.strNamespace + "@" + libraryPath3];
						libInfoNew3.strReferencingLibrary = text;
						libInfoNew3.qualifiedOnly = this._visibleLibrariesQOnly[text][libraryPath3];
						libInfoNew3.publishSymbols = this._visibleLibrariesPublishSymbols[text][libraryPath3];
						llist.Add(libInfoNew3);
					}
				}
				return llist;
			}
			set
			{
				foreach (ILibInfoSerializable libInfoSerializable in value)
				{
					LibInfoNew libInfoNew = (LibInfoNew)libInfoSerializable;
					if (libInfoNew.strLibraryId != string.Empty)
					{
						if (!this._visibleLibrariesContextList.ContainsKey(libInfoNew.strLibraryId))
						{
							this._visibleLibrariesContextList[libInfoNew.strLibraryId] = new LList<_IPreCompileContext>();
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
						if (!this._visibleLibrariesPublishSymbols.ContainsKey(libInfoNew.strLibraryId))
						{
							this._visibleLibrariesPublishSymbols[libInfoNew.strLibraryId] = new CaseInsensitiveDictionary<bool>();
						}
						this.AddLibraryInVersionFreeLibraryIdTable(libInfoNew.strLibraryId);
					}
					if (!this._htLibraryIdTable.ContainsKey(libInfoNew.nId))
					{
						this._htLibraryIdTable.Add(libInfoNew.nId, libInfoNew.strLibraryId);
						this._htIdLibraryTable.Add(libInfoNew.strNamespace + "@" + libInfoNew.strLibraryId, libInfoNew.nId);
					}
					_IPreCompileContext libraryContext = APEnvironmentFacade.Instance.LanguageModelMgr.LibList.GetLibraryContext(libInfoNew.strLibraryId);
					if (libraryContext != null)
					{
						if (libInfoNew.strReferencingLibrary != string.Empty)
						{
							if (!this._visibleLibrariesContextList.ContainsKey(libInfoNew.strReferencingLibrary))
							{
								this._visibleLibrariesContextList[libInfoNew.strReferencingLibrary] = new LList<_IPreCompileContext>();
							}
							if (!this._visibleLibrariesNamespaces.ContainsKey(libInfoNew.strReferencingLibrary))
							{
								this._visibleLibrariesNamespaces[libInfoNew.strReferencingLibrary] = new CaseInsensitiveDictionary<string>();
							}
							if (!this._visibleLibrariesQOnly.ContainsKey(libInfoNew.strReferencingLibrary))
							{
								this._visibleLibrariesQOnly[libInfoNew.strReferencingLibrary] = new CaseInsensitiveDictionary<bool>();
							}
							if (!this._visibleLibrariesPublishSymbols.ContainsKey(libInfoNew.strReferencingLibrary))
							{
								this._visibleLibrariesPublishSymbols[libInfoNew.strReferencingLibrary] = new CaseInsensitiveDictionary<bool>();
							}
							if (!this._namespaceToLibraries_Libs.ContainsKey(libInfoNew.strReferencingLibrary))
							{
								this._namespaceToLibraries_Libs[libInfoNew.strReferencingLibrary] = new CaseInsensitiveDictionary<string>();
							}
							this._visibleLibrariesContextList[libInfoNew.strReferencingLibrary].Add(libraryContext);
							this._visibleLibrariesNamespaces[libInfoNew.strReferencingLibrary][libInfoNew.strLibraryId] = libInfoNew.strNamespace;
							this._visibleLibrariesQOnly[libInfoNew.strReferencingLibrary][libInfoNew.strLibraryId] = libInfoNew.qualifiedOnly;
							this._visibleLibrariesPublishSymbols[libInfoNew.strReferencingLibrary][libInfoNew.strLibraryId] = libInfoNew.publishSymbols;
							this._namespaceToLibraries_Libs[libInfoNew.strReferencingLibrary][libInfoNew.strNamespace] = libInfoNew.strLibraryId;
						}
						else if (libInfoNew.bOutOfPool)
						{
							this._poolLibContextList.Add(libraryContext);
							this._sortedVisibleILibraries = null;
							this._poolLibNamespaces[libInfoNew.strLibraryId] = libInfoNew.strNamespace;
							this._applicationLibQOnly[libInfoNew.strLibraryId] = libInfoNew.qualifiedOnly;
							this._namespaceToLibraries_Pool[libInfoNew.strNamespace] = libInfoNew.strLibraryId;
						}
						else
						{
							this._applicationLibContextList.Add(libraryContext);
							this._sortedVisibleILibraries = null;
							this._applicationLibNamespaces[libInfoNew.strLibraryId] = libInfoNew.strNamespace;
							this._applicationLibQOnly[libInfoNew.strLibraryId] = libInfoNew.qualifiedOnly;
							this._namespaceToLibraries_App[libInfoNew.strNamespace] = libInfoNew.strLibraryId;
						}
					}
				}
			}
		}

		// Token: 0x17000282 RID: 642
		// (get) Token: 0x06000AC0 RID: 2752 RVA: 0x0001B210 File Offset: 0x0001A210
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

		// Token: 0x06000AC1 RID: 2753 RVA: 0x0001B228 File Offset: 0x0001A228
		public LibraryTableWithoutPlaceholders()
		{
		}

		// Token: 0x06000AC2 RID: 2754 RVA: 0x0001B304 File Offset: 0x0001A304
		public LibraryTableWithoutPlaceholders(CompileContext comcon, CompileContext comconRef)
		{
			this._libidman = comcon.LibraryIdManager;
			for (CompileContext compileContext = comcon; compileContext != null; compileContext = (compileContext.ParentContext as CompileContext))
			{
				_IPreCompileContext precom = APEnvironmentFacade.Instance.LanguageModelMgr._GetPrecompileContext(compileContext.ApplicationGuid);
				this.AddVisibleLibrariesRecursive(compileContext.ApplicationGuid, precom, compileContext, comconRef);
			}
		}

		// Token: 0x06000AC3 RID: 2755 RVA: 0x0001B424 File Offset: 0x0001A424
		public LibraryTableWithoutPlaceholders(Guid appGuid, _IPreCompileContext precom)
		{
			while (precom != null)
			{
				if (precom.ApplicationGuid == Guid.Empty && string.IsNullOrEmpty(precom.LibraryId))
				{
					precom = APEnvironmentFacade.Instance.LanguageModelMgr._GetPrecompileContext(appGuid);
				}
				if (precom == null)
				{
					break;
				}
				this.AddVisibleLibrariesRecursive(appGuid, precom);
				Guid parentApplicationGuid = APEnvironmentFacade.Instance.LMServiceProvider.LanguageModelProviderService.GetParentApplicationGuid(precom.ApplicationGuid);
				if (!(parentApplicationGuid != Guid.Empty))
				{
					break;
				}
				precom = APEnvironmentFacade.Instance.LanguageModelMgr._GetPrecompileContext(parentApplicationGuid);
			}
		}

		// Token: 0x06000AC4 RID: 2756 RVA: 0x0001B578 File Offset: 0x0001A578
		public IList<string> AllReferencedLibraries()
		{
			return new LList<string>(this._dicAllReferencedLibraries.Keys);
		}

		// Token: 0x06000AC5 RID: 2757 RVA: 0x0001B58C File Offset: 0x0001A58C
		public bool LibraryIsUnique(string stLibraryPath)
		{
			string text = LibraryHelper.VersionFreeLibraryPath(stLibraryPath);
			LDictionary<Version, Version> ldictionary;
			return this._dicLibraryVersions.TryGetValue(text, ref ldictionary) && ldictionary.Count == 1;
		}

		// Token: 0x06000AC6 RID: 2758 RVA: 0x00005F0F File Offset: 0x00004F0F
		public string GetLibraryOfPlaceholder(string stPlaceholder)
		{
			return null;
		}

		// Token: 0x06000AC7 RID: 2759 RVA: 0x0001B5BC File Offset: 0x0001A5BC
		public string VersionFreeLibraryPath(string stLibraryPath)
		{
			string result;
			if (this._dicAllReferencedLibraries.TryGetValue(stLibraryPath, ref result))
			{
				return result;
			}
			return stLibraryPath;
		}

		// Token: 0x06000AC8 RID: 2760 RVA: 0x0001B5DC File Offset: 0x0001A5DC
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
				if (!this._dicLibraryVersions.TryGetValue(text, ref ldictionary))
				{
					ldictionary = new LDictionary<Version, Version>();
					this._dicLibraryVersions[text] = ldictionary;
				}
				ldictionary[version] = version;
			}
		}

		// Token: 0x06000AC9 RID: 2761 RVA: 0x0001B63C File Offset: 0x0001A63C
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

		// Token: 0x06000ACA RID: 2762 RVA: 0x0001B6C4 File Offset: 0x0001A6C4
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

		// Token: 0x06000ACB RID: 2763 RVA: 0x0001B6F8 File Offset: 0x0001A6F8
		public string GetLibraryById(int nId)
		{
			string result;
			if (this._htLibraryIdTable.TryGetValue(nId, ref result))
			{
				return result;
			}
			return null;
		}

		// Token: 0x06000ACC RID: 2764 RVA: 0x0001B718 File Offset: 0x0001A718
		public IList<IPreCompileContext> GetLocalVisibleILibraries()
		{
			LList<IPreCompileContext> llist = new LList<IPreCompileContext>();
			foreach (_IPreCompileContext ipreCompileContext in this._applicationLibContextList)
			{
				llist.Add(ipreCompileContext);
			}
			return llist;
		}

		// Token: 0x06000ACD RID: 2765 RVA: 0x0001B76C File Offset: 0x0001A76C
		private _IPreCompileContext[] CollectVisibleILibrariesSorted(string stLibraryId)
		{
			if (string.IsNullOrEmpty(stLibraryId))
			{
				_IPreCompileContext[] result;
				if ((result = this._sortedVisibleILibraries) == null)
				{
					result = (this._sortedVisibleILibraries = (from l in this._applicationLibContextList.Concat(this._poolLibContextList)
					where l != null
					select l).Distinct<_IPreCompileContext>().OrderBy((_IPreCompileContext pcc) => pcc.LibraryId, StringComparer.Ordinal).ToArray<_IPreCompileContext>());
				}
				return result;
			}
			LList<_IPreCompileContext> source;
			if (this._visibleLibrariesContextList.TryGetValue(stLibraryId, ref source))
			{
				return (from l in source
				where l != null
				select l).Distinct<_IPreCompileContext>().OrderBy((_IPreCompileContext pcc) => pcc.LibraryId, StringComparer.Ordinal).ToArray<_IPreCompileContext>();
			}
			return Array.Empty<_IPreCompileContext>();
		}

		// Token: 0x06000ACE RID: 2766 RVA: 0x0001B86C File Offset: 0x0001A86C
		private IEnumerable<IPreCompileContext> CollectVisibleILibraries(string stLibraryId)
		{
			LHashSet<IPreCompileContext> lhashSet = new LHashSet<IPreCompileContext>();
			if (string.IsNullOrEmpty(stLibraryId))
			{
				foreach (_IPreCompileContext ipreCompileContext in this._applicationLibContextList)
				{
					lhashSet.Add(ipreCompileContext);
				}
				using (IEnumerator<_IPreCompileContext> enumerator = this._poolLibContextList.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						_IPreCompileContext ipreCompileContext2 = enumerator.Current;
						lhashSet.Add(ipreCompileContext2);
					}
					goto IL_AF;
				}
			}
			LList<_IPreCompileContext> llist;
			if (this._visibleLibrariesContextList.TryGetValue(stLibraryId, ref llist))
			{
				foreach (_IPreCompileContext ipreCompileContext3 in llist)
				{
					lhashSet.Add(ipreCompileContext3);
				}
			}
			IL_AF:
			lhashSet.Remove(null);
			return lhashSet;
		}

		// Token: 0x06000ACF RID: 2767 RVA: 0x0001B95C File Offset: 0x0001A95C
		public IEnumerable<IPreCompileContext> GetVisibleILibraries(string stLibraryId)
		{
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV352200)
			{
				return this.CollectVisibleILibrariesSorted(stLibraryId);
			}
			return this.CollectVisibleILibraries(stLibraryId);
		}

		// Token: 0x06000AD0 RID: 2768 RVA: 0x0001B980 File Offset: 0x0001A980
		public IList<IPreCompileContext> GetVisibleILibraries(_IPreCompileContext precom)
		{
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV352200 && (!(precom.ApplicationGuid == Guid.Empty) || !string.IsNullOrEmpty(precom.LibraryId)))
			{
				return this.CollectVisibleILibrariesSorted(precom.LibraryId);
			}
			return this.GetVisibleLibraryPaths(precom).ConvertAll<IPreCompileContext>((_IPreCompileContext item) => item);
		}

		// Token: 0x06000AD1 RID: 2769 RVA: 0x0001B9F5 File Offset: 0x0001A9F5
		public IList<_IPreCompileContext> GetVisibleLibraries(_IPreCompileContext precom)
		{
			return Enumerable.ToReadonlyList<_IPreCompileContext>(this.GetVisibleLibraryPaths(precom));
		}

		// Token: 0x06000AD2 RID: 2770 RVA: 0x0001BA04 File Offset: 0x0001AA04
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

		// Token: 0x06000AD3 RID: 2771 RVA: 0x0001BA54 File Offset: 0x0001AA54
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
			foreach (IPreCompileContext preCompileContext in this.GetVisibleILibraries(precomLocal))
			{
				string localLibraryNamespaceRecursiveExtChecked = this.GetLocalLibraryNamespaceRecursiveExtChecked(preCompileContext as _IPreCompileContext, stLibraryToFind, recursionTest);
				if (localLibraryNamespaceRecursiveExtChecked != null)
				{
					string namespaceOfLibrary2 = this.GetNamespaceOfLibrary(precomLocal, preCompileContext.LibraryPath);
					if (namespaceOfLibrary2 == null)
					{
						return null;
					}
					return namespaceOfLibrary2 + "." + localLibraryNamespaceRecursiveExtChecked;
				}
			}
			return null;
		}

		// Token: 0x06000AD4 RID: 2772 RVA: 0x0001BAFC File Offset: 0x0001AAFC
		public string GetLocalLibraryNamespaceRecursiveExt(_IPreCompileContext precomLocal, string stLibraryToFind)
		{
			return this.GetLocalLibraryNamespaceRecursiveExtChecked(precomLocal, stLibraryToFind, new Dictionary<_IPreCompileContext, _IPreCompileContext>());
		}

		// Token: 0x06000AD5 RID: 2773 RVA: 0x0001BB0C File Offset: 0x0001AB0C
		public string GetLocalLibraryNamespaceRecursive(_IPreCompileContext precomLocal, string stLibraryToFind)
		{
			string namespaceOfLibrary = this.GetNamespaceOfLibrary(precomLocal, stLibraryToFind);
			if (namespaceOfLibrary != null)
			{
				return namespaceOfLibrary;
			}
			foreach (IPreCompileContext preCompileContext in this.GetVisibleILibraries(precomLocal))
			{
				_IPreCompileContext ipreCompileContext = (_IPreCompileContext)preCompileContext;
				string localLibraryNamespaceRecursive = this.GetLocalLibraryNamespaceRecursive(precomLocal, ipreCompileContext, new Dictionary<_IPreCompileContext, _IPreCompileContext>());
				if (localLibraryNamespaceRecursive != null)
				{
					return this.GetNamespaceOfLibrary(precomLocal, ipreCompileContext.LibraryId) + "." + localLibraryNamespaceRecursive;
				}
			}
			return null;
		}

		// Token: 0x06000AD6 RID: 2774 RVA: 0x0001BB98 File Offset: 0x0001AB98
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

		// Token: 0x06000AD7 RID: 2775 RVA: 0x0001BC38 File Offset: 0x0001AC38
		private bool? GetQualifiedOnlyRecursiveChecked(_IPreCompileContext precomToSearchIn, string stLibraryIdToLookup, Dictionary<_IPreCompileContext, _IPreCompileContext> recursionTest)
		{
			if (recursionTest.ContainsKey(precomToSearchIn))
			{
				return null;
			}
			recursionTest.Add(precomToSearchIn, precomToSearchIn);
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
					return new bool?(this.GetQualifiedOnlyExt(precomToSearchIn, ipreCompileContext.LibraryPath).GetValueOrDefault());
				}
			}
			return null;
		}

		// Token: 0x06000AD8 RID: 2776 RVA: 0x0001BD08 File Offset: 0x0001AD08
		public bool? GetQualifiedOnlyRecursive(_IPreCompileContext precomLocal, string stLibraryIdToLookup)
		{
			Dictionary<_IPreCompileContext, _IPreCompileContext> recursionTest = new Dictionary<_IPreCompileContext, _IPreCompileContext>();
			return this.GetQualifiedOnlyRecursiveChecked(precomLocal, stLibraryIdToLookup, recursionTest);
		}

		// Token: 0x06000AD9 RID: 2777 RVA: 0x0001BD24 File Offset: 0x0001AD24
		public string GetNamespaceOfLocalLibrary(string stLibraryIdToLookup)
		{
			string result;
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

		// Token: 0x06000ADA RID: 2778 RVA: 0x0001BD58 File Offset: 0x0001AD58
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
				flag = this._applicationLibQOnly.TryGetValue(stLibraryIdToLookup, ref value);
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

		// Token: 0x06000ADB RID: 2779 RVA: 0x0001BDDC File Offset: 0x0001ADDC
		public bool GetQualifiedOnly(_IPreCompileContext precomLocal, string stLibraryIdToLookup)
		{
			bool? qualifiedOnlyExt = this.GetQualifiedOnlyExt(precomLocal, stLibraryIdToLookup);
			return qualifiedOnlyExt != null && qualifiedOnlyExt.Value;
		}

		// Token: 0x06000ADC RID: 2780 RVA: 0x0001BE04 File Offset: 0x0001AE04
		public IEnumerable Get32BitOnly()
		{
			return this._libraries32BitOnlyList.Values;
		}

		// Token: 0x06000ADD RID: 2781 RVA: 0x0001BE14 File Offset: 0x0001AE14
		public bool GetPublishSymbols(_IPreCompileContext precomLocal, string stLibraryIdToLookup)
		{
			bool result = false;
			bool flag = false;
			CaseInsensitiveDictionary<bool> caseInsensitiveDictionary;
			if (precomLocal.ApplicationGuid != Guid.Empty)
			{
				flag = this._applicationLibPublishSymbols.TryGetValue(stLibraryIdToLookup, ref result);
			}
			else if (string.IsNullOrEmpty(precomLocal.LibraryPath))
			{
				flag = this._applicationLibPublishSymbols.TryGetValue(stLibraryIdToLookup, ref result);
			}
			else if (this._visibleLibrariesPublishSymbols.TryGetValue(precomLocal.LibraryPath, ref caseInsensitiveDictionary))
			{
				flag = caseInsensitiveDictionary.TryGetValue(stLibraryIdToLookup, ref result);
			}
			if (flag && APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV352200)
			{
				return result;
			}
			return flag;
		}

		// Token: 0x06000ADE RID: 2782 RVA: 0x0001BE9C File Offset: 0x0001AE9C
		public string GetNamespaceOfLibrary(_IPreCompileContext precomLocal, string stLibraryIdToLookup)
		{
			string result = null;
			bool flag = false;
			CaseInsensitiveDictionary<string> caseInsensitiveDictionary;
			if (precomLocal.ApplicationGuid != Guid.Empty)
			{
				flag = this._applicationLibNamespaces.TryGetValue(stLibraryIdToLookup, ref result);
				if (!flag)
				{
					flag = this._poolLibNamespaces.TryGetValue(stLibraryIdToLookup, ref result);
				}
			}
			else if (string.IsNullOrEmpty(precomLocal.LibraryPath))
			{
				flag = this._poolLibNamespaces.TryGetValue(stLibraryIdToLookup, ref result);
			}
			else if (this._visibleLibrariesNamespaces.TryGetValue(precomLocal.LibraryPath, ref caseInsensitiveDictionary))
			{
				flag = caseInsensitiveDictionary.TryGetValue(stLibraryIdToLookup, ref result);
			}
			if (!flag)
			{
				return null;
			}
			return result;
		}

		// Token: 0x06000ADF RID: 2783 RVA: 0x0001BF28 File Offset: 0x0001AF28
		public _IPreCompileContext GetLibraryContextByNamespace(string stNamespace)
		{
			string libraryIdByNamespace = this.GetLibraryIdByNamespace(stNamespace);
			if (!string.IsNullOrEmpty(libraryIdByNamespace))
			{
				return APEnvironmentFacade.Instance.LanguageModelMgr.GetLibraryContext(libraryIdByNamespace);
			}
			return null;
		}

		// Token: 0x06000AE0 RID: 2784 RVA: 0x0001BF58 File Offset: 0x0001AF58
		public _IPreCompileContext GetLibraryContextByNamespace(string stNamespace, _IPreCompileContext precomLocal)
		{
			string libraryIdByNamespace = this.GetLibraryIdByNamespace(stNamespace, precomLocal);
			if (!string.IsNullOrEmpty(libraryIdByNamespace))
			{
				return APEnvironmentFacade.Instance.LanguageModelMgr.GetLibraryContext(libraryIdByNamespace);
			}
			return null;
		}

		// Token: 0x06000AE1 RID: 2785 RVA: 0x0001BF88 File Offset: 0x0001AF88
		public string GetLibraryIdByNamespace(string stNamespace)
		{
			string result;
			bool flag = this._namespaceToLibraries_App.TryGetValue(stNamespace, ref result);
			if (!flag)
			{
				flag = this._namespaceToLibraries_Pool.TryGetValue(stNamespace, ref result);
			}
			if (!flag)
			{
				return null;
			}
			return result;
		}

		// Token: 0x06000AE2 RID: 2786 RVA: 0x0001BFBC File Offset: 0x0001AFBC
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
			if (!flag)
			{
				return null;
			}
			return result;
		}

		// Token: 0x06000AE3 RID: 2787 RVA: 0x0001C034 File Offset: 0x0001B034
		private LList<_IPreCompileContext> GetVisibleLibraryPaths(_IPreCompileContext precom)
		{
			if (precom.ApplicationGuid != Guid.Empty)
			{
				return this._applicationLibContextList;
			}
			if (string.IsNullOrEmpty(precom.LibraryPath))
			{
				return this._poolLibContextList;
			}
			LList<_IPreCompileContext> result;
			if (this._visibleLibrariesContextList.TryGetValue(precom.LibraryPath, ref result))
			{
				return result;
			}
			return new LList<_IPreCompileContext>(0);
		}

		// Token: 0x06000AE4 RID: 2788 RVA: 0x0001C08C File Offset: 0x0001B08C
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

		// Token: 0x06000AE5 RID: 2789 RVA: 0x0001C0D5 File Offset: 0x0001B0D5
		private void AddVisibleLibrariesRecursive(Guid appGuid, _IPreCompileContext precom)
		{
			this.AddVisibleLibrariesRecursive(appGuid, precom, null, null);
		}

		// Token: 0x06000AE6 RID: 2790 RVA: 0x0001C0E4 File Offset: 0x0001B0E4
		private void AddVisibleLibrariesRecursive(Guid appGuid, _IPreCompileContext precom, CompileContext comcon, CompileContext comconRef)
		{
			if (precom == null)
			{
				return;
			}
			LList<ILMLibraryInfo> llist = new LList<ILMLibraryInfo>();
			IVisibleLibrariesCollector visibleLibrariesCollector;
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351500)
			{
				visibleLibrariesCollector = new VisibleLibrariesCollector();
			}
			else
			{
				visibleLibrariesCollector = new IVisibleLibrariesCollectorBeforeV351500();
			}
			visibleLibrariesCollector.GetAllVisibleLibraries(appGuid, precom, llist);
			IEnumerable<ILMLibraryInfo> enumerable = (from libinfo in llist
			where libinfo is ILMLibraryInfo5 && !((ILMLibraryInfo5)libinfo).UnresolvedReference
			select libinfo).ToArray<ILMLibraryInfo>();
			CaseInsensitiveDictionary<bool> qonlyTable;
			CaseInsensitiveDictionary<bool> publishSymbolTable;
			if (string.IsNullOrEmpty(precom.LibraryPath))
			{
				foreach (ILMLibraryInfo ilmlibraryInfo in enumerable)
				{
					_IPreCompileContext libraryContext = APEnvironmentFacade.Instance.LanguageModelMgr.GetLibraryContext(ilmlibraryInfo.Identification);
					if (libraryContext != null)
					{
						ILMLibraryInfo5 ilmlibraryInfo2 = ilmlibraryInfo as ILMLibraryInfo5;
						if (ilmlibraryInfo2 != null && (ilmlibraryInfo2.PoolLibrary || precom.ApplicationGuid == Guid.Empty))
						{
							if (!this._poolLibContextList.Contains(libraryContext))
							{
								this._poolLibContextList.Add(libraryContext);
								this._sortedVisibleILibraries = null;
							}
							if (!this._poolLibNamespaces.ContainsKey(ilmlibraryInfo2.Identification))
							{
								this._poolLibNamespaces.Add(ilmlibraryInfo2.Identification, ilmlibraryInfo2.Namespace);
							}
							if (!this._namespaceToLibraries_Pool.ContainsKey(ilmlibraryInfo2.Namespace))
							{
								this._namespaceToLibraries_Pool[ilmlibraryInfo2.Namespace] = ilmlibraryInfo2.Identification;
							}
						}
						else
						{
							if (!this._applicationLibContextList.Contains(libraryContext))
							{
								this._applicationLibContextList.Add(libraryContext);
								this._sortedVisibleILibraries = null;
							}
							if (!this._applicationLibNamespaces.ContainsKey(ilmlibraryInfo.Identification))
							{
								this._applicationLibNamespaces.Add(ilmlibraryInfo.Identification, ilmlibraryInfo.Namespace);
							}
							if (!this._namespaceToLibraries_App.ContainsKey(ilmlibraryInfo.Namespace))
							{
								this._namespaceToLibraries_App[ilmlibraryInfo.Namespace] = ilmlibraryInfo.Identification;
							}
						}
					}
				}
				qonlyTable = this._applicationLibQOnly;
				publishSymbolTable = this._applicationLibPublishSymbols;
			}
			else
			{
				qonlyTable = LibraryTableWithoutPlaceholders.GetOrCreateTable(this._visibleLibrariesQOnly, precom);
				publishSymbolTable = LibraryTableWithoutPlaceholders.GetOrCreateTable(this._visibleLibrariesPublishSymbols, precom);
				this._visibleLibrariesContextList[precom.LibraryPath] = new LList<_IPreCompileContext>();
				CaseInsensitiveDictionary<string> caseInsensitiveDictionary = new CaseInsensitiveDictionary<string>();
				CaseInsensitiveDictionary<string> caseInsensitiveDictionary2 = new CaseInsensitiveDictionary<string>();
				bool greaterEqualV = APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351600;
				foreach (ILMLibraryInfo ilmlibraryInfo3 in enumerable)
				{
					if (!caseInsensitiveDictionary.ContainsKey(ilmlibraryInfo3.Identification))
					{
						_IPreCompileContext libraryContext2 = APEnvironmentFacade.Instance.LanguageModelMgr.GetLibraryContext(ilmlibraryInfo3.Identification);
						if (libraryContext2 != null)
						{
							this._visibleLibrariesContextList[precom.LibraryPath].Add(libraryContext2);
						}
						caseInsensitiveDictionary.Add(ilmlibraryInfo3.Identification, ilmlibraryInfo3.Namespace);
						string @namespace = ilmlibraryInfo3.Namespace;
						if (!caseInsensitiveDictionary2.ContainsKey(@namespace))
						{
							caseInsensitiveDictionary2[@namespace] = ilmlibraryInfo3.Identification;
						}
						bool flag = !greaterEqualV && precom.Namespace == @namespace;
						if (flag)
						{
							this.AddLibraryConflictMessage(@namespace);
						}
					}
				}
				this._namespaceToLibraries_Libs[precom.LibraryPath] = caseInsensitiveDictionary2;
				this._visibleLibrariesNamespaces[precom.LibraryPath] = caseInsensitiveDictionary;
			}
			if (precom.Support32BitOnly && !this._libraries32BitOnlyList.ContainsKey(precom.LibraryId))
			{
				this._libraries32BitOnlyList[precom.LibraryId] = precom;
			}
			this.StoreLibraryPropertiesInLUTs(appGuid, comcon, comconRef, enumerable, qonlyTable, publishSymbolTable);
		}

		// Token: 0x06000AE7 RID: 2791 RVA: 0x0001C4A8 File Offset: 0x0001B4A8
		private void StoreLibraryPropertiesInLUTs(Guid appGuid, CompileContext comcon, CompileContext comconRef, IEnumerable<ILMLibraryInfo> liblist, IDictionary<string, bool> QOnlyTable, IDictionary<string, bool> PublishSymbolTable)
		{
			foreach (ILMLibraryInfo ilmlibraryInfo in liblist)
			{
				string @namespace = ilmlibraryInfo.Namespace;
				string identification = ilmlibraryInfo.Identification;
				this.AddLibraryInVersionFreeLibraryIdTable(identification);
				if (comcon != null)
				{
					this.AddLibraryInIdTable(comcon, comconRef, identification, @namespace);
				}
				_IPreCompileContext libraryContext = APEnvironmentFacade.Instance.LanguageModelMgr.GetLibraryContext(identification);
				if (libraryContext != null)
				{
					if (!this._visibleLibrariesContextList.ContainsKey(identification))
					{
						this.AddVisibleLibrariesRecursive(appGuid, libraryContext, comcon, comconRef);
					}
					if (!QOnlyTable.ContainsKey(identification))
					{
						QOnlyTable.Add(identification, ((ILMLibraryInfo2)ilmlibraryInfo).QualifiedOnlyLocal || ilmlibraryInfo.QualifiedOnly);
					}
					if (!PublishSymbolTable.ContainsKey(identification))
					{
						PublishSymbolTable.Add(identification, ilmlibraryInfo.PublishSymbols);
					}
					if (libraryContext.Support32BitOnly && !this._libraries32BitOnlyList.ContainsKey(libraryContext.LibraryId))
					{
						this._libraries32BitOnlyList[libraryContext.LibraryId] = libraryContext;
					}
				}
			}
		}

		// Token: 0x06000AE8 RID: 2792 RVA: 0x0001C5B8 File Offset: 0x0001B5B8
		private static CaseInsensitiveDictionary<bool> GetOrCreateTable(IDictionary<string, CaseInsensitiveDictionary<bool>> visibleLibraries, _IPreCompileContext precom)
		{
			CaseInsensitiveDictionary<bool> caseInsensitiveDictionary;
			if (!visibleLibraries.TryGetValue(precom.LibraryPath, out caseInsensitiveDictionary))
			{
				caseInsensitiveDictionary = new CaseInsensitiveDictionary<bool>();
				visibleLibraries.Add(precom.LibraryPath, caseInsensitiveDictionary);
			}
			return caseInsensitiveDictionary;
		}

		// Token: 0x06000AE9 RID: 2793 RVA: 0x0001C5E9 File Offset: 0x0001B5E9
		public static string GetNameOfLibrary(_IPreCompileContext containerPrecom, _IPreCompileContext precom, ITargetSettings tarset, Guid appObjectGuid, IDeviceIdentification devid)
		{
			return containerPrecom._GetLibraryTable(appObjectGuid).GetNamespaceOfLibrary(containerPrecom, precom.LibraryId);
		}

		// Token: 0x06000AEA RID: 2794 RVA: 0x0001C5FE File Offset: 0x0001B5FE
		public static ICollection<IPreCompileContext> LibraryContextsWithResolvedPlaceholders(PreCompileContext targetPrecom, Guid guidApplication, bool bWithPublishedSymbols)
		{
			return new LList<IPreCompileContext>(targetPrecom._GetLibraryTable(guidApplication).GetVisibleLibraries(targetPrecom));
		}

		// Token: 0x06000AEB RID: 2795 RVA: 0x0001C614 File Offset: 0x0001B614
		public uint CalculateChecksum(_IPreCompileContext precom)
		{
			ChecksumStream checksumStream = CompilerProxy.CreateChecksumStream(true);
			BinaryWriter binaryWriter = new BinaryWriter(checksumStream);
			SortedDictionary<string, string> sortedDictionary = new SortedDictionary<string, string>();
			foreach (string text in this.AllReferencedLibraries())
			{
				string text2 = text.ToUpperInvariant();
				if (!sortedDictionary.ContainsKey(text2))
				{
					sortedDictionary.Add(text2, text2);
				}
			}
			foreach (string text3 in sortedDictionary.Keys)
			{
				string value = text3;
				_IPreCompileContext libraryContext = APEnvironmentFacade.Instance.LanguageModelMgr.GetLibraryContext(text3);
				if (libraryContext != null)
				{
					value = libraryContext.LibraryId.ToUpperInvariant();
				}
				binaryWriter.Write(value);
			}
			binaryWriter.Flush();
			checksumStream.Close();
			return checksumStream.Checksum;
		}

		// Token: 0x040001A8 RID: 424
		private readonly CaseInsensitiveDictionary<LList<_IPreCompileContext>> _visibleLibrariesContextList = new CaseInsensitiveDictionary<LList<_IPreCompileContext>>();

		// Token: 0x040001A9 RID: 425
		private readonly LList<_IPreCompileContext> _applicationLibContextList = new LList<_IPreCompileContext>();

		// Token: 0x040001AA RID: 426
		private readonly LList<_IPreCompileContext> _poolLibContextList = new LList<_IPreCompileContext>();

		// Token: 0x040001AB RID: 427
		private _IPreCompileContext[] _sortedVisibleILibraries;

		// Token: 0x040001AC RID: 428
		private readonly CaseInsensitiveDictionary<CaseInsensitiveDictionary<string>> _visibleLibrariesNamespaces = new CaseInsensitiveDictionary<CaseInsensitiveDictionary<string>>();

		// Token: 0x040001AD RID: 429
		private readonly CaseInsensitiveDictionary<string> _applicationLibNamespaces = new CaseInsensitiveDictionary<string>();

		// Token: 0x040001AE RID: 430
		private readonly CaseInsensitiveDictionary<string> _poolLibNamespaces = new CaseInsensitiveDictionary<string>();

		// Token: 0x040001AF RID: 431
		private readonly CaseInsensitiveDictionary<CaseInsensitiveDictionary<bool>> _visibleLibrariesQOnly = new CaseInsensitiveDictionary<CaseInsensitiveDictionary<bool>>();

		// Token: 0x040001B0 RID: 432
		private readonly CaseInsensitiveDictionary<bool> _applicationLibQOnly = new CaseInsensitiveDictionary<bool>();

		// Token: 0x040001B1 RID: 433
		private readonly CaseInsensitiveDictionary<CaseInsensitiveDictionary<bool>> _visibleLibrariesPublishSymbols = new CaseInsensitiveDictionary<CaseInsensitiveDictionary<bool>>();

		// Token: 0x040001B2 RID: 434
		private readonly CaseInsensitiveDictionary<bool> _applicationLibPublishSymbols = new CaseInsensitiveDictionary<bool>();

		// Token: 0x040001B3 RID: 435
		private readonly CaseInsensitiveHashtable _libraries32BitOnlyList = new CaseInsensitiveHashtable();

		// Token: 0x040001B4 RID: 436
		private readonly CaseInsensitiveDictionary<CaseInsensitiveDictionary<string>> _namespaceToLibraries_Libs = new CaseInsensitiveDictionary<CaseInsensitiveDictionary<string>>();

		// Token: 0x040001B5 RID: 437
		private readonly CaseInsensitiveDictionary<string> _namespaceToLibraries_App = new CaseInsensitiveDictionary<string>();

		// Token: 0x040001B6 RID: 438
		private readonly CaseInsensitiveDictionary<string> _namespaceToLibraries_Pool = new CaseInsensitiveDictionary<string>();

		// Token: 0x040001B7 RID: 439
		private readonly LDictionary<int, string> _htLibraryIdTable = new LDictionary<int, string>();

		// Token: 0x040001B8 RID: 440
		private readonly CaseInsensitiveDictionary<int> _htIdLibraryTable = new CaseInsensitiveDictionary<int>();

		// Token: 0x040001B9 RID: 441
		private readonly CaseInsensitiveDictionary<LDictionary<Version, Version>> _dicLibraryVersions = new CaseInsensitiveDictionary<LDictionary<Version, Version>>();

		// Token: 0x040001BA RID: 442
		private readonly CaseInsensitiveDictionary<string> _dicAllReferencedLibraries = new CaseInsensitiveDictionary<string>();

		// Token: 0x040001BB RID: 443
		private List<_ICompilerMessage> _messages;

		// Token: 0x040001BC RID: 444
		private readonly IdMan _libidman;
	}
}
