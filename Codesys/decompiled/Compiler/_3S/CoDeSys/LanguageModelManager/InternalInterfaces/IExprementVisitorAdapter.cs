using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface IExprementVisitorAdapter : IExprementVisitor2, IExprementVisitor
	{
		void visit(_IProgramCounterExpression pc);

		void visit(_IFramePointerExpression pc);

		void visit(_ICallInstanceExpression cie);
	}
}
