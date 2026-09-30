using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[ReleasedInterface]
	public interface IWhitePropertyAccessorStatement : IWhitePOU, IWhitePOUSyntax, IWhiteStatement, IWhiteExprement, INode, IStatementSyntax
	{
		[Nullable(1)]
		IEndPropertyToken2 EndProperty
		{
			[NullableContext(1)]
			get;
			[NullableContext(1)]
			set;
		}
	}
}
