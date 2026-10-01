using System;
using CODESYS.Objects.Properties;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x020000B5 RID: 181
	[ReleasedClass]
	public class DeleteObjectConstraintException : ObjectManagerException
	{
		// Token: 0x060002E8 RID: 744 RVA: 0x000050E8 File Offset: 0x000032E8
		[Obsolete("Use the other constructor.", true)]
		public DeleteObjectConstraintException(int nProjectHandle, Guid objectGuid) : base(nProjectHandle, objectGuid, string.Empty)
		{
		}

		// Token: 0x060002E9 RID: 745 RVA: 0x000050F7 File Offset: 0x000032F7
		public DeleteObjectConstraintException(int nProjectHandle, Guid objectGuid, string objectName, string parentObjectFullName) : base(nProjectHandle, objectGuid, string.Empty, objectName)
		{
			this._parentObjectFullName = parentObjectFullName;
		}

		// Token: 0x1700010B RID: 267
		// (get) Token: 0x060002EA RID: 746 RVA: 0x00005110 File Offset: 0x00003310
		public override string Message
		{
			get
			{
				string arg = string.IsNullOrEmpty(this._parentObjectFullName) ? "<root>" : this._parentObjectFullName;
				return string.Format(Resources.DeleteObjectConstraintException, base.ObjectName, arg);
			}
		}

		// Token: 0x0400011C RID: 284
		private string _parentObjectFullName;
	}
}
