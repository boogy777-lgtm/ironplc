using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IParser5 : IParser4, IParser3, IParser2, IParser
	{
		IExpression ParseInitialisation();
	}
}
