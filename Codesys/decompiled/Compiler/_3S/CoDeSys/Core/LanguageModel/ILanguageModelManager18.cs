using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILanguageModelManager18 : ILanguageModelManager17, ILanguageModelManager16, ILanguageModelManager15, ILanguageModelManager14, ILanguageModelManager13, ILanguageModelManager12, ILanguageModelManager11, ILanguageModelManager10, ILanguageModelManager9, ILanguageModelManager8, ILanguageModelManager7, ILanguageModelManager6, ILanguageModelManager5, ILanguageModelManager4, ILanguageModelManager3, ILanguageModelManager2, ILanguageModelManager
	{
		IAddressCalculation CreateAddressCalculaton(ICompileContext comcon);

		IDictionary<string, IList<IAccessInfo>> GetPrecompiledCrossReferences(Regex regex);

		IApplicationContent BuildApplicationContentFromUpload(byte[] bytes, bool bIsMotorolaByteOrder);

		string GetApplicationNameByGuid(Guid guidApplication, bool bSimulationMode);

		void UpdateDownloadContextSynchWriteContext(Guid guidApplication, bool bCreateBootDuplicate);
	}
}
