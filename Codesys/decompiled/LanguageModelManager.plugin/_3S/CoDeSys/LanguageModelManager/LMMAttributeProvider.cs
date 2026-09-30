using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.Legacy;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x020000BD RID: 189
	[TypeGuid("{55E31F97-5279-4416-82DB-CAEE0116016F}")]
	public class LMMAttributeProvider : IAttributeProvider
	{
		// Token: 0x06000B72 RID: 2930 RVA: 0x0001CE3A File Offset: 0x0001BE3A
		public LMMAttributeProvider()
		{
			this._legacyAttributeProvider = new LMMAttributeProvider();
		}

		// Token: 0x170002C4 RID: 708
		// (get) Token: 0x06000B73 RID: 2931 RVA: 0x0001CE4D File Offset: 0x0001BE4D
		public IEnumerable<IAttribute> ProvidedAttributes
		{
			get
			{
				if (VersionedCompilerFactory._GranularityCalculator_OrNull == null)
				{
					return this._legacyAttributeProvider.ProvidedAttributes;
				}
				return VersionedCompilerFactory._LMAttributeProvider_OrNull.GetProvidedAttributes();
			}
		}

		// Token: 0x040001E7 RID: 487
		private readonly LMMAttributeProvider _legacyAttributeProvider;
	}
}
