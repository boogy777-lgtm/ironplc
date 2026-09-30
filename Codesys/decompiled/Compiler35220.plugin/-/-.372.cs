using System;
using \u0001;
using \u0008;
using \u0012;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0018
{
	// Token: 0x020003BE RID: 958
	internal static class \u0012
	{
		// Token: 0x060036C6 RID: 14022 RVA: 0x000DE7EC File Offset: 0x000DC9EC
		internal static void \u0001(_ISignature \u0002, _ISignature \u0003, _ICompileContext \u0004, _ICompileContext \u0005)
		{
			if (!\u0004.OnlineChangeSupported || !global::\u0012.\u0014.\u0002(\u0002))
			{
				return;
			}
			_ISignature signOld = ((\u0003 != null) ? \u0003.GetSubSignature(IdentifierConstants.PartialInitMethodName) : null) as _ISignature;
			global::\u0008.\u0012.\u0001(global::\u0001.\u0013.PartialInitMethodSignature.CreateCompiledSignature(signOld, \u0004.HasByteSupport()), \u0002, \u0003, \u0004, \u0005).SetFlag(SignatureFlag.Generated, true);
		}
	}
}
