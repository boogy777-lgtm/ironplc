using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CODESYS.WhiteParseTrees.Nodes.Statements
{
	[System.Runtime.CompilerServices.NullableContext(1)]
	[System.Runtime.CompilerServices.Nullable(0)]
	public class WhiteSequenceStatement : WhiteExprement, IWhiteSequenceStatement, IWhiteStatement, IWhiteExprement, INode, IStatementSyntax, IList<IWhiteStatement>, ICollection<IWhiteStatement>, IEnumerable<IWhiteStatement>, IEnumerable
	{
		private List<IWhiteStatement> Statements { get; set; } = new List<IWhiteStatement>();


		public int Count => Statements.Count;

		public bool IsReadOnly => false;

		public IWhiteStatement this[int index]
		{
			get
			{
				return Statements[index];
			}
			set
			{
				Statements[index] = value;
			}
		}

		public override IEnumerable<INode> GetChildren()
		{
			return Statements;
		}

		public void Accept(IStatementSyntax.IStatementVisitor visitor)
		{
			visitor.visit(this);
		}

		public T Accept<[System.Runtime.CompilerServices.Nullable(2)] T>(IStatementSyntax.IStatementVisitor<T> visitor)
		{
			return visitor.visit(this);
		}

		public T Accept<[System.Runtime.CompilerServices.Nullable(2)] T, [System.Runtime.CompilerServices.Nullable(2)] TContext>(IStatementSyntax.IStatementVisitor<T, TContext> visitor, TContext context)
		{
			return visitor.visit(this, context);
		}

		public IEnumerator<IWhiteStatement> GetEnumerator()
		{
			return Statements.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return Statements.GetEnumerator();
		}

		public void Add(IWhiteStatement item)
		{
			Statements.Add(item);
		}

		public void Clear()
		{
			Statements.Clear();
		}

		public bool Contains(IWhiteStatement item)
		{
			return Statements.Contains(item);
		}

		public void CopyTo(IWhiteStatement[] array, int arrayIndex)
		{
			Statements.CopyTo(array, arrayIndex);
		}

		public bool Remove(IWhiteStatement item)
		{
			return Statements.Remove(item);
		}

		public int IndexOf(IWhiteStatement item)
		{
			return Statements.IndexOf(item);
		}

		public void Insert(int index, IWhiteStatement item)
		{
			Statements.Insert(index, item);
		}

		public void RemoveAt(int index)
		{
			Statements.RemoveAt(index);
		}
	}
}
