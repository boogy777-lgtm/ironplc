using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core
{
	// Token: 0x0200000E RID: 14
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
	[ReleasedClass]
	public class StorageSaveAsLegacyGenericCollectionAttribute : Attribute
	{
		// Token: 0x06000036 RID: 54 RVA: 0x00002373 File Offset: 0x00000573
		public StorageSaveAsLegacyGenericCollectionAttribute()
		{
			this._vrl = null;
		}

		// Token: 0x06000037 RID: 55 RVA: 0x00002382 File Offset: 0x00000582
		public StorageSaveAsLegacyGenericCollectionAttribute(string stVersionRangeList)
		{
			this._vrl = new VersionRangeList(stVersionRangeList);
		}

		// Token: 0x17000020 RID: 32
		// (get) Token: 0x06000038 RID: 56 RVA: 0x00002396 File Offset: 0x00000596
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

		// Token: 0x06000039 RID: 57 RVA: 0x000023AD File Offset: 0x000005AD
		public bool IsVersionInVersionRangeList(Version version)
		{
			return this._vrl == null || this._vrl.Contains(version);
		}

		// Token: 0x0600003A RID: 58 RVA: 0x000023C8 File Offset: 0x000005C8
		public static StorageSaveAsLegacyGenericCollectionAttribute FromObject(object obj)
		{
			if (obj == null)
			{
				throw new ArgumentNullException("obj");
			}
			object[] customAttributes = obj.GetType().GetCustomAttributes(typeof(StorageSaveAsLegacyGenericCollectionAttribute), false);
			if (customAttributes != null && customAttributes.Length != 0)
			{
				return customAttributes[0] as StorageSaveAsLegacyGenericCollectionAttribute;
			}
			return null;
		}

		// Token: 0x0400000A RID: 10
		private VersionRangeList _vrl;
	}
}
