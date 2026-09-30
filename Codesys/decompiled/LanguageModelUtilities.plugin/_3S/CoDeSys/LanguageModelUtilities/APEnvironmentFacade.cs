namespace _3S.CoDeSys.LanguageModelUtilities
{
	public static class APEnvironmentFacade
	{
		private static IAPEnvironmentFacade s_apEnvironmentFacade;

		public static IAPEnvironmentFacade Instance
		{
			get
			{
				if (s_apEnvironmentFacade == null)
				{
					s_apEnvironmentFacade = new APEnvironmentFacadeDesktop();
				}
				return s_apEnvironmentFacade;
			}
			set
			{
				s_apEnvironmentFacade = value;
			}
		}
	}
}
