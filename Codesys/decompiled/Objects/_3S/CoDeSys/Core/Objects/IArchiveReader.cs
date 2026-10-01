using System;
using System.IO;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x0200002C RID: 44
	[ReleasedInterface]
	public interface IArchiveReader
	{
		// Token: 0x060000BD RID: 189
		void Initialize(string stFileName);

		// Token: 0x060000BE RID: 190
		void Initialize(Stream stream);

		// Token: 0x060000BF RID: 191
		IArchivable Load();

		// Token: 0x060000C0 RID: 192
		void Fill(IArchivable obj);

		// Token: 0x1700003D RID: 61
		// (get) Token: 0x060000C1 RID: 193
		// (set) Token: 0x060000C2 RID: 194
		bool ThrowIfTypeIsMissing { get; set; }
	}
}
