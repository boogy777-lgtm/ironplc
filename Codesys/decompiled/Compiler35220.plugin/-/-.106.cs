using System;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0017
{
	// Token: 0x0200013E RID: 318
	internal interface \u0006 : ICommonScope2, ICommonScope
	{
		// Token: 0x060015F7 RID: 5623
		bool \u0001(_IExpression \u0002);

		// Token: 0x060015F8 RID: 5624
		bool \u0002(_IExpression \u0002);

		// Token: 0x060015F9 RID: 5625
		_IVariable \u0001(_IExpression \u0002);

		// Token: 0x060015FA RID: 5626
		_ISignature \u0001(_IExpression \u0002);

		// Token: 0x060015FB RID: 5627
		_IExpression \u0001(_ISignature \u0002, _IVariable \u0003);

		// Token: 0x060015FC RID: 5628
		\u0006 \u0001(_IExpression \u0002, _IUserdefType \u0003);

		// Token: 0x060015FD RID: 5629
		\u0006 \u0001(_ISignature \u0002);

		// Token: 0x17000527 RID: 1319
		// (get) Token: 0x060015FE RID: 5630
		bool ContainsCopyCode { get; }

		// Token: 0x060015FF RID: 5631
		void \u0001(_IExpression \u0002, out _IVariable \u0003, out _ISignature \u0004, out \u0006 \u0005);
	}
}
