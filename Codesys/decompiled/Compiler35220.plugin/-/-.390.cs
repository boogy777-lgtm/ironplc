using System;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using \u0080;

namespace \u0015
{
	// Token: 0x020003E5 RID: 997
	internal sealed class \u000E : \u001B
	{
		// Token: 0x1700090C RID: 2316
		// (get) Token: 0x06003781 RID: 14209 RVA: 0x000E44A8 File Offset: 0x000E26A8
		private int[] AreaStartAddresses { get; }

		// Token: 0x06003782 RID: 14210 RVA: 0x000E44B0 File Offset: 0x000E26B0
		public \u000E(int[] \u008D\u0005)
		{
			this.AreaStartAddresses = \u008D\u0005;
		}

		// Token: 0x06003783 RID: 14211 RVA: 0x000E44C0 File Offset: 0x000E26C0
		public int \u0001(_IArea \u0002)
		{
			return this.AreaStartAddresses[\u0002.Index];
		}

		// Token: 0x04000AE8 RID: 2792
		[CompilerGenerated]
		private readonly int[] \u0001;
	}
}
