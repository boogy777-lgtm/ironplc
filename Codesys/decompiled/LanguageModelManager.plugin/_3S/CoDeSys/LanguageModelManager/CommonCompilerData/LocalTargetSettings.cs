using System;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.TargetSettings;

namespace _3S.CoDeSys.LanguageModelManager.CommonCompilerData
{
	// Token: 0x020001CF RID: 463
	[TypeGuid("{477B6BC5-2E9F-4EBF-97AB-87CBDDEDB6AA}")]
	internal class LocalTargetSettings : ITargetSettingsUser
	{
		// Token: 0x1700087A RID: 2170
		// (get) Token: 0x06002079 RID: 8313 RVA: 0x00059B93 File Offset: 0x00058B93
		internal static IRegisteredTargetSetting RuntimeVersion
		{
			get
			{
				return APEnvironmentFacade.Instance.GetTargetSetting("runtime_identification\\version");
			}
		}

		// Token: 0x1700087B RID: 2171
		// (get) Token: 0x0600207A RID: 8314 RVA: 0x00059BA4 File Offset: 0x00058BA4
		internal static IRegisteredTargetSetting MinimalSystem
		{
			get
			{
				return APEnvironmentFacade.Instance.GetTargetSetting("runtime_features\\minimal_system");
			}
		}

		// Token: 0x1700087C RID: 2172
		// (get) Token: 0x0600207B RID: 8315 RVA: 0x00059BB5 File Offset: 0x00058BB5
		internal static IRegisteredTargetSetting SimpleCycle
		{
			get
			{
				return APEnvironmentFacade.Instance.GetTargetSetting("runtime_features\\simple_cycle");
			}
		}

		// Token: 0x1700087D RID: 2173
		// (get) Token: 0x0600207C RID: 8316 RVA: 0x00059BC6 File Offset: 0x00058BC6
		internal static IRegisteredTargetSetting CycleControlVersion2
		{
			get
			{
				return APEnvironmentFacade.Instance.GetTargetSetting("runtime_features\\cycle_control_version_2");
			}
		}

		// Token: 0x1700087E RID: 2174
		// (get) Token: 0x0600207D RID: 8317 RVA: 0x00059BD7 File Offset: 0x00058BD7
		internal static IRegisteredTargetSetting CycleControlInIec
		{
			get
			{
				return APEnvironmentFacade.Instance.GetTargetSetting("runtime_features\\cycle_control_in_iec");
			}
		}

		// Token: 0x1700087F RID: 2175
		// (get) Token: 0x0600207E RID: 8318 RVA: 0x00059BE8 File Offset: 0x00058BE8
		internal static IRegisteredTargetSetting CompactDownload
		{
			get
			{
				return APEnvironmentFacade.Instance.GetTargetSetting("runtime_features\\compact_download");
			}
		}

		// Token: 0x17000880 RID: 2176
		// (get) Token: 0x0600207F RID: 8319 RVA: 0x00059BF9 File Offset: 0x00058BF9
		internal static IRegisteredTargetSetting BreakpointsSupported
		{
			get
			{
				return APEnvironmentFacade.Instance.GetTargetSetting("runtime_features\\breakpoints_supported");
			}
		}

		// Token: 0x17000881 RID: 2177
		// (get) Token: 0x06002080 RID: 8320 RVA: 0x00059C0A File Offset: 0x00058C0A
		internal static IRegisteredTargetSetting EnableBreakpointLogging
		{
			get
			{
				return APEnvironmentFacade.Instance.GetTargetSetting("runtime_features\\enable_breakpoint_logging");
			}
		}

		// Token: 0x17000882 RID: 2178
		// (get) Token: 0x06002081 RID: 8321 RVA: 0x00059C1B File Offset: 0x00058C1B
		internal static IRegisteredTargetSetting OptimizedOnlineChange
		{
			get
			{
				return APEnvironmentFacade.Instance.GetTargetSetting("runtime_features\\optimized_online_change");
			}
		}

