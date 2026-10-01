using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x02000129 RID: 297
	[ReleasedInterface]
	public interface IPastedObject2 : IPastedObject
	{
		// Token: 0x0600049D RID: 1181
		void ChangeName(string stNewName);

		// Token: 0x170001B5 RID: 437
		// (get) Token: 0x0600049E RID: 1182
		Guid SrcParentSVNodeGuid { get; }

		// Token: 0x170001B6 RID: 438
		// (get) Token: 0x0600049F RID: 1183
		int SrcChildIndex { get; }

		// Token: 0x060004A0 RID: 1184
		void RemoveProperty(Guid gdProperty);
	}
}
