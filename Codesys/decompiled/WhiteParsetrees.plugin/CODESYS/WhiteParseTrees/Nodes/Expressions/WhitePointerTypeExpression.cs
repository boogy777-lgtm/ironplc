using System.Collections.Generic;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Expressions
{
	[System.Runtime.CompilerServices.NullableContext(1)]
	[System.Runtime.CompilerServices.Nullable(0)]
	public class WhitePointerTypeExpression : WhiteTypeExpression, IWhitePointerTypeExpression, IWhiteTypeExpression, IWhiteExpression, IWhiteExprement, INode, IExpressionSyntax
	{
		public IPointerToken Pointer { get; set; }

		public IToToken To { get; set; }

		public IWhiteTypeExpression BaseType { get; set; }

		public override TypeClass Class => TypeClass.Pointer;

		public WhitePointerTypeExpression(IPointerToken pointer, IToToken to, IWhiteTypeExpression baseType)
		{
			Pointer = pointer;
			To = to;
			BaseType = baseType;
		}

		public override IEnumerable<INode> GetChildren()
		{
			yield return Pointer;
			yield return To;
			yield return BaseType;
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
