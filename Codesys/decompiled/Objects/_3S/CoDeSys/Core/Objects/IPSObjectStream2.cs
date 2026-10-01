using System;
using System.IO;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x0200011B RID: 283
	[ReleasedInterface]
	public interface IPSObjectStream2 : IPSObjectStream
	{
		// Token: 0x06000464 RID: 1124
		void Read(Stream stream, Guid archiveReaderGuid, bool throwIfTypeIsMissing);
	}
}
