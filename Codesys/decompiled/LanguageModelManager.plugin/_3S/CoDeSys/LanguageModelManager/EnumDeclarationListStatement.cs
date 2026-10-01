using System;
using System.Collections.Generic;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x0200008B RID: 139
	[TypeGuid("{a4365e9b-6fe4-415b-bb12-35385b301a2f}")]
	[StorageVersion("3.3.0.0")]
	public class EnumDeclarationListStatement : PositionStatement, _IEnumDeclarationListStatement, _IStatement, _IExprement, IExprement3, IExprement2, IExprement, IStatement, IEnumDeclarationListStatement
	{
		// Token: 0x060008AC RID: 2220 RVA: 0x00014CF3 File Offset: 0x00013CF3
		public EnumDeclarationListStatement()
		{
		}

		// Token: 0x060008AD RID: 2221 RVA: 0x00014D11 File Offset: 0x00013D11
		internal EnumDeclarationListStatement(IToken token) : base(token)
		{
		}

		// Token: 0x170001F1 RID: 497
		// (get) Token: 0x060008AE RID: 2222 RVA: 0x00014D30 File Offset: 0x00013D30
		public ICollection<_IEnumDeclarationStatement> Enums
		{
			get
			{
				return this.m_alEnumDeclarations;
			}
		}

		// Token: 0x060008AF RID: 2223 RVA: 0x00014D38 File Offset: 0x00013D38
		public void AddEnumDeclaration(string stName, _IExpression expInit, _ISequenceStatement seqOptAttributesEtc, IToken token)
		{
			this.m_alEnumDeclarations.Add(new EnumDeclarationStatement(stName, expInit, seqOptAttributesEtc, token));
		}

		// Token: 0x060008B0 RID: 2224 RVA: 0x00014D4F File Offset: 0x00013D4F
		public void AddEnumDeclaration(_IEnumDeclarationStatement eds)
		{
			this.m_alEnumDeclarations.Add(eds as EnumDeclarationStatement);
		}

		// Token: 0x060008B1 RID: 2225 RVA: 0x00014D62 File Offset: 0x00013D62
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x060008B2 RID: 2226 RVA: 0x00014D6B File Offset: 0x00013D6B
		public override T Accept<T>(IExprementVisitor<T> visitor)
		{
			return visitor.visit(this);
		}

		// Token: 0x060008B3 RID: 2227 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public override void AcceptVisitor(IExprVisitor visitor)
		{
		}

		// Token: 0x060008B4 RID: 2228 RVA: 0x00014D74 File Offset: 0x00013D74
		public override _IExprement Duplicate()
		{
			EnumDeclarationListStatement enumDeclarationListStatement = new EnumDeclarationListStatement();
			this.DuplicateCommon(enumDeclarationListStatement);
			foreach (_IEnumDeclarationStatement ienumDeclarationStatement in this.m_alEnumDeclarations)
			{
				enumDeclarationListStatement.AddEnumDeclaration(ienumDeclarationStatement.Duplicate() as _IEnumDeclarationStatement);
			}
			if (this.m_defaultValue != null)
			{
				enumDeclarationListStatement.m_defaultValue = (this.m_defaultValue.Duplicate() as _IVariableExpression);
			}
			EnumDeclarationListStatement enumDeclarationListStatement2 = enumDeclarationListStatement;
			_IType typeBase = this.m_typeBase;
			enumDeclarationListStatement2.m_typeBase = ((typeBase != null) ? typeBase._Duplicate(false) : null);
			return enumDeclarationListStatement;
		}

		// Token: 0x170001F2 RID: 498
		// (get) Token: 0x060008B5 RID: 2229 RVA: 0x00014E10 File Offset: 0x00013E10
		// (set) Token: 0x060008B6 RID: 2230 RVA: 0x00014E18 File Offset: 0x00013E18
		public _IType _BaseType
		{
			get
			{
				return this.m_typeBase;
			}
			set
			{
				this.m_typeBase = value;
			}
		}

		// Token: 0x170001F3 RID: 499
		// (get) Token: 0x060008B7 RID: 2231 RVA: 0x00014E21 File Offset: 0x00013E21
		// (set) Token: 0x060008B8 RID: 2232 RVA: 0x00014E29 File Offset: 0x00013E29
		public _IVariableExpression _DefaultValue
		{
			get
			{
				return this.m_defaultValue;
			}
			set
			{
				this.m_defaultValue = value;
			}
		}

		// Token: 0x170001F4 RID: 500
		// (get) Token: 0x060008B9 RID: 2233 RVA: 0x00014E34 File Offset: 0x00013E34
		// (set) Token: 0x060008BA RID: 2234 RVA: 0x00014E88 File Offset: 0x00013E88
		public List<IEnumDeclarationStatement> Enumerations
		{
			get
			{
				List<IEnumDeclarationStatement> list = new List<IEnumDeclarationStatement>();
				foreach (IEnumDeclarationStatement item in this.m_alEnumDeclarations)
				{
					list.Add(item);
				}
				return list;
			}
			set
			{
				this.m_alEnumDeclarations = new LList<_IEnumDeclarationStatement>();
				foreach (IEnumDeclarationStatement enumDeclarationStatement in value)
				{
					this.m_alEnumDeclarations.Add(enumDeclarationStatement as _IEnumDeclarationStatement);
				}
			}
		}

		// Token: 0x170001F5 RID: 501
		// (get) Token: 0x060008BB RID: 2235 RVA: 0x00014E10 File Offset: 0x00013E10
		public IType BaseType
		{
			get
			{
				return this.m_typeBase;
			}
		}

		// Token: 0x0400012A RID: 298
		[DefaultSerialization("BaseType")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private _IType m_typeBase = TypeTable.Int;

		// Token: 0x0400012B RID: 299
		[DefaultSerialization("Declarations")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private LList<_IEnumDeclarationStatement> m_alEnumDeclarations = new LList<_IEnumDeclarationStatement>();

		// Token: 0x0400012C RID: 300
		[DefaultSerialization("DefaultValue")]
		[StorageVersion("3.5.14.0")]
		[StorageDefaultValue(null)]
		private _IVariableExpression m_defaultValue;
	}
}
