using System;
using System.Collections.Generic;
using \u0004;
using \u0081;

namespace \u0012
{
	// Token: 0x020003A1 RID: 929
	internal sealed class \u0015 : global::\u0004.\u0017
	{
		// Token: 0x060035EC RID: 13804 RVA: 0x000D78BC File Offset: 0x000D5ABC
		internal \u0015(Guid \u0080\u0006)
		{
			this.\u0001 = \u0080\u0006;
		}

		// Token: 0x060035ED RID: 13805 RVA: 0x000D78CC File Offset: 0x000D5ACC
		public string \u0001(string \u0002)
		{
			string arg;
			if (global::\u0012.\u0015.\u0001.TryGetValue(this.\u0001, out arg))
			{
				return string.Format(\u0081.\u0001.Err_NoCodegeneratorDetailed, arg);
			}
			return \u0002;
		}

		// Token: 0x04000A7B RID: 2683
		private static Dictionary<Guid, string> \u0001 = new Dictionary<Guid, string>
		{
			{
				new Guid("{CC6D601F-AB32-4991-AC54-1EE1B3E1C871}"),
				\u0081.\u0001.CodeGenAddon_ARM
			},
			{
				new Guid("{A371AE3C-17DA-4847-AE8B-E9EC561C936C}"),
				\u0081.\u0001.CodeGenAddon_ARM64
			},
			{
				new Guid("{66F31C7B-3DF3-4493-9C8C-546FC2F5FBF6}"),
				\u0081.\u0001.CodeGenAddon_CortexM3
			},
			{
				new Guid("{7661791D-5708-436a-8F06-8069CD7D8FA6}"),
				\u0081.\u0001.CodeGenAddon_MIPS
			},
			{
				new Guid("{F21EA3A3-23D3-4d1b-B90B-A65960403063}"),
				\u0081.\u0001.CodeGenAddon_PowerPC
			},
			{
				new Guid("{210570D9-61E1-4054-970E-41D1A1FC86F2}"),
				\u0081.\u0001.CodeGenAddon_SH
			},
			{
				new Guid("{E08C90CC-88B2-46a3-8363-4B2EC483F27E}"),
				\u0081.\u0001.CodeGenAddon_TriCore
			},
			{
				new Guid("{7F1E9CCC-40DA-460b-8FD1-71D896ED3B54}"),
				\u0081.\u0001.CodeGenAddon_Blackfin
			},
			{
				new Guid("{6CBEEF4A-08EC-4110-828B-14BE8A10332D}"),
				\u0081.\u0001.CodeGenAddon_RX
			},
			{
				new Guid("{D606AFCB-1826-4FE0-B6D4-B9E285176C8D}"),
				\u0081.\u0001.CodeGenAddon_ColdFire
			},
			{
				new Guid("{1B488D71-13AA-43D1-BDA4-7FF0416A1A34}"),
				\u0081.\u0001.CodeGenAddon_LoongArch64
			}
		};

		// Token: 0x04000A7C RID: 2684
		private readonly Guid \u0001;
	}
}
