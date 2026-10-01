using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Online;
using _3S.CoDeSys.OnlineExpressionInterpreter;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000028 RID: 40
	internal class FlowVarRefProvider : IOnlineVarRefProvider3, IOnlineVarRefProvider2, IOnlineVarRefProvider
	{
		// Token: 0x17000054 RID: 84
		// (get) Token: 0x060001B4 RID: 436 RVA: 0x00005E58 File Offset: 0x00004E58
		public bool GenerateVarRefForCall
		{
			get
			{
				return true;
			}
		}

		// Token: 0x060001B5 RID: 437 RVA: 0x00005E5B File Offset: 0x00004E5B
		public FlowVarRefProvider(LList<Flowposition> flowpositions)
		{
			this._flowpositions = flowpositions;
		}

		// Token: 0x060001B6 RID: 438 RVA: 0x00005E6C File Offset: 0x00004E6C
		public IOnlineVarRef GetVarRef(IOnlineVarRefCreateArguments variable)
		{
			if (variable is FlowVarRefCreateArguments)
			{
				IExpression expression = (variable as FlowVarRefCreateArguments).Expression;
				Flowposition flowposition = null;
				for (int i = this._flowpositions.Count - 1; i >= 0; i--)
				{
					if (this._flowpositions[i].Position.PositionCombination == expression.Position.PositionCombination)
					{
						flowposition = this._flowpositions[i];
					}
				}
				if (flowposition == null)
				{
					return null;
				}
				try
				{
					return APEnvironmentFacade.Instance.CreateWatch(flowposition);
				}
				catch (Exception)
				{
					return null;
				}
			}
			return null;
		}

		// Token: 0x060001B7 RID: 439 RVA: 0x00005F08 File Offset: 0x00004F08
		public IOnlineVarRefCreateArgumentsFactory GetFactory()
		{
			return new FlowVarRefCreateArgumentsFactory();
		}

		// Token: 0x060001B8 RID: 440 RVA: 0x00005F0F File Offset: 0x00004F0F
		public IOnlineVarRef GetVarRef(string stVariable)
		{
			return null;
		}

		// Token: 0x060001B9 RID: 441 RVA: 0x00005F12 File Offset: 0x00004F12
		public object GetKeyOfExpression(IExpression exp)
		{
			return exp;
		}

		// Token: 0x04000041 RID: 65
		private readonly LList<Flowposition> _flowpositions;
	}
}
