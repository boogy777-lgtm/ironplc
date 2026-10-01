using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	[System.Runtime.CompilerServices.NullableContext(1)]
	[System.Runtime.CompilerServices.Nullable(0)]
	public class DirectVariableToken : WhiteToken, IDirectVariableToken, IWhiteToken, INode
	{
		public object Overflow { get; }

		public int[] Components { get; }

		public DirectVariableSize Size { get; }

		public DirectVariableLocation Location { get; }

		public override WhiteTokenType Type => WhiteTokenType.DirectVariable;

		public DirectVariableToken(string stText, DirectVariableLocation location, DirectVariableSize size, int[] components, bool bOverflow)
			: base(stText)
		{
			Location = location;
			Size = size;
			Components = components;
			Overflow = bOverflow;
		}
	}
}
