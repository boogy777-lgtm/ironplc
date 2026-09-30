using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x02000031 RID: 49
	[ReleasedInterface]
	public interface IArchiveWriter3 : IArchiveWriter2, IArchiveWriter
	{
		// Token: 0x1700003E RID: 62
		// (get) Token: 0x060000D3 RID: 211
		bool SupportsCanonicalRepresentation { get; }

		// Token: 0x1700003F RID: 63
		// (get) Token: 0x060000D4 RID: 212
		// (set) Token: 0x060000D5 RID: 213
		bool WriteCanonicalRepresentation { get; set; }
	}
}
