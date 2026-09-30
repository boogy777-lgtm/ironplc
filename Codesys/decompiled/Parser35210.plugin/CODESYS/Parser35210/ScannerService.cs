using System;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using CODESYS.Parser;
using CODESYS.Parser35210.PragmaScanner;
using CODESYS.Parser35210.Scanner;

namespace CODESYS.Parser35210
{
	[TypeGuid("{fc3ba1fd-982c-45db-bd61-8c89f1cf1ae8}")]
	public class ScannerService : IScannerService, ILanguageVersionDependentService
	{
		public Version LanguageVersion => ParserService.s_LanguageVersion;

		public ITokenFactory TokenFactory => TokenFactoryClass.Singleton;

		public _IScanner5 CreateInternalScanner(ITypeTable typeTable, IOverflowChecker overflowChecker, IScannerOptionsService scannerOptionsService)
		{
			return new InternalScanner(typeTable, overflowChecker, scannerOptionsService);
		}

		public IMultiStringScanner CreateMultiStringScanner(ITypeTable typeTable, IOverflowChecker overflowChecker, IScannerOptionsService scannerOptionsService)
		{
			return new MultiStringScanner(typeTable, overflowChecker, scannerOptionsService);
		}

		public string GetTextOfOperator(Operator op)
		{
			return OperatorTable.Instance.GetTextOfOperator(op);
		}

		public string GetTextOfOperator(Operator op, bool bShort)
		{
			return OperatorTable.Instance.GetTextOfOperator(op, bShort);
		}

		public Operator GetOperatorFromText(string stOperator)
		{
			return OperatorTable.Instance.GetOperatorFromText(stOperator);
		}

		public void UpdateOperatorTable()
		{
		}

		public bool IsReservedUnusedKeyword(string stToken)
		{
			return ReservedUnusedKeywords.IsReservedUnusedKeyword(stToken);
		}

		public IPragmaScanner CreatePragmaScanner(IScanner9 scanner)
		{
			return new CODESYS.Parser35210.PragmaScanner.PragmaScanner(scanner);
		}
	}
}
