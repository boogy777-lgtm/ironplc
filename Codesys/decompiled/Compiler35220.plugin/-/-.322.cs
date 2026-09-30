using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace \u0011
{
	// Token: 0x0200036A RID: 874
	internal sealed class \u0014
	{
		// Token: 0x1700089F RID: 2207
		// (get) Token: 0x06003424 RID: 13348 RVA: 0x000CD238 File Offset: 0x000CB438
		internal IList<_IVariable> DeletedVariables
		{
			get
			{
				LList<_IVariable> result;
				if ((result = this.\u0001) == null)
				{
					result = (this.\u0001 = new LList<_IVariable>());
				}
				return result;
			}
		}

		// Token: 0x170008A0 RID: 2208
		// (get) Token: 0x06003425 RID: 13349 RVA: 0x000CD260 File Offset: 0x000CB460
		// (set) Token: 0x06003426 RID: 13350 RVA: 0x000CD268 File Offset: 0x000CB468
		internal _ISignature CompiledSignature { get; set; }

		// Token: 0x170008A1 RID: 2209
		// (get) Token: 0x06003427 RID: 13351 RVA: 0x000CD274 File Offset: 0x000CB474
		// (set) Token: 0x06003428 RID: 13352 RVA: 0x000CD27C File Offset: 0x000CB47C
		internal _ISignature PrecompileSignature { get; set; }

		// Token: 0x170008A2 RID: 2210
		// (get) Token: 0x06003429 RID: 13353 RVA: 0x000CD288 File Offset: 0x000CB488
		internal bool RegenerateInit
		{
			get
			{
				return this.\u0010 || this.\u000E || this.\u000F;
			}
		}

		// Token: 0x170008A3 RID: 2211
		// (get) Token: 0x0600342A RID: 13354 RVA: 0x000CD2A4 File Offset: 0x000CB4A4
		internal bool RegenerateFbInit
		{
			get
			{
				return this.\u0008 || this.\u0006 || this.\u0007;
			}
		}

		// Token: 0x170008A4 RID: 2212
		// (get) Token: 0x0600342B RID: 13355 RVA: 0x000CD2C0 File Offset: 0x000CB4C0
		internal bool CheckInitialValues
		{
			get
			{
				return this.VariableAdded || this.\u0002 || this.\u0005 || this.\u0008 || this.\u0010;
			}
		}

		// Token: 0x170008A5 RID: 2213
		// (get) Token: 0x0600342C RID: 13356 RVA: 0x000CD2EC File Offset: 0x000CB4EC
		internal bool VariableAdded
		{
			get
			{
				return this.\u0003 || this.\u000E || this.\u0006;
			}
		}

		// Token: 0x170008A6 RID: 2214
		// (get) Token: 0x0600342D RID: 13357 RVA: 0x000CD308 File Offset: 0x000CB508
		internal bool VariableDeleted
		{
			get
			{
				return this.\u0004 || this.\u000F || this.\u0007;
			}
		}

		// Token: 0x04000A1C RID: 2588
		internal bool \u0001;

		// Token: 0x04000A1D RID: 2589
		internal bool \u0002;

		// Token: 0x04000A1E RID: 2590
		internal bool \u0003;

		// Token: 0x04000A1F RID: 2591
		internal bool \u0004;

		// Token: 0x04000A20 RID: 2592
		internal bool \u0005;

		// Token: 0x04000A21 RID: 2593
		internal bool \u0006;

		// Token: 0x04000A22 RID: 2594
		internal bool \u0007;

		// Token: 0x04000A23 RID: 2595
		internal bool \u0008;

		// Token: 0x04000A24 RID: 2596
		internal bool \u000E;

		// Token: 0x04000A25 RID: 2597
		internal bool \u000F;

		// Token: 0x04000A26 RID: 2598
		internal bool \u0010;

		// Token: 0x04000A27 RID: 2599
		internal bool \u0011;

		// Token: 0x04000A28 RID: 2600
		internal bool \u0012;

		// Token: 0x04000A29 RID: 2601
		internal bool \u0013;

		// Token: 0x04000A2A RID: 2602
		internal bool \u0014;

		// Token: 0x04000A2B RID: 2603
		internal LList<_IVariable> \u0001;

		// Token: 0x04000A2C RID: 2604
		internal bool \u0015;

		// Token: 0x04000A2D RID: 2605
		[CompilerGenerated]
		private _ISignature \u0001;

		// Token: 0x04000A2E RID: 2606
		[CompilerGenerated]
		private _ISignature \u0002;
	}
}
