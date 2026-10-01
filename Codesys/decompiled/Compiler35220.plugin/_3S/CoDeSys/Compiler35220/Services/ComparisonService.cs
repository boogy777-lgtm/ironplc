using System;
using System.Collections.Generic;
using \u0015;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler35220.Services
{
	// Token: 0x020000D0 RID: 208
	public class ComparisonService : IComparisonService
	{
		// Token: 0x06000ED8 RID: 3800 RVA: 0x00028E2C File Offset: 0x0002702C
		public bool HaveEqualAttributes(IDictionary<string, string> attributes1, IDictionary<string, string> attributes2, IgnoreAttributes attributesToIgnore)
		{
			return new \u0001(attributesToIgnore).\u0001(attributes1, attributes2);
		}

		// Token: 0x06000ED9 RID: 3801 RVA: 0x00028E3C File Offset: 0x0002703C
		public bool IsEqual(_IVariable varLeft, _IVariable varRight, bool bCompareInitValues, bool bCompareAttributes, bool bCompiled, IScope scope, IPrecompileScope scopeThis, IPrecompileScope scopeParameter, ref bool bConstantArrayLimitOnlyQualifiedChanged)
		{
			return VariableComparer.IsEqual(varLeft, varRight, bCompareInitValues, bCompareAttributes, bCompiled, scope, scopeThis, scopeParameter, ref bConstantArrayLimitOnlyQualifiedChanged);
		}

		// Token: 0x06000EDA RID: 3802 RVA: 0x00028E60 File Offset: 0x00027060
		public bool InitialValueEquals(_IVariable left, _IVariable right, bool bCompiled)
		{
			return VariableComparer.InitialValueEquals(left, right, bCompiled);
		}
	}
}
