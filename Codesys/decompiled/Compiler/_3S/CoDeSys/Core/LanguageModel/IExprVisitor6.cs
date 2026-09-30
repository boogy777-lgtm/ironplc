using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IExprVisitor6 : IExprVisitor5, IExprVisitor4, IExprVisitor3, IExprVisitor2, IExprVisitor
	{
		void visit(IErrorExpression error);

		void visit(IErrorStatement error);
	}
}
