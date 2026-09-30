using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[NullableContext(1)]
	[ReleasedInterface]
	public interface IWhiteReferenceTypeExpression : IWhiteTypeExpression, IWhiteExpression, IWhiteExprement, INode, IExpressionSyntax
	{
		IReferenceToken Reference { get; set; }

		IToToken To { get; set; }

		IWhiteTypeExpression BaseType { get; set; }
	}
}
