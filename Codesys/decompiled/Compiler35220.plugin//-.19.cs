using System;
using \u0004;
using \u0018;
using _3S.CoDeSys.Compiler35220.Phase3_Location;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using \u0084;

namespace \u0080
{
	// Token: 0x02000386 RID: 902
	internal sealed class \u0018 : \u001E
	{
		// Token: 0x0600349A RID: 13466 RVA: 0x000CF7EC File Offset: 0x000CD9EC
		public bool \u0001(global::\u0018.\u0010 \u0002)
		{
			_ISignature isignature = \u0002.ComconNew.GetSignature(IdentifierConstants.OnlineChangePOUName) as _ISignature;
			_ICompiledPOU cpou = \u0002.ComconNew.GetCompiledPOUById(isignature.Id) as _ICompiledPOU;
			\u0002.ComconNew.RemoveSignature(isignature);
			\u0002.ComconNew.RemoveCompiledPOU(cpou);
			global::\u0004.\u0014.\u0001(\u0002.ComconNew, \u0002.ComconOld, false);
			_ICompiledPOU icompiledPOU = \u0002.ComconNew.GetCompiledPOUById(isignature.Id) as _ICompiledPOU;
			\u0002.compiledpous.Add(icompiledPOU);
			_ISignature u = \u0002.ComconNew.GetSignature(IdentifierConstants.OnlineChangePOUName) as _ISignature;
			Locator.\u0001(\u0002.ComconNew, u, isignature);
			return true;
		}
	}
}
