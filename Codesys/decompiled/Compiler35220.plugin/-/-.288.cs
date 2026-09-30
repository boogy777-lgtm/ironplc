using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u001C
{
	// Token: 0x02000306 RID: 774
	internal interface \u0011
	{
		// Token: 0x06002F0A RID: 12042
		_ISignature4 \u0001(_ISignature4 \u0002);

		// Token: 0x06002F0B RID: 12043
		IEnumerable<string> \u0001(_ISignature4 \u0002);

		// Token: 0x06002F0C RID: 12044
		IList<_ISignature> \u0001(_ISignature4 \u0002, string \u0003);

		// Token: 0x06002F0D RID: 12045
		bool \u0001(_ISignature \u0002, _ISignature \u0003);

		// Token: 0x06002F0E RID: 12046
		ICompiledType \u0001(_IExpression \u0002, int \u0003);

		// Token: 0x06002F0F RID: 12047
		string \u0001(_ISignature \u0002);
	}
}
