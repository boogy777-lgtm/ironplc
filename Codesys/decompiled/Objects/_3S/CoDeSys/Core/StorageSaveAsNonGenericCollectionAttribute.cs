using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core
{
	// Token: 0x0200000F RID: 15
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
	[ReleasedClass]
	public class StorageSaveAsNonGenericCollectionAttribute : Attribute
	{
		// Token: 0x0600003B RID: 59 RVA: 0x0000240B File Offset: 0x0000060B
		public StorageSaveAsNonGenericCollectionAttribute()
		{
			this._vrl = null;
		}

		// Token: 0x0600003C RID: 60 RVA: 0x0000241A File Offset: 0x0000061A
		public StorageSaveAsNonGenericCollectionAttribute(string stVersionRangeList)
		{
			this._vrl = new VersionRangeList(stVersionRangeList);
		}

		// Token: 0x17000021 RID: 33
		// (get) Token: 0x0600003D RID: 61 RVA: 0x0000242E File Offset: 0x0000062E
		public string VersionRangeList
		{
			get
			{
				if (this._vrl == null)
				{
					return null;
				}
				return this._vrl.ToString();
			}
		}

		// Token: 0x0600003E RID: 62 RVA: 0x00002445 File Offset: 0x00000645
		public bool IsVersionInVersionRangeList(Version version)
		{
			return this._vrl == null || this._vrl.Contains(version);
		}

		// Token: 0x0600003F RID: 63 RVA: 0x00002460 File Offset: 0x00000660
		public static StorageSaveAsNonGenericCollectionAttribute FromObject(object obj)
		{
			if (obj == null)
			{
				throw new ArgumentNullException("obj");
			}
			object[] customAttributes = obj.GetType().GetCustomAttributes(typeof(StorageSaveAsNonGenericCollectionAttribute), false);
			if (customAttributes != null && customAttributes.Length != 0)
			{
				return customAttributes[0] as StorageSaveAsNonGenericCollectionAttribute;
			}
			return null;
		}

		// Token: 0x0400000B RID: 11
		private VersionRangeList _vrl;
	}
}
