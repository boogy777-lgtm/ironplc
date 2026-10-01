using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface IComparisonService
	{
		bool HaveEqualAttributes(IDictionary<string, string> attributes1, IDictionary<string, string> attributes2, IgnoreAttributes attributesToIgnore);

		bool IsEqual(_IVariable varLeft, _IVariable varRight, bool bCompareInitValues, bool bCompareAttributes, bool bCompiled, IScope scope, IPrecompileScope scopeThis, IPrecompileScope scopeParameter, ref bool bConstantArrayLimitOnlyQualifiedChanged);

		bool InitialValueEquals(_IVariable left, _IVariable right, bool bCompiled);
	}
}
