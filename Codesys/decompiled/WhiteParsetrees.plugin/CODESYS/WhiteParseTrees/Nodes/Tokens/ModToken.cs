using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class ModToken : WhiteOperatorToken, IModToken, IWhiteInfixOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.Mod;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public ModToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
