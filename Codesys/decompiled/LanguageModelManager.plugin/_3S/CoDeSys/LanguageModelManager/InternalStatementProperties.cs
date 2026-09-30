using System;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000130 RID: 304
	[Flags]
	public enum InternalStatementProperties : byte
	{
		// Token: 0x04000572 RID: 1394
		GenerateFlow = 1,
		// Token: 0x04000573 RID: 1395
		GenerateBP = 2,
		// Token: 0x04000574 RID: 1396
		Implicit = 4
	}
}
