using System;
using System.Collections.Generic;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using \u0084;

namespace \u001F
{
	// Token: 0x02000303 RID: 771
	internal sealed class \u000F : \u001A
	{
		// Token: 0x06002EFC RID: 12028 RVA: 0x000B10F0 File Offset: 0x000AF2F0
		public \u000F(_ICallExpression \u009F\u0007)
		{
			this.\u0001 = \u009F\u0007;
		}

		// Token: 0x06002EFD RID: 12029 RVA: 0x000B1100 File Offset: 0x000AF300
		public IVariable \u0001(_ISignature \u0002, int \u0003)
		{
			if (this.\u0001.Inputs[\u0003] != null)
			{
				return \u0002[this.\u0001.Inputs[\u0003].ToString()];
			}
			IVariable[] allInputs = \u0002.AllInputs;
			if (\u0003 >= allInputs.Length)
			{
				return null;
			}
			return allInputs[\u0003];
		}

		// Token: 0x06002EFE RID: 12030 RVA: 0x000B1150 File Offset: 0x000AF350
		public IEnumerable<_IExpression> \u0001()
		{
			return this.\u0001.ParamExpressions;
		}

		// Token: 0x06002EFF RID: 12031 RVA: 0x000B1160 File Offset: 0x000AF360
		public bool \u0001(CaseInsensitiveDictionary<IVariable> \u0002, _ISignature \u0003)
		{
			bool result = false;
			foreach (IVariable variable in \u0003.AllInputs)
			{
				if (!(variable.Name == "__INSTANCEPOINTER") && !variable.HasAttribute(CompileAttributes.ATTRIBUTE_IMPLICIT_INPUT) && variable.Initial == null && !\u0002.ContainsKey(variable.Name))
				{
					result = true;
				}
			}
			return result;
		}

		// Token: 0x040008F3 RID: 2291
		private readonly _ICallExpression \u0001;
	}
}
