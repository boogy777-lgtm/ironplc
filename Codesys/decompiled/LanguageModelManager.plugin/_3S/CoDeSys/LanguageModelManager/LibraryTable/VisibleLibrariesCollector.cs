using System;
using System.Collections.Generic;
using System.Linq;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.LibraryTable
{
	// Token: 0x020001BF RID: 447
	internal class VisibleLibrariesCollector : IVisibleLibrariesCollector
	{
		// Token: 0x06001FD0 RID: 8144 RVA: 0x00057CE8 File Offset: 0x00056CE8
		public void GetAllVisibleLibraries(Guid appGuid, _IPreCompileContext precom, IList<ILMLibraryInfo> visibleLibs)
		{
			VisibleLibrariesCollector.FilterLibrary libFilter = (ILMLibraryInfo lmi) => !((ILMLibraryInfo5)lmi).PoolLibrary;
			this.GetAllVisibleLibraries(appGuid, precom, visibleLibs, new CaseInsensitiveDictionary<string>(), libFilter);
			VisibleLibrariesCollector.FilterLibrary libFilter2 = (ILMLibraryInfo lmi) => ((ILMLibraryInfo5)lmi).PoolLibrary;
			this.GetAllVisibleLibraries(appGuid, precom, visibleLibs, new CaseInsensitiveDictionary<string>(), libFilter2);
		}

		// Token: 0x06001FD1 RID: 8145 RVA: 0x00057D54 File Offset: 0x00056D54
		private void GetAllVisibleLibraries(Guid appGuid, _IPreCompileContext precom, ICollection<ILMLibraryInfo> llibs, ICaseInsensitiveDictionary<string> dicLibraries, VisibleLibrariesCollector.FilterLibrary libFilter)
		{
			foreach (ILMLibraryInfo ilmlibraryInfo in from libinfo in APEnvironmentFacade.Instance.LanguageModelMgr.GetLibListForApp(appGuid, precom).Libraries
			where libFilter(libinfo)
			select libinfo)
			{
				ILMLibraryInfo5 ilmlibraryInfo2 = (ILMLibraryInfo5)ilmlibraryInfo;
				string identification = ilmlibraryInfo2.Identification;
				if (!dicLibraries.ContainsKey(identification))
				{
					llibs.Add(ilmlibraryInfo2);
					dicLibraries[identification] = identification;
				}
			}
			this.AddVisibleSubLibraries(appGuid, precom, llibs, dicLibraries, libFilter);
		}

		// Token: 0x06001FD2 RID: 8146 RVA: 0x00057E00 File Offset: 0x00056E00
		private void AddVisibleSubLibraries(Guid appGuid, _IPreCompileContext precom, ICollection<ILMLibraryInfo> llibs, ICaseInsensitiveDictionary<string> dicLibraries, VisibleLibrariesCollector.FilterLibrary libFilter)
		{
			foreach (ILMLibraryInfo ilmlibraryInfo in from libinfo in APEnvironmentFacade.Instance.LanguageModelMgr.GetLibListForApp(appGuid, precom).Libraries
			where libFilter(libinfo)
			where !((ILMLibraryInfo5)libinfo).QualifiedOnlyLocal && !libinfo.QualifiedOnly
			select libinfo)
			{
				string identification = ((ILMLibraryInfo5)ilmlibraryInfo).Identification;
				_IPreCompileContext libraryContext = APEnvironmentFacade.Instance.LanguageModelMgr.LibList.GetLibraryContext(identification);
				if (libraryContext != null)
				{
					this.AddVisibleLibraries(appGuid, libraryContext, llibs, dicLibraries, libFilter);
				}
			}
		}

		// Token: 0x06001FD3 RID: 8147 RVA: 0x00057ED0 File Offset: 0x00056ED0
		private void AddVisibleLibraries(Guid appGuid, _IPreCompileContext precom, ICollection<ILMLibraryInfo> llibs, ICaseInsensitiveDictionary<string> dicLibraries, VisibleLibrariesCollector.FilterLibrary libFilter)
		{
			foreach (ILMLibraryInfo ilmlibraryInfo in from libinfo in APEnvironmentFacade.Instance.LanguageModelMgr.GetLibListForApp(appGuid, precom).Libraries
			where libinfo.PublishSymbols
			where libFilter(libinfo)
			select libinfo)
			{
				ILMLibraryInfo5 ilmlibraryInfo2 = (ILMLibraryInfo5)ilmlibraryInfo;
				string identification = ilmlibraryInfo2.Identification;
				if (!dicLibraries.ContainsKey(identification))
				{
					dicLibraries[identification] = identification;
					llibs.Add(ilmlibraryInfo2);
				}
			}
		}

		// Token: 0x020002C5 RID: 709
		// (Invoke) Token: 0x06002C2A RID: 11306
		private delegate bool FilterLibrary(ILMLibraryInfo lmi);
	}
}
