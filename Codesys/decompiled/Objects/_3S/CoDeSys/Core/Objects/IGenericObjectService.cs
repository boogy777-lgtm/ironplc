using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x020000DB RID: 219
	[ReleasedInterface]
	public interface IGenericObjectService
	{
		// Token: 0x06000366 RID: 870
		object Clone(GenericObject go);

		// Token: 0x06000367 RID: 871
		string[] GetSerializableValueNames(GenericObject go);

		// Token: 0x06000368 RID: 872
		object GetSerializableValue(GenericObject go, string valueName);

		// Token: 0x06000369 RID: 873
		void SetSerializableValue(GenericObject go, string valueName, object value);

		// Token: 0x0600036A RID: 874
		string[] GetSerializableValueNames(GenericObject2 go, IArchiveVersionInfo info, IArchiveReporter reporter);

		// Token: 0x0600036B RID: 875
		object GetSerializableValue(GenericObject2 go, string valueName, IArchiveVersionInfo info, IArchiveReporter reporter);

		// Token: 0x0600036C RID: 876
		void SetSerializableValue(GenericObject2 go, string valueName, object value, IArchiveReporter reporter);

		// Token: 0x0600036D RID: 877
		void BeforeSerialize(GenericObject2 go, IArchiveVersionInfo info);

		// Token: 0x0600036E RID: 878
		object CreateSerializableValue(GenericObject2 go, string valueName, byte[] nesting, IArchiveReporter reporter);

		// Token: 0x0600036F RID: 879
		GenericCollectionSerializationMode? GetGenericSerializationMode(GenericObject2 go, string valueName, IArchiveVersionInfo info, bool forceLegacy, IArchiveReporter reporter);

		// Token: 0x06000370 RID: 880
		bool IsSerializableForVersion(GenericObject2 go, IArchiveVersionInfo info);
	}
}
