using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[ReleasedInterface]
	public interface IProgramBuilder : IPouBuilder, IHasVarDeclaration, IHasAttributes
	{
		void AddMethod(IMethodBuilder method);
	}
}
