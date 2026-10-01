using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[Flags]
	public enum GUIHidingFlags : ulong
	{
		[ReleasedEnumMember]
		None = 0uL,
		[ReleasedEnumMember]
		VarImplicit = 1uL,
		[ReleasedEnumMember]
		VarLazy = 2uL,
		[ReleasedEnumMember]
		VarImplicitParamsStruct = 4uL,
		[ReleasedEnumMember]
		VarEnum = 8uL,
		[ReleasedEnumMember]
		VarConstant = 0x10uL,
		[ReleasedEnumMember]
		VarRelativeStack = 0x20uL,
		[ReleasedEnumMember]
		VarReplacedConstant = 0x40uL,
		[ReleasedEnumMember]
		VarTemp = 0x80uL,
		[ReleasedEnumMember]
		CommonVarFlags = 1uL,
		[ReleasedEnumMember]
		AllVarFlags = 0xFFFFuL,
		[ReleasedEnumMember]
		SignatureGenerated = 0x10000uL,
		[ReleasedEnumMember]
		SignatureImplictInterfaceUnion = 0x20000uL,
		[ReleasedEnumMember]
		SignatureImplictParamsStruct = 0x40000uL,
		[ReleasedEnumMember]
		SignatureInternal = 0x80000uL,
		[ReleasedEnumMember]
		SignaturePrivate = 0x100000uL,
		[ReleasedEnumMember]
		SignatureProtected = 0x200000uL,
		[ReleasedEnumMember]
		CommonSignatureFlags = 0x70000uL,
		[ReleasedEnumMember]
		AllSignatureFlags = 0xFFFF0000uL,
		[ReleasedEnumMember]
		DoNotEvaluateAttributeHide = 0x100000000uL,
		[ReleasedEnumMember]
		EvaluateFlags = 0x1000000000000000uL,
		[ReleasedEnumMember]
		EvaluateImplicitNames = 0x2000000000000000uL,
		[ReleasedEnumMember]
		EvaluateAttributes = 0x4000000000000000uL,
		[ReleasedEnumMember]
		AllEvaluation = 18446462598732840960uL,
		[ReleasedEnumMember]
		AllCommon = 18446462598733299713uL,
		[ReleasedEnumMember]
		AllChecks = 18446744069414584319uL,
		[Obsolete("This member includes exception flags and thus should be avoided. Use AllChecks instead")]
		[ReleasedEnumMember]
		AllDefault = ulong.MaxValue
	}
}
