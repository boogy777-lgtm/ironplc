using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace CODESYS.Parser
{
	[ReleasedInterface]
	public interface IParserService : ILanguageVersionDependentService
	{
		IInternalParser CreateParser(IScanner9 scanner, IErrorHandler errorhandler, _ILanguageModelBuilder7 factory, ITypeTable3 typeTable, ILMCompileOptions3 compileOptions, _ICompilerVersionSettings compilerVersionSettings);
	}
}
