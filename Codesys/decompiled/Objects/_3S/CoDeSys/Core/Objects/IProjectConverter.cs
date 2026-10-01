using System;
using System.IO;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x020000FB RID: 251
	[ReleasedInterface]
	public interface IProjectConverter
	{
		// Token: 0x060003EC RID: 1004
		ProjectConversionResult Import(int nProjectHandle, Stream stream, string stStreamName);
	}
}
