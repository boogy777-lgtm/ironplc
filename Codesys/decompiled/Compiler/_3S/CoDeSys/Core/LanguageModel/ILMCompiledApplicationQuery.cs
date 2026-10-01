using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILMCompiledApplicationQuery
	{
		IDataManager2 DataManager { get; }

		ICodegenerator Codegenerator { get; }

		int PointerSize { get; }

		IApplicationContent ApplicationContent { get; }

		string[] InstancePaths(ISignature sign, out IVariable[] varInstances, out ISignature[] declaringSignatures);

		string[] InstancePaths(ISignature sign, out IVariable[] varInstances, out ISignature[] declaringSignatures, bool bWithNamespace, bool bWithStackVariables, bool bWithDerivedClasses);

		IEnumerable<IInstancePathInfo> InstancePaths(ISignature sign, bool bWithDerivedFunctionBlocks);

		string[] SubElements(string stSignatureName, string stAccessPath, out bool bValid);

		string[] SubElements(string stSignatureName, int nProjectHandle, Guid guidObject, string stAccessPath, out bool bValid);

		string[] SubElementsWithRange(string stSignatureName, int nProjectHandle, Guid guidObject, string stAccessPath, int nStartIndex, int nEndIndex, out bool bValid);

		string[] SubElementsWithRange(IType type, string stAccessPath, int nStartIndex, int nEndIndex, out bool bValid);

		IEnumerable<ITaskInfo> GetTasksReferencingSignature(ISignature sign);

		byte[] GetTaskIds(IVariable var, ISignature signDecl, bool bWriteOnly);

		IEnumerable<ICodePosition> GetAccessPositionsOfVariable(int nSignatureIdWithReferences, int nSignatureIdWithVar, int nVariableId);

		IDataLocation LocateAddress(out bool bError, IDirectVariable dirvar);

		IMemoryManager GetMemoryManager(ushort usArea);

		IDirectVariable FindDirectVariable(IAbsoluteAddressInfo addressInfo);

		IEnumerable<string> FindChangedObjects();

		IEnumerable<IChangedLMObject> FindChangedObjectsDetailed();

		byte[] GetInitializationBlob(IScope5 scope, bool isMotorolaByteOrder, IVariable var, out IRelocationList2 relocations);

		string GetDefaultInitializationCode(IVariable var, ISignature sign, IScope scope, string stInstancePath);
	}
}
