using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ICodePosition
	{
		long EditorPosition { get; }

		short PositionOffset { get; }

		AccessFlag Access { get; }
	}
}
