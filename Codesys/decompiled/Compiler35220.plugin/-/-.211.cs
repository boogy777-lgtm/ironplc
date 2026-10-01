using System;
using System.Linq;
using System.Runtime.CompilerServices;
using \u0006;
using \u0013;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u001A
{
	// Token: 0x0200024A RID: 586
	internal sealed class \u000F : global::\u0006.\u0002
	{
		// Token: 0x17000704 RID: 1796
		// (get) Token: 0x06002662 RID: 9826 RVA: 0x00086150 File Offset: 0x00084350
		// (set) Token: 0x06002663 RID: 9827 RVA: 0x00086158 File Offset: 0x00084358
		private int CurrentIndex { get; set; }

		// Token: 0x17000705 RID: 1797
		// (get) Token: 0x06002664 RID: 9828 RVA: 0x00086164 File Offset: 0x00084364
		private _ICompiledPOU POU { get; }

		// Token: 0x06002665 RID: 9829 RVA: 0x0008616C File Offset: 0x0008436C
		private \u000F(_ICompiledPOU \u0012\u0002)
		{
			this.POU = \u0012\u0002;
		}

		// Token: 0x06002666 RID: 9830 RVA: 0x0008617C File Offset: 0x0008437C
		public override void \u0001(_ITryCatchStatement \u0002)
		{
			base.\u0001(\u0002);
			Debug.\u0001(this.POU.TryCatchFPAddresses != null && this.POU.TryCatchFPAddresses.Count<IDataLocation>() > this.CurrentIndex);
			\u0002.Index = this.CurrentIndex;
			int num = this.CurrentIndex;
			this.CurrentIndex = num + 1;
		}

		// Token: 0x06002667 RID: 9831 RVA: 0x000861DC File Offset: 0x000843DC
		public static void \u0001(_IExprement \u0002, _ICompiledPOU \u0003)
		{
			if (\u0003.GetFlagInternal(InternalCompiledPOUFlags.ContainsTryCatch) && \u0003.TryCatchFPAddresses != null)
			{
				\u001A.\u000F u000F = new \u001A.\u000F(\u0003);
				IStatementTraverser ivisit = new \u0013.\u0001(u000F);
				\u0002.Accept(ivisit);
				Debug.\u0001(\u0003.TryCatchFPAddresses != null && u000F.CurrentIndex == \u0003.TryCatchFPAddresses.Count<IDataLocation>());
			}
		}

		// Token: 0x04000708 RID: 1800
		[CompilerGenerated]
		private new int \u0001;

		// Token: 0x04000709 RID: 1801
		[CompilerGenerated]
		private new readonly _ICompiledPOU \u0001;
	}
}
