using System;
using SmartAssembly.Attributes;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.GreenTrees
{
	// Token: 0x020001DA RID: 474
	internal abstract class Statement_Green : Exprement_Green, _IStatement2, _IStatement, _IExprement, IExprement3, IExprement2, IExprement, IStatement
	{
		// Token: 0x06002179 RID: 8569 RVA: 0x0005A520 File Offset: 0x00059520
		public bool GetFlag(StatementFlag sfFlag)
		{
			InternalStatementProperties myBy = this.GetMyBy(sfFlag);
			return (this.m_stateFlag & myBy) == myBy;
		}

		// Token: 0x0600217A RID: 8570 RVA: 0x0005A540 File Offset: 0x00059540
		public void SetFlag(StatementFlag sfFlag, bool bSetTrue)
		{
			InternalStatementProperties myBy = this.GetMyBy(sfFlag);
			if (bSetTrue)
			{
				this.m_stateFlag |= myBy;
				return;
			}
			this.m_stateFlag &= ~myBy;
		}

		// Token: 0x0600217B RID: 8571 RVA: 0x00014493 File Offset: 0x00013493
		private InternalStatementProperties GetMyBy(StatementFlag state)
		{
			return (InternalStatementProperties)state;
		}

		// Token: 0x0600217C RID: 8572 RVA: 0x0005A577 File Offset: 0x00059577
		[ObfuscateControlFlow]
		public override string ToString()
		{
			if (this.GetFlag(StatementFlag.Library))
			{
				CodeAccessSecurity.AssertCallerHasKeyFlag("Decompile", 2);
			}
			return base.ToString();
		}

		// Token: 0x17000932 RID: 2354
		// (get) Token: 0x0600217D RID: 8573 RVA: 0x0005A594 File Offset: 0x00059594
		// (set) Token: 0x0600217E RID: 8574 RVA: 0x0005A59D File Offset: 0x0005959D
		public StatementFlag Flags
		{
			get
			{
				return (StatementFlag)((ulong)this.m_stateFlag);
			}
			set
			{
				this.m_stateFlag = this.GetMyBy(value);
			}
		}

		// Token: 0x04000673 RID: 1651
		protected InternalStatementProperties m_stateFlag;
	}
}
