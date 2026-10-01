using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.LibraryTable
{
	// Token: 0x020001BE RID: 446
	public interface IVisibleLibrariesCollector
	{
		// Token: 0x06001FCF RID: 8143
		void GetAllVisibleLibraries(Guid appGuid, _IPreCompileContext precom, IList<ILMLibraryInfo> visibleLibs);
	}
}
