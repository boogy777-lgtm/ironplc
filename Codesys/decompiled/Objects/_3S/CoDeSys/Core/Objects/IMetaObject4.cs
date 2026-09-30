using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x020000E3 RID: 227
	[ReleasedInterface]
	public interface IMetaObject4 : IMetaObject3, IMetaObject2, IMetaObject, IGenericObject, IArchivable, ICloneable, IComparable
	{
		// Token: 0x1700015A RID: 346
		// (get) Token: 0x0600038B RID: 907
		int ModificationCounter { get; }
	}
}
