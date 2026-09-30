using System.Collections.Generic;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[NullableContext(1)]
	[ReleasedInterface]
	public interface IWhiteStructureInitialization : IWhiteExpression, IWhiteExprement, INode, IExpressionSyntax
	{
		[Nullable(2)]
		IStructToken StructOp
		{
			[NullableContext(2)]
			get;
			[NullableContext(2)]
			set;
		}

		ILeftParenthesisToken LeftParenthesis { get; set; }

		IEnumerable<IWhiteExpression> CompoInits { get; set; }

		IRightParenthesisToken RightParenthesis { get; set; }
	}
}
