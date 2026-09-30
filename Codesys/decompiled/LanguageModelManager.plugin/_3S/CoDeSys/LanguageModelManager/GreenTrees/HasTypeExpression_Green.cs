using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.GreenTrees
{
	// Token: 0x02000221 RID: 545
	internal class HasTypeExpression_Green : Expression_Green, _IHasTypeExpression, _IPragmaExpression, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression, IHasTypeExpression
	{
		// Token: 0x06002401 RID: 9217 RVA: 0x0005BFBE File Offset: 0x0005AFBE
		public HasTypeExpression_Green(_IVariableReference varref, ICompiledType type)
		{
			this.m_varref = varref;
			this.m_type = type;
		}

		// Token: 0x17000A26 RID: 2598
		// (get) Token: 0x06002402 RID: 9218 RVA: 0x0005BFD4 File Offset: 0x0005AFD4
		// (set) Token: 0x06002403 RID: 9219 RVA: 0x0005A471 File Offset: 0x00059471
		public _IExpression Variable
		{
			get
			{
				if (this.m_varref == null)
				{
					return new NullExpression_Green();
				}
				return this.m_varref;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		// Token: 0x17000A27 RID: 2599
		// (get) Token: 0x06002404 RID: 9220 RVA: 0x0005BFEA File Offset: 0x0005AFEA
		public IExpression Instance
		{
			get
			{
				return this.Variable;
			}
		}

		// Token: 0x17000A28 RID: 2600
		// (get) Token: 0x06002405 RID: 9221 RVA: 0x00005E58 File Offset: 0x00004E58
		public virtual bool Exact
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000A29 RID: 2601
		// (get) Token: 0x06002406 RID: 9222 RVA: 0x0005BFF2 File Offset: 0x0005AFF2
		// (set) Token: 0x06002407 RID: 9223 RVA: 0x0005A471 File Offset: 0x00059471
		public ICompiledType ReferencedType
		{
			get
			{
				return this.m_type;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		// Token: 0x06002408 RID: 9224 RVA: 0x0000F097 File Offset: 0x0000E097
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x06002409 RID: 9225 RVA: 0x0000F0A9 File Offset: 0x0000E0A9
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x17000A2A RID: 2602
		// (get) Token: 0x0600240A RID: 9226 RVA: 0x0005BFFA File Offset: 0x0005AFFA
		public _IExpression VarRef
		{
			get
			{
				return this.m_varref;
			}
		}

		// Token: 0x17000A2B RID: 2603
		// (get) Token: 0x0600240B RID: 9227 RVA: 0x0005A471 File Offset: 0x00059471
		// (set) Token: 0x0600240C RID: 9228 RVA: 0x0005A471 File Offset: 0x00059471
		public bool Value
		{
			get
			{
				throw new NotSupportedException();
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		// Token: 0x17000A2C RID: 2604
		// (get) Token: 0x0600240D RID: 9229 RVA: 0x0005A471 File Offset: 0x00059471
		// (set) Token: 0x0600240E RID: 9230 RVA: 0x0005A471 File Offset: 0x00059471
		public bool ValueStillUndecided
		{
			get
			{
				throw new NotSupportedException();
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		// Token: 0x040006E2 RID: 1762
		protected _IVariableReference m_varref;

		// Token: 0x040006E3 RID: 1763
		protected ICompiledType m_type;
	}
}
