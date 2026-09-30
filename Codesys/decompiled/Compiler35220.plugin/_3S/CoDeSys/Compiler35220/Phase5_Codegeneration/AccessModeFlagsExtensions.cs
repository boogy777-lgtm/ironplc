using System;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.Compiler35220.Phase5_Codegeneration
{
	// Token: 0x0200020F RID: 527
	public static class AccessModeFlagsExtensions
	{
		// Token: 0x06002309 RID: 8969 RVA: 0x00078588 File Offset: 0x00076788
		public static bool GetFlag(this AccessModeFlags self, AccessModeFlags check)
		{
			return (self & check) == check;
		}
	}
}
