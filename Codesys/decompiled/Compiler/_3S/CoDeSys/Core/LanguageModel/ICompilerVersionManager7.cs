using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ICompilerVersionManager7 : ICompilerVersionManager6, ICompilerVersionManager5, ICompilerVersionManager4, ICompilerVersionManager3, ICompilerVersionManager2, ICompilerVersionManager
	{
		void SetCompilerVersionExact(Version version);

		void ClearLanguageModel();

		string MapFromInternalToOEMTextSave(Version vinternal);

		Version MapFromOEMTextToInternalSave(string stOEMString);

		Version GetCustomizationVersion();
	}
}
