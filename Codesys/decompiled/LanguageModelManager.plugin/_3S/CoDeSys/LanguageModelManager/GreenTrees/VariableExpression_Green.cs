using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.GreenTrees
{
	// Token: 0x02000209 RID: 521
	internal class VariableExpression_Green : Expression_Green, _IVariableExpression, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression, IVariableExpression3, IVariableExpression2, IVariableExpression, IQualifiedNameExpression
	{
		// Token: 0x06002351 RID: 9041 RVA: 0x0005BB17 File Offset: 0x0005AB17
		public VariableExpression_Green(string stName)
		{
			this.m_stName = stName;
		}

		// Token: 0x170009EB RID: 2539
		// (get) Token: 0x06002352 RID: 9042 RVA: 0x0005BB26 File Offset: 0x0005AB26
		// (set) Token: 0x06002353 RID: 9043 RVA: 0x0005A471 File Offset: 0x00059471
		public string Name
		{
			get
			{
				return this.m_stName;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		// Token: 0x170009EC RID: 2540
		// (get) Token: 0x06002354 RID: 9044 RVA: 0x0000E8C4 File Offset: 0x0000D8C4
		public string Namespace
		{
			get
			{
				return string.Empty;
			}
		}

		// Token: 0x170009ED RID: 2541
		// (get) Token: 0x06002355 RID: 9045 RVA: 0x0005A471 File Offset: 0x00059471
		// (set) Token: 0x06002356 RID: 9046 RVA: 0x0005A471 File Offset: 0x00059471
		public int ScopeId
		{
			get
			{
				throw new NotSupportedException();
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		// Token: 0x170009EE RID: 2542
		// (get) Token: 0x06002357 RID: 9047 RVA: 0x0005A471 File Offset: 0x00059471
		// (set) Token: 0x06002358 RID: 9048 RVA: 0x0005A471 File Offset: 0x00059471
		public IVariableExprInfo VarInfo
		{
			get
			{
				throw new NotSupportedException();
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		// Token: 0x170009EF RID: 2543
		// (get) Token: 0x06002359 RID: 9049 RVA: 0x0005A471 File Offset: 0x00059471
		// (set) Token: 0x0600235A RID: 9050 RVA: 0x0005A471 File Offset: 0x00059471
		public bool NoVirtual
		{
			get
			{
				throw new NotSupportedException();
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		// Token: 0x170009F0 RID: 2544
		// (get) Token: 0x0600235B RID: 9051 RVA: 0x0005A471 File Offset: 0x00059471
		int IVariableExpression.ScopeId
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		// Token: 0x0600235C RID: 9052 RVA: 0x00013429 File Offset: 0x00012429
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x0600235D RID: 9053 RVA: 0x0001343B File Offset: 0x0001243B
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x0600235E RID: 9054 RVA: 0x0005BB2E File Offset: 0x0005AB2E
		public override string ToString()
		{
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV34100)
			{
				return this.m_stName;
			}
			return base.ToString();
		}

		// Token: 0x0600235F RID: 9055 RVA: 0x0000677E File Offset: 0x0000577E
		public bool GetFlag(VarExprFlag vfFlag)
		{
			throw new NotImplementedException();
		}

		// Token: 0x06002360 RID: 9056 RVA: 0x0000677E File Offset: 0x0000577E
		public void SetFlag(VarExprFlag vfFlag, bool bSetTrue)
		{
			throw new NotImplementedException();
		}

		// Token: 0x06002361 RID: 9057 RVA: 0x0000677E File Offset: 0x0000577E
		public ISignature GetSignature(IPrecompileScope scope)
		{
			throw new NotImplementedException();
		}

		// Token: 0x06002362 RID: 9058 RVA: 0x0000677E File Offset: 0x0000577E
		public IScope GetScope(IScope scope)
		{
			throw new NotImplementedException();
		}

		// Token: 0x040006C5 RID: 1733
		private readonly string m_stName;
	}
}
