using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x020000DC RID: 220
	internal class PreCompileErrors : IPrecompileErrors
	{
		// Token: 0x17000472 RID: 1138
		// (get) Token: 0x06000F87 RID: 3975 RVA: 0x0002A914 File Offset: 0x00029914
		internal static PreCompileErrors Singleton
		{
			get
			{
				return PreCompileErrors.s_singleton;
			}
		}

		// Token: 0x06000F88 RID: 3976 RVA: 0x0002A91B File Offset: 0x0002991B
		public void ShowPrecompileErrors(_IPreCompileContext pcc, IList<_ISignature> signs)
		{
			PreCompileErrors._ShowPrecompileErrors(pcc, signs);
		}

		// Token: 0x06000F89 RID: 3977 RVA: 0x0002A924 File Offset: 0x00029924
		internal static void _ShowAllPrecompileErrors()
		{
			_ILanguageModelManagerConsolidated languageModelMgr = APEnvironmentFacade.Instance.LanguageModelMgr;
			if (!languageModelMgr.LateLibraryLoadFinished)
			{
				return;
			}
			IMessageCategory singleton = PreCompileMessageCategory.Singleton;
			APEnvironmentFacade.Instance.MessageStorage.ClearMessages(singleton);
			foreach (_IPreCompileContext ipreCompileContext in languageModelMgr._PrecompileContexts)
			{
				ipreCompileContext.MessageOutput(APEnvironmentFacade.Instance.MessageStorage, singleton);
			}
			languageModelMgr.Pool.MessageOutput(APEnvironmentFacade.Instance.MessageStorage, singleton);
		}

		// Token: 0x06000F8A RID: 3978 RVA: 0x0002A9BC File Offset: 0x000299BC
		internal static void _ForceShowPrecompileErrors()
		{
			_ILanguageModelManagerConsolidated languageModelMgr = APEnvironmentFacade.Instance.LanguageModelMgr;
			if (APEnvironmentFacade.Instance.LanguageModelMgr.LateLibraryLoadFinished)
			{
				foreach (_IPreCompileContext ipreCompileContext in languageModelMgr._PrecompileContexts)
				{
					ipreCompileContext.Dirty = true;
				}
				languageModelMgr.Pool.Dirty = true;
			}
		}

		// Token: 0x06000F8B RID: 3979 RVA: 0x0002AA30 File Offset: 0x00029A30
		internal static void _ShowPrecompileErrors(_IPreCompileContext pcc, IList<_ISignature> signs)
		{
			if (!APEnvironmentFacade.Instance.LanguageModelMgr.LateLibraryLoadFinished)
			{
				return;
			}
			bool flag = pcc != null;
			IMessageCategory singleton = PreCompileMessageCategory.Singleton;
			foreach (_ISignature isignature in signs)
			{
				if (!APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35940 || string.IsNullOrEmpty(isignature.LibraryId))
				{
					if (!flag)
					{
						pcc = (APEnvironmentFacade.Instance.LanguageModelMgr.GetPrecompileContextOfSignature(isignature) as PreCompileContext);
					}
					if (pcc != null && (!APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35900 || APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35940 || !APEnvironmentFacade.Instance.LanguageModelMgr.IsHiddenSignature(isignature, GUIHidingFlags.AllCommon)))
					{
						pcc.MessageOutput(APEnvironmentFacade.Instance.MessageStorage, singleton, isignature.ObjectGuid);
					}
				}
			}
		}

		// Token: 0x04000393 RID: 915
		private static PreCompileErrors s_singleton = new PreCompileErrors();
	}
}
