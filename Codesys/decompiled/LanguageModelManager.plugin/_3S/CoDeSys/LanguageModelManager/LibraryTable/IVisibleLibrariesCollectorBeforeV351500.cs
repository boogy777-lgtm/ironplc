using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.LibraryTable
{
	// Token: 0x020001C0 RID: 448
	internal class IVisibleLibrariesCollectorBeforeV351500 : IVisibleLibrariesCollector
	{
		// Token: 0x06001FD5 RID: 8149 RVA: 0x00057F90 File Offset: 0x00056F90
		public void GetAllVisibleLibraries(Guid appGuid, _IPreCompileContext precom, IList<ILMLibraryInfo> visibleLibs)
		{
			ICaseInsensitiveDictionary<string> caseInsensitiveDictionary = new CaseInsensitiveDictionary<string>();
			foreach (ILMLibraryInfo4 ilmlibraryInfo in APEnvironmentFacade.Instance.LanguageModelMgr.GetLibListForApp(appGuid, precom).Libraries)
			{
				string identification = ilmlibraryInfo.Identification;
				if (!caseInsensitiveDictionary.ContainsKey(identification))
				{
					visibleLibs.Add(ilmlibraryInfo);
					caseInsensitiveDictionary[identification] = identification;
				}
			}
			this.AddVisibleSubLibraries(appGuid, precom, visibleLibs, caseInsensitiveDictionary);
		}

		// Token: 0x06001FD6 RID: 8150 RVA: 0x00058000 File Offset: 0x00057000
		private void AddVisibleSubLibraries(Guid appGuid, _IPreCompileContext precom, ICollection<ILMLibraryInfo> llibs, ICaseInsensitiveDictionary<string> dicLibraries)
		{
			foreach (ILMLibraryInfo4 ilmlibraryInfo in APEnvironmentFacade.Instance.LanguageModelMgr.GetLibListForApp(appGuid, precom).Libraries)
			{
				string identification = ilmlibraryInfo.Identification;
				_IPreCompileContext libraryContext = APEnvironmentFacade.Instance.LanguageModelMgr.LibList.GetLibraryContext(identification);
				if (libraryContext != null && !ilmlibraryInfo.QualifiedOnlyLocal && !ilmlibraryInfo.QualifiedOnly)
				{
					this.AddVisibleLibraries(appGuid, libraryContext, llibs, dicLibraries);
				}
			}
		}

		// Token: 0x06001FD7 RID: 8151 RVA: 0x0005807C File Offset: 0x0005707C
		private void AddVisibleLibraries(Guid appGuid, _IPreCompileContext precom, ICollection<ILMLibraryInfo> llibs, ICaseInsensitiveDictionary<string> dicLibraries)
		{
			foreach (ILMLibraryInfo4 ilmlibraryInfo in APEnvironmentFacade.Instance.LanguageModelMgr.GetLibListForApp(appGuid, precom).Libraries)
			{
				string identification = ilmlibraryInfo.Identification;
				if (ilmlibraryInfo.PublishSymbols && !dicLibraries.ContainsKey(identification))
				{
					dicLibraries[identification] = identification;
					llibs.Add(ilmlibraryInfo);
				}
			}
		}
	}
}
