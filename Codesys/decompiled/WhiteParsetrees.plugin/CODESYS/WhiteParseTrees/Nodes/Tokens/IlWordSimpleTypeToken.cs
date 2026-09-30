using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class IlWordSimpleTypeToken : WhiteOperatorToken, ILWordSimpleTypeToken, IWhiteSimpleTypeToken, IWhiteTypeToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.LWord;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public IlWordSimpleTypeToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
