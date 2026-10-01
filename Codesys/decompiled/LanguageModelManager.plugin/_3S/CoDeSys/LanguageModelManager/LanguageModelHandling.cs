using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x020000C8 RID: 200
	internal static class LanguageModelHandling
	{
		// Token: 0x17000315 RID: 789
		// (get) Token: 0x06000C36 RID: 3126 RVA: 0x0001F560 File Offset: 0x0001E560
		private static ILanguageModelHandling _LMMHandler
		{
			get
			{
				return VersionedCompilerFactory._LMMHandler;
			}
		}

		// Token: 0x06000C37 RID: 3127 RVA: 0x0001F567 File Offset: 0x0001E567
		internal static void AddImplicitLanguageModel(_ILanguageModelManagerConsolidated lmm)
		{
			LanguageModelHandling._LMMHandler.AddImplicitLanguageModel(lmm);
		}

		// Token: 0x06000C38 RID: 3128 RVA: 0x0001F574 File Offset: 0x0001E574
		internal static void AddLanguageModel(_ILanguageModelManagerConsolidated lmm, string stContribution, bool bExternal, string stCompilerDefines, Guid guidApplication, Guid guidLanguageModelControl, string stLibraryPath, bool bEnableSystemCall, bool bShowSyntaxErrors, SignatureFlag sf_defaultFlag, IList<IList<string>> strStringListTable)
		{
			LanguageModelHandling._LMMHandler.AddLanguageModel(lmm, stContribution, bExternal, stCompilerDefines, guidApplication, guidLanguageModelControl, stLibraryPath, bEnableSystemCall, bShowSyntaxErrors, sf_defaultFlag, strStringListTable);
		}

		// Token: 0x06000C39 RID: 3129 RVA: 0x0001F59D File Offset: 0x0001E59D
		public static void AddStructuredLanguageModel(_ILanguageModelManagerConsolidated lmm, ILanguageModel lm, bool bExternal, string stCompilerDefines, bool bEnableSystemCall, SignatureFlag sfDefaultFlag, bool bShowSyntaxErrors)
		{
			LanguageModelHandling._LMMHandler.AddStructuredLanguageModel(lmm, lm, bExternal, stCompilerDefines, bEnableSystemCall, sfDefaultFlag, bShowSyntaxErrors);
		}

		// Token: 0x06000C3A RID: 3130 RVA: 0x0001F5B3 File Offset: 0x0001E5B3
		public static ILanguageModel CreateLanguageModelOfXml(string stContribution, IList<IList<string>> stringlistTable)
		{
			return LanguageModelHandling._LMMHandler.CreateLanguageModelOfXml(stContribution, stringlistTable);
		}

		// Token: 0x06000C3B RID: 3131 RVA: 0x0001F5C1 File Offset: 0x0001E5C1
		public static void AddLanguageModelForPOU(_ILanguageModelManagerConsolidated lmm, ILMPOU lmpou, Guid objectGuidLmGlobal, _IPreCompileContext comcon, string stLibraryId)
		{
			ILanguageModelHandling2 languageModelHandling = LanguageModelHandling._LMMHandler as ILanguageModelHandling2;
			if (languageModelHandling == null)
			{
				return;
			}
			languageModelHandling.AddLanguageModelForPOU(lmm, lmpou, objectGuidLmGlobal, comcon, stLibraryId);
		}

		// Token: 0x06000C3C RID: 3132 RVA: 0x0001F5DD File Offset: 0x0001E5DD
		public static void AddLanguageModelForDUT(_ILanguageModelManagerConsolidated lmm, ILMDataType lmdut, Guid objectGuidLmGlobal, _IPreCompileContext comcon, string stLibraryId)
		{
			ILanguageModelHandling2 languageModelHandling = LanguageModelHandling._LMMHandler as ILanguageModelHandling2;
			if (languageModelHandling == null)
			{
				return;
			}
			languageModelHandling.AddLanguageModelForDUT(lmm, lmdut, objectGuidLmGlobal, comcon, stLibraryId);
		}

		// Token: 0x06000C3D RID: 3133 RVA: 0x0001F5F9 File Offset: 0x0001E5F9
		public static void AddLanguageModelForGVL(_ILanguageModelManagerConsolidated lmm, ILMGlobVarlist lmgvl, Guid objectGuidLmGlobal, _IPreCompileContext comcon, string stLibraryId)
		{
			ILanguageModelHandling2 languageModelHandling = LanguageModelHandling._LMMHandler as ILanguageModelHandling2;
			if (languageModelHandling == null)
			{
				return;
			}
			languageModelHandling.AddLanguageModelForGVL(lmm, lmgvl, objectGuidLmGlobal, comcon, stLibraryId);
		}
	}
}
