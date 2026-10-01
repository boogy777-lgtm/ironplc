using System.Runtime.CompilerServices;

namespace CODESYS.WhiteParseTrees.Services.Formatter.Passes
{
	[System.Runtime.CompilerServices.NullableContext(1)]
	public interface IExpressionFormattingPass
	{
		void Format(INode node, IFormatterSettings settings, int indentation);
	}
}
