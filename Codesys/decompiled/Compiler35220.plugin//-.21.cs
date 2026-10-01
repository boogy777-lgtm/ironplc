using System;
using System.Collections.Generic;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0084
{
	// Token: 0x02000304 RID: 772
	internal interface \u001A
	{
		// Token: 0x06002F00 RID: 12032
		IVariable \u0001(_ISignature \u0002, int \u0003);

		// Token: 0x06002F01 RID: 12033
		IEnumerable<_IExpression> \u0001();

		// Token: 0x06002F02 RID: 12034
		bool \u0001(CaseInsensitiveDictionary<IVariable> \u0002, _ISignature \u0003);
	}
}
