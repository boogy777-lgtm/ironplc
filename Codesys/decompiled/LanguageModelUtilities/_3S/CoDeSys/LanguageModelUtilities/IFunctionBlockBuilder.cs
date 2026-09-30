using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[ReleasedInterface]
	public interface IFunctionBlockBuilder : IPouBuilder, IHasVarDeclaration, IHasAttributes
	{
		void SetExtends(IExprementPosition pos, string stNamespace, string stBaseFunctionBlock);

		void AddMethod(IMethodBuilder method);
	}
}
