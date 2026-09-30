using System;
using System.Collections.Generic;
using System.Linq;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler35220.Features
{
	// Token: 0x020001CA RID: 458
	internal static class SignatureExtensions
	{
		// Token: 0x060020AC RID: 8364 RVA: 0x0006F618 File Offset: 0x0006D818
		public static IEnumerable<_IVariable> \u0001(this _ISignature \u0002)
		{
			return \u0002.All.OfType<_IVariable>().Where(new Func<_IVariable, bool>(SignatureExtensions.<>c.<>9.\u0001));
		}

		// Token: 0x060020AD RID: 8365 RVA: 0x0006F64C File Offset: 0x0006D84C
		public static IEnumerable<_IVariable> \u0002(this _ISignature \u0002)
		{
			return \u0002.All.OfType<_IVariable>().Where(new Func<_IVariable, bool>(SignatureExtensions.<>c.<>9.\u0002));
		}
	}
}
