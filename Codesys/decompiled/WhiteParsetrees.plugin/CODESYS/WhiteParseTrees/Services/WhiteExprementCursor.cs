using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace CODESYS.WhiteParseTrees.Services
{
	[System.Runtime.CompilerServices.NullableContext(1)]
	[System.Runtime.CompilerServices.Nullable(0)]
	internal sealed class WhiteExprementCursor : IWhiteCursor
	{
		public int ChildIndex;

		[System.Runtime.CompilerServices.Nullable(new byte[] { 2, 1 })]
		private IWhiteCursor[] _Children;

		public IWhiteExprement Node { get; }

		INode IWhiteCursor.Node => Node;

		public int Length { get; }

		public int Offset { get; }

		[System.Runtime.CompilerServices.Nullable(2)]
		[field: System.Runtime.CompilerServices.Nullable(2)]
		public WhiteExprementCursor Parent
		{
			[System.Runtime.CompilerServices.NullableContext(2)]
			get;
		}

		public IWhiteCursor[] Children => _Children ?? (_Children = CreateChildren());

		public static WhiteExprementCursor FromRoot(IWhiteExprement root)
		{
			return new WhiteExprementCursor(null, root, -1, 0);
		}

		private WhiteExprementCursor([System.Runtime.CompilerServices.Nullable(2)] WhiteExprementCursor parent, IWhiteExprement node, int childIndex, int offset)
		{
			Node = node ?? throw new ArgumentNullException("node");
			Length = node.GetTextLength();
			Offset = offset;
			Parent = parent;
			ChildIndex = childIndex;
		}

		private IWhiteCursor[] CreateChildren()
		{
			INode[] array = Node.GetChildren().ToArray();
			IWhiteCursor[] array2 = new IWhiteCursor[array.Length];
			int num = Offset;
			for (int i = 0; i < array.Length; i++)
			{
				int num2 = i;
				INode node = array[i];
				IWhiteCursor whiteCursor;
				if (!(node is IWhiteToken token))
				{
					if (!(node is IWhiteExprement node2))
					{
						throw new InvalidOperationException($"Unexpected node type {array[i].GetType()}");
					}
					whiteCursor = new WhiteExprementCursor(this, node2, i, num);
				}
				else
				{
					whiteCursor = new WhiteTokenCursor(this, token, num, null);
				}
				array2[num2] = whiteCursor;
				num += array2[i].Length;
			}
			return array2;
		}

		[System.Runtime.CompilerServices.NullableContext(2)]
		public WhiteTokenCursor GetTokenAtOffset(int offset)
		{
			if (offset >= Offset && offset < Offset + Length)
			{
				IWhiteCursor[] children = Children;
				for (int i = 0; i < children.Length; i++)
				{
					WhiteTokenCursor tokenAtOffset = children[i].GetTokenAtOffset(offset);
					if (tokenAtOffset != null)
					{
						return tokenAtOffset;
					}
				}
			}
			return null;
		}

		public override string ToString()
		{
			return Node.ToString();
		}
	}
}
