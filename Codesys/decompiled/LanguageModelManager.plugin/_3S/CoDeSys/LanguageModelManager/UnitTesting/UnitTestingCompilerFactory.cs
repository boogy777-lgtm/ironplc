using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.UnitTesting
{
	// Token: 0x020001B0 RID: 432
	internal static class UnitTestingCompilerFactory
	{
		// Token: 0x1700080D RID: 2061
		// (get) Token: 0x06001EEC RID: 7916 RVA: 0x00054E50 File Offset: 0x00053E50
		internal static IAddressCalculator AddressCalculator
		{
			get
			{
				return VersionedCompilerFactory._AddressCalculator;
			}
		}

		// Token: 0x1700080E RID: 2062
		// (get) Token: 0x06001EED RID: 7917 RVA: 0x00054E57 File Offset: 0x00053E57
		internal static ICompilerHelper3 CompilerHelper3
		{
			get
			{
				return VersionedCompilerFactory._Helper as ICompilerHelper3;
			}
		}
	}
}
