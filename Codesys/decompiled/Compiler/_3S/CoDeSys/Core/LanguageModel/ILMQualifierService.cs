using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILMQualifierService
	{
		IType GetQualifiedType(IType type, ILMPreCompileSet precomSource, ILMPreCompileSet precomDest);

		IExpression GetQualifiedExpression(IExpression expressionToQualify, ILMPreCompileSet precomSource, ILMPreCompileSet precomDest);

		IExpression GetQualifiedExpression(IExpression expressionToQualify, string stQualificationNamespace);
	}
}
