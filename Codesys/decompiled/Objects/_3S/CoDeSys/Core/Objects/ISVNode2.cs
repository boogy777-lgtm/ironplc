using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x02000158 RID: 344
	[ReleasedInterface]
	public interface ISVNode2 : ISVNode, IComparable
	{
		// Token: 0x0600052F RID: 1327
		void AddObject(Guid objectGuid, IObject obj, string stName, int nIndex, out Guid resultObjectGuid);
	}
}
