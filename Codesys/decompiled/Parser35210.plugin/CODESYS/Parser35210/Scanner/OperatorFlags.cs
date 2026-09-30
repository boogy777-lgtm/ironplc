using System;

namespace CODESYS.Parser35210.Scanner
{
	[Flags]
	internal enum OperatorFlags : uint
	{
		DataType = 1u,
		Operator = 2u,
		Keyword = 4u,
		NumericDataType = 8u,
		SafetyDataType = 0x10u,
		Contextual = 0x20u,
		StructuredText = 0x10000u,
		InstructionList = 0x20000u,
		FunctionBlockDiagram = 0x40000u,
		Declaration = 0x80000u,
		Internal = 0x100000u,
		AllLanguages = 0xFFFF0000u
	}
}
