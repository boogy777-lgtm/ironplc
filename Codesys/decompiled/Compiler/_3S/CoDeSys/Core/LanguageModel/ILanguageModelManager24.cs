using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILanguageModelManager24 : ILanguageModelManager23, ILanguageModelManager22, ILanguageModelManager21
	{
		bool PrecompileInformationUpToDate { get; }

		event EventHandler<AfterGenerateGlobalInitEventArgs> AfterGenerateGlobalInitCode;

		event EventHandler<CompileEventArgs> BeforeGenerateCompiledCode;

		bool IsHiddenVariable(ISignature6 signature, IVariable variable, GUIHidingFlags flagsToConsider, ISignature signCurrent);

		IList<IDirectVariableAccess> GetAllDirectVariableAccesses();
	}
}
