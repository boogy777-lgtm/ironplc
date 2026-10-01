using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface ICompilerHandleInstanceVariables
	{
		void AddInstVarsToParent(_ISignature sign, _ICompileContext comconNew, _ICompileContext comconRef);
	}
}
