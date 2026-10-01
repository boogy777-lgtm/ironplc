using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.Services
{
	// Token: 0x0200024F RID: 591
	internal class PreCompileStorageSizeEstimatorService : ILMPreCompileStorageSizeEstimatorService
	{
		// Token: 0x0600279C RID: 10140 RVA: 0x00063772 File Offset: 0x00062772
		public int CalculatePointerSize(Guid guidApplication)
		{
			return Help.CalculatePointerSize(guidApplication);
		}

		// Token: 0x0600279D RID: 10141 RVA: 0x0006377A File Offset: 0x0006277A
		public int CalculateSignatureSize(ILMPreCompileSet preCompileSet, ISignature3 sign)
		{
			return ((_IPreCompileContext)preCompileSet).CalculateSignatureSize(sign);
		}

		// Token: 0x0600279E RID: 10142 RVA: 0x00063788 File Offset: 0x00062788
		public int CalculateTypeSize(ILMPreCompileSet preCompileSet, ISignature sign, IType type)
		{
			return ((_IPreCompileContext)preCompileSet).CalculateTypeSize(sign, type);
		}

		// Token: 0x0600279F RID: 10143 RVA: 0x00063797 File Offset: 0x00062797
		public bool CalculateVariableSizes(ILMPreCompileSet precompileSet, IList<ISignature> signaturelist, IList<IVariable> varlist, out IList<int> sizes)
		{
			return ((_IPreCompileContext)precompileSet).CalculateVariableSizes(signaturelist, varlist, out sizes);
		}

		// Token: 0x060027A0 RID: 10144 RVA: 0x000637A8 File Offset: 0x000627A8
		public int GetGranularity(ILMPreCompileSet precom, ICompiledType type)
		{
			IPrecompileScope4 prescope = (IPrecompileScope4)((_IPreCompileContext)precom).CreatePrecompileScope(Guid.Empty);
			return GranularityCalculator.GetGranularity(type, 0, prescope);
		}
	}
}
