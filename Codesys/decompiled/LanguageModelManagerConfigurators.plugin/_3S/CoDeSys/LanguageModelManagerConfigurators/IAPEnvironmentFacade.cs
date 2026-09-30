using System;
using System.Drawing;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManagerConfigurators
{
	public interface IAPEnvironmentFacade
	{
		ILMCompileOptions CompileOptions { get; }

		Version[] AvailableCompilerVersionsOEMFilteredNotReplaced { get; }

		Icon GetIcon(Type type, string location);

		Version CompilerVersionToUse();

		void SetCompilerVersionExact(Version version);

		string MapFromInternalToOEMTextSave(Version version);

		string MapFromInternalToOEMText(Version version);

		Version GetCustomizationVersion();

		Version MapFromOEMTextToInternal(string version);

		Version MapFromOEMTextToInternalSave(string version);

		void ClearLanguageModel();

		bool IsLibraryWithPinnedStorageVersion();
	}
}
