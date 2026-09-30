using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Expressions
{
	[System.Runtime.CompilerServices.NullableContext(1)]
	[System.Runtime.CompilerServices.Nullable(0)]
	[ExcludeFromCodeCoverage]
	public class EnumerationTypeExpression : WhiteTypeExpression, IEnumerationTypeExpression, IWhiteTypeExpression, IWhiteExpression, IWhiteExprement, INode, IExpressionSyntax
	{
		public ILeftParenthesisToken LeftParenthesis { get; set; }

		public IEnumerable<IWhiteExpression> Definitions { get; set; }

		public IRightParenthesisToken RightParenthesis { get; set; }

		public override TypeClass Class => TypeClass.Enum;

		public EnumerationTypeExpression(ILeftParenthesisToken leftParenthesis, IEnumerable<IWhiteExpression> definitions, IRightParenthesisToken rightParenthesis)
		{
			LeftParenthesis = leftParenthesis;
			Definitions = definitions;
			RightParenthesis = rightParenthesis;
		}

		public override IEnumerable<INode> GetChildren()
		{
			yield return LeftParenthesis;
			foreach (IWhiteExpression definition in Definitions)
			{
				yield return definition;
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
