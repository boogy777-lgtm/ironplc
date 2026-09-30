using System;
using System.Reflection;
using SmartAssembly.Attributes;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000080 RID: 128
	[TypeGuid("{4e6fde22-9719-4174-85d0-65453dc21de7}")]
	[StorageVersion("3.3.0.0")]
	public abstract class Statement : Exprement, _IStatement2, _IStatement, _IExprement, IExprement3, IExprement2, IExprement, IStatement
	{
		// Token: 0x0600082F RID: 2095 RVA: 0x0000E573 File Offset: 0x0000D573
		protected Statement()
		{
		}

		// Token: 0x06000830 RID: 2096 RVA: 0x0000E57B File Offset: 0x0000D57B
		protected Statement(IToken token) : base(token)
		{
		}

		// Token: 0x06000831 RID: 2097 RVA: 0x00014410 File Offset: 0x00013410
		public bool GetFlag(StatementFlag sfFlag)
		{
			InternalStatementProperties myBy = this.GetMyBy(sfFlag);
			return (this.m_stateFlag & myBy) == myBy;
		}

		// Token: 0x06000832 RID: 2098 RVA: 0x00014430 File Offset: 0x00013430
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

		// Token: 0x06000833 RID: 2099 RVA: 0x00014468 File Offset: 0x00013468
		public override void DuplicateCommon(Exprement exprem)
		{
			base.DuplicateCommon(exprem);
			Statement statement = exprem as Statement;
			if (statement == null)
			{
				return;
			}
			statement.m_stateFlag = this.m_stateFlag;
		}

		// Token: 0x06000834 RID: 2100 RVA: 0x00014493 File Offset: 0x00013493
		private InternalStatementProperties GetMyBy(StatementFlag state)
		{
			return (InternalStatementProperties)state;
		}

		// Token: 0x06000835 RID: 2101 RVA: 0x00014497 File Offset: 0x00013497
		[ObfuscateControlFlow]
		public override string ToString()
		{
			if (this.GetFlag(StatementFlag.Library))
			{
				CodeAccessSecurity.AssertCallerHasKeyFlag("Decompile", 2);
			}
			return base.ToString();
		}

		// Token: 0x170001D1 RID: 465
		// (get) Token: 0x06000836 RID: 2102 RVA: 0x000144B4 File Offset: 0x000134B4
		// (set) Token: 0x06000837 RID: 2103 RVA: 0x000144BD File Offset: 0x000134BD
		[DefaultSerialization("StatementFlags")]
		[StorageVersion("3.3.0.0")]
		public StatementFlag flagsToSave
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

		// Token: 0x170001D2 RID: 466
		// (get) Token: 0x06000838 RID: 2104 RVA: 0x000144B4 File Offset: 0x000134B4
		// (set) Token: 0x06000839 RID: 2105 RVA: 0x000144BD File Offset: 0x000134BD
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

		// Token: 0x04000118 RID: 280
		[Obfuscation(Feature = "rename")]
		protected InternalStatementProperties m_stateFlag;
	}
}
