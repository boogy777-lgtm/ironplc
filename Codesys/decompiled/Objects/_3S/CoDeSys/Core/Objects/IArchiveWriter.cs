using System;
using System.IO;
using System.Text;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x0200002F RID: 47
	[ReleasedInterface]
	public interface IArchiveWriter
	{
		// Token: 0x060000CA RID: 202
		void Initialize(string stFileName, Encoding encoding);

		// Token: 0x060000CB RID: 203
		void Initialize(Stream stream, Encoding encoding);

		// Token: 0x060000CC RID: 204
		void Save(IArchivable obj);
	}
}
