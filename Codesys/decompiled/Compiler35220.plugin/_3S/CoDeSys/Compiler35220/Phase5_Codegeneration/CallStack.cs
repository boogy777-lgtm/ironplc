using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.Compiler35220.Phase5_Codegeneration
{
	// Token: 0x02000219 RID: 537
	public sealed class CallStack : IEnumerable<CallStackEntry>, IEnumerable, IStackUsage
	{
		// Token: 0x060023D7 RID: 9175 RVA: 0x0007AA88 File Offset: 0x00078C88
		public CallStack()
		{
			this.\u0001 = new LStack<CallStackEntry>();
		}

		// Token: 0x060023D8 RID: 9176 RVA: 0x0007AA9C File Offset: 0x00078C9C
		public CallStack(_ISignature signCalled, _ISignature signImplemented, int stackSize)
		{
			this.\u0001 = new LStack<CallStackEntry>();
			this.\u0001(signCalled, signImplemented, stackSize);
		}

		// Token: 0x060023D9 RID: 9177 RVA: 0x0007AAB8 File Offset: 0x00078CB8
		private CallStack(IEnumerable<CallStackEntry> inner)
		{
			this.\u0001 = new LStack<CallStackEntry>(inner);
		}

		// Token: 0x1700068E RID: 1678
		// (get) Token: 0x060023DA RID: 9178 RVA: 0x0007AACC File Offset: 0x00078CCC
		public IEnumerable<IStackUsageEntry> Entries
		{
			get
			{
				return this.\u0001.Reverse<CallStackEntry>();
			}
		}

		// Token: 0x1700068F RID: 1679
		// (get) Token: 0x060023DB RID: 9179 RVA: 0x0007AADC File Offset: 0x00078CDC
		public IEnumerable<CallStackEntry> CallStackEntries
		{
			get
			{
				return this.\u0001.Reverse<CallStackEntry>();
			}
		}

		// Token: 0x17000690 RID: 1680
		// (get) Token: 0x060023DC RID: 9180 RVA: 0x0007AAEC File Offset: 0x00078CEC
		// (set) Token: 0x060023DD RID: 9181 RVA: 0x0007AAF4 File Offset: 0x00078CF4
		public int MaxStackSize { get; internal set; }

		// Token: 0x17000691 RID: 1681
		// (get) Token: 0x060023DE RID: 9182 RVA: 0x0007AB00 File Offset: 0x00078D00
		// (set) Token: 0x060023DF RID: 9183 RVA: 0x0007AB08 File Offset: 0x00078D08
		public int MaxStackSizeForExternalCalls { get; internal set; }

		// Token: 0x17000692 RID: 1682
		// (get) Token: 0x060023E0 RID: 9184 RVA: 0x0007AB14 File Offset: 0x00078D14
		// (set) Token: 0x060023E1 RID: 9185 RVA: 0x0007AB1C File Offset: 0x00078D1C
		public IScope Scope { get; internal set; }

		// Token: 0x17000693 RID: 1683
		// (get) Token: 0x060023E2 RID: 9186 RVA: 0x0007AB28 File Offset: 0x00078D28
		// (set) Token: 0x060023E3 RID: 9187 RVA: 0x0007AB30 File Offset: 0x00078D30
		public bool StackOverflow { get; internal set; }

		// Token: 0x060023E4 RID: 9188 RVA: 0x0007AB3C File Offset: 0x00078D3C
		internal void \u0001()
		{
			this.\u0001.Pop();
		}

		// Token: 0x060023E5 RID: 9189 RVA: 0x0007AB4C File Offset: 0x00078D4C
		internal _ISignature \u0001()
		{
			return this.\u0001.Peek().SignImplemented;
		}

		// Token: 0x060023E6 RID: 9190 RVA: 0x0007AB60 File Offset: 0x00078D60
		internal CallStack \u0001()
		{
			return new CallStack(this.\u0001.Reverse<CallStackEntry>());
		}

		// Token: 0x060023E7 RID: 9191 RVA: 0x0007AB74 File Offset: 0x00078D74
		internal void \u0001(_ISignature \u0002, _ISignature \u0003, int \u0004)
		{
			this.\u0001.Push(new CallStackEntry(\u0002, \u0003, \u0004));
		}

		// Token: 0x060023E8 RID: 9192 RVA: 0x0007AB8C File Offset: 0x00078D8C
		internal void \u0001(CallStack \u0002)
		{
			foreach (CallStackEntry callStackEntry in \u0002.\u0001.Reverse<CallStackEntry>())
			{
				this.\u0001.Push(callStackEntry);
			}
		}

		// Token: 0x060023E9 RID: 9193 RVA: 0x0007ABE4 File Offset: 0x00078DE4
		public IEnumerator<CallStackEntry> GetEnumerator()
		{
			return this.\u0001.GetEnumerator();
		}

		// Token: 0x060023EA RID: 9194 RVA: 0x0007ABF4 File Offset: 0x00078DF4
		IEnumerator IEnumerable.\u0001()
		{
			return this.\u0001.GetEnumerator();
		}

		// Token: 0x04000642 RID: 1602
		private readonly LStack<CallStackEntry> \u0001;

		// Token: 0x04000643 RID: 1603
		[CompilerGenerated]
		private int \u0001;

		// Token: 0x04000644 RID: 1604
		[CompilerGenerated]
		private int \u0002;

		// Token: 0x04000645 RID: 1605
		[CompilerGenerated]
		private IScope \u0001;

		// Token: 0x04000646 RID: 1606
		[CompilerGenerated]
		private bool \u0001;
	}
}
