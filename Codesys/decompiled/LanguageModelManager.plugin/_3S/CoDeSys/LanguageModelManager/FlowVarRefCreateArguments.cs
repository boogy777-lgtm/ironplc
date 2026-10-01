using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.OnlineExpressionInterpreter;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000026 RID: 38
	internal class FlowVarRefCreateArguments : IOnlineVarRefCreateArguments
	{
		// Token: 0x060001AA RID: 426 RVA: 0x00005DDE File Offset: 0x00004DDE
		internal FlowVarRefCreateArguments(string stVariableName)
		{
			this._stName = stVariableName;
		}

		// Token: 0x060001AB RID: 427 RVA: 0x00005DED File Offset: 0x00004DED
		internal FlowVarRefCreateArguments(IExpression exp)
		{
			this._exp = exp;
		}

		// Token: 0x060001AC RID: 428 RVA: 0x00005DFC File Offset: 0x00004DFC
		public string GetVariableName()
		{
			return this._stName;
		}

		// Token: 0x060001AD RID: 429 RVA: 0x00005E04 File Offset: 0x00004E04
		public IExpression ParseExpression()
		{
			IScanner scanner = APEnvironmentFacade.Instance.LanguageModelMgr.CreateScanner(this._stName, false, false, false, false);
			return APEnvironmentFacade.Instance.LanguageModelMgr.CreateParser(scanner).ParseOperand();
		}

		// Token: 0x17000053 RID: 83
		// (get) Token: 0x060001AE RID: 430 RVA: 0x00005E40 File Offset: 0x00004E40
		public IExpression Expression
		{
			get
			{
				return this._exp;
			}
		}

		// Token: 0x0400003F RID: 63
		private readonly string _stName;

		// Token: 0x04000040 RID: 64
		private readonly IExpression _exp;
	}
}
