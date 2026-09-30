using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CODESYS.WhiteParseTrees.Nodes.Statements
{
	[System.Runtime.CompilerServices.NullableContext(1)]
	[System.Runtime.CompilerServices.Nullable(0)]
	public class WhiteErrorStatement : WhiteStatement, IWhiteErrorStatement2, IWhiteErrorStatement, IWhiteStatement, IWhiteExprement, INode, IStatementSyntax
	{
		[System.Runtime.CompilerServices.Nullable(new byte[] { 2, 1 })]
		private readonly IEnumerable<IWhiteStatement> _erroneousStatements;

		public IEnumerable<IWhiteToken> TokenList { get; set; }

		[System.Runtime.CompilerServices.Nullable(2)]
		[field: System.Runtime.CompilerServices.Nullable(2)]
		public string ErrorMessage
		{
			[System.Runtime.CompilerServices.NullableContext(2)]
			get;
			[System.Runtime.CompilerServices.NullableContext(2)]
			set;
		}

		[System.Runtime.CompilerServices.Nullable(2)]
		[field: System.Runtime.CompilerServices.Nullable(2)]
		public IWhiteToken ErrorToken
		{
			[System.Runtime.CompilerServices.NullableContext(2)]
			get;
			[System.Runtime.CompilerServices.NullableContext(2)]
			set;
		}

		[System.Runtime.CompilerServices.NullableContext(2)]
		public WhiteErrorStatement([System.Runtime.CompilerServices.Nullable(1)] IEnumerable<IWhiteToken> tokens, string errorMessage, IWhiteToken errorToken)
		{
			TokenList = tokens;
			ErrorMessage = errorMessage;
			ErrorToken = errorToken;
		}

		public WhiteErrorStatement(IEnumerable<IWhiteToken> tokens)
			: this(tokens, null, null)
		{
		}

		public WhiteErrorStatement(IEnumerable<IWhiteStatement> erroneousStatements, string errorMessage, [System.Runtime.CompilerServices.Nullable(2)] IWhiteToken errorToken)
		{
			_erroneousStatements = erroneousStatements;
			ErrorMessage = errorMessage;
			ErrorToken = errorToken;
			TokenList = new IWhiteToken[0];
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
			if (_erroneousStatements != null)
			{
				List<INode> list = new List<INode>();
				{
					foreach (IWhiteStatement erroneousStatement in _erroneousStatements)
					{
						list.AddRange(erroneousStatement.GetChildren());
					}
					return list;
				}
			}
			return TokenList;
		}
	}
}
