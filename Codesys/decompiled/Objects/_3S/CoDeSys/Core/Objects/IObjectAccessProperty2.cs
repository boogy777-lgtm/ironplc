using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x020000F2 RID: 242
	[ReleasedInterface]
	public interface IObjectAccessProperty2 : IObjectAccessProperty, IObjectProperty, IGenericObject, IArchivable, ICloneable, IComparable
	{
		// Token: 0x060003B3 RID: 947
		void MergeGroups(int projectHandle, Guid objectGuid);
	}
}
