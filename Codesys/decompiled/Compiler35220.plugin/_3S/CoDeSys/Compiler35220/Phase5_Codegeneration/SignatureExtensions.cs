using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler35220.Phase5_Codegeneration
{
	// Token: 0x02000247 RID: 583
	public static class SignatureExtensions
	{
		// Token: 0x06002659 RID: 9817 RVA: 0x00085DCC File Offset: 0x00083FCC
		public static bool CanSignatureBeVirtual(this _ISignature sign, IScope scope)
		{
			if (sign.POUType != Operator.Method)
			{
				return false;
			}
			if (sign.GetFlag(SignatureFlag.Final) || sign.GetFlag(SignatureFlag.Private))
			{
				return false;
			}
			if (sign.GetFlag(SignatureFlag.Action))
			{
				ISignature signature = scope[sign.ParentSignatureId];
				return signature == null || !signature.HasAttribute("no_virtual_actions");
			}
			return true;
		}

		// Token: 0x0600265A RID: 9818 RVA: 0x00085E3C File Offset: 0x0008403C
		public static bool IsMethodInInheritanceChainOf(this ISignature signToCompare, _ISignature sign, IScope5 scope)
		{
			while (signToCompare != null)
			{
				if (sign.ParentSignatureId == signToCompare.Id)
				{
					return true;
				}
				signToCompare = scope[signToCompare.BaseSignatureId];
			}
			return false;
		}
	}
}
