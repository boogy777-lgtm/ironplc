using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x020000F3 RID: 243
	[ReleasedInterface]
	public interface IDongleEncryption
	{
		// Token: 0x060003B4 RID: 948
		string[] GetRegisteredDongles();

		// Token: 0x060003B5 RID: 949
		bool IsDongleRegistered(string stId);

		// Token: 0x060003B6 RID: 950
		void RegisterDongle(string stId);

		// Token: 0x060003B7 RID: 951
		void UnregisterDongle(string stId);

		// Token: 0x060003B8 RID: 952
		string GetDongleComment(string stId);

		// Token: 0x060003B9 RID: 953
		void SetDongleComment(string stId, string stComment);

		// Token: 0x060003BA RID: 954
		string GetDongleSerialNumber(string stId);
	}
}
