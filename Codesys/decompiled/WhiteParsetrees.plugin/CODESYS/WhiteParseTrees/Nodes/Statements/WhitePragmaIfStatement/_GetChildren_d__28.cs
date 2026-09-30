using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CODESYS.WhiteParseTrees.Nodes.Statements
{
	[System.Runtime.CompilerServices.NullableContext(1)]
	[System.Runtime.CompilerServices.Nullable(0)]
	public class WhitePragmaIfStatement : WhiteStatement, IWhitePragmaIfStatement, IWhiteStatement, IWhiteExprement, INode, IStatementSyntax
	{
		public IWhitePragmaStatement If { get; set; }

		public IWhiteSequenceStatement ThenStatement { get; set; }

		public IEnumerable<IWhitePragmaElseIfStatement> ElseIfStatement { get; set; }

		[System.Runtime.CompilerServices.Nullable(2)]
		[field: System.Runtime.CompilerServices.Nullable(2)]
		public IWhitePragmaStatement Else
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

		public IWhitePragmaStatement EndIf { get; set; }

		public WhitePragmaIfStatement(IWhitePragmaStatement @if, IWhiteSequenceStatement then, IEnumerable<IWhitePragmaElseIfStatement> elseifs, [System.Runtime.CompilerServices.Nullable(2)] IWhitePragmaStatement @else, [System.Runtime.CompilerServices.Nullable(2)] IWhiteSequenceStatement elseStatement, IWhitePragmaStatement endif)
		{
			If = @if;
			ThenStatement = then;
			ElseIfStatement = elseifs;
			Else = @else;
			ElseStatement = elseStatement;
			EndIf = endif;
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
			yield return ThenStatement;
			foreach (IWhitePragmaElseIfStatement item in ElseIfStatement)
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
