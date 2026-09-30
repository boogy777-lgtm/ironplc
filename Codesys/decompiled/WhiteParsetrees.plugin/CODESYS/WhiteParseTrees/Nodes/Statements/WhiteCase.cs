using System.Runtime.CompilerServices;

namespace CODESYS.WhiteParseTrees.Nodes.Statements
{
	[System.Runtime.CompilerServices.NullableContext(1)]
	[System.Runtime.CompilerServices.Nullable(0)]
	public class WhiteCase : IWhiteCase
	{
		public IWhiteCaseLabelStatement Label { get; set; }

		public IWhiteSequenceStatement Controlled { get; set; }

		public WhiteCase(IWhiteCaseLabelStatement label, IWhiteSequenceStatement controlled)
		{
			Label = label;
			Controlled = controlled;
		}
	}
}
