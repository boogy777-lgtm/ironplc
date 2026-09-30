using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[ReleasedInterface]
	public interface IPouBuilder : IHasVarDeclaration
	{
		void AddStatement(IExprementPosition pos, IStatement statement);

		void SetInhibitOnlineChange(bool bInhibitOnlineChange);
	}
}
