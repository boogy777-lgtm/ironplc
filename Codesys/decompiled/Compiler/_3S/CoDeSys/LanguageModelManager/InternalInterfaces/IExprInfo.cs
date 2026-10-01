using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface IExprInfo
	{
		bool DoGeneration { get; set; }

		int NestingDepth { get; set; }

		bool HasSideEffect { get; set; }
	}
}
