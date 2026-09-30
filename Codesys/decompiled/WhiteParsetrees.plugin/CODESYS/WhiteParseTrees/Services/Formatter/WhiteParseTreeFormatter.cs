using System.Runtime.CompilerServices;
using CODESYS.WhiteParseTrees.Services.Formatter.Passes;

namespace CODESYS.WhiteParseTrees.Services.Formatter
{
	public class WhiteParseTreeFormatter : IWhiteParseTreeFormatter
	{
		[System.Runtime.CompilerServices.NullableContext(1)]
		public void RemoveAllWhitespaces(INode node, IFormatterSettings settings)
		{
			WhiteSpaceRemover.RemoveWhiteSpaces(node, settings);
		}

		[System.Runtime.CompilerServices.NullableContext(1)]
		public void FormatStatements(INode node, IFormatterSettings settings)
		{
			FormatterPipeline.Format(node, settings);
		}
	}
}