		// Token: 0x17000883 RID: 2179
		// (get) Token: 0x06002082 RID: 8322 RVA: 0x00059C2C File Offset: 0x00058C2C
		internal static IRegisteredTargetSetting CodegeneratorGuid
		{
			get
			{
				return APEnvironmentFacade.Instance.GetTargetSetting("codegenerator\\codegenerator-guid");
			}
		}

		// Token: 0x17000884 RID: 2180
		// (get) Token: 0x06002083 RID: 8323 RVA: 0x00059C3D File Offset: 0x00058C3D
		internal static IRegisteredTargetSetting CompilerDefines
		{
			get
			{
				return APEnvironmentFacade.Instance.GetTargetSetting("codegenerator\\compiler-defines");
			}
		}

		// Token: 0x17000885 RID: 2181
		// (get) Token: 0x06002084 RID: 8324 RVA: 0x00059C4E File Offset: 0x00058C4E
		internal static IRegisteredTargetSetting SupportSystemApplications
		{
			get
			{
				return APEnvironmentFacade.Instance.GetTargetSetting("codegenerator\\support-system-applications");
			}
		}

		// Token: 0x17000886 RID: 2182
		// (get) Token: 0x06002085 RID: 8325 RVA: 0x00059C5F File Offset: 0x00058C5F
		internal static IRegisteredTargetSetting NewVfTable
		{
			get
			{
				return APEnvironmentFacade.Instance.GetTargetSetting("codegenerator\\new_vf_table");
			}
		}

		// Token: 0x17000887 RID: 2183
		// (get) Token: 0x06002086 RID: 8326 RVA: 0x00059C70 File Offset: 0x00058C70
		internal static IRegisteredTargetSetting LintDataTypes
		{
			get
			{
				return APEnvironmentFacade.Instance.GetTargetSetting("codegenerator\\lint-data-types");
			}
		}

		// Token: 0x17000888 RID: 2184
		// (get) Token: 0x06002087 RID: 8327 RVA: 0x00059C81 File Offset: 0x00058C81
		internal static IRegisteredTargetSetting LRealDataType
		{
			get
			{
				return APEnvironmentFacade.Instance.GetTargetSetting("codegenerator\\lreal-data-type");
			}
		}

		// Token: 0x17000889 RID: 2185
		// (get) Token: 0x06002088 RID: 8328 RVA: 0x00059C92 File Offset: 0x00058C92
		internal static IRegisteredTargetSetting NoPrecompileMessages
		{
			get
			{
				return APEnvironmentFacade.Instance.GetTargetSetting("codegenerator\\no-precompile-messages");
			}
		}

		// Token: 0x1700088A RID: 2186
		// (get) Token: 0x06002089 RID: 8329 RVA: 0x00059CA3 File Offset: 0x00058CA3
		internal static IRegisteredTargetSetting NoBytesInRetain
		{
			get
			{
				return APEnvironmentFacade.Instance.GetTargetSetting("codegenerator\\no-bytes-in-retain");
			}
		}

		// Token: 0x1700088B RID: 2187
		// (get) Token: 0x0600208A RID: 8330 RVA: 0x00059CB4 File Offset: 0x00058CB4
		internal static IRegisteredTargetSetting CheckMisalignedAddress
		{
			get
			{
				return APEnvironmentFacade.Instance.GetTargetSetting("codegenerator\\check_misaligned_address");
			}
		}

		// Token: 0x1700088C RID: 2188
		// (get) Token: 0x0600208B RID: 8331 RVA: 0x00059CC5 File Offset: 0x00058CC5
		internal static IRegisteredTargetSetting LinkAllGlobalVariables
		{
			get
			{
				return APEnvironmentFacade.Instance.GetTargetSetting("codegenerator\\link-all-globalvariables");
			}
		}

		// Token: 0x1700088D RID: 2189
		// (get) Token: 0x0600208C RID: 8332 RVA: 0x00059CD6 File Offset: 0x00058CD6
		internal static IRegisteredTargetSetting NoDefaultInitialisation
		{
			get
			{
				return APEnvironmentFacade.Instance.GetTargetSetting("codegenerator\\no_default_initialisation");
			}
		}

