using System.Collections.Generic;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Expressions
{
	[System.Runtime.CompilerServices.NullableContext(1)]
	[System.Runtime.CompilerServices.Nullable(0)]
	public class WhiteArrayTypeExpression : WhiteTypeExpression, IWhiteArrayTypeExpression, IWhiteTypeExpression, IWhiteExpression, IWhiteExprement, INode, IExpressionSyntax
	{
		public IArrayToken Array { get; set; }

		public ILeftBracketToken LeftBracket { get; set; }

		public IEnumerable<IWhiteExpression> Ranges { get; set; }

		public IRightBracketToken RightBracket { get; set; }

		public IOfToken Of { get; set; }

		public IWhiteTypeExpression BaseType { get; set; }

		public override TypeClass Class => TypeClass.Array;

		public WhiteArrayTypeExpression(IArrayToken array, ILeftBracketToken leftBracket, IEnumerable<IWhiteExpression> ranges, IRightBracketToken rightBracket, IOfToken of, IWhiteTypeExpression baseType)
		{
			Array = array;
			LeftBracket = leftBracket;
			Ranges = ranges;
			RightBracket = rightBracket;
			Of = of;
			BaseType = baseType;
		}

		public override IEnumerable<INode> GetChildren()
		{
			yield return Array;
			yield return LeftBracket;
			foreach (IWhiteExpression range in Ranges)
			{
				yield return range;
			}
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
