using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using \u000F;
using \u0014;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0084
{
	// Token: 0x02000123 RID: 291
	internal sealed class \u0006 : global::\u000F.\u0006
	{
		// Token: 0x170004FB RID: 1275
		// (get) Token: 0x060014B6 RID: 5302 RVA: 0x0003D2F0 File Offset: 0x0003B4F0
		internal static global::\u000F.\u0006 Instance { get; } = new \u0084.\u0006();

		// Token: 0x060014B7 RID: 5303 RVA: 0x0003D2F8 File Offset: 0x0003B4F8
		private \u0006()
		{
		}

		// Token: 0x060014B8 RID: 5304 RVA: 0x0003D300 File Offset: 0x0003B500
		public bool \u0001(global::\u0014.\u0003 \u0002)
		{
			return !\u0002.DetailedPrecompileCheck || !\u0084.\u0006.\u0001(\u0002) || \u0002.Strategy.IsUpToDate;
		}

		// Token: 0x060014B9 RID: 5305 RVA: 0x0003D320 File Offset: 0x0003B520
		private static bool \u0001(global::\u0014.\u0003 \u0002)
		{
			using (IEnumerator<_ISignature> enumerator = \u0002.PreCompileContext.AllSignatures.OfType<_ISignature>().GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (\u0084.\u0006.\u0001(enumerator.Current, \u0002))
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x060014BA RID: 5306 RVA: 0x0003D380 File Offset: 0x0003B580
		private static bool \u0001(_ISignature \u0002, global::\u0014.\u0003 \u0003)
		{
			bool flag = \u0002.GetFlag(SignatureFlag.SuperGlobal) && !\u0002.GetFlag(SignatureFlag.InhibitOnlineChange);
			if (flag && \u0002.HasAttribute(CompileAttributes.ATTRIBUTE_CHECKSUPERGLOBAL))
			{
				flag = false;
			}
			if (\u0002.GetFlag(SignatureFlag.Generated) || flag)
			{
				return false;
			}
			_ISignature isignature = \u0003.CompileContext[\u0002.GetSearchName(\u0003.CompileContext)];
			if (isignature == null && \u0002.HasFlag(SignatureFlag.TimeStampOnly))
			{
				return false;
			}
			if (isignature == null || isignature.Name != \u0002.Name)
			{
				isignature = \u0003.CompileContext[\u0002.Name];
			}
			if (isignature != null && isignature.GetFlag(SignatureFlag.Generated))
			{
				\u0002.SetFlag(SignatureFlag.Generated, true);
				return false;
			}
			return global::\u0014.\u0004.\u0001(isignature, \u0002) && \u0003.Strategy.AdditionalSignInPrecompile(\u0002, isignature);
		}

		// Token: 0x04000398 RID: 920
		[CompilerGenerated]
		private static readonly global::\u000F.\u0006 \u0001;
	}
}
