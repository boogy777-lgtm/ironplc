using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.GreenTrees
{
	// Token: 0x0200022C RID: 556
	internal class DefineStatement_Green : Statement_Green, _IDefineStatement, _IStatement, _IExprement, IExprement3, IExprement2, IExprement, IStatement, IDefineStatement
	{
		// Token: 0x06002465 RID: 9317 RVA: 0x0005C3AB File Offset: 0x0005B3AB
		public DefineStatement_Green(bool bDefine, string stIdent, string stValue)
		{
			this.m_bDefine = bDefine;
			this.m_stIdent = stIdent;
			this.m_stValue = stValue;
		}

		// Token: 0x17000A50 RID: 2640
		// (get) Token: 0x06002466 RID: 9318 RVA: 0x0005C3C8 File Offset: 0x0005B3C8
		// (set) Token: 0x06002467 RID: 9319 RVA: 0x0005A471 File Offset: 0x00059471
		public bool Define
		{
			get
			{
				return this.m_bDefine;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		// Token: 0x17000A51 RID: 2641
		// (get) Token: 0x06002468 RID: 9320 RVA: 0x0005C3D0 File Offset: 0x0005B3D0
		// (set) Token: 0x06002469 RID: 9321 RVA: 0x0005A471 File Offset: 0x00059471
		public string Ident
		{
			get
			{
				return this.m_stIdent;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		// Token: 0x17000A52 RID: 2642
		// (get) Token: 0x0600246A RID: 9322 RVA: 0x0005C3D8 File Offset: 0x0005B3D8
		// (set) Token: 0x0600246B RID: 9323 RVA: 0x0005A471 File Offset: 0x00059471
		public string Value
		{
			get
			{
				return this.m_stValue;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		// Token: 0x0600246C RID: 9324 RVA: 0x00014A8C File Offset: 0x00013A8C
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x0600246D RID: 9325 RVA: 0x00014A9E File Offset: 0x00013A9E
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x040006F8 RID: 1784
		private readonly bool m_bDefine;

		// Token: 0x040006F9 RID: 1785
		private readonly string m_stIdent;

		// Token: 0x040006FA RID: 1786
		private readonly string m_stValue;
	}
}
