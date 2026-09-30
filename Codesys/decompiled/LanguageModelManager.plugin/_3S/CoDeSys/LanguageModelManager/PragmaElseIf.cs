using System;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x0200009C RID: 156
	[TypeGuid("{e15b6afd-87e6-4d01-8697-ba8ce1cd09ed}")]
	[StorageVersion("3.3.0.0")]
	public class PragmaElseIf : GenericObject2, _IPragmaElseIf
	{
		// Token: 0x06000973 RID: 2419 RVA: 0x0000AC39 File Offset: 0x00009C39
		public PragmaElseIf()
		{
		}

		// Token: 0x06000974 RID: 2420 RVA: 0x0001606C File Offset: 0x0001506C
		public PragmaElseIf(_IPragmaExpression expCondition, _IStatement stControlled)
		{
			this.m_expCondition = expCondition;
			this.m_stControlled = stControlled;
		}

		// Token: 0x1700022B RID: 555
		// (get) Token: 0x06000975 RID: 2421 RVA: 0x00016082 File Offset: 0x00015082
		// (set) Token: 0x06000976 RID: 2422 RVA: 0x00016098 File Offset: 0x00015098
		public _IExpression Condition
		{
			get
			{
				if (this.m_expCondition == null)
				{
					return new NullExpression();
				}
				return this.m_expCondition;
			}
			set
			{
				this.m_expCondition = value;
			}
		}

		// Token: 0x1700022C RID: 556
		// (get) Token: 0x06000977 RID: 2423 RVA: 0x000160A1 File Offset: 0x000150A1
		// (set) Token: 0x06000978 RID: 2424 RVA: 0x000160B7 File Offset: 0x000150B7
		public _IStatement Controlled
		{
			get
			{
				if (this.m_stControlled == null)
				{
					return new NullStatement();
				}
				return this.m_stControlled;
			}
			set
			{
				this.m_stControlled = value;
			}
		}

		// Token: 0x06000979 RID: 2425 RVA: 0x000160C0 File Offset: 0x000150C0
		public _IElseIf CreateElseIf()
		{
			return new ElseIf(this.m_expCondition, this.m_stControlled);
		}

		// Token: 0x0600097A RID: 2426 RVA: 0x000160D4 File Offset: 0x000150D4
		public _IPragmaElseIf Duplicate()
		{
			PragmaElseIf pragmaElseIf = new PragmaElseIf();
			if (this.m_expCondition != null)
			{
				pragmaElseIf.m_expCondition = (this.m_expCondition.Duplicate() as Expression);
			}
			if (this.m_stControlled != null)
			{
				pragmaElseIf.m_stControlled = (this.m_stControlled.Duplicate() as Statement);
			}
			return pragmaElseIf;
		}

		// Token: 0x0400014F RID: 335
		[DefaultSerialization("Condition")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private _IExpression m_expCondition;

		// Token: 0x04000150 RID: 336
		[DefaultSerialization("Controlled")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private _IStatement m_stControlled;
	}
}
