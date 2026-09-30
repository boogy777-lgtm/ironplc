using System;
using System.Collections.Generic;
using _3S.CoDeSys.Compiler35220.Phase5_Codegeneration.Optimization;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u000F
{
	// Token: 0x0200025D RID: 605
	internal static class \u0011
	{
		// Token: 0x0600274F RID: 10063 RVA: 0x000875CC File Offset: 0x000857CC
		internal static CallParameterEnumerable \u0001(this _ICallExpression \u0002)
		{
			IList<_IExpression> inputs = \u0002.Inputs;
			return new CallParameterEnumerable(\u0002.ParamExpressions, inputs);
		}

		// Token: 0x06002750 RID: 10064 RVA: 0x000875EC File Offset: 0x000857EC
		internal static CallOutputParameterEnumerable \u0001(this _ICallExpression \u0002)
		{
			IList<_IExpression> outputExpressions = \u0002.OutputExpressions;
			IList<_IExpression> outputs = \u0002.Outputs;
			return new CallOutputParameterEnumerable(outputExpressions, outputs);
		}
	}
}
