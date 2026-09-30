using System;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x020000A4 RID: 164
	[TypeGuid("{5c4aac53-6e3d-4e71-b167-59708a033072}")]
	[StorageVersion("3.3.0.0")]
	public class TypeDeclarationStatement : PositionStatement, _ITypeDeclarationStatement, _IStatement, _IExprement, IExprement3, IExprement2, IExprement, IStatement, ITypeDeclarationStatement
	{
		// Token: 0x060009DC RID: 2524 RVA: 0x00016A43 File Offset: 0x00015A43
		public TypeDeclarationStatement()
		{
		}

		// Token: 0x060009DD RID: 2525 RVA: 0x00016A56 File Offset: 0x00015A56
		internal TypeDeclarationStatement(IToken token) : base(token)
		{
		}

		// Token: 0x1700024D RID: 589
		// (get) Token: 0x060009DE RID: 2526 RVA: 0x00016A6A File Offset: 0x00015A6A
		// (set) Token: 0x060009DF RID: 2527 RVA: 0x00016A72 File Offset: 0x00015A72
		public _IType Type
		{
			get
			{
				return this.m_type;
			}
			set
			{
				this.m_type = value;
			}
		}

		// Token: 0x1700024E RID: 590
		// (get) Token: 0x060009E0 RID: 2528 RVA: 0x00016A7B File Offset: 0x00015A7B
		// (set) Token: 0x060009E1 RID: 2529 RVA: 0x00016A83 File Offset: 0x00015A83
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

		// Token: 0x1700024F RID: 591
		// (get) Token: 0x060009E2 RID: 2530 RVA: 0x00016A8C File Offset: 0x00015A8C
		// (set) Token: 0x060009E3 RID: 2531 RVA: 0x00016AAD File Offset: 0x00015AAD
		public _IExpression NameExpression
		{
			get
			{
				if (this.m_nameExpression == null)
				{
					this.m_nameExpression = new VariableExpression(this.m_stName);
				}
				return this.m_nameExpression;
			}
			set
			{
				this.m_nameExpression = value;
			}
		}

		// Token: 0x17000250 RID: 592
		// (get) Token: 0x060009E4 RID: 2532 RVA: 0x00016AB6 File Offset: 0x00015AB6
		// (set) Token: 0x060009E5 RID: 2533 RVA: 0x00016ABE File Offset: 0x00015ABE
		public _IExpression Extends
		{
			get
			{
				return this.m_qneExtends;
			}
			set
			{
				this.m_qneExtends = value;
			}
		}

		// Token: 0x17000251 RID: 593
		// (get) Token: 0x060009E6 RID: 2534 RVA: 0x00016AC7 File Offset: 0x00015AC7
		// (set) Token: 0x060009E7 RID: 2535 RVA: 0x00016ACF File Offset: 0x00015ACF
		public _IExpression Initial
		{
			get
			{
				return this.m_expInitial;
			}
			set
			{
				this.m_expInitial = value;
			}
		}

		// Token: 0x17000252 RID: 594
		// (get) Token: 0x060009E8 RID: 2536 RVA: 0x00016AD8 File Offset: 0x00015AD8
		// (set) Token: 0x060009E9 RID: 2537 RVA: 0x00016AEE File Offset: 0x00015AEE
		public _IStatement Declarations
		{
			get
			{
				if (this.m_stateDecl == null)
				{
					return new NullStatement();
				}
				return this.m_stateDecl;
			}
			set
			{
				this.m_stateDecl = value;
			}
		}

		// Token: 0x17000253 RID: 595
		// (get) Token: 0x060009EA RID: 2538 RVA: 0x00016AF7 File Offset: 0x00015AF7
		// (set) Token: 0x060009EB RID: 2539 RVA: 0x00016AFF File Offset: 0x00015AFF
		public new SignatureFlag Flags
		{
			get
			{
				return this.m_sfFlags;
			}
			set
			{
				this.m_sfFlags = value;
			}
		}

		// Token: 0x060009EC RID: 2540 RVA: 0x00016B08 File Offset: 0x00015B08
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x060009ED RID: 2541 RVA: 0x00016B11 File Offset: 0x00015B11
		public override T Accept<T>(IExprementVisitor<T> visitor)
		{
			return visitor.visit(this);
		}

		// Token: 0x060009EE RID: 2542 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public override void AcceptVisitor(IExprVisitor visitor)
		{
		}

		// Token: 0x060009EF RID: 2543 RVA: 0x00016B1C File Offset: 0x00015B1C
		public override _IExprement Duplicate()
		{
			TypeDeclarationStatement typeDeclarationStatement = new TypeDeclarationStatement();
			this.DuplicateCommon(typeDeclarationStatement);
			typeDeclarationStatement.Type = this.Type;
			typeDeclarationStatement.Flags = this.Flags;
			typeDeclarationStatement.Declarations = (this.Declarations.Duplicate() as Statement);
			if (this.m_expInitial != null)
			{
				typeDeclarationStatement.Initial = (this.m_expInitial.Duplicate() as Expression);
			}
			typeDeclarationStatement.m_stName = this.m_stName;
			typeDeclarationStatement.m_nameExpression = this.m_nameExpression;
			if (this.m_defaultValue != null)
			{
				typeDeclarationStatement.m_defaultValue = (this.m_defaultValue.Duplicate() as Expression);
			}
			TypeDeclarationStatement typeDeclarationStatement2 = typeDeclarationStatement;
			_IExpression qneExtends = this.m_qneExtends;
			typeDeclarationStatement2.m_qneExtends = (((qneExtends != null) ? qneExtends.Duplicate() : null) as _IExpression);
			return typeDeclarationStatement;
		}

		// Token: 0x17000254 RID: 596
		// (get) Token: 0x060009F0 RID: 2544 RVA: 0x00016BD6 File Offset: 0x00015BD6
		// (set) Token: 0x060009F1 RID: 2545 RVA: 0x00016BDE File Offset: 0x00015BDE
		public _IExpression _DefaultValue
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

		// Token: 0x17000255 RID: 597
		// (get) Token: 0x060009F2 RID: 2546 RVA: 0x00016A6A File Offset: 0x00015A6A
		public IType AliasType
		{
			get
			{
				return this.m_type;
			}
		}

		// Token: 0x17000256 RID: 598
		// (get) Token: 0x060009F3 RID: 2547 RVA: 0x00016AB6 File Offset: 0x00015AB6
		public IExpression BaseType
		{
			get
			{
				return this.m_qneExtends;
			}
		}

		// Token: 0x17000257 RID: 599
		// (get) Token: 0x060009F4 RID: 2548 RVA: 0x00016AC7 File Offset: 0x00015AC7
		public IExpression InitialValue
		{
			get
			{
				return this.m_expInitial;
			}
		}

		// Token: 0x17000258 RID: 600
		// (get) Token: 0x060009F5 RID: 2549 RVA: 0x00016BE7 File Offset: 0x00015BE7
		public IVariableDeclarationListStatement StructDeclarationList
		{
			get
			{
				return this.m_stateDecl as IVariableDeclarationListStatement;
			}
		}

		// Token: 0x17000259 RID: 601
		// (get) Token: 0x060009F6 RID: 2550 RVA: 0x00016BF4 File Offset: 0x00015BF4
		public IEnumDeclarationListStatement EnumDeclarationList
		{
			get
			{
				return this.m_stateDecl as IEnumDeclarationListStatement;
			}
		}

		// Token: 0x04000163 RID: 355
		[DefaultSerialization("Type")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private _IType m_type;

		// Token: 0x04000164 RID: 356
		[DefaultSerialization("Name")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private string m_stName = string.Empty;

		// Token: 0x04000165 RID: 357
		[DefaultSerialization("Decl")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private _IStatement m_stateDecl;

		// Token: 0x04000166 RID: 358
		[DefaultSerialization("SignatureFlag")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private SignatureFlag m_sfFlags;

		// Token: 0x04000167 RID: 359
		[DefaultSerialization("Initial")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private _IExpression m_expInitial;

		// Token: 0x04000168 RID: 360
		[DefaultSerialization("Base")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private _IExpression m_qneExtends;

		// Token: 0x04000169 RID: 361
		[DefaultSerialization("NameExpression")]
		[StorageVersion("3.5.7.0")]
		[StorageIgnorable]
		[Obfuscation(Feature = "rename")]
		private _IExpression m_nameExpression;

		// Token: 0x0400016A RID: 362
		[DefaultSerialization("DefaultValue")]
		[StorageVersion("3.5.14.0")]
		[StorageDefaultValue(null)]
		private _IExpression m_defaultValue;
	}
}
