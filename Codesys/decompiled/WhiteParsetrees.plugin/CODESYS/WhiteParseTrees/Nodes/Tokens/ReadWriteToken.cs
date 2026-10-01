using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class ReadWriteToken : WhiteOperatorToken, IReadWriteToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.ReadWrite;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public ReadWriteToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
