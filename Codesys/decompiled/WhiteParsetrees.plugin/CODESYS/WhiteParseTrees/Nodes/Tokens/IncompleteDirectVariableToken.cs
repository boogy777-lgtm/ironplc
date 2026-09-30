using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class IncompleteDirectVariableToken : WhiteToken, IIncompleteDirectVariableToken, IWhiteToken, INode
	{
		public DirectVariableLocation Location { get; }

		public override WhiteTokenType Type => WhiteTokenType.IncompleteDirectVariable;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public IncompleteDirectVariableToken(string stText, DirectVariableLocation location)
			: base(stText)
		{
			Location = location;
		}
	}
}
