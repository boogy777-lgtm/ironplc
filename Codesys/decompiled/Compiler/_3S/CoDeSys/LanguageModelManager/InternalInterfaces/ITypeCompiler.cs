using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface ITypeCompiler
	{
		_IType CheckType(_IType type, IScope scope, _ICompileContext comcon, ISourcePosition sp, _ISignature signDecl, ref IExpression expInitial);
	}
}
