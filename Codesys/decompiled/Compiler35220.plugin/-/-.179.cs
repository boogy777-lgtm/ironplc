using System;
using System.Diagnostics;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0003
{
	// Token: 0x020001EC RID: 492
	[DebuggerDisplay("({_stInstancePath}, {_sign.OrgName}, {_var.OrgName}, OnStack: {_bStackVariable}")]
	internal sealed class \u000E
	{
		// Token: 0x06002195 RID: 8597 RVA: 0x00073A48 File Offset: 0x00071C48
		internal \u000E(string \u0003\u0004, IVariable \u001A\u0002, ISignature \u001C\u0002, bool \u0004\u0004)
		{
			this.\u0001 = \u0003\u0004;
			this.\u0001 = \u001A\u0002;
			this.\u0001 = \u001C\u0002;
			this.\u0001 = \u0004\u0004;
		}

		// Token: 0x1700064F RID: 1615
		// (get) Token: 0x06002196 RID: 8598 RVA: 0x00073A70 File Offset: 0x00071C70
		internal string InstancePath
		{
			get
			{
				return this.\u0001;
			}
		}

		// Token: 0x17000650 RID: 1616
		// (get) Token: 0x06002197 RID: 8599 RVA: 0x00073A78 File Offset: 0x00071C78
		internal IVariable Variable
		{
			get
			{
				return this.\u0001;
			}
		}

		// Token: 0x17000651 RID: 1617
		// (get) Token: 0x06002198 RID: 8600 RVA: 0x00073A80 File Offset: 0x00071C80
		internal _ISignature Signature
		{
			get
			{
				return this.\u0001 as _ISignature;
			}
		}

		// Token: 0x17000652 RID: 1618
		// (get) Token: 0x06002199 RID: 8601 RVA: 0x00073A90 File Offset: 0x00071C90
		internal bool IsStackVariable
		{
			get
			{
				return this.\u0001;
			}
		}

		// Token: 0x040005B8 RID: 1464
		private string \u0001;

		// Token: 0x040005B9 RID: 1465
		private IVariable \u0001;

		// Token: 0x040005BA RID: 1466
		private ISignature \u0001;

		// Token: 0x040005BB RID: 1467
		private bool \u0001;
	}
}
