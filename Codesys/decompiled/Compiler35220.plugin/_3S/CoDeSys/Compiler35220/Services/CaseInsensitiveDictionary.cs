using System;
using System.Collections;
using System.Collections.Generic;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.Compiler35220.Services
{
	// Token: 0x020000F1 RID: 241
	public class CaseInsensitiveDictionary<TValue> : LDictionary<string, TValue>, ICaseInsensitiveDictionary<TValue>, IDictionary<string, TValue>, ICollection<KeyValuePair<string, TValue>>, IEnumerable<KeyValuePair<string, TValue>>, IEnumerable
	{
		// Token: 0x0600104A RID: 4170 RVA: 0x0002E244 File Offset: 0x0002C444
		public CaseInsensitiveDictionary() : base(StringComparer.OrdinalIgnoreCase)
		{
		}
	}
}
