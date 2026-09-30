using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class TypeToken : WhiteOperatorToken, ITypeToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.Type;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public TypeToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
