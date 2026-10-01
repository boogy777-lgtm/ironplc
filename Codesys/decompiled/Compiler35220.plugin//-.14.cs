using System;
using \u0004;
using \u0018;
using _3S.CoDeSys.Compiler35220.CompilerPhases;
using _3S.CoDeSys.Compiler35220.ImplicitCode;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using \u0084;

namespace \u0082
{
	// Token: 0x02000373 RID: 883
	internal sealed class \u0013 : \u001E
	{
		// Token: 0x06003441 RID: 13377 RVA: 0x000CD8A0 File Offset: 0x000CBAA0
		public bool \u0001(global::\u0018.\u0010 \u0002)
		{
			_ISignature sign = \u0002.ComconNew[IdentifierConstants.GlobalImplicitFunctionPointers];
			\u0002.ComconNew.RemoveSignature(sign);
			_ISignature isignature = \u0002.ComconNew.GetSignature("GLOBAL__COPY__CODE") as _ISignature;
			if (isignature != null)
			{
				_ICompiledPOU cpou = \u0002.ComconNew.GetCompiledPOUById(isignature.Id) as _ICompiledPOU;
				\u0002.ComconNew.RemoveSignature(isignature);
				\u0002.ComconNew.RemoveCompiledPOU(cpou);
			}
			if (\u0002.\u0001)
			{
				global::\u0004.\u0014.\u0001(\u0002.ComconNew, \u0002.ComconOld);
			}
			CodeInitGenerator.\u0001(\u0002.ComconNew, \u0002.ComconOld);
			CompilerPhase5_Codegenerator.\u0001(\u0002.ComconNew, \u0002.ComconOld, true);
			return true;
		}
	}
}
