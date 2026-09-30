using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _ISlotPOUList2 : _ISlotPOUList
	{
		IEnumerable<Guid> AllTaskObjectGuids { get; }

		IEnumerable<Guid> GetAllTaskSlotPouGuids(Guid guidTask, int nSlot);
	}
}
