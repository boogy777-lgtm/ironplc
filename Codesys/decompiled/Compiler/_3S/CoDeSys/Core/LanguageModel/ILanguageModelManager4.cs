using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILanguageModelManager4 : ILanguageModelManager3, ILanguageModelManager2, ILanguageModelManager
	{
		IPreCompileContext2 GetPrecompileContextOfSignature(ISignature sign);

		ISignature2 GetBaseSignature(ISignature2 sign);

		ISignature2[] GetInterfaceSignatures(ISignature2 sign);

		IVariable2[] GetAllVariables(ISignature2 sign);

		ISignature2[] GetAllMethods(ISignature2 sign);

		ISignature2[] GetAllInterfaces(ISignature2 sign);

		IDownloadInfo GetOfflineBootProjectInfo(Guid guidApplication);

		IDownloadInfo GetOnlineBootProjectInfo(Guid guidApplication);
	}
}
