using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Expressions
{
	[System.Runtime.CompilerServices.NullableContext(1)]
	[System.Runtime.CompilerServices.Nullable(0)]
	[ExcludeFromCodeCoverage]
	public class VectorTypeExpression : WhiteTypeExpression, IVectorTypeExpression, IWhiteTypeExpression, IWhiteExpression, IWhiteExprement, INode, IExpressionSyntax
	{
		public I__VectorToken Vector { get; set; }

		public ILeftBracketToken LeftBracket { get; set; }

		public IWhiteExpression Length { get; set; }

		public IRightBracketToken RightBracket { get; set; }

		public IOfToken Of { get; set; }

		public IWhiteTypeExpression BaseType { get; set; }

		public override TypeClass Class => TypeClass.__Vector;

		public VectorTypeExpression(I__VectorToken vector, ILeftBracketToken leftBracket, IWhiteExpression length, IRightBracketToken rightBracket, IOfToken of, IWhiteTypeExpression baseType)
		{
			Vector = vector;
			LeftBracket = leftBracket;
			Length = length;
			RightBracket = rightBracket;
			Of = of;
			BaseType = baseType;
		}

		public override IEnumerable<INode> GetChildren()
		{
			yield return Vector;
			yield return LeftBracket;
			yield return Length;
			yield return RightBracket;
			yield return Of;
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