		// Token: 0x1700088E RID: 2190
		// (get) Token: 0x0600208D RID: 8333 RVA: 0x00059CE7 File Offset: 0x00058CE7
		internal static IRegisteredTargetSetting UnsupportedOperators
		{
			get
			{
				return APEnvironmentFacade.Instance.GetTargetSetting("codegenerator\\unsupported-operators");
			}
		}

		// Token: 0x1700088F RID: 2191
		// (get) Token: 0x0600208E RID: 8334 RVA: 0x00059CF8 File Offset: 0x00058CF8
		internal static IRegisteredTargetSetting SupportedSystemOperators
		{
			get
			{
				return APEnvironmentFacade.Instance.GetTargetSetting("codegenerator\\supported-system-operators");
			}
		}

		// Token: 0x17000890 RID: 2192
		// (get) Token: 0x0600208F RID: 8335 RVA: 0x00059D09 File Offset: 0x00058D09
		internal static IRegisteredTargetSetting ByteSupport
		{
			get
			{
				return APEnvironmentFacade.Instance.GetTargetSetting(CGConstants.TRG_BYTESUPPORT);
			}
		}

		// Token: 0x17000891 RID: 2193
		// (get) Token: 0x06002090 RID: 8336 RVA: 0x00059D1A File Offset: 0x00058D1A
		internal static IRegisteredTargetSetting LRealAsReal
		{
			get
			{
				return APEnvironmentFacade.Instance.GetTargetSetting("codegenerator\\lreal-as-real");
			}
		}

		// Token: 0x17000892 RID: 2194
		// (get) Token: 0x06002091 RID: 8337 RVA: 0x00059D2B File Offset: 0x00058D2B
		internal static IRegisteredTargetSetting Int64AsInt32
		{
			get
			{
				return APEnvironmentFacade.Instance.GetTargetSetting("codegenerator\\int64-as-int32");
			}
		}

		// Token: 0x17000893 RID: 2195
		// (get) Token: 0x06002092 RID: 8338 RVA: 0x00059D3C File Offset: 0x00058D3C
		internal static IRegisteredTargetSetting GenerateDirectCalls
		{
			get
			{
				return APEnvironmentFacade.Instance.GetTargetSetting("codegenerator\\generate_direct_calls");
			}
		}

		// Token: 0x17000894 RID: 2196
		// (get) Token: 0x06002093 RID: 8339 RVA: 0x00059D4D File Offset: 0x00058D4D
		internal static IRegisteredTargetSetting GvlInitFunctions
		{
			get
			{
				return APEnvironmentFacade.Instance.GetTargetSetting("codegenerator\\gvl_init_functions");
			}
		}

		// Token: 0x17000895 RID: 2197
		// (get) Token: 0x06002094 RID: 8340 RVA: 0x00059D5E File Offset: 0x00058D5E
		internal static IRegisteredTargetSetting RetainInCycle
		{
			get
			{
				return APEnvironmentFacade.Instance.GetTargetSetting("codegenerator\\retain-in-cycle");
			}
		}

		// Token: 0x17000896 RID: 2198
		// (get) Token: 0x06002095 RID: 8341 RVA: 0x00059D6F File Offset: 0x00058D6F
		internal static IRegisteredTargetSetting RetainCycleTask
		{
			get
			{
				return APEnvironmentFacade.Instance.GetTargetSetting("codegenerator\\retain-cycle-task");
			}
		}

		// Token: 0x17000897 RID: 2199
		// (get) Token: 0x06002096 RID: 8342 RVA: 0x00059D80 File Offset: 0x00058D80
		internal static IRegisteredTargetSetting RetainInCycleUseCopyFunction
		{
			get
			{
				return APEnvironmentFacade.Instance.GetTargetSetting("codegenerator\\retain-in-cycle-use-copy-function");
			}
		}

