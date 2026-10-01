using System;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x020000B2 RID: 178
	internal class CheckedPrecompileSignatureManager
	{
		// Token: 0x17000272 RID: 626
		// (get) Token: 0x06000A5C RID: 2652 RVA: 0x00017A31 File Offset: 0x00016A31
		// (set) Token: 0x06000A5D RID: 2653 RVA: 0x00017A39 File Offset: 0x00016A39
		public bool AnySignDirty { get; set; } = true;

		// Token: 0x17000273 RID: 627
		// (get) Token: 0x06000A5E RID: 2654 RVA: 0x00017A42 File Offset: 0x00016A42
		// (set) Token: 0x06000A5F RID: 2655 RVA: 0x00017A4A File Offset: 0x00016A4A
		private bool AllSignsDirty { get; set; }

		// Token: 0x06000A60 RID: 2656 RVA: 0x00017A53 File Offset: 0x00016A53
		public CheckedPrecompileSignatureManager(_IPreCompileContext precom)
		{
			this._precom.SetTarget(precom);
		}

		// Token: 0x06000A61 RID: 2657 RVA: 0x00017A88 File Offset: 0x00016A88
		internal void SetAllSignsUnchecked()
		{
			object flags = this._flags;
			lock (flags)
			{
				if (!this.AllSignsDirty)
				{
					_IPreCompileContext ipreCompileContext;
					if (this._precom.TryGetTarget(out ipreCompileContext))
					{
						foreach (_ICompiledPOU icompiledPOU in ipreCompileContext.AllCompiledPOUs)
						{
							icompiledPOU.SetFlagInternal(InternalCompiledPOUFlags.Checked, false);
						}
						foreach (_ISignature isignature in ipreCompileContext.AllFlat)
						{
							isignature.SetFlagInternal(SignatureFlagInternal.Checked | SignatureFlagInternal.ReadyToCheck, false);
						}
						this.AllSignsDirty = true;
					}
				}
			}
		}

		// Token: 0x06000A62 RID: 2658 RVA: 0x00017B60 File Offset: 0x00016B60
		public void SetSignatureChecked(_ISignature sign)
		{
			object flags = this._flags;
			lock (flags)
			{
				sign.SetFlagInternal(SignatureFlagInternal.Checked, true);
				this.AllSignsDirty = false;
			}
		}

		// Token: 0x06000A63 RID: 2659 RVA: 0x00017BAC File Offset: 0x00016BAC
		public void SetPouChecked(_ICompiledPOU cpou)
		{
			object flags = this._flags;
			lock (flags)
			{
				cpou.SetFlagInternal(InternalCompiledPOUFlags.Checked, true);
				this.AllSignsDirty = false;
			}
		}

		// Token: 0x04000182 RID: 386
		private readonly WeakReference<_IPreCompileContext> _precom = new WeakReference<_IPreCompileContext>(null);

		// Token: 0x04000183 RID: 387
		private readonly object _flags = new object();
	}
}
