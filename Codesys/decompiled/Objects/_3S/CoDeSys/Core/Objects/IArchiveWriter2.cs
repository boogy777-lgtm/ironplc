using System;
using System.IO;
using System.Text;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x02000030 RID: 48
	[ReleasedInterface]
	public interface IArchiveWriter2 : IArchiveWriter
	{
		// Token: 0x060000CD RID: 205
		void Initialize(string stFileName, Encoding encoding, bool bOmitXMLDeclaration);

		// Token: 0x060000CE RID: 206
		void Initialize(Stream stream, Encoding encoding, bool bOmitXMLDeclaration);

		// Token: 0x060000CF RID: 207
		void Save(IArchivable2 obj, Profile profile, IArchiveReporter reporter);

		// Token: 0x060000D0 RID: 208
		ISharedDataStorage CreateSharedDataStorage();

		// Token: 0x060000D1 RID: 209
		void Save(IArchivable obj, ISharedDataStorage sds);

		// Token: 0x060000D2 RID: 210
		void Save(IArchivable2 obj, ISharedDataStorage sds, Profile profile, IArchiveReporter reporter);
	}
}
