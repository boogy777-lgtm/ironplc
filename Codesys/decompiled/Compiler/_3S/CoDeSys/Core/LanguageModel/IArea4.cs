using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IArea4 : IArea3, IArea2, IArea
	{
		int MinimalAreaSize { get; }

		int MaximalAreaSize { get; }
	}
}
