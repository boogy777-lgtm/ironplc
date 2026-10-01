using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x02000032 RID: 50
	[ReleasedInterface]
	public interface IArchiveWriter4 : IArchiveWriter3, IArchiveWriter2, IArchiveWriter
	{
		// Token: 0x17000040 RID: 64
		// (get) Token: 0x060000D6 RID: 214
		// (set) Token: 0x060000D7 RID: 215
		bool ForceNonGenericCollectionSerialization { get; set; }
	}
}
