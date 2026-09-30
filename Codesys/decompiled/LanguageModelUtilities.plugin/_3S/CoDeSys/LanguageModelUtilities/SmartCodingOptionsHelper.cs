using _3S.CoDeSys.Core.Options;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	internal abstract class SmartCodingOptionsHelper
	{
		private static readonly string SHOWALLINSTANCEVARS = "ShowAllInstanceVars";

		private static readonly string ENABLE_SHOWSYMBOLSOFSUBLIBRARIES = "EnableShowSymbolsOfSubLibraries";

		internal static readonly string SMARTCODING_SUB_KEY = "{24F0D403-55B9-41af-A07F-8C46BDAEEF8E}";

		public static bool ShowAllInstanceVars
		{
			get
			{
				if (OptionKey.HasValue(SHOWALLINSTANCEVARS, typeof(bool)))
				{
					return (bool)OptionKey[SHOWALLINSTANCEVARS];
				}
				return false;
			}
		}

		internal static bool ShowSymbolsOfSubLibraries
		{
			get
			{
				if (OptionKey.HasValue(ENABLE_SHOWSYMBOLSOFSUBLIBRARIES, typeof(bool)))
				{
					return (bool)OptionKey[ENABLE_SHOWSYMBOLSOFSUBLIBRARIES];
				}
				return true;
			}
		}

		private static IOptionKey OptionKey => APEnvironmentFacade.Instance.CreateSubKey(OptionRoot.User, SMARTCODING_SUB_KEY);
	}
}
