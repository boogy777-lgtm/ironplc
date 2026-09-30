using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ICallExpression4 : ICallExpression3, ICallExpression2, ICallExpression, IExpression2, IExpression, IExprement
	{
		IBreakpoint BeforeCallBreakpoint { get; }

		IBreakpoint SetBeforeCallBreakpoint(int nOffset, byte bySize);
	}
}
