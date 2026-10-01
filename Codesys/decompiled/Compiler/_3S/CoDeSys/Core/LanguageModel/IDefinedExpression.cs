using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IDefinedExpression : IExpression2, IExpression, IExprement
	{
		IExpression ReferencedItem { get; }
	}
}
