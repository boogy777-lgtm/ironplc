using System;
using System.Linq;
using \u0001;
using \u0003;
using \u0004;
using \u000F;
using \u0012;
using \u001C;
using _3S.CoDeSys.Compiler35220;
using _3S.CoDeSys.Compiler35220.Services.UpToDateChecks;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using \u007F;
using \u0080;
using \u0083;
using \u0084;

namespace \u0014
{
	// Token: 0x02000127 RID: 295
	internal sealed class \u0004 : IUpToDateChecker
	{
		// Token: 0x060014E9 RID: 5353 RVA: 0x0003D6FC File Offset: 0x0003B8FC
		public bool \u0001(_ICompileContext \u0002, _IPreCompileContext \u0003, _IPreCompileContext \u0004, _IIsUpTopDateStrategy \u0005)
		{
			global::\u0014.\u0004.\u0001();
			global::\u0014.\u0003 u = new global::\u0014.\u0003(\u0002, \u0003, \u0004, \u0005);
			global::\u000F.\u0006[] array = new global::\u000F.\u0006[]
			{
				\u0080.\u0006.Instance,
				\u001C.\u0006.Instance,
				global::\u0001.\u0005.Instance,
				global::\u0003.\u0004.Instance,
				global::\u0012.\u0008.Instance,
				GlobalErrorChangeChecker.Instance,
				ChangedSignatureChecker.Instance,
				\u007F.\u0003.Instance,
				global::\u0004.\u0002.Instance,
				\u0084.\u0006.Instance,
				\u0083.\u0002.Instance
			};
			bool flag = true;
			global::\u000F.\u0006[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				flag = array2[i].\u0001(u);
				if (!flag)
				{
					break;
				}
			}
			\u0005.CheckGlobalOnlineChangePreConditions();
			return flag;
		}

		// Token: 0x060014EA RID: 5354 RVA: 0x0003D7A0 File Offset: 0x0003B9A0
		public _IIsUpTopDateStrategy \u0001()
		{
			return new FastIsUpToDateStrategy();
		}

		// Token: 0x060014EB RID: 5355 RVA: 0x0003D7A8 File Offset: 0x0003B9A8
		public _IIsUpTopDateStrategy \u0001(_ICompileContext \u0002, _IPreCompileContext \u0003, bool \u0004)
		{
			return new DetailedIsUpToDateStrategy(\u0002, \u0003, \u0004);
		}

		// Token: 0x060014EC RID: 5356 RVA: 0x0003D7B4 File Offset: 0x0003B9B4
		private static void \u0001()
		{
			foreach (_IPreCompileContext ipreCompileContext in APEnvironmentFacade.Instance.LanguageModelMgr._AllPreCompileContexts(true, true).OfType<_IPreCompileContext>())
			{
				ipreCompileContext.RemoveTimeStampOnlyObjects();
			}
		}

		// Token: 0x060014ED RID: 5357 RVA: 0x0003D810 File Offset: 0x0003BA10
		internal static bool \u0001(_ISignature \u0002, _ISignature \u0003)
		{
			return \u0002 == null || (\u0002.Checksum != 0U && \u0002.Checksum != \u0003.Checksum);
		}
	}
}
