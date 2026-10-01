using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CODESYS.WhiteParseTrees.Nodes.Statements
{
	[System.Runtime.CompilerServices.NullableContext(1)]
	[System.Runtime.CompilerServices.Nullable(0)]
	public class WhiteSubrangeDeclarationStatement : WhiteStatement, IWhiteSubrangeDeclarationStatement, IWhiteStatement, IWhiteExprement, INode, IStatementSyntax
	{
		public IEnumerable<IWhiteExpression> VariableNames { get; set; }

		public IColonToken Colon { get; set; }

		public ISubRangeTypeExpression SubRangeType { get; set; }

		public ISemicolonToken Semicolon { get; set; }

		public WhiteSubrangeDeclarationStatement(IList<IWhiteExpression> variableNames, IColonToken colon, ISubRangeTypeExpression subRangeType, ISemicolonToken semicolon)
		{
			VariableNames = variableNames;
			Colon = colon;
			SubRangeType = subRangeType;
			Semicolon = semicolon;
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

		public override IEnumerable<INode> GetChildren()
		{
			foreach (IWhiteExpression variableName in VariableNames)
			{
				yield return variableName;
			}
			yield return Colon;
			yield return SubRangeType;
			yield return Semicolon;
		}
	}
}
