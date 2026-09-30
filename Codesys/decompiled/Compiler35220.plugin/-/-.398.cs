using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using \u0002;
using \u0004;
using \u0014;
using \u001A;
using _3S.CoDeSys.Compiler35220.CompilerPhases;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u000E
{
	// Token: 0x020003EE RID: 1006
	internal sealed class \u001B
	{
		// Token: 0x060037B8 RID: 14264 RVA: 0x000E4E40 File Offset: 0x000E3040
		internal \u001B(Guid \u0084\u0002, Guid \u008D\u0004, bool \u008A\u0004, bool \u008E\u0004, bool \u0014\u0004)
		{
			this.ApplicationGuid = \u0084\u0002;
			this.DeviceGuid = \u008D\u0004;
			this.OnlineChange = \u008A\u0004;
			this.BootProject = \u008E\u0004;
			this.KeepCompileInformation = \u0014\u0004;
			if (this.BootProject)
			{
				this.OnlineChange = true;
			}
			this.Timer = new global::\u0014.\u0001();
			this.CodeChanged = false;
			this.InterfacesChanged = false;
			this.InitValuesChanged = false;
			this.OnlineChangeDetails = null;
		}

		// Token: 0x060037B9 RID: 14265 RVA: 0x000E4EB0 File Offset: 0x000E30B0
		internal void \u0001()
		{
			Debug.\u0001(!\u001B.\u0001.ContainsKey(this.ApplicationGuid));
			\u001B.\u0001.Add(this.ApplicationGuid, new \u001A.\u0013(this));
		}

		// Token: 0x060037BA RID: 14266 RVA: 0x000E4EE0 File Offset: 0x000E30E0
		internal void \u0002()
		{
			Debug.\u0001(\u001B.\u0001.ContainsKey(this.ApplicationGuid));
			\u001B.\u0001.Remove(this.ApplicationGuid);
		}

		// Token: 0x1700091B RID: 2331
		// (get) Token: 0x060037BB RID: 14267 RVA: 0x000E4F08 File Offset: 0x000E3108
		// (set) Token: 0x060037BC RID: 14268 RVA: 0x000E4F10 File Offset: 0x000E3110
		internal _IPreCompileContext PrecompPool { get; set; }

		// Token: 0x1700091C RID: 2332
		// (get) Token: 0x060037BD RID: 14269 RVA: 0x000E4F1C File Offset: 0x000E311C
		internal Guid ApplicationGuid { get; }

		// Token: 0x1700091D RID: 2333
		// (get) Token: 0x060037BE RID: 14270 RVA: 0x000E4F24 File Offset: 0x000E3124
		internal Guid DeviceGuid { get; }

		// Token: 0x1700091E RID: 2334
		// (get) Token: 0x060037BF RID: 14271 RVA: 0x000E4F2C File Offset: 0x000E312C
		// (set) Token: 0x060037C0 RID: 14272 RVA: 0x000E4F34 File Offset: 0x000E3134
		internal Guid ParentApplicationGuid { get; set; }

		// Token: 0x1700091F RID: 2335
		// (get) Token: 0x060037C1 RID: 14273 RVA: 0x000E4F40 File Offset: 0x000E3140
		// (set) Token: 0x060037C2 RID: 14274 RVA: 0x000E4F48 File Offset: 0x000E3148
		internal bool OnlineChange { get; set; }

		// Token: 0x17000920 RID: 2336
		// (get) Token: 0x060037C3 RID: 14275 RVA: 0x000E4F54 File Offset: 0x000E3154
		internal bool BootProject { get; }

		// Token: 0x17000921 RID: 2337
		// (get) Token: 0x060037C4 RID: 14276 RVA: 0x000E4F5C File Offset: 0x000E315C
		internal bool KeepCompileInformation { get; }

		// Token: 0x17000922 RID: 2338
		// (get) Token: 0x060037C5 RID: 14277 RVA: 0x000E4F64 File Offset: 0x000E3164
		// (set) Token: 0x060037C6 RID: 14278 RVA: 0x000E4F6C File Offset: 0x000E316C
		internal bool CodeChanged { get; set; }

		// Token: 0x17000923 RID: 2339
		// (get) Token: 0x060037C7 RID: 14279 RVA: 0x000E4F78 File Offset: 0x000E3178
		// (set) Token: 0x060037C8 RID: 14280 RVA: 0x000E4F80 File Offset: 0x000E3180
		internal bool InterfacesChanged { get; set; }

		// Token: 0x17000924 RID: 2340
		// (get) Token: 0x060037C9 RID: 14281 RVA: 0x000E4F8C File Offset: 0x000E318C
		// (set) Token: 0x060037CA RID: 14282 RVA: 0x000E4F94 File Offset: 0x000E3194
		internal bool InitValuesChanged { get; set; }

		// Token: 0x17000925 RID: 2341
		// (get) Token: 0x060037CB RID: 14283 RVA: 0x000E4FA0 File Offset: 0x000E31A0
		// (set) Token: 0x060037CC RID: 14284 RVA: 0x000E4FA8 File Offset: 0x000E31A8
		internal IOnlineChangeDetails OnlineChangeDetails { get; set; }

		// Token: 0x060037CD RID: 14285 RVA: 0x000E4FB4 File Offset: 0x000E31B4
		internal static \u001A.\u0013 \u0001(Guid \u0002)
		{
			Debug.\u0001(\u001B.\u0001.ContainsKey(\u0002));
			return \u001B.\u0001[\u0002];
		}

		// Token: 0x17000926 RID: 2342
		// (get) Token: 0x060037CE RID: 14286 RVA: 0x000E4FD4 File Offset: 0x000E31D4
		internal \u001A.\u0013 InterfaceCompiler
		{
			get
			{
				return \u001B.\u0001(this.ApplicationGuid);
			}
		}

		// Token: 0x17000927 RID: 2343
		// (get) Token: 0x060037CF RID: 14287 RVA: 0x000E4FE4 File Offset: 0x000E31E4
		// (set) Token: 0x060037D0 RID: 14288 RVA: 0x000E4FEC File Offset: 0x000E31EC
		internal _ILanguageModelManagerConsolidated LMM { get; set; }

		// Token: 0x17000928 RID: 2344
		// (get) Token: 0x060037D1 RID: 14289 RVA: 0x000E4FF8 File Offset: 0x000E31F8
		// (set) Token: 0x060037D2 RID: 14290 RVA: 0x000E5000 File Offset: 0x000E3200
		internal _IPreCompileContext Precomp { get; set; }

		// Token: 0x17000929 RID: 2345
		// (get) Token: 0x060037D3 RID: 14291 RVA: 0x000E500C File Offset: 0x000E320C
		// (set) Token: 0x060037D4 RID: 14292 RVA: 0x000E5014 File Offset: 0x000E3214
		internal _ICompileContext ComconNew { get; set; }

		// Token: 0x1700092A RID: 2346
		// (get) Token: 0x060037D5 RID: 14293 RVA: 0x000E5020 File Offset: 0x000E3220
		// (set) Token: 0x060037D6 RID: 14294 RVA: 0x000E5048 File Offset: 0x000E3248
		internal _ICompileContext ComconOld
		{
			get
			{
				if (this.\u0003 == null)
				{
					_ICompileContext u = this.\u0002;
					this.\u0003 = ((u != null) ? u.Duplicate() : null);
				}
				return this.\u0003;
			}
			set
			{
				if (this.\u0002 != value)
				{
					this.\u0002 = value;
					this.\u0003 = null;
				}
			}
		}

		// Token: 0x1700092B RID: 2347
		// (get) Token: 0x060037D7 RID: 14295 RVA: 0x000E5064 File Offset: 0x000E3264
		// (set) Token: 0x060037D8 RID: 14296 RVA: 0x000E506C File Offset: 0x000E326C
		internal _ICompileContext ComconParent { get; set; }

		// Token: 0x1700092C RID: 2348
		// (get) Token: 0x060037D9 RID: 14297 RVA: 0x000E5078 File Offset: 0x000E3278
		// (set) Token: 0x060037DA RID: 14298 RVA: 0x000E5080 File Offset: 0x000E3280
		internal _ICompileContext ComconDevice { get; set; }

		// Token: 0x1700092D RID: 2349
		// (get) Token: 0x060037DB RID: 14299 RVA: 0x000E508C File Offset: 0x000E328C
		// (set) Token: 0x060037DC RID: 14300 RVA: 0x000E5094 File Offset: 0x000E3294
		internal IProgressCallback Callback { get; set; }

		// Token: 0x1700092E RID: 2350
		// (get) Token: 0x060037DD RID: 14301 RVA: 0x000E50A0 File Offset: 0x000E32A0
		// (set) Token: 0x060037DE RID: 14302 RVA: 0x000E50A8 File Offset: 0x000E32A8
		internal bool CheckAll { get; set; }

		// Token: 0x1700092F RID: 2351
		// (get) Token: 0x060037DF RID: 14303 RVA: 0x000E50B4 File Offset: 0x000E32B4
		// (set) Token: 0x060037E0 RID: 14304 RVA: 0x000E50BC File Offset: 0x000E32BC
		internal global::\u0014.\u0001 Timer { get; set; }

		// Token: 0x17000930 RID: 2352
		// (get) Token: 0x060037E1 RID: 14305 RVA: 0x000E50C8 File Offset: 0x000E32C8
		// (set) Token: 0x060037E2 RID: 14306 RVA: 0x000E50D0 File Offset: 0x000E32D0
		internal CompilerPhase1_Typifier CompilerPhase1_Typifier { get; set; }

		// Token: 0x17000931 RID: 2353
		// (get) Token: 0x060037E3 RID: 14307 RVA: 0x000E50DC File Offset: 0x000E32DC
		// (set) Token: 0x060037E4 RID: 14308 RVA: 0x000E50E4 File Offset: 0x000E32E4
		internal CompilerPhase2_AfterTypification CompilerPhase2_AfterTypification { get; set; }

		// Token: 0x17000932 RID: 2354
		// (get) Token: 0x060037E5 RID: 14309 RVA: 0x000E50F0 File Offset: 0x000E32F0
		// (set) Token: 0x060037E6 RID: 14310 RVA: 0x000E50F8 File Offset: 0x000E32F8
		internal CompilerPhase3_Locator CompilerPhase3_Locator { get; set; }

		// Token: 0x17000933 RID: 2355
		// (get) Token: 0x060037E7 RID: 14311 RVA: 0x000E5104 File Offset: 0x000E3304
		// (set) Token: 0x060037E8 RID: 14312 RVA: 0x000E510C File Offset: 0x000E330C
		internal CompilerPhase4_Typechecker CompilerPhase4_Typechecker { get; set; }

		// Token: 0x17000934 RID: 2356
		// (get) Token: 0x060037E9 RID: 14313 RVA: 0x000E5118 File Offset: 0x000E3318
		// (set) Token: 0x060037EA RID: 14314 RVA: 0x000E5120 File Offset: 0x000E3320
		internal CompilerPhase5_Codegenerator CompilerPhase5_Codegenerator { get; set; }

		// Token: 0x17000935 RID: 2357
		// (get) Token: 0x060037EB RID: 14315 RVA: 0x000E512C File Offset: 0x000E332C
		// (set) Token: 0x060037EC RID: 14316 RVA: 0x000E5134 File Offset: 0x000E3334
		internal global::\u0002.\u0014 CompilerPhase6_AfterCodegeneration { get; set; }

		// Token: 0x17000936 RID: 2358
		// (get) Token: 0x060037ED RID: 14317 RVA: 0x000E5140 File Offset: 0x000E3340
		// (set) Token: 0x060037EE RID: 14318 RVA: 0x000E5148 File Offset: 0x000E3348
		internal \u001B CompilerPhaseControllerCompile { get; set; }

		// Token: 0x17000937 RID: 2359
		// (get) Token: 0x060037EF RID: 14319 RVA: 0x000E5154 File Offset: 0x000E3354
		internal _ICompileContext ComconOldOriginal
		{
			get
			{
				return this.\u0002;
			}
		}

		// Token: 0x04000AFE RID: 2814
		private static Dictionary<Guid, \u001A.\u0013> \u0001 = new Dictionary<Guid, \u001A.\u0013>();

		// Token: 0x04000AFF RID: 2815
		[CompilerGenerated]
		private _IPreCompileContext \u0001;

		// Token: 0x04000B00 RID: 2816
		[CompilerGenerated]
		private readonly Guid \u0001;

		// Token: 0x04000B01 RID: 2817
		[CompilerGenerated]
		private readonly Guid \u0002;

		// Token: 0x04000B02 RID: 2818
		[CompilerGenerated]
		private Guid \u0003;

		// Token: 0x04000B03 RID: 2819
		[CompilerGenerated]
		private bool \u0001;

		// Token: 0x04000B04 RID: 2820
		[CompilerGenerated]
		private readonly bool \u0002;

		// Token: 0x04000B05 RID: 2821
		[CompilerGenerated]
		private readonly bool \u0003;

		// Token: 0x04000B06 RID: 2822
		[CompilerGenerated]
		private bool \u0004;

		// Token: 0x04000B07 RID: 2823
		[CompilerGenerated]
		private bool \u0005;

		// Token: 0x04000B08 RID: 2824
		[CompilerGenerated]
		private bool \u0006;

		// Token: 0x04000B09 RID: 2825
		[CompilerGenerated]
		private IOnlineChangeDetails \u0001;

		// Token: 0x04000B0A RID: 2826
		[CompilerGenerated]
		private _ILanguageModelManagerConsolidated \u0001;

		// Token: 0x04000B0B RID: 2827
		[CompilerGenerated]
		private _IPreCompileContext \u0002;

		// Token: 0x04000B0C RID: 2828
		[CompilerGenerated]
		private _ICompileContext \u0001;

		// Token: 0x04000B0D RID: 2829
		private _ICompileContext \u0002;

		// Token: 0x04000B0E RID: 2830
		private _ICompileContext \u0003;

		// Token: 0x04000B0F RID: 2831
		[CompilerGenerated]
		private _ICompileContext \u0004;

		// Token: 0x04000B10 RID: 2832
		[CompilerGenerated]
		private _ICompileContext \u0005;

		// Token: 0x04000B11 RID: 2833
		[CompilerGenerated]
		private IProgressCallback \u0001;

		// Token: 0x04000B12 RID: 2834
		[CompilerGenerated]
		private bool \u0007;

		// Token: 0x04000B13 RID: 2835
		[CompilerGenerated]
		private global::\u0014.\u0001 \u0001;

		// Token: 0x04000B14 RID: 2836
		[CompilerGenerated]
		private CompilerPhase1_Typifier \u0001;

		// Token: 0x04000B15 RID: 2837
		[CompilerGenerated]
		private CompilerPhase2_AfterTypification \u0001;

		// Token: 0x04000B16 RID: 2838
		[CompilerGenerated]
		private CompilerPhase3_Locator \u0001;

		// Token: 0x04000B17 RID: 2839
		[CompilerGenerated]
		private CompilerPhase4_Typechecker \u0001;

		// Token: 0x04000B18 RID: 2840
		[CompilerGenerated]
		private CompilerPhase5_Codegenerator \u0001;

		// Token: 0x04000B19 RID: 2841
		[CompilerGenerated]
		private global::\u0002.\u0014 \u0001;

		// Token: 0x04000B1A RID: 2842
		[CompilerGenerated]
		private \u001B \u0001;
	}
}
