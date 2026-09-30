using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.Core.Options;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x020000D2 RID: 210
	[TypeGuid("{F316FA9B-EEEE-4959-B1E0-9B1201C86456}")]
	[SystemInterface("_3S.CoDeSys.Core.LanguageModel.ICompilerVersionManager")]
	[SuppressMessage("Major Code Smell", "S1200:Classes should not be coupled to too many other classes (Single Responsibility Principle)", Justification = "Class cannot be divided into subclasses because of released interfaces")]
	public class CompilerVersionManager : ICompilerVersionManager7, ICompilerVersionManager6, ICompilerVersionManager5, ICompilerVersionManager4, ICompilerVersionManager3, ICompilerVersionManager2, ICompilerVersionManager, _ICompilerVersionSettings, ISystemInstanceRequiresInitialization
	{
		// Token: 0x17000400 RID: 1024
		// (get) Token: 0x06000EB5 RID: 3765 RVA: 0x00026F65 File Offset: 0x00025F65
		private IEnumerable<Version> CompilerVersionsAvailable
		{
			get
			{
				IEnumerable<_ICompilerVersionsProvider> enumerable = APEnvironmentFacade.Instance.CreateCompilerVersionsProviders();
				foreach (_ICompilerVersionsProvider icompilerVersionsProvider in enumerable)
				{
					foreach (Version version in icompilerVersionsProvider.CompilerVersions)
					{
						yield return version;
					}
					IEnumerator<Version> enumerator2 = null;
				}
				IEnumerator<_ICompilerVersionsProvider> enumerator = null;
				yield break;
				yield break;
			}
		}

		// Token: 0x06000EB7 RID: 3767 RVA: 0x00026F84 File Offset: 0x00025F84
		public void OnAllSystemInstancesAvailable()
		{
			APEnvironmentFacade.Instance.BeforePrimaryProjectSwitched += this.OnBeforePrimaryProjectSwitched;
			APEnvironmentFacade.Instance.OptionCreated += this.OnOptionCreated;
			APEnvironmentFacade.Instance.OptionChanged += this.OnOptionChanged;
			APEnvironmentFacade.Instance.OptionDeleted += this.OnOptionDeleted;
		}

		// Token: 0x06000EB8 RID: 3768 RVA: 0x00026FE9 File Offset: 0x00025FE9
		public void OnBeforePrimaryProjectSwitched(IProject oldProject, IProject newProject)
		{
			this._versionToUse = null;
			VersionedCompilerFactory.Reset();
		}

		// Token: 0x06000EB9 RID: 3769 RVA: 0x00026FF7 File Offset: 0x00025FF7
		private void OnOptionCreated(object sender, OptionEventArgs e)
		{
			if (e.OptionKey != null && e.OptionKey.Name == "{535658C0-5AF5-460d-99A4-BFFB984A829A}")
			{
				this._versionToUse = null;
				VersionedCompilerFactory.ResetScannerParserProvider();
			}
		}

		// Token: 0x06000EBA RID: 3770 RVA: 0x00026FF7 File Offset: 0x00025FF7
		public void OnOptionChanged(object sender, OptionEventArgs e)
		{
			if (e.OptionKey != null && e.OptionKey.Name == "{535658C0-5AF5-460d-99A4-BFFB984A829A}")
			{
				this._versionToUse = null;
				VersionedCompilerFactory.ResetScannerParserProvider();
			}
		}

		// Token: 0x06000EBB RID: 3771 RVA: 0x00026FF7 File Offset: 0x00025FF7
		private void OnOptionDeleted(object sender, OptionEventArgs e)
		{
			if (e.OptionKey != null && e.OptionKey.Name == "{535658C0-5AF5-460d-99A4-BFFB984A829A}")
			{
				this._versionToUse = null;
				VersionedCompilerFactory.ResetScannerParserProvider();
			}
		}

		// Token: 0x17000401 RID: 1025
		// (get) Token: 0x06000EBC RID: 3772 RVA: 0x00027024 File Offset: 0x00026024
		public Version[] AvailableCompilerVersions
		{
			get
			{
				Version V34 = new Version("3.4.0.0");
				return (from v in this.CompilerVersionsAvailable
				where v >= V34
				select v).ToArray<Version>();
			}
		}

		// Token: 0x17000402 RID: 1026
		// (get) Token: 0x06000EBD RID: 3773 RVA: 0x00027063 File Offset: 0x00026063
		public VersionConstraint VersionConstraintFromOptions
		{
			get
			{
				return OptionsHelper.GetCompilerVersionConstraint();
			}
		}

		// Token: 0x17000403 RID: 1027
		// (get) Token: 0x06000EBE RID: 3774 RVA: 0x0002706A File Offset: 0x0002606A
		// (set) Token: 0x06000EBF RID: 3775 RVA: 0x00027088 File Offset: 0x00026088
		public bool UseNewestVersion
		{
			get
			{
				if (this._versionToUse == null)
				{
					this.CompilerVersionToUse();
				}
				return this._bUseNewest;
			}
			set
			{
				bool bUseNewest = this._bUseNewest;
				Version version = this.CompilerVersionToUse();
				Exception ex;
				if (value != bUseNewest && this.RaiseUseNewestCompilerVersionChanging(bUseNewest, value, out ex))
				{
					throw ex;
				}
				if (value)
				{
					OptionsHelper.SetCompilerVersionConstraint(new NewestVersionConstraint());
				}
				this._bUseNewest = value;
				Version version2 = (from v in this.CompilerVersionsAvailable
				orderby v
				select v).Last<Version>();
				if (!value)
				{
					OptionsHelper.SetCompilerVersionConstraint(new ExactVersionConstraint(version2));
				}
				this.UpdateShortcuts(version2);
				if (version != version2)
				{
					this.RaiseCompilerVersionChanged(version, version2);
				}
				if (value != bUseNewest)
				{
					this.RaiseUseNewestCompilerVersionChanged(bUseNewest, value);
				}
			}
		}

		// Token: 0x17000404 RID: 1028
		// (get) Token: 0x06000EC0 RID: 3776 RVA: 0x0002712B File Offset: 0x0002612B
		public bool GreaterEqualV3131
		{
			get
			{
				if (this._versionToUse == null)
				{
					this.CompilerVersionToUse();
				}
				return this._bGreaterEq3131;
			}
		}

		// Token: 0x17000405 RID: 1029
		// (get) Token: 0x06000EC1 RID: 3777 RVA: 0x00027148 File Offset: 0x00026148
		public bool GreaterEqualV3203
		{
			get
			{
				if (this._versionToUse == null)
				{
					this.CompilerVersionToUse();
				}
				return this._bGreaterEq3203;
			}
		}

		// Token: 0x17000406 RID: 1030
		// (get) Token: 0x06000EC2 RID: 3778 RVA: 0x00027165 File Offset: 0x00026165
		public bool GreaterEqualV3204
		{
			get
			{
				if (this._versionToUse == null)
				{
					this.CompilerVersionToUse();
				}
				return this._bGreaterEq3204;
			}
		}

		// Token: 0x17000407 RID: 1031
		// (get) Token: 0x06000EC3 RID: 3779 RVA: 0x00027182 File Offset: 0x00026182
		public bool GreaterEqualV3211
		{
			get
			{
				if (this._versionToUse == null)
				{
					this.CompilerVersionToUse();
				}
				return this._bGreaterEq3211;
			}
		}

		// Token: 0x17000408 RID: 1032
		// (get) Token: 0x06000EC4 RID: 3780 RVA: 0x0002719F File Offset: 0x0002619F
		public bool GreaterEqualV32120
		{
			get
			{
				if (this._versionToUse == null)
				{
					this.CompilerVersionToUse();
				}
				return this._bGreaterEq32120;
			}
		}

		// Token: 0x17000409 RID: 1033
		// (get) Token: 0x06000EC5 RID: 3781 RVA: 0x000271BC File Offset: 0x000261BC
		public bool GreaterEqualV32220
		{
			get
			{
				if (this._versionToUse == null)
				{
					this.CompilerVersionToUse();
				}
				return this._bGreaterEq32220;
			}
		}

		// Token: 0x1700040A RID: 1034
		// (get) Token: 0x06000EC6 RID: 3782 RVA: 0x000271D9 File Offset: 0x000261D9
		public bool GreaterEqualV3300
		{
			get
			{
				if (this._versionToUse == null)
				{
					this.CompilerVersionToUse();
				}
				return this._bGreaterEq33000;
			}
		}

		// Token: 0x1700040B RID: 1035
		// (get) Token: 0x06000EC7 RID: 3783 RVA: 0x000271F6 File Offset: 0x000261F6
		public bool GreaterEqualV33010
		{
			get
			{
				if (this._versionToUse == null)
				{
					this.CompilerVersionToUse();
				}
				return this._bGreaterEq33010;
			}
		}

		// Token: 0x1700040C RID: 1036
		// (get) Token: 0x06000EC8 RID: 3784 RVA: 0x00027213 File Offset: 0x00026213
		public bool GreaterEqualV33020
		{
			get
			{
				if (this._versionToUse == null)
				{
					this.CompilerVersionToUse();
				}
				return this._bGreaterEq33020;
			}
		}

		// Token: 0x1700040D RID: 1037
		// (get) Token: 0x06000EC9 RID: 3785 RVA: 0x00027230 File Offset: 0x00026230
		public bool GreaterEqualV33100
		{
			get
			{
				if (this._versionToUse == null)
				{
					this.CompilerVersionToUse();
				}
				return this._bGreaterEq33100;
			}
		}

		// Token: 0x1700040E RID: 1038
		// (get) Token: 0x06000ECA RID: 3786 RVA: 0x0002724D File Offset: 0x0002624D
		public bool GreaterEqualV33102
		{
			get
			{
				if (this._versionToUse == null)
				{
					this.CompilerVersionToUse();
				}
				return this._bGreaterEq33102;
			}
		}

		// Token: 0x1700040F RID: 1039
		// (get) Token: 0x06000ECB RID: 3787 RVA: 0x0002726A File Offset: 0x0002626A
		public bool GreaterEqualV33120
		{
			get
			{
				if (this._versionToUse == null)
				{
					this.CompilerVersionToUse();
				}
				return this._bGreaterEq33120;
			}
		}

		// Token: 0x17000410 RID: 1040
		// (get) Token: 0x06000ECC RID: 3788 RVA: 0x00027287 File Offset: 0x00026287
		public bool GreaterEqualV33140
		{
			get
			{
				if (this._versionToUse == null)
				{
					this.CompilerVersionToUse();
				}
				return (this._bGreaterEq33140 && !this._bGreaterEq33200) || this._bGreaterEq34000;
			}
		}

		// Token: 0x17000411 RID: 1041
		// (get) Token: 0x06000ECD RID: 3789 RVA: 0x000272B6 File Offset: 0x000262B6
		public bool GreaterEqualV33200
		{
			get
			{
				if (this._versionToUse == null)
				{
					this.CompilerVersionToUse();
				}
				return this._bGreaterEq33200;
			}
		}

		// Token: 0x17000412 RID: 1042
		// (get) Token: 0x06000ECE RID: 3790 RVA: 0x000272D3 File Offset: 0x000262D3
		public bool GreaterEqualV33220
		{
			get
			{
				if (this._versionToUse == null)
				{
					this.CompilerVersionToUse();
				}
				return this._bGreaterEq33220;
			}
		}

		// Token: 0x17000413 RID: 1043
		// (get) Token: 0x06000ECF RID: 3791 RVA: 0x000272F0 File Offset: 0x000262F0
		public bool GreaterEqualV33250
		{
			get
			{
				if (this._versionToUse == null)
				{
					this.CompilerVersionToUse();
				}
				return this._bGreaterEq33250;
			}
		}

		// Token: 0x17000414 RID: 1044
		// (get) Token: 0x06000ED0 RID: 3792 RVA: 0x0002730D File Offset: 0x0002630D
		public bool GreaterEqualV34000
		{
			get
			{
				if (this._versionToUse == null)
				{
					this.CompilerVersionToUse();
				}
				return this._bGreaterEq34000;
			}
		}

		// Token: 0x17000415 RID: 1045
		// (get) Token: 0x06000ED1 RID: 3793 RVA: 0x0002732A File Offset: 0x0002632A
		public bool GreaterEqualV34100
		{
			get
			{
				if (this._versionToUse == null)
				{
					this.CompilerVersionToUse();
				}
				return this._bGreaterEq34100;
			}
		}

		// Token: 0x17000416 RID: 1046
		// (get) Token: 0x06000ED2 RID: 3794 RVA: 0x00027347 File Offset: 0x00026347
		public bool GreaterEqualV34110
		{
			get
			{
				if (this._versionToUse == null)
				{
					this.CompilerVersionToUse();
				}
				return this._bGreaterEq34110;
			}
		}

		// Token: 0x17000417 RID: 1047
		// (get) Token: 0x06000ED3 RID: 3795 RVA: 0x00027364 File Offset: 0x00026364
		public bool GreaterEqualV34140
		{
			get
			{
				if (this._versionToUse == null)
				{
					this.CompilerVersionToUse();
				}
				return this._bGreaterEq34140;
			}
		}

		// Token: 0x17000418 RID: 1048
		// (get) Token: 0x06000ED4 RID: 3796 RVA: 0x00027381 File Offset: 0x00026381
		public bool GreaterEqualV34180
		{
			get
			{
				if (this._versionToUse == null)
				{
					this.CompilerVersionToUse();
				}
				return this._bGreaterEq34180;
			}
		}

		// Token: 0x17000419 RID: 1049
		// (get) Token: 0x06000ED5 RID: 3797 RVA: 0x0002739E File Offset: 0x0002639E
		public bool GreaterEqualV34200
		{
			get
			{
				if (this._versionToUse == null)
				{
					this.CompilerVersionToUse();
				}
				return this._bGreaterEq34200;
			}
		}

		// Token: 0x1700041A RID: 1050
		// (get) Token: 0x06000ED6 RID: 3798 RVA: 0x000273BB File Offset: 0x000263BB
		public bool GreaterEqualV3202
		{
			get
			{
				if (this._versionToUse == null)
				{
					this.CompilerVersionToUse();
				}
				return this._bGreaterEq3202;
			}
		}

		// Token: 0x1700041B RID: 1051
		// (get) Token: 0x06000ED7 RID: 3799 RVA: 0x000273D8 File Offset: 0x000263D8
		public bool GreaterEqualV3201
		{
			get
			{
				if (this._versionToUse == null)
				{
					this.CompilerVersionToUse();
				}
				return this._bGreaterEq3201;
			}
		}

		// Token: 0x1700041C RID: 1052
		// (get) Token: 0x06000ED8 RID: 3800 RVA: 0x000273F5 File Offset: 0x000263F5
		public bool GreaterEqualV3200
		{
			get
			{
				if (this._versionToUse == null)
				{
					this.CompilerVersionToUse();
				}
				return this._bGreaterEq3200;
			}
		}

		// Token: 0x1700041D RID: 1053
		// (get) Token: 0x06000ED9 RID: 3801 RVA: 0x00027412 File Offset: 0x00026412
		public bool GreaterEqualV34300
		{
			get
			{
				if (this._versionToUse == null)
				{
					this.CompilerVersionToUse();
				}
				return this._bGreaterEq34300;
			}
		}

		// Token: 0x1700041E RID: 1054
		// (get) Token: 0x06000EDA RID: 3802 RVA: 0x0002742F File Offset: 0x0002642F
		public bool GreaterEqualV34400
		{
			get
			{
				if (this._versionToUse == null)
				{
					this.CompilerVersionToUse();
				}
				return this._bGreaterEq34400;
			}
		}

		// Token: 0x1700041F RID: 1055
		// (get) Token: 0x06000EDB RID: 3803 RVA: 0x0002744C File Offset: 0x0002644C
		public bool GreaterEqualV34421
		{
			get
			{
				if (this._versionToUse == null)
				{
					this.CompilerVersionToUse();
				}
				return this._bGreaterEq34421;
			}
		}

		// Token: 0x17000420 RID: 1056
		// (get) Token: 0x06000EDC RID: 3804 RVA: 0x00027469 File Offset: 0x00026469
		public bool GreaterEqualV34430
		{
			get
			{
				if (this._versionToUse == null)
				{
					this.CompilerVersionToUse();
				}
				return this._bGreaterEq34430;
			}
		}

		// Token: 0x17000421 RID: 1057
		// (get) Token: 0x06000EDD RID: 3805 RVA: 0x00027486 File Offset: 0x00026486
		public bool GreaterEqualV34451
		{
			get
			{
				if (this._versionToUse == null)
				{
					this.CompilerVersionToUse();
				}
				return this._bGreaterEq34451;
			}
		}

		// Token: 0x17000422 RID: 1058
		// (get) Token: 0x06000EDE RID: 3806 RVA: 0x000274A3 File Offset: 0x000264A3
		public bool GreaterEqualV34500
		{
			get
			{
				if (this._versionToUse == null)
				{
					this.CompilerVersionToUse();
				}
				return this._bGreaterEq34500;
			}
		}

		// Token: 0x17000423 RID: 1059
		// (get) Token: 0x06000EDF RID: 3807 RVA: 0x000274C0 File Offset: 0x000264C0
		public bool GreaterEqualV34561
		{
			get
			{
				if (this._versionToUse == null)
				{
					this.CompilerVersionToUse();
				}
				return this._bGreaterEq34561;
			}
		}

		// Token: 0x17000424 RID: 1060
		// (get) Token: 0x06000EE0 RID: 3808 RVA: 0x000274DD File Offset: 0x000264DD
		public bool GreaterEqualV345100
		{
			get
			{
				if (this._versionToUse == null)
				{
					this.CompilerVersionToUse();
				}
				return this._bGreaterEq345100;
			}
		}

		// Token: 0x17000425 RID: 1061
		// (get) Token: 0x06000EE1 RID: 3809 RVA: 0x000274FA File Offset: 0x000264FA
		public bool GreaterEqualV345110
		{
			get
			{
				if (this._versionToUse == null)
				{
					this.CompilerVersionToUse();
				}
				return this._bGreaterEq345110;
			}
		}

		// Token: 0x17000426 RID: 1062
		// (get) Token: 0x06000EE2 RID: 3810 RVA: 0x00027517 File Offset: 0x00026517
		public bool GreaterEqualV345120
		{
			get
			{
				if (this._versionToUse == null)
				{
					this.CompilerVersionToUse();
				}
				return this._bGreaterEq345120;
			}
		}

		// Token: 0x17000427 RID: 1063
		// (get) Token: 0x06000EE3 RID: 3811 RVA: 0x00027534 File Offset: 0x00026534
		public bool GreaterEqualV35000
		{
			get
			{
				if (this._versionToUse == null)
				{
					this.CompilerVersionToUse();
				}
				return this._bGreaterEq35000;
			}
		}

		// Token: 0x17000428 RID: 1064
		// (get) Token: 0x06000EE4 RID: 3812 RVA: 0x00027551 File Offset: 0x00026551
		public bool GreaterEqualV35100
		{
			get
			{
				if (this._versionToUse == null)
				{
					this.CompilerVersionToUse();
				}
				return this._bGreaterEq35100;
			}
		}

		// Token: 0x17000429 RID: 1065
		// (get) Token: 0x06000EE5 RID: 3813 RVA: 0x0002756E File Offset: 0x0002656E
		public bool GreaterEqualV35110
		{
			get
			{
				if (this._versionToUse == null)
				{
					this.CompilerVersionToUse();
				}
				return this._bGreaterEq35110;
			}
		}

		// Token: 0x1700042A RID: 1066
		// (get) Token: 0x06000EE6 RID: 3814 RVA: 0x0002758B File Offset: 0x0002658B
		public bool GreaterEqualV35200
		{
			get
			{
				if (this._versionToUse == null)
				{
					this.CompilerVersionToUse();
				}
				return this._bGreaterEq35200;
			}
		}

		// Token: 0x1700042B RID: 1067
		// (get) Token: 0x06000EE7 RID: 3815 RVA: 0x000275A8 File Offset: 0x000265A8
		public bool GreaterEqualV35300
		{
			get
			{
				if (this._versionToUse == null)
				{
					this.CompilerVersionToUse();
				}
				return this._bGreaterEq35300;
			}
		}

		// Token: 0x1700042C RID: 1068
		// (get) Token: 0x06000EE8 RID: 3816 RVA: 0x000275C5 File Offset: 0x000265C5
		public bool GreaterEqualV35340
		{
			get
			{
				if (this._versionToUse == null)
				{
					this.CompilerVersionToUse();
				}
				return this._bGreaterEq35340;
			}
		}

		// Token: 0x1700042D RID: 1069
		// (get) Token: 0x06000EE9 RID: 3817 RVA: 0x000275E2 File Offset: 0x000265E2
		public bool GreaterEqualV35350
		{
			get
			{
				if (this._versionToUse == null)
				{
					this.CompilerVersionToUse();
				}
				return this._bGreaterEq35350;
			}
		}

		// Token: 0x1700042E RID: 1070
		// (get) Token: 0x06000EEA RID: 3818 RVA: 0x000275FF File Offset: 0x000265FF
		public bool GreaterEqualV35370
		{
			get
			{
				if (this._versionToUse == null)
				{
					this.CompilerVersionToUse();
				}
				return this._bGreaterEq35370;
			}
		}

		// Token: 0x1700042F RID: 1071
		// (get) Token: 0x06000EEB RID: 3819 RVA: 0x0002761C File Offset: 0x0002661C
		public bool GreaterEqualV35400
		{
			get
			{
				if (this._versionToUse == null)
				{
					this.CompilerVersionToUse();
				}
				return this._bGreaterEq35400;
			}
		}

		// Token: 0x17000430 RID: 1072
		// (get) Token: 0x06000EEC RID: 3820 RVA: 0x00027639 File Offset: 0x00026639
		public bool GreaterEqualV35420
		{
			get
			{
				if (this._versionToUse == null)
				{
					this.CompilerVersionToUse();
				}
				return this._bGreaterEq35420;
			}
		}

		// Token: 0x17000431 RID: 1073
		// (get) Token: 0x06000EED RID: 3821 RVA: 0x00027656 File Offset: 0x00026656
		public bool GreaterEqualV35430
		{
			get
			{
				if (this._versionToUse == null)
				{
					this.CompilerVersionToUse();
				}
				return this._bGreaterEq35430;
			}
		}

		// Token: 0x17000432 RID: 1074
		// (get) Token: 0x06000EEE RID: 3822 RVA: 0x00027673 File Offset: 0x00026673
		public bool GreaterEqualV35500
		{
			get
			{
				if (this._versionToUse == null)
				{
					this.CompilerVersionToUse();
				}
				return this._bGreaterEq35500;
			}
		}

		// Token: 0x17000433 RID: 1075
		// (get) Token: 0x06000EEF RID: 3823 RVA: 0x00027690 File Offset: 0x00026690
		public bool GreaterEqualV35510
		{
			get
			{
				if (this._versionToUse == null)
				{
					this.CompilerVersionToUse();
				}
				return this._bGreaterEq35510;
			}
		}

		// Token: 0x17000434 RID: 1076
		// (get) Token: 0x06000EF0 RID: 3824 RVA: 0x000276AD File Offset: 0x000266AD
		public bool GreaterEqualV35530
		{
			get
			{
				if (this._versionToUse == null)
				{
					this.CompilerVersionToUse();
				}
				return this._bGreaterEq35530;
			}
		}

		// Token: 0x17000435 RID: 1077
		// (get) Token: 0x06000EF1 RID: 3825 RVA: 0x000276CA File Offset: 0x000266CA
		public bool GreaterEqualV35600
		{
			get
			{
				if (this._versionToUse == null)
				{
					this.CompilerVersionToUse();
				}
				return this._bGreaterEq35600;
			}
		}

		// Token: 0x17000436 RID: 1078
		// (get) Token: 0x06000EF2 RID: 3826 RVA: 0x000276E7 File Offset: 0x000266E7
		public bool GreaterEqualV35610
		{
			get
			{
				if (this._versionToUse == null)
				{
					this.CompilerVersionToUse();
				}
				return this._bGreaterEq35610;
			}
		}

		// Token: 0x17000437 RID: 1079
		// (get) Token: 0x06000EF3 RID: 3827 RVA: 0x00027704 File Offset: 0x00026704
		public bool GreaterEqualV35620
		{
			get
			{
				if (this._versionToUse == null)
				{
					this.CompilerVersionToUse();
				}
				return this._bGreaterEq35620;
			}
		}

		// Token: 0x17000438 RID: 1080
		// (get) Token: 0x06000EF4 RID: 3828 RVA: 0x00027721 File Offset: 0x00026721
		public bool GreaterEqualV35640
		{
			get
			{
				if (this._versionToUse == null)
				{
					this.CompilerVersionToUse();
				}
				return this._bGreaterEq35640;
			}
		}

		// Token: 0x17000439 RID: 1081
		// (get) Token: 0x06000EF5 RID: 3829 RVA: 0x0002773E File Offset: 0x0002673E
		public bool GreaterEqualV35660
		{
			get
			{
				if (this._versionToUse == null)
				{
					this.CompilerVersionToUse();
				}
				return this._bGreaterEq35660;
			}
		}

		// Token: 0x1700043A RID: 1082
		// (get) Token: 0x06000EF6 RID: 3830 RVA: 0x0002775B File Offset: 0x0002675B
		public bool GreaterEqualV35666
		{
			get
			{
				if (this._versionToUse == null)
				{
					this.CompilerVersionToUse();
				}
				return this._bGreaterEq35666;
			}
		}

		// Token: 0x1700043B RID: 1083
		// (get) Token: 0x06000EF7 RID: 3831 RVA: 0x00027778 File Offset: 0x00026778
		public bool GreaterEqualV35670
		{
			get
			{
				if (this._versionToUse == null)
				{
					this.CompilerVersionToUse();
				}
				return this._bGreaterEq35670;
			}
		}

		// Token: 0x1700043C RID: 1084
		// (get) Token: 0x06000EF8 RID: 3832 RVA: 0x00027795 File Offset: 0x00026795
		public bool GreaterEqualV35700
		{
			get
			{
				if (this._versionToUse == null)
				{
					this.CompilerVersionToUse();
				}
				return this._bGreaterEq35700;
			}
		}

		// Token: 0x1700043D RID: 1085
		// (get) Token: 0x06000EF9 RID: 3833 RVA: 0x000277B2 File Offset: 0x000267B2
		public bool GreaterEqualV35720
		{
			get
			{
				if (this._versionToUse == null)
				{
					this.CompilerVersionToUse();
				}
				return this._bGreaterEq35720;
			}
		}

		// Token: 0x1700043E RID: 1086
		// (get) Token: 0x06000EFA RID: 3834 RVA: 0x000277CF File Offset: 0x000267CF
		public bool GreaterEqualV35730
		{
			get
			{
				if (this._versionToUse == null)
				{
					this.CompilerVersionToUse();
				}
				return this._bGreaterEq35730;
			}
		}

		// Token: 0x1700043F RID: 1087
		// (get) Token: 0x06000EFB RID: 3835 RVA: 0x000277EC File Offset: 0x000267EC
		public bool GreaterEqualV35800
		{
			get
			{
				if (this._versionToUse == null)
				{
					this.CompilerVersionToUse();
				}
				return this._bGreaterEq35800;
			}
		}

		// Token: 0x17000440 RID: 1088
		// (get) Token: 0x06000EFC RID: 3836 RVA: 0x00027809 File Offset: 0x00026809
		public bool GreaterEqualV35900
		{
			get
			{
				if (this._versionToUse == null)
				{
					this.CompilerVersionToUse();
				}
				return this._bGreaterEq35900;
			}
		}

		// Token: 0x17000441 RID: 1089
		// (get) Token: 0x06000EFD RID: 3837 RVA: 0x00027826 File Offset: 0x00026826
		public bool GreaterEqualV35940
		{
			get
			{
				if (this._versionToUse == null)
				{
					this.CompilerVersionToUse();
				}
				return this._bGreaterEq35940;
			}
		}

		// Token: 0x17000442 RID: 1090
		// (get) Token: 0x06000EFE RID: 3838 RVA: 0x00027843 File Offset: 0x00026843
		public bool GreaterEqualV35950
		{
			get
			{
				if (this._versionToUse == null)
				{
					this.CompilerVersionToUse();
				}
				return this._bGreaterEq35950;
			}
		}

		// Token: 0x17000443 RID: 1091
		// (get) Token: 0x06000EFF RID: 3839 RVA: 0x00027860 File Offset: 0x00026860
		public bool GreaterEqualV35980
		{
			get
			{
				if (this._versionToUse == null)
				{
					this.CompilerVersionToUse();
				}
				return this._bGreaterEq35980;
			}
		}

		// Token: 0x17000444 RID: 1092
		// (get) Token: 0x06000F00 RID: 3840 RVA: 0x0002787D File Offset: 0x0002687D
		public bool GreaterEqualV351000
		{
			get
			{
				if (this._versionToUse == null)
				{
					this.CompilerVersionToUse();
				}
				return this._bGreaterEq351000;
			}
		}

		// Token: 0x17000445 RID: 1093
		// (get) Token: 0x06000F01 RID: 3841 RVA: 0x0002789A File Offset: 0x0002689A
		public bool GreaterEqualV351020
		{
			get
			{
				if (this._versionToUse == null)
				{
					this.CompilerVersionToUse();
				}
				return this._bGreaterEq351020;
			}
		}

		// Token: 0x17000446 RID: 1094
		// (get) Token: 0x06000F02 RID: 3842 RVA: 0x000278B7 File Offset: 0x000268B7
		public bool GreaterEqualV351040
		{
			get
			{
				if (this._versionToUse == null)
				{
					this.CompilerVersionToUse();
				}
				return this._bGreaterEq351040;
			}
		}

		// Token: 0x17000447 RID: 1095
		// (get) Token: 0x06000F03 RID: 3843 RVA: 0x000278D4 File Offset: 0x000268D4
		public bool GreaterEqualV351050
		{
			get
			{
				if (this._versionToUse == null)
				{
					this.CompilerVersionToUse();
				}
				return this._bGreaterEq351050;
			}
		}

		// Token: 0x17000448 RID: 1096
		// (get) Token: 0x06000F04 RID: 3844 RVA: 0x000278F1 File Offset: 0x000268F1
		public bool GreaterEqualV351100
		{
			get
			{
				if (this._versionToUse == null)
				{
					this.CompilerVersionToUse();
				}
				return this._bGreaterEq351100;
			}
		}

		// Token: 0x17000449 RID: 1097
		// (get) Token: 0x06000F05 RID: 3845 RVA: 0x0002790E File Offset: 0x0002690E
		public bool GreaterEqualV351110
		{
			get
			{
				if (this._versionToUse == null)
				{
					this.CompilerVersionToUse();
				}
				return this._bGreaterEq351110;
			}
		}

		// Token: 0x1700044A RID: 1098
		// (get) Token: 0x06000F06 RID: 3846 RVA: 0x0002792B File Offset: 0x0002692B
		public bool GreaterEqualV351120
		{
			get
			{
				if (this._versionToUse == null)
				{
					this.CompilerVersionToUse();
				}
				return this._bGreaterEq351120;
			}
		}

		// Token: 0x1700044B RID: 1099
		// (get) Token: 0x06000F07 RID: 3847 RVA: 0x00027948 File Offset: 0x00026948
		public bool GreaterEqualV351200
		{
			get
			{
				if (this._versionToUse == null)
				{
					this.CompilerVersionToUse();
				}
				return this._bGreaterEq351200;
			}
		}

		// Token: 0x1700044C RID: 1100
		// (get) Token: 0x06000F08 RID: 3848 RVA: 0x00027965 File Offset: 0x00026965
		public bool GreaterEqualWithinSPV351250
		{
			get
			{
				if (this._versionToUse == null)
				{
					this.CompilerVersionToUse();
				}
				return this._bGreaterEq351250 && !this._bGreaterEq351300;
			}
		}

		// Token: 0x1700044D RID: 1101
		// (get) Token: 0x06000F09 RID: 3849 RVA: 0x0002798F File Offset: 0x0002698F
		public bool GreaterEqualV351300
		{
			get
			{
				if (this._versionToUse == null)
				{
					this.CompilerVersionToUse();
				}
				return this._bGreaterEq351300;
			}
		}

		// Token: 0x1700044E RID: 1102
		// (get) Token: 0x06000F0A RID: 3850 RVA: 0x000279AC File Offset: 0x000269AC
		public bool GreaterEqualV351310
		{
			get
			{
				if (this._versionToUse == null)
				{
					this.CompilerVersionToUse();
				}
				return this._bGreaterEq351310;
			}
		}

		// Token: 0x1700044F RID: 1103
		// (get) Token: 0x06000F0B RID: 3851 RVA: 0x000279C9 File Offset: 0x000269C9
		public bool GreaterEqualV351400
		{
			get
			{
				if (this._versionToUse == null)
				{
					this.CompilerVersionToUse();
				}
				return this._bGreaterEq351400;
			}
		}

		// Token: 0x17000450 RID: 1104
		// (get) Token: 0x06000F0C RID: 3852 RVA: 0x000279E6 File Offset: 0x000269E6
		public bool GreaterEqualV351410
		{
			get
			{
				if (this._versionToUse == null)
				{
					this.CompilerVersionToUse();
				}
				return this._bGreaterEq351410;
			}
		}

		// Token: 0x17000451 RID: 1105
		// (get) Token: 0x06000F0D RID: 3853 RVA: 0x00027A03 File Offset: 0x00026A03
		public bool GreaterEqualV351430
		{
			get
			{
				if (this._versionToUse == null)
				{
					this.CompilerVersionToUse();
				}
				return this._bGreaterEq351430;
			}
		}

		// Token: 0x17000452 RID: 1106
		// (get) Token: 0x06000F0E RID: 3854 RVA: 0x00027A20 File Offset: 0x00026A20
		public bool GreaterEqualV351500
		{
			get
			{
				if (this._versionToUse == null)
				{
					this.CompilerVersionToUse();
				}
				return this._bGreaterEq351500;
			}
		}

		// Token: 0x17000453 RID: 1107
		// (get) Token: 0x06000F0F RID: 3855 RVA: 0x00027A3D File Offset: 0x00026A3D
		public bool GreaterEqualV351600
		{
			get
			{
				if (this._versionToUse == null)
				{
					this.CompilerVersionToUse();
				}
				return this._bGreaterEq351600;
			}
		}

		// Token: 0x17000454 RID: 1108
		// (get) Token: 0x06000F10 RID: 3856 RVA: 0x00027A5A File Offset: 0x00026A5A
		public bool GreaterEqualV351630
		{
			get
			{
				if (this._versionToUse == null)
				{
					this.CompilerVersionToUse();
				}
				return this._bGreaterEq351630;
			}
		}

		// Token: 0x17000455 RID: 1109
		// (get) Token: 0x06000F11 RID: 3857 RVA: 0x00027A77 File Offset: 0x00026A77
		public bool GreaterEqualV351700
		{
			get
			{
				if (this._versionToUse == null)
				{
					this.CompilerVersionToUse();
				}
				return this._bGreaterEq351700;
			}
		}

		// Token: 0x17000456 RID: 1110
		// (get) Token: 0x06000F12 RID: 3858 RVA: 0x00027A94 File Offset: 0x00026A94
		public bool GreaterEqualV351710
		{
			get
			{
				if (this._versionToUse == null)
				{
					this.CompilerVersionToUse();
				}
				return this._bGreaterEq351710;
			}
		}

		// Token: 0x17000457 RID: 1111
		// (get) Token: 0x06000F13 RID: 3859 RVA: 0x00027AB1 File Offset: 0x00026AB1
		public bool GreaterEqualV351740
		{
			get
			{
				if (this._versionToUse == null)
				{
					this.CompilerVersionToUse();
				}
				return this._bGreaterEq351740;
			}
		}

		// Token: 0x17000458 RID: 1112
		// (get) Token: 0x06000F14 RID: 3860 RVA: 0x00027ACE File Offset: 0x00026ACE
		public bool GreaterEqualV351760
		{
			get
			{
				if (this._versionToUse == null)
				{
					this.CompilerVersionToUse();
				}
				return this._bGreaterEq351760;
			}
		}

		// Token: 0x17000459 RID: 1113
		// (get) Token: 0x06000F15 RID: 3861 RVA: 0x00027AEB File Offset: 0x00026AEB
		public bool GreaterEqualV351800
		{
			get
			{
				if (this._versionToUse == null)
				{
					this.CompilerVersionToUse();
				}
				return this._bGreaterEq351800;
			}
		}

		// Token: 0x1700045A RID: 1114
		// (get) Token: 0x06000F16 RID: 3862 RVA: 0x00027B08 File Offset: 0x00026B08
		public bool GreaterEqualV351810
		{
			get
			{
				if (this._versionToUse == null)
				{
					this.CompilerVersionToUse();
				}
				return this._bGreaterEq351810;
			}
		}

		// Token: 0x1700045B RID: 1115
		// (get) Token: 0x06000F17 RID: 3863 RVA: 0x00027B25 File Offset: 0x00026B25
		public bool GreaterEqualV351840
		{
			get
			{
				if (this._versionToUse == null)
				{
					this.CompilerVersionToUse();
				}
				return this._bGreaterEq351840;
			}
		}

		// Token: 0x1700045C RID: 1116
		// (get) Token: 0x06000F18 RID: 3864 RVA: 0x00027B42 File Offset: 0x00026B42
		public bool GreaterEqualV351850
		{
			get
			{
				if (this._versionToUse == null)
				{
					this.CompilerVersionToUse();
				}
				return this._bGreaterEq351850;
			}
		}

		// Token: 0x1700045D RID: 1117
		// (get) Token: 0x06000F19 RID: 3865 RVA: 0x00027B5F File Offset: 0x00026B5F
		public bool GreaterEqualV351900
		{
			get
			{
				if (this._versionToUse == null)
				{
					this.CompilerVersionToUse();
				}
				return this._bGreaterEq351900;
			}
		}

		// Token: 0x1700045E RID: 1118
		// (get) Token: 0x06000F1A RID: 3866 RVA: 0x00027B7C File Offset: 0x00026B7C
		public bool GreaterEqualV351910
		{
			get
			{
				if (this._versionToUse == null)
				{
					this.CompilerVersionToUse();
				}
				return this._bGreaterEq351910;
			}
		}

		// Token: 0x1700045F RID: 1119
		// (get) Token: 0x06000F1B RID: 3867 RVA: 0x00027B99 File Offset: 0x00026B99
		public bool GreaterEqualV351920
		{
			get
			{
				if (this._versionToUse == null)
				{
					this.CompilerVersionToUse();
				}
				return this._bGreaterEq351920;
			}
		}

		// Token: 0x17000460 RID: 1120
		// (get) Token: 0x06000F1C RID: 3868 RVA: 0x00027BB6 File Offset: 0x00026BB6
		public bool GreaterEqualV351930
		{
			get
			{
				if (this._versionToUse == null)
				{
					this.CompilerVersionToUse();
				}
				return this._bGreaterEq351930;
			}
		}

		// Token: 0x17000461 RID: 1121
		// (get) Token: 0x06000F1D RID: 3869 RVA: 0x00027BD3 File Offset: 0x00026BD3
		public bool GreaterEqualV352000
		{
			get
			{
				if (this._versionToUse == null)
				{
					this.CompilerVersionToUse();
				}
				return this._bGreaterEq352000;
			}
		}

		// Token: 0x17000462 RID: 1122
		// (get) Token: 0x06000F1E RID: 3870 RVA: 0x00027BF0 File Offset: 0x00026BF0
		public bool GreaterEqualV352010
		{
			get
			{
				if (this._versionToUse == null)
				{
					this.CompilerVersionToUse();
				}
				return this._bGreaterEq352010;
			}
		}

		// Token: 0x17000463 RID: 1123
		// (get) Token: 0x06000F1F RID: 3871 RVA: 0x00027C0D File Offset: 0x00026C0D
		public bool GreaterEqualV352100
		{
			get
			{
				if (this._versionToUse == null)
				{
					this.CompilerVersionToUse();
				}
				return this._bGreaterEq352100;
			}
		}

		// Token: 0x17000464 RID: 1124
		// (get) Token: 0x06000F20 RID: 3872 RVA: 0x00027C2A File Offset: 0x00026C2A
		public bool GreaterEqualV352200
		{
			get
			{
				if (this._versionToUse == null)
				{
					this.CompilerVersionToUse();
				}
				return this._bGreaterEq352200;
			}
		}

		// Token: 0x06000F21 RID: 3873 RVA: 0x00027C48 File Offset: 0x00026C48
		private static IntegerUnion CreateIntegerUnion(ushort uGen, ushort uV, ushort uSP, ushort uPatch)
		{
			return new IntegerUnion
			{
				m_ushort0 = uPatch,
				m_ushort1 = uSP,
				m_ushort2 = uV,
				m_ushort3 = uGen
			};
		}

		// Token: 0x06000F22 RID: 3874 RVA: 0x00027C80 File Offset: 0x00026C80
		private static IntegerUnion CreateIntegerUnion(Version v)
		{
			return new IntegerUnion
			{
				m_ushort0 = (ushort)v.Revision,
				m_ushort1 = (ushort)v.Build,
				m_ushort2 = (ushort)v.Minor,
				m_ushort3 = (ushort)v.Major
			};
		}

		// Token: 0x06000F23 RID: 3875 RVA: 0x00027CD0 File Offset: 0x00026CD0
		private void UpdateShortcuts(Version version)
		{
			this._bGreaterEq3131 = (version >= CompilerVersionManager.V3131);
			this._bGreaterEq3200 = (version >= CompilerVersionManager.V3200);
			this._bGreaterEq3201 = (version >= CompilerVersionManager.V3201);
			this._bGreaterEq3202 = (version >= CompilerVersionManager.V3202);
			this._bGreaterEq3203 = (version >= CompilerVersionManager.V3203);
			this._bGreaterEq3204 = (version >= CompilerVersionManager.V3204);
			this._bGreaterEq3211 = (version >= CompilerVersionManager.V3211);
			this._bGreaterEq32120 = (version >= CompilerVersionManager.V32120);
			this._bGreaterEq32220 = (version >= CompilerVersionManager.V32220);
			this._bGreaterEq33000 = (version >= CompilerVersionManager.V33000);
			this._bGreaterEq33010 = (version >= CompilerVersionManager.V33010);
			this._bGreaterEq33020 = (version >= CompilerVersionManager.V33020);
			this._bGreaterEq33100 = (version >= CompilerVersionManager.V33100);
			this._bGreaterEq33102 = (version >= CompilerVersionManager.V33102);
			this._bGreaterEq33120 = (version >= CompilerVersionManager.V33120);
			this._bGreaterEq33140 = (version >= CompilerVersionManager.V33140);
			this._bGreaterEq33200 = (version >= CompilerVersionManager.V33200);
			this._bGreaterEq33220 = (version >= CompilerVersionManager.V33220);
			this._bGreaterEq33250 = (version >= CompilerVersionManager.V33250);
			this._bGreaterEq34000 = (version >= CompilerVersionManager.V34000);
			this._bGreaterEq34100 = (version >= CompilerVersionManager.V34100);
			this._bGreaterEq34110 = (version >= CompilerVersionManager.V34110);
			this._bGreaterEq34140 = (version >= CompilerVersionManager.V34140);
			this._bGreaterEq34180 = (version >= CompilerVersionManager.V34180);
			this._bGreaterEq34200 = (version >= CompilerVersionManager.V34200);
			this._bGreaterEq34300 = (version >= CompilerVersionManager.V34300);
			this._bGreaterEq34400 = (version >= CompilerVersionManager.V34400);
			this._bGreaterEq34421 = (version >= CompilerVersionManager.V34421);
			this._bGreaterEq34430 = (version >= CompilerVersionManager.V34430);
			this._bGreaterEq34451 = (version >= CompilerVersionManager.V34451);
			this._bGreaterEq34500 = (version >= CompilerVersionManager.V34500);
			this._bGreaterEq34561 = (version >= CompilerVersionManager.V34561);
			this._bGreaterEq345100 = (version >= CompilerVersionManager.V345100);
			this._bGreaterEq345110 = (version >= CompilerVersionManager.V345110);
			this._bGreaterEq345120 = (version >= CompilerVersionManager.V345120);
			this._bGreaterEq35000 = (version >= CompilerVersionManager.V35000);
			this._bGreaterEq35100 = (version >= CompilerVersionManager.V35100);
			this._bGreaterEq35110 = (version >= CompilerVersionManager.V35110);
			this._bGreaterEq35200 = (version >= CompilerVersionManager.V35200);
			this._bGreaterEq35300 = (version >= CompilerVersionManager.V35300);
			this._bGreaterEq35340 = (version >= CompilerVersionManager.V35340);
			this._bGreaterEq35350 = (version >= CompilerVersionManager.V35350);
			this._bGreaterEq35370 = (version >= CompilerVersionManager.V35370);
			this._bGreaterEq35400 = (version >= CompilerVersionManager.V35400);
			this._bGreaterEq35420 = (version >= CompilerVersionManager.V35420);
			this._bGreaterEq35430 = (version >= CompilerVersionManager.V35430);
			this._bGreaterEq35500 = (version >= CompilerVersionManager.V35500);
			this._bGreaterEq35510 = (version >= CompilerVersionManager.V35510);
			this._bGreaterEq35530 = (version >= CompilerVersionManager.V35530);
			this._bGreaterEq35600 = (version >= CompilerVersionManager.V35600);
			this._bGreaterEq35610 = (version >= CompilerVersionManager.V35610);
			this._bGreaterEq35620 = (version >= CompilerVersionManager.V35620);
			this._bGreaterEq35640 = (version >= CompilerVersionManager.V35640);
			this._bGreaterEq35660 = (version >= CompilerVersionManager.V35660);
			this._bGreaterEq35666 = (version >= CompilerVersionManager.V35666);
			this._bGreaterEq35670 = (version >= CompilerVersionManager.V35670);
			this._bGreaterEq35700 = (version >= CompilerVersionManager.V35700);
			this._bGreaterEq35720 = (version >= CompilerVersionManager.V35720);
			this._bGreaterEq35730 = (version >= CompilerVersionManager.V35730);
			this._bGreaterEq35800 = (version >= CompilerVersionManager.V35800);
			this._bGreaterEq35900 = (version >= CompilerVersionManager.V35900);
			this._bGreaterEq35940 = (version >= CompilerVersionManager.V35940);
			this._bGreaterEq35950 = (version >= CompilerVersionManager.V35950);
			this._bGreaterEq35980 = (version >= CompilerVersionManager.V35980);
			this._bGreaterEq351000 = (version >= CompilerVersionManager.V351000);
			this._bGreaterEq351020 = (version >= CompilerVersionManager.V351020);
			this._bGreaterEq351040 = (version >= CompilerVersionManager.V351040);
			this._bGreaterEq351050 = (version >= CompilerVersionManager.V351050);
			this._bGreaterEq351100 = (version >= CompilerVersionManager.V351100);
			this._bGreaterEq351110 = (version >= CompilerVersionManager.V351110);
			this._bGreaterEq351120 = (version >= CompilerVersionManager.V351120);
			this._bGreaterEq351200 = (version >= CompilerVersionManager.V351200);
			this._bGreaterEq351250 = (version >= CompilerVersionManager.V351250);
			this._bGreaterEq351300 = (version >= CompilerVersionManager.V351300);
			this._bGreaterEq351310 = (version >= CompilerVersionManager.V351310);
			this._bGreaterEq351400 = (version >= CompilerVersionManager.V351400);
			this._bGreaterEq351410 = (version >= CompilerVersionManager.V351410);
			this._bGreaterEq351430 = (version >= CompilerVersionManager.V351430);
			this._bGreaterEq351500 = (version >= CompilerVersionManager.V351500);
			this._bGreaterEq351600 = (version >= CompilerVersionManager.V351600);
			this._bGreaterEq351630 = (version >= CompilerVersionManager.V351630);
			this._bGreaterEq351700 = (version >= CompilerVersionManager.V351700);
			this._bGreaterEq351710 = (version >= CompilerVersionManager.V351710);
			this._bGreaterEq351740 = (version >= CompilerVersionManager.V351740);
			this._bGreaterEq351760 = (version >= CompilerVersionManager.V351760);
			this._bGreaterEq351800 = (version >= CompilerVersionManager.V351800);
			this._bGreaterEq351810 = (version >= CompilerVersionManager.V351810);
			this._bGreaterEq351840 = (version >= CompilerVersionManager.V351840);
			this._bGreaterEq351850 = (version >= CompilerVersionManager.V351850);
			this._bGreaterEq351900 = (version >= CompilerVersionManager.V351900);
			this._bGreaterEq351910 = (version >= CompilerVersionManager.V351910);
			this._bGreaterEq351920 = (version >= CompilerVersionManager.V351920);
			this._bGreaterEq351930 = (version >= CompilerVersionManager.V351930);
			this._bGreaterEq352000 = (version >= CompilerVersionManager.V352000);
			this._bGreaterEq352010 = (version >= CompilerVersionManager.V352010);
			this._bGreaterEq352100 = (version >= CompilerVersionManager.V352100);
			this._bGreaterEq352200 = (version >= CompilerVersionManager.V352200);
			this._versionToUseUnion = CompilerVersionManager.CreateIntegerUnion(version);
			this._versionToUse = version;
			CompilerProxy.UpdateScannerOperatorTable();
		}

		// Token: 0x06000F24 RID: 3876 RVA: 0x00028366 File Offset: 0x00027366
		[Obsolete("Use only for unit testing")]
		public void SetCompilerVersionForTest(Version version)
		{
			this._bUseNewest = false;
			this.UpdateShortcuts(version);
			CompilerProxy.UpdateScannerOperatorTable();
			this.CheckAndUpdateCompileOptions(version);
			VersionedCompilerFactory.Reset();
		}

		// Token: 0x06000F25 RID: 3877 RVA: 0x00028388 File Offset: 0x00027388
		public void SetCompilerVersionExact(Version version)
		{
			IUndoManager undoManager = null;
			int nProjectHandle;
			if (APEnvironmentFacade.Instance.DoesPrimaryProjectExist(out nProjectHandle))
			{
				undoManager = APEnvironmentFacade.Instance.GetUndoManager(nProjectHandle);
				if (undoManager != null)
				{
					undoManager.BeginCompoundAction(Strings.UpdatingCompilerVersion);
				}
			}
			try
			{
				bool bUseNewest = this._bUseNewest;
				Version version2 = this.CompilerVersionToUse();
				Exception ex;
				if (version != version2 && this.RaiseCompilerVersionChanging(this.CompilerVersionToUse(), version, out ex))
				{
					throw ex;
				}
				OptionsHelper.SetCompilerVersionConstraint(new ExactVersionConstraint(version));
				this._bUseNewest = false;
				this.UpdateShortcuts(version);
				if (bUseNewest)
				{
					this.RaiseUseNewestCompilerVersionChanged(true, false);
				}
				if (version != version2)
				{
					this.RaiseCompilerVersionChanged(version2, version);
				}
			}
			finally
			{
				if (undoManager != null && undoManager.InCompoundAction)
				{
					undoManager.EndCompoundAction();
				}
			}
		}

		// Token: 0x06000F26 RID: 3878 RVA: 0x00028444 File Offset: 0x00027444
		public bool CompilerVersionAvailable()
		{
			Version v = this.CompilerVersionToUse();
			Debug.Assert(v != null);
			foreach (Version v2 in this.CompilerVersionsAvailable)
			{
				if (v == v2)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06000F27 RID: 3879 RVA: 0x000284B0 File Offset: 0x000274B0
		public Version GetCustomizationVersion()
		{
			Version result;
			try
			{
				IOEMCustomization oemcustomization = APEnvironmentFacade.Instance.OEMCustomization;
				if (oemcustomization == null)
				{
					result = null;
				}
				else if (!oemcustomization.HasValue("LanguageModelManager", "CompilerVersionOverride"))
				{
					result = null;
				}
				else
				{
					result = new Version(oemcustomization.GetStringValue("LanguageModelManager", "CompilerVersionOverride"));
				}
			}
			catch
			{
				result = null;
			}
			return result;
		}

		// Token: 0x06000F28 RID: 3880 RVA: 0x00028514 File Offset: 0x00027514
		public Version CompilerVersionToUseInternal()
		{
			return this.CompilerVersionToUse();
		}

		// Token: 0x06000F29 RID: 3881 RVA: 0x0002851C File Offset: 0x0002751C
		public virtual Version CompilerVersionToUse()
		{
			object obj = this.lockobj;
			lock (obj)
			{
				if (this._versionToUse == null)
				{
					Version version = this.GetCustomizationVersion();
					if (version == null)
					{
						VersionConstraint versionConstraint = this.VersionConstraintFromOptions;
						if (versionConstraint == null)
						{
							versionConstraint = new NewestVersionConstraint();
						}
						this._bUseNewest = (versionConstraint is NewestVersionConstraint);
						version = versionConstraint.FindVersion(this.CompilerVersionsAvailable.ToArray<Version>());
						if (version == null && versionConstraint is ExactVersionConstraint)
						{
							version = (versionConstraint as ExactVersionConstraint).Version;
						}
						if (null == version)
						{
							Console.WriteLine(Strings.NoCompilerError);
							Environment.Exit(41096);
						}
					}
					this.UpdateShortcuts(version);
				}
			}
			return this._versionToUse;
		}

		// Token: 0x06000F2A RID: 3882 RVA: 0x000285F0 File Offset: 0x000275F0
		public Version CompilerVersionToUse(int nProjectHandle)
		{
			Version result = null;
			ExactVersionConstraint exactVersionConstraint = OptionsHelper.GetCompilerVersionConstraint(nProjectHandle) as ExactVersionConstraint;
			if (exactVersionConstraint != null)
			{
				result = exactVersionConstraint.Version;
			}
			return result;
		}

		// Token: 0x14000013 RID: 19
		// (add) Token: 0x06000F2B RID: 3883 RVA: 0x00028616 File Offset: 0x00027616
		// (remove) Token: 0x06000F2C RID: 3884 RVA: 0x0002862A File Offset: 0x0002762A
		public event UseNewestCompilerVersionChangingEventHandler UseNewestCompilerVersionChanging
		{
			add
			{
				this._ehUseNewestCompilerVersionChanging = WeakMulticastDelegate.CombineUnique(this._ehUseNewestCompilerVersionChanging, value);
			}
			remove
			{
				this._ehUseNewestCompilerVersionChanging = WeakMulticastDelegate.Remove(this._ehUseNewestCompilerVersionChanging, value);
			}
		}

		// Token: 0x14000014 RID: 20
		// (add) Token: 0x06000F2D RID: 3885 RVA: 0x0002863E File Offset: 0x0002763E
		// (remove) Token: 0x06000F2E RID: 3886 RVA: 0x00028652 File Offset: 0x00027652
		public event UseNewestCompilerVersionChangedEventHandler UseNewestCompilerVersionChanged
		{
			add
			{
				this._ehUseNewestCompilerVersionChanged = WeakMulticastDelegate.CombineUnique(this._ehUseNewestCompilerVersionChanged, value);
			}
			remove
			{
				this._ehUseNewestCompilerVersionChanged = WeakMulticastDelegate.Remove(this._ehUseNewestCompilerVersionChanged, value);
			}
		}

		// Token: 0x14000015 RID: 21
		// (add) Token: 0x06000F2F RID: 3887 RVA: 0x00028666 File Offset: 0x00027666
		// (remove) Token: 0x06000F30 RID: 3888 RVA: 0x0002867A File Offset: 0x0002767A
		public event CompilerVersionChangingEventHandler CompilerVersionChanging
		{
			add
			{
				this._ehCompilerVersionChanging = WeakMulticastDelegate.CombineUnique(this._ehCompilerVersionChanging, value);
			}
			remove
			{
				this._ehCompilerVersionChanging = WeakMulticastDelegate.Remove(this._ehCompilerVersionChanging, value);
			}
		}

		// Token: 0x14000016 RID: 22
		// (add) Token: 0x06000F31 RID: 3889 RVA: 0x0002868E File Offset: 0x0002768E
		// (remove) Token: 0x06000F32 RID: 3890 RVA: 0x000286A2 File Offset: 0x000276A2
		public event CompilerVersionChangedEventHandler CompilerVersionChanged
		{
			add
			{
				this._ehCompilerVersionChanged = WeakMulticastDelegate.CombineUnique(this._ehCompilerVersionChanged, value);
			}
			remove
			{
				this._ehCompilerVersionChanged = WeakMulticastDelegate.Remove(this._ehCompilerVersionChanged, value);
			}
		}

		// Token: 0x06000F33 RID: 3891 RVA: 0x000286B8 File Offset: 0x000276B8
		public void SetCompilerVersion(Version version)
		{
			if (version == null)
			{
				throw new ArgumentException("version");
			}
			if (Array.Find<Version>(this.AvailableCompilerVersions, (Version v) => v == version) == null)
			{
				throw new ArgumentException("version");
			}
			this.SetCompilerVersionExact(version);
		}

		// Token: 0x17000465 RID: 1125
		// (get) Token: 0x06000F34 RID: 3892 RVA: 0x00028724 File Offset: 0x00027724
		public Version[] AvailableCompilerVersionsOEMFilteredNotReplaced
		{
			get
			{
				Version[] array = APEnvironmentFacade.Instance.CompilerVersionMgr.AvailableCompilerVersions;
				List<Version> list = new List<Version>();
				foreach (Version version in array)
				{
					if (version != new Version(3, 4, 3, 0))
					{
						list.Add(version);
					}
				}
				array = list.ToArray();
				Version[] array2;
				try
				{
					IOEMCustomization oemcustomization = APEnvironmentFacade.Instance.OEMCustomization;
					if (oemcustomization == null)
					{
						array2 = array;
					}
					else if (!oemcustomization.HasValue("LanguageModelManager", "CompilerVersionFilter") && !oemcustomization.HasValue("LanguageModelManager", "CompilerVersionFilterEx"))
					{
						array2 = array;
					}
					else
					{
						List<Version> list2 = new List<Version>();
						foreach (Version version2 in array)
						{
							if (this.MapFromInternalToOEMText(version2) != null)
							{
								list2.Add(version2);
							}
						}
						array2 = list2.ToArray();
					}
				}
				catch
				{
					array2 = array;
				}
				return array2;
			}
		}

		// Token: 0x06000F35 RID: 3893 RVA: 0x00028810 File Offset: 0x00027810
		public string MapFromInternalToOEMTextSave(Version vinternal)
		{
			string text = this.MapFromInternalToOEMText(vinternal);
			if (text == null)
			{
				return vinternal.ToString();
			}
			return text;
		}

		// Token: 0x06000F36 RID: 3894 RVA: 0x00028830 File Offset: 0x00027830
		public string MapFromInternalToOEMText(Version vinternal)
		{
			IOEMCustomization oemcustomization = APEnvironmentFacade.Instance.OEMCustomization;
			if (oemcustomization == null)
			{
				return vinternal.ToString();
			}
			if (oemcustomization.HasValue("LanguageModelManager", "CompilerVersionFilterEx"))
			{
				string stKey = "ExtendedMapVersion:" + ((vinternal != null) ? vinternal.ToString() : null);
				if (!oemcustomization.HasValue("LanguageModelManager", stKey))
				{
					return null;
				}
				try
				{
					return oemcustomization.GetStringValue("LanguageModelManager", stKey);
				}
				catch
				{
					return null;
				}
			}
			if (oemcustomization.HasValue("LanguageModelManager", "CompilerVersionFilter"))
			{
				string stKey2 = "MapVersion:" + ((vinternal != null) ? vinternal.ToString() : null);
				if (!oemcustomization.HasValue("LanguageModelManager", stKey2))
				{
					return null;
				}
				try
				{
					return oemcustomization.GetStringValue("LanguageModelManager", stKey2);
				}
				catch
				{
					return null;
				}
			}
			return vinternal.ToString();
		}

		// Token: 0x06000F37 RID: 3895 RVA: 0x00028914 File Offset: 0x00027914
		public Version MapFromOEMTextToInternalSave(string stOEMString)
		{
			Version version = this.MapFromOEMTextToInternal(stOEMString);
			if (version == null && !Version.TryParse(stOEMString, out version))
			{
				return null;
			}
			return version;
		}

		// Token: 0x06000F38 RID: 3896 RVA: 0x00028940 File Offset: 0x00027940
		public Version MapFromOEMTextToInternal(string stDisplayText)
		{
			IOEMCustomization oemcustomization = APEnvironmentFacade.Instance.OEMCustomization;
			if (oemcustomization == null)
			{
				return new Version(stDisplayText);
			}
			if (!oemcustomization.HasValue("LanguageModelManager", "CompilerVersionFilterEx") && !oemcustomization.HasValue("LanguageModelManager", "CompilerVersionFilter"))
			{
				return new Version(stDisplayText);
			}
			foreach (Version version in APEnvironmentFacade.Instance.CompilerVersionMgr.AvailableCompilerVersions)
			{
				if (this.MapFromInternalToOEMText(version) == stDisplayText)
				{
					return version;
				}
			}
			return null;
		}

		// Token: 0x17000466 RID: 1126
		// (get) Token: 0x06000F39 RID: 3897 RVA: 0x000289C4 File Offset: 0x000279C4
		[Obsolete("now use AvailableCompilerVersionsOEMFilteredText")]
		public Version[] AvailableCompilerVersionsOEMFiltered
		{
			get
			{
				Version[] array = APEnvironmentFacade.Instance.CompilerVersionMgr.AvailableCompilerVersions;
				List<Version> list = new List<Version>();
				foreach (Version version in array)
				{
					if (version != new Version(3, 4, 3, 0))
					{
						list.Add(version);
					}
				}
				array = list.ToArray();
				Version[] array2;
				try
				{
					IOEMCustomization oemcustomization = APEnvironmentFacade.Instance.OEMCustomization;
					if (oemcustomization == null)
					{
						array2 = array;
					}
					else if (!oemcustomization.HasValue("LanguageModelManager", "CompilerVersionFilter"))
					{
						array2 = array;
					}
					else
					{
						List<Version> list2 = new List<Version>();
						foreach (Version vinternal in array)
						{
							Version version2 = APEnvironmentFacade.Instance.CompilerVersionMgr.MapFromInternalToOEM(vinternal);
							if (version2 != null)
							{
								list2.Add(version2);
							}
						}
						array2 = list2.ToArray();
					}
				}
				catch
				{
					array2 = array;
				}
				return array2;
			}
		}

		// Token: 0x06000F3A RID: 3898 RVA: 0x00028AB0 File Offset: 0x00027AB0
		[Obsolete("now use MapFromInternalToOEMText")]
		public Version MapFromInternalToOEM(Version vinternal)
		{
			IOEMCustomization oemcustomization = APEnvironmentFacade.Instance.OEMCustomization;
			if (oemcustomization == null)
			{
				return vinternal;
			}
			if (!oemcustomization.HasValue("LanguageModelManager", "CompilerVersionFilter"))
			{
				return vinternal;
			}
			string stKey = "MapVersion:" + vinternal.ToString();
			if (!oemcustomization.HasValue("LanguageModelManager", stKey))
			{
				return null;
			}
			Version result;
			try
			{
				result = new Version(oemcustomization.GetStringValue("LanguageModelManager", stKey));
			}
			catch
			{
				result = null;
			}
			return result;
		}

		// Token: 0x06000F3B RID: 3899 RVA: 0x00028B30 File Offset: 0x00027B30
		[Obsolete("now use MapFromOEMTextToInternal")]
		public Version MapFromOEMToInternal(Version voem)
		{
			IOEMCustomization oemcustomization = APEnvironmentFacade.Instance.OEMCustomization;
			if (oemcustomization == null)
			{
				return voem;
			}
			if (!oemcustomization.HasValue("LanguageModelManager", "CompilerVersionFilter"))
			{
				return voem;
			}
			foreach (Version version in APEnvironmentFacade.Instance.CompilerVersionMgr.AvailableCompilerVersions)
			{
				if (APEnvironmentFacade.Instance.CompilerVersionMgr.MapFromInternalToOEM(version) == voem)
				{
					return version;
				}
			}
			Debug.Assert(false);
			return null;
		}

		// Token: 0x14000017 RID: 23
		// (add) Token: 0x06000F3C RID: 3900 RVA: 0x00028BA4 File Offset: 0x00027BA4
		// (remove) Token: 0x06000F3D RID: 3901 RVA: 0x00028BB8 File Offset: 0x00027BB8
		public event UseNewestCompilerVersionChangedEventHandler AfterUseNewestCompilerVersionChanged
		{
			add
			{
				this._ehAfterUseNewestCompilerVersionChanged = WeakMulticastDelegate.CombineUnique(this._ehAfterUseNewestCompilerVersionChanged, value);
			}
			remove
			{
				this._ehAfterUseNewestCompilerVersionChanged = WeakMulticastDelegate.Remove(this._ehAfterUseNewestCompilerVersionChanged, value);
			}
		}

		// Token: 0x06000F3E RID: 3902 RVA: 0x00028BCC File Offset: 0x00027BCC
		private bool RaiseCompilerVersionChanging(Version vOld, Version vNew, out Exception exCancel)
		{
			exCancel = null;
			if (this._ehCompilerVersionChanging != null)
			{
				CompilerVersionChangingEventArgs compilerVersionChangingEventArgs = new CompilerVersionChangingEventArgs(vOld, vNew);
				this._ehCompilerVersionChanging.Invoke(new object[]
				{
					this,
					compilerVersionChangingEventArgs
				});
				exCancel = compilerVersionChangingEventArgs.Exception;
			}
			if (exCancel == null)
			{
				CompilerProxy.GetCheckerThread().Disable();
			}
			return exCancel != null;
		}

		// Token: 0x06000F3F RID: 3903 RVA: 0x00028C20 File Offset: 0x00027C20
		private void RaiseCompilerVersionChanged(Version vOld, Version vNew)
		{
			Debug.Assert(!APEnvironmentFacade.Instance.LanguageModelMgr.CompilationInProgress, "The compiler version may not be changed while a compile is in progress.");
			VersionedCompilerFactory.Reset();
			CompilerProxy.UpdateScannerOperatorTable();
			this.CheckAndUpdateCompileOptions(vNew);
			this.ClearLanguageModel();
			WeakMulticastDelegate ehCompilerVersionChanged = this._ehCompilerVersionChanged;
			if (ehCompilerVersionChanged != null)
			{
				ehCompilerVersionChanged.Invoke(new object[]
				{
					this,
					new CompilerVersionChangedEventArgs(vOld, vNew)
				});
			}
			_ICheckerThread checkerThread = CompilerProxy.GetCheckerThread();
			checkerThread.Enable();
			checkerThread.TryStart();
		}

		// Token: 0x06000F40 RID: 3904 RVA: 0x00028C98 File Offset: 0x00027C98
		private bool RaiseUseNewestCompilerVersionChanging(bool bOldValue, bool bNewValue, out Exception exCancel)
		{
			exCancel = null;
			if (this._ehUseNewestCompilerVersionChanging != null)
			{
				UseNewestCompilerVersionChangingEventArgs useNewestCompilerVersionChangingEventArgs = new UseNewestCompilerVersionChangingEventArgs(bOldValue, bNewValue);
				this._ehUseNewestCompilerVersionChanging.Invoke(new object[]
				{
					this,
					useNewestCompilerVersionChangingEventArgs
				});
				exCancel = useNewestCompilerVersionChangingEventArgs.Exception;
			}
			return exCancel != null;
		}

		// Token: 0x06000F41 RID: 3905 RVA: 0x00028CE0 File Offset: 0x00027CE0
		private void RaiseUseNewestCompilerVersionChanged(bool bOldValue, bool bNewValue)
		{
			if (this._ehUseNewestCompilerVersionChanged != null)
			{
				this._ehUseNewestCompilerVersionChanged.Invoke(new object[]
				{
					this,
					new UseNewestCompilerVersionChangedEventArgs(bOldValue, bNewValue)
				});
			}
			if (this._ehAfterUseNewestCompilerVersionChanged != null)
			{
				this._ehAfterUseNewestCompilerVersionChanged.Invoke(new object[]
				{
					this,
					new UseNewestCompilerVersionChangedEventArgs(bOldValue, bNewValue)
				});
			}
		}

		// Token: 0x06000F42 RID: 3906 RVA: 0x00028D3B File Offset: 0x00027D3B
		public void ClearLanguageModel()
		{
			APEnvironmentFacade.Instance.ExecuteCleanAllCommand(new string[]
			{
				"--force"
			});
		}

		// Token: 0x06000F43 RID: 3907 RVA: 0x00028D56 File Offset: 0x00027D56
		internal void CheckAndUpdateCompileOptions(Version vNew)
		{
			if (vNew < new Version("3.3.2.0"))
			{
				OptionsHelper.SetCompileOption("ReplaceConstants", true);
			}
			if (vNew < new Version("3.5.5.0"))
			{
				OptionsHelper.SetCompileOption("EnableBreakpointLogging", false);
			}
		}

		// Token: 0x06000F44 RID: 3908 RVA: 0x00028D94 File Offset: 0x00027D94
		public bool CompilerVersionGreaterEq(ushort Generation, ushort Version, ushort ServicePack, ushort Patch)
		{
			IntegerUnion integerUnion = CompilerVersionManager.CreateIntegerUnion(Generation, Version, ServicePack, Patch);
			return this._versionToUseUnion.m_ulong >= integerUnion.m_ulong;
		}

		// Token: 0x040002A4 RID: 676
		private Version _versionToUse;

		// Token: 0x040002A5 RID: 677
		private IntegerUnion _versionToUseUnion;

		// Token: 0x040002A6 RID: 678
		private WeakMulticastDelegate _ehCompilerVersionChanging;

		// Token: 0x040002A7 RID: 679
		private WeakMulticastDelegate _ehCompilerVersionChanged;

		// Token: 0x040002A8 RID: 680
		private WeakMulticastDelegate _ehUseNewestCompilerVersionChanging;

		// Token: 0x040002A9 RID: 681
		private WeakMulticastDelegate _ehUseNewestCompilerVersionChanged;

		// Token: 0x040002AA RID: 682
		private WeakMulticastDelegate _ehAfterUseNewestCompilerVersionChanged;

		// Token: 0x040002AB RID: 683
		private bool _bGreaterEq3131;

		// Token: 0x040002AC RID: 684
		private bool _bGreaterEq3200;

		// Token: 0x040002AD RID: 685
		private bool _bGreaterEq3201;

		// Token: 0x040002AE RID: 686
		private bool _bGreaterEq3202;

		// Token: 0x040002AF RID: 687
		private bool _bGreaterEq3203;

		// Token: 0x040002B0 RID: 688
		private bool _bGreaterEq3204;

		// Token: 0x040002B1 RID: 689
		private bool _bGreaterEq3211;

		// Token: 0x040002B2 RID: 690
		private bool _bGreaterEq32120;

		// Token: 0x040002B3 RID: 691
		private bool _bGreaterEq32220;

		// Token: 0x040002B4 RID: 692
		private bool _bGreaterEq33000;

		// Token: 0x040002B5 RID: 693
		private bool _bGreaterEq33010;

		// Token: 0x040002B6 RID: 694
		private bool _bGreaterEq33020;

		// Token: 0x040002B7 RID: 695
		private bool _bGreaterEq33100;

		// Token: 0x040002B8 RID: 696
		private bool _bGreaterEq33102;

		// Token: 0x040002B9 RID: 697
		private bool _bGreaterEq33120;

		// Token: 0x040002BA RID: 698
		private bool _bGreaterEq33140;

		// Token: 0x040002BB RID: 699
		private bool _bGreaterEq33200;

		// Token: 0x040002BC RID: 700
		private bool _bGreaterEq33220;

		// Token: 0x040002BD RID: 701
		private bool _bGreaterEq33250;

		// Token: 0x040002BE RID: 702
		private bool _bGreaterEq34000;

		// Token: 0x040002BF RID: 703
		private bool _bGreaterEq34100;

		// Token: 0x040002C0 RID: 704
		private bool _bGreaterEq34110;

		// Token: 0x040002C1 RID: 705
		private bool _bGreaterEq34140;

		// Token: 0x040002C2 RID: 706
		private bool _bGreaterEq34180;

		// Token: 0x040002C3 RID: 707
		private bool _bGreaterEq34200;

		// Token: 0x040002C4 RID: 708
		private bool _bGreaterEq34300;

		// Token: 0x040002C5 RID: 709
		private bool _bGreaterEq34400;

		// Token: 0x040002C6 RID: 710
		private bool _bGreaterEq34421;

		// Token: 0x040002C7 RID: 711
		private bool _bGreaterEq34430;

		// Token: 0x040002C8 RID: 712
		private bool _bGreaterEq34451;

		// Token: 0x040002C9 RID: 713
		private bool _bGreaterEq34500;

		// Token: 0x040002CA RID: 714
		private bool _bGreaterEq34561;

		// Token: 0x040002CB RID: 715
		private bool _bGreaterEq345100;

		// Token: 0x040002CC RID: 716
		private bool _bGreaterEq345110;

		// Token: 0x040002CD RID: 717
		private bool _bGreaterEq345120;

		// Token: 0x040002CE RID: 718
		private bool _bGreaterEq35000;

		// Token: 0x040002CF RID: 719
		private bool _bGreaterEq35100;

		// Token: 0x040002D0 RID: 720
		private bool _bGreaterEq35110;

		// Token: 0x040002D1 RID: 721
		private bool _bGreaterEq35200;

		// Token: 0x040002D2 RID: 722
		private bool _bGreaterEq35300;

		// Token: 0x040002D3 RID: 723
		private bool _bGreaterEq35340;

		// Token: 0x040002D4 RID: 724
		private bool _bGreaterEq35350;

		// Token: 0x040002D5 RID: 725
		private bool _bGreaterEq35370;

		// Token: 0x040002D6 RID: 726
		private bool _bGreaterEq35400;

		// Token: 0x040002D7 RID: 727
		private bool _bGreaterEq35420;

		// Token: 0x040002D8 RID: 728
		private bool _bGreaterEq35430;

		// Token: 0x040002D9 RID: 729
		private bool _bGreaterEq35500;

		// Token: 0x040002DA RID: 730
		private bool _bGreaterEq35510;

		// Token: 0x040002DB RID: 731
		private bool _bGreaterEq35530;

		// Token: 0x040002DC RID: 732
		private bool _bGreaterEq35600;

		// Token: 0x040002DD RID: 733
		private bool _bGreaterEq35610;

		// Token: 0x040002DE RID: 734
		private bool _bGreaterEq35620;

		// Token: 0x040002DF RID: 735
		private bool _bGreaterEq35640;

		// Token: 0x040002E0 RID: 736
		private bool _bGreaterEq35660;

		// Token: 0x040002E1 RID: 737
		private bool _bGreaterEq35666;

		// Token: 0x040002E2 RID: 738
		private bool _bGreaterEq35670;

		// Token: 0x040002E3 RID: 739
		private bool _bGreaterEq35700;

		// Token: 0x040002E4 RID: 740
		private bool _bGreaterEq35720;

		// Token: 0x040002E5 RID: 741
		private bool _bGreaterEq35730;

		// Token: 0x040002E6 RID: 742
		private bool _bGreaterEq35800;

		// Token: 0x040002E7 RID: 743
		private bool _bGreaterEq35900;

		// Token: 0x040002E8 RID: 744
		private bool _bGreaterEq35940;

		// Token: 0x040002E9 RID: 745
		private bool _bGreaterEq35950;

		// Token: 0x040002EA RID: 746
		private bool _bGreaterEq35980;

		// Token: 0x040002EB RID: 747
		private bool _bGreaterEq351000;

		// Token: 0x040002EC RID: 748
		private bool _bGreaterEq351020;

		// Token: 0x040002ED RID: 749
		private bool _bGreaterEq351040;

		// Token: 0x040002EE RID: 750
		private bool _bGreaterEq351050;

		// Token: 0x040002EF RID: 751
		private bool _bGreaterEq351100;

		// Token: 0x040002F0 RID: 752
		private bool _bGreaterEq351110;

		// Token: 0x040002F1 RID: 753
		private bool _bGreaterEq351120;

		// Token: 0x040002F2 RID: 754
		private bool _bGreaterEq351200;

		// Token: 0x040002F3 RID: 755
		private bool _bGreaterEq351250;

		// Token: 0x040002F4 RID: 756
		private bool _bGreaterEq351300;

		// Token: 0x040002F5 RID: 757
		private bool _bGreaterEq351310;

		// Token: 0x040002F6 RID: 758
		private bool _bGreaterEq351400;

		// Token: 0x040002F7 RID: 759
		private bool _bGreaterEq351410;

		// Token: 0x040002F8 RID: 760
		private bool _bGreaterEq351430;

		// Token: 0x040002F9 RID: 761
		private bool _bGreaterEq351500;

		// Token: 0x040002FA RID: 762
		private bool _bGreaterEq351600;

		// Token: 0x040002FB RID: 763
		private bool _bGreaterEq351630;

		// Token: 0x040002FC RID: 764
		private bool _bGreaterEq351700;

		// Token: 0x040002FD RID: 765
		private bool _bGreaterEq351710;

		// Token: 0x040002FE RID: 766
		private bool _bGreaterEq351740;

		// Token: 0x040002FF RID: 767
		private bool _bGreaterEq351760;

		// Token: 0x04000300 RID: 768
		private bool _bGreaterEq351800;

		// Token: 0x04000301 RID: 769
		private bool _bGreaterEq351810;

		// Token: 0x04000302 RID: 770
		private bool _bGreaterEq351840;

		// Token: 0x04000303 RID: 771
		private bool _bGreaterEq351850;

		// Token: 0x04000304 RID: 772
		private bool _bGreaterEq351900;

		// Token: 0x04000305 RID: 773
		private bool _bGreaterEq351910;

		// Token: 0x04000306 RID: 774
		private bool _bGreaterEq351920;

		// Token: 0x04000307 RID: 775
		private bool _bGreaterEq351930;

		// Token: 0x04000308 RID: 776
		private bool _bGreaterEq352000;

		// Token: 0x04000309 RID: 777
		private bool _bGreaterEq352010;

		// Token: 0x0400030A RID: 778
		private bool _bGreaterEq352100;

		// Token: 0x0400030B RID: 779
		private bool _bGreaterEq352200;

		// Token: 0x0400030C RID: 780
		private bool _bUseNewest;

		// Token: 0x0400030D RID: 781
		private static readonly Version V352200 = new Version("3.5.22.0");

		// Token: 0x0400030E RID: 782
		private static readonly Version V352100 = new Version("3.5.21.0");

		// Token: 0x0400030F RID: 783
		private static readonly Version V352010 = new Version("3.5.20.10");

		// Token: 0x04000310 RID: 784
		private static readonly Version V352000 = new Version("3.5.20.0");

		// Token: 0x04000311 RID: 785
		private static readonly Version V351930 = new Version("3.5.19.30");

		// Token: 0x04000312 RID: 786
		private static readonly Version V351920 = new Version("3.5.19.20");

		// Token: 0x04000313 RID: 787
		private static readonly Version V351910 = new Version("3.5.19.10");

		// Token: 0x04000314 RID: 788
		private static readonly Version V351900 = new Version("3.5.19.0");

		// Token: 0x04000315 RID: 789
		private static readonly Version V351850 = new Version("3.5.18.50");

		// Token: 0x04000316 RID: 790
		private static readonly Version V351840 = new Version("3.5.18.40");

		// Token: 0x04000317 RID: 791
		private static readonly Version V351810 = new Version("3.5.18.10");

		// Token: 0x04000318 RID: 792
		private static readonly Version V351800 = new Version("3.5.18.0");

		// Token: 0x04000319 RID: 793
		private static readonly Version V351760 = new Version("3.5.17.60");

		// Token: 0x0400031A RID: 794
		private static readonly Version V351740 = new Version("3.5.17.40");

		// Token: 0x0400031B RID: 795
		private static readonly Version V351710 = new Version("3.5.17.10");

		// Token: 0x0400031C RID: 796
		private static readonly Version V351700 = new Version("3.5.17.0");

		// Token: 0x0400031D RID: 797
		private static readonly Version V351630 = new Version("3.5.16.30");

		// Token: 0x0400031E RID: 798
		private static readonly Version V351600 = new Version("3.5.16.0");

		// Token: 0x0400031F RID: 799
		private static readonly Version V351500 = new Version("3.5.15.0");

		// Token: 0x04000320 RID: 800
		private static readonly Version V351430 = new Version("3.5.14.30");

		// Token: 0x04000321 RID: 801
		private static readonly Version V351410 = new Version("3.5.14.10");

		// Token: 0x04000322 RID: 802
		private static readonly Version V351400 = new Version("3.5.14.0");

		// Token: 0x04000323 RID: 803
		private static readonly Version V351310 = new Version("3.5.13.10");

		// Token: 0x04000324 RID: 804
		private static readonly Version V351300 = new Version("3.5.13.0");

		// Token: 0x04000325 RID: 805
		private static readonly Version V351250 = new Version("3.5.12.50");

		// Token: 0x04000326 RID: 806
		private static readonly Version V351200 = new Version("3.5.12.0");

		// Token: 0x04000327 RID: 807
		private static readonly Version V351120 = new Version("3.5.11.20");

		// Token: 0x04000328 RID: 808
		private static readonly Version V351110 = new Version("3.5.11.10");

		// Token: 0x04000329 RID: 809
		private static readonly Version V351100 = new Version("3.5.11.0");

		// Token: 0x0400032A RID: 810
		private static readonly Version V351050 = new Version("3.5.10.50");

		// Token: 0x0400032B RID: 811
		private static readonly Version V351040 = new Version("3.5.10.40");

		// Token: 0x0400032C RID: 812
		private static readonly Version V351020 = new Version("3.5.10.20");

		// Token: 0x0400032D RID: 813
		private static readonly Version V351000 = new Version("3.5.10.0");

		// Token: 0x0400032E RID: 814
		private static readonly Version V35980 = new Version("3.5.9.80");

		// Token: 0x0400032F RID: 815
		private static readonly Version V35950 = new Version("3.5.9.50");

		// Token: 0x04000330 RID: 816
		private static readonly Version V35940 = new Version("3.5.9.40");

		// Token: 0x04000331 RID: 817
		private static readonly Version V35900 = new Version("3.5.9.0");

		// Token: 0x04000332 RID: 818
		private static readonly Version V35800 = new Version("3.5.8.0");

		// Token: 0x04000333 RID: 819
		private static readonly Version V35730 = new Version("3.5.7.30");

		// Token: 0x04000334 RID: 820
		private static readonly Version V35720 = new Version("3.5.7.20");

		// Token: 0x04000335 RID: 821
		private static readonly Version V35700 = new Version("3.5.7.0");

		// Token: 0x04000336 RID: 822
		private static readonly Version V35670 = new Version("3.5.6.70");

		// Token: 0x04000337 RID: 823
		private static readonly Version V35666 = new Version("3.5.6.66");

		// Token: 0x04000338 RID: 824
		private static readonly Version V35660 = new Version("3.5.6.60");

		// Token: 0x04000339 RID: 825
		private static readonly Version V35640 = new Version("3.5.6.40");

		// Token: 0x0400033A RID: 826
		private static readonly Version V35620 = new Version("3.5.6.20");

		// Token: 0x0400033B RID: 827
		private static readonly Version V35610 = new Version("3.5.6.10");

		// Token: 0x0400033C RID: 828
		private static readonly Version V35600 = new Version("3.5.6.0");

		// Token: 0x0400033D RID: 829
		private static readonly Version V35530 = new Version("3.5.5.30");

		// Token: 0x0400033E RID: 830
		private static readonly Version V35510 = new Version("3.5.5.10");

		// Token: 0x0400033F RID: 831
		private static readonly Version V35500 = new Version("3.5.5.0");

		// Token: 0x04000340 RID: 832
		private static readonly Version V35430 = new Version("3.5.4.30");

		// Token: 0x04000341 RID: 833
		private static readonly Version V35420 = new Version("3.5.4.20");

		// Token: 0x04000342 RID: 834
		private static readonly Version V35400 = new Version("3.5.4.0");

		// Token: 0x04000343 RID: 835
		private static readonly Version V35370 = new Version("3.5.3.70");

		// Token: 0x04000344 RID: 836
		private static readonly Version V35350 = new Version("3.5.3.50");

		// Token: 0x04000345 RID: 837
		private static readonly Version V35340 = new Version("3.5.3.40");

		// Token: 0x04000346 RID: 838
		private static readonly Version V35300 = new Version("3.5.3.0");

		// Token: 0x04000347 RID: 839
		private static readonly Version V35200 = new Version("3.5.2.0");

		// Token: 0x04000348 RID: 840
		private static readonly Version V35110 = new Version("3.5.1.10");

		// Token: 0x04000349 RID: 841
		public static readonly Version V35100 = new Version("3.5.1.0");

		// Token: 0x0400034A RID: 842
		private static readonly Version V35000 = new Version("3.5.0.0");

		// Token: 0x0400034B RID: 843
		private static readonly Version V345120 = new Version("3.4.5.120");

		// Token: 0x0400034C RID: 844
		private static readonly Version V345110 = new Version("3.4.5.110");

		// Token: 0x0400034D RID: 845
		private static readonly Version V345100 = new Version("3.4.5.100");

		// Token: 0x0400034E RID: 846
		private static readonly Version V34561 = new Version("3.4.5.61");

		// Token: 0x0400034F RID: 847
		private static readonly Version V34500 = new Version("3.4.5.0");

		// Token: 0x04000350 RID: 848
		private static readonly Version V34451 = new Version("3.4.4.51");

		// Token: 0x04000351 RID: 849
		private static readonly Version V34430 = new Version("3.4.4.30");

		// Token: 0x04000352 RID: 850
		private static readonly Version V34421 = new Version("3.4.4.21");

		// Token: 0x04000353 RID: 851
		private static readonly Version V34400 = new Version("3.4.4.0");

		// Token: 0x04000354 RID: 852
		private static readonly Version V34300 = new Version("3.4.3.0");

		// Token: 0x04000355 RID: 853
		private static readonly Version V34200 = new Version("3.4.2.0");

		// Token: 0x04000356 RID: 854
		private static readonly Version V34180 = new Version("3.4.1.80");

		// Token: 0x04000357 RID: 855
		private static readonly Version V34140 = new Version("3.4.1.40");

		// Token: 0x04000358 RID: 856
		private static readonly Version V34110 = new Version("3.4.1.10");

		// Token: 0x04000359 RID: 857
		private static readonly Version V34100 = new Version("3.4.1.0");

		// Token: 0x0400035A RID: 858
		private static readonly Version V34000 = new Version("3.4.0.0");

		// Token: 0x0400035B RID: 859
		private static readonly Version V33250 = new Version("3.3.2.50");

		// Token: 0x0400035C RID: 860
		private static readonly Version V33220 = new Version("3.3.2.20");

		// Token: 0x0400035D RID: 861
		private static readonly Version V33200 = new Version("3.3.2.0");

		// Token: 0x0400035E RID: 862
		private static readonly Version V33140 = new Version("3.3.1.40");

		// Token: 0x0400035F RID: 863
		private static readonly Version V33120 = new Version("3.3.1.20");

		// Token: 0x04000360 RID: 864
		private static readonly Version V33102 = new Version("3.3.1.2");

		// Token: 0x04000361 RID: 865
		private static readonly Version V33100 = new Version("3.3.1.0");

		// Token: 0x04000362 RID: 866
		private static readonly Version V33020 = new Version("3.3.0.20");

		// Token: 0x04000363 RID: 867
		private static readonly Version V33010 = new Version("3.3.0.10");

		// Token: 0x04000364 RID: 868
		private static readonly Version V33000 = new Version("3.3.0.0");

		// Token: 0x04000365 RID: 869
		private static readonly Version V32220 = new Version("3.2.2.20");

		// Token: 0x04000366 RID: 870
		private static readonly Version V32120 = new Version("3.2.1.20");

		// Token: 0x04000367 RID: 871
		private static readonly Version V3211 = new Version("3.2.1.1");

		// Token: 0x04000368 RID: 872
		private static readonly Version V3204 = new Version("3.2.0.4");

		// Token: 0x04000369 RID: 873
		private static readonly Version V3203 = new Version("3.2.0.3");

		// Token: 0x0400036A RID: 874
		private static readonly Version V3202 = new Version("3.2.0.2");

		// Token: 0x0400036B RID: 875
		private static readonly Version V3201 = new Version("3.2.0.1");

		// Token: 0x0400036C RID: 876
		private static readonly Version V3200 = new Version("3.2.0.0");

		// Token: 0x0400036D RID: 877
		private static readonly Version V3131 = new Version("3.1.3.1");

		// Token: 0x0400036E RID: 878
		private readonly object lockobj = new object();
	}
}
