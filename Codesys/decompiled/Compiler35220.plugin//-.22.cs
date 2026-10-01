using System;
using \u001C;
using _3S.CoDeSys.Compiler35220.Phase1_Typification;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0084
{
	// Token: 0x02000312 RID: 786
	internal sealed class \u001B
	{
		// Token: 0x06002F5A RID: 12122 RVA: 0x000B234C File Offset: 0x000B054C
		internal \u001B(\u0011 \u0002\u0008)
		{
			this.\u0001 = \u0002\u0008;
		}

		// Token: 0x06002F5B RID: 12123 RVA: 0x000B235C File Offset: 0x000B055C
		internal bool \u0001(ICompiledType \u0002, ICompiledType \u0003, ICommonScope \u0004)
		{
			string a = NameManglingService.\u0001(\u0002, this.\u0001, \u0004);
			string b = NameManglingService.\u0001(\u0003, this.\u0001, \u0004);
			return a == b;
		}

		// Token: 0x04000902 RID: 2306
		private readonly \u0011 \u0001;
	}
}
