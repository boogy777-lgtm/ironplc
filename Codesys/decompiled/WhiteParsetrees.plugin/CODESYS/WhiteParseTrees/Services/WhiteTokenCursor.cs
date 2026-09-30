using System;
using System.Runtime.CompilerServices;

namespace CODESYS.WhiteParseTrees.Services
{
	[System.Runtime.CompilerServices.NullableContext(2)]
	[System.Runtime.CompilerServices.Nullable(0)]
	internal sealed class WhiteTokenCursor : IWhiteCursor
	{
		[System.Runtime.CompilerServices.Nullable(1)]
		public readonly IWhiteToken Token;

		public readonly WhiteTokenCursor TrailingToken;

		private WhiteTokenCursor _LeadingToken;

		public int Offset { get; }

		public int Length => Token.Text.Length;

		[System.Runtime.CompilerServices.Nullable(1)]
		INode IWhiteCursor.Node
		{
			[System.Runtime.CompilerServices.NullableContext(1)]
			get
			{
				return Token;
			}
		}

		public WhiteExprementCursor Parent { get; }

		public WhiteTokenCursor LeadingToken => _LeadingToken ?? (_LeadingToken = CreateTokenCursor());

		public WhiteTokenCursor(WhiteExprementCursor parentNode, [System.Runtime.CompilerServices.Nullable(1)] IWhiteToken token, int offset, WhiteTokenCursor trailingToken)
		{
			Offset = offset;
			Token = token ?? throw new ArgumentNullException("token");
			TrailingToken = trailingToken;
			Parent = parentNode;
		}

		private WhiteTokenCursor CreateTokenCursor()
		{
			if (Token.Leading != null)
			{
				return new WhiteTokenCursor(Parent, Token.Leading, Offset - Token.Leading.Text.Length, this);
			}
			return null;
		}

		public WhiteTokenCursor GetTokenAtOffset(int offset)
		{
			if (offset < Offset || offset >= Offset + Length)
			{
				return LeadingToken?.GetTokenAtOffset(offset);
			}
			return this;
		}

		[System.Runtime.CompilerServices.NullableContext(1)]
		public override string ToString()
		{
			return Token.ToString();
		}
	}
}
