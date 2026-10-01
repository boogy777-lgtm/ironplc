using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILibParameterTable2 : ILibParameterTable
	{
		void AddParameter(string stName, IExpression expression);
	}
}
