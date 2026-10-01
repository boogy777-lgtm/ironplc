using System;
using _3S.CoDeSys.Compiler35220.PreCompile.Typification;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u001F
{
	// Token: 0x020001B7 RID: 439
	internal interface \u0007
	{
		// Token: 0x0600203A RID: 8250
		bool \u0001(_IExprement \u0002, ICompiledType \u0003, ICompiledType \u0004, IScope5 \u0005, ref _IExpression \u0006);

		// Token: 0x0600203B RID: 8251
		bool \u0001(_IExprement \u0002, ICompiledType \u0003, ICompiledType \u0004, IScope5 \u0005, ref _IExpression \u0006, out bool \u0007);

		// Token: 0x0600203C RID: 8252
		bool \u0001(_IExprement \u0002, ICompiledType \u0003, TypeClass \u0004, IScope5 \u0005, ref _IExpression \u0006);

		// Token: 0x0600203D RID: 8253
		bool \u0001(ICompiledType \u0002, ICompiledType \u0003, IScope5 \u0004, _IExpression \u0005);

		// Token: 0x0600203E RID: 8254
		void \u0001(_IExprement \u0002, MessageId \u0003, params object[] \u0004);

		// Token: 0x0600203F RID: 8255
		void \u0001(_IExprement \u0002, ISignature \u0003, _IExpression \u0004, _IVariable \u0005);

		// Token: 0x06002040 RID: 8256
		void \u0001(_IExprement \u0002, _ICompilerMessage \u0003);

		// Token: 0x170005FB RID: 1531
		// (get) Token: 0x06002041 RID: 8257
		// (set) Token: 0x06002042 RID: 8258
		bool ExplicitConversion { get; set; }

		// Token: 0x170005FC RID: 1532
		// (get) Token: 0x06002043 RID: 8259
		// (set) Token: 0x06002044 RID: 8260
		bool AddCrossReferences { get; set; }

		// Token: 0x170005FD RID: 1533
		// (get) Token: 0x06002045 RID: 8261
		// (set) Token: 0x06002046 RID: 8262
		IMessageSuppressionController MessageSuppressionController { get; set; }

		// Token: 0x170005FE RID: 1534
		// (get) Token: 0x06002047 RID: 8263
		// (set) Token: 0x06002048 RID: 8264
		Guid MessageGuid { get; set; }

		// Token: 0x170005FF RID: 1535
		// (get) Token: 0x06002049 RID: 8265
		// (set) Token: 0x0600204A RID: 8266
		bool ConvertAllTypeMismatches { get; set; }

		// Token: 0x17000600 RID: 1536
		// (get) Token: 0x0600204B RID: 8267
		// (set) Token: 0x0600204C RID: 8268
		bool InImplicitCode { get; set; }
	}
}
