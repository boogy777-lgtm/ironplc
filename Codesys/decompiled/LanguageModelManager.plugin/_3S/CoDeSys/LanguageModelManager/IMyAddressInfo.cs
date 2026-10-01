using System;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x020000E5 RID: 229
	internal interface IMyAddressInfo : IAddressInfo4, IAddressInfo3, IAddressInfo2, IAddressInfo
	{
		// Token: 0x06001139 RID: 4409
		void SetSize(int nSize);

		// Token: 0x0600113A RID: 4410
		IMyAddressInfo Duplicate();
	}
}
