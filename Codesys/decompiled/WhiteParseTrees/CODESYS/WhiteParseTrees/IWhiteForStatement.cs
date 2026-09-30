using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[NullableContext(1)]
	[ReleasedInterface]
	public interface IWhiteForStatement : IWhiteStatement, IWhiteExprement, INode, IStatementSyntax
	{
		IForToken For { get; set; }

		IWhiteAssignmentExpression StartExpression { get; set; }

		IToToken To { get; set; }

		IWhiteExpression UpperBound { get; set; }

		[Nullable(2)]
		IByToken By
		{
			[NullableContext(2)]
			get;
			[NullableContext(2)]
			set;
		}

		[Nullable(2)]
		IWhiteExpression StepWidth
		{
			[NullableContext(2)]
			get;
			[NullableContext(2)]
			set;
		}

		IDoToken Do { get; set; }

		IWhiteSequenceStatement Controlled { get; set; }

		IEndForToken EndFor { get; set; }
	}
}
