using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IAddressCodePosition
	{
		long EditorPosition { get; }

		short PositionOffset { get; }

		AccessFlag Access { get; }

		int TypeSize { get; }
	}
}
