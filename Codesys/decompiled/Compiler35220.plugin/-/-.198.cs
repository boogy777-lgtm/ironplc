using System;
using System.Runtime.CompilerServices;
using \u0011;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0018
{
	// Token: 0x02000233 RID: 563
	internal sealed class \u0004 : \u0005, IExprInfo, IAddressExprInfo
	{
		// Token: 0x06002545 RID: 9541 RVA: 0x00081BA8 File Offset: 0x0007FDA8
		private \u0004()
		{
		}

		// Token: 0x06002546 RID: 9542 RVA: 0x00081BBC File Offset: 0x0007FDBC
		public static \u0004 \u0001()
		{
			return new \u0004();
		}

		// Token: 0x170006C8 RID: 1736
		// (get) Token: 0x06002547 RID: 9543 RVA: 0x00081BC4 File Offset: 0x0007FDC4
		// (set) Token: 0x06002548 RID: 9544 RVA: 0x00081BCC File Offset: 0x0007FDCC
		public IDataLocation DataLocation { get; set; }

		// Token: 0x170006C9 RID: 1737
		// (get) Token: 0x06002549 RID: 9545 RVA: 0x00081BD8 File Offset: 0x0007FDD8
		public IAccessMode AccessMode
		{
			get
			{
				return this._AccessMode;
			}
		}

		// Token: 0x170006CA RID: 1738
		// (get) Token: 0x0600254A RID: 9546 RVA: 0x00081BE0 File Offset: 0x0007FDE0
		private \u0008 _AccessMode { get; } = new \u0008();

		// Token: 0x0600254B RID: 9547 RVA: 0x00081BE8 File Offset: 0x0007FDE8
		public bool \u0001(AccessModeFlags \u0002)
		{
			return this._AccessMode.\u0001(\u0002);
		}

		// Token: 0x0600254C RID: 9548 RVA: 0x00081BF8 File Offset: 0x0007FDF8
		public bool \u0002(AccessModeFlags \u0002)
		{
			return this._AccessMode.\u0002(\u0002);
		}

		// Token: 0x0600254D RID: 9549 RVA: 0x00081C08 File Offset: 0x0007FE08
		public void \u0001(AccessModeFlags \u0002, bool \u0003)
		{
			this._AccessMode.\u0001(\u0002, \u0003);
		}

		// Token: 0x170006CB RID: 1739
		// (get) Token: 0x0600254E RID: 9550 RVA: 0x00081C18 File Offset: 0x0007FE18
		// (set) Token: 0x0600254F RID: 9551 RVA: 0x00081C20 File Offset: 0x0007FE20
		public ICompiledType CompiledType { get; set; }

		// Token: 0x170006CC RID: 1740
		// (get) Token: 0x06002550 RID: 9552 RVA: 0x00081C2C File Offset: 0x0007FE2C
		// (set) Token: 0x06002551 RID: 9553 RVA: 0x00081C34 File Offset: 0x0007FE34
		public _IAddressExpression WithoutBit { get; set; }

		// Token: 0x040006AC RID: 1708
		[CompilerGenerated]
		private IDataLocation \u0001;

		// Token: 0x040006AD RID: 1709
		[CompilerGenerated]
		private readonly \u0008 \u0001;

		// Token: 0x040006AE RID: 1710
		[CompilerGenerated]
		private ICompiledType \u0001;

		// Token: 0x040006AF RID: 1711
		[CompilerGenerated]
		private _IAddressExpression \u0001;
	}
}
