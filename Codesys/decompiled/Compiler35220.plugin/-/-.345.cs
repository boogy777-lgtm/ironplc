using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using \u0003;
using \u0010;
using \u0013;
using \u0015;
using \u001C;
using \u001E;
using _3S.CoDeSys.Compiler35220.OnlineChange.FastOnlinechange.Steps.ChangedSignatureProcessor;

namespace \u0014
{
	// Token: 0x02000389 RID: 905
	internal sealed class \u0014
	{
		// Token: 0x170008B0 RID: 2224
		// (get) Token: 0x060034A9 RID: 13481 RVA: 0x000CF9CC File Offset: 0x000CDBCC
		internal static global::\u0014.\u0014 Instance { get; } = new global::\u0014.\u0014();

		// Token: 0x060034AA RID: 13482 RVA: 0x000CF9D4 File Offset: 0x000CDBD4
		private \u0014()
		{
		}

		// Token: 0x170008B1 RID: 2225
		// (get) Token: 0x060034AB RID: 13483 RVA: 0x000CF9DC File Offset: 0x000CDBDC
		internal IEnumerable<\u001C.\u0012> ChangedSignatureProcessorSteps
		{
			get
			{
				return global::\u0014.\u0014.\u0001;
			}
		}

		// Token: 0x04000A48 RID: 2632
		[CompilerGenerated]
		private static readonly global::\u0014.\u0014 \u0001;

		// Token: 0x04000A49 RID: 2633
		private static \u001C.\u0012[] \u0001 = new \u001C.\u0012[]
		{
			new global::\u0003.\u0013(),
			new global::\u0013.\u000F(),
			new NewLocalOffsetsCalculator(),
			new global::\u0015.\u0007(),
			new global::\u0010.\u000F(),
			new \u001E.\u0017(),
			new \u001E.\u0015()
		};
	}
}