		// Token: 0x17000898 RID: 2200
		// (get) Token: 0x06002097 RID: 8343 RVA: 0x00059D91 File Offset: 0x00058D91
		internal static IRegisteredTargetSetting DoPersistentCode
		{
			get
			{
				return APEnvironmentFacade.Instance.GetTargetSetting("codegenerator\\do-persistent-code");
			}
		}

		// Token: 0x17000899 RID: 2201
		// (get) Token: 0x06002098 RID: 8344 RVA: 0x00059DA2 File Offset: 0x00058DA2
		internal static IRegisteredTargetSetting VarconfigGeneratorGuid
		{
			get
			{
				return APEnvironmentFacade.Instance.GetTargetSetting("codegenerator\\varconfig-generator-guid");
			}
		}

		// Token: 0x1700089A RID: 2202
		// (get) Token: 0x06002099 RID: 8345 RVA: 0x00059DB3 File Offset: 0x00058DB3
		internal static IRegisteredTargetSetting MemoryAllocationCallback
		{
			get
			{
				return APEnvironmentFacade.Instance.GetTargetSetting("codegenerator\\memory-allocation-callback");
			}
		}

		// Token: 0x1700089B RID: 2203
		// (get) Token: 0x0600209A RID: 8346 RVA: 0x00059DC4 File Offset: 0x00058DC4
		internal static IRegisteredTargetSetting CodegenMultithreading
		{
			get
			{
				return APEnvironmentFacade.Instance.GetTargetSetting("codegenerator\\multithreading");
			}
		}

		// Token: 0x1700089C RID: 2204
		// (get) Token: 0x0600209B RID: 8347 RVA: 0x00059DD5 File Offset: 0x00058DD5
		internal static IRegisteredTargetSetting CheckMultipleTaskOutputWrite
		{
			get
			{
				return APEnvironmentFacade.Instance.GetTargetSetting("codegenerator\\check-multiple-task-output-write");
			}
		}

		// Token: 0x1700089D RID: 2205
		// (get) Token: 0x0600209C RID: 8348 RVA: 0x00059DE6 File Offset: 0x00058DE6
		internal static IRegisteredTargetSetting MaximumNumApplications
		{
			get
			{
				return APEnvironmentFacade.Instance.GetTargetSetting("codegenerator\\maximum_num_applications");
			}
		}

		// Token: 0x1700089E RID: 2206
		// (get) Token: 0x0600209D RID: 8349 RVA: 0x00059DF7 File Offset: 0x00058DF7
		internal static IRegisteredTargetSetting UnsupportedDataTypes
		{
			get
			{
				return APEnvironmentFacade.Instance.GetTargetSetting("codegenerator\\unsupported-data-types");
			}
		}

		// Token: 0x1700089F RID: 2207
		// (get) Token: 0x0600209E RID: 8350 RVA: 0x00059E08 File Offset: 0x00058E08
		internal static IRegisteredTargetSetting AddressCalculatorGuid
		{
			get
			{
				return APEnvironmentFacade.Instance.GetTargetSetting("codegenerator\\address-calculator-guid");
			}
		}

		// Token: 0x170008A0 RID: 2208
		// (get) Token: 0x0600209F RID: 8351 RVA: 0x00059E19 File Offset: 0x00058E19
		internal static IRegisteredTargetSetting ExternalRealStringConversions
		{
			get
			{
				return APEnvironmentFacade.Instance.GetTargetSetting("codegenerator\\external-real-string-conversions");
			}
		}

		// Token: 0x170008A1 RID: 2209
		// (get) Token: 0x060020A0 RID: 8352 RVA: 0x00059E2A File Offset: 0x00058E2A
		internal static IRegisteredTargetSetting SupportMulticore
		{
			get
			{
				return APEnvironmentFacade.Instance.GetTargetSetting("runtime_features\\support-multicore");
			}
		}

		// Token: 0x170008A2 RID: 2210
		// (get) Token: 0x060020A1 RID: 8353 RVA: 0x00059E3B File Offset: 0x00058E3B
		internal static IRegisteredTargetSetting MemorySize
		{
			get
			{
				return APEnvironmentFacade.Instance.GetTargetSetting("memory-layout\\memory-size");
			}
		}

