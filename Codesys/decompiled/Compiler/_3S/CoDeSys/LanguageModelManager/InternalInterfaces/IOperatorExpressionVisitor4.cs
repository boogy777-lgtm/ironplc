using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface IOperatorExpressionVisitor4 : IOperatorExpressionVisitor3, IOperatorExpressionVisitor2, IOperatorExpressionVisitor
	{
		void visitMemoryBarrier(_IOperatorExpression op);

		void visitVector(_IOperatorExpression op);
	}
}
