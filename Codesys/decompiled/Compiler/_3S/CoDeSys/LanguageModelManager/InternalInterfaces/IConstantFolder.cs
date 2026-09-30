using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface IConstantFolder
	{
		ILiteralValue GetLiteral(_IOperatorExpression opexp, ILiteralValue[] litvalOps, bool bPrecompile);
	}
}
