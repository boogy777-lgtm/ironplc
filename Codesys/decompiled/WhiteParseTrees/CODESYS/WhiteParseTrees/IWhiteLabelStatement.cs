using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[NullableContext(1)]
	[ReleasedInterface]
	public interface IWhiteLabelStatement : IWhiteStatement, IWhiteExprement, INode, IStatementSyntax
	{
		string Label { get; }

		IIdentifierToken LabelToken { get; set; }

		IColonToken Colon { get; set; }
	}
}
