using System.Runtime.CompilerServices;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class XByteStringToken : WhiteToken, IXByteStringToken, IWhiteToken, INode
	{
		[System.Runtime.CompilerServices.Nullable(1)]
		[field: System.Runtime.CompilerServices.Nullable(1)]
		public string MyString
		{
			[System.Runtime.CompilerServices.NullableContext(1)]
			get;
		}

		public override WhiteTokenType Type => WhiteTokenType.XByteString;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public XByteStringToken(string stText, string stringin)
			: base(stText)
		{
			MyString = stringin;
		}
	}
}
