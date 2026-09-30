using System;
using _3S.CoDeSys.Compiler35220;
using _3S.CoDeSys.Core.Options;

namespace \u0012
{
	// Token: 0x0200000C RID: 12
	internal static class \u0001
	{
		// Token: 0x17000038 RID: 56
		// (get) Token: 0x0600008F RID: 143 RVA: 0x0000285C File Offset: 0x00000A5C
		// (set) Token: 0x06000090 RID: 144 RVA: 0x000028A8 File Offset: 0x00000AA8
		internal static int MaxDegreeOfParallelism
		{
			get
			{
				if (\u0012.\u0001.OptionKey.HasValue("MaxDegreeOfParallelism", typeof(int)))
				{
					return (int)\u0012.\u0001.OptionKey["MaxDegreeOfParallelism"];
				}
				return Math.Max(1, Environment.ProcessorCount / 2 - 1);
			}
			set
			{
				\u0012.\u0001.OptionKey["MaxDegreeOfParallelism"] = value;
			}
		}

		// Token: 0x17000039 RID: 57
		// (get) Token: 0x06000091 RID: 145 RVA: 0x000028C0 File Offset: 0x00000AC0
		private static IOptionKey OptionKey
		{
			get
			{
				return APEnvironmentFacade.Instance.CreateSubKey(OptionRoot.User, \u0012.\u0001.\u0002);
			}
		}

		// Token: 0x04000007 RID: 7
		private const string \u0001 = "MaxDegreeOfParallelism";

		// Token: 0x04000008 RID: 8
		private static readonly string \u0002 = "{24F0D403-55B9-41af-A07F-8C46BDAEEF8E}";
	}
}
