using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILanguageModelManager12 : ILanguageModelManager11, ILanguageModelManager10, ILanguageModelManager9, ILanguageModelManager8, ILanguageModelManager7, ILanguageModelManager6, ILanguageModelManager5, ILanguageModelManager4, ILanguageModelManager3, ILanguageModelManager2, ILanguageModelManager
	{
		IFlowVarRef[] GetAllFlowVarReferences(string stInstance, long[] alPositionsOfInterest);

		IFlowVarRef GetFlowVarReference(string stExpression, string stInstancePath, long lPosition);
	}
}
