using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees
{
	[ReleasedInterface]
	public interface IConversionToken : IWhiteOperatorToken, IWhiteToken, INode
	{
		TypeClass From { get; }

		TypeClass To { get; }
	}
}
