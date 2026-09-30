using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[NullableContext(1)]
	[ReleasedInterface]
	public interface IWhiteJumpStatement : IWhiteStatement, IWhiteExprement, INode, IStatementSyntax
	{
		IJumpToken Jump { get; set; }

		[Nullable(2)]
		IParenthesizedExpression Condition
		{
			[NullableContext(2)]
			get;
			[NullableContext(2)]
			set;
		}

		IIdentifierToken LabelToken { get; set; }

		string Label { get; }

		ISemicolonToken Semicolon { get; set; }
	}
}
