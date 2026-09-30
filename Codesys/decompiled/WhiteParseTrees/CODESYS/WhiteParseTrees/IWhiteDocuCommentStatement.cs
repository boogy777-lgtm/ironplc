using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[ReleasedInterface]
	public interface IWhiteDocuCommentStatement : IWhiteStatement, IWhiteExprement, INode, IStatementSyntax
	{
		[Nullable(1)]
		IDocCommentToken DocuCommentToken
		{
			[NullableContext(1)]
			get;
			[NullableContext(1)]
			set;
		}
	}
}
