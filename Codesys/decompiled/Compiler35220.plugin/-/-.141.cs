using System;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0017
{
	// Token: 0x0200018F RID: 399
	internal interface \u0010
	{
		// Token: 0x170005B4 RID: 1460
		// (get) Token: 0x06001BFC RID: 7164
		bool ConvertAllTypeMismatches { get; }

		// Token: 0x06001BFD RID: 7165
		void AddError(_IExprement exp, MessageId mid, params object[] args);

		// Token: 0x06001BFE RID: 7166
		void \u0001(_IExprement \u0002, MessageId \u0003, params object[] \u0004);

		// Token: 0x170005B5 RID: 1461
		// (get) Token: 0x06001BFF RID: 7167
		bool InImplicitCode { get; }

		// Token: 0x170005B6 RID: 1462
		// (get) Token: 0x06001C00 RID: 7168
		Guid ApplicationGuid { get; }

		// Token: 0x170005B7 RID: 1463
		// (get) Token: 0x06001C01 RID: 7169
		bool TreatLRealAsReal { get; }

		// Token: 0x170005B8 RID: 1464
		// (get) Token: 0x06001C02 RID: 7170
		bool TreatInt64AsInt32 { get; }

		// Token: 0x170005B9 RID: 1465
		// (get) Token: 0x06001C03 RID: 7171
		bool NoConversionChecks { get; }

		// Token: 0x06001C04 RID: 7172
		bool \u0001(_IImplicitConversionExpression \u0002);
	}
}
