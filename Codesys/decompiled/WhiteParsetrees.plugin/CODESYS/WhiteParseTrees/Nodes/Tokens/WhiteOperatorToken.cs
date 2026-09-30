using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public abstract class WhiteOperatorToken : WhiteToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public Operator Operator { get; set; }

		[System.Runtime.CompilerServices.NullableContext(1)]
		protected WhiteOperatorToken(string stText, Operator op)
			: base(stText)
		{
			Operator = op;
		}
	}
}
