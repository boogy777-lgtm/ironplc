using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.GreenTrees
{
	// Token: 0x020001E2 RID: 482
	internal class Case_Green : _ICase, ICase
	{
		// Token: 0x060021CF RID: 8655 RVA: 0x0005A70F File Offset: 0x0005970F
		internal Case_Green(_ICaseLabelStatement caselabel, _IStatement statement)
		{
			this.m_caselabel = caselabel;
			this.m_statement = statement;
		}

		// Token: 0x1700094F RID: 2383
		// (get) Token: 0x060021D0 RID: 8656 RVA: 0x0005A725 File Offset: 0x00059725
		public ICaseLabelStatement Label
		{
			get
			{
				return this._Label;
			}
		}

		// Token: 0x17000950 RID: 2384
		// (get) Token: 0x060021D1 RID: 8657 RVA: 0x0005A72D File Offset: 0x0005972D
		// (set) Token: 0x060021D2 RID: 8658 RVA: 0x0005A609 File Offset: 0x00059609
		public _ICaseLabelStatement _Label
		{
			get
			{
				return this.m_caselabel;
			}
			set
			{
				throw new NotSupportedException("no manipulation of green tree expressions");
			}
		}

		// Token: 0x17000951 RID: 2385
		// (get) Token: 0x060021D3 RID: 8659 RVA: 0x0005A735 File Offset: 0x00059735
		public IStatement Controlled
		{
			get
			{
				return this._Controlled;
			}
		}

		// Token: 0x17000952 RID: 2386
		// (get) Token: 0x060021D4 RID: 8660 RVA: 0x0005A73D File Offset: 0x0005973D
		// (set) Token: 0x060021D5 RID: 8661 RVA: 0x0005A609 File Offset: 0x00059609
		public _IStatement _Controlled
		{
			get
			{
				return this.m_statement;
			}
			set
			{
				throw new NotSupportedException("no manipulation of green tree expressions");
			}
		}

		// Token: 0x0400067B RID: 1659
		private readonly _ICaseLabelStatement m_caselabel;

		// Token: 0x0400067C RID: 1660
		private readonly _IStatement m_statement;
	}
}
