using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0084
{
	// Token: 0x02000354 RID: 852
	internal static class \u001D
	{
		// Token: 0x06003336 RID: 13110 RVA: 0x000C6684 File Offset: 0x000C4884
		internal static void \u0001(_ICallExpression \u0002, ISignature \u0003, ISignature \u0004, _ICompileContext \u0005, IScope5 \u0006)
		{
			if (\u0003 != null && (\u0003.HasAttribute("init_inputs_on_onlchange") || \u0005.IsDefined("init_inputs_on_onlchange")))
			{
				_IVariable ivariable = \u0002._Callee.GetVariable(\u0006) as _IVariable;
				if (ivariable != null)
				{
					ivariable.SetAttributeValue("@callattribute", \u0002.ToString());
					return;
				}
				if (\u0004 != null && \u0004.POUType == Operator.Program)
				{
					(\u0004 as _ISignature).AddAttribute("@callattribute", \u0002.ToString());
				}
			}
		}
	}
}
