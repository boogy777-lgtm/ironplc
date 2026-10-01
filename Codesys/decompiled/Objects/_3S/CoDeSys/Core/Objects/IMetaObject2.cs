using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x020000E1 RID: 225
	[ReleasedInterface]
	public interface IMetaObject2 : IMetaObject, IGenericObject, IArchivable, ICloneable, IComparable
	{
		// Token: 0x06000389 RID: 905
		void ReplaceObject(IObject obj);
	}
}
