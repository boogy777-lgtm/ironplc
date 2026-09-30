using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IVariable2 : IVariable
	{
		void AddAttribute(string stAttribute, string stValue);
	}
}
