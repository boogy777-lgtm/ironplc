using System;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[ReleasedInterface]
	public interface IMethodBuilder2 : IMethodBuilder, IPouBuilder, IHasVarDeclaration, IHasAttributes
	{
		void Initialize(ILanguageModelBuilder lmbuilder, Guid gdParent, Guid gdObject, Guid gdMessage);

		void SetComplexReturnType(string stComplexType);
	}
}
