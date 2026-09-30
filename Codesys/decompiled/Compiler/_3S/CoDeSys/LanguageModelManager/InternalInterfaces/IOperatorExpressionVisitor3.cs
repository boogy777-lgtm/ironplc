using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface IOperatorExpressionVisitor3 : IOperatorExpressionVisitor2, IOperatorExpressionVisitor
	{
		void visitXAdd(_IOperatorExpression op);
	}
}
