using System;
using System.Collections.Generic;
using _3S.CoDeSys.Compiler.Serialization;

namespace _3S.CoDeSys.LanguageModelManager.Features
{
	// Token: 0x02000283 RID: 643
	public class TextualPreCompCrossReferenceSerializable : ITextualPreCompCrossReferenceSerializable
	{
		// Token: 0x06002AEF RID: 10991 RVA: 0x0007246C File Offset: 0x0007146C
		internal TextualPreCompCrossReferenceSerializable(string name, IDictionary<int, ICollection<int>> accObjByMdgGuid)
		{
			this.Name = name;
			this.AccessingObjectsByMessageGuid = accObjByMdgGuid;
		}

		// Token: 0x17000BF7 RID: 3063
		// (get) Token: 0x06002AF0 RID: 10992 RVA: 0x00072482 File Offset: 0x00071482
		public string Name { get; }

		// Token: 0x17000BF8 RID: 3064
		// (get) Token: 0x06002AF1 RID: 10993 RVA: 0x0007248A File Offset: 0x0007148A
		public IDictionary<int, ICollection<int>> AccessingObjectsByMessageGuid { get; }
	}
}
