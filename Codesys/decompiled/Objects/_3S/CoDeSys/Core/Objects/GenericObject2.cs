using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x020000DA RID: 218
	[ReleasedClass]
	public abstract class GenericObject2 : GenericObject, IArchivable6, IArchivable5, IArchivable4, IArchivable3, IArchivable2, IArchivable
	{
		// Token: 0x0600035E RID: 862 RVA: 0x00005893 File Offset: 0x00003A93
		public virtual string[] GetSerializableValueNames(IArchiveVersionInfo info, IArchiveReporter reporter)
		{
			return base.GenericObjectService.GetSerializableValueNames(this, info, reporter);
		}

		// Token: 0x0600035F RID: 863 RVA: 0x000058A3 File Offset: 0x00003AA3
		public virtual object GetSerializableValue(string stValueName, IArchiveVersionInfo info, IArchiveReporter reporter)
		{
			return base.GenericObjectService.GetSerializableValue(this, stValueName, info, reporter);
		}

		// Token: 0x06000360 RID: 864 RVA: 0x000058B4 File Offset: 0x00003AB4
		public virtual void SetSerializableValue(string stValueName, object value, IArchiveReporter reporter)
		{
			base.GenericObjectService.SetSerializableValue(this, stValueName, value, reporter);
		}

		// Token: 0x06000361 RID: 865 RVA: 0x000058C5 File Offset: 0x00003AC5
		public virtual void BeforeSerialize(IArchiveVersionInfo info)
		{
			base.GenericObjectService.BeforeSerialize(this, info);
		}

		// Token: 0x06000362 RID: 866 RVA: 0x000058D4 File Offset: 0x00003AD4
		public virtual object CreateSerializableValue(string valueName, byte[] nesting, IArchiveReporter reporter)
		{
			return base.GenericObjectService.CreateSerializableValue(this, valueName, nesting, reporter);
		}

		// Token: 0x06000363 RID: 867 RVA: 0x000058E5 File Offset: 0x00003AE5
		public virtual GenericCollectionSerializationMode? GetGenericCollectionSerializationMode(string valueName, IArchiveVersionInfo info, bool forceLegacy, IArchiveReporter reporter)
		{
			return base.GenericObjectService.GetGenericSerializationMode(this, valueName, info, forceLegacy, reporter);
		}

		// Token: 0x06000364 RID: 868 RVA: 0x000058F8 File Offset: 0x00003AF8
		public virtual bool IsSerializableForVersion(IArchiveVersionInfo info)
		{
			return base.GenericObjectService.IsSerializableForVersion(this, info);
		}
	}
}
