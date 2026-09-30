using System;
using System.Collections;
using System.Collections.Generic;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000134 RID: 308
	public class CaseInsensitiveDictionary<TValue> : LDictionary<string, TValue>, ICaseInsensitiveDictionary<TValue>, IDictionary<string, TValue>, ICollection<KeyValuePair<string, TValue>>, IEnumerable<KeyValuePair<string, TValue>>, IEnumerable
	{
		// Token: 0x06001A9F RID: 6815 RVA: 0x0004BFDF File Offset: 0x0004AFDF
		public CaseInsensitiveDictionary() : base(StringComparer.OrdinalIgnoreCase)
		{
		}

		// Token: 0x06001AA0 RID: 6816 RVA: 0x0004BFEC File Offset: 0x0004AFEC
		public CaseInsensitiveDictionary(int capacity) : base(capacity, StringComparer.OrdinalIgnoreCase)
		{
		}
	}
}
