using System;
using System.Runtime.CompilerServices;
using \u000E;
using _3S.CoDeSys.Compiler35220.Features;
using _3S.CoDeSys.Compiler35220.Phase1_Typification;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using \u0083;

namespace \u001A
{
	// Token: 0x0200031F RID: 799
	internal sealed class \u0013
	{
		// Token: 0x170007F0 RID: 2032
		// (get) Token: 0x06002FD2 RID: 12242 RVA: 0x000B4B78 File Offset: 0x000B2D78
		private \u001B CompileInformation { get; }

		// Token: 0x170007F1 RID: 2033
		// (get) Token: 0x06002FD3 RID: 12243 RVA: 0x000B4B80 File Offset: 0x000B2D80
		private SignatureChecker SignatureChecker { get; }

		// Token: 0x170007F2 RID: 2034
		// (get) Token: 0x06002FD4 RID: 12244 RVA: 0x000B4B88 File Offset: 0x000B2D88
		internal ConstGenericController ConstGenericController { get; }

		// Token: 0x06002FD5 RID: 12245 RVA: 0x000B4B90 File Offset: 0x000B2D90
		internal \u0013(\u001B \u0082\u0005)
		{
			this.CompileInformation = \u0082\u0005;
			this.SignatureChecker = new SignatureChecker(\u0082\u0005);
			this.ConstGenericController = new ConstGenericController(\u0082\u0005);
		}

		// Token: 0x06002FD6 RID: 12246 RVA: 0x000B4BB8 File Offset: 0x000B2DB8
		internal bool \u0001(_ISignature \u0002, IScope5 \u0003)
		{
			this.SignatureChecker.\u0001(\u0002, \u0003);
			this.SignatureChecker.\u0002(\u0002, \u0003);
			this.SignatureChecker.\u0001(\u0002, \u0003);
			return true;
		}

		// Token: 0x06002FD7 RID: 12247 RVA: 0x000B4BE4 File Offset: 0x000B2DE4
		internal bool \u0002(_ISignature \u0002, IScope5 \u0003)
		{
			return this.SignatureChecker.\u0001(\u0002, \u0003);
		}

		// Token: 0x06002FD8 RID: 12248 RVA: 0x000B4BF4 File Offset: 0x000B2DF4
		internal void \u0001(_ISignature \u0002, IScope5 \u0003)
		{
			this.SignatureChecker.\u0001(\u0002, \u0003);
		}

		// Token: 0x06002FD9 RID: 12249 RVA: 0x000B4C04 File Offset: 0x000B2E04
		internal void \u0002(_ISignature \u0002, IScope5 \u0003)
		{
			this.SignatureChecker.\u0002(\u0002, \u0003);
		}

		// Token: 0x06002FDA RID: 12250 RVA: 0x000B4C14 File Offset: 0x000B2E14
		internal bool \u0003(_ISignature \u0002, IScope5 \u0003)
		{
			return new \u0083.\u0008(\u0002, this.CompileInformation).\u0001(\u0003);
		}

		// Token: 0x06002FDB RID: 12251 RVA: 0x000B4C28 File Offset: 0x000B2E28
		internal bool \u0004(_ISignature \u0002, IScope5 \u0003)
		{
			return new \u0083.\u0008(\u0002, this.CompileInformation).\u0003(\u0003);
		}

		// Token: 0x06002FDC RID: 12252 RVA: 0x000B4C3C File Offset: 0x000B2E3C
		internal bool \u0005(_ISignature \u0002, IScope5 \u0003)
		{
			return new \u0083.\u0008(\u0002, this.CompileInformation).\u0002(\u0003);
		}

		// Token: 0x04000921 RID: 2337
		[CompilerGenerated]
		private readonly \u001B \u0001;

		// Token: 0x04000922 RID: 2338
		[CompilerGenerated]
		private readonly SignatureChecker \u0001;

		// Token: 0x04000923 RID: 2339
		[CompilerGenerated]
		private readonly ConstGenericController \u0001;
	}
}
