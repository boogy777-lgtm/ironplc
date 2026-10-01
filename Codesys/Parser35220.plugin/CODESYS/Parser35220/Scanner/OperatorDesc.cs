using System;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.Parser35220.Scanner
{
	// Token: 0x02000011 RID: 17
	internal class OperatorDesc
	{
		// Token: 0x060001A1 RID: 417 RVA: 0x000089CB File Offset: 0x00006BCB
		public OperatorDesc(Operator op, OperatorFlags flags)
		{
			this.Operator = op;
			this.Flags = flags;
		}

		// Token: 0x1700002F RID: 47
		// (get) Token: 0x060001A2 RID: 418 RVA: 0x000089E1 File Offset: 0x00006BE1
		public Operator Operator { get; }

		// Token: 0x17000030 RID: 48
		// (get) Token: 0x060001A3 RID: 419 RVA: 0x000089E9 File Offset: 0x00006BE9
		public OperatorFlags Flags { get; }
	}
}
