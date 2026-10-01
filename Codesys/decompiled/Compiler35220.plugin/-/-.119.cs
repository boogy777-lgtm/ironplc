using System;
using System.Runtime.CompilerServices;
using CODESYS.Parser;
using _3S.CoDeSys.Core.LanguageModel;
using \u0084;

namespace \u0016
{
	// Token: 0x02000163 RID: 355
	internal sealed class \u0007 : IOverflowChecker
	{
		// Token: 0x17000581 RID: 1409
		// (get) Token: 0x06001855 RID: 6229 RVA: 0x0004BFB4 File Offset: 0x0004A1B4
		public static IOverflowChecker Singleton { get; } = new \u0007();

		// Token: 0x06001856 RID: 6230 RVA: 0x0004BFBC File Offset: 0x0004A1BC
		public bool \u0001(TypeClass \u0002, bool \u0003, ulong \u0004)
		{
			return \u0004.\u0001(\u0002, \u0003, \u0004);
		}

		// Token: 0x0400044B RID: 1099
		[CompilerGenerated]
		private static readonly IOverflowChecker \u0001;
	}
}
