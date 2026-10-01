using System;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000055 RID: 85
	[TypeGuid("{094cb1fa-837a-431b-ae69-cb3b8b003b3f}")]
	[StorageVersion("3.3.0.0")]
	public class HasAttributeExpression : PragmaExpression, _IHasAttributeExpression, _IPragmaExpression, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression, IHasAttributeExpression
	{
		// Token: 0x06000510 RID: 1296 RVA: 0x0000C6EA File Offset: 0x0000B6EA
		public HasAttributeExpression()
		{
		}

		// Token: 0x06000511 RID: 1297 RVA: 0x0000C6F2 File Offset: 0x0000B6F2
		public HasAttributeExpression(IToken token) : base(token)
		{
		}

		// Token: 0x06000512 RID: 1298 RVA: 0x0000ECE7 File Offset: 0x0000DCE7
		public HasAttributeExpression(IToken token, _IItemReference itref, string stAttribute) : base(token)
		{
			this.m_itref = (itref as ItemReference);
			this.m_stAttribute = stAttribute;
		}

		// Token: 0x1700010F RID: 271
		// (get) Token: 0x06000513 RID: 1299 RVA: 0x0000ED03 File Offset: 0x0000DD03
		// (set) Token: 0x06000514 RID: 1300 RVA: 0x0000ED19 File Offset: 0x0000DD19
		public _IExpression ItemReference
		{
			get
			{
				if (this.m_itref == null)
				{
					return new NullExpression();
				}
				return this.m_itref;
			}
			set
			{
				this.m_itref = (value as ItemReference);
			}
		}

		// Token: 0x17000110 RID: 272
		// (get) Token: 0x06000515 RID: 1301 RVA: 0x0000ED27 File Offset: 0x0000DD27
		// (set) Token: 0x06000516 RID: 1302 RVA: 0x0000ED19 File Offset: 0x0000DD19
		public IExpression ReferencedItem
		{
			get
			{
				return this.ItemReference;
			}
			set
			{
				this.m_itref = (value as ItemReference);
			}
		}

		// Token: 0x17000111 RID: 273
		// (get) Token: 0x06000517 RID: 1303 RVA: 0x0000ED2F File Offset: 0x0000DD2F
		// (set) Token: 0x06000518 RID: 1304 RVA: 0x0000ED37 File Offset: 0x0000DD37
		public string Attribute
		{
			get
			{
				return this.m_stAttribute;
			}
			set
			{
				this.m_stAttribute = value;
			}
		}

		// Token: 0x06000519 RID: 1305 RVA: 0x0000ED40 File Offset: 0x0000DD40
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x0600051A RID: 1306 RVA: 0x0000ED49 File Offset: 0x0000DD49
		public override T Accept<T>(IExprementVisitor<T> visitor)
		{
			return visitor.visit(this);
		}

		// Token: 0x0600051B RID: 1307 RVA: 0x0000ED52 File Offset: 0x0000DD52
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x0600051C RID: 1308 RVA: 0x00004E6B File Offset: 0x00003E6B
		public override bool IsLValue(IScope scope, bool bWriteToConstants)
		{
			return false;
		}

		// Token: 0x0600051D RID: 1309 RVA: 0x0000ED5C File Offset: 0x0000DD5C
		public override _IExprement Duplicate()
		{
			HasAttributeExpression hasAttributeExpression = new HasAttributeExpression();
			if (this.m_itref != null)
			{
				hasAttributeExpression.m_itref = (this.m_itref.Duplicate() as ItemReference);
			}
			hasAttributeExpression.m_stAttribute = this.m_stAttribute;
			this.DuplicateCommon(hasAttributeExpression);
			return hasAttributeExpression;
		}

		// Token: 0x040000B8 RID: 184
		[DefaultSerialization("Reference")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private ItemReference m_itref;

		// Token: 0x040000B9 RID: 185
		[DefaultSerialization("Attribute")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private string m_stAttribute;
	}
}
