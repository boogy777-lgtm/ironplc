using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILanguageModel
	{
		Guid ApplicationGuid { get; set; }

		Guid DeviceGuid { get; set; }

		Guid LanguageModelObject { get; set; }

		string LibraryId { get; set; }

		ILMDevice LMDevice { get; set; }

		ILMApplication LMApplication { get; set; }

		ILMTaskList LMTaskList { get; set; }

		ILMLibraryList LMLibraryList { get; set; }

		ILMPOU[] Pous { get; }

		ILMGlobVarlist[] GlobalVariableLists { get; }

		ILMDataType[] DataTypes { get; }

		void AddPou(ILMPOU lmpou);

		void AddGlobalVariableList(ILMGlobVarlist lmgvl);

		void AddDataType(ILMDataType lmdut);
	}
}
