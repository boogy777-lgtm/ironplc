using System;
using System.Collections.Generic;
using \u0018;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using \u0084;

namespace \u0004
{
	// Token: 0x02000384 RID: 900
	internal sealed class \u0016 : \u001E
	{
		// Token: 0x06003496 RID: 13462 RVA: 0x000CF6F0 File Offset: 0x000CD8F0
		public bool \u0001(\u0018.\u0010 \u0002)
		{
			using (IEnumerator<_ISignature> enumerator = \u0002.ComconNew.AllSignatureList.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (!enumerator.Current.GetFlag(SignatureFlag.Located))
					{
						return false;
					}
				}
			}
			return true;
		}
	}
}
