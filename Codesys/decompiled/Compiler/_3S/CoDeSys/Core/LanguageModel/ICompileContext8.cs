using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ICompileContext8 : ICompileContext7, ICompileContext6, ICompileContext5, ICompileContext4, ICompileContext3, ICompileContext2, ICompileContext, ICompileContextCommon
	{
		string[] InstancePaths(ISignature sign, out IVariable[] varInstances, out ISignature[] declaringSignatures, bool bWithNamespace, bool bWithStackVariables, bool bWithDerivedClasses);

		string GetLibraryNamespace(string stLibraryId);
	}
}
