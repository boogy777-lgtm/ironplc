using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ICompileContext : ICompileContextCommon
	{
		ICodegenerator Codegenerator { get; }

		ITaskInfo[] AllTasks { get; }

		long TimeStampContext { get; }

		long TimeStampPool { get; }

		ISignature GetSignatureById(int nId);

		ICompiledPOU GetCompiledPOUById(int nId);

		int NumSubElements(string stSignatureName, string stAccessPath, out bool bValid);

		string[] SubElements(string stSignatureName, string stAccessPath, out bool bValid);

		[Obsolete("Please use InstancePaths(ISignature sign, out IVariable[] varInstances, out ISignature[] declaringSignatures); instead")]
		string[] InstancePaths(string stSignatureName);

		string[] InstancePaths(ISignature sign, out IVariable[] varInstances, out ISignature[] declaringSignatures);

		string DumpCode(ICompiledPOU cpou);

		string DumpDataManager();

		ITaskInfo[] GetTasksReferencingSignature(ISignature sign);

		IScope CreateGlobalIScope();

		IScope CreateIScope(int nIdLocal);

		IScope CreateIScope(int nIdLocal, int nIdMethod);

		IDirectVariableCrossRefTable GetDirectVariableTable();

		IDataLocation LocateAddress(out bool bError, IDirectVariable dirvar);

		IBreakpoint GetBreakpointByCodePosition(ushort usArea, uint uiOffset, out ICompiledPOU cpou);

		ICompiledPOU GetPOUByCodePosition(ushort usArea, uint uiOffset);

		ICompiledPOU GetTaskSuccessor(ICompiledPOU cpouPredecessor, ISignature signTaskPOU);
	}
}
