using System;
using System.Diagnostics;

namespace CODESYS.Parser35220.Tools
{
	// Token: 0x0200000C RID: 12
	public class Debug
	{
		// Token: 0x06000059 RID: 89 RVA: 0x00003022 File Offset: 0x00001222
		internal static void Assert(bool bAssert)
		{
			if (!bAssert)
			{
				Debug.Fail(string.Empty);
			}
		}

		// Token: 0x1700001A RID: 26
		// (get) Token: 0x0600005A RID: 90 RVA: 0x00003031 File Offset: 0x00001231
		// (set) Token: 0x0600005B RID: 91 RVA: 0x00003038 File Offset: 0x00001238
		public static Debug Singleton { get; set; } = new Debug();

		// Token: 0x0600005C RID: 92 RVA: 0x00003040 File Offset: 0x00001240
		internal static void Fail(string text)
		{
			Debug.Singleton.FailMockable(text);
		}

		// Token: 0x0600005D RID: 93 RVA: 0x0000304D File Offset: 0x0000124D
		public virtual void FailMockable(string text)
		{
			if (Debugger.IsAttached)
			{
				Debugger.Break();
			}
		}
	}
}
