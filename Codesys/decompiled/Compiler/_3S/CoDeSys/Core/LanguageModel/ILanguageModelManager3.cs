using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILanguageModelManager3 : ILanguageModelManager2, ILanguageModelManager
	{
		ITypeInfo TypeInfo { get; }

		event CompileEventHandler BeforeLocation;

		event CompileEventHandler AfterLocation;
	}
}
