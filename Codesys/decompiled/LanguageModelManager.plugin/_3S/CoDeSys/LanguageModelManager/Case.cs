using System;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000082 RID: 130
	[TypeGuid("{27279b56-9257-4ca1-8e64-4c13542bae17}")]
	[StorageVersion("3.3.0.0")]
	public class Case : GenericObject2, _ICase, ICase
	{
		// Token: 0x06000844 RID: 2116 RVA: 0x0000AC39 File Offset: 0x00009C39
		public Case()
		{
		}

		// Token: 0x06000845 RID: 2117 RVA: 0x00014587 File Offset: 0x00013587
		internal Case(_ICaseLabelStatement caselabel, _IStatement statement)
		{
			this.m_caselabel = caselabel;
			this.m_statement = statement;
		}

		// Token: 0x170001D5 RID: 469
		// (get) Token: 0x06000846 RID: 2118 RVA: 0x0001459D File Offset: 0x0001359D
		public ICaseLabelStatement Label
		{
			get
			{
				return this._Label;
			}
		}

		// Token: 0x170001D6 RID: 470
		// (get) Token: 0x06000847 RID: 2119 RVA: 0x000145A5 File Offset: 0x000135A5
		public _ICaseLabelStatement _Label
		{
			get
			{
				return this.m_caselabel;
			}
		}

		// Token: 0x170001D7 RID: 471
		// (get) Token: 0x06000848 RID: 2120 RVA: 0x000145AD File Offset: 0x000135AD
		public IStatement Controlled
		{
			get
			{
				return this._Controlled;
			}
		}

		// Token: 0x170001D8 RID: 472
		// (get) Token: 0x06000849 RID: 2121 RVA: 0x000145B5 File Offset: 0x000135B5
		public _IStatement _Controlled
		{
			get
			{
				return this.m_statement;
			}
		}

		// Token: 0x0600084A RID: 2122 RVA: 0x000145C0 File Offset: 0x000135C0
		public Case Duplicate()
		{
			Case @case = new Case();
			if (this.m_caselabel != null)
			{
				@case.m_caselabel = (this.m_caselabel.Duplicate() as CaseLabelStatement);
			}
			if (this.m_statement != null)
			{
				@case.m_statement = (this.m_statement.Duplicate() as Statement);
			}
			return @case;
		}

		// Token: 0x0400011B RID: 283
		[DefaultSerialization("CaseLabel")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private _ICaseLabelStatement m_caselabel;

		// Token: 0x0400011C RID: 284
		[DefaultSerialization("Statement")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private _IStatement m_statement;
	}
}
