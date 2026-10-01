using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace CODESYS.Parser
{
	[ReleasedInterface]
	public interface IScannerService : ILanguageVersionDependentService
	{
		ITokenFactory TokenFactory { get; }

		_IScanner5 CreateInternalScanner(ITypeTable typeTable, IOverflowChecker overflowChecker, IScannerOptionsService scannerOptionsService);

		IMultiStringScanner CreateMultiStringScanner(ITypeTable typeTable, IOverflowChecker overflowChecker, IScannerOptionsService scannerOptionsService);

		string GetTextOfOperator(Operator op);

		string GetTextOfOperator(Operator op, bool bShort);

		Operator GetOperatorFromText(string stOperator);

		void UpdateOperatorTable();

		bool IsReservedUnusedKeyword(string stToken);

		IPragmaScanner CreatePragmaScanner(IScanner9 scanner);
	}
}
