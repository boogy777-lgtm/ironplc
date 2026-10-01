using System;
using System.Collections.Generic;
using \u0016;
using CODESYS.Parser;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler35220.PreCompile
{
	// Token: 0x0200016D RID: 365
	public static class Scanner
	{
		// Token: 0x060018C6 RID: 6342 RVA: 0x0004CEE0 File Offset: 0x0004B0E0
		internal static _IScanner5 \u0001()
		{
			return LanguageServices.ScannerService.CreateInternalScanner(TypeTableClass.Singleton, \u0007.Singleton, ScannerOptionsService.Singleton);
		}

		// Token: 0x060018C7 RID: 6343 RVA: 0x0004CEFC File Offset: 0x0004B0FC
		internal static _IScanner5 \u0001(Version \u0002)
		{
			return LanguageServices.GetScannerService(\u0002).CreateInternalScanner(TypeTableClass.Singleton, \u0007.Singleton, ScannerOptionsService.Singleton);
		}

		// Token: 0x060018C8 RID: 6344 RVA: 0x0004CF18 File Offset: 0x0004B118
		public static _IScanner CreateMultiStringScanner(IList<string> strlText, bool bIncludeComments, bool bIncludeEndOfLines, bool bIncludePragmas, bool bIncludeWhitespaces)
		{
			IMultiStringScanner multiStringScanner = LanguageServices.ScannerService.CreateMultiStringScanner(TypeTableClass.Singleton, \u0007.Singleton, ScannerOptionsService.Singleton);
			multiStringScanner.AllowMultipleUnderlines = false;
			multiStringScanner.AllowNestedComments = APEnvironmentFacade.Instance.LanguageModelMgr.AllowNestedComments;
			multiStringScanner.IgnoreCase = true;
			multiStringScanner.IncludeComments = bIncludeComments;
			multiStringScanner.IncludeEndOfLines = bIncludeEndOfLines;
			multiStringScanner.IncludePragmas = bIncludePragmas;
			multiStringScanner.IncludeWhitespaces = bIncludeWhitespaces;
			multiStringScanner.InitializeMulti(strlText);
			return multiStringScanner;
		}

		// Token: 0x060018C9 RID: 6345 RVA: 0x0004CF88 File Offset: 0x0004B188
		internal static bool \u0001(char \u0002)
		{
			return (\u0002 >= 'A' && \u0002 <= 'Z') || (\u0002 >= 'a' && \u0002 <= 'z') || (\u0002 >= '0' && \u0002 <= '9') || \u0002 == '_';
		}

		// Token: 0x060018CA RID: 6346 RVA: 0x0004CFB0 File Offset: 0x0004B1B0
		public static string GetTextOfOperator(Operator op)
		{
			return LanguageServices.ScannerService.GetTextOfOperator(op);
		}

		// Token: 0x060018CB RID: 6347 RVA: 0x0004CFC0 File Offset: 0x0004B1C0
		public static string GetTextOfOperator(Operator op, bool bShort)
		{
			return LanguageServices.ScannerService.GetTextOfOperator(op, bShort);
		}

		// Token: 0x060018CC RID: 6348 RVA: 0x0004CFD0 File Offset: 0x0004B1D0
		public static Operator GetOperatorFromText(string stOperator)
		{
			return LanguageServices.ScannerService.GetOperatorFromText(stOperator);
		}

		// Token: 0x060018CD RID: 6349 RVA: 0x0004CFE0 File Offset: 0x0004B1E0
		public static void UpdateOperatorTable()
		{
			LanguageServices.ScannerService.UpdateOperatorTable();
		}

		// Token: 0x060018CE RID: 6350 RVA: 0x0004CFEC File Offset: 0x0004B1EC
		public static bool IsReservedUnusedKeyword(string stToken)
		{
			return LanguageServices.ScannerService.IsReservedUnusedKeyword(stToken);
		}

		// Token: 0x1700058F RID: 1423
		// (get) Token: 0x060018CF RID: 6351 RVA: 0x0004CFFC File Offset: 0x0004B1FC
		internal static bool UnicodeIdentifierOption
		{
			get
			{
				return APEnvironmentFacade.Instance.CompileOptions.UnicodeIdentifiers;
			}
		}

		// Token: 0x17000590 RID: 1424
		// (get) Token: 0x060018D0 RID: 6352 RVA: 0x0004D010 File Offset: 0x0004B210
		public static long InvalidPosition
		{
			get
			{
				return -1L;
			}
		}

		// Token: 0x060018D1 RID: 6353 RVA: 0x0004D014 File Offset: 0x0004B214
		public static bool IsContextualOperator(Operator op)
		{
			IScannerService2 scannerService = LanguageServices.ScannerService as IScannerService2;
			return scannerService != null && scannerService.IsContextualOperator(op);
		}
	}
}
