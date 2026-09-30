using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0015
{
	// Token: 0x02000145 RID: 325
	internal interface \u0002 : ICommonScope, _IPrecompileScope, IPrecompileScope6, IPrecompileScope5, IPrecompileScope4, IPrecompileScope3, IPrecompileScope2, IPrecompileScope, IPrecompileScope7, IPrecompileScopeWithAliasService
	{
		// Token: 0x0600169F RID: 5791
		string \u0001();

		// Token: 0x060016A0 RID: 5792
		\u0002 \u0001();

		// Token: 0x060016A1 RID: 5793
		\u0002 \u0001(_ISignature \u0002);

		// Token: 0x060016A2 RID: 5794
		\u0002 \u0001(IExpression \u0002);

		// Token: 0x060016A3 RID: 5795
		\u0002 \u0001(_IPreCompileContext \u0002);

		// Token: 0x060016A4 RID: 5796
		\u0002 \u0002(_ISignature \u0002);

		// Token: 0x060016A5 RID: 5797
		ISignature[] \u0001(string \u0002);

		// Token: 0x060016A6 RID: 5798
		bool \u0001(string \u0002, out IVariable[] \u0003, out ISignature[] \u0004, out \u0002 \u0005);

		// Token: 0x17000542 RID: 1346
		ISignature this[Guid \u0002]
		{
			get;
		}

		// Token: 0x17000543 RID: 1347
		ISignature[] this[string \u0002]
		{
			get;
		}

		// Token: 0x060016A9 RID: 5801
		\u0002 \u0002(IExpression \u0002);

		// Token: 0x060016AA RID: 5802
		string \u0001(string \u0002);

		// Token: 0x17000544 RID: 1348
		// (get) Token: 0x060016AB RID: 5803
		ISignature MostLocalSignature { get; }

		// Token: 0x17000545 RID: 1349
		// (get) Token: 0x060016AC RID: 5804
		// (set) Token: 0x060016AD RID: 5805
		bool IgnoreImplicitEnumMembers { get; set; }

		// Token: 0x17000546 RID: 1350
		// (get) Token: 0x060016AE RID: 5806
		// (set) Token: 0x060016AF RID: 5807
		bool IgnoreActions { get; set; }
	}
}
