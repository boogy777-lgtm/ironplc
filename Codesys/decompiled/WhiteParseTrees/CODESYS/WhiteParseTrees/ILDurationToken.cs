using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[ReleasedInterface]
	public interface ILDurationToken : IWhiteToken, INode
	{
		ulong Value { get; }

		bool Overflow { get; }
	}
}
