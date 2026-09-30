using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.GreenTrees
{
	// Token: 0x02000206 RID: 518
	internal class FloatLiteralExpression_Green : LiteralExpression_Green, _IFloatLiteralExpression, _ILiteralExpression, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression, ILiteralExpression2, ILiteralExpression
	{
		// Token: 0x06002337 RID: 9015 RVA: 0x0005BA90 File Offset: 0x0005AA90
		public FloatLiteralExpression_Green(double dVal, TypeClass tc)
		{
			this.m_dValue = dVal;
			this.m_type = tc;
			this.m_tcOriginal = tc;
		}

		// Token: 0x06002338 RID: 9016 RVA: 0x0005BAAD File Offset: 0x0005AAAD
		public FloatLiteralExpression_Green(double dVal)
		{
			this.m_dValue = dVal;
			this.m_type = TypeClass.AnyReal;
			this.m_tcOriginal = TypeClass.None;
		}

		// Token: 0x170009E1 RID: 2529
		// (get) Token: 0x06002339 RID: 9017 RVA: 0x0000E8C0 File Offset: 0x0000D8C0
		// (set) Token: 0x0600233A RID: 9018 RVA: 0x0005A471 File Offset: 0x00059471
		public override long LongValue
		{
			get
			{
				return 0L;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		// Token: 0x170009E2 RID: 2530
		// (get) Token: 0x0600233B RID: 9019 RVA: 0x00004E6B File Offset: 0x00003E6B
		// (set) Token: 0x0600233C RID: 9020 RVA: 0x0005A471 File Offset: 0x00059471
		public override bool Negative
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

		// Token: 0x170009E3 RID: 2531
		// (get) Token: 0x0600233D RID: 9021 RVA: 0x0000E8C0 File Offset: 0x0000D8C0
		public override ulong ULongValue
		{
			get
			{
				return 0UL;
			}
		}

		// Token: 0x170009E4 RID: 2532
		// (get) Token: 0x0600233E RID: 9022 RVA: 0x0000E8C4 File Offset: 0x0000D8C4
		// (set) Token: 0x0600233F RID: 9023 RVA: 0x0000677E File Offset: 0x0000577E
		public override string StringValue
		{
			get
			{
				return string.Empty;
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		// Token: 0x170009E5 RID: 2533
		// (get) Token: 0x06002340 RID: 9024 RVA: 0x0005BACC File Offset: 0x0005AACC
		// (set) Token: 0x06002341 RID: 9025 RVA: 0x0000677E File Offset: 0x0000577E
		public override double RealValue
		{
			get
			{
				return this.m_dValue;
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		// Token: 0x170009E6 RID: 2534
		// (get) Token: 0x06002342 RID: 9026 RVA: 0x0005BAD4 File Offset: 0x0005AAD4
		// (set) Token: 0x06002343 RID: 9027 RVA: 0x0000677E File Offset: 0x0000577E
		public override TypeClass ConstantType
		{
			get
			{
				return this.m_type;
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		// Token: 0x040006C1 RID: 1729
		private readonly TypeClass m_type;

		// Token: 0x040006C2 RID: 1730
		private readonly double m_dValue;
	}
}
