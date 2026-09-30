using System;
using \u0019;
using _3S.CoDeSys.Core.TargetSettings;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0016
{
	// Token: 0x020000D6 RID: 214
	internal abstract class \u0004 : ITargetSettingsUser
	{
		// Token: 0x1700044B RID: 1099
		// (get) Token: 0x06000EED RID: 3821 RVA: 0x00029540 File Offset: 0x00027740
		private static ILanguageModelManagerTargetSettings4 TargetSettings
		{
			get
			{
				ILanguageModelManagerTargetSettings4 result;
				if ((result = global::\u0016.\u0004.\u0001) == null)
				{
					result = (global::\u0016.\u0004.\u0001 = global::\u0019.\u0003.\u0001());
				}
				return result;
			}
		}

		// Token: 0x1700044C RID: 1100
		// (get) Token: 0x06000EEE RID: 3822 RVA: 0x00029558 File Offset: 0x00027758
		internal static IRegisteredTargetSetting RuntimeVersion
		{
			get
			{
				return global::\u0016.\u0004.TargetSettings.RuntimeVersion;
			}
		}

		// Token: 0x1700044D RID: 1101
		// (get) Token: 0x06000EEF RID: 3823 RVA: 0x00029564 File Offset: 0x00027764
		internal static IRegisteredTargetSetting MinimalSystem
		{
			get
			{
				return global::\u0016.\u0004.TargetSettings.MinimalSystem;
			}
		}

		// Token: 0x1700044E RID: 1102
		// (get) Token: 0x06000EF0 RID: 3824 RVA: 0x00029570 File Offset: 0x00027770
		internal static IRegisteredTargetSetting SimpleCycle
		{
			get
			{
				return global::\u0016.\u0004.TargetSettings.SimpleCycle;
			}
		}

		// Token: 0x1700044F RID: 1103
		// (get) Token: 0x06000EF1 RID: 3825 RVA: 0x0002957C File Offset: 0x0002777C
		internal static IRegisteredTargetSetting CycleControlVersion2
		{
			get
			{
				return global::\u0016.\u0004.TargetSettings.CycleControlVersion2;
			}
		}

		// Token: 0x17000450 RID: 1104
		// (get) Token: 0x06000EF2 RID: 3826 RVA: 0x00029588 File Offset: 0x00027788
		internal static IRegisteredTargetSetting CycleControlInIec
		{
			get
			{
				return global::\u0016.\u0004.TargetSettings.CycleControlInIec;
			}
		}

		// Token: 0x17000451 RID: 1105
		// (get) Token: 0x06000EF3 RID: 3827 RVA: 0x00029594 File Offset: 0x00027794
		internal static IRegisteredTargetSetting CompactDownload
		{
			get
			{
				return global::\u0016.\u0004.TargetSettings.CompactDownload;
			}
		}

		// Token: 0x17000452 RID: 1106
		// (get) Token: 0x06000EF4 RID: 3828 RVA: 0x000295A0 File Offset: 0x000277A0
		internal static IRegisteredTargetSetting CodegeneratorGuid
		{
			get
			{
				return global::\u0016.\u0004.TargetSettings.CodegeneratorGuid;
			}
		}

		// Token: 0x17000453 RID: 1107
		// (get) Token: 0x06000EF5 RID: 3829 RVA: 0x000295AC File Offset: 0x000277AC
		internal static IRegisteredTargetSetting BackendGuid
		{
			get
			{
				return global::\u0016.\u0004.TargetSettings.BackendGuid;
			}
		}

		// Token: 0x17000454 RID: 1108
		// (get) Token: 0x06000EF6 RID: 3830 RVA: 0x000295B8 File Offset: 0x000277B8
		internal static IRegisteredTargetSetting CompilerDefines
		{
			get
			{
				return global::\u0016.\u0004.TargetSettings.CompilerDefines;
			}
		}

		// Token: 0x17000455 RID: 1109
		// (get) Token: 0x06000EF7 RID: 3831 RVA: 0x000295C4 File Offset: 0x000277C4
		internal static IRegisteredTargetSetting LintDataTypes
		{
			get
			{
				return global::\u0016.\u0004.TargetSettings.LintDataTypes;
			}
		}

		// Token: 0x17000456 RID: 1110
		// (get) Token: 0x06000EF8 RID: 3832 RVA: 0x000295D0 File Offset: 0x000277D0
		internal static IRegisteredTargetSetting LRealDataType
		{
			get
			{
				return global::\u0016.\u0004.TargetSettings.LRealDataType;
			}
		}

		// Token: 0x17000457 RID: 1111
		// (get) Token: 0x06000EF9 RID: 3833 RVA: 0x000295DC File Offset: 0x000277DC
		internal static IRegisteredTargetSetting NoBytesInRetain
		{
			get
			{
				return global::\u0016.\u0004.TargetSettings.NoBytesInRetain;
			}
		}

		// Token: 0x17000458 RID: 1112
		// (get) Token: 0x06000EFA RID: 3834 RVA: 0x000295E8 File Offset: 0x000277E8
		internal static IRegisteredTargetSetting CheckMisalignedAddress
		{
			get
			{
				return global::\u0016.\u0004.TargetSettings.CheckMisalignedAddress;
			}
		}

		// Token: 0x17000459 RID: 1113
		// (get) Token: 0x06000EFB RID: 3835 RVA: 0x000295F4 File Offset: 0x000277F4
		internal static IRegisteredTargetSetting UnsupportedOperators
		{
			get
			{
				return global::\u0016.\u0004.TargetSettings.UnsupportedOperators;
			}
		}

		// Token: 0x1700045A RID: 1114
		// (get) Token: 0x06000EFC RID: 3836 RVA: 0x00029600 File Offset: 0x00027800
		internal static IRegisteredTargetSetting SupportedSystemOperators
		{
			get
			{
				return global::\u0016.\u0004.TargetSettings.SupportedSystemOperators;
			}
		}

		// Token: 0x1700045B RID: 1115
		// (get) Token: 0x06000EFD RID: 3837 RVA: 0x0002960C File Offset: 0x0002780C
		internal static IRegisteredTargetSetting GenerateDirectCalls
		{
			get
			{
				return global::\u0016.\u0004.TargetSettings.GenerateDirectCalls;
			}
		}

		// Token: 0x1700045C RID: 1116
		// (get) Token: 0x06000EFE RID: 3838 RVA: 0x00029618 File Offset: 0x00027818
		internal static IRegisteredTargetSetting GvlInitFunctions
		{
			get
			{
				return global::\u0016.\u0004.TargetSettings.GvlInitFunctions;
			}
		}

		// Token: 0x1700045D RID: 1117
		// (get) Token: 0x06000EFF RID: 3839 RVA: 0x00029624 File Offset: 0x00027824
		internal static IRegisteredTargetSetting DoPersistentCode
		{
			get
			{
				return global::\u0016.\u0004.TargetSettings.DoPersistentCode;
			}
		}

		// Token: 0x1700045E RID: 1118
		// (get) Token: 0x06000F00 RID: 3840 RVA: 0x00029630 File Offset: 0x00027830
		internal static IRegisteredTargetSetting VarconfigGeneratorGuid
		{
			get
			{
				return global::\u0016.\u0004.TargetSettings.VarconfigGeneratorGuid;
			}
		}

		// Token: 0x1700045F RID: 1119
		// (get) Token: 0x06000F01 RID: 3841 RVA: 0x0002963C File Offset: 0x0002783C
		internal static IRegisteredTargetSetting MemoryAllocationCallback
		{
			get
			{
				return global::\u0016.\u0004.TargetSettings.MemoryAllocationCallback;
			}
		}

		// Token: 0x17000460 RID: 1120
		// (get) Token: 0x06000F02 RID: 3842 RVA: 0x00029648 File Offset: 0x00027848
		internal static IRegisteredTargetSetting CodegenMultithreading
		{
			get
			{
				return global::\u0016.\u0004.TargetSettings.CodegenMultithreading;
			}
		}

		// Token: 0x17000461 RID: 1121
		// (get) Token: 0x06000F03 RID: 3843 RVA: 0x00029654 File Offset: 0x00027854
		internal static IRegisteredTargetSetting CheckMultipleTaskOutputWrite
		{
			get
			{
				return global::\u0016.\u0004.TargetSettings.CheckMultipleTaskOutputWrite;
			}
		}

		// Token: 0x17000462 RID: 1122
		// (get) Token: 0x06000F04 RID: 3844 RVA: 0x00029660 File Offset: 0x00027860
		internal static IRegisteredTargetSetting MaximumNumApplications
		{
			get
			{
				return global::\u0016.\u0004.TargetSettings.MaximumNumApplications;
			}
		}

		// Token: 0x17000463 RID: 1123
		// (get) Token: 0x06000F05 RID: 3845 RVA: 0x0002966C File Offset: 0x0002786C
		internal static IRegisteredTargetSetting AddressCalculatorGuid
		{
			get
			{
				return global::\u0016.\u0004.TargetSettings.AddressCalculatorGuid;
			}
		}

		// Token: 0x17000464 RID: 1124
		// (get) Token: 0x06000F06 RID: 3846 RVA: 0x00029678 File Offset: 0x00027878
		internal static IRegisteredTargetSetting ExternalRealStringConversions
		{
			get
			{
				return global::\u0016.\u0004.TargetSettings.ExternalRealStringConversions;
			}
		}

		// Token: 0x17000465 RID: 1125
		// (get) Token: 0x06000F07 RID: 3847 RVA: 0x00029684 File Offset: 0x00027884
		internal static IRegisteredTargetSetting SupportMulticore
		{
			get
			{
				return global::\u0016.\u0004.TargetSettings.SupportMulticore;
			}
		}

		// Token: 0x17000466 RID: 1126
		// (get) Token: 0x06000F08 RID: 3848 RVA: 0x00029690 File Offset: 0x00027890
		internal static IRegisteredTargetSetting PackMode
		{
			get
			{
				return global::\u0016.\u0004.TargetSettings.PackMode;
			}
		}

		// Token: 0x17000467 RID: 1127
		// (get) Token: 0x06000F09 RID: 3849 RVA: 0x0002969C File Offset: 0x0002789C
		internal static IRegisteredTargetSetting ConstantsInOwnSegment
		{
			get
			{
				return global::\u0016.\u0004.TargetSettings.ConstantsInOwnSegment;
			}
		}

		// Token: 0x17000468 RID: 1128
		// (get) Token: 0x06000F0A RID: 3850 RVA: 0x000296A8 File Offset: 0x000278A8
		internal static IRegisteredTargetSetting MaxStackSize
		{
			get
			{
				return global::\u0016.\u0004.TargetSettings.MaxStackSize;
			}
		}

		// Token: 0x17000469 RID: 1129
		// (get) Token: 0x06000F0B RID: 3851 RVA: 0x000296B4 File Offset: 0x000278B4
		internal static IRegisteredTargetSetting MaxStackSizeExternalCall
		{
			get
			{
				return global::\u0016.\u0004.TargetSettings.MaxStackSizeExternalCall;
			}
		}

		// Token: 0x1700046A RID: 1130
		// (get) Token: 0x06000F0C RID: 3852 RVA: 0x000296C0 File Offset: 0x000278C0
		internal static IRegisteredTargetSetting ReportStackCheckRecursionWarningAsError
		{
			get
			{
				return global::\u0016.\u0004.TargetSettings.ReportStackCheckRecursionWarningAsError;
			}
		}

		// Token: 0x1700046B RID: 1131
		// (get) Token: 0x06000F0D RID: 3853 RVA: 0x000296CC File Offset: 0x000278CC
		internal static IRegisteredTargetSetting SupportUserCheckFunctions
		{
			get
			{
				return global::\u0016.\u0004.TargetSettings.SupportUserCheckFunctions;
			}
		}

		// Token: 0x1700046C RID: 1132
		// (get) Token: 0x06000F0E RID: 3854 RVA: 0x000296D8 File Offset: 0x000278D8
		internal static IRegisteredTargetSetting MemoryBarrier
		{
			get
			{
				return global::\u0016.\u0004.TargetSettings.MemoryBarrier;
			}
		}

		// Token: 0x1700046D RID: 1133
		// (get) Token: 0x06000F0F RID: 3855 RVA: 0x000296E4 File Offset: 0x000278E4
		internal static IRegisteredTargetSetting SingleOnlineChangeArea
		{
			get
			{
				return global::\u0016.\u0004.TargetSettings.SingleOnlineChangeArea;
			}
		}

		// Token: 0x1700046E RID: 1134
		// (get) Token: 0x06000F10 RID: 3856 RVA: 0x000296F0 File Offset: 0x000278F0
		internal static IRegisteredTargetSetting ByteSupport
		{
			get
			{
				return global::\u0016.\u0004.TargetSettings.ByteSupport;
			}
		}

		// Token: 0x1700046F RID: 1135
		// (get) Token: 0x06000F11 RID: 3857 RVA: 0x000296FC File Offset: 0x000278FC
		internal static IRegisteredTargetSetting UnsupportedDataTypes
		{
			get
			{
				return global::\u0016.\u0004.TargetSettings.UnsupportedDataTypes;
			}
		}

		// Token: 0x17000470 RID: 1136
		// (get) Token: 0x06000F12 RID: 3858 RVA: 0x00029708 File Offset: 0x00027908
		internal static IRegisteredTargetSetting LRealAsReal
		{
			get
			{
				return global::\u0016.\u0004.TargetSettings.LRealAsReal;
			}
		}

		// Token: 0x17000471 RID: 1137
		// (get) Token: 0x06000F13 RID: 3859 RVA: 0x00029714 File Offset: 0x00027914
		internal static IRegisteredTargetSetting LinkAllGlobalVariables
		{
			get
			{
				return global::\u0016.\u0004.TargetSettings.LinkAllGlobalVariables;
			}
		}

		// Token: 0x040002A1 RID: 673
		private static ILanguageModelManagerTargetSettings4 \u0001;
	}
}
