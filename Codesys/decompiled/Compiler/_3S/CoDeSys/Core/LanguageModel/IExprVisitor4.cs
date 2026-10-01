using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IExprVisitor4 : IExprVisitor3, IExprVisitor2, IExprVisitor
	{
		void visit(ICastExpression cast);

		void visit(ICompilerVersionExpression compverExpr);

		void visit(IRuntimeVersionExpression runverExpr);
	}
}
