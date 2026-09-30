using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class StructToken : WhiteOperatorToken, IStructToken, IVarListStartToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.Struct;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public StructToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
