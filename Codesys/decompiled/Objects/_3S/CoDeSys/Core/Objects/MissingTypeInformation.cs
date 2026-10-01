using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x02000046 RID: 70
	[ReleasedClass]
	[Serializable]
	public struct MissingTypeInformation
	{
		// Token: 0x06000126 RID: 294 RVA: 0x00003EAA File Offset: 0x000020AA
		public MissingTypeInformation(Guid missingTypeGuid, Guid owningPackageId, string stOwningPackageName, Version owningPackageVersion)
		{
			this.MissingTypeGuid = missingTypeGuid;
			this.OwningPackageId = owningPackageId;
			this.OwningPackageName = stOwningPackageName;
			this.OwningPackageVersion = owningPackageVersion;
		}

		// Token: 0x0400004B RID: 75
		public readonly Guid MissingTypeGuid;

		// Token: 0x0400004C RID: 76
		public readonly Guid OwningPackageId;

		// Token: 0x0400004D RID: 77
		public readonly string OwningPackageName;

		// Token: 0x0400004E RID: 78
		public readonly Version OwningPackageVersion;
	}
}
