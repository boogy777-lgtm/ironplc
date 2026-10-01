using System;
using System.Collections.Generic;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000126 RID: 294
	internal class RelatedObjectTable : ILMRelatedObjectTable
	{
		// Token: 0x1700061A RID: 1562
		// (get) Token: 0x06001912 RID: 6418 RVA: 0x00048A2B File Offset: 0x00047A2B
		public IEnumerable<Guid> Keys
		{
			get
			{
				return this.Dictionary.Keys;
			}
		}

		// Token: 0x06001913 RID: 6419 RVA: 0x00048A38 File Offset: 0x00047A38
		public void Add(Guid key, IEnumerable<Guid> values)
		{
			LDictionary<Guid, Guid> ldictionary;
			if (!this.Dictionary.TryGetValue(key, ref ldictionary))
			{
				ldictionary = new LDictionary<Guid, Guid>();
				this.Dictionary[key] = ldictionary;
			}
			foreach (Guid guid in values)
			{
				ldictionary[guid] = guid;
			}
		}

		// Token: 0x06001914 RID: 6420 RVA: 0x00048AA4 File Offset: 0x00047AA4
		public IEnumerable<Guid> GetValues(Guid key)
		{
			return this.Dictionary[key].Values;
		}

		// Token: 0x0400052D RID: 1325
		public LDictionary<Guid, LDictionary<Guid, Guid>> Dictionary = new LDictionary<Guid, LDictionary<Guid, Guid>>();
	}
}
