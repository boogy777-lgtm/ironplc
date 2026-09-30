using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.OnlineExpressionInterpreter;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000027 RID: 39
	internal class FlowVarRefCreateArgumentsFactory : IOnlineVarRefCreateArgumentsFactory2, IOnlineVarRefCreateArgumentsFactory
	{
		// Token: 0x060001AF RID: 431 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public void Initialize(Guid gdApplication)
		{
		}

		// Token: 0x060001B0 RID: 432 RVA: 0x00005E48 File Offset: 0x00004E48
		public IOnlineVarRefCreateArguments CreateInstance(string stVariable)
		{
			return new FlowVarRefCreateArguments(stVariable);
		}

		// Token: 0x060001B1 RID: 433 RVA: 0x00005E48 File Offset: 0x00004E48
		public IOnlineVarRefCreateArguments CreateInstance(string stExpression, ICompiledType resultType)
		{
			return new FlowVarRefCreateArguments(stExpression);
		}

		// Token: 0x060001B2 RID: 434 RVA: 0x00005E50 File Offset: 0x00004E50
		public IOnlineVarRefCreateArguments CreateInstance(IExpression exp)
		{
			return new FlowVarRefCreateArguments(exp);
		}
	}
}
