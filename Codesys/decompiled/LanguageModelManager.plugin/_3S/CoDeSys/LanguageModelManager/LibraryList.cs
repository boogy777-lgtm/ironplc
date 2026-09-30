using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000124 RID: 292
	[TypeGuid("{b867b171-9196-46fe-acae-66578e50d07c}")]
	[StorageVersion("3.3.0.0")]
	public class LibraryList : GenericObject2, _ILibraryList
	{
		// Token: 0x06001900 RID: 6400 RVA: 0x00048470 File Offset: 0x00047470
		internal void Clear()
		{
			this.m_htLibraryReferences.Clear();
			CaseInsensitiveDictionary<_IPreCompileContext> htLibraries = this.m_htLibraries;
			lock (htLibraries)
			{
				this.m_htLibraries.Clear();
			}
		}

		// Token: 0x17000616 RID: 1558
		// (get) Token: 0x06001901 RID: 6401 RVA: 0x000484C0 File Offset: 0x000474C0
		public _IPreCompileContext[] AllLibraryContexts
		{
			get
			{
				CaseInsensitiveDictionary<_IPreCompileContext> htLibraries = this.m_htLibraries;
				_IPreCompileContext[] result;
				lock (htLibraries)
				{
					result = this.m_htLibraries.Values.ToArray<_IPreCompileContext>();
				}
				return result;
			}
		}

		// Token: 0x17000617 RID: 1559
		public _IPreCompileContext this[string stId]
		{
			get
			{
				CaseInsensitiveDictionary<_IPreCompileContext> htLibraries = this.m_htLibraries;
				_IPreCompileContext result;
				lock (htLibraries)
				{
					_IPreCompileContext ipreCompileContext;
					if (this.m_htLibraries.TryGetValue(stId, ref ipreCompileContext))
					{
						result = (ipreCompileContext as PreCompileContext);
					}
					else
					{
						result = null;
					}
				}
				return result;
			}
			set
			{
				CaseInsensitiveDictionary<_IPreCompileContext> htLibraries = this.m_htLibraries;
				lock (htLibraries)
				{
					this.m_htLibraries[stId] = value;
				}
			}
		}

		// Token: 0x06001904 RID: 6404 RVA: 0x000485AC File Offset: 0x000475AC
		public string[] GetReferencedLibraries(Guid guidLibMan)
		{
			LList<string> llist = new LList<string>();
			foreach (object obj in this.m_htLibraryReferences.Keys)
			{
				string text = (string)obj;
				if ((this.m_htLibraryReferences[text] as ArrayList).Contains(guidLibMan))
				{
					llist.Add(text);
				}
			}
			return llist.ToArray();
		}

		// Token: 0x06001905 RID: 6405 RVA: 0x00048634 File Offset: 0x00047634
		public Guid[] GetLibraryReferences(string stId)
		{
			ArrayList arrayList = this.m_htLibraryReferences[stId] as ArrayList;
			if (arrayList == null)
			{
				return Array.Empty<Guid>();
			}
			Guid[] array = new Guid[arrayList.Count];
			arrayList.CopyTo(array);
			return array;
		}

		// Token: 0x06001906 RID: 6406 RVA: 0x00048670 File Offset: 0x00047670
		public void RemoveLibraryReferences(Guid libmanGuid)
		{
			string[] array = new string[this.m_htLibraryReferences.Keys.Count];
			this.m_htLibraryReferences.Keys.CopyTo(array, 0);
			foreach (string key in array)
			{
				(this.m_htLibraryReferences[key] as ArrayList).Remove(libmanGuid);
			}
		}

		// Token: 0x06001907 RID: 6407 RVA: 0x000486D8 File Offset: 0x000476D8
		public void RemoveLibrary(string stId)
		{
			CaseInsensitiveDictionary<_IPreCompileContext> htLibraries = this.m_htLibraries;
			lock (htLibraries)
			{
				this.m_htLibraries.Remove(stId);
			}
			this.m_htLibraryReferences.Remove(stId);
		}

		// Token: 0x06001908 RID: 6408 RVA: 0x0004872C File Offset: 0x0004772C
		public void AddLibraryReference(Guid guidLibman, string stId)
		{
			ArrayList arrayList = this.m_htLibraryReferences[stId] as ArrayList;
			if (arrayList == null)
			{
				arrayList = new ArrayList(1);
				this.m_htLibraryReferences[stId] = arrayList;
			}
			arrayList.Add(guidLibman);
		}

		// Token: 0x06001909 RID: 6409 RVA: 0x00048770 File Offset: 0x00047770
		public void CheckForUnreferencedLibraries()
		{
			CaseInsensitiveDictionary<_IPreCompileContext> htLibraries = this.m_htLibraries;
			lock (htLibraries)
			{
				string[] array = new string[this.m_htLibraries.Keys.Count];
				this.m_htLibraries.Keys.CopyTo(array, 0);
				foreach (string text in array)
				{
					ArrayList arrayList = this.m_htLibraryReferences[text] as ArrayList;
					if (arrayList == null || arrayList.Count == 0)
					{
						this.m_htLibraries.Remove(text);
					}
				}
			}
		}

		// Token: 0x0600190A RID: 6410 RVA: 0x0004881C File Offset: 0x0004781C
		public _IPreCompileContext GetLibraryContext(string stLibraryId)
		{
			return this.GetLibraryContext(stLibraryId, false);
		}

		// Token: 0x0600190B RID: 6411 RVA: 0x00048828 File Offset: 0x00047828
		public _IPreCompileContext GetLibraryContext(string stLibraryId, bool bCreateIfNotExist)
		{
			if (stLibraryId == null)
			{
				return null;
			}
			_IPreCompileContext ipreCompileContext = this[stLibraryId];
			if (ipreCompileContext == null && bCreateIfNotExist)
			{
				ipreCompileContext = new PreCompileContext(stLibraryId, Guid.Empty, KindOfContext.Library);
				LibraryInfo libraryInfo = APEnvironmentFacade.Instance.GetLibraryInfo(stLibraryId);
				LanguageModelManagerConsolidated.GetLibraryInfoEx(ipreCompileContext);
				ipreCompileContext.LinkInSimulation = libraryInfo.LinkInSimulation;
				ipreCompileContext.QualifiedAccessOnly = libraryInfo.QualifiedAccessOnly;
				ipreCompileContext.IsInterfaceLibrary = libraryInfo.IsInterfaceLibrary;
				ipreCompileContext.Support32BitOnly = libraryInfo.Support32BitOnly;
				ipreCompileContext.OnlineChangeable = libraryInfo.OnlineChangeable;
				ipreCompileContext.IgnoreLinkAll = libraryInfo.IgnoreLinkAll;
				ipreCompileContext.UnitTestingDefine = libraryInfo.UnitTestingDefine;
				CaseInsensitiveDictionary<_IPreCompileContext> htLibraries = this.m_htLibraries;
				lock (htLibraries)
				{
					this.m_htLibraries.Add(stLibraryId, ipreCompileContext);
				}
				if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351200)
				{
					IPreCompileContext[] array = APEnvironmentFacade.Instance.LanguageModelMgr.AllPreCompileContexts(true, false);
					for (int i = 0; i < array.Length; i++)
					{
						(array[i] as _IPreCompileContext).Dirty = true;
					}
					PreCompileContext.ClearLibraryTables();
				}
			}
			return ipreCompileContext;
		}

		// Token: 0x0600190C RID: 6412 RVA: 0x00048950 File Offset: 0x00047950
		public int GetProjectHandle(string stLibraryId)
		{
			if (string.IsNullOrEmpty(stLibraryId))
			{
				if (!APEnvironmentFacade.Instance.ExistsPrimaryProject)
				{
					return -1;
				}
				return APEnvironmentFacade.Instance.PrimaryProjectHandle;
			}
			else
			{
				IProject projectByLibraryId = APEnvironmentFacade.Instance.GetProjectByLibraryId(stLibraryId);
				if (projectByLibraryId != null)
				{
					return projectByLibraryId.Handle;
				}
				if (!APEnvironmentFacade.Instance.ExistsPrimaryProject)
				{
					return -1;
				}
				return APEnvironmentFacade.Instance.PrimaryProjectHandle;
			}
		}

		// Token: 0x0600190D RID: 6413 RVA: 0x000489AC File Offset: 0x000479AC
		public IProject GetProjectByLibraryId(string stLibraryId)
		{
			return APEnvironmentFacade.Instance.GetProjectByLibraryId(stLibraryId);
		}

		// Token: 0x04000529 RID: 1321
		[DefaultSerialization("LibraryReferences")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private CaseInsensitiveHashtable m_htLibraryReferences = new CaseInsensitiveHashtable();

		// Token: 0x0400052A RID: 1322
		[DefaultSerialization("Libraries")]
		[StorageVersion("3.3.0.0")]
		[StorageSaveAsNonGenericCollection("3.3.0.0-3.5.6.255")]
		[Obfuscation(Feature = "rename")]
		private CaseInsensitiveDictionary<_IPreCompileContext> m_htLibraries = new CaseInsensitiveDictionary<_IPreCompileContext>();
	}
}
