using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IArea3 : IArea2, IArea
	{
		bool OnlineChangeArea { get; }
	}
}
