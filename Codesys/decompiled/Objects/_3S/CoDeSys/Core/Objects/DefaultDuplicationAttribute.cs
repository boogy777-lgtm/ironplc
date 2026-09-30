using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x02000035 RID: 53
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = true)]
	[ReleasedClass]
	public class DefaultDuplicationAttribute : Attribute
	{
		// Token: 0x060000DC RID: 220 RVA: 0x00002B64 File Offset: 0x00000D64
		public DefaultDuplicationAttribute(DuplicationMethod method)
		{
			this._method = method;
		}

		// Token: 0x17000041 RID: 65
		// (get) Token: 0x060000DD RID: 221 RVA: 0x00002B73 File Offset: 0x00000D73
		public DuplicationMethod Method
		{
			get
			{
				return this._method;
			}
		}

		// Token: 0x060000DE RID: 222 RVA: 0x00002B7C File Offset: 0x00000D7C
		public static DefaultDuplicationAttribute FromObject(object obj)
		{
			if (obj == null)
			{
				throw new ArgumentNullException("obj");
			}
			object[] customAttributes = obj.GetType().GetCustomAttributes(typeof(DefaultDuplicationAttribute), false);
			if (customAttributes != null && customAttributes.Length != 0)
			{
				return customAttributes[0] as DefaultDuplicationAttribute;
			}
			return null;
		}

		// Token: 0x04000028 RID: 40
		private DuplicationMethod _method;
	}
}
