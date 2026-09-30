using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees
{
	[ReleasedInterface]
	public interface IWhiteOperatorToken : IWhiteToken, INode
	{
		Operator Operator { get; set; }
	}
}
