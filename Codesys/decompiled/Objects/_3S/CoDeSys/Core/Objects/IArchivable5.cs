using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x02000138 RID: 312
	[ReleasedInterface]
	public interface IArchivable5 : IArchivable4, IArchivable3, IArchivable2, IArchivable
	{
		// Token: 0x060004D6 RID: 1238
		object CreateSerializableValue(string valueName, byte[] nesting, IArchiveReporter reporter);

		// Token: 0x060004D7 RID: 1239
		GenericCollectionSerializationMode? GetGenericCollectionSerializationMode(string valueName, IArchiveVersionInfo info, bool forceLegacy, IArchiveReporter reporter);
	}
}
