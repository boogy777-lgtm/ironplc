using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelUtilities.Legacy;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	public class LocalCodeWriter : ILocalCodeWriter
	{
		IPreCompileContext ILocalCodeWriter.LocalContext => LocalContext;

		public IPreCompileContext3 LocalContext { get; }

		public LocalCodeWriter(IPreCompileContext3 context)
		{
			LocalContext = context ?? throw new ArgumentNullException("context");
		}

		public string GetSignatureInterface(ISignature sign)
		{
			IPreCompileContext2 precompileContextOfSignature = APEnvironmentFacade.Instance.LanguageModelMgr.GetPrecompileContextOfSignature(sign);
			return TextualInterfaceCreator.CreateTextualInterface((ISignature2)sign, (IPreCompileContext3)precompileContextOfSignature, LocalContext);
		}

		public string GetTypeText(IType type, IPreCompileContext precomLib)
		{
			return LegacySwitch.GetQualifiedTypeText(type, (IPreCompileContext3)precomLib, LocalContext);
		}
	}
}
