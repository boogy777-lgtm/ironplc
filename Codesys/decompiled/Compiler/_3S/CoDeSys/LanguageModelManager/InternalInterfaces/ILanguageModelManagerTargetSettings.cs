using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.TargetSettings;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface ILanguageModelManagerTargetSettings
	{
		IRegisteredTargetSetting RuntimeVersion { get; }

		IRegisteredTargetSetting MinimalSystem { get; }

		IRegisteredTargetSetting SimpleCycle { get; }

		IRegisteredTargetSetting CycleControlVersion2 { get; }

		IRegisteredTargetSetting CycleControlInIec { get; }

		IRegisteredTargetSetting CompactDownload { get; }

		IRegisteredTargetSetting BreakpointsSupported { get; }

		IRegisteredTargetSetting EnableBreakpointLogging { get; }

		IRegisteredTargetSetting OptimizedOnlineChange { get; }

		IRegisteredTargetSetting CodegeneratorGuid { get; }

		IRegisteredTargetSetting CompilerDefines { get; }

		IRegisteredTargetSetting SupportSystemApplications { get; }

		IRegisteredTargetSetting NewVfTable { get; }

		IRegisteredTargetSetting LintDataTypes { get; }

		IRegisteredTargetSetting LRealDataType { get; }

		IRegisteredTargetSetting NoPrecompileMessages { get; }

		IRegisteredTargetSetting NoBytesInRetain { get; }

		IRegisteredTargetSetting CheckMisalignedAddress { get; }

		IRegisteredTargetSetting LinkAllGlobalVariables { get; }

		IRegisteredTargetSetting NoDefaultInitialisation { get; }

		IRegisteredTargetSetting UnsupportedOperators { get; }

		IRegisteredTargetSetting SupportedSystemOperators { get; }

		IRegisteredTargetSetting ByteSupport { get; }

		IRegisteredTargetSetting LRealAsReal { get; }

		IRegisteredTargetSetting Int64AsInt32 { get; }

		IRegisteredTargetSetting GenerateDirectCalls { get; }

		IRegisteredTargetSetting GvlInitFunctions { get; }

		IRegisteredTargetSetting RetainInCycle { get; }

		IRegisteredTargetSetting RetainCycleTask { get; }

		IRegisteredTargetSetting DoPersistentCode { get; }

		IRegisteredTargetSetting VarconfigGeneratorGuid { get; }

		IRegisteredTargetSetting MemoryAllocationCallback { get; }

		IRegisteredTargetSetting CodegenMultithreading { get; }

		IRegisteredTargetSetting CheckMultipleTaskOutputWrite { get; }

		IRegisteredTargetSetting MaximumNumApplications { get; }

		IRegisteredTargetSetting UnsupportedDataTypes { get; }

		IRegisteredTargetSetting AddressCalculatorGuid { get; }

		IRegisteredTargetSetting ExternalRealStringConversions { get; }

		IRegisteredTargetSetting SupportMulticore { get; }

		IRegisteredTargetSetting MemorySize { get; }

		IRegisteredTargetSetting OutputSize { get; }

		IRegisteredTargetSetting RetainSize { get; }

		IRegisteredTargetSetting InputSize { get; }

		IRegisteredTargetSetting StaticAreaSize { get; }

		IRegisteredTargetSetting RetainInOwnSegment { get; }

		IRegisteredTargetSetting PackMode { get; }

		IRegisteredTargetSetting VectorGranularity { get; }

		IRegisteredTargetSetting StackAlignment { get; }

		IRegisteredTargetSetting CodeSegmentSize { get; }

		IRegisteredTargetSetting DataSegmentSize { get; }

		IRegisteredTargetSetting AdditionalAreas { get; }

		IRegisteredTargetSetting DynamicRetain { get; }

		IRegisteredTargetSetting DynamicPersistent { get; }

		IRegisteredTargetSetting CodeSegmentPrologSize { get; }

		IRegisteredTargetSetting CodeSegmentHeaderSize { get; }

		IRegisteredTargetSetting OnlineChangeInOwnSegment { get; }

		IRegisteredTargetSetting OnlineChangeSupported { get; }

		IRegisteredTargetSetting ByteAddressing { get; }

		IRegisteredTargetSetting BitByteAddressing { get; }

		IRegisteredTargetSetting BitWordAddressing { get; }

		IRegisteredTargetSetting MaximumDataAndCodeSize { get; }

		IRegisteredTargetSetting MinimalStructureGranularity { get; }

		IRegisteredTargetSetting ConstantsInOwnSegment { get; }

		IRegisteredTargetSetting StaticAreaStartAddress { get; }

		IRegisteredTargetSetting StaticAreaAllowUserDefinedSize { get; }

		IRegisteredTargetSetting AreasNumber { get; }

		IRegisteredTargetSetting AreaNFlag { get; }

		IRegisteredTargetSetting AreaNAreaFlags { get; }

		IRegisteredTargetSetting AreaNMinimalAreaSize { get; }

		IRegisteredTargetSetting AreaNMaximalAreaSize { get; }

		IRegisteredTargetSetting AreaNAllocationPlusInPercent { get; }

		IRegisteredTargetSetting AreaNAvailableAreaSize { get; }

		IRegisteredTargetSetting AreaNStartAddress { get; }

		IRegisteredTargetSetting MaxStackSize { get; }

		IRegisteredTargetSetting MaxStackSizeExternalCall { get; }

		IRegisteredTargetSetting SupportUserCheckFunctions { get; }

		IRegisteredTargetSetting MemoryBarrier { get; }
	}
}
