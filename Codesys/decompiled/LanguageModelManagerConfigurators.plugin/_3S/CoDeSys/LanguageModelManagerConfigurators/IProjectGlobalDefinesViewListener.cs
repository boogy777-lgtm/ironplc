using System.Collections.Generic;

namespace _3S.CoDeSys.LanguageModelManagerConfigurators
{
	public interface IProjectGlobalDefinesViewListener
	{
		void Initialize(IAPEnvironmentFacade apEnvironmentFacade, IList<string> projectDefines);

		void TableContentChanged(string oldEntry, string newEntry);

		void SelectionChanged(int iIndex);

		void AddDefine();

		void RemoveDefine();

		void ApplyChanges();

		bool ValidateIdentifier(string defineName);
	}
}
