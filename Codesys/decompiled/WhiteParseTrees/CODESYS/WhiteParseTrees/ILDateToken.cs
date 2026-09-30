using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[ReleasedInterface]
	public interface ILDateToken : IWhiteToken, INode
	{
		long Value { get; }

		bool Overflow { get; }
	}
}
