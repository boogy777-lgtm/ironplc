using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IScope3 : IScope2, IScope
	{
		int PointerSize { get; }
	}
}
