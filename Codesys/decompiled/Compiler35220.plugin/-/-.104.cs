using System;
using _3S.CoDeSys.Core.LanguageModel;

namespace \u0003
{
	// Token: 0x02000135 RID: 309
	internal class \u0005 : IAttribute, ICheckedAttribute
	{
		// Token: 0x060015CE RID: 5582 RVA: 0x0003FF5C File Offset: 0x0003E15C
		public \u0005(string \u0089\u0002, string \u0084\u0007, Version \u0099\u0007, AttributeScope \u009B\u0002, Func<string, AttributeScope, ISignature, IVariable, string> \u009A\u0007)
		{
			this.\u0001 = \u0089\u0002;
			this.\u0002 = \u0084\u0007;
			this.\u0001 = \u0099\u0007;
			this.\u0001 = \u009B\u0002;
			this.\u0001 = \u009A\u0007;
		}

		// Token: 0x17000523 RID: 1315
		// (get) Token: 0x060015CF RID: 5583 RVA: 0x0003FF8C File Offset: 0x0003E18C
		public string Name
		{
			get
			{
				return this.\u0001;
			}
		}

		// Token: 0x17000524 RID: 1316
		// (get) Token: 0x060015D0 RID: 5584 RVA: 0x0003FF94 File Offset: 0x0003E194
		public string Description
		{
			get
			{
				return this.\u0002;
			}
		}

		// Token: 0x17000525 RID: 1317
		// (get) Token: 0x060015D1 RID: 5585 RVA: 0x0003FF9C File Offset: 0x0003E19C
		public Version RequiredCompilerVersion
		{
			get
			{
				return this.\u0001;
			}
		}

		// Token: 0x17000526 RID: 1318
		// (get) Token: 0x060015D2 RID: 5586 RVA: 0x0003FFA4 File Offset: 0x0003E1A4
		public AttributeScope Scope
		{
			get
			{
				return this.\u0001;
			}
		}

		// Token: 0x060015D3 RID: 5587 RVA: 0x0003FFAC File Offset: 0x0003E1AC
		public bool \u0001(string \u0002, AttributeScope \u0003, ISignature \u0004, IVariable \u0005, out string \u0006)
		{
			\u0006 = this.\u0001(\u0002, \u0003, \u0004, \u0005);
			return string.IsNullOrWhiteSpace(\u0006);
		}

		// Token: 0x040003C4 RID: 964
		private readonly string \u0001;

		// Token: 0x040003C5 RID: 965
		private readonly string \u0002;

		// Token: 0x040003C6 RID: 966
		private readonly Version \u0001;

		// Token: 0x040003C7 RID: 967
		private readonly AttributeScope \u0001;

		// Token: 0x040003C8 RID: 968
		private readonly Func<string, AttributeScope, ISignature, IVariable, string> \u0001;
	}
}
