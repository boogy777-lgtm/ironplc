using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface ITypeVisitor4 : ITypeVisitor3, ITypeVisitor2, ITypeVisitor
	{
		void visit(IGenericUserdefType genericUserdefType);
	}
}
