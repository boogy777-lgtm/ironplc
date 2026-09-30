using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ICallExpression2 : ICallExpression, IExpression2, IExpression, IExprement
	{
		IBreakpoint CallBreakpoint { get; }

		IBreakpoint SetCallBreakpoint(int nOffset, byte bySize);
	}
}
