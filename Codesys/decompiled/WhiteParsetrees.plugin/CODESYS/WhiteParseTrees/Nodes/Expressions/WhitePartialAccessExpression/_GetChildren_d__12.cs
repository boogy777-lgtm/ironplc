using System.Collections.Generic;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Expressions
{
	public class WhitePartialAccessExpression : WhiteExpression, IWhitePartialAccessExpression, IWhiteExpression, IWhiteExprement, INode, IExpressionSyntax
	{
		public int Offset => PartialAccessToken.Offset;

		public DirectVariableSize Size => PartialAccessToken.Size;

		public IPartialAccessToken PartialAccessToken { get; set; }

		public WhitePartialAccessExpression(IPartialAccessToken token)
		{
			PartialAccessToken = token;
		}

		public override void Accept(IExpressionSyntax.IExpressionVisitor visitor)
		{
			if (visitor is IExpressionSyntax2.IExpressionVisitor2 expressionVisitor)
			{
				expressionVisitor.visit(this);
			}
		}

		public override T Accept<[System.Runtime.CompilerServices.Nullable(2)] T>(IExpressionSyntax.IExpressionVisitor<T> visitor)
		{
			if (visitor is IExpressionSyntax2.IExpressionVisitor2<T> expressionVisitor)
			{
				return expressionVisitor.visit(this);
			}
			return default(T);
		}

		public override T Accept<[System.Runtime.CompilerServices.Nullable(2)] T, [System.Runtime.CompilerServices.Nullable(2)] TContext>(IExpressionSyntax.IExpressionVisitor<T, TContext> visitor, TContext context)
		{
			if (visitor is IExpressionSyntax2.IExpressionVisitor2<T, TContext> expressionVisitor)
			{
				return expressionVisitor.visit(this, context);
			}
			return default(T);
		}

		public override IEnumerable<INode> GetChildren()
		{
			yield return PartialAccessToken;
		}
	}
}
