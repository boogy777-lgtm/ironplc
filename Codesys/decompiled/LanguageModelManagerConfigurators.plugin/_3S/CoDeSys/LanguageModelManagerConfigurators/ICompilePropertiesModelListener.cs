using System.Collections.Generic;

namespace _3S.CoDeSys.LanguageModelManagerConfigurators
{
	public interface ICompilePropertiesModelListener
	{
		void InitializeView();

		void ShowCompilerversions(IEnumerable<string> compilerversions);

		void ShowCompilerversion(string stCompilerversion);

		void ShowMaxNumbersOfWarnings(IEnumerable<string> maxNumbersOfWarnings);

		void ShowMaxNumberOfWarnings(string stMaxNumberOfWarnings);

		void SetEnabled(ECompilePropertiesEditorPosition position, bool enabled);

		void SetOptionState(ECompilePropertiesEditorPosition position, bool selected);

		bool GetOption(ECompilePropertiesEditorPosition position);

		string GetText(ECompilePropertiesEditorPosition position);

		void SetMessage(ECompilePropertiesEditorPosition position, string message, string iconName);

		void HideMessage();

		object GetSelected(ECompilePropertiesEditorPosition position);

		void DisableCustomTextInput(ECompilePropertiesEditorPosition position);

		IList<string> StartDialog(ECompilePropertiesEditorPosition position, IList<string> startMessage);
	}
}
