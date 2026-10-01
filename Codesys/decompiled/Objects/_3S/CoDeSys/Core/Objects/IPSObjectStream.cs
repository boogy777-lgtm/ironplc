using System;
using System.IO;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x0200011A RID: 282
	[ReleasedInterface]
	public interface IPSObjectStream
	{
		// Token: 0x0600045C RID: 1116
		void GetProfile(out Profile profile, out string profileName);

		// Token: 0x0600045D RID: 1117
		void SetProfile(Profile profile, string profileName);

		// Token: 0x17000198 RID: 408
		// (get) Token: 0x0600045E RID: 1118
		int EntryCount { get; }

		// Token: 0x0600045F RID: 1119
		IPSObjectStreamEntry GetEntry(int index);

		// Token: 0x06000460 RID: 1120
		IPSObjectStreamEntry CreateEntry(bool isRoot, IMetaObject metaObject, Guid parentPSNodeGuid, string[] path);

		// Token: 0x06000461 RID: 1121
		void AddEntry(IPSObjectStreamEntry entry);

		// Token: 0x06000462 RID: 1122
		void Write(Stream stream, Guid archiveWriterGuid, IArchiveReporter reporter);

		// Token: 0x06000463 RID: 1123
		void Read(Stream stream, Guid archiveReaderGuid);
	}
}
