using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _IScanner : IScanner6, IScanner5, IScanner4, IScanner3, IScanner2, IScanner
	{
		IPragmaNotifier PragmaNotifier { get; set; }

		string GetInputSubString(int startIndex, int length);

		string _GetTextOfOperator(Operator op, bool bShort);

		Operator GetOperatorByText(string stOperator);
	}
}
