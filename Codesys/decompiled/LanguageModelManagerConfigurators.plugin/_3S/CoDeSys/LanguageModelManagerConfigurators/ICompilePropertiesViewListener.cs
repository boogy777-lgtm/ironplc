namespace _3S.CoDeSys.LanguageModelManagerConfigurators
{
	public interface ICompilePropertiesViewListener
	{
		void Initialize(bool runsInDebugmode);

		void CompilerVersionChanged(string stNewCompilerVersion);

		void Triggered(ECompilePropertiesEditorPosition position);

		ValidationResult ValidateAndSaveModelFromView();
	}
}
