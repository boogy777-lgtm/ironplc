using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.GreenTrees
{
	// Token: 0x020001FE RID: 510
	internal class NewExpression_Green : Expression_Green, _INewExpression, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression, INewExpression
	{
		// Token: 0x060022DD RID: 8925 RVA: 0x0005B6CD File Offset: 0x0005A6CD
		public NewExpression_Green(_IType typeIn, _IExpression expCount, IAssignmentExpression[] fbinitparams)
		{
			this.m_typetocast = typeIn;
			this.m_expCount = expCount;
			this._fbinitparams = fbinitparams;
		}

		// Token: 0x170009BC RID: 2492
		// (get) Token: 0x060022DE RID: 8926 RVA: 0x0005B6EA File Offset: 0x0005A6EA
		// (set) Token: 0x060022DF RID: 8927 RVA: 0x0005A471 File Offset: 0x00059471
		public bool PositionOK
		{
			get
			{
				return this._bPosOK;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		// Token: 0x060022E0 RID: 8928 RVA: 0x0005B6F2 File Offset: 0x0005A6F2
		public int ElementCount(out bool bValid)
		{
			bValid = false;
			if (this.m_expCount is _IIntegerLiteralExpression)
			{
				return (this.m_expCount as _IIntegerLiteralExpression).LiteralValue.GetInt(out bValid);
			}
			return 0;
		}

		// Token: 0x060022E1 RID: 8929 RVA: 0x000107D7 File Offset: 0x0000F7D7
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x060022E2 RID: 8930 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public override void AcceptVisitor(IExprVisitor visitor)
		{
		}

		// Token: 0x170009BD RID: 2493
		// (get) Token: 0x060022E3 RID: 8931 RVA: 0x0005B71C File Offset: 0x0005A71C
		// (set) Token: 0x060022E4 RID: 8932 RVA: 0x0005A471 File Offset: 0x00059471
		public _IType _TypeToCast
		{
			get
			{
				if (this.m_typetocast == null)
				{
					return null;
				}
				return this.m_typetocast.EffectiveType as _IType;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		// Token: 0x170009BE RID: 2494
		// (get) Token: 0x060022E5 RID: 8933 RVA: 0x0005B738 File Offset: 0x0005A738
		// (set) Token: 0x060022E6 RID: 8934 RVA: 0x0005A471 File Offset: 0x00059471
		public _IExpression _Count
		{
			get
			{
				return this.m_expCount;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		// Token: 0x170009BF RID: 2495
		// (get) Token: 0x060022E7 RID: 8935 RVA: 0x0005B740 File Offset: 0x0005A740
		public ICompiledType TypeToCreate
		{
			get
			{
				return this.m_typetocast;
			}
		}

		// Token: 0x170009C0 RID: 2496
		// (get) Token: 0x060022E8 RID: 8936 RVA: 0x0005B738 File Offset: 0x0005A738
		public IExpression Count
		{
			get
			{
				return this.m_expCount;
			}
		}

		// Token: 0x170009C1 RID: 2497
		// (get) Token: 0x060022E9 RID: 8937 RVA: 0x0005B748 File Offset: 0x0005A748
		// (set) Token: 0x060022EA RID: 8938 RVA: 0x0005A471 File Offset: 0x00059471
		public IEnumerable<IAssignmentExpression> FBInitParams
		{
			get
			{
				return this._fbinitparams;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		// Token: 0x170009C2 RID: 2498
		// (get) Token: 0x060022EB RID: 8939 RVA: 0x0005B748 File Offset: 0x0005A748
		public IList<IAssignmentExpression> _FBInitParams
		{
			get
			{
				return this._fbinitparams;
			}
		}

		// Token: 0x060022EC RID: 8940 RVA: 0x0005A471 File Offset: 0x00059471
		public void AddFBInitParam(IAssignmentExpression assexp)
		{
			throw new NotSupportedException();
		}

		// Token: 0x040006B2 RID: 1714
		private readonly _IType m_typetocast;

		// Token: 0x040006B3 RID: 1715
		private readonly _IExpression m_expCount;

		// Token: 0x040006B4 RID: 1716
		private readonly IAssignmentExpression[] _fbinitparams;

		// Token: 0x040006B5 RID: 1717
		private readonly bool _bPosOK;
	}
}
