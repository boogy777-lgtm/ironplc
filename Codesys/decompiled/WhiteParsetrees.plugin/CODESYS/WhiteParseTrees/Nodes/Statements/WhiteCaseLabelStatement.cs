using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CODESYS.WhiteParseTrees.Nodes.Statements
{
	[System.Runtime.CompilerServices.NullableContext(1)]
	[System.Runtime.CompilerServices.Nullable(0)]
	public class WhiteCaseLabelStatement : WhiteStatement, IWhiteCaseLabelStatement, IWhiteStatement, IWhiteExprement, INode, IStatementSyntax
	{
		public IEnumerable<IWhiteExpression> CaseExpressionList { get; set; }

		public IColonToken Colon { get; set; }

		public WhiteCaseLabelStatement(IEnumerable<IWhiteExpression> caseExpressionList, IColonToken colon)
		{
			CaseExpressionList = caseExpressionList;
			Colon = colon;
		}

		public override IEnumerable<INode> GetChildren()
		{
			foreach (IWhiteExpression caseExpression in CaseExpressionList)
			{
				yield return caseExpression;
			}
			yield return Colon;
		}

		public override void Accept(IStatementSyntax.IStatementVisitor visitor)
		{
			visitor.visit(this);
		}

		public override T Accept<[System.Runtime.CompilerServices.Nullable(2)] T>(IStatementSyntax.IStatementVisitor<T> visitor)
		{
			return visitor.visit(this);
		}

		public override T Accept<[System.Runtime.CompilerServices.Nullable(2)] T, [System.Runtime.CompilerServices.Nullable(2)] TContext>(IStatementSyntax.IStatementVisitor<T, TContext> visitor, TContext context)
		{
			return visitor.visit(this, context);
		}
	}
}
