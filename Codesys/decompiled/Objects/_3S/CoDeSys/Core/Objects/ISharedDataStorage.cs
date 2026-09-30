using System;
using System.IO;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x02000033 RID: 51
	[ReleasedInterface]
	public interface ISharedDataStorage
	{
		// Token: 0x060000D8 RID: 216
		void Save(Stream stringTableStream, Stream schemaTableStream);

		// Token: 0x060000D9 RID: 217
		void Load(Stream stringTableStream, Stream schemaTableStream);
	}
}
