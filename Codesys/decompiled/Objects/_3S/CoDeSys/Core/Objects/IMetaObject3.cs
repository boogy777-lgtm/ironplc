using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x020000E2 RID: 226
	[ReleasedInterface]
	public interface IMetaObject3 : IMetaObject2, IMetaObject, IGenericObject, IArchivable, ICloneable, IComparable
	{
		// Token: 0x17000159 RID: 345
		// (get) Token: 0x0600038A RID: 906
		long TimeStamp { get; }
	}
}
