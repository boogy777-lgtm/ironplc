using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace \u0017
{
	// Token: 0x02000408 RID: 1032
	[CompilerGenerated]
	internal sealed class \u0019
	{
		// Token: 0x06003968 RID: 14696 RVA: 0x000EDC9C File Offset: 0x000EBE9C
		internal static uint \u0001(string \u0002)
		{
			uint num;
			if (\u0002 != null)
			{
				num = 2166136261U;
				for (int i = 0; i < \u0002.Length; i++)
				{
					num = ((uint)\u0002[i] ^ num) * 16777619U;
				}
			}
			return num;
		}

		// Token: 0x04000B72 RID: 2930 RVA: 0x000EDCDC File Offset: 0x000EBEDC
		internal static readonly \u0019.\u0002 \u0001;

		// Token: 0x04000B73 RID: 2931 RVA: 0x000EDCF0 File Offset: 0x000EBEF0
		internal static readonly \u0019.\u0003 \u0001;

		// Token: 0x04000B74 RID: 2932 RVA: 0x000EDD08 File Offset: 0x000EBF08
		internal static readonly long \u0001;

		// Token: 0x04000B75 RID: 2933 RVA: 0x000EDD10 File Offset: 0x000EBF10
		internal static readonly \u0019.\u0004 \u0001;

		// Token: 0x04000B76 RID: 2934 RVA: 0x000EDD30 File Offset: 0x000EBF30
		internal static readonly \u0019.\u0005 \u0001;

		// Token: 0x04000B77 RID: 2935 RVA: 0x000EE0AC File Offset: 0x000EC2AC
		internal static readonly \u0019.\u0001 \u0001;

		// Token: 0x04000B78 RID: 2936 RVA: 0x000EE0B8 File Offset: 0x000EC2B8
		internal static readonly \u0019.\u0003 \u0002;

		// Token: 0x04000B79 RID: 2937 RVA: 0x000EE0D0 File Offset: 0x000EC2D0
		internal static readonly \u0019.\u0006 \u0001;

		// Token: 0x04000B7A RID: 2938 RVA: 0x000EE460 File Offset: 0x000EC660
		internal static readonly \u0019.\u0002 \u0002;

		// Token: 0x04000B7B RID: 2939 RVA: 0x000EE474 File Offset: 0x000EC674
		internal static readonly \u0019.\u0001 \u0002;

		// Token: 0x02000409 RID: 1033
		[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 12)]
		private struct \u0001
		{
		}

		// Token: 0x0200040A RID: 1034
		[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 20)]
		private struct \u0002
		{
		}

		// Token: 0x0200040B RID: 1035
		[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 24)]
		private struct \u0003
		{
		}

		// Token: 0x0200040C RID: 1036
		[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 32)]
		private struct \u0004
		{
		}

		// Token: 0x0200040D RID: 1037
		[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 892)]
		private struct \u0005
		{
		}

		// Token: 0x0200040E RID: 1038
		[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 912)]
		private struct \u0006
		{
		}
	}
}
