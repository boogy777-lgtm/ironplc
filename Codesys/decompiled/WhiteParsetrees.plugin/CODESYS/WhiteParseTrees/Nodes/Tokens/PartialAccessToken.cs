using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class PartialAccessToken : WhiteToken, IPartialAccessToken, IWhiteToken, INode
	{
		[System.Runtime.CompilerServices.Nullable(1)]
		[field: System.Runtime.CompilerServices.Nullable(1)]
		public object Overflow
		{
			[System.Runtime.CompilerServices.NullableContext(1)]
			get;
		}

		public int Offset { get; }

		public DirectVariableSize Size { get; }

		public override WhiteTokenType Type => WhiteTokenType.PartialAccess;

		[System.Runtime.CompilerServices.NullableContext(1)]
		internal PartialAccessToken(string stText, DirectVariableSize size, int iOffset, bool bOverflow)
			: base(stText)
		{
			Size = size;
			Offset = iOffset;
			Overflow = bOverflow;
		}
	}
}
