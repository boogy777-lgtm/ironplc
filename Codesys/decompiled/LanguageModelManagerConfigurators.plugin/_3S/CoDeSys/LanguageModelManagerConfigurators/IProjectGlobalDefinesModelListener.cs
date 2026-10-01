using System.Collections.Generic;

namespace _3S.CoDeSys.LanguageModelManagerConfigurators
{
	public interface IProjectGlobalDefinesModelListener
	{
		IList<string> DialogAnswer { get; set; }

		void SetEnabled(EProjectGlobalDefinesDialogPosition position, bool enabled);

		void SetTableModel(ProjectGlobalDefinesTableModel tableModel);
	}
}
