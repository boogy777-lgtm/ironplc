using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core
{
	// Token: 0x02000010 RID: 16
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Interface, Inherited = false, AllowMultiple = false)]
	[ReleasedClass]
	public class StorageVersionAttribute : Attribute
	{
		// Token: 0x06000040 RID: 64 RVA: 0x000024A3 File Offset: 0x000006A3
		public StorageVersionAttribute(string stVersionRangeList)
		{
			this._vrl = new VersionRangeList(stVersionRangeList);
		}

		// Token: 0x17000022 RID: 34
		// (get) Token: 0x06000041 RID: 65 RVA: 0x000024B7 File Offset: 0x000006B7
		public string VersionRangeList
		{
			get
			{
				return this._vrl.ToString();
			}
		}

		// Token: 0x06000042 RID: 66 RVA: 0x000024C4 File Offset: 0x000006C4
		public bool IsVersionInVersionRangeList(Version version)
		{
			return this._vrl.Contains(version);
		}

		// Token: 0x06000043 RID: 67 RVA: 0x000024D4 File Offset: 0x000006D4
		public static StorageVersionAttribute FromObject(object obj)
		{
			if (obj == null)
			{
				throw new ArgumentNullException("obj");
			}
			object[] customAttributes = obj.GetType().GetCustomAttributes(typeof(StorageVersionAttribute), false);
			if (customAttributes != null && customAttributes.Length != 0)
			{
				return customAttributes[0] as StorageVersionAttribute;
			}
			return null;
		}

		// Token: 0x0400000C RID: 12
		private VersionRangeList _vrl;
	}
}
