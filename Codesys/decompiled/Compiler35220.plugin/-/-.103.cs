using System;
using _3S.CoDeSys.Core.LanguageModel;

namespace \u0007
{
	// Token: 0x02000134 RID: 308
	internal sealed class \u0004 : IAttribute
	{
		// Token: 0x060015CC RID: 5580 RVA: 0x0003FF44 File Offset: 0x0003E144
		public \u0004(string \u0089\u0002)
		{
			this.\u0001 = \u0089\u0002;
		}

		// Token: 0x17000522 RID: 1314
		// (get) Token: 0x060015CD RID: 5581 RVA: 0x0003FF54 File Offset: 0x0003E154
		public string Name
		{
			get
			{
				return this.\u0001;
			}
		}

		// Token: 0x040003C3 RID: 963
		private readonly string \u0001;
	}
}
