using System;

namespace _3S.CoDeSys.LanguageModelManager.CommonCompilerData
{
	// Token: 0x020001CE RID: 462
	public class LateCompileErrorException : ApplicationException
	{
		// Token: 0x06002077 RID: 8311 RVA: 0x00059B82 File Offset: 0x00058B82
		internal LateCompileErrorException()
		{
		}

		// Token: 0x06002078 RID: 8312 RVA: 0x00059B8A File Offset: 0x00058B8A
		internal LateCompileErrorException(string msg) : base(msg)
		{
		}
	}
}
