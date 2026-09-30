using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core
{
	// Token: 0x02000009 RID: 9
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
	[ReleasedClass]
	public class StorageDefaultValueAttribute : StorageDefaultValueBaseAttribute
	{
		// Token: 0x06000029 RID: 41 RVA: 0x0000227F File Offset: 0x0000047F
		public StorageDefaultValueAttribute(object defaultValue)
		{
			this._defaultValue = defaultValue;
		}

		// Token: 0x1700001E RID: 30
		// (get) Token: 0x0600002A RID: 42 RVA: 0x0000228E File Offset: 0x0000048E
		public object DefaultValue
		{
			get
			{
				return this._defaultValue;
			}
		}

		// Token: 0x0600002B RID: 43 RVA: 0x00002296 File Offset: 0x00000496
		public override bool IsDefaultValue(object value)
		{
			return object.Equals(value, this._defaultValue);
		}

		// Token: 0x04000008 RID: 8
		private object _defaultValue;
	}
}
