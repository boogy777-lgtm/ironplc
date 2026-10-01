using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0008
{
	// Token: 0x020003BC RID: 956
	internal static class \u0012
	{
		// Token: 0x060036C4 RID: 14020 RVA: 0x000DE6C8 File Offset: 0x000DC8C8
		internal static _ISignature \u0001(_ISignature \u0002, _ISignature \u0003, _ISignature \u0004, _ICompileContext \u0005, _ICompileContext \u0006)
		{
			_ISignature signRef = null;
			if (\u0004 != null)
			{
				signRef = (\u0004.GetSubSignature(\u0002.Name) as _ISignature);
			}
			\u0002.ParentObjectGuid = \u0003.ObjectGuid;
			\u0002.ParentSignatureId = \u0003.Id;
			\u0005.AddSignature(\u0002, signRef, \u0006, true);
			\u0003.AddSubSignature(\u0002);
			\u0002.SetFlag(SignatureFlag.NonVirtual, true);
			return \u0002;
		}
	}
}
