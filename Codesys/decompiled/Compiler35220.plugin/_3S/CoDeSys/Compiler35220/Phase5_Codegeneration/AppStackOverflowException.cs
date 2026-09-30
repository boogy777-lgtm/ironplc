using System;
using System.Runtime.Serialization;

namespace _3S.CoDeSys.Compiler35220.Phase5_Codegeneration
{
	// Token: 0x02000224 RID: 548
	[Serializable]
	public class AppStackOverflowException : ApplicationException
	{
		// Token: 0x0600243E RID: 9278 RVA: 0x0007C63C File Offset: 0x0007A83C
		public AppStackOverflowException()
		{
		}

		// Token: 0x0600243F RID: 9279 RVA: 0x0007C644 File Offset: 0x0007A844
		internal AppStackOverflowException(int nCalculatedSize, CallStack callStack)
		{
			this.CalculatedSize = nCalculatedSize;
			this.CallStack = callStack;
		}

		// Token: 0x06002440 RID: 9280 RVA: 0x0007C65C File Offset: 0x0007A85C
		protected AppStackOverflowException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		// Token: 0x1700069E RID: 1694
		// (get) Token: 0x06002441 RID: 9281 RVA: 0x0007C668 File Offset: 0x0007A868
		// (set) Token: 0x06002442 RID: 9282 RVA: 0x0007C670 File Offset: 0x0007A870
		internal int CalculatedSize { get; set; }

		// Token: 0x1700069F RID: 1695
		// (get) Token: 0x06002443 RID: 9283 RVA: 0x0007C67C File Offset: 0x0007A87C
		// (set) Token: 0x06002444 RID: 9284 RVA: 0x0007C684 File Offset: 0x0007A884
		internal CallStack CallStack { get; set; }
	}
}
