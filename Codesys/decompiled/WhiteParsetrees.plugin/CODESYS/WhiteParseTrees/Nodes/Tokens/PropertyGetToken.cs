using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class PropertyGetToken : WhiteOperatorToken, IPropertyGetToken2, IPropertyGetToken, IWhiteToken, INode, IPouTypeToken, IWhiteOperatorToken
	{
		public override WhiteTokenType Type => WhiteTokenType.PropertyGet;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public PropertyGetToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
