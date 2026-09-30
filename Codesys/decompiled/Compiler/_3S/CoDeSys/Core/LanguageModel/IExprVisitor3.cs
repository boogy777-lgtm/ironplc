using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IExprVisitor3 : IExprVisitor2, IExprVisitor
	{
		void visit(IStructureInitialization structInit);

		void visit(IArrayInitialization arrayInit);
	}
}
