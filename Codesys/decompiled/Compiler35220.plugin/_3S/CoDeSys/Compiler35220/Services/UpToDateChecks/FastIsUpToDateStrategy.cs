using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler35220.Services.UpToDateChecks
{
	// Token: 0x02000126 RID: 294
	public class FastIsUpToDateStrategy : _IIsUpTopDateStrategy
	{
		// Token: 0x17000502 RID: 1282
		// (get) Token: 0x060014CC RID: 5324 RVA: 0x0003D600 File Offset: 0x0003B800
		public IList<string> Changes { get; } = new List<string>();

		// Token: 0x17000503 RID: 1283
		// (get) Token: 0x060014CD RID: 5325 RVA: 0x0003D608 File Offset: 0x0003B808
		public IList<IChangedLMObject> DetailedChanges { get; }

		// Token: 0x060014CE RID: 5326 RVA: 0x0003D610 File Offset: 0x0003B810
		public void CheckNextPOU()
		{
		}

		// Token: 0x17000504 RID: 1284
		// (get) Token: 0x060014CF RID: 5327 RVA: 0x0003D614 File Offset: 0x0003B814
		// (set) Token: 0x060014D0 RID: 5328 RVA: 0x0003D61C File Offset: 0x0003B81C
		public bool IsUpToDate { get; private set; }

		// Token: 0x17000505 RID: 1285
		// (get) Token: 0x060014D1 RID: 5329 RVA: 0x0003D628 File Offset: 0x0003B828
		// (set) Token: 0x060014D2 RID: 5330 RVA: 0x0003D630 File Offset: 0x0003B830
		public bool FastOnlineChangePossible { get; private set; }

		// Token: 0x17000506 RID: 1286
		// (get) Token: 0x060014D3 RID: 5331 RVA: 0x0003D63C File Offset: 0x0003B83C
		// (set) Token: 0x060014D4 RID: 5332 RVA: 0x0003D644 File Offset: 0x0003B844
		public bool OnlineChangePossible { get; private set; }

		// Token: 0x060014D5 RID: 5333 RVA: 0x0003D650 File Offset: 0x0003B850
		public bool AdditionalSignInPrecompile(_ISignature sign, _ISignature signCompiled)
		{
			return this.\u0001();
		}

		// Token: 0x060014D6 RID: 5334 RVA: 0x0003D658 File Offset: 0x0003B858
		public bool AdditionalSignInPool(_ISignature signPool, _ISignature signCompiled)
		{
			return this.\u0001();
		}

		// Token: 0x060014D7 RID: 5335 RVA: 0x0003D660 File Offset: 0x0003B860
		public void CheckGlobalOnlineChangePreConditions()
		{
		}

		// Token: 0x060014D8 RID: 5336 RVA: 0x0003D664 File Offset: 0x0003B864
		public bool CompiledPouChanged(_ISignature sign, _ICompiledPOU cpou, _ICompiledPOU cpouPrecomp)
		{
			return this.\u0001();
		}

		// Token: 0x060014D9 RID: 5337 RVA: 0x0003D66C File Offset: 0x0003B86C
		public bool CompileOptionsChanged()
		{
			return this.\u0001();
		}

		// Token: 0x060014DA RID: 5338 RVA: 0x0003D674 File Offset: 0x0003B874
		public bool DefinesChanged()
		{
			return this.\u0001();
		}

		// Token: 0x060014DB RID: 5339 RVA: 0x0003D67C File Offset: 0x0003B87C
		public bool ExternalSignatureFlagChanged()
		{
			return this.\u0001();
		}

		// Token: 0x060014DC RID: 5340 RVA: 0x0003D684 File Offset: 0x0003B884
		public bool GlobalError()
		{
			return this.\u0001();
		}

		// Token: 0x060014DD RID: 5341 RVA: 0x0003D68C File Offset: 0x0003B88C
		public bool LibraryListChanged()
		{
			return this.\u0001();
		}

		// Token: 0x060014DE RID: 5342 RVA: 0x0003D694 File Offset: 0x0003B894
		public bool LibraryParamTablesChanged()
		{
			return this.\u0001();
		}

		// Token: 0x060014DF RID: 5343 RVA: 0x0003D69C File Offset: 0x0003B89C
		public bool MemorySettingsChanged()
		{
			return this.\u0001();
		}

		// Token: 0x060014E0 RID: 5344 RVA: 0x0003D6A4 File Offset: 0x0003B8A4
		public bool ParentContextChanged()
		{
			return this.\u0001();
		}

		// Token: 0x060014E1 RID: 5345 RVA: 0x0003D6AC File Offset: 0x0003B8AC
		public bool ParentContextNull()
		{
			return this.\u0001();
		}

		// Token: 0x060014E2 RID: 5346 RVA: 0x0003D6B4 File Offset: 0x0003B8B4
		public bool SignatureChanged(_ISignature sign, _ISignature signPrecom)
		{
			return this.\u0001();
		}

		// Token: 0x060014E3 RID: 5347 RVA: 0x0003D6BC File Offset: 0x0003B8BC
		public bool SignatureChangedByChecksumAttribute(_ISignature sign, _ISignature signPrecom)
		{
			return this.\u0001();
		}

		// Token: 0x060014E4 RID: 5348 RVA: 0x0003D6C4 File Offset: 0x0003B8C4
		public bool SimulationModeChanged()
		{
			return this.\u0001();
		}

		// Token: 0x060014E5 RID: 5349 RVA: 0x0003D6CC File Offset: 0x0003B8CC
		public bool SubSignaturesChanged(_ISignature sign)
		{
			return this.\u0001();
		}

		// Token: 0x060014E6 RID: 5350 RVA: 0x0003D6D4 File Offset: 0x0003B8D4
		public bool PrecomNameHashChanged()
		{
			return this.\u0001();
		}

		// Token: 0x060014E7 RID: 5351 RVA: 0x0003D6DC File Offset: 0x0003B8DC
		private bool \u0001()
		{
			this.IsUpToDate = false;
			return true;
		}

		// Token: 0x0400039F RID: 927
		[CompilerGenerated]
		private readonly IList<string> \u0001;

		// Token: 0x040003A0 RID: 928
		[CompilerGenerated]
		private readonly IList<IChangedLMObject> \u0001;

		// Token: 0x040003A1 RID: 929
		[CompilerGenerated]
		private bool \u0001;

		// Token: 0x040003A2 RID: 930
		[CompilerGenerated]
		private bool \u0002;

		// Token: 0x040003A3 RID: 931
		[CompilerGenerated]
		private bool \u0003;
	}
}
