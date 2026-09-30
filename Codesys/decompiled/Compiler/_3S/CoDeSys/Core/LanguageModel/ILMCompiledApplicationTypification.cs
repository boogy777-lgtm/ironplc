using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILMCompiledApplicationTypification
	{
		IExpressionTypifier CreateTypifier(IScope scope, bool bContributeToCompile, bool bInterpretPragmas);

		IExpressionTypifier CreateTypifier(int idSignature);

		IExpressionTypifier CreateTypifier(int idSignature, bool bContributeToCompile, bool bInterpretPragmas);

		IScope CreateGlobalScope();

		IScope CreateScope(int nIdLocal);

		IScope CreateScope(ISignature sign);

		IScope CreateScope(int nIdLocal, int nIdMethod);

		IScope CreateOnlineExpressionScope(int nIdLocal);
	}
}
