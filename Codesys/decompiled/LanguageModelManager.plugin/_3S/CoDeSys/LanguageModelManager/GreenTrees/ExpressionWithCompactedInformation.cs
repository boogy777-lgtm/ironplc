using System;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.GreenTrees
{
	// Token: 0x02000234 RID: 564
	internal class ExpressionWithCompactedInformation
	{
		// Token: 0x06002507 RID: 9479 RVA: 0x0005CF86 File Offset: 0x0005BF86
		public ExpressionWithCompactedInformation(_IExpression exp, ICompactedParseTreeInformation info)
		{
			this.CompactedInitialValueInformation = info;
			this.Expression = exp;
		}

		// Token: 0x17000A6D RID: 2669
		// (get) Token: 0x06002508 RID: 9480 RVA: 0x0005CF9C File Offset: 0x0005BF9C
		// (set) Token: 0x06002509 RID: 9481 RVA: 0x0005CFA4 File Offset: 0x0005BFA4
		public ICompactedParseTreeInformation CompactedInitialValueInformation { get; set; }

		// Token: 0x17000A6E RID: 2670
		// (get) Token: 0x0600250A RID: 9482 RVA: 0x0005CFAD File Offset: 0x0005BFAD
		// (set) Token: 0x0600250B RID: 9483 RVA: 0x0005CFB5 File Offset: 0x0005BFB5
		public _IExpression Expression { get; set; }
	}
}
