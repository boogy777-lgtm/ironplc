using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _IDirectVariableCrossRefTable : IDirectVariableCrossRefTable2, IDirectVariableCrossRefTable
	{
		void SetNumberOfTasks(int nTaskNum);

		void AddProcessImageLocation(IDataLocation datloc, int nSize, int nTaskId, DirectVariableLocation loc, AccessFlag access);
	}
}
