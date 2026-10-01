using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class PropertyToken : WhiteOperatorToken, IPropertyToken, IPouTypeToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.Property;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public PropertyToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
