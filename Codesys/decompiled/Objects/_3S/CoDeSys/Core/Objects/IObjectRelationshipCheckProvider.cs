using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x020000F7 RID: 247
	[ReleasedInterface]
	public interface IObjectRelationshipCheckProvider
	{
		// Token: 0x060003E4 RID: 996
		void CheckParentObjectAcceptance(int projectHandle, Type childObjectType, Type[] embeddedChildObjectTypes, IObject parentObject, int index, out bool handled, out bool acceptable);

		// Token: 0x060003E5 RID: 997
		void CheckParentObjectAcceptance(int projectHandle, IObject obj, IObject parentObject, out bool handled, out bool acceptable);

		// Token: 0x060003E6 RID: 998
		void CheckRelationships(IObject obj, IObject parentObject, IObject[] childObjects, out bool handled, out int result);
	}
}
