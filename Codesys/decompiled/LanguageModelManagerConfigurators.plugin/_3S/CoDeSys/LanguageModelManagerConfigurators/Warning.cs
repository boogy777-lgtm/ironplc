namespace _3S.CoDeSys.LanguageModelManagerConfigurators
{
	internal class Warning
	{
		private readonly int _nId;

		internal string IdString => $"C{_nId:D4}";

		internal int Id => _nId;

		internal string Message
		{
			get
			{
				string text = APEnvironment.LMServiceProvider.ConfigurationService.WarningConfiguration.GetLocalizedWarningText(_nId);
				int num = 0;
				while (true)
				{
					string oldValue = $"{{{num}}}";
					if (!(text != text.Replace(oldValue, "...")))
					{
						break;
					}
					text = text.Replace(oldValue, "...");
					num++;
				}
				return text;
			}
		}

		internal bool Active { get; set; }

		internal bool AsError { get; set; }

		internal Warning(int nId, bool bActive, bool bAsError)
		{
			_nId = nId;
			Active = bActive;
			AsError = bAsError;
		}
	}
}
