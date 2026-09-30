using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x02000127 RID: 295
	[ReleasedInterface]
	public interface IFolderObject : IObject, IGenericObject, IArchivable, ICloneable, IComparable
	{
		// Token: 0x170001AC RID: 428
		// (get) Token: 0x06000490 RID: 1168
		// (set) Token: 0x06000491 RID: 1169
		Guid StructuredViewGuid { get; set; }
	}
}
