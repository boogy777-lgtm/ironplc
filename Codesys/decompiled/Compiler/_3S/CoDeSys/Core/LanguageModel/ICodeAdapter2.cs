using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ICodeAdapter2 : ICodeAdapter
	{
		IBreakpoint SetBreakpoint(IExprement expr, int nOffset, byte bySize);

		IBreakpoint GetBreakpoint(IExprement expr);

		void SetCGAttributes(IExprement expr, ICodeGeneratorAttributes attr);

		ICodeGeneratorAttributes GetCGAttributes(IExprement expr);
	}
}
