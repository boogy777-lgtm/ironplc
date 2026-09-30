using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface ICompilerServiceFactory
	{
		_ICompilerMessageCreator CompilerMessageCreator { get; }

		ICompiler CreateCompiler();

		ICompilerHelper CreateHelper();

		ITypeComparer CreateTypeComparer();

		ILanguageModelHandling CreateLMHandler();

		ITypeCompiler CreateTypeCompiler();
	}
}
