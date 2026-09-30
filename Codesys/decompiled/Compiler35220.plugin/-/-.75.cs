using System;
using _3S.CoDeSys.Core.LanguageModel;

namespace \u0008
{
	// Token: 0x02000102 RID: 258
	internal sealed class \u0006 : IIdentifierInfo2, IIdentifierInfo
	{
		// Token: 0x0600132F RID: 4911 RVA: 0x000350AC File Offset: 0x000332AC
		public \u0006(string \u0017\u0002, string \u0018\u0002, IdentifierInfoFlag \u0019\u0002, IType \u0017)
		{
			this.\u0001 = \u0019\u0002;
			this.\u0001 = \u0017\u0002;
			this.\u0002 = \u0018\u0002;
			this.\u0001 = \u0017;
		}

		// Token: 0x06001330 RID: 4912 RVA: 0x000350E8 File Offset: 0x000332E8
		public \u0006(IVariable \u001A\u0002, ISignature \u001B\u0002, string \u0017\u0002, string \u0018\u0002, IdentifierInfoFlag \u0019\u0002, IType \u0017)
		{
			this.\u0001 = \u0019\u0002;
			this.\u0001 = \u0017\u0002;
			this.\u0002 = \u0018\u0002;
			this.\u0001 = \u0017;
			this.\u0001 = \u001A\u0002;
			this.\u0002 = \u001B\u0002;
		}

		// Token: 0x06001331 RID: 4913 RVA: 0x00035140 File Offset: 0x00033340
		public \u0006(ISignature \u001C\u0002, string \u0017\u0002, string \u0018\u0002, IdentifierInfoFlag \u0019\u0002, IType \u0017)
		{
			this.\u0001 = \u0019\u0002;
			this.\u0001 = \u0017\u0002;
			this.\u0002 = \u0018\u0002;
			this.\u0001 = \u0017;
			this.\u0001 = \u001C\u0002;
		}

		// Token: 0x170004BD RID: 1213
		// (get) Token: 0x06001332 RID: 4914 RVA: 0x00035190 File Offset: 0x00033390
		// (set) Token: 0x06001333 RID: 4915 RVA: 0x00035198 File Offset: 0x00033398
		public string Name
		{
			get
			{
				return this.\u0001;
			}
			set
			{
				this.\u0001 = value;
			}
		}

		// Token: 0x170004BE RID: 1214
		// (get) Token: 0x06001334 RID: 4916 RVA: 0x000351A4 File Offset: 0x000333A4
		// (set) Token: 0x06001335 RID: 4917 RVA: 0x000351BC File Offset: 0x000333BC
		public string Comment
		{
			get
			{
				if (this.\u0002 == null)
				{
					return string.Empty;
				}
				return this.\u0002;
			}
			set
			{
				this.\u0002 = value;
			}
		}

		// Token: 0x170004BF RID: 1215
		// (get) Token: 0x06001336 RID: 4918 RVA: 0x000351C8 File Offset: 0x000333C8
		// (set) Token: 0x06001337 RID: 4919 RVA: 0x000351D0 File Offset: 0x000333D0
		public IdentifierInfoFlag Flags
		{
			get
			{
				return this.\u0001;
			}
			set
			{
				this.\u0001 = value;
			}
		}

		// Token: 0x170004C0 RID: 1216
		// (get) Token: 0x06001338 RID: 4920 RVA: 0x000351DC File Offset: 0x000333DC
		// (set) Token: 0x06001339 RID: 4921 RVA: 0x000351E4 File Offset: 0x000333E4
		public IType Type
		{
			get
			{
				return this.\u0001;
			}
			set
			{
				this.\u0001 = value;
			}
		}

		// Token: 0x170004C1 RID: 1217
		// (get) Token: 0x0600133A RID: 4922 RVA: 0x000351F0 File Offset: 0x000333F0
		// (set) Token: 0x0600133B RID: 4923 RVA: 0x000351F8 File Offset: 0x000333F8
		public IVariable Variable
		{
			get
			{
				return this.\u0001;
			}
			set
			{
				this.\u0001 = value;
			}
		}

		// Token: 0x170004C2 RID: 1218
		// (get) Token: 0x0600133C RID: 4924 RVA: 0x00035204 File Offset: 0x00033404
		// (set) Token: 0x0600133D RID: 4925 RVA: 0x0003520C File Offset: 0x0003340C
		public ISignature Signature
		{
			get
			{
				return this.\u0001;
			}
			set
			{
				this.\u0001 = value;
			}
		}

		// Token: 0x170004C3 RID: 1219
		// (get) Token: 0x0600133E RID: 4926 RVA: 0x00035218 File Offset: 0x00033418
		public IScope Scope
		{
			get
			{
				return null;
			}
		}

		// Token: 0x170004C4 RID: 1220
		// (get) Token: 0x0600133F RID: 4927 RVA: 0x0003521C File Offset: 0x0003341C
		public ISignature ContainingSignature
		{
			get
			{
				return this.\u0002;
			}
		}

		// Token: 0x04000330 RID: 816
		private string \u0001 = string.Empty;

		// Token: 0x04000331 RID: 817
		private string \u0002 = string.Empty;

		// Token: 0x04000332 RID: 818
		private IdentifierInfoFlag \u0001;

		// Token: 0x04000333 RID: 819
		private IType \u0001;

		// Token: 0x04000334 RID: 820
		private IVariable \u0001;

		// Token: 0x04000335 RID: 821
		private ISignature \u0001;

		// Token: 0x04000336 RID: 822
		private ISignature \u0002;
	}
}
