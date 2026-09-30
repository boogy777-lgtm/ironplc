using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILanguageModelBuilder9 : ILanguageModelBuilder8, ILanguageModelBuilder7, ILanguageModelBuilder6, ILanguageModelBuilder5, ILanguageModelBuilder4, ILanguageModelBuilder3, ILanguageModelBuilder2, ILanguageModelBuilder
	{
		IVariable5 AddLocalStackVariableToCompiledSignature(string stVariableName, VarFlag vf, ICompiledType ctype, IExpression expInit, ISignature sign);

		void SetType(IExpression exp, ICompiledType type);
	}
}
