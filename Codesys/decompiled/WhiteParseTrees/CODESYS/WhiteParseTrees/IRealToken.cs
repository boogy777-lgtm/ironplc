using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees
{
	[ReleasedInterface]
	public interface IRealToken : IWhiteToken, INode
	{
		bool Overflow { get; }

		Operator Op { get; }

		double Value { get; }
	}
}
