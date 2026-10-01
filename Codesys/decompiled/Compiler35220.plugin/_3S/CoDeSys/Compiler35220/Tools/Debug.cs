using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace _3S.CoDeSys.Compiler35220.Tools
{
	// Token: 0x02000059 RID: 89
	public class Debug
	{
		// Token: 0x0600066D RID: 1645 RVA: 0x0000DB34 File Offset: 0x0000BD34
		internal static void \u0001(bool \u0002)
		{
			if (!\u0002)
			{
				Debug.\u0001(string.Empty);
			}
		}

		// Token: 0x0600066E RID: 1646 RVA: 0x0000DB44 File Offset: 0x0000BD44
		internal static void \u0001(bool \u0002, string \u0003)
		{
			if (!\u0002)
			{
				Debug.\u0001(\u0003);
			}
		}

		// Token: 0x0600066F RID: 1647 RVA: 0x0000DB50 File Offset: 0x0000BD50
		internal static void \u0002(bool \u0002)
		{
			if (!\u0002)
			{
				throw new InvalidStateCompilerException();
			}
		}

		// Token: 0x06000670 RID: 1648 RVA: 0x0000DB5C File Offset: 0x0000BD5C
		internal static void \u0003(bool \u0002)
		{
			if (\u0002)
			{
				throw new LateCompileErrorException();
			}
		}

		// Token: 0x06000671 RID: 1649 RVA: 0x0000DB68 File Offset: 0x0000BD68
		internal static void \u0004(bool \u0002)
		{
			if (\u0002)
			{
				throw new FatalCompileErrorException();
			}
		}

		// Token: 0x17000356 RID: 854
		// (get) Token: 0x06000672 RID: 1650 RVA: 0x0000DB74 File Offset: 0x0000BD74
		// (set) Token: 0x06000673 RID: 1651 RVA: 0x0000DB7C File Offset: 0x0000BD7C
		public static Debug Singleton { get; set; } = new Debug();

		// Token: 0x06000674 RID: 1652 RVA: 0x0000DB84 File Offset: 0x0000BD84
		internal static void \u0001(string \u0002)
		{
			Debug.Singleton.FailMockable(\u0002);
		}

		// Token: 0x06000675 RID: 1653 RVA: 0x0000DB94 File Offset: 0x0000BD94
		public virtual void FailMockable(string text)
		{
			if (Debugger.IsAttached)
			{
				Debugger.Break();
			}
		}

		// Token: 0x06000676 RID: 1654 RVA: 0x0000DBA4 File Offset: 0x0000BDA4
		internal static void \u0002(string \u0002)
		{
		}

		// Token: 0x040000B2 RID: 178
		[CompilerGenerated]
		private static Debug \u0001;
	}
}
