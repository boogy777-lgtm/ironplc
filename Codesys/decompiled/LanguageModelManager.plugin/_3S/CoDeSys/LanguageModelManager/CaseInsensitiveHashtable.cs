using System;
using System.Collections;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000133 RID: 307
	[TypeGuid("{73ce5f25-d76d-4bc9-a6d5-0f1336fc56e3}")]
	[StorageVersion("3.3.0.0")]
	public class CaseInsensitiveHashtable : Hashtable, ICaseInsensitiveHashtable, IDictionary, ICollection, IEnumerable, ICloneable
	{
		// Token: 0x06001A9D RID: 6813 RVA: 0x0004BFC4 File Offset: 0x0004AFC4
		public CaseInsensitiveHashtable() : base(StringComparer.OrdinalIgnoreCase)
		{
		}

		// Token: 0x06001A9E RID: 6814 RVA: 0x0004BFD1 File Offset: 0x0004AFD1
		public CaseInsensitiveHashtable(int capacity) : base(capacity, StringComparer.OrdinalIgnoreCase)
		{
		}
	}
}
