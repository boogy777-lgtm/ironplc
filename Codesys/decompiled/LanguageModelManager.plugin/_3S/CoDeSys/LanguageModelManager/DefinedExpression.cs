using System;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x0200004C RID: 76
	[TypeGuid("{1691b137-f52c-4dba-a11e-da225ef769aa}")]
	[StorageVersion("3.3.0.0")]
	public class DefinedExpression : PragmaExpression, _IDefinedExpression, _IPragmaExpression, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression, IDefinedExpression
	{
		// Token: 0x06000465 RID: 1125 RVA: 0x0000C6EA File Offset: 0x0000B6EA
		public DefinedExpression()
		{
		}

		// Token: 0x06000466 RID: 1126 RVA: 0x0000C6F2 File Offset: 0x0000B6F2
		public DefinedExpression(IToken token) : base(token)
		{
		}

		// Token: 0x06000467 RID: 1127 RVA: 0x0000E2C1 File Offset: 0x0000D2C1
		public DefinedExpression(IToken token, _IItemReference itref) : base(token)
		{
			this.m_itref = (itref as ItemReference);
		}

		// Token: 0x170000E6 RID: 230
		// (get) Token: 0x06000468 RID: 1128 RVA: 0x0000E2D6 File Offset: 0x0000D2D6
		// (set) Token: 0x06000469 RID: 1129 RVA: 0x0000E2EC File Offset: 0x0000D2EC
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

		// Token: 0x170000E7 RID: 231
		// (get) Token: 0x0600046A RID: 1130 RVA: 0x0000E2FA File Offset: 0x0000D2FA
		// (set) Token: 0x0600046B RID: 1131 RVA: 0x0000E2EC File Offset: 0x0000D2EC
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

		// Token: 0x0600046C RID: 1132 RVA: 0x0000E302 File Offset: 0x0000D302
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x0600046D RID: 1133 RVA: 0x0000E30B File Offset: 0x0000D30B
		public override T Accept<T>(IExprementVisitor<T> visitor)
		{
			return visitor.visit(this);
		}

		// Token: 0x0600046E RID: 1134 RVA: 0x0000E314 File Offset: 0x0000D314
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x0600046F RID: 1135 RVA: 0x00004E6B File Offset: 0x00003E6B
		public override bool IsLValue(IScope scope, bool bWriteToConstants)
		{
			return false;
		}

		// Token: 0x06000470 RID: 1136 RVA: 0x0000E320 File Offset: 0x0000D320
		public override _IExprement Duplicate()
		{
			DefinedExpression definedExpression = new DefinedExpression();
			if (this.m_itref != null)
			{
				definedExpression.m_itref = (this.m_itref.Duplicate() as ItemReference);
			}
			this.DuplicateCommon(definedExpression);
			return definedExpression;
		}

		// Token: 0x040000AC RID: 172
		[DefaultSerialization("Reference")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private ItemReference m_itref;
	}
}
