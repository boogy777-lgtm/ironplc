using System;
using _3S.CoDeSys.Core.Device;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.TargetSettings;
using _3S.CoDeSys.LanguageModelManager.CommonCompilerData;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x0200012D RID: 301
	internal class MemorySettingsHelperX : IMemorySettingsHelper
	{
		// Token: 0x06001A0A RID: 6666 RVA: 0x0004A920 File Offset: 0x00049920
		internal static IMemorySettingsProvider _GetMemorySettingsProvider(Guid guidApplication)
		{
			IMemorySettingsProvider result;
			try
			{
				Guid memorySettingsProvider = APEnvironmentFacade.Instance.LanguageModelMgr.ApplicationDeviceTable.GetMemorySettingsProvider(guidApplication);
				result = APEnvironmentFacade.Instance.GetMemorySettingsProvider(APEnvironmentFacade.Instance.PrimaryProjectHandle, memorySettingsProvider);
			}
			catch
			{
				result = null;
			}
			return result;
		}

		// Token: 0x06001A0B RID: 6667 RVA: 0x0004A974 File Offset: 0x00049974
		internal static MemorySettings _GetChildApplicationMemorySettings(Guid guidDevice, Guid guidApplication)
		{
			IMemorySettingsProvider memorySettingsProvider = MemorySettingsHelperX._GetMemorySettingsProvider(guidApplication);
			if (guidDevice == Guid.Empty)
			{
				guidDevice = guidApplication;
			}
			IDeviceIdentification targetIdOfDevice = APEnvironmentFacade.Instance.LanguageModelMgr.ApplicationDeviceTable.GetTargetIdOfDevice(guidDevice);
			ITargetSettings targetSettingsById = APEnvironmentFacade.Instance.GetTargetSettingsById(targetIdOfDevice);
			MemorySettings empty = MemorySettings.Empty;
			empty.MemoryDataSize = 0;
			empty.OutputDataSize = 0;
			empty.InputDataSize = 0;
			empty.RetainDataSize = 0;
			empty.RetainInOwnSegment = true;
			empty.PackMode = LocalTargetSettings.PackMode.GetIntValue(targetSettingsById);
			empty.StackAlignment = LocalTargetSettings.StackAlignment.GetIntValue(targetSettingsById);
			if (memorySettingsProvider != null)
			{
				memorySettingsProvider.UpdateMemorySettings(empty);
			}
			else
			{
				empty.AddArea(new Area(DataSegmentFlags.Data | DataSegmentFlags.Code, AreaFlags.DynamicSize)
				{
					MinimalAreaSize = 1048576,
					MaximalAreaSize = int.MaxValue,
					AllocationPlusInPercent = 100
				});
			}
			return empty;
		}

		// Token: 0x170006D0 RID: 1744
		// (get) Token: 0x06001A0C RID: 6668 RVA: 0x0004AA48 File Offset: 0x00049A48
		internal static MemorySettings _DefaultMemorySettings
		{
			get
			{
				MemorySettings empty = MemorySettings.Empty;
				empty.MemoryDataSize = 4096;
				empty.OutputDataSize = 4096;
				empty.InputDataSize = 4096;
				empty.RetainDataSize = 4096;
				empty.RetainInOwnSegment = true;
				empty.PackMode = 8;
				empty.StackAlignment = 4;
				empty.ByteAddressing = false;
				empty.BitByteAddressing = false;
				empty.BitWordAddressing = false;
				empty.OnlineChangeInOwnSegment = false;
				empty.MinGranularity = -1;
				empty.AddArea(new Area(DataSegmentFlags.All, AreaFlags.DynamicSize)
				{
					MinimalAreaSize = 1048576,
					MaximalAreaSize = int.MaxValue,
					AllocationPlusInPercent = 30
				});
				return empty;
			}
		}

		// Token: 0x06001A0D RID: 6669 RVA: 0x0004AAEF File Offset: 0x00049AEF
		internal static MemorySettings _GetMemorySettings(Guid guidApplication, bool bSimulation)
		{
			return MemorySettingsHelperX._GetMemorySettings(APEnvironmentFacade.Instance.LanguageModelMgr.ApplicationDeviceTable.GetDeviceOfApplication(guidApplication), APEnvironmentFacade.Instance.LanguageModelMgr.ApplicationDeviceTable.GetParentApplication(guidApplication), guidApplication, bSimulation);
		}

		// Token: 0x06001A0E RID: 6670 RVA: 0x0004AB24 File Offset: 0x00049B24
		internal static void _UpdateTargetMemorySettings(Guid guidApplication, _IMemorySettings memset)
		{
			PreCompileContext preCompileContext = APEnvironmentFacade.Instance.LanguageModelMgr.GetPrecompileContext(guidApplication) as PreCompileContext;
			if (preCompileContext != null)
			{
				if (preCompileContext.TargetInputSize > 0)
				{
					memset.InputDataSize = preCompileContext.TargetInputSize;
				}
				if (preCompileContext.TargetOutputSize > 0)
				{
					memset.OutputDataSize = preCompileContext.TargetOutputSize;
				}
				if (preCompileContext.TargetMemorySize > 0)
				{
					memset.MemoryDataSize = preCompileContext.TargetMemorySize;
				}
				if (preCompileContext.TargetStaticSize > 0)
				{
					memset.StaticAreaSize = preCompileContext.TargetStaticSize;
				}
				if (preCompileContext.IsDefined("bit_byte_addressing"))
				{
					memset.BitByteAddressing = true;
					memset.BitWordAddressing = false;
					return;
				}
				if (preCompileContext.IsDefined("bit_word_addressing"))
				{
					memset.BitByteAddressing = false;
					memset.BitWordAddressing = true;
				}
			}
		}

		// Token: 0x06001A0F RID: 6671 RVA: 0x0004ABD8 File Offset: 0x00049BD8
		private static int GetStaticAreaSize(MemorySettings memsetRet, ITargetSettings target)
		{
			int result = LocalTargetSettings.StaticAreaSize.GetIntValue(target);
			if (memsetRet.StaticAreaSize > 0 && LocalTargetSettings.StaticAreaAllowUserDefinedSize.GetBoolValue(target))
			{
				result = memsetRet.StaticAreaSize;
			}
			return result;
		}

		// Token: 0x06001A10 RID: 6672 RVA: 0x0004AC0F File Offset: 0x00049C0F
		private static bool HasStaticArea(MemorySettings memsetRet, ITargetSettings target)
		{
			return MemorySettingsHelperX.GetStaticAreaSize(memsetRet, target) > 0;
		}

		// Token: 0x06001A11 RID: 6673 RVA: 0x0004AC1C File Offset: 0x00049C1C
		private static void UpdateStaticArea(MemorySettings memsetRet, ITargetSettings target, ITargetSettings simutarget)
		{
			int staticAreaSize = MemorySettingsHelperX.GetStaticAreaSize(memsetRet, target);
			if (staticAreaSize <= 0)
			{
				return;
			}
			memsetRet.OneSRAM = true;
			int num = LocalTargetSettings.StaticAreaStartAddress.GetIntValue(target);
			if (simutarget != null && APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351500)
			{
				num = 0;
			}
			AreaFlags areaFlags = AreaFlags.DynamicSize;
			if (num != 0 && APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351300)
			{
				areaFlags |= AreaFlags.Fixed;
			}
			Area area = new Area(DataSegmentFlags.Retain | DataSegmentFlags.Persistent, areaFlags)
			{
				MinimalAreaSize = staticAreaSize,
				MaximalAreaSize = staticAreaSize,
				Size = staticAreaSize,
				AllocationPlusInPercent = 0,
				StartAddress = num
			};
			memsetRet.AddArea(area);
		}

		// Token: 0x06001A12 RID: 6674 RVA: 0x0004ACB0 File Offset: 0x00049CB0
		internal static MemorySettings _GetMemorySettings(Guid guidDevice, Guid guidParentApplication, Guid guidApplication, bool bSimulation)
		{
			if (guidDevice == Guid.Empty)
			{
				guidDevice = guidApplication;
			}
			ITargetSettings targetSettings;
			ITargetSettings targetSettings2;
			MemorySettingsHelperX.GetDeviceTarget(guidDevice, bSimulation, out targetSettings, out targetSettings2);
			if (targetSettings == null)
			{
				return MemorySettingsHelperX._DefaultMemorySettings;
			}
			MemorySettings memorySettings = new MemorySettings(true);
			memorySettings.MemoryDataSize = LocalTargetSettings.MemorySize.GetIntValue(targetSettings);
			memorySettings.OutputDataSize = LocalTargetSettings.OutputSize.GetIntValue(targetSettings);
			memorySettings.InputDataSize = LocalTargetSettings.InputSize.GetIntValue(targetSettings);
			memorySettings.RetainDataSize = LocalTargetSettings.RetainSize.GetIntValue(targetSettings);
			memorySettings.RetainInOwnSegment = LocalTargetSettings.RetainInOwnSegment.GetBoolValue(targetSettings);
			memorySettings.ByteAddressing = LocalTargetSettings.ByteAddressing.GetBoolValue(targetSettings);
			memorySettings.BitByteAddressing = LocalTargetSettings.BitByteAddressing.GetBoolValue(targetSettings);
			memorySettings.BitWordAddressing = LocalTargetSettings.BitWordAddressing.GetBoolValue(targetSettings);
			MemorySettingsHelperX._UpdateTargetMemorySettings(guidApplication, memorySettings);
			MemorySettingsHelperX._UpdateCodegenMemorySettings(guidApplication, memorySettings);
			if (guidParentApplication != Guid.Empty)
			{
				if (!APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV34200)
				{
					return MemorySettingsHelperX._GetChildApplicationMemorySettings(guidDevice, guidApplication);
				}
				if (MemorySettingsHelperX._GetMemorySettingsProvider(guidApplication) != null)
				{
					return MemorySettingsHelperX._GetChildApplicationMemorySettings(guidDevice, guidApplication);
				}
				memorySettings.MemoryDataSize = 0;
				memorySettings.OutputDataSize = 0;
				memorySettings.InputDataSize = 0;
			}
			memorySettings.PackMode = LocalTargetSettings.PackMode.GetIntValue(targetSettings);
			if (targetSettings2 != null && APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351100)
			{
				memorySettings.PackMode = LocalTargetSettings.PackMode.GetIntValue(targetSettings2);
			}
			memorySettings.StackAlignment = LocalTargetSettings.StackAlignment.GetIntValue(targetSettings);
			if (targetSettings2 != null)
			{
				memorySettings.StackAlignment = LocalTargetSettings.StackAlignment.GetIntValue(targetSettings2);
			}
			memorySettings.CodeSegmentSize = LocalTargetSettings.CodeSegmentSize.GetIntValue(targetSettings);
			memorySettings.DataSegmentSize = LocalTargetSettings.DataSegmentSize.GetIntValue(targetSettings);
			memorySettings.AdditionalAreas = LocalTargetSettings.AdditionalAreas.GetBoolValue(targetSettings);
			memorySettings.RetainDynamic = LocalTargetSettings.DynamicRetain.GetBoolValue(targetSettings);
			if (memorySettings.RetainDynamic && APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV34000)
			{
				memorySettings.RetainInOwnSegment = true;
			}
			memorySettings.PersistentDynamic = LocalTargetSettings.DynamicPersistent.GetBoolValue(targetSettings);
			memorySettings.CodeSegmentPrologSize = LocalTargetSettings.CodeSegmentPrologSize.GetIntValue(targetSettings);
			if (targetSettings2 != null)
			{
				memorySettings.CodeSegmentPrologSize = LocalTargetSettings.CodeSegmentPrologSize.GetIntValue(targetSettings2);
			}
			memorySettings.CodeSegmentHeaderSize = LocalTargetSettings.CodeSegmentHeaderSize.GetIntValue(targetSettings);
			if (targetSettings2 != null)
			{
				memorySettings.CodeSegmentHeaderSize = LocalTargetSettings.CodeSegmentHeaderSize.GetIntValue(targetSettings2);
			}
			memorySettings.OnlineChangeInOwnSegment = LocalTargetSettings.OnlineChangeInOwnSegment.GetBoolValue(targetSettings);
			memorySettings.MaxSizeForOnlineChange = LocalTargetSettings.MaximumDataAndCodeSize.GetIntValue(targetSettings);
			memorySettings.MinGranularity = LocalTargetSettings.MinimalStructureGranularity.GetIntValue(targetSettings);
			memorySettings.LateRelocationForFixedAreas = LocalTargetSettings.LateRelocationForFixedAreas.GetBoolValue(targetSettings);
			if (guidParentApplication == Guid.Empty)
			{
				IMemorySettingsProvider memorySettingsProvider = MemorySettingsHelperX._GetMemorySettingsProvider(guidApplication);
				if (memorySettingsProvider != null)
				{
					memorySettingsProvider.UpdateMemorySettings(memorySettings);
					return memorySettings;
				}
			}
			MemorySettingsHelperX.AddMemoryAreasFromTargetSettings(guidParentApplication, guidApplication, memorySettings, targetSettings, targetSettings2);
			return memorySettings;
		}

		// Token: 0x06001A13 RID: 6675 RVA: 0x0004AF54 File Offset: 0x00049F54
		internal static void _UpdateCodegenMemorySettings(Guid guidApplication, MemorySettings memsetRet)
		{
			ICompileContext compileContext = APEnvironmentFacade.Instance.LanguageModelMgr.GetCompileContext(guidApplication);
			ICodegenerator codegenerator = (compileContext != null) ? compileContext.Codegenerator : null;
			if (codegenerator != null && codegenerator is ICodegenerator11)
			{
				int areaAlignment;
				if (!(codegenerator as ICodegenerator11).TryGetIntProperty(CodegeneratorIntProperty.AreaAlignment, out areaAlignment))
				{
					areaAlignment = 8;
				}
				memsetRet.AreaAlignment = areaAlignment;
			}
		}

		// Token: 0x06001A14 RID: 6676 RVA: 0x0004AFA4 File Offset: 0x00049FA4
		internal static void GetDeviceTarget(Guid guidDevice, bool bSimulation, out ITargetSettings target, out ITargetSettings simutarget)
		{
			IDeviceIdentification targetIdOfDevice = APEnvironmentFacade.Instance.LanguageModelMgr.ApplicationDeviceTable.GetTargetIdOfDevice(guidDevice);
			target = APEnvironmentFacade.Instance.GetTargetSettingsById(targetIdOfDevice);
			simutarget = null;
			if (bSimulation && APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35200)
			{
				simutarget = APEnvironmentFacade.Instance.GetSimulationTargetSettings(targetIdOfDevice, guidDevice);
				if (!APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35340)
				{
					target = simutarget;
					simutarget = null;
				}
			}
		}

		// Token: 0x06001A15 RID: 6677 RVA: 0x0004B014 File Offset: 0x0004A014
		internal static void AddMemoryAreasFromTargetSettings(Guid guidParentApplication, Guid guidApplication, MemorySettings memsetRet, ITargetSettings target, ITargetSettings simutarget)
		{
			int intValue = LocalTargetSettings.AreasNumber.GetIntValue(target);
			if (intValue == 0)
			{
				foreach (IArea area in MemorySettingsHelperX._DefaultMemorySettings.Areas)
				{
					memsetRet.AddArea(area);
				}
			}
			else
			{
				bool flag = MemorySettingsHelperX.HasStaticArea(memsetRet, target);
				for (int j = 0; j < intValue; j++)
				{
					string str = string.Format("memory-layout\\areas\\area_{0}", j);
					DataSegmentFlags dataSegmentFlags = (DataSegmentFlags)target.GetIntValue(str + "\\flags", (int)LocalTargetSettings.AreaNFlag.DefaultValue);
					if (!flag || (dataSegmentFlags != DataSegmentFlags.Retain && dataSegmentFlags != (DataSegmentFlags.Retain | DataSegmentFlags.Persistent)))
					{
						if (guidParentApplication != Guid.Empty && APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV34200)
						{
							dataSegmentFlags &= ~(DataSegmentFlags.Input | DataSegmentFlags.Output | DataSegmentFlags.Memory);
							if (dataSegmentFlags == DataSegmentFlags.None)
							{
								goto IL_201;
							}
						}
						AreaFlags areaflags = (AreaFlags)target.GetIntValue(str + "\\area_flags", (int)LocalTargetSettings.AreaNAreaFlags.DefaultValue);
						if (simutarget != null)
						{
							areaflags = AreaFlags.DynamicSize;
						}
						Area area2 = new Area(dataSegmentFlags, areaflags);
						area2.MinimalAreaSize = target.GetIntValue(str + "\\minimal-area-size", (int)LocalTargetSettings.AreaNMinimalAreaSize.DefaultValue);
						area2.AllocationPlusInPercent = target.GetIntValue(str + "\\allocation-plus-in-percent", (int)LocalTargetSettings.AreaNAllocationPlusInPercent.DefaultValue);
						area2.StartAddress = target.GetIntValue(str + "\\start-address", (int)LocalTargetSettings.AreaNStartAddress.DefaultValue);
						if (simutarget != null)
						{
							area2.StartAddress = 0;
						}
						area2.Size = target.GetIntValue(str + "\\maximal-area-size", (int)LocalTargetSettings.AreaNMaximalAreaSize.DefaultValue);
						area2.MaximalAreaSize = target.GetIntValue(str + "\\maximal-area-size", (int)LocalTargetSettings.AreaNMaximalAreaSize.DefaultValue);
						area2.AvailableSize = target.GetIntValue(str + "\\available-area-size", (int)LocalTargetSettings.AreaNAvailableAreaSize.DefaultValue);
						memsetRet.AddArea(area2);
					}
					IL_201:;
				}
			}
			MemorySettingsHelperX.UpdateStaticArea(memsetRet, target, simutarget);
		}

		// Token: 0x170006D1 RID: 1745
		// (get) Token: 0x06001A16 RID: 6678 RVA: 0x0004B239 File Offset: 0x0004A239
		internal static IMemorySettingsHelper Singleton
		{
			get
			{
				return MemorySettingsHelperX.s_singleton;
			}
		}

		// Token: 0x06001A17 RID: 6679 RVA: 0x0004B240 File Offset: 0x0004A240
		public IMemorySettingsProvider GetMemorySettingsProvider(Guid guidApplication)
		{
			return MemorySettingsHelperX._GetMemorySettingsProvider(guidApplication);
		}

		// Token: 0x06001A18 RID: 6680 RVA: 0x0004B248 File Offset: 0x0004A248
		public _IMemorySettings GetChildApplicationMemorySettings(Guid guidDevice, Guid guidApplication)
		{
			return MemorySettingsHelperX._GetChildApplicationMemorySettings(guidDevice, guidApplication);
		}

		// Token: 0x170006D2 RID: 1746
		// (get) Token: 0x06001A19 RID: 6681 RVA: 0x0004B251 File Offset: 0x0004A251
		public _IMemorySettings DefaultMemorySettings
		{
			get
			{
				return MemorySettingsHelperX._DefaultMemorySettings;
			}
		}

		// Token: 0x06001A1A RID: 6682 RVA: 0x0004B258 File Offset: 0x0004A258
		public _IMemorySettings GetMemorySettings(Guid guidApplication, bool bSimulation)
		{
			return MemorySettingsHelperX._GetMemorySettings(guidApplication, bSimulation);
		}

		// Token: 0x06001A1B RID: 6683 RVA: 0x0004B261 File Offset: 0x0004A261
		public void UpdateTargetMemorySettings(Guid guidApplication, _IMemorySettings memset)
		{
			MemorySettingsHelperX._UpdateTargetMemorySettings(guidApplication, memset);
		}

		// Token: 0x06001A1C RID: 6684 RVA: 0x0004B26A File Offset: 0x0004A26A
		public _IMemorySettings GetMemorySettings(Guid guidDevice, Guid guidParentApplication, Guid guidApplication, bool bSimulation)
		{
			return MemorySettingsHelperX._GetMemorySettings(guidDevice, guidParentApplication, guidApplication, bSimulation);
		}

		// Token: 0x0400054A RID: 1354
		private static readonly MemorySettingsHelperX s_singleton = new MemorySettingsHelperX();
	}
}
