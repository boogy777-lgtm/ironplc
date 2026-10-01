using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IArrayInitialization : IExpression, IExprement
	{
		IExpression[] InitValues { get; }
	}
}
