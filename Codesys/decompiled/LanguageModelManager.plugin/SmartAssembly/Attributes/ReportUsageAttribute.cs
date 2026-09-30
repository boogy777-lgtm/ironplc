using System;

namespace SmartAssembly.Attributes
{
	// Token: 0x0200000D RID: 13
	[AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Method)]
	public class ReportUsageAttribute : Attribute
	{
		// Token: 0x06000012 RID: 18 RVA: 0x00002050 File Offset: 0x00001050
		public ReportUsageAttribute()
		{
		}

		// Token: 0x06000013 RID: 19 RVA: 0x00002050 File Offset: 0x00001050
		public ReportUsageAttribute(string featureName)
		{
		}
	}
}
