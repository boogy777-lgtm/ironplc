using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x02000047 RID: 71
	[ReleasedClass]
	[Serializable]
	public struct MissingTypeInformation2
	{
		// Token: 0x06000127 RID: 295 RVA: 0x00003EC9 File Offset: 0x000020C9
		public MissingTypeInformation2(Guid missingTypeGuid, string stPlugInName, Version plugInVersion, Guid owningPackageId, string stOwningPackageName, Version owningPackageVersion)
		{
			this.MissingTypeGuid = missingTypeGuid;
			this.PlugInName = stPlugInName;
			this.PlugInVersion = plugInVersion;
			this.OwningPackageId = owningPackageId;
			this.OwningPackageName = stOwningPackageName;
			this.OwningPackageVersion = owningPackageVersion;
		}

		// Token: 0x0400004F RID: 79
		public readonly Guid MissingTypeGuid;

		// Token: 0x04000050 RID: 80
		public readonly string PlugInName;

		// Token: 0x04000051 RID: 81
		public readonly Version PlugInVersion;

		// Token: 0x04000052 RID: 82
		public readonly Guid OwningPackageId;

		// Token: 0x04000053 RID: 83
		public readonly string OwningPackageName;

		// Token: 0x04000054 RID: 84
		public readonly Version OwningPackageVersion;
	}
}
