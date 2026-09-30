using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.GreenTrees
{
	// Token: 0x02000203 RID: 515
	internal class IntegerLiteralExpression_Green : LiteralExpression_Green, _IIntegerLiteralExpression, _ILiteralExpression, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression, ILiteralExpression2, ILiteralExpression
	{
		// Token: 0x06002312 RID: 8978 RVA: 0x0005B92D File Offset: 0x0005A92D
		public IntegerLiteralExpression_Green(long lVal, TypeClass tc, bool negative) : this(lVal, tc)
		{
			this.m_bNegative = negative;
		}

		// Token: 0x06002313 RID: 8979 RVA: 0x0005B93E File Offset: 0x0005A93E
		public IntegerLiteralExpression_Green(long lVal, TypeClass tc)
		{
			this.m_lValue = lVal;
			this.m_type = tc;
			this.m_tcOriginal = tc;
			this.m_bNegative = false;
		}

		// Token: 0x06002314 RID: 8980 RVA: 0x0005B962 File Offset: 0x0005A962
		internal IntegerLiteralExpression_Green(ulong ulVal, TypeClass tc)
		{
			this.m_lValue = (long)ulVal;
			this.m_bNegative = false;
			this.m_type = tc;
			this.m_tcOriginal = tc;
		}

		// Token: 0x06002315 RID: 8981 RVA: 0x0005B986 File Offset: 0x0005A986
		internal IntegerLiteralExpression_Green(long lVal)
		{
			this.m_lValue = lVal;
			this.m_bNegative = (this.m_lValue < 0L);
			this.m_type = TypeClass.AnyInt;
			this.m_tcOriginal = TypeClass.None;
		}

		// Token: 0x06002316 RID: 8982 RVA: 0x0005B9B5 File Offset: 0x0005A9B5
		internal IntegerLiteralExpression_Green(ulong ulVal)
		{
			this.m_lValue = (long)ulVal;
			this.m_bNegative = false;
			this.m_type = TypeClass.AnyInt;
			this.m_tcOriginal = TypeClass.None;
		}

		// Token: 0x170009D2 RID: 2514
		// (get) Token: 0x06002317 RID: 8983 RVA: 0x0005B9DB File Offset: 0x0005A9DB
		// (set) Token: 0x06002318 RID: 8984 RVA: 0x0005A471 File Offset: 0x00059471
		public override long LongValue
		{
			get
			{
				return this.m_lValue;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		// Token: 0x170009D3 RID: 2515
		// (get) Token: 0x06002319 RID: 8985 RVA: 0x0005B9E3 File Offset: 0x0005A9E3
		// (set) Token: 0x0600231A RID: 8986 RVA: 0x0005A471 File Offset: 0x00059471
		public override bool Negative
		{
			get
			{
				return this.m_bNegative;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		// Token: 0x170009D4 RID: 2516
		// (get) Token: 0x0600231B RID: 8987 RVA: 0x0005B9DB File Offset: 0x0005A9DB
		public override ulong ULongValue
		{
			get
			{
				return (ulong)this.m_lValue;
			}
		}

		// Token: 0x170009D5 RID: 2517
		// (get) Token: 0x0600231C RID: 8988 RVA: 0x0000E8C4 File Offset: 0x0000D8C4
		// (set) Token: 0x0600231D RID: 8989 RVA: 0x0005A471 File Offset: 0x00059471
		public override string StringValue
		{
			get
			{
				return string.Empty;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		// Token: 0x170009D6 RID: 2518
		// (get) Token: 0x0600231E RID: 8990 RVA: 0x0000FDF5 File Offset: 0x0000EDF5
		// (set) Token: 0x0600231F RID: 8991 RVA: 0x0005A471 File Offset: 0x00059471
		public override double RealValue
		{
			get
			{
				return 0.0;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		// Token: 0x170009D7 RID: 2519
		// (get) Token: 0x06002320 RID: 8992 RVA: 0x0005B9EB File Offset: 0x0005A9EB
		// (set) Token: 0x06002321 RID: 8993 RVA: 0x0005A471 File Offset: 0x00059471
		public override TypeClass ConstantType
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

		// Token: 0x170009D8 RID: 2520
		// (get) Token: 0x06002322 RID: 8994 RVA: 0x00005E58 File Offset: 0x00004E58
		public override bool IsLiteral
		{
			get
			{
				return true;
			}
		}

		// Token: 0x040006BA RID: 1722
		protected long m_lValue;

		// Token: 0x040006BB RID: 1723
		protected TypeClass m_type;

		// Token: 0x040006BC RID: 1724
		protected bool m_bNegative;
	}
}