		// Token: 0x170008A3 RID: 2211
		// (get) Token: 0x060020A2 RID: 8354 RVA: 0x00059E4C File Offset: 0x00058E4C
		internal static IRegisteredTargetSetting OutputSize
		{
			get
			{
				return APEnvironmentFacade.Instance.GetTargetSetting("memory-layout\\output-size");
			}
		}

		// Token: 0x170008A4 RID: 2212
		// (get) Token: 0x060020A3 RID: 8355 RVA: 0x00059E5D File Offset: 0x00058E5D
		internal static IRegisteredTargetSetting RetainSize
		{
			get
			{
				return APEnvironmentFacade.Instance.GetTargetSetting("memory-layout\\retain-size");
			}
		}

		// Token: 0x170008A5 RID: 2213
		// (get) Token: 0x060020A4 RID: 8356 RVA: 0x00059E6E File Offset: 0x00058E6E
		internal static IRegisteredTargetSetting InputSize
		{
			get
			{
				return APEnvironmentFacade.Instance.GetTargetSetting("memory-layout\\input-size");
			}
		}

		// Token: 0x170008A6 RID: 2214
		// (get) Token: 0x060020A5 RID: 8357 RVA: 0x00059E7F File Offset: 0x00058E7F
		internal static IRegisteredTargetSetting StaticAreaSize
		{
			get
			{
				return APEnvironmentFacade.Instance.GetTargetSetting("memory-layout\\static-area\\size");
			}
		}

		// Token: 0x170008A7 RID: 2215
		// (get) Token: 0x060020A6 RID: 8358 RVA: 0x00059E90 File Offset: 0x00058E90
		internal static IRegisteredTargetSetting RetainInOwnSegment
		{
			get
			{
				return APEnvironmentFacade.Instance.GetTargetSetting("memory-layout\\retain-in-own-segment");
			}
		}

		// Token: 0x170008A8 RID: 2216
		// (get) Token: 0x060020A7 RID: 8359 RVA: 0x00059EA1 File Offset: 0x00058EA1
		internal static IRegisteredTargetSetting PackMode
		{
			get
			{
				return APEnvironmentFacade.Instance.GetTargetSetting("memory-layout\\pack-mode");
			}
		}

		// Token: 0x170008A9 RID: 2217
		// (get) Token: 0x060020A8 RID: 8360 RVA: 0x00059EB2 File Offset: 0x00058EB2
		internal static IRegisteredTargetSetting VectorGranularity
		{
			get
			{
				return APEnvironmentFacade.Instance.GetTargetSetting("memory-layout\\vector-granularity");
			}
		}

		// Token: 0x170008AA RID: 2218
		// (get) Token: 0x060020A9 RID: 8361 RVA: 0x00059EC3 File Offset: 0x00058EC3
		internal static IRegisteredTargetSetting StackAlignment
		{
			get
			{
				return APEnvironmentFacade.Instance.GetTargetSetting("memory-layout\\stack-alignment");
			}
		}

		// Token: 0x170008AB RID: 2219
		// (get) Token: 0x060020AA RID: 8362 RVA: 0x00059ED4 File Offset: 0x00058ED4
		internal static IRegisteredTargetSetting CodeSegmentSize
		{
			get
			{
				return APEnvironmentFacade.Instance.GetTargetSetting("memory-layout\\code-segment-size");
			}
		}

		// Token: 0x170008AC RID: 2220
		// (get) Token: 0x060020AB RID: 8363 RVA: 0x00059EE5 File Offset: 0x00058EE5
		internal static IRegisteredTargetSetting DataSegmentSize
		{
			get
			{
				return APEnvironmentFacade.Instance.GetTargetSetting("memory-layout\\data-segment-size");
			}
		}

