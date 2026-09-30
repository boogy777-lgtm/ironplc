using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[ReleasedInterface]
	public interface IWhiteUserDefTypeExpression : IWhiteTypeExpression, IWhiteExpression, IWhiteExprement, INode, IExpressionSyntax
	{
		[Nullable(1)]
		IWhiteExpression NameExpression
		{
			[NullableContext(1)]
			get;
			[NullableContext(1)]
			set;
		}
	}
}
