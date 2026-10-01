using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[ReleasedInterface]
	public interface IFunctionBlockBuilder2 : IFunctionBlockBuilder, IPouBuilder, IHasVarDeclaration, IHasAttributes
	{
		void AddImplements(IExprementPosition pos, string stNamespace, string stInterface);
	}
}
