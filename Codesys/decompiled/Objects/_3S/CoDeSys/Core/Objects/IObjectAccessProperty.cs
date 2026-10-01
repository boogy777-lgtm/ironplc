using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x020000F1 RID: 241
	[ReleasedInterface]
	public interface IObjectAccessProperty : IObjectProperty, IGenericObject, IArchivable, ICloneable, IComparable
	{
		// Token: 0x060003AC RID: 940
		Guid GetPermissionId(string stVerb);

		// Token: 0x060003AD RID: 941
		void SetPermissionId(string stVerb, Guid permissionId);

		// Token: 0x060003AE RID: 942
		bool IsPermissionSpecified(Guid groupId, Guid permissionId);

		// Token: 0x060003AF RID: 943
		bool IsPermissionGranted(Guid groupId, Guid permissionId);

		// Token: 0x060003B0 RID: 944
		[Obsolete("Use IUserManagement3.SetPermissionState instead.")]
		void GrantPermission(Guid groupId, Guid permissionId);

		// Token: 0x060003B1 RID: 945
		[Obsolete("Use IUserManagement3.SetPermissionState instead.")]
		void DenyPermission(Guid groupId, Guid permissionId);

		// Token: 0x060003B2 RID: 946
		[Obsolete("Use IUserManagement3.SetPermissionState instead.")]
		void ClearPermission(Guid groupId, Guid permissionId);
	}
}
