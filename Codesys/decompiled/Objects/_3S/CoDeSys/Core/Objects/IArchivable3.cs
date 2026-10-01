using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x02000136 RID: 310
	[ReleasedInterface]
	public interface IArchivable3 : IArchivable2, IArchivable
	{
		// Token: 0x060004D4 RID: 1236
		void SetSerializableValue(string stValueName, object value, IArchiveReporter reporter);
	}
}
