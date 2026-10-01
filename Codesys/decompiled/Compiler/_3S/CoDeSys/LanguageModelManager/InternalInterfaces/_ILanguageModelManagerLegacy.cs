using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _ILanguageModelManagerLegacy
	{
		IEnumerable<IPreCompileContext> LibraryContexts { get; }

		IPreCompileContext SystemContext { get; }

		IPreCompileContext[] AllPreCompileContexts(bool bWithDevices, bool bWithLibraries);

		ISignature FindSignature(Guid guidObject, out IPreCompileContext precom);

		void ForceRebuildAll(Guid guidApplication);

		void ClearDownloadContext(Guid guidApplication);

		void UpdateDownloadContext(Guid guidApplication);

		bool GenerateCode(Guid guidApplication, bool bOnlineChange, bool bKeepCompileInformation);

		IScanner CreateScanner(string stText, bool bIncludeComments, bool bIncludeEndOfLines, bool bIncludePragmas, bool bIncludeWhitespaces);

		IParser CreateParser(IScanner scanner);

		IPreCompileContext GetPrecompileContext(Guid guidApplication);

		ICompileContext GetCompileContext(Guid guidApplication);

		string GetApplicationNameByGuid(Guid guidApplication);

		IPreCompileContext2 GetPrecompileContextOfSignature(ISignature sign);

		void CreateBootDuplicate(Guid guidApplication);

		bool IsHiddenSignature(ISignature signature, GUIHidingFlags flagsToConsider);

		ILanguageModelBuilder CreateLanguageModelBuilder();

		string GetApplicationNameByGuid(Guid guidApplication, bool bSimulationMode);

		bool IsHiddenVariable(ISignature6 signature, IVariable variable, GUIHidingFlags flagsToConsider);

		ISignature6 GetSignatureForPrecompileID(int precompileId);

		bool IsHiddenVariable(IVariable variable, GUIHidingFlags flagsToConsider);
	}
}
