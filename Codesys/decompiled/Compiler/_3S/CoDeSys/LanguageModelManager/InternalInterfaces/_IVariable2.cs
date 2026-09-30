using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _IVariable2 : _IVariable, IVariable5, IVariable4, IVariable3, IVariable2, IVariable
	{
		new VarFlag Flags { get; set; }

		bool IsEqual(IVariable varRight, bool bCompareInitValues, bool bCompareAttributes, bool bCompiled, IScope scope, IPrecompileScope scopeThis, IPrecompileScope scopeParameter, ref bool bConstantArrayLimitOnlyQualifiedChanged);
	}
}
