using System;
using System.Collections.Generic;
using System.Linq;
using _3S.CoDeSys.Compiler.Serialization;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager.Features
{
	// Token: 0x02000282 RID: 642
	public class TextualPreCompCrossReferencesSerializable : ITextualPreCompCrossReferencesSerializable
	{
		// Token: 0x06002AEC RID: 10988 RVA: 0x000723E4 File Offset: 0x000713E4
		public TextualPreCompCrossReferencesSerializable(SharedGuidTable shGdTbl, IEnumerable<ITextualPreCompCrossReferenceSerializable> crossRefs)
		{
			this.SharedGuidTable = Enumerable.ToReadonlyList<Guid>(Enumerable.ToLList<Guid>(shGdTbl.Table.OrderBy(delegate(KeyValuePair<Guid, int> entry)
			{
				KeyValuePair<Guid, int> keyValuePair = entry;
				return keyValuePair.Value;
			}).Select(delegate(KeyValuePair<Guid, int> entry)
			{
				KeyValuePair<Guid, int> keyValuePair = entry;
				return keyValuePair.Key;
			})));
			this.CrossReferences = crossRefs;
		}

		// Token: 0x17000BF5 RID: 3061
		// (get) Token: 0x06002AED RID: 10989 RVA: 0x0007245C File Offset: 0x0007145C
		public IList<Guid> SharedGuidTable { get; }

		// Token: 0x17000BF6 RID: 3062
		// (get) Token: 0x06002AEE RID: 10990 RVA: 0x00072464 File Offset: 0x00071464
		public IEnumerable<ITextualPreCompCrossReferenceSerializable> CrossReferences { get; }
	}
}
