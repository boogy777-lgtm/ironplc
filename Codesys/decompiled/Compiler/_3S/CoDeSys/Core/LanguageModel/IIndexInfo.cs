using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IIndexInfo
	{
		IExpression IndexExpression { get; }

		int BaseSize { get; }
	}
}
