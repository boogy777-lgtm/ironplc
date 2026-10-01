using System;
using \u0003;
using \u001C;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u001E
{
	// Token: 0x0200036B RID: 875
	internal sealed class \u0015 : \u001C.\u0012
	{
		// Token: 0x0600342F RID: 13359 RVA: 0x000CD32C File Offset: 0x000CB52C
		public bool \u0001(\u0016 \u0002)
		{
			if (\u0002.signChanges.\u0005)
			{
				_ICompiledPOU u;
				_ICompiledPOU u2;
				if (!\u0002.focContext.\u0002(\u0002.sign, out u, out u2))
				{
					return true;
				}
				\u0002.focContext.\u0001(u2, u);
			}
			return true;
		}
	}
}
