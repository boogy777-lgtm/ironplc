using System;
using System.Linq;
using System.Runtime.CompilerServices;
using \u000F;
using \u0014;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0083
{
	// Token: 0x02000124 RID: 292
	internal sealed class \u0002 : global::\u000F.\u0006
	{
		// Token: 0x170004FC RID: 1276
		// (get) Token: 0x060014BC RID: 5308 RVA: 0x0003D46C File Offset: 0x0003B66C
		internal static global::\u000F.\u0006 Instance { get; } = new \u0083.\u0002();

		// Token: 0x060014BD RID: 5309 RVA: 0x0003D474 File Offset: 0x0003B674
		private \u0002()
		{
		}

		// Token: 0x060014BE RID: 5310 RVA: 0x0003D47C File Offset: 0x0003B67C
		public bool \u0001(global::\u0014.\u0003 \u0002)
		{
			return !\u0002.DetailedPrecompileCheck || !\u0083.\u0002.\u0001(\u0002) || \u0002.Strategy.IsUpToDate;
		}

		// Token: 0x060014BF RID: 5311 RVA: 0x0003D49C File Offset: 0x0003B69C
		private static bool \u0001(global::\u0014.\u0003 \u0002)
		{
			foreach (_ISignature isignature in \u0002.PreCompileContextPool.AllSignatures.OfType<_ISignature>())
			{
				_ISignature isignature2 = \u0002.CompileContext[isignature.ObjectGuid];
				if (isignature2 == null || isignature2.Name != isignature.Name)
				{
					isignature2 = \u0002.CompileContext[isignature.Name];
				}
				if ((isignature2 != null || isignature.GetFlag(SignatureFlag.TopLevel) || isignature.HasAttribute(CompileAttributes.ATTRIBUTE_LINK_ALWAYS)) && global::\u0014.\u0004.\u0001(isignature2, isignature) && \u0002.Strategy.AdditionalSignInPool(isignature, isignature2))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x04000399 RID: 921
		[CompilerGenerated]
		private static readonly global::\u000F.\u0006 \u0001;
	}
}
