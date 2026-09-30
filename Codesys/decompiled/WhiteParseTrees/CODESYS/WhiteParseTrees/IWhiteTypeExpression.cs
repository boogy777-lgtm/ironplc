using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees
{
	[ReleasedInterface]
	public interface IWhiteTypeExpression : IWhiteExpression, IWhiteExprement, INode, IExpressionSyntax
	{
		TypeClass Class { get; }
	}
}
