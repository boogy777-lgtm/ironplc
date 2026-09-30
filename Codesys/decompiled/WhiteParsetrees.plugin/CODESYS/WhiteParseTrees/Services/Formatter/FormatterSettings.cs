namespace CODESYS.WhiteParseTrees.Services.Formatter
{
	public class FormatterSettings : IFormatterSettings
	{
		public bool AlignVariableDeclarationsToLongestName { get; set; }

		public bool AlignVariableDeclarationInitializations { get; set; }

		public bool AlignVariableDeclarationTrailingComments { get; set; }

		public int MaxCalleeParamsBeforeBreak { get; set; } = int.MaxValue;


		public int MaxCalleeParamsCharCountBeforeBreak { get; set; } = int.MaxValue;


		public bool KeepLeadingEmptyLine { get; set; } = true;

	}
}
