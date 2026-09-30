using System;
using System.Linq;
using System.Runtime.CompilerServices;
using CODESYS.WhiteParseTrees.Nodes.Factories;

namespace CODESYS.WhiteParseTrees.Services
{
	[System.Runtime.CompilerServices.NullableContext(1)]
	[System.Runtime.CompilerServices.Nullable(0)]
	public static class ExprementExtensions
	{
		[return: System.Runtime.CompilerServices.Nullable(2)]
		public static IWhiteToken GetLeadingToken(this IWhiteExprement exprement)
		{
			INode node = exprement.GetChildren().FirstOrDefault();
			if (!(node is IWhiteExprement exprement2))
			{
				if (node is IWhiteToken whiteToken)
				{
					return whiteToken.Leading;
				}
				return null;
			}
			return exprement2.GetLeadingToken();
		}

		public static void SetLeadingToken(this IWhiteExprement exprement, [System.Runtime.CompilerServices.Nullable(2)] IWhiteToken leading)
		{
			INode node = exprement.GetChildren().FirstOrDefault();
			if (!(node is IWhiteExprement exprement2))
			{
				if (node is IWhiteToken whiteToken)
				{
					whiteToken.Leading = leading;
				}
			}
			else
			{
				exprement2.SetLeadingToken(leading);
			}
		}

		public static IWhiteExpression Duplicate(this IWhiteExpression expression)
		{
			return new ExpressionFactory().ParseExpression(expression.ToString()) ?? throw new InvalidOperationException();
		}
	}
}
