using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IIndexAccessExpression : IExpression2, IExpression, IExprement
	{
		IExpression Var { get; }

		IExpression[] Accesses { get; }
	}
}
