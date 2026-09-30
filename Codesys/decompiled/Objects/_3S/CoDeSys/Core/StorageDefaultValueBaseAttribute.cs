using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core
{
	// Token: 0x0200000A RID: 10
	[ReleasedClass]
	public abstract class StorageDefaultValueBaseAttribute : Attribute
	{
		// Token: 0x0600002C RID: 44
		public abstract bool IsDefaultValue(object value);
	}
}
