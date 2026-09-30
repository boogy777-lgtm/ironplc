using System.Collections.Generic;
using System.Runtime.CompilerServices;
using CODESYS.WhiteParseTrees.Services;

namespace CODESYS.WhiteParseTrees.Nodes.Statements
{
	public abstract class WhiteExprement : IWhiteExprement, INode
	{
		[System.Runtime.CompilerServices.NullableContext(1)]
		public abstract IEnumerable<INode> GetChildren();

		[System.Runtime.CompilerServices.NullableContext(1)]
		public override string ToString()
		{
			return StringConverter.ConvertToString(this);
		}

		public int GetTextLength()
		{
			return TextLengthCalculator.CalculateTextLength(this);
		}

		public int GetTextLengthNetto()
		{
			return TextLengthCalculator.CalculateTextLengthWithoutLeadingWhitespace(this);
		}
	}
}
