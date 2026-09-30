using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[ReleasedInterface]
	public interface ILTimeOfDayToken : IWhiteToken, INode
	{
		long Value { get; }

		bool Overflow { get; }
	}
}
