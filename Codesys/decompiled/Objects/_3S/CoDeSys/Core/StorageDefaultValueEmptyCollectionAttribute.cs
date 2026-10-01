using System;
using System.Collections;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core
{
	// Token: 0x0200000B RID: 11
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
	[ReleasedClass]
	public class StorageDefaultValueEmptyCollectionAttribute : StorageDefaultValueBaseAttribute
	{
		// Token: 0x0600002F RID: 47 RVA: 0x000022B4 File Offset: 0x000004B4
		public override bool IsDefaultValue(object value)
		{
			ICollection collection = value as ICollection;
			return collection != null && collection.Count == 0;
		}
	}
}
