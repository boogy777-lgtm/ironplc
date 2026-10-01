using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0001
{
	// Token: 0x020003C2 RID: 962
	internal static class \u0015
	{
		// Token: 0x060036CF RID: 14031 RVA: 0x000DED7C File Offset: 0x000DCF7C
		internal static void \u0001(_ISignature \u0002, _ISignature \u0003, _ICompileContext \u0004, _ICompileContext \u0005)
		{
			if (\u0004.MinimalSystem)
			{
				return;
			}
			_ISignature isignature = null;
			if (\u0003 != null)
			{
				isignature = (\u0003.GetSubSignature(IdentifierConstants.VFInitMethodName) as _ISignature);
			}
			_ISignature isignature2 = \u0013.VFInitMethod.CreateCompiledSignature(isignature, \u0004.HasByteSupport());
			isignature2.ParentObjectGuid = \u0002.ObjectGuid;
			isignature2.ParentSignatureId = \u0002.Id;
			\u0004.AddSignature(isignature2, isignature, \u0005, true);
			\u0002.AddSubSignature(isignature2);
			isignature2.SetFlag(SignatureFlag.NonVirtual, true);
		}
	}
}
