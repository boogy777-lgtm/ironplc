using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[ReleasedInterface]
	public interface IDurationToken : IWhiteToken, INode
	{
		uint Value { get; }

		bool Overflow { get; }
	}
}
