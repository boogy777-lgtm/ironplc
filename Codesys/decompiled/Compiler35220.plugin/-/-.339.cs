using System;
using System.Collections.Generic;
using \u0018;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using \u0084;

namespace \u001A
{
	// Token: 0x02000382 RID: 898
	internal sealed class \u0015 : \u001E
	{
		// Token: 0x06003492 RID: 13458 RVA: 0x000CF510 File Offset: 0x000CD710
		public bool \u0001(\u0018.\u0010 \u0002)
		{
			if (\u0002.ComconNew == null)
			{
				return true;
			}
			if (\u0002.ComconNew.IsDefined("init_inputs_on_onlchange"))
			{
				return false;
			}
			using (IEnumerator<_ISignature> enumerator = \u0002.ComconNew.AllSignatureList.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.HasAttribute("init_inputs_on_onlchange"))
					{
						return false;
					}
				}
			}
			return !\u0002.ComconNew.IsDefined("no_fast_online_change");
		}
	}
}
