using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface ICompilerHelper6 : ICompilerHelper5, ICompilerHelper4, ICompilerHelper3, ICompilerHelper2, ICompilerHelper
	{
		string[] InstancePathsWithPoolNamespace(_ICompileContext comcon, ISignature sign, out IVariable[] varInstances, out ISignature[] declaringSignatures, bool bWithNamespace, bool bWithStackVariables, bool bWithDerivedClasses);
	}
}
