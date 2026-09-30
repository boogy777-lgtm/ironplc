using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _ISlotPOUList
	{
		void Add(Guid guidTask, int nSlot, Guid guidObject);

		void AddDownloadSlot(int nSlot, Guid guidObject);

		void AddOnlineChangeSlot(int nSlot, Guid guidObject);

		void Remove(Guid ObjectGuid);

		void RemoveByTaskGuid(Guid TaskGuid);

		int[] GetSlotArray(Guid guidTask);

		_ISignature[] GetTaskSlotSignatures(Guid guidTask, int nSlot, _ICompileContext comcon);

		_ICompiledPOU[] GetTaskSlotPOUs(Guid guidTask, int nSlot, _ICompileContext comcon);

		_ISlotPOUList Duplicate();

		Guid[] GetAllTasksForObjectGuid(Guid guidObject);

		_ISignature[] GetAllTaskPOUs(_ICompileContext comcon);

		Guid[] GetDownloadGuidsSortedBySlot(out int[] nSlots);

		Guid[] GetOnlineChangeGuidsSortedBySlot(out int[] nSlots);
	}
}
