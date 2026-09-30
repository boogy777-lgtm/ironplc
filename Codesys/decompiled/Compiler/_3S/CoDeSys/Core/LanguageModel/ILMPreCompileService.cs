using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILMPreCompileService
	{
		ILMPreCompileSet SystemSet { get; }

		IEnumerable<ILMPreCompileSet> PrecompileSets { get; }

		IEnumerable<ILMPreCompileSet> LibrarySets { get; }

		event IsHiddenVariableEventHandler IsHiddenVariableHandler;

		event SimulationModeChangedEventHandler AfterSimulationModeChanged;

		event CompileEventHandler TaskConfigChanged;

		event SignatureChangedEventHandler SignatureChanged;

		event SignatureChangedEventHandler SignatureDeleted;

		event SignatureChangedEventHandler SignatureInserted;

		event CompiledPOUChangedEventHandler CompiledPOUChanged;

		event CompiledPOUChangedEventHandler CompiledPOUDeleted;

		event CompiledPOUChangedEventHandler CompiledPOUInserted;

		event EventHandler<LibraryContextDeletedEventArgs> LibrarySetDeleted;

		IPrecompileScope CreatePrecompileScope(ILMPreCompileSet precompileSet, Guid guidSignature);

		bool IsHiddenVariable(ISignature6 signature, IVariable variable, GUIHidingFlags flagsToConsider, ISignature signCurrent);

		bool IsHiddenVariable(ISignature6 signature, IVariable variable, GUIHidingFlags flagsToConsider);

		bool IsHiddenSignature(ISignature signature, GUIHidingFlags flagsToConsider);

		ISignature6 GetSignatureForPrecompileID(int precompileId);

		ILMPreCompileSet GetPreCompileSet(Guid guidApplication);

		ILMPreCompileSet GetLibraryPrecompileSet(string stLibraryId);

		IEnumerable<ISignature> AllPrecompiledSignatures(bool bWithLibraries, bool bWithResources);

		IEnumerable<ILMPreCompileSet> AllPreCompileSets(bool bWithDevices, bool bWithLibraries);

		ISignature FindSignature(Guid guidObject, out ILMPreCompileSet precom);

		IEnumerable<ISignature> FindSignaturesByName(int nProjectHandle, Guid callingObjectGuid, string stName);

		IEnumerable<ISignature> FindSignaturesByName(int nProjectHandle, Guid applicationGuid, Guid callingObjectGuid, string stName);

		ILMPreCompileSet GetPrecompileSetOfSignature(ISignature sign);

		ISignature2 GetBaseSignature(ISignature2 sign);

		IEnumerable<ISignature2> GetInterfaceSignatures(ISignature2 sign);

		IEnumerable<IVariable2> GetAllVariables(ISignature2 sign);

		IEnumerable<ISignature2> GetAllMethods(ISignature2 sign);

		IEnumerable<ISignature2> GetAllInterfaces(ISignature2 sign);

		ISignature2 GetBaseSignature(ISignature2 sign, Guid guidApplication);

		IEnumerable<ISignature2> GetInterfaceSignatures(ISignature2 sign, Guid guidApplication);

		IEnumerable<IVariable2> GetAllVariables(ISignature2 sign, Guid guidApplication);

		IEnumerable<ISignature2> GetAllMethods(ISignature2 sign, Guid guidApplication);

		IEnumerable<ISignature2> GetAllInterfaces(ISignature2 sign, Guid guidApplication);

		bool ExpressionsEqual(IExprement exp1, IExprement exp2);
	}
}
