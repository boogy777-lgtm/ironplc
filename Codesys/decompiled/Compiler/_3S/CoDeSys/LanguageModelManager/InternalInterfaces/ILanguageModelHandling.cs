using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface ILanguageModelHandling
	{
		void AddImplicitLanguageModel(_ILanguageModelManagerConsolidated lmm);

		void AddLanguageModel(_ILanguageModelManagerConsolidated lmm, string stContribution, bool bExternal, string stCompilerDefines, Guid guidApplication, Guid guidLanguageModelControl, string stLibraryPath, bool bEnableSystemCall, bool bShowSyntaxErrors, SignatureFlag defaultFlag, IList<IList<string>> strStringListTable);

		void AddStructuredLanguageModel(_ILanguageModelManagerConsolidated lmm, ILanguageModel lm, bool bExternal, string stCompilerDefines, bool bEnableSystemCall, SignatureFlag sfDefaultFlag, bool bShowSyntaxErrors);

		ILanguageModel CreateLanguageModelOfXml(string stContribution, IList<IList<string>> stringlistTable);
	}
}
