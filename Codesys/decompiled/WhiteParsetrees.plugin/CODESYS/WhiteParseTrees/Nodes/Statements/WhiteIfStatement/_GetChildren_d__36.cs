using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CODESYS.WhiteParseTrees.Nodes.Statements
{
	[System.Runtime.CompilerServices.NullableContext(1)]
	[System.Runtime.CompilerServices.Nullable(0)]
	public class WhiteIfStatement : WhiteStatement, IWhiteIfStatement, IWhiteStatement, IWhiteExprement, INode, IStatementSyntax
	{
		public IIfToken If { get; set; }

		public IWhiteExpression Condition { get; set; }

		public IThenToken Then { get; set; }

		public IWhiteSequenceStatement ThenStatement { get; set; }

		public IEnumerable<IWhiteElseIfStatement> ElseIfStatement { get; set; }

		[System.Runtime.CompilerServices.Nullable(2)]
		[field: System.Runtime.CompilerServices.Nullable(2)]
		public IElseToken Else
		{
			[System.Runtime.CompilerServices.NullableContext(2)]
			get;
			[System.Runtime.CompilerServices.NullableContext(2)]
			set;
		}

		[System.Runtime.CompilerServices.Nullable(2)]
		[field: System.Runtime.CompilerServices.Nullable(2)]
		public IWhiteSequenceStatement ElseStatement
		{
			[System.Runtime.CompilerServices.NullableContext(2)]
			get;
			[System.Runtime.CompilerServices.NullableContext(2)]
			set;
		}

		public IEndIfToken EndIf { get; set; }

		public WhiteIfStatement(IIfToken _if, IWhiteExpression condition, IThenToken _then, IWhiteSequenceStatement thenStatement, IEnumerable<IWhiteElseIfStatement> elseIfStatements, [System.Runtime.CompilerServices.Nullable(2)] IElseToken _else, [System.Runtime.CompilerServices.Nullable(2)] IWhiteSequenceStatement elseStatement, IEndIfToken _endif)
		{
			If = _if;
			Condition = condition;
			Then = _then;
			ThenStatement = thenStatement;
			ElseIfStatement = elseIfStatements;
			Else = _else;
			ElseStatement = elseStatement;
			EndIf = _endif;
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
			yield return If;
			yield return Condition;
			yield return Then;
			yield return ThenStatement;
			foreach (IWhiteElseIfStatement item in ElseIfStatement)
			{
				yield return item;
			}
			if (Else != null)
			{
				yield return Else;
			}
			if (ElseStatement != null)
			{
				yield return ElseStatement;
			}
			yield return EndIf;
		}
	}
}
