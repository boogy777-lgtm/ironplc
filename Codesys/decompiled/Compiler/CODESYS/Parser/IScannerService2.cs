using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.Parser
{
	[ReleasedInterface]
	public interface IScannerService2 : IScannerService, ILanguageVersionDependentService
	{
		bool IsContextualOperator(Operator op);
	}
}
