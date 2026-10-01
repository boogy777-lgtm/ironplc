using System;
using System.Collections;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.Services
{
	// Token: 0x0200024B RID: 587
	public class ObsoleteService : ILMObsoleteService2, ILMObsoleteService
	{
		// Token: 0x06002728 RID: 10024 RVA: 0x00062650 File Offset: 0x00061650
		public IScope CreateScope(ISignature isign, ICollection Signatures, Guid guidApplication)
		{
			_ICompileContext icompileContext = APEnvironmentFacade.Instance.LanguageModelMgr[guidApplication];
			if (icompileContext == null)
			{
				return null;
			}
			return CompilerProxy.CreateScope(icompileContext, isign.Id);
		}

		// Token: 0x06002729 RID: 10025 RVA: 0x000478AA File Offset: 0x000468AA
		public bool IsHiddenVariable(IVariable variable, GUIHidingFlags flagsToConsider)
		{
			return APEnvironmentFacade.Instance.LanguageModelMgr.IsHiddenVariable(null, variable, flagsToConsider);
		}

		// Token: 0x0600272A RID: 10026 RVA: 0x00062680 File Offset: 0x00061680
		public IExpressionTypifier CreateTypifier(Guid guidApplication, int idSignature, bool bContributeToCompile, bool bInterpretPragmas)
		{
			_ICompileContext icompileContext = null;
			_ICompiledPOU cpou = null;
			if (!object.Equals(guidApplication, Guid.Empty))
			{
				icompileContext = APEnvironmentFacade.Instance.LanguageModelMgr[guidApplication];
				if (icompileContext == null)
				{
					return null;
				}
				cpou = icompileContext._GetCompiledPOUById(idSignature);
			}
			return CompilerProxy.CreateTypifier(idSignature, icompileContext, null, bInterpretPragmas, bContributeToCompile, false, cpou);
		}
	}
}
