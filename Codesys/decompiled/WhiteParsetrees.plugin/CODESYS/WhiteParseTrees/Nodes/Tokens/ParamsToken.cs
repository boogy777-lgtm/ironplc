using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class ParamsToken : WhiteOperatorToken, IParamsToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.Params;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public ParamsToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