		// Token: 0x170008AD RID: 2221
		// (get) Token: 0x060020AC RID: 8364 RVA: 0x00059EF6 File Offset: 0x00058EF6
		internal static IRegisteredTargetSetting AdditionalAreas
		{
			get
			{
				return APEnvironmentFacade.Instance.GetTargetSetting("memory-layout\\additional-areas");
			}
		}

		// Token: 0x170008AE RID: 2222
		// (get) Token: 0x060020AD RID: 8365 RVA: 0x00059F07 File Offset: 0x00058F07
		internal static IRegisteredTargetSetting DynamicRetain
		{
			get
			{
				return APEnvironmentFacade.Instance.GetTargetSetting("memory-layout\\dynamic-retain");
			}
		}

		// Token: 0x170008AF RID: 2223
		// (get) Token: 0x060020AE RID: 8366 RVA: 0x00059F18 File Offset: 0x00058F18
		internal static IRegisteredTargetSetting DynamicPersistent
		{
			get
			{
				return APEnvironmentFacade.Instance.GetTargetSetting("memory-layout\\dynamic-persistent");
			}
		}

		// Token: 0x170008B0 RID: 2224
		// (get) Token: 0x060020AF RID: 8367 RVA: 0x00059F29 File Offset: 0x00058F29
		internal static IRegisteredTargetSetting CodeSegmentPrologSize
		{
			get
			{
				return APEnvironmentFacade.Instance.GetTargetSetting("memory-layout\\code-segment-prolog-size");
			}
		}

		// Token: 0x170008B1 RID: 2225
		// (get) Token: 0x060020B0 RID: 8368 RVA: 0x00059F3A File Offset: 0x00058F3A
		internal static IRegisteredTargetSetting CodeSegmentHeaderSize
		{
			get
			{
				return APEnvironmentFacade.Instance.GetTargetSetting("memory-layout\\code-segment-header-size");
			}
		}

		// Token: 0x170008B2 RID: 2226
		// (get) Token: 0x060020B1 RID: 8369 RVA: 0x00059F4B File Offset: 0x00058F4B
		internal static IRegisteredTargetSetting OnlineChangeInOwnSegment
		{
			get
			{
				return APEnvironmentFacade.Instance.GetTargetSetting("memory-layout\\online-change-in-own-segment");
			}
		}

		// Token: 0x170008B3 RID: 2227
		// (get) Token: 0x060020B2 RID: 8370 RVA: 0x00059F5C File Offset: 0x00058F5C
		internal static IRegisteredTargetSetting OnlineChangeSupported
		{
			get
			{
				return APEnvironmentFacade.Instance.GetTargetSetting("runtime_features\\online_change_supported");
			}
		}

		// Token: 0x170008B4 RID: 2228
		// (get) Token: 0x060020B3 RID: 8371 RVA: 0x00059F6D File Offset: 0x00058F6D
		internal static IRegisteredTargetSetting ByteAddressing
		{
			get
			{
				return APEnvironmentFacade.Instance.GetTargetSetting("memory-layout\\byte-addressing");
			}
		}

		// Token: 0x170008B5 RID: 2229
		// (get) Token: 0x060020B4 RID: 8372 RVA: 0x00059F7E File Offset: 0x00058F7E
		internal static IRegisteredTargetSetting BitByteAddressing
		{
			get
			{
				return APEnvironmentFacade.Instance.GetTargetSetting("memory-layout\\bit-byte-addressing");
			}
		}

		// Token: 0x170008B6 RID: 2230
		// (get) Token: 0x060020B5 RID: 8373 RVA: 0x00059F8F File Offset: 0x00058F8F
		internal static IRegisteredTargetSetting BitWordAddressing
		{
			get
			{
				return APEnvironmentFacade.Instance.GetTargetSetting("memory-layout\\bit-word-addressing");
			}
		}

		// Token: 0x170008B7 RID: 2231
		// (get) Token: 0x060020B6 RID: 8374 RVA: 0x00059FA0 File Offset: 0x00058FA0
		internal static IRegisteredTargetSetting MaximumDataAndCodeSize
		{
			get
			{
				return APEnvironmentFacade.Instance.GetTargetSetting("memory-layout\\maximum-data-and-code-size");
			}
		}

