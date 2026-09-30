using System;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x0200008C RID: 140
	[TypeGuid("{528063d1-3f0b-4579-88db-c76224f9c6af}")]
	[StorageVersion("3.3.0.0")]
	public class EnumDeclarationStatement : PositionStatement, _IEnumDeclarationStatement, _IStatement, _IExprement, IExprement3, IExprement2, IExprement, IStatement, IEnumDeclarationStatement
	{
		// Token: 0x060008BC RID: 2236 RVA: 0x00014EEC File Offset: 0x00013EEC
		public EnumDeclarationStatement()
		{
		}

		// Token: 0x060008BD RID: 2237 RVA: 0x00014EFF File Offset: 0x00013EFF
		public EnumDeclarationStatement(string stName, _IExpression expInit, _ISequenceStatement seqOptAttributesEtc)
		{
			this.Name = stName;
			this._Value = expInit;
		}

		// Token: 0x060008BE RID: 2238 RVA: 0x00014F20 File Offset: 0x00013F20
		public EnumDeclarationStatement(string stName, _IExpression expInit, _ISequenceStatement seqOptAttributesEtc, IToken token) : base(token)
		{
			this.Name = stName;
			this._Value = expInit;
			this.m_OptSeqAttributesEtc = seqOptAttributesEtc;
		}

		// Token: 0x170001F6 RID: 502
		// (get) Token: 0x060008BF RID: 2239 RVA: 0x00014F4A File Offset: 0x00013F4A
		// (set) Token: 0x060008C0 RID: 2240 RVA: 0x00014F52 File Offset: 0x00013F52
		public _IExpression _Value
		{
			get
			{
				return this.m_expValue;
			}
			set
			{
				this.m_expValue = value;
			}
		}

		// Token: 0x170001F7 RID: 503
		// (get) Token: 0x060008C1 RID: 2241 RVA: 0x00014F5B File Offset: 0x00013F5B
		// (set) Token: 0x060008C2 RID: 2242 RVA: 0x00014F63 File Offset: 0x00013F63
		public string Name
		{
			get
			{
				return this.m_stName;
			}
			set
			{
				this.m_stName = value;
			}
		}

		// Token: 0x170001F8 RID: 504
		// (get) Token: 0x060008C3 RID: 2243 RVA: 0x00014F6C File Offset: 0x00013F6C
		// (set) Token: 0x060008C4 RID: 2244 RVA: 0x00014F74 File Offset: 0x00013F74
		public _IStatement AttributesEtc
		{
			get
			{
				return this.m_OptSeqAttributesEtc;
			}
			set
			{
				this.m_OptSeqAttributesEtc = value;
			}
		}

		// Token: 0x060008C5 RID: 2245 RVA: 0x00014F7D File Offset: 0x00013F7D
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x060008C6 RID: 2246 RVA: 0x00014F86 File Offset: 0x00013F86
		public override T Accept<T>(IExprementVisitor<T> visitor)
		{
			return visitor.visit(this);
		}

		// Token: 0x060008C7 RID: 2247 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public override void AcceptVisitor(IExprVisitor visitor)
		{
		}

		// Token: 0x060008C8 RID: 2248 RVA: 0x00014F90 File Offset: 0x00013F90
		public override _IExprement Duplicate()
		{
			EnumDeclarationStatement enumDeclarationStatement = new EnumDeclarationStatement();
			this.DuplicateCommon(enumDeclarationStatement);
			enumDeclarationStatement.Name = this.Name;
			EnumDeclarationStatement enumDeclarationStatement2 = enumDeclarationStatement;
			_IExpression value = this._Value;
			enumDeclarationStatement2._Value = (((value != null) ? value.Duplicate() : null) as Expression);
			if (this.m_OptSeqAttributesEtc != null)
			{
				enumDeclarationStatement.m_OptSeqAttributesEtc = (this.m_OptSeqAttributesEtc.Duplicate() as Statement);
			}
			return enumDeclarationStatement;
		}

		// Token: 0x170001F9 RID: 505
		// (get) Token: 0x060008C9 RID: 2249 RVA: 0x00014F4A File Offset: 0x00013F4A
		public IExpression Value
		{
			get
			{
				return this.m_expValue;
			}
		}

		// Token: 0x0400012D RID: 301
		[DefaultSerialization("Name")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private string m_stName = string.Empty;

		// Token: 0x0400012E RID: 302
		[DefaultSerialization("Value")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private _IExpression m_expValue;

		// Token: 0x0400012F RID: 303
		[DefaultSerialization("AttrsEtc")]
		[StorageVersion("3.4.3.0")]
		[StorageIgnorable]
		[Obfuscation(Feature = "rename")]
		private _IStatement m_OptSeqAttributesEtc;
	}
}
