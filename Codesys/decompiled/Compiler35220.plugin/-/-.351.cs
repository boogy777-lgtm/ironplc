using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using \u0015;
using \u001E;
using _3S.CoDeSys.Compiler35220;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace \u0018
{
	// Token: 0x02000397 RID: 919
	internal sealed class \u0011
	{
		// Token: 0x06003548 RID: 13640 RVA: 0x000D305C File Offset: 0x000D125C
		internal \u0011(_ICompileContext \u0001\u0002, IMessageStorage \u0088\u0005, IMessageCategory \u0089\u0005)
		{
			this.Storage = \u0088\u0005;
			this.Category = \u0089\u0005;
			this.ComCon = \u0001\u0002;
		}

		// Token: 0x170008BA RID: 2234
		// (get) Token: 0x06003549 RID: 13641 RVA: 0x000D3098 File Offset: 0x000D1298
		internal IMessageStorage Storage { get; }

		// Token: 0x170008BB RID: 2235
		// (get) Token: 0x0600354A RID: 13642 RVA: 0x000D30A0 File Offset: 0x000D12A0
		internal IMessageCategory Category { get; }

		// Token: 0x170008BC RID: 2236
		// (get) Token: 0x0600354B RID: 13643 RVA: 0x000D30A8 File Offset: 0x000D12A8
		internal _ICompileContext ComCon { get; }

		// Token: 0x170008BD RID: 2237
		// (get) Token: 0x0600354C RID: 13644 RVA: 0x000D30B0 File Offset: 0x000D12B0
		internal IDictionary<string, int> SummarizedLibErrors { get; } = new LDictionary<string, int>();

		// Token: 0x0600354D RID: 13645 RVA: 0x000D30B8 File Offset: 0x000D12B8
		internal void \u0001(_ICompilerMessage \u0002, _ISignature \u0003)
		{
			this.\u0001(\u0002, \u0003.MessageGuid, \u0003.LibraryPath);
		}

		// Token: 0x0600354E RID: 13646 RVA: 0x000D30D0 File Offset: 0x000D12D0
		internal void \u0001(_ICompilerMessage \u0002, _ICompiledPOU \u0003)
		{
			this.\u0001(\u0002, \u0003.MessageGuid, \u0003.LibraryPath);
		}

		// Token: 0x0600354F RID: 13647 RVA: 0x000D30E8 File Offset: 0x000D12E8
		internal void \u0001(_ICompilerMessage \u0002, Guid \u0003, string \u0004)
		{
			if (\u0002.ObjectGuid == Guid.Empty)
			{
				\u0002.ObjectGuid = \u0003;
			}
			if (\u0002.ProjectHandle == -1)
			{
				\u0002.ProjectHandle = APEnvironmentFacade.Instance.LanguageModelMgr.LibList.GetProjectHandle(\u0004);
			}
		}

		// Token: 0x06003550 RID: 13648 RVA: 0x000D3128 File Offset: 0x000D1328
		internal void \u0001(_ICompilerMessage \u0002, _ISignature \u0003, string \u0004)
		{
			\u0015.\u0004 item = new \u0015.\u0004(\u0002);
			if (this.\u0001.Contains(item))
			{
				return;
			}
			this.\u0001.Add(item);
			new \u0018(\u0002, this, \u0003, \u0004).\u0001();
		}

		// Token: 0x04000A5B RID: 2651
		internal bool \u0001 = true;

		// Token: 0x04000A5C RID: 2652
		internal int \u0001;

		// Token: 0x04000A5D RID: 2653
		internal int \u0002;

		// Token: 0x04000A5E RID: 2654
		internal bool \u0002;

		// Token: 0x04000A5F RID: 2655
		private readonly HashSet<\u0015.\u0004> \u0001 = new HashSet<\u0015.\u0004>();

		// Token: 0x04000A60 RID: 2656
		[CompilerGenerated]
		private readonly IMessageStorage \u0001;

		// Token: 0x04000A61 RID: 2657
		[CompilerGenerated]
		private readonly IMessageCategory \u0001;

		// Token: 0x04000A62 RID: 2658
		[CompilerGenerated]
		private readonly _ICompileContext \u0001;

		// Token: 0x04000A63 RID: 2659
		[CompilerGenerated]
		private readonly IDictionary<string, int> \u0001;
	}
}
