using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IExprVisitor9 : IExprVisitor8, IExprVisitor7, IExprVisitor6, IExprVisitor5, IExprVisitor4, IExprVisitor3, IExprVisitor2, IExprVisitor
	{
		void visit(_IProjectDefinedExpression projectDefinedExpression);
	}
}
