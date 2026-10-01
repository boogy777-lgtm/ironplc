using System;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x0200003E RID: 62
	internal class LegacySerializableMemberAccess
	{
		// Token: 0x0600010C RID: 268 RVA: 0x00003994 File Offset: 0x00001B94
		public LegacySerializableMemberAccess(string stTagName, StorageVersionAttribute storageVersion, bool bIgnorable, StorageDefaultValueAttribute defaultValue, StorageSaveAsArrayAttribute saveAsArray, StorageSaveAsNonGenericCollectionAttribute saveAsNonGenericCollection, Type type, LegacyGetterDelegate getter, LegacySetterDelegate setter)
		{
			this.TagName = stTagName;
			this.StorageVersion = storageVersion;
			this.Ignorable = bIgnorable;
			this.DefaultValue = defaultValue;
			this.SaveAsArray = saveAsArray;
			this.SaveAsNonGenericCollection = saveAsNonGenericCollection;
			this.Type = type;
			this.Getter = getter;
			this.Setter = setter;
		}

		// Token: 0x04000034 RID: 52
		public readonly string TagName;

		// Token: 0x04000035 RID: 53
		public readonly StorageVersionAttribute StorageVersion;

		// Token: 0x04000036 RID: 54
		public readonly bool Ignorable;

		// Token: 0x04000037 RID: 55
		public readonly StorageDefaultValueAttribute DefaultValue;

		// Token: 0x04000038 RID: 56
		public readonly StorageSaveAsArrayAttribute SaveAsArray;

		// Token: 0x04000039 RID: 57
		public readonly StorageSaveAsNonGenericCollectionAttribute SaveAsNonGenericCollection;

		// Token: 0x0400003A RID: 58
		public readonly Type Type;

		// Token: 0x0400003B RID: 59
		public readonly LegacyGetterDelegate Getter;

		// Token: 0x0400003C RID: 60
		public readonly LegacySetterDelegate Setter;
	}
}
