using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ICallExpression3 : ICallExpression2, ICallExpression, IExpression2, IExpression, IExprement
	{
		KindOfCall KindOfCall { get; }
	}
}
