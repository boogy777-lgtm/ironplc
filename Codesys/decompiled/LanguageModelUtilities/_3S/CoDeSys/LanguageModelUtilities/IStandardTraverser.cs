using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[ReleasedInterface]
	public interface IStandardTraverser
	{
		IPrecompileScope5 Scope { get; }

		bool Abort { get; set; }
	}
}
