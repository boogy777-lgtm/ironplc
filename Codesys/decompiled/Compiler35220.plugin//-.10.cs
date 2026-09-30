using System;
using System.Collections.Generic;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace \u0083
{
	// Token: 0x020003CA RID: 970
	internal static class \u000F
	{
		// Token: 0x060036E4 RID: 14052 RVA: 0x000DFFF0 File Offset: 0x000DE1F0
		internal static IList<_ISignature> \u0001(_ICompileContext \u0002, out IList<IExpression> \u0003)
		{
			return \u000F.\u0001(\u0002, "call_before_global_exit_slot", out \u0003);
		}

		// Token: 0x060036E5 RID: 14053 RVA: 0x000E0000 File Offset: 0x000DE200
		internal static IList<_ISignature> \u0002(_ICompileContext \u0002, out IList<IExpression> \u0003)
		{
			return \u000F.\u0001(\u0002, "call_after_global_init_slot", out \u0003);
		}

		// Token: 0x060036E6 RID: 14054 RVA: 0x000E0010 File Offset: 0x000DE210
		internal static IList<_ISignature> \u0003(_ICompileContext \u0002, out IList<IExpression> \u0003)
		{
			return \u000F.\u0001(\u0002, "call_after_online_change_slot", out \u0003);
		}

		// Token: 0x060036E7 RID: 14055 RVA: 0x000E0020 File Offset: 0x000DE220
		internal static IList<_ISignature> \u0004(_ICompileContext \u0002, out IList<IExpression> \u0003)
		{
			return \u000F.\u0001(\u0002, "call_before_online_change_concurrent_slot", out \u0003);
		}

		// Token: 0x060036E8 RID: 14056 RVA: 0x000E0030 File Offset: 0x000DE230
		internal static IList<_ISignature> \u0005(_ICompileContext \u0002, out IList<IExpression> \u0003)
		{
			return \u000F.\u0001(\u0002, "call_after_online_change_concurrent_slot", out \u0003);
		}

		// Token: 0x060036E9 RID: 14057 RVA: 0x000E0040 File Offset: 0x000DE240
		internal static IList<_ISignature> \u0001(_ICompileContext \u0002, string \u0003, out IList<IExpression> \u0004)
		{
			LList<IExpression> llist;
			IList<_ISignature> result = Helper.\u0001(\u0002, \u0003, out llist);
			\u0004 = new LList<IExpression>(llist);
			return result;
		}
	}
}