		// Token: 0x170008B8 RID: 2232
		// (get) Token: 0x060020B7 RID: 8375 RVA: 0x00059FB1 File Offset: 0x00058FB1
		internal static IRegisteredTargetSetting MinimalStructureGranularity
		{
			get
			{
				return APEnvironmentFacade.Instance.GetTargetSetting("memory-layout\\minimal-structure-granularity");
			}
		}

		// Token: 0x170008B9 RID: 2233
		// (get) Token: 0x060020B8 RID: 8376 RVA: 0x00059FC2 File Offset: 0x00058FC2
		internal static IRegisteredTargetSetting ConstantsInOwnSegment
		{
			get
			{
				return APEnvironmentFacade.Instance.GetTargetSetting("memory-layout\\constants-in-own-segment");
			}
		}

		// Token: 0x170008BA RID: 2234
		// (get) Token: 0x060020B9 RID: 8377 RVA: 0x00059FD3 File Offset: 0x00058FD3
		internal static IRegisteredTargetSetting StaticAreaStartAddress
		{
			get
			{
				return APEnvironmentFacade.Instance.GetTargetSetting("memory-layout\\static-area\\start-address");
			}
		}

		// Token: 0x170008BB RID: 2235
		// (get) Token: 0x060020BA RID: 8378 RVA: 0x00059FE4 File Offset: 0x00058FE4
		internal static IRegisteredTargetSetting StaticAreaAllowUserDefinedSize
		{
			get
			{
				return APEnvironmentFacade.Instance.GetTargetSetting("memory-layout\\static-area\\allow-user-defined-size");
			}
		}

		// Token: 0x170008BC RID: 2236
		// (get) Token: 0x060020BB RID: 8379 RVA: 0x00059FF5 File Offset: 0x00058FF5
		internal static IRegisteredTargetSetting AreasNumber
		{
			get
			{
				return APEnvironmentFacade.Instance.GetTargetSetting("memory-layout\\areas\\number");
			}
		}

		// Token: 0x170008BD RID: 2237
		// (get) Token: 0x060020BC RID: 8380 RVA: 0x0005A006 File Offset: 0x00059006
		internal static IRegisteredTargetSetting AreaNFlag
		{
			get
			{
				return APEnvironmentFacade.Instance.GetTargetSetting("memory-layout\\areas\\area_{0}\\flags");
			}
		}

		// Token: 0x170008BE RID: 2238
		// (get) Token: 0x060020BD RID: 8381 RVA: 0x0005A017 File Offset: 0x00059017
		internal static IRegisteredTargetSetting AreaNAreaFlags
		{
			get
			{
				return APEnvironmentFacade.Instance.GetTargetSetting("memory-layout\\areas\\area_{0}\\area_flags");
			}
		}

		// Token: 0x170008BF RID: 2239
		// (get) Token: 0x060020BE RID: 8382 RVA: 0x0005A028 File Offset: 0x00059028
		internal static IRegisteredTargetSetting AreaNMinimalAreaSize
		{
			get
			{
				return APEnvironmentFacade.Instance.GetTargetSetting("memory-layout\\areas\\area_{0}\\minimal-area-size");
			}
		}

		// Token: 0x170008C0 RID: 2240
		// (get) Token: 0x060020BF RID: 8383 RVA: 0x0005A039 File Offset: 0x00059039
		internal static IRegisteredTargetSetting AreaNMaximalAreaSize
		{
			get
			{
				return APEnvironmentFacade.Instance.GetTargetSetting("memory-layout\\areas\\area_{0}\\maximal-area-size");
			}
		}

		// Token: 0x170008C1 RID: 2241
		// (get) Token: 0x060020C0 RID: 8384 RVA: 0x0005A04A File Offset: 0x0005904A
		internal static IRegisteredTargetSetting AreaNAllocationPlusInPercent
		{
			get
			{
				return APEnvironmentFacade.Instance.GetTargetSetting("memory-layout\\areas\\area_{0}\\allocation-plus-in-percent");
			}
		}

