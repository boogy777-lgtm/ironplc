using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[ReleasedInterface]
	public interface ILanguageModelUtilities2 : ILanguageModelUtilities
	{
		ICompileUtilities CompileUtils { get; }
	}
}
