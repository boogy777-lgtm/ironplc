using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x02000037 RID: 55
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = true)]
	[ReleasedClass]
	public class DefaultSerializationAttribute : Attribute
	{
		// Token: 0x060000DF RID: 223 RVA: 0x00002BBF File Offset: 0x00000DBF
		public DefaultSerializationAttribute(string stValueName)
		{
			if (stValueName == null)
			{
				throw new ArgumentNullException("stValueName");
			}
			this._stValueName = stValueName;
		}

		// Token: 0x17000042 RID: 66
		// (get) Token: 0x060000E0 RID: 224 RVA: 0x00002BDC File Offset: 0x00000DDC
		public string ValueName
		{
			get
			{
				return this._stValueName;
			}
		}

		// Token: 0x060000E1 RID: 225 RVA: 0x00002BE4 File Offset: 0x00000DE4
		public static DefaultSerializationAttribute FromObject(object obj)
		{
			if (obj == null)
			{
				throw new ArgumentNullException("obj");
			}
			object[] customAttributes = obj.GetType().GetCustomAttributes(typeof(DefaultSerializationAttribute), false);
			if (customAttributes != null && customAttributes.Length != 0)
			{
				return customAttributes[0] as DefaultSerializationAttribute;
			}
			return null;
		}

		// Token: 0x0400002C RID: 44
		private string _stValueName;
	}
}
