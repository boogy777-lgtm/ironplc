using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.GreenTrees
{
	// Token: 0x02000205 RID: 517
	internal class StringLiteralExpression_Green : LiteralExpression_Green, _IStringLiteralExpression2, _IStringLiteralExpression, _ILiteralExpression, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression, ILiteralExpression2, ILiteralExpression
	{
		// Token: 0x06002327 RID: 8999 RVA: 0x0005BA20 File Offset: 0x0005AA20
		public StringLiteralExpression_Green(string stVal, TypeClass tc)
		{
			this.m_stValue = stVal;
			this.m_type = tc;
			this.m_tcOriginal = tc;
		}

		// Token: 0x06002328 RID: 9000 RVA: 0x0005BA3D File Offset: 0x0005AA3D
		public StringLiteralExpression_Green(string stVal, TypeClass tc, StringEncoding stringEncoding)
		{
			this.m_stValue = stVal;
			this.m_type = tc;
			this.m_tcOriginal = tc;
			this.m_stringEncoding = stringEncoding;
		}

		// Token: 0x06002329 RID: 9001 RVA: 0x0005BA61 File Offset: 0x0005AA61
		public StringLiteralExpression_Green(string stVal)
		{
			this.m_stValue = stVal;
			this.m_type = TypeClass.String;
		}

		// Token: 0x170009DA RID: 2522
		// (get) Token: 0x0600232A RID: 9002 RVA: 0x0000E8C0 File Offset: 0x0000D8C0
		// (set) Token: 0x0600232B RID: 9003 RVA: 0x0005A471 File Offset: 0x00059471
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

		// Token: 0x170009DB RID: 2523
		// (get) Token: 0x0600232C RID: 9004 RVA: 0x00004E6B File Offset: 0x00003E6B
		// (set) Token: 0x0600232D RID: 9005 RVA: 0x0005A471 File Offset: 0x00059471
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

		// Token: 0x170009DC RID: 2524
		// (get) Token: 0x0600232E RID: 9006 RVA: 0x0000E8C0 File Offset: 0x0000D8C0
		public override ulong ULongValue
		{
			get
			{
				return 0UL;
			}
		}

		// Token: 0x170009DD RID: 2525
		// (get) Token: 0x0600232F RID: 9007 RVA: 0x0005BA78 File Offset: 0x0005AA78
		// (set) Token: 0x06002330 RID: 9008 RVA: 0x0000677E File Offset: 0x0000577E
		public override string StringValue
		{
			get
			{
				return this.m_stValue;
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		// Token: 0x170009DE RID: 2526
		// (get) Token: 0x06002331 RID: 9009 RVA: 0x0000FDF5 File Offset: 0x0000EDF5
		// (set) Token: 0x06002332 RID: 9010 RVA: 0x0000677E File Offset: 0x0000577E
		public override double RealValue
		{
			get
			{
				return 0.0;
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		// Token: 0x170009DF RID: 2527
		// (get) Token: 0x06002333 RID: 9011 RVA: 0x0005BA80 File Offset: 0x0005AA80
		// (set) Token: 0x06002334 RID: 9012 RVA: 0x0000677E File Offset: 0x0000577E
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

		// Token: 0x170009E0 RID: 2528
		// (get) Token: 0x06002335 RID: 9013 RVA: 0x0005BA88 File Offset: 0x0005AA88
		// (set) Token: 0x06002336 RID: 9014 RVA: 0x0000677E File Offset: 0x0000577E
		public StringEncoding StringEncoding
		{
			get
			{
				return this.m_stringEncoding;
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		// Token: 0x040006BE RID: 1726
		private readonly TypeClass m_type;

		// Token: 0x040006BF RID: 1727
		private readonly string m_stValue;

		// Token: 0x040006C0 RID: 1728
		private readonly StringEncoding m_stringEncoding;
	}
}
