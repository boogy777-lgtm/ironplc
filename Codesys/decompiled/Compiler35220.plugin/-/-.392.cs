using System;
using System.IO;
using \u0002;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0007
{
	// Token: 0x020003E7 RID: 999
	internal sealed class \u0014 : \u0013
	{
		// Token: 0x0600378A RID: 14218 RVA: 0x000E4588 File Offset: 0x000E2788
		internal \u0014(bool \u008E\u0004, _ICompileContext \u0001\u0002) : base(\u008E\u0004, \u0001\u0002)
		{
		}

		// Token: 0x0600378B RID: 14219 RVA: 0x000E4594 File Offset: 0x000E2794
		public override void \u0001(_ICompiledPOU \u0002, Stream \u0003, bool \u0004, int \u0005, IRelocation \u0006)
		{
			\u0005 = base.\u0001(\u0003, \u0005, \u0006);
			if (!base.DirectRelocator.\u0001(\u0005))
			{
				base.TableRelocator.\u0001(\u0002, \u0003, \u0005, \u0006);
			}
		}

		// Token: 0x0600378C RID: 14220 RVA: 0x000E45C4 File Offset: 0x000E27C4
		public override void \u0001(ICompiledCode4 \u0002)
		{
		}
	}
}
