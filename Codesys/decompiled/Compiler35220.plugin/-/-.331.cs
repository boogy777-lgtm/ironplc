using System;
using \u0018;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using \u0084;

namespace \u000F
{
	// Token: 0x02000375 RID: 885
	internal sealed class \u0017 : \u001E
	{
		// Token: 0x06003445 RID: 13381 RVA: 0x000CD990 File Offset: 0x000CBB90
		public bool \u0001(\u0018.\u0010 \u0002)
		{
			_ISignature isignature = \u0002.ComconNew.GetSignature(IdentifierConstants.GlobalImplicitSignature) as _ISignature;
			_ICompiledPOU cpou = \u0002.ComconNew.GetCompiledPOUById(isignature.Id) as _ICompiledPOU;
			\u0002.ComconNew.RemoveSignature(isignature);
			\u0002.ComconNew.RemoveCompiledPOU(cpou);
			_ISignature isignature2 = \u0002.ComconNew.GetSignature(IdentifierConstants.RelocateCodeName) as _ISignature;
			_ICompiledPOU cpou2 = \u0002.ComconNew.GetCompiledPOUById(isignature2.Id) as _ICompiledPOU;
			\u0002.ComconNew.RemoveSignature(isignature2);
			\u0002.ComconNew.RemoveCompiledPOU(cpou2);
			_ISignature isignature3 = \u0002.ComconNew.GetSignature("__GLOBAL_RELOC_DEFINITIONS") as _ISignature;
			_ICompiledPOU cpou3 = \u0002.ComconNew.GetCompiledPOUById(isignature3.Id) as _ICompiledPOU;
			\u0002.ComconNew.RemoveSignature(isignature3);
			\u0002.ComconNew.RemoveCompiledPOU(cpou3);
			return true;
		}
	}
}
