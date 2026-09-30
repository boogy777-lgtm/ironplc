using System;
using System.Runtime.CompilerServices;
using \u0003;
using \u0011;
using _3S.CoDeSys.Core.LanguageModel;

namespace \u001B
{
	// Token: 0x02000139 RID: 313
	internal sealed class \u0003 : global::\u0003.\u0005
	{
		// Token: 0x060015DA RID: 5594 RVA: 0x000400D0 File Offset: 0x0003E2D0
		public \u0003(string \u0089\u0002, string \u0084\u0007, Version \u0099\u0007, AttributeScope \u009B\u0002, long \u009B\u0007, long \u009C\u0007)
		{
			\u001B.\u0003.\u0001 u = new \u001B.\u0003.\u0001();
			u.\u0001 = \u0089\u0002;
			u.\u0001 = \u009B\u0007;
			u.\u0002 = \u009C\u0007;
			base..ctor(u.\u0001, \u0084\u0007, \u0099\u0007, \u009B\u0002, new Func<string, AttributeScope, ISignature, IVariable, string>(u.\u0001));
		}

		// Token: 0x0200013A RID: 314
		[CompilerGenerated]
		private new sealed class \u0001
		{
			// Token: 0x060015DC RID: 5596 RVA: 0x00040120 File Offset: 0x0003E320
			internal string \u0001(string \u0002, AttributeScope \u0003, ISignature \u0004, IVariable \u0005)
			{
				long num;
				if (!long.TryParse(\u0002, out num))
				{
					return string.Format(global::\u0011.\u0001.AttributeInvalidTypeInteger, \u0002, this.\u0001);
				}
				if (num < this.\u0001 || num > this.\u0002)
				{
					return string.Format(global::\u0011.\u0001.AttributeInvalidRange, new object[]
					{
						\u0002,
						this.\u0001,
						this.\u0001,
						this.\u0002
					});
				}
				return string.Empty;
			}

			// Token: 0x040003CD RID: 973
			public string \u0001;

			// Token: 0x040003CE RID: 974
			public long \u0001;

			// Token: 0x040003CF RID: 975
			public long \u0002;
		}
	}
}
