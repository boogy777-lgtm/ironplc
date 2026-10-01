using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelUtilities.MonitoringByteCode
{
	internal static class TMH
	{
		public static ICompiledType GetTypeFromClass(TypeClass tc)
		{
			ILMTypeService typeService = GetTypeService();
			if (typeService == null)
			{
				return GetTypeFromClassLegacy(tc);
			}
			return typeService.GetType(tc);
		}

		private static ILMTypeService GetTypeService()
		{
			if (APEnvironmentFacade.Instance.LMServiceProvider.PreCompileService is ILMPreCompileService5 iLMPreCompileService)
			{
				return iLMPreCompileService.CreateTypeService();
			}
			return null;
		}

		private static ICompiledType GetTypeFromClassLegacy(TypeClass tc)
		{
			string stText;
			switch (tc)
			{
			case TypeClass.DateAndTime:
				stText = "DATE_AND_TIME";
				break;
			case TypeClass.TimeOfDay:
				stText = "TIME_OF_DAY";
				break;
			default:
				stText = tc.ToString();
				break;
			}
			IScanner scanner = APEnvironmentFacade.Instance.LanguageModelMgr.CreateScanner(stText, bIncludeComments: false, bIncludeEndOfLines: false, bIncludePragmas: false, bIncludeWhitespaces: false);
			return APEnvironmentFacade.Instance.LanguageModelMgr.CreateParser(scanner).ParseTypeDeclaration();
		}
	}
}
