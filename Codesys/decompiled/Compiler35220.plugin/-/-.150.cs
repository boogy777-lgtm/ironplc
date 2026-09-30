using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0015
{
	// Token: 0x020001A1 RID: 417
	internal static class \u0003
	{
		// Token: 0x06001DBA RID: 7610 RVA: 0x0006024C File Offset: 0x0005E44C
		internal static ICompiledType[] \u0001(_ICallExpression \u0002, ISignature \u0003)
		{
			IList<_IExpression> paramExpressions = \u0002.ParamExpressions;
			IList<_IExpression> inputs = \u0002.Inputs;
			IVariable[] array = null;
			if (\u0003 != null)
			{
				array = \u0003.AllInputs;
			}
			ICompiledType[] array2 = new ICompiledType[paramExpressions.Count];
			for (int i = 0; i < paramExpressions.Count; i++)
			{
				if (inputs.Count > i && inputs[i] != null)
				{
					array2[i] = inputs[i].Type;
				}
				else if (array != null && array.Length > i)
				{
					array2[i] = array[i].CompiledType;
				}
				else
				{
					array2[i] = null;
				}
			}
			return array2;
		}

		// Token: 0x06001DBB RID: 7611 RVA: 0x000602DC File Offset: 0x0005E4DC
		internal static IVariable[] \u0001(_ICallExpression \u0002, ISignature \u0003)
		{
			if (\u0003 == null)
			{
				throw new ArgumentNullException("sign");
			}
			if (\u0002 == null)
			{
				throw new ArgumentNullException("call");
			}
			IList<_IExpression> paramExpressions = \u0002.ParamExpressions;
			IList<_IExpression> inputs = \u0002.Inputs;
			IVariable[] allInputs = \u0003.AllInputs;
			IVariable[] array = new IVariable[paramExpressions.Count];
			for (int i = 0; i < paramExpressions.Count; i++)
			{
				if (inputs.Count > i && inputs[i] != null)
				{
					array[i] = \u0003[inputs[i].VariableId];
				}
				else if (allInputs != null && allInputs.Length > i)
				{
					array[i] = allInputs[i];
				}
				else
				{
					array[i] = null;
				}
			}
			return array;
		}

		// Token: 0x06001DBC RID: 7612 RVA: 0x00060384 File Offset: 0x0005E584
		internal static ICompiledType[] \u0002(_ICallExpression \u0002, ISignature \u0003)
		{
			IList<_IExpression> outputExpressions = \u0002.OutputExpressions;
			IList<_IExpression> outputs = \u0002.Outputs;
			ICompiledType[] array = new ICompiledType[outputExpressions.Count];
			for (int i = 0; i < outputExpressions.Count; i++)
			{
				if (outputExpressions[i] != null)
				{
					if (outputs.Count > i && outputs[i] != null)
					{
						array[i] = outputs[i].Type;
					}
					else
					{
						array[i] = null;
					}
				}
			}
			return array;
		}
	}
}
