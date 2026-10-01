using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface IFlowPosVisitor : IExprementVisitorNoTraversion
	{
		IBreakpoint LastListedBreakpoint { get; set; }

		IScope IScope { get; }

		void SetCurrentBreakpoint(IBreakpoint bp);

		void GenerateDummyFlow(IBreakpoint bp);

		IBreakpoint GetCurrentBreakpoint();

		void visitCompoAccess(_ICompoAccessExpression compo, AccessFlag access);

		void visitIndexAccess(_IIndexAccessExpression index, AccessFlag access);

		void visitDeRefAccess(_IDeRefAccessExpression deref, AccessFlag access);
	}
}
