using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILanguageModelManager27 : ILanguageModelManager26, ILanguageModelManager25, ILanguageModelManager24, ILanguageModelManager23, ILanguageModelManager22, ILanguageModelManager21
	{
		IFlowVarRef[] GetAllFlowVarReferences(Guid objectguid, Guid guidExplicitApplicationGuid, string stInstance, long[] alPositionsOfInterest);
	}
}
