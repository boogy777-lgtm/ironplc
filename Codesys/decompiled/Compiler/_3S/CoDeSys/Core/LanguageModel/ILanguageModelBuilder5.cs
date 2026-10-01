using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILanguageModelBuilder5 : ILanguageModelBuilder4, ILanguageModelBuilder3, ILanguageModelBuilder2, ILanguageModelBuilder
	{
		IExpression ParseExpression(IExprementPosition pos, string stExpression, bool allowImplicit, bool bGenerateErrorForAdditionalToken, out string stRestText);
	}
}
