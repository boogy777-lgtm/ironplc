using System;
using \u0011;

namespace \u001F
{
	// Token: 0x0200014F RID: 335
	internal static class \u0006
	{
		// Token: 0x06001784 RID: 6020 RVA: 0x000486B8 File Offset: 0x000468B8
		public static int \u0001(\u0005 \u0002, \u0005 \u0003)
		{
			int num = Array.IndexOf<\u0005>(\u0006.\u0001, \u0002);
			if (num < 0)
			{
				num = Array.IndexOf<\u0005>(\u0006.\u0002, \u0002);
			}
			int num2 = Array.IndexOf<\u0005>(\u0006.\u0001, \u0003);
			if (num2 < 0)
			{
				num2 = Array.IndexOf<\u0005>(\u0006.\u0002, \u0003);
			}
			return num.CompareTo(num2);
		}

		// Token: 0x06001785 RID: 6021 RVA: 0x00048708 File Offset: 0x00046908
		public static bool \u0001(\u0005 \u0002, \u0005 \u0003)
		{
			return \u0002 != \u0003 && \u0006.\u0001(\u0002, \u0003) < 0;
		}

		// Token: 0x04000423 RID: 1059
		private static \u0005[] \u0001 = new \u0005[]
		{
			\u0005.\u0001,
			\u0005.\u0003,
			\u0005.\u0002,
			\u0005.\u0004,
			\u0005.\u0005
		};

		// Token: 0x04000424 RID: 1060
		private static \u0005[] \u0002 = new \u0005[]
		{
			\u0005.\u0006,
			\u0005.\u0008,
			\u0005.\u0007,
			\u0005.\u000E,
			\u0005.\u000F
		};
	}
}
