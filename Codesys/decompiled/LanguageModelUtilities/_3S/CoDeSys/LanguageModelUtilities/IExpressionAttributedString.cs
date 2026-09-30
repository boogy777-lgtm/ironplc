using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[ReleasedInterface]
	public interface IExpressionAttributedString : IAttributedString
	{
		IExpression Expression { get; }

		bool Available { get; }
	}
}
