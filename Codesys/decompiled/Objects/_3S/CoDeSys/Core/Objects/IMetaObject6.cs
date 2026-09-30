using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x020000E5 RID: 229
	[ReleasedInterface]
	public interface IMetaObject6 : IMetaObject5, IMetaObject4, IMetaObject3, IMetaObject2, IMetaObject, IGenericObject, IArchivable, ICloneable, IComparable
	{
		// Token: 0x1700015C RID: 348
		// (get) Token: 0x0600038D RID: 909
		IIncompleteDeserializationInfo IncompleteDeserializationInfo { get; }
	}
}
