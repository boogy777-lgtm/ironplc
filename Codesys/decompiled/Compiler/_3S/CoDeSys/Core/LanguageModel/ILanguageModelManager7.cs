using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILanguageModelManager7 : ILanguageModelManager6, ILanguageModelManager5, ILanguageModelManager4, ILanguageModelManager3, ILanguageModelManager2, ILanguageModelManager
	{
		event AddImplicitCodeEventHandler AddDownloadCode;

		event AddImplicitCodeEventHandler AddGlobalInitCode;

		event AddImplicitCodeEventHandler AddOnlineChangeCode;

		IVariable GetVariableCompiled(string stVariableName);

		string GetDefaultInitializationCode(IVariable var, ISignature sign, IScope scope, string stInstancePath);

		void UpdateDownloadContextSynchWriteContext(Guid guidApplication);
	}
}
