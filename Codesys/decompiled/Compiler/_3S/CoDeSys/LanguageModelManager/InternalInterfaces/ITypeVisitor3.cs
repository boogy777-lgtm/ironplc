using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface ITypeVisitor3 : ITypeVisitor2, ITypeVisitor
	{
		void visit(_IAliasType type);

		void visit(_IXDIntType type);

		void visit(_IXDWordType type);

		void visit(_IXLWordType type);
	}
}
