using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x02000146 RID: 326
	[ReleasedInterface]
	public interface IHiddenObjectAdorner
	{
		// Token: 0x060004E7 RID: 1255
		bool ShouldBeHidden(IObject obj);

		// Token: 0x060004E8 RID: 1256
		bool ShouldBeHidden(IMetaObjectStub mos);

		// Token: 0x060004E9 RID: 1257
		bool ShouldBeHidden(int projectHandle, Guid objectGuid);
	}
}
