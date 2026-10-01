using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[ReleasedInterface]
	public interface ILDateAndTimeToken : IWhiteToken, INode
	{
		long Value { get; }

		bool Overflow { get; }
	}
}
