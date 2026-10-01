using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.Legacy
{
	// Token: 0x02000278 RID: 632
	public class FastIsUpToDateStrategy : _IIsUpTopDateStrategy
	{
		// Token: 0x17000BE1 RID: 3041
		// (get) Token: 0x06002A60 RID: 10848 RVA: 0x0006D2B7 File Offset: 0x0006C2B7
		public IList<string> Changes { get; } = new List<string>();

		// Token: 0x17000BE2 RID: 3042
		// (get) Token: 0x06002A61 RID: 10849 RVA: 0x0006D2BF File Offset: 0x0006C2BF
		public IList<IChangedLMObject> DetailedChanges { get; }

		// Token: 0x06002A62 RID: 10850 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public void CheckNextPOU()
		{
		}

		// Token: 0x17000BE3 RID: 3043
		// (get) Token: 0x06002A63 RID: 10851 RVA: 0x0006D2C7 File Offset: 0x0006C2C7
		// (set) Token: 0x06002A64 RID: 10852 RVA: 0x0006D2CF File Offset: 0x0006C2CF
		public bool IsUpToDate { get; private set; }

		// Token: 0x17000BE4 RID: 3044
		// (get) Token: 0x06002A65 RID: 10853 RVA: 0x0006D2D8 File Offset: 0x0006C2D8
		// (set) Token: 0x06002A66 RID: 10854 RVA: 0x0006D2E0 File Offset: 0x0006C2E0
		public bool FastOnlineChangePossible { get; private set; }

		// Token: 0x17000BE5 RID: 3045
		// (get) Token: 0x06002A67 RID: 10855 RVA: 0x0006D2E9 File Offset: 0x0006C2E9
		// (set) Token: 0x06002A68 RID: 10856 RVA: 0x0006D2F1 File Offset: 0x0006C2F1
		public bool OnlineChangePossible { get; private set; }

		// Token: 0x06002A69 RID: 10857 RVA: 0x0006D2FA File Offset: 0x0006C2FA
		public bool AdditionalSignInPrecompile(_ISignature sign, _ISignature signCompiled)
		{
			return this.ReturnTrueAndSetIsUpToDateToFalse();
		}

		// Token: 0x06002A6A RID: 10858 RVA: 0x0006D2FA File Offset: 0x0006C2FA
		public bool AdditionalSignInPool(_ISignature signPool, _ISignature signCompiled)
		{
			return this.ReturnTrueAndSetIsUpToDateToFalse();
		}

		// Token: 0x06002A6B RID: 10859 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public void CheckGlobalOnlineChangePreConditions()
		{
		}

		// Token: 0x06002A6C RID: 10860 RVA: 0x0006D2FA File Offset: 0x0006C2FA
		public bool CompiledPouChanged(_ISignature sign, _ICompiledPOU cpou, _ICompiledPOU cpouPrecomp)
		{
			return this.ReturnTrueAndSetIsUpToDateToFalse();
		}

		// Token: 0x06002A6D RID: 10861 RVA: 0x0006D2FA File Offset: 0x0006C2FA
		public bool CompileOptionsChanged()
		{
			return this.ReturnTrueAndSetIsUpToDateToFalse();
		}

		// Token: 0x06002A6E RID: 10862 RVA: 0x0006D2FA File Offset: 0x0006C2FA
		public bool DefinesChanged()
		{
			return this.ReturnTrueAndSetIsUpToDateToFalse();
		}

		// Token: 0x06002A6F RID: 10863 RVA: 0x0006D2FA File Offset: 0x0006C2FA
		public bool ExternalSignatureFlagChanged()
		{
			return this.ReturnTrueAndSetIsUpToDateToFalse();
		}

		// Token: 0x06002A70 RID: 10864 RVA: 0x0006D2FA File Offset: 0x0006C2FA
		public bool GlobalError()
		{
			return this.ReturnTrueAndSetIsUpToDateToFalse();
		}

		// Token: 0x06002A71 RID: 10865 RVA: 0x0006D2FA File Offset: 0x0006C2FA
		public bool LibraryListChanged()
		{
			return this.ReturnTrueAndSetIsUpToDateToFalse();
		}

		// Token: 0x06002A72 RID: 10866 RVA: 0x0006D2FA File Offset: 0x0006C2FA
		public bool LibraryParamTablesChanged()
		{
			return this.ReturnTrueAndSetIsUpToDateToFalse();
		}

		// Token: 0x06002A73 RID: 10867 RVA: 0x0006D2FA File Offset: 0x0006C2FA
		public bool MemorySettingsChanged()
		{
			return this.ReturnTrueAndSetIsUpToDateToFalse();
		}

		// Token: 0x06002A74 RID: 10868 RVA: 0x0006D2FA File Offset: 0x0006C2FA
		public bool ParentContextChanged()
		{
			return this.ReturnTrueAndSetIsUpToDateToFalse();
		}

		// Token: 0x06002A75 RID: 10869 RVA: 0x0006D2FA File Offset: 0x0006C2FA
		public bool ParentContextNull()
		{
			return this.ReturnTrueAndSetIsUpToDateToFalse();
		}

		// Token: 0x06002A76 RID: 10870 RVA: 0x0006D2FA File Offset: 0x0006C2FA
		public bool SignatureChanged(_ISignature sign, _ISignature signPrecom)
		{
			return this.ReturnTrueAndSetIsUpToDateToFalse();
		}

		// Token: 0x06002A77 RID: 10871 RVA: 0x0006D2FA File Offset: 0x0006C2FA
		public bool SignatureChangedByChecksumAttribute(_ISignature sign, _ISignature signPrecom)
		{
			return this.ReturnTrueAndSetIsUpToDateToFalse();
		}

		// Token: 0x06002A78 RID: 10872 RVA: 0x0006D2FA File Offset: 0x0006C2FA
		public bool SimulationModeChanged()
		{
			return this.ReturnTrueAndSetIsUpToDateToFalse();
		}

		// Token: 0x06002A79 RID: 10873 RVA: 0x0006D2FA File Offset: 0x0006C2FA
		public bool SubSignaturesChanged(_ISignature sign)
		{
			return this.ReturnTrueAndSetIsUpToDateToFalse();
		}

		// Token: 0x06002A7A RID: 10874 RVA: 0x0006D2FA File Offset: 0x0006C2FA
		public bool PrecomNameHashChanged()
		{
			return this.ReturnTrueAndSetIsUpToDateToFalse();
		}

		// Token: 0x06002A7B RID: 10875 RVA: 0x0006D302 File Offset: 0x0006C302
		private bool ReturnTrueAndSetIsUpToDateToFalse()
		{
			this.IsUpToDate = false;
			return true;
		}
	}
}
