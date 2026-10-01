using System;
using _3S.CoDeSys.Core.TargetSettings;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.CommonCompilerData
{
	// Token: 0x020001D0 RID: 464
	internal class LocalTargetSettingsObject : ILanguageModelManagerTargetSettings4, ILanguageModelManagerTargetSettings3, ILanguageModelManagerTargetSettings2, ILanguageModelManagerTargetSettings
	{
		// Token: 0x170008CC RID: 2252
		// (get) Token: 0x060020CC RID: 8396 RVA: 0x0005A105 File Offset: 0x00059105
		public IRegisteredTargetSetting AdditionalAreas
		{
			get
			{
				return LocalTargetSettings.AdditionalAreas;
			}
		}

		// Token: 0x170008CD RID: 2253
		// (get) Token: 0x060020CD RID: 8397 RVA: 0x0005A10C File Offset: 0x0005910C
		public IRegisteredTargetSetting AddressCalculatorGuid
		{
			get
			{
				return LocalTargetSettings.AddressCalculatorGuid;
			}
		}

		// Token: 0x170008CE RID: 2254
		// (get) Token: 0x060020CE RID: 8398 RVA: 0x0005A113 File Offset: 0x00059113
		public IRegisteredTargetSetting AreaNAllocationPlusInPercent
		{
			get
			{
				return LocalTargetSettings.AreaNAllocationPlusInPercent;
			}
		}

		// Token: 0x170008CF RID: 2255
		// (get) Token: 0x060020CF RID: 8399 RVA: 0x0005A11A File Offset: 0x0005911A
		public IRegisteredTargetSetting AreaNAreaFlags
		{
			get
			{
				return LocalTargetSettings.AreaNAreaFlags;
			}
		}

		// Token: 0x170008D0 RID: 2256
		// (get) Token: 0x060020D0 RID: 8400 RVA: 0x0005A121 File Offset: 0x00059121
		public IRegisteredTargetSetting AreaNAvailableAreaSize
		{
			get
			{
				return LocalTargetSettings.AreaNAvailableAreaSize;
			}
		}

		// Token: 0x170008D1 RID: 2257
		// (get) Token: 0x060020D1 RID: 8401 RVA: 0x0005A128 File Offset: 0x00059128
		public IRegisteredTargetSetting AreaNFlag
		{
			get
			{
				return LocalTargetSettings.AreaNFlag;
			}
		}

		// Token: 0x170008D2 RID: 2258
		// (get) Token: 0x060020D2 RID: 8402 RVA: 0x0005A12F File Offset: 0x0005912F
		public IRegisteredTargetSetting AreaNMaximalAreaSize
		{
			get
			{
				return LocalTargetSettings.AreaNMaximalAreaSize;
			}
		}

		// Token: 0x170008D3 RID: 2259
		// (get) Token: 0x060020D3 RID: 8403 RVA: 0x0005A136 File Offset: 0x00059136
		public IRegisteredTargetSetting AreaNMinimalAreaSize
		{
			get
			{
				return LocalTargetSettings.AreaNMinimalAreaSize;
			}
		}

		// Token: 0x170008D4 RID: 2260
		// (get) Token: 0x060020D4 RID: 8404 RVA: 0x0005A13D File Offset: 0x0005913D
		public IRegisteredTargetSetting AreaNStartAddress
		{
			get
			{
				return LocalTargetSettings.AreaNStartAddress;
			}
		}

		// Token: 0x170008D5 RID: 2261
		// (get) Token: 0x060020D5 RID: 8405 RVA: 0x0005A144 File Offset: 0x00059144
		public IRegisteredTargetSetting AreasNumber
		{
			get
			{
				return LocalTargetSettings.AreasNumber;
			}
		}

		// Token: 0x170008D6 RID: 2262
		// (get) Token: 0x060020D6 RID: 8406 RVA: 0x0005A14B File Offset: 0x0005914B
		public IRegisteredTargetSetting BitByteAddressing
		{
			get
			{
				return LocalTargetSettings.BitByteAddressing;
			}
		}

		// Token: 0x170008D7 RID: 2263
		// (get) Token: 0x060020D7 RID: 8407 RVA: 0x0005A152 File Offset: 0x00059152
		public IRegisteredTargetSetting BitWordAddressing
		{
			get
			{
				return LocalTargetSettings.BitWordAddressing;
			}
		}

		// Token: 0x170008D8 RID: 2264
		// (get) Token: 0x060020D8 RID: 8408 RVA: 0x0005A159 File Offset: 0x00059159
		public IRegisteredTargetSetting BreakpointsSupported
		{
			get
			{
				return LocalTargetSettings.BreakpointsSupported;
			}
		}

		// Token: 0x170008D9 RID: 2265
		// (get) Token: 0x060020D9 RID: 8409 RVA: 0x0005A160 File Offset: 0x00059160
		public IRegisteredTargetSetting ByteAddressing
		{
			get
			{
				return LocalTargetSettings.ByteAddressing;
			}
		}

		// Token: 0x170008DA RID: 2266
		// (get) Token: 0x060020DA RID: 8410 RVA: 0x0005A167 File Offset: 0x00059167
		public IRegisteredTargetSetting ByteSupport
		{
			get
			{
				return LocalTargetSettings.ByteSupport;
			}
		}

		// Token: 0x170008DB RID: 2267
		// (get) Token: 0x060020DB RID: 8411 RVA: 0x0005A16E File Offset: 0x0005916E
		public IRegisteredTargetSetting CheckMisalignedAddress
		{
			get
			{
				return LocalTargetSettings.CheckMisalignedAddress;
			}
		}

		// Token: 0x170008DC RID: 2268
		// (get) Token: 0x060020DC RID: 8412 RVA: 0x0005A175 File Offset: 0x00059175
		public IRegisteredTargetSetting CheckMultipleTaskOutputWrite
		{
			get
			{
				return LocalTargetSettings.CheckMultipleTaskOutputWrite;
			}
		}

		// Token: 0x170008DD RID: 2269
		// (get) Token: 0x060020DD RID: 8413 RVA: 0x0005A17C File Offset: 0x0005917C
		public IRegisteredTargetSetting CodegeneratorGuid
		{
			get
			{
				return LocalTargetSettings.CodegeneratorGuid;
			}
		}

		// Token: 0x170008DE RID: 2270
		// (get) Token: 0x060020DE RID: 8414 RVA: 0x0005A183 File Offset: 0x00059183
		public IRegisteredTargetSetting CodegenMultithreading
		{
			get
			{
				return LocalTargetSettings.CodegenMultithreading;
			}
		}

		// Token: 0x170008DF RID: 2271
		// (get) Token: 0x060020DF RID: 8415 RVA: 0x0005A18A File Offset: 0x0005918A
		public IRegisteredTargetSetting CodeSegmentHeaderSize
		{
			get
			{
				return LocalTargetSettings.CodeSegmentHeaderSize;
			}
		}

		// Token: 0x170008E0 RID: 2272
		// (get) Token: 0x060020E0 RID: 8416 RVA: 0x0005A191 File Offset: 0x00059191
		public IRegisteredTargetSetting CodeSegmentPrologSize
		{
			get
			{
				return LocalTargetSettings.CodeSegmentPrologSize;
			}
		}

		// Token: 0x170008E1 RID: 2273
		// (get) Token: 0x060020E1 RID: 8417 RVA: 0x0005A198 File Offset: 0x00059198
		public IRegisteredTargetSetting CodeSegmentSize
		{
			get
			{
				return LocalTargetSettings.CodeSegmentSize;
			}
		}

		// Token: 0x170008E2 RID: 2274
		// (get) Token: 0x060020E2 RID: 8418 RVA: 0x0005A19F File Offset: 0x0005919F
		public IRegisteredTargetSetting CompactDownload
		{
			get
			{
				return LocalTargetSettings.CompactDownload;
			}
		}

		// Token: 0x170008E3 RID: 2275
		// (get) Token: 0x060020E3 RID: 8419 RVA: 0x0005A1A6 File Offset: 0x000591A6
		public IRegisteredTargetSetting CompilerDefines
		{
			get
			{
				return LocalTargetSettings.CompilerDefines;
			}
		}

		// Token: 0x170008E4 RID: 2276
		// (get) Token: 0x060020E4 RID: 8420 RVA: 0x0005A1AD File Offset: 0x000591AD
		public IRegisteredTargetSetting ConstantsInOwnSegment
		{
			get
			{
				return LocalTargetSettings.ConstantsInOwnSegment;
			}
		}

		// Token: 0x170008E5 RID: 2277
		// (get) Token: 0x060020E5 RID: 8421 RVA: 0x0005A1B4 File Offset: 0x000591B4
		public IRegisteredTargetSetting CycleControlInIec
		{
			get
			{
				return LocalTargetSettings.CycleControlInIec;
			}
		}

		// Token: 0x170008E6 RID: 2278
		// (get) Token: 0x060020E6 RID: 8422 RVA: 0x0005A1BB File Offset: 0x000591BB
		public IRegisteredTargetSetting CycleControlVersion2
		{
			get
			{
				return LocalTargetSettings.CycleControlVersion2;
			}
		}

		// Token: 0x170008E7 RID: 2279
		// (get) Token: 0x060020E7 RID: 8423 RVA: 0x0005A1C2 File Offset: 0x000591C2
		public IRegisteredTargetSetting DataSegmentSize
		{
			get
			{
				return LocalTargetSettings.DataSegmentSize;
			}
		}

		// Token: 0x170008E8 RID: 2280
		// (get) Token: 0x060020E8 RID: 8424 RVA: 0x0005A1C9 File Offset: 0x000591C9
		public IRegisteredTargetSetting DoPersistentCode
		{
			get
			{
				return LocalTargetSettings.DoPersistentCode;
			}
		}

		// Token: 0x170008E9 RID: 2281
		// (get) Token: 0x060020E9 RID: 8425 RVA: 0x0005A1D0 File Offset: 0x000591D0
		public IRegisteredTargetSetting DynamicPersistent
		{
			get
			{
				return LocalTargetSettings.DynamicPersistent;
			}
		}

		// Token: 0x170008EA RID: 2282
		// (get) Token: 0x060020EA RID: 8426 RVA: 0x0005A1D7 File Offset: 0x000591D7
		public IRegisteredTargetSetting DynamicRetain
		{
			get
			{
				return LocalTargetSettings.DynamicRetain;
			}
		}

		// Token: 0x170008EB RID: 2283
		// (get) Token: 0x060020EB RID: 8427 RVA: 0x0005A1DE File Offset: 0x000591DE
		public IRegisteredTargetSetting EnableBreakpointLogging
		{
			get
			{
				return LocalTargetSettings.EnableBreakpointLogging;
			}
		}

		// Token: 0x170008EC RID: 2284
		// (get) Token: 0x060020EC RID: 8428 RVA: 0x0005A1E5 File Offset: 0x000591E5
		public IRegisteredTargetSetting ExternalRealStringConversions
		{
			get
			{
				return LocalTargetSettings.ExternalRealStringConversions;
			}
		}

		// Token: 0x170008ED RID: 2285
		// (get) Token: 0x060020ED RID: 8429 RVA: 0x0005A1EC File Offset: 0x000591EC
		public IRegisteredTargetSetting GenerateDirectCalls
		{
			get
			{
				return LocalTargetSettings.GenerateDirectCalls;
			}
		}

		// Token: 0x170008EE RID: 2286
		// (get) Token: 0x060020EE RID: 8430 RVA: 0x0005A1F3 File Offset: 0x000591F3
		public IRegisteredTargetSetting GvlInitFunctions
		{
			get
			{
				return LocalTargetSettings.GvlInitFunctions;
			}
		}

		// Token: 0x170008EF RID: 2287
		// (get) Token: 0x060020EF RID: 8431 RVA: 0x0005A1FA File Offset: 0x000591FA
		public IRegisteredTargetSetting InputSize
		{
			get
			{
				return LocalTargetSettings.InputSize;
			}
		}

		// Token: 0x170008F0 RID: 2288
		// (get) Token: 0x060020F0 RID: 8432 RVA: 0x0005A201 File Offset: 0x00059201
		public IRegisteredTargetSetting Int64AsInt32
		{
			get
			{
				return LocalTargetSettings.Int64AsInt32;
			}
		}

		// Token: 0x170008F1 RID: 2289
		// (get) Token: 0x060020F1 RID: 8433 RVA: 0x0005A208 File Offset: 0x00059208
		public IRegisteredTargetSetting LinkAllGlobalVariables
		{
			get
			{
				return LocalTargetSettings.LinkAllGlobalVariables;
			}
		}

		// Token: 0x170008F2 RID: 2290
		// (get) Token: 0x060020F2 RID: 8434 RVA: 0x0005A20F File Offset: 0x0005920F
		public IRegisteredTargetSetting LintDataTypes
		{
			get
			{
				return LocalTargetSettings.LintDataTypes;
			}
		}

		// Token: 0x170008F3 RID: 2291
		// (get) Token: 0x060020F3 RID: 8435 RVA: 0x0005A216 File Offset: 0x00059216
		public IRegisteredTargetSetting LRealAsReal
		{
			get
			{
				return LocalTargetSettings.LRealAsReal;
			}
		}

		// Token: 0x170008F4 RID: 2292
		// (get) Token: 0x060020F4 RID: 8436 RVA: 0x0005A21D File Offset: 0x0005921D
		public IRegisteredTargetSetting LRealDataType
		{
			get
			{
				return LocalTargetSettings.LRealDataType;
			}
		}

		// Token: 0x170008F5 RID: 2293
		// (get) Token: 0x060020F5 RID: 8437 RVA: 0x0005A224 File Offset: 0x00059224
		public IRegisteredTargetSetting MaximumDataAndCodeSize
		{
			get
			{
				return LocalTargetSettings.MaximumDataAndCodeSize;
			}
		}

		// Token: 0x170008F6 RID: 2294
		// (get) Token: 0x060020F6 RID: 8438 RVA: 0x0005A22B File Offset: 0x0005922B
		public IRegisteredTargetSetting MaximumNumApplications
		{
			get
			{
				return LocalTargetSettings.MaximumNumApplications;
			}
		}

		// Token: 0x170008F7 RID: 2295
		// (get) Token: 0x060020F7 RID: 8439 RVA: 0x0005A232 File Offset: 0x00059232
		public IRegisteredTargetSetting MaxStackSize
		{
			get
			{
				return LocalTargetSettings.MaxStackSize;
			}
		}

		// Token: 0x170008F8 RID: 2296
		// (get) Token: 0x060020F8 RID: 8440 RVA: 0x0005A239 File Offset: 0x00059239
		public IRegisteredTargetSetting MaxStackSizeExternalCall
		{
			get
			{
				return LocalTargetSettings.MaxStackSizeExternalCall;
			}
		}

		// Token: 0x170008F9 RID: 2297
		// (get) Token: 0x060020F9 RID: 8441 RVA: 0x0005A240 File Offset: 0x00059240
		public IRegisteredTargetSetting ReportStackCheckRecursionWarningAsError
		{
			get
			{
				return LocalTargetSettings.ReportStackCheckRecursionWarningAsError;
			}
		}

		// Token: 0x170008FA RID: 2298
		// (get) Token: 0x060020FA RID: 8442 RVA: 0x0005A247 File Offset: 0x00059247
		public IRegisteredTargetSetting MemoryAllocationCallback
		{
			get
			{
				return LocalTargetSettings.MemoryAllocationCallback;
			}
		}

		// Token: 0x170008FB RID: 2299
		// (get) Token: 0x060020FB RID: 8443 RVA: 0x0005A24E File Offset: 0x0005924E
		public IRegisteredTargetSetting MemoryBarrier
		{
			get
			{
				return LocalTargetSettings.MemoryBarrier;
			}
		}

		// Token: 0x170008FC RID: 2300
		// (get) Token: 0x060020FC RID: 8444 RVA: 0x0005A255 File Offset: 0x00059255
		public IRegisteredTargetSetting MemorySize
		{
			get
			{
				return LocalTargetSettings.MemorySize;
			}
		}

		// Token: 0x170008FD RID: 2301
		// (get) Token: 0x060020FD RID: 8445 RVA: 0x0005A25C File Offset: 0x0005925C
		public IRegisteredTargetSetting MinimalStructureGranularity
		{
			get
			{
				return LocalTargetSettings.MinimalStructureGranularity;
			}
		}

		// Token: 0x170008FE RID: 2302
		// (get) Token: 0x060020FE RID: 8446 RVA: 0x0005A263 File Offset: 0x00059263
		public IRegisteredTargetSetting MinimalSystem
		{
			get
			{
				return LocalTargetSettings.MinimalSystem;
			}
		}

		// Token: 0x170008FF RID: 2303
		// (get) Token: 0x060020FF RID: 8447 RVA: 0x0005A26A File Offset: 0x0005926A
		public IRegisteredTargetSetting NewVfTable
		{
			get
			{
				return LocalTargetSettings.NewVfTable;
			}
		}

		// Token: 0x17000900 RID: 2304
		// (get) Token: 0x06002100 RID: 8448 RVA: 0x0005A271 File Offset: 0x00059271
		public IRegisteredTargetSetting NoBytesInRetain
		{
			get
			{
				return LocalTargetSettings.NoBytesInRetain;
			}
		}

		// Token: 0x17000901 RID: 2305
		// (get) Token: 0x06002101 RID: 8449 RVA: 0x0005A278 File Offset: 0x00059278
		public IRegisteredTargetSetting NoDefaultInitialisation
		{
			get
			{
				return LocalTargetSettings.NoDefaultInitialisation;
			}
		}

		// Token: 0x17000902 RID: 2306
		// (get) Token: 0x06002102 RID: 8450 RVA: 0x0005A27F File Offset: 0x0005927F
		public IRegisteredTargetSetting NoPrecompileMessages
		{
			get
			{
				return LocalTargetSettings.NoPrecompileMessages;
			}
		}

		// Token: 0x17000903 RID: 2307
		// (get) Token: 0x06002103 RID: 8451 RVA: 0x0005A286 File Offset: 0x00059286
		public IRegisteredTargetSetting OnlineChangeInOwnSegment
		{
			get
			{
				return LocalTargetSettings.OnlineChangeInOwnSegment;
			}
		}

		// Token: 0x17000904 RID: 2308
		// (get) Token: 0x06002104 RID: 8452 RVA: 0x0005A28D File Offset: 0x0005928D
		public IRegisteredTargetSetting OnlineChangeSupported
		{
			get
			{
				return LocalTargetSettings.OnlineChangeSupported;
			}
		}

		// Token: 0x17000905 RID: 2309
		// (get) Token: 0x06002105 RID: 8453 RVA: 0x0005A294 File Offset: 0x00059294
		public IRegisteredTargetSetting OptimizedOnlineChange
		{
			get
			{
				return LocalTargetSettings.OptimizedOnlineChange;
			}
		}

		// Token: 0x17000906 RID: 2310
		// (get) Token: 0x06002106 RID: 8454 RVA: 0x0005A29B File Offset: 0x0005929B
		public IRegisteredTargetSetting OutputSize
		{
			get
			{
				return LocalTargetSettings.OutputSize;
			}
		}

		// Token: 0x17000907 RID: 2311
		// (get) Token: 0x06002107 RID: 8455 RVA: 0x0005A2A2 File Offset: 0x000592A2
		public IRegisteredTargetSetting PackMode
		{
			get
			{
				return LocalTargetSettings.PackMode;
			}
		}

		// Token: 0x17000908 RID: 2312
		// (get) Token: 0x06002108 RID: 8456 RVA: 0x0005A2A9 File Offset: 0x000592A9
		public IRegisteredTargetSetting RetainCycleTask
		{
			get
			{
				return LocalTargetSettings.RetainCycleTask;
			}
		}

		// Token: 0x17000909 RID: 2313
		// (get) Token: 0x06002109 RID: 8457 RVA: 0x0005A2B0 File Offset: 0x000592B0
		public IRegisteredTargetSetting RetainInCycle
		{
			get
			{
				return LocalTargetSettings.RetainInCycle;
			}
		}

		// Token: 0x1700090A RID: 2314
		// (get) Token: 0x0600210A RID: 8458 RVA: 0x0005A2B7 File Offset: 0x000592B7
		public IRegisteredTargetSetting RetainInOwnSegment
		{
			get
			{
				return LocalTargetSettings.RetainInOwnSegment;
			}
		}

		// Token: 0x1700090B RID: 2315
		// (get) Token: 0x0600210B RID: 8459 RVA: 0x0005A2BE File Offset: 0x000592BE
		public IRegisteredTargetSetting RetainSize
		{
			get
			{
				return LocalTargetSettings.RetainSize;
			}
		}

		// Token: 0x1700090C RID: 2316
		// (get) Token: 0x0600210C RID: 8460 RVA: 0x0005A2C5 File Offset: 0x000592C5
		public IRegisteredTargetSetting RuntimeVersion
		{
			get
			{
				return LocalTargetSettings.RuntimeVersion;
			}
		}

		// Token: 0x1700090D RID: 2317
		// (get) Token: 0x0600210D RID: 8461 RVA: 0x0005A2CC File Offset: 0x000592CC
		public IRegisteredTargetSetting SimpleCycle
		{
			get
			{
				return LocalTargetSettings.SimpleCycle;
			}
		}

		// Token: 0x1700090E RID: 2318
		// (get) Token: 0x0600210E RID: 8462 RVA: 0x0005A2D3 File Offset: 0x000592D3
		public IRegisteredTargetSetting StackAlignment
		{
			get
			{
				return LocalTargetSettings.StackAlignment;
			}
		}

		// Token: 0x1700090F RID: 2319
		// (get) Token: 0x0600210F RID: 8463 RVA: 0x0005A2DA File Offset: 0x000592DA
		public IRegisteredTargetSetting StaticAreaAllowUserDefinedSize
		{
			get
			{
				return LocalTargetSettings.StaticAreaAllowUserDefinedSize;
			}
		}

		// Token: 0x17000910 RID: 2320
		// (get) Token: 0x06002110 RID: 8464 RVA: 0x0005A2E1 File Offset: 0x000592E1
		public IRegisteredTargetSetting StaticAreaSize
		{
			get
			{
				return LocalTargetSettings.StaticAreaSize;
			}
		}

		// Token: 0x17000911 RID: 2321
		// (get) Token: 0x06002111 RID: 8465 RVA: 0x0005A2E8 File Offset: 0x000592E8
		public IRegisteredTargetSetting StaticAreaStartAddress
		{
			get
			{
				return LocalTargetSettings.StaticAreaStartAddress;
			}
		}

		// Token: 0x17000912 RID: 2322
		// (get) Token: 0x06002112 RID: 8466 RVA: 0x0005A2EF File Offset: 0x000592EF
		public IRegisteredTargetSetting SupportedSystemOperators
		{
			get
			{
				return LocalTargetSettings.SupportedSystemOperators;
			}
		}

		// Token: 0x17000913 RID: 2323
		// (get) Token: 0x06002113 RID: 8467 RVA: 0x0005A2F6 File Offset: 0x000592F6
		public IRegisteredTargetSetting SupportMulticore
		{
			get
			{
				return LocalTargetSettings.SupportMulticore;
			}
		}

		// Token: 0x17000914 RID: 2324
		// (get) Token: 0x06002114 RID: 8468 RVA: 0x0005A2FD File Offset: 0x000592FD
		public IRegisteredTargetSetting SupportSystemApplications
		{
			get
			{
				return LocalTargetSettings.SupportSystemApplications;
			}
		}

		// Token: 0x17000915 RID: 2325
		// (get) Token: 0x06002115 RID: 8469 RVA: 0x0005A304 File Offset: 0x00059304
		public IRegisteredTargetSetting SupportUserCheckFunctions
		{
			get
			{
				return LocalTargetSettings.SupportUserCheckFunctions;
			}
		}

		// Token: 0x17000916 RID: 2326
		// (get) Token: 0x06002116 RID: 8470 RVA: 0x0005A30B File Offset: 0x0005930B
		public IRegisteredTargetSetting UnsupportedDataTypes
		{
			get
			{
				return LocalTargetSettings.UnsupportedDataTypes;
			}
		}

		// Token: 0x17000917 RID: 2327
		// (get) Token: 0x06002117 RID: 8471 RVA: 0x0005A312 File Offset: 0x00059312
		public IRegisteredTargetSetting UnsupportedOperators
		{
			get
			{
				return LocalTargetSettings.UnsupportedOperators;
			}
		}

		// Token: 0x17000918 RID: 2328
		// (get) Token: 0x06002118 RID: 8472 RVA: 0x0005A319 File Offset: 0x00059319
		public IRegisteredTargetSetting VarconfigGeneratorGuid
		{
			get
			{
				return LocalTargetSettings.VarconfigGeneratorGuid;
			}
		}

		// Token: 0x17000919 RID: 2329
		// (get) Token: 0x06002119 RID: 8473 RVA: 0x0005A320 File Offset: 0x00059320
		public IRegisteredTargetSetting VectorGranularity
		{
			get
			{
				return LocalTargetSettings.VectorGranularity;
			}
		}

		// Token: 0x1700091A RID: 2330
		// (get) Token: 0x0600211A RID: 8474 RVA: 0x0005A327 File Offset: 0x00059327
		public IRegisteredTargetSetting SingleOnlineChangeArea
		{
			get
			{
				return LocalTargetSettings.SingleOnlineChangeArea;
			}
		}

		// Token: 0x1700091B RID: 2331
		// (get) Token: 0x0600211B RID: 8475 RVA: 0x0005A32E File Offset: 0x0005932E
		public IRegisteredTargetSetting BackendGuid
		{
			get
			{
				return LocalTargetSettings.BackendGuid;
			}
		}
	}
}
