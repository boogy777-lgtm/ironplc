using System;
using CODESYS.Parser;
using CODESYS.Parser35220.PragmaScanner;
using CODESYS.Parser35220.Scanner;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace CODESYS.Parser35220
{
	// Token: 0x0200000A RID: 10
	[TypeGuid("{98ddbe67-3f30-4d5b-bf09-e367d7fdf0a3}")]
	public class ScannerService : IScannerService2, IScannerService, ILanguageVersionDependentService
	{
		// Token: 0x1700000D RID: 13
		// (get) Token: 0x06000040 RID: 64 RVA: 0x00002DE9 File Offset: 0x00000FE9
		public Version LanguageVersion
		{
			get
			{
				return ParserService.s_LanguageVersion;
			}
		}

		// Token: 0x06000041 RID: 65 RVA: 0x00002EC1 File Offset: 0x000010C1
		public _IScanner5 CreateInternalScanner(ITypeTable typeTable, IOverflowChecker overflowChecker, IScannerOptionsService scannerOptionsService)
		{
			return new InternalScanner(typeTable, overflowChecker, scannerOptionsService);
		}

		// Token: 0x06000042 RID: 66 RVA: 0x00002ECB File Offset: 0x000010CB
		public IMultiStringScanner CreateMultiStringScanner(ITypeTable typeTable, IOverflowChecker overflowChecker, IScannerOptionsService scannerOptionsService)
		{
			return new MultiStringScanner(typeTable, overflowChecker, scannerOptionsService);
		}

		// Token: 0x06000043 RID: 67 RVA: 0x00002ED5 File Offset: 0x000010D5
		public string GetTextOfOperator(Operator op)
		{
			return OperatorTable.Instance.GetTextOfOperator(op);
		}

		// Token: 0x06000044 RID: 68 RVA: 0x00002EE2 File Offset: 0x000010E2
		public string GetTextOfOperator(Operator op, bool bShort)
		{
			return OperatorTable.Instance.GetTextOfOperator(op, bShort);
		}

		// Token: 0x06000045 RID: 69 RVA: 0x00002EF0 File Offset: 0x000010F0
		public Operator GetOperatorFromText(string stOperator)
		{
			return OperatorTable.Instance.GetOperatorFromText(stOperator);
		}

		// Token: 0x06000046 RID: 70 RVA: 0x00002074 File Offset: 0x00000274
		public void UpdateOperatorTable()
		{
		}

		// Token: 0x06000047 RID: 71 RVA: 0x00002EFD File Offset: 0x000010FD
		public bool IsReservedUnusedKeyword(string stToken)
		{
			return ReservedUnusedKeywords.IsReservedUnusedKeyword(stToken);
		}

		// Token: 0x06000048 RID: 72 RVA: 0x00002F05 File Offset: 0x00001105
		public IPragmaScanner CreatePragmaScanner(IScanner9 scanner)
		{
			return new PragmaScanner(scanner);
		}

		// Token: 0x1700000E RID: 14
		// (get) Token: 0x06000049 RID: 73 RVA: 0x00002F0D File Offset: 0x0000110D
		public ITokenFactory TokenFactory
		{
			get
			{
				return TokenFactoryClass.Singleton;
			}
		}

		// Token: 0x0600004A RID: 74 RVA: 0x00002F14 File Offset: 0x00001114
		public bool IsContextualOperator(Operator op)
		{
			return OperatorTable.Instance.IsContextualOperator(op);
		}
	}
}
