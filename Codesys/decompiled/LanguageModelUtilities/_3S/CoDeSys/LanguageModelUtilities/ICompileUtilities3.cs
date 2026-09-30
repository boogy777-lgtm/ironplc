using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[ReleasedInterface]
	public interface ICompileUtilities3 : ICompileUtilities2, ICompileUtilities
	{
		ISignature GetCalledSignatureForStackVariable(IVarRef2 varRef, out IExpression callingExpression);
	}
}
