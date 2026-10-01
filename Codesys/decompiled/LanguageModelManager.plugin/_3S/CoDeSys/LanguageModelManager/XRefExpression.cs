using System;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x0200007B RID: 123
	[TypeGuid("{49c5a0c4-2df3-418f-b906-3dec1c102883}")]
	[StorageVersion("3.3.0.0")]
	public class XRefExpression : PragmaExpression, _IXRefExpression, _IPragmaExpression, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression
	{
		// Token: 0x06000802 RID: 2050 RVA: 0x0000C6EA File Offset: 0x0000B6EA
		public XRefExpression()
		{
		}

		// Token: 0x06000803 RID: 2051 RVA: 0x0000C6F2 File Offset: 0x0000B6F2
		public XRefExpression(IToken token) : base(token)
		{
		}

		// Token: 0x06000804 RID: 2052 RVA: 0x00013F2D File Offset: 0x00012F2D
		public XRefExpression(IToken token, _IItemReference itref, _IItemReference itrefFrom) : base(token)
		{
			this.m_itref = (itref as ItemReference);
			this.m_itrefFrom = (itrefFrom as ItemReference);
		}

		// Token: 0x170001C6 RID: 454
		// (get) Token: 0x06000805 RID: 2053 RVA: 0x00013F4E File Offset: 0x00012F4E
		// (set) Token: 0x06000806 RID: 2054 RVA: 0x00013F64 File Offset: 0x00012F64
		public _IExpression XRef
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

		// Token: 0x170001C7 RID: 455
		// (get) Token: 0x06000807 RID: 2055 RVA: 0x00013F72 File Offset: 0x00012F72
		// (set) Token: 0x06000808 RID: 2056 RVA: 0x00013F88 File Offset: 0x00012F88
		public _IExpression XRefFrom
		{
			get
			{
				if (this.m_itrefFrom == null)
				{
					return new NullExpression();
				}
				return this.m_itrefFrom;
			}
			set
			{
				this.m_itrefFrom = (value as ItemReference);
			}
		}

		// Token: 0x06000809 RID: 2057 RVA: 0x00013F96 File Offset: 0x00012F96
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x0600080A RID: 2058 RVA: 0x00013F9F File Offset: 0x00012F9F
		public override T Accept<T>(IExprementVisitor<T> visitor)
		{
			return visitor.visit(this);
		}

		// Token: 0x0600080B RID: 2059 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public override void AcceptVisitor(IExprVisitor visitor)
		{
		}

		// Token: 0x0600080C RID: 2060 RVA: 0x00004E6B File Offset: 0x00003E6B
		public override bool IsLValue(IScope scope, bool bWriteToConstants)
		{
			return false;
		}

		// Token: 0x0600080D RID: 2061 RVA: 0x00013FA8 File Offset: 0x00012FA8
		public override _IExprement Duplicate()
		{
			XRefExpression xrefExpression = new XRefExpression();
			if (this.m_itref != null)
			{
				xrefExpression.m_itref = (this.m_itref.Duplicate() as ItemReference);
			}
			if (this.m_itrefFrom != null)
			{
				xrefExpression.m_itrefFrom = (this.m_itrefFrom.Duplicate() as ItemReference);
			}
			this.DuplicateCommon(xrefExpression);
			return xrefExpression;
		}

		// Token: 0x0400010D RID: 269
		[DefaultSerialization("Reference")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private ItemReference m_itref;

		// Token: 0x0400010E RID: 270
		[DefaultSerialization("ReferenceFrom")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private ItemReference m_itrefFrom;
	}
}
