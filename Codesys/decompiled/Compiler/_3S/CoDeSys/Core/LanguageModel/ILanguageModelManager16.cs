using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILanguageModelManager16 : ILanguageModelManager15, ILanguageModelManager14, ILanguageModelManager13, ILanguageModelManager12, ILanguageModelManager11, ILanguageModelManager10, ILanguageModelManager9, ILanguageModelManager8, ILanguageModelManager7, ILanguageModelManager6, ILanguageModelManager5, ILanguageModelManager4, ILanguageModelManager3, ILanguageModelManager2, ILanguageModelManager
	{
		event IsHiddenVariableEventHandler IsHiddenVariableHandler;

		event SimulationModeChangedEventHandler AfterSimulationModeChanged;

		ILanguageModelBuilder CreateLanguageModelBuilder();

		IList<ITaskCrossref> GetTaskReferencesOfInstancePath(string stInstancePath);
	}
}
