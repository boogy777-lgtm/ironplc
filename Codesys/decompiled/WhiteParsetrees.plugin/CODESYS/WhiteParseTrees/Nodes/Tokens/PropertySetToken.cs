using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class PropertySetToken : WhiteOperatorToken, IPropertySetToken2, IPropertySetToken, IWhiteToken, INode, IPouTypeToken, IWhiteOperatorToken
	{
		public override WhiteTokenType Type => WhiteTokenType.PropertySet;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public PropertySetToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
