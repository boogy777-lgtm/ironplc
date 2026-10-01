using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IJumpTable
	{
		long Start { get; }

		long End { get; }

		int Count { get; }

		IExpression this[int i] { get; }
	}
}