		// Token: 0x170008C2 RID: 2242
		// (get) Token: 0x060020C1 RID: 8385 RVA: 0x0005A05B File Offset: 0x0005905B
		internal static IRegisteredTargetSetting AreaNAvailableAreaSize
		{
			get
			{
				return APEnvironmentFacade.Instance.GetTargetSetting("memory-layout\\areas\\area_{0}\\available-area-size");
			}
		}

		// Token: 0x170008C3 RID: 2243
		// (get) Token: 0x060020C2 RID: 8386 RVA: 0x0005A06C File Offset: 0x0005906C
		internal static IRegisteredTargetSetting AreaNStartAddress
		{
			get
			{
				return APEnvironmentFacade.Instance.GetTargetSetting("memory-layout\\areas\\area_{0}\\start-address");
			}
		}

		// Token: 0x170008C4 RID: 2244
		// (get) Token: 0x060020C3 RID: 8387 RVA: 0x0005A07D File Offset: 0x0005907D
		internal static IRegisteredTargetSetting MaxStackSize
		{
			get
			{
				return APEnvironmentFacade.Instance.GetTargetSetting("memory-layout\\max-stack-size");
			}
		}

		// Token: 0x170008C5 RID: 2245
		// (get) Token: 0x060020C4 RID: 8388 RVA: 0x0005A08E File Offset: 0x0005908E
		internal static IRegisteredTargetSetting MaxStackSizeExternalCall
		{
			get
			{
				return APEnvironmentFacade.Instance.GetTargetSetting("memory-layout\\max-stack-size-external-call");
			}
		}

		// Token: 0x170008C6 RID: 2246
		// (get) Token: 0x060020C5 RID: 8389 RVA: 0x0005A09F File Offset: 0x0005909F
		internal static IRegisteredTargetSetting ReportStackCheckRecursionWarningAsError
		{
			get
			{
				return APEnvironmentFacade.Instance.GetTargetSetting("memory-layout\\report-stack-check-recursion-warning-as-error");
			}
		}

		// Token: 0x170008C7 RID: 2247
		// (get) Token: 0x060020C6 RID: 8390 RVA: 0x0005A0B0 File Offset: 0x000590B0
		internal static IRegisteredTargetSetting SupportUserCheckFunctions
		{
			get
			{
				return APEnvironmentFacade.Instance.GetTargetSetting("runtime_features\\support_user_check_functions");
			}
		}

		// Token: 0x170008C8 RID: 2248
		// (get) Token: 0x060020C7 RID: 8391 RVA: 0x0005A0C1 File Offset: 0x000590C1
		internal static IRegisteredTargetSetting MemoryBarrier
		{
			get
			{
				return APEnvironmentFacade.Instance.GetTargetSetting("codegenerator\\memory-barrier");
			}
		}

		// Token: 0x170008C9 RID: 2249
		// (get) Token: 0x060020C8 RID: 8392 RVA: 0x0005A0D2 File Offset: 0x000590D2
		internal static IRegisteredTargetSetting LateRelocationForFixedAreas
		{
			get
			{
				return APEnvironmentFacade.Instance.GetTargetSetting("memory-layout\\late-relocation-for-fixed-areas");
			}
		}

		// Token: 0x170008CA RID: 2250
		// (get) Token: 0x060020C9 RID: 8393 RVA: 0x0005A0E3 File Offset: 0x000590E3
		public static IRegisteredTargetSetting SingleOnlineChangeArea
		{
			get
			{
				return APEnvironmentFacade.Instance.GetTargetSetting("memory-layout\\single-online-change-area");
			}
		}

		// Token: 0x170008CB RID: 2251
		// (get) Token: 0x060020CA RID: 8394 RVA: 0x0005A0F4 File Offset: 0x000590F4
		public static IRegisteredTargetSetting BackendGuid
		{
			get
			{
				return APEnvironmentFacade.Instance.GetTargetSetting("codegenerator\\backend-guid");
			}
		}
	}
}
