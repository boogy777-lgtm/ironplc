using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[ReleasedInterface]
	public interface IWhiteFinalStatement : IWhiteStatement, IWhiteExprement, INode, IStatementSyntax
	{
		[Nullable(1)]
		IEndToken FinalToken
		{
			[NullableContext(1)]
			get;
			[NullableContext(1)]
			set;
		}
	}
}
