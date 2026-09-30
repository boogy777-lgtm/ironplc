using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface IOperatorExpressionVisitor2 : IOperatorExpressionVisitor
	{
		void visitCheckLicenseBit(_IOperatorExpression op);
	}
}
