using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees
{
	[ReleasedInterface]
	public interface IWhitePartialAccessExpression : IWhiteExpression, IWhiteExprement, INode, IExpressionSyntax
	{
		int Offset { get; }

		DirectVariableSize Size { get; }

		[Nullable(1)]
		IPartialAccessToken PartialAccessToken
		{
			[NullableContext(1)]
			get;
			[NullableContext(1)]
			set;
		}
	}
}
