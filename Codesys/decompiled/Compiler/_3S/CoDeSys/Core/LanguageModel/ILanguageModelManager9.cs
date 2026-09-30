using System;
using System.IO;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Objects;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILanguageModelManager9 : ILanguageModelManager8, ILanguageModelManager7, ILanguageModelManager6, ILanguageModelManager5, ILanguageModelManager4, ILanguageModelManager3, ILanguageModelManager2, ILanguageModelManager
	{
		int PendingLibrariesInLoadQueue { get; }

		IExpressionTypifier CreateTypifier(Guid guidApplication, int idSignature, bool bContributeToCompile, bool bInterpretPragmas);

		void EnqueueSetLibraryPreCompileContextFromArchive(Stream stream, Guid readerGuid, string stLibraryId, ISharedDataStorage sharedDataStorage, SetLibraryPreCompileContextCompletionEventHandler completionEventHandler, object callerData);

		void ProcessQueuedLibraryPreCompileContexts(bool bWaitForCompletion);

		ISignature[] FindSignaturesByName(int nProjectHandle, Guid applicationGuid, Guid callingObjectGuid, string stName);

		IVarRef GetVarReference(Guid guidApplication, string stExpression, bool bAllowShortExpressions);

		void SimulationModeChanged(Guid guidDevice);
	}
}
