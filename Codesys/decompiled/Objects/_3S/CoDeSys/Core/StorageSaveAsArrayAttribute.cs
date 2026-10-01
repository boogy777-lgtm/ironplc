using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core
{
	// Token: 0x0200000D RID: 13
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
	[ReleasedClass]
	public class StorageSaveAsArrayAttribute : Attribute
	{
		// Token: 0x06000031 RID: 49 RVA: 0x000022DE File Offset: 0x000004DE
		public StorageSaveAsArrayAttribute()
		{
			this._vrl = null;
		}

		// Token: 0x06000032 RID: 50 RVA: 0x000022ED File Offset: 0x000004ED
		public StorageSaveAsArrayAttribute(string stVersionRangeList)
		{
			this._vrl = new VersionRangeList(stVersionRangeList);
		}

		// Token: 0x1700001F RID: 31
		// (get) Token: 0x06000033 RID: 51 RVA: 0x00002301 File Offset: 0x00000501
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

		// Token: 0x06000034 RID: 52 RVA: 0x00002318 File Offset: 0x00000518
		public bool IsVersionInVersionRangeList(Version version)
		{
			return this._vrl == null || this._vrl.Contains(version);
		}

		// Token: 0x06000035 RID: 53 RVA: 0x00002330 File Offset: 0x00000530
		public static StorageSaveAsArrayAttribute FromObject(object obj)
		{
			if (obj == null)
			{
				throw new ArgumentNullException("obj");
			}
			object[] customAttributes = obj.GetType().GetCustomAttributes(typeof(StorageSaveAsArrayAttribute), false);
			if (customAttributes != null && customAttributes.Length != 0)
			{
				return customAttributes[0] as StorageSaveAsArrayAttribute;
			}
			return null;
		}

		// Token: 0x04000009 RID: 9
		private VersionRangeList _vrl;
	}
}
