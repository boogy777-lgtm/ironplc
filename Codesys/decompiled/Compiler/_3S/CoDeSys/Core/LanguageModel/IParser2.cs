using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IParser2 : IParser
	{
		IExpression ParseExpression();
	}
}
