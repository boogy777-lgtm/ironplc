using System;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x020000B9 RID: 185
	internal static class LibraryHelper
	{
		// Token: 0x06000AEC RID: 2796 RVA: 0x0001C708 File Offset: 0x0001B708
		internal static string VersionFreeLibraryPath(string stDisplayName)
		{
			return CompilerProxy.VersionFreeLibraryPath(stDisplayName);
		}

		// Token: 0x06000AED RID: 2797 RVA: 0x0001C710 File Offset: 0x0001B710
		internal static bool IsNewestLibrary(string stLibrary)
		{
			return CompilerProxy.IsNewestLibrary(stLibrary);
		}

		// Token: 0x06000AEE RID: 2798 RVA: 0x0001C718 File Offset: 0x0001B718
		internal static bool ParseLibraryId(string stLibraryId, out string stTitle, out string stCompany, out Version v)
		{
			return CompilerProxy.ParseLibraryId(stLibraryId, out stTitle, out stCompany, out v);
		}

		// Token: 0x06000AEF RID: 2799 RVA: 0x0001C723 File Offset: 0x0001B723
		internal static bool IsEqualLibraryNoVersion(string stLibraryId1, string stLibraryId2, out Version foundVersion)
		{
			return CompilerProxy.IsEqualLibraryNoVersion(stLibraryId1, stLibraryId2, out foundVersion);
		}
	}
}
