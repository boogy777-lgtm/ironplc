using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _IArrayType2 : _IArrayType, _IType, ICompiledType5, ICompiledType4, ICompiledType3, ICompiledType2, ICompiledType, IType, IArchivable, IArrayType2, IArrayType
	{
		bool IsEqual(ICompiledType itype, IScope scope, bool bCompiled, IPrecompileScope scopeThis, IPrecompileScope scopeParameter, ref bool bConstantArrayLimitOnlyQualifiedChanged);
	}
}
