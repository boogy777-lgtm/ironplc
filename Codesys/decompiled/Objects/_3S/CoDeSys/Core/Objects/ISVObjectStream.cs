using System;
using System.IO;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x02000159 RID: 345
	[ReleasedInterface]
	public interface ISVObjectStream
	{
		// Token: 0x06000530 RID: 1328
		void GetProfile(out Profile profile, out string stProfileName);

		// Token: 0x06000531 RID: 1329
		void SetProfile(Profile profile, string stProfileName);

		// Token: 0x170001D9 RID: 473
		// (get) Token: 0x06000532 RID: 1330
		int EntryCount { get; }

		// Token: 0x06000533 RID: 1331
		ISVObjectStreamEntry GetEntry(int nIndex);

		// Token: 0x06000534 RID: 1332
		ISVObjectStreamEntry CreateEntry(bool bIsRoot, IMetaObject metaObject, Guid parentSVNodeGuid, string[] path);

		// Token: 0x06000535 RID: 1333
		void AddEntry(ISVObjectStreamEntry entry);

		// Token: 0x06000536 RID: 1334
		void Write(Stream stream, Guid archiveWriterGuid, IArchiveReporter reporter);

		// Token: 0x06000537 RID: 1335
		void Read(Stream stream, Guid archiveReaderGuid);
	}
}
