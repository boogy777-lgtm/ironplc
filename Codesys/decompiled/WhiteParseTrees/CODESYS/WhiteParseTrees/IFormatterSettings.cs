using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[ReleasedInterface]
	public interface IFormatterSettings
	{
		int MaxCalleeParamsBeforeBreak { get; set; }

		int MaxCalleeParamsCharCountBeforeBreak { get; set; }

		bool KeepLeadingEmptyLine { get; set; }

		bool AlignVariableDeclarationInitializations { get; set; }

		bool AlignVariableDeclarationTrailingComments { get; set; }

		bool AlignVariableDeclarationsToLongestName { get; set; }
	}
}
