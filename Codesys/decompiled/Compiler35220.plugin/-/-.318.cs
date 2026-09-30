using System;
using _3S.CoDeSys.Compiler35220;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0012
{
	// Token: 0x02000365 RID: 869
	internal static class \u0014
	{
		// Token: 0x060033ED RID: 13293 RVA: 0x000CBFE0 File Offset: 0x000CA1E0
		internal static bool \u0001(_ISignature \u0002)
		{
			bool flag = \u0002.GetFlag(SignatureFlag.Generated) && \u0002.GetFlagInternal(SignatureFlagInternal.ContainsGenericConstants);
			GUIHidingFlags guihidingFlags = GUIHidingFlags.AllCommon;
			if (flag)
			{
				guihidingFlags ^= GUIHidingFlags.SignatureGenerated;
			}
			return \u0002.HasMemoryReserve && !\u0002.IsLibraryObject && !APEnvironmentFacade.Instance.LanguageModelMgr.IsHiddenSignature(\u0002, guihidingFlags);
		}

		// Token: 0x060033EE RID: 13294 RVA: 0x000CC048 File Offset: 0x000CA248
		internal static bool \u0002(_ISignature \u0002)
		{
			return !\u0002.IsLibraryObject;
		}

		// Token: 0x060033EF RID: 13295 RVA: 0x000CC054 File Offset: 0x000CA254
		internal static void \u0001(_ISignature \u0002)
		{
			if (\u0002.HasMemoryReserve && \u0002.HasFlag(SignatureFlag.External))
			{
				\u0002.AddMessage(Severity.Error, MessageId.Err_NoMemoryReserveForExternal, Array.Empty<object>());
			}
		}
	}
}
