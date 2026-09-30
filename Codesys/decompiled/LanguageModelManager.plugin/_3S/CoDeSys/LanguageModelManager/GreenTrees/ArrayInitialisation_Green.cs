using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.GreenTrees
{
	// Token: 0x0200022D RID: 557
	internal class ArrayInitialisation_Green : Expression_Green, _IArrayInitialization, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression, IArrayInitialization
	{
		// Token: 0x0600246E RID: 9326 RVA: 0x0005C3E0 File Offset: 0x0005B3E0
		public ArrayInitialisation_Green(IList<_IExpression> initexprs)
		{
			this.m_initlist = new _IExpression[initexprs.Count];
			initexprs.CopyTo(this.m_initlist, 0);
		}

		// Token: 0x0600246F RID: 9327 RVA: 0x0000B507 File Offset: 0x0000A507
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x06002470 RID: 9328 RVA: 0x0005C408 File Offset: 0x0005B408
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			IExprVisitor3 exprVisitor = visitor as IExprVisitor3;
			if (exprVisitor != null)
			{
				exprVisitor.visit(this);
			}
		}

		// Token: 0x17000A53 RID: 2643
		// (get) Token: 0x06002471 RID: 9329 RVA: 0x0005C428 File Offset: 0x0005B428
		public IExpression[] InitValues
		{
			get
			{
				return this.m_initlist;
			}
		}

		// Token: 0x17000A54 RID: 2644
		// (get) Token: 0x06002472 RID: 9330 RVA: 0x0005C43D File Offset: 0x0005B43D
		public IList<_IExpression> _InitValues
		{
			get
			{
				return this.m_initlist;
			}
		}

		// Token: 0x06002473 RID: 9331 RVA: 0x0005C448 File Offset: 0x0005B448
		[SuppressMessage("Major Code Smell", "S1168:Empty arrays and collections should be returned instead of null", Justification = "the null return value is used by caller and has a special meaning")]
		public IList<_IExpression> GetFlatList(IScope scope, out bool bValid)
		{
			List<_IExpression> list = new List<_IExpression>();
			bValid = true;
			foreach (_IExpression iexpression in this.m_initlist)
			{
				if (iexpression is _IMultipleIndexInitialization)
				{
					_IMultipleIndexInitialization imultipleIndexInitialization = iexpression as _IMultipleIndexInitialization;
					ILiteralValue literalValue = imultipleIndexInitialization._Number.Literal(scope);
					if (literalValue == null)
					{
						bValid = false;
						return null;
					}
					int @int = literalValue.GetInt(out bValid);
					if (!bValid)
					{
						return null;
					}
					for (int j = 0; j < @int; j++)
					{
						list.Add(imultipleIndexInitialization._Value);
					}
				}
				else
				{
					list.Add(iexpression);
				}
			}
			return list;
		}

		// Token: 0x17000A55 RID: 2645
		public _IExpression this[int i]
		{
			get
			{
				return this.m_initlist[i];
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		// Token: 0x06002476 RID: 9334 RVA: 0x0005A8D1 File Offset: 0x000598D1
		[Obsolete("no list access")]
		public void AddInitValue(_IExpression exp)
		{
			Debug.Assert(false);
		}

		// Token: 0x17000A56 RID: 2646
		// (get) Token: 0x06002477 RID: 9335 RVA: 0x00004E6B File Offset: 0x00003E6B
		// (set) Token: 0x06002478 RID: 9336 RVA: 0x0005A471 File Offset: 0x00059471
		[Obsolete("not used for compile")]
		public bool DefaultInitializationDone
		{
			get
			{
				return false;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		// Token: 0x040006FB RID: 1787
		private readonly _IExpression[] m_initlist;
	}
}
