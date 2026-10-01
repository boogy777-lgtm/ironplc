using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using \u000E;
using \u001E;
using _3S.CoDeSys.Compiler35220.Serialization;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0003
{
	// Token: 0x02000097 RID: 151
	internal sealed class \u0001 : global::\u000E.\u0002
	{
		// Token: 0x17000426 RID: 1062
		// (get) Token: 0x06000CD2 RID: 3282 RVA: 0x00020A24 File Offset: 0x0001EC24
		// (set) Token: 0x06000CD3 RID: 3283 RVA: 0x00020A2C File Offset: 0x0001EC2C
		private IList<_IExprement> ExprementTable { get; set; }

		// Token: 0x06000CD4 RID: 3284 RVA: 0x00020A38 File Offset: 0x0001EC38
		protected \u0001(BinaryReader \u009E\u0002, ITreeFactory \u0003\u0003, IList<_IExprement> \u0004\u0003) : base(\u009E\u0002, \u0003\u0003)
		{
			this.ExprementTable = \u0004\u0003;
		}

		// Token: 0x06000CD5 RID: 3285 RVA: 0x00020A4C File Offset: 0x0001EC4C
		public new static _IExprement \u0001(BinaryReader \u0002, ITreeFactory \u0003, IList<_IExprement> \u0004)
		{
			return new global::\u0003.\u0001(\u0002, \u0003, \u0004).\u0001();
		}

		// Token: 0x06000CD6 RID: 3286 RVA: 0x00020A5C File Offset: 0x0001EC5C
		public new static void \u0001(BinaryReader \u0002, ITreeFactory \u0003, IList<_IExprement> \u0004, out _IExprement \u0005, out ICompactedParseTreeInformation \u0006)
		{
			global::\u0003.\u0001 u = new global::\u0003.\u0001(\u0002, \u0003, \u0004);
			\u0005 = u.\u0001();
			\u0006 = u.\u0001(\u0002);
		}

		// Token: 0x06000CD7 RID: 3287 RVA: 0x00020A84 File Offset: 0x0001EC84
		public new static void \u0001(BinaryReader \u0002, ITreeFactory \u0003, IList<_IExprement> \u0004)
		{
			\u001E.\u0003 u = new \u001E.\u0003(\u0002, \u0003);
			int num = \u0002.ReadInt32();
			for (int i = 0; i < num; i++)
			{
				_IExprement item = u.\u0001<_IExprement>(\u0002);
				\u0004.Add(item);
			}
		}

		// Token: 0x06000CD8 RID: 3288 RVA: 0x00020ABC File Offset: 0x0001ECBC
		public override _IExprement \u0001(ExprementTag \u0002)
		{
			if (\u0002 == ExprementTag.TableContent)
			{
				int num = base.Reader.ReadInt32();
				Debug.\u0001(num >= 0 && num < this.ExprementTable.Count);
				return this.ExprementTable[num];
			}
			return base.\u0001(\u0002);
		}

		// Token: 0x0400022B RID: 555
		[CompilerGenerated]
		private new IList<_IExprement> \u0001;
	}
}
