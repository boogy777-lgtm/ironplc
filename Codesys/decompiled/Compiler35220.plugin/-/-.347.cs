using System;
using \u0003;
using \u001C;
using _3S.CoDeSys.Compiler35220.InitialisationCode;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0015
{
	// Token: 0x0200038B RID: 907
	internal sealed class \u0007 : \u001C.\u0012
	{
		// Token: 0x060034AF RID: 13487 RVA: 0x000CFC1C File Offset: 0x000CDE1C
		public bool \u0001(\u0016 \u0002)
		{
			if (!\u0002.signChanges.RegenerateInit)
			{
				return true;
			}
			_ISignature initFunction = \u0002.focContext.ComconNew.GetInitFunction(\u0002.sign);
			if (initFunction == null)
			{
				return false;
			}
			_ICompiledPOU icompiledPOU = \u0002.focContext.ComconNew.GetCompiledPOUById(initFunction.Id) as _ICompiledPOU;
			if (icompiledPOU == null)
			{
				return true;
			}
			if (\u0002.signRef == null)
			{
				return false;
			}
			\u0002.focContext.ComconNew.InFastOnlineChange = true;
			bool flag;
			_ICompiledPOU icompiledPOU2 = GVLInitialisationFunctionCreator.\u0001(\u0002.focContext.ComconNew, \u0002.signCompiled, \u0002.signRef, initFunction, \u0002.focContext.ComconOld, out flag);
			\u0002.focContext.ComconNew.InFastOnlineChange = false;
			if (flag)
			{
				return false;
			}
			\u0002.signCompiled.AddAttribute(CompileAttributes.ATTRIBUTE_GEN_IMPLICIT_INIT_FUN, null);
			icompiledPOU2.SignatureId = icompiledPOU.SignatureId;
			icompiledPOU2.SetFlag(CompiledPOUFlags.ToCompile, true);
			\u0002.focContext.ComconNew.RemoveCompiledPOU(icompiledPOU);
			\u0002.focContext.ComconNew.AddCompiledPOUSimple(icompiledPOU2);
			\u0002.focContext.changedpous[icompiledPOU2.SignatureId] = icompiledPOU;
			\u0002.focContext.compiledpous.Add(icompiledPOU2);
			return true;
		}
	}
}
