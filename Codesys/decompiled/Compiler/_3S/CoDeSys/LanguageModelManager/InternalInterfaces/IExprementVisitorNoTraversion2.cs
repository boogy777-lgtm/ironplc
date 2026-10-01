using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface IExprementVisitorNoTraversion2 : IExprementVisitorNoTraversion
	{
		void visit(_IPragmaIfStatement pifst, out bool bDoneAlready);
	}
}
