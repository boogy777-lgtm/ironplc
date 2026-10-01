using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x0200013A RID: 314
	public enum GenericCollectionSerializationMode
	{
		// Token: 0x0400015A RID: 346
		[ReleasedEnumMember]
		AsGenericCollection,
		// Token: 0x0400015B RID: 347
		[ReleasedEnumMember]
		Default = 0,
		// Token: 0x0400015C RID: 348
		[ReleasedEnumMember]
		AsNonGenericCollection,
		// Token: 0x0400015D RID: 349
		[ReleasedEnumMember]
		AsLegacyGenericCollection,
		// Token: 0x0400015E RID: 350
		[ReleasedEnumMember]
		AsArray
	}
}
