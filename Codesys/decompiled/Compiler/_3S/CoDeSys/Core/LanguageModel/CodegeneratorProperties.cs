using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	public enum CodegeneratorProperties
	{
		[ReleasedEnumMember]
		PositiveStackGrow,
		[ReleasedEnumMember]
		WordAddressing,
		[ReleasedEnumMember]
		MinimalGranularity2,
		[ReleasedEnumMember]
		LWordPointer,
		[ReleasedEnumMember]
		UseBaseRegAsDestInDeref,
		[ReleasedEnumMember]
		CCallingConvention,
		[ReleasedEnumMember]
		DirectCallPossible,
		[ReleasedEnumMember]
		OperatorMemSetImplemented,
		[ReleasedEnumMember]
		CheckConcurrentBitAccess,
		[ReleasedEnumMember]
		DestIsSecondOperand,
		[ReleasedEnumMember]
		Thumb2,
		[ReleasedEnumMember]
		SupportsVectorOperations,
		[ReleasedEnumMember]
		UseTryCatchBaseRegister,
		[ReleasedEnumMember]
		StringLiteral2ByteAligned,
		[ReleasedEnumMember]
		StringLiteral4ByteAligned,
		[ReleasedEnumMember]
		OptimizeMemcopy,
		[ReleasedEnumMember]
		EABIStackframe
	}
}
