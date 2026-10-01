using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[NullableContext(2)]
	[ReleasedInterface]
	public interface IWhiteErrorStatement2 : IWhiteErrorStatement, IWhiteStatement, IWhiteExprement, INode, IStatementSyntax
	{
		string ErrorMessage { get; set; }

		IWhiteToken ErrorToken { get; set; }
	}
}
