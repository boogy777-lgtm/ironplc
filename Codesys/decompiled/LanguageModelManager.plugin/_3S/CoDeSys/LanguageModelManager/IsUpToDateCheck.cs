using System;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.LanguageModelManager.Legacy;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x020000B4 RID: 180
	internal static class IsUpToDateCheck
	{
		// Token: 0x06000A7C RID: 2684 RVA: 0x00017F7D File Offset: 0x00016F7D
		internal static bool IsLmUpToDate(CompileContext comcon, _IPreCompileContext precomp, _IPreCompileContext precompPool, _IIsUpTopDateStrategy strategy)
		{
			if (VersionedCompilerFactory._UpToDateChecker_OrNull == null)
			{
				return IsUpToDateCheck.IsLmUpToDate(comcon, precomp, precompPool, strategy);
			}
			return VersionedCompilerFactory._UpToDateChecker_OrNull.IsLmUpToDate(comcon, precomp, precompPool, strategy);
		}

		// Token: 0x06000A7D RID: 2685 RVA: 0x00017F9E File Offset: 0x00016F9E
		internal static _IIsUpTopDateStrategy CreateFastIsUpToDateStrategy()
		{
			if (VersionedCompilerFactory._UpToDateChecker_OrNull == null)
			{
				return new FastIsUpToDateStrategy();
			}
			return VersionedCompilerFactory._UpToDateChecker_OrNull.CreateFastIsUpToDateStrategy();
		}

		// Token: 0x06000A7E RID: 2686 RVA: 0x00017FB7 File Offset: 0x00016FB7
		internal static _IIsUpTopDateStrategy CreateDetailedIsUpToDateStrategy(_ICompileContext comcon, _IPreCompileContext precomp, bool bCollectAllOnlineChangeProhibitingChanges)
		{
			if (VersionedCompilerFactory._UpToDateChecker_OrNull == null)
			{
				return new DetailedIsUpToDateStrategy(comcon, precomp, bCollectAllOnlineChangeProhibitingChanges);
			}
			return VersionedCompilerFactory._UpToDateChecker_OrNull.CreateDetailedIsUpToDateStrategy(comcon, precomp, bCollectAllOnlineChangeProhibitingChanges);
		}
	}
}
