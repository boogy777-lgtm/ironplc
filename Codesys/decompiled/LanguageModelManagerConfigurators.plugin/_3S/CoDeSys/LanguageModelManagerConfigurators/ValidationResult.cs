namespace _3S.CoDeSys.LanguageModelManagerConfigurators
{
	public class ValidationResult
	{
		public bool IsValid { get; set; }

		public string OptionalErrorMessage { get; set; }

		public ECompilePropertiesEditorPosition FailedControl { get; set; }
	}
}
