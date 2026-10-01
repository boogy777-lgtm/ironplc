using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x0200014D RID: 333
	[ReleasedInterface]
	public interface IObject2 : IObject, IGenericObject, IArchivable, ICloneable, IComparable
	{
		// Token: 0x06000505 RID: 1285
		bool AcceptsTopLevel(int nProjectHandle);
	}
}
