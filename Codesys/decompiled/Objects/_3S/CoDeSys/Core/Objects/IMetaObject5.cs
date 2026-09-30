using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x020000E4 RID: 228
	[ReleasedInterface]
	public interface IMetaObject5 : IMetaObject4, IMetaObject3, IMetaObject2, IMetaObject, IGenericObject, IArchivable, ICloneable, IComparable
	{
		// Token: 0x1700015B RID: 347
		// (get) Token: 0x0600038C RID: 908
		bool DeserializedIncompletely { get; }
	}
}
