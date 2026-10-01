using System;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.GreenTrees
{
	// Token: 0x0200022A RID: 554
	internal class PragmaElseIf_Green : _IPragmaElseIf
	{
		// Token: 0x0600244F RID: 9295 RVA: 0x0005C249 File Offset: 0x0005B249
		public PragmaElseIf_Green(_IPragmaExpression expCondition, _IStatement stControlled)
		{
			this.m_expCondition = expCondition;
			this.m_stControlled = stControlled;
		}

		// Token: 0x17000A46 RID: 2630
		// (get) Token: 0x06002450 RID: 9296 RVA: 0x0005C25F File Offset: 0x0005B25F
		// (set) Token: 0x06002451 RID: 9297 RVA: 0x0005A471 File Offset: 0x00059471
		public _IExpression Condition
		{
			get
			{
				if (this.m_expCondition == null)
				{
					return new NullExpression_Green();
				}
				return this.m_expCondition;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		// Token: 0x17000A47 RID: 2631
		// (get) Token: 0x06002452 RID: 9298 RVA: 0x0005C275 File Offset: 0x0005B275
		// (set) Token: 0x06002453 RID: 9299 RVA: 0x0005A471 File Offset: 0x00059471
		public _IStatement Controlled
		{
			get
			{
				if (this.m_stControlled == null)
				{
					return new NullStatement_Green();
				}
				return this.m_stControlled;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		// Token: 0x06002454 RID: 9300 RVA: 0x0005C28B File Offset: 0x0005B28B
		public _IElseIf CreateElseIf()
		{
			return new ElseIf_Green(this.m_expCondition, this.m_stControlled);
		}

		// Token: 0x06002455 RID: 9301 RVA: 0x0005C2A0 File Offset: 0x0005B2A0
		public _IPragmaElseIf Duplicate()
		{
			_IPragmaExpression expCondition = null;
			_IStatement stControlled = null;
			if (this.m_expCondition != null)
			{
				expCondition = (this.m_expCondition.Duplicate() as _IPragmaExpression);
			}
			if (this.m_stControlled != null)
			{
				stControlled = (this.m_stControlled.Duplicate() as _IStatement);
			}
			return new PragmaElseIf_Green(expCondition, stControlled);
		}

		// Token: 0x040006F2 RID: 1778
		private readonly _IExpression m_expCondition;

		// Token: 0x040006F3 RID: 1779
		private readonly _IStatement m_stControlled;
	}
}
