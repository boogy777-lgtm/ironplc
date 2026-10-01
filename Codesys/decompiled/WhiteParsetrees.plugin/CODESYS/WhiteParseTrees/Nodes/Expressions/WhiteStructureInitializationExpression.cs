using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CODESYS.WhiteParseTrees.Nodes.Expressions
{
	[System.Runtime.CompilerServices.NullableContext(1)]
	[System.Runtime.CompilerServices.Nullable(0)]
	internal class WhiteStructureInitializationExpression : WhiteExpression, IWhiteStructureInitialization, IWhiteExpression, IWhiteExprement, INode, IExpressionSyntax
	{
		[System.Runtime.CompilerServices.Nullable(2)]
		[field: System.Runtime.CompilerServices.Nullable(2)]
		public IStructToken StructOp
		{
			[System.Runtime.CompilerServices.NullableContext(2)]
			get;
			[System.Runtime.CompilerServices.NullableContext(2)]
			set;
		}

		public ILeftParenthesisToken LeftParenthesis { get; set; }

		public IEnumerable<IWhiteExpression> CompoInits { get; set; }

		public IRightParenthesisToken RightParenthesis { get; set; }

		internal WhiteStructureInitializationExpression(ILeftParenthesisToken leftParenthesis, IEnumerable<IWhiteExpression> compoInits, IRightParenthesisToken rightParenthesis)
		{
			CompoInits = compoInits;
			LeftParenthesis = leftParenthesis;
			RightParenthesis = rightParenthesis;
		}

		public override IEnumerable<INode> GetChildren()
		{
			if (StructOp != null)
			{
				yield return StructOp;
			}
			yield return LeftParenthesis;
			foreach (IWhiteExpression compoInit in CompoInits)
			{
				yield return compoInit;
			}
			yield return RightParenthesis;
		}

		public override void Accept(IExpressionSyntax.IExpressionVisitor visitor)
		{
			visitor.visit(this);
		}

		public override T Accept<[System.Runtime.CompilerServices.Nullable(2)] T>(IExpressionSyntax.IExpressionVisitor<T> visitor)
		{
			return visitor.visit(this);
		}

		public override T Accept<[System.Runtime.CompilerServices.Nullable(2)] T, [System.Runtime.CompilerServices.Nullable(2)] TContext>(IExpressionSyntax.IExpressionVisitor<T, TContext> visitor, TContext context)
		{
			return visitor.visit(this, context);
		}
	}
}
