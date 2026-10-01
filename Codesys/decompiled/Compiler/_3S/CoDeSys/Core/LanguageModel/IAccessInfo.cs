using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IAccessInfo
	{
		ISourcePosition Position { get; }

		AccessFlag Access { get; }
	}
}
