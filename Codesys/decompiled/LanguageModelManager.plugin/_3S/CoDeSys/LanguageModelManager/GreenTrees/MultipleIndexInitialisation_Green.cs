using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.GreenTrees
{
	// Token: 0x0200022F RID: 559
	internal class MultipleIndexInitialisation_Green : Expression_Green, _IMultipleIndexInitialization, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression, IMultipleIndexInitialization
	{
		// Token: 0x06002481 RID: 9345 RVA: 0x0005C545 File Offset: 0x0005B545
		public MultipleIndexInitialisation_Green(_IExpression expValue, _IExpression expNumber)
		{
			this.m_expValue = expValue;
			this.m_expNumber = expNumber;
		}

		// Token: 0x06002482 RID: 9346 RVA: 0x0001036A File Offset: 0x0000F36A
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x06002483 RID: 9347 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public override void AcceptVisitor(IExprVisitor visitor)
		{
		}

		// Token: 0x17000A5A RID: 2650
		// (get) Token: 0x06002484 RID: 9348 RVA: 0x0005C55B File Offset: 0x0005B55B
		// (set) Token: 0x06002485 RID: 9349 RVA: 0x0005A471 File Offset: 0x00059471
		public _IExpression _Number
		{
			get
			{
				if (this.m_expNumber == null)
				{
					return new NullExpression_Green();
				}
				return this.m_expNumber;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		// Token: 0x17000A5B RID: 2651
		// (get) Token: 0x06002486 RID: 9350 RVA: 0x0005C571 File Offset: 0x0005B571
		// (set) Token: 0x06002487 RID: 9351 RVA: 0x0005A471 File Offset: 0x00059471
		public _IExpression _Value
		{
			get
			{
				if (this.m_expValue == null)
				{
					return new NullExpression_Green();
				}
				return this.m_expValue;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		// Token: 0x17000A5C RID: 2652
		// (get) Token: 0x06002488 RID: 9352 RVA: 0x0005C587 File Offset: 0x0005B587
		public IExpression Number
		{
			get
			{
				return this._Number;
			}
		}

		// Token: 0x17000A5D RID: 2653
		// (get) Token: 0x06002489 RID: 9353 RVA: 0x0005C58F File Offset: 0x0005B58F
		public IExpression Value
		{
			get
			{
				return this._Value;
			}
		}

		// Token: 0x0600248A RID: 9354 RVA: 0x0005C598 File Offset: 0x0005B598
		public int NumberInt(out bool bValid, IScope scope)
		{
			int result = TypeHelper.GetInt(this.m_expNumber, scope, out bValid);
			if (!bValid)
			{
				result = -1;
			}
			return result;
		}

		// Token: 0x0600248B RID: 9355 RVA: 0x0005C5BC File Offset: 0x0005B5BC
		public int NumberInt(out bool bValid, IPrecompileScope scope)
		{
			int result = TypeHelper.GetInt(this.m_expNumber, scope, out bValid);
			if (!bValid)
			{
				result = -1;
			}
			return result;
		}

		// Token: 0x040006FD RID: 1789
		private readonly _IExpression m_expValue;

		// Token: 0x040006FE RID: 1790
		private readonly _IExpression m_expNumber;
	}
}
