using System;

namespace CODESYS.Parser35220.Scanner
{
	// Token: 0x02000012 RID: 18
	[Flags]
	internal enum OperatorFlags : uint
	{
		// Token: 0x04000036 RID: 54
		DataType = 1U,
		// Token: 0x04000037 RID: 55
		Operator = 2U,
		// Token: 0x04000038 RID: 56
		Keyword = 4U,
		// Token: 0x04000039 RID: 57
		NumericDataType = 8U,
		// Token: 0x0400003A RID: 58
		SafetyDataType = 16U,
		// Token: 0x0400003B RID: 59
		Contextual = 32U,
		// Token: 0x0400003C RID: 60
		StructuredText = 65536U,
		// Token: 0x0400003D RID: 61
		InstructionList = 131072U,
		// Token: 0x0400003E RID: 62
		FunctionBlockDiagram = 262144U,
		// Token: 0x0400003F RID: 63
		Declaration = 524288U,
		// Token: 0x04000040 RID: 64
		Internal = 1048576U,
		// Token: 0x04000041 RID: 65
		AllLanguages = 4294901760U
	}
}
