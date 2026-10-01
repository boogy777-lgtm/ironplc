using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x0200005E RID: 94
	[TypeGuid("{88513019-926a-4125-ab4f-260cf5e4c63e}")]
	[StorageVersion("3.3.0.0")]
	public class IntegerLiteralExpression : LiteralExpression, _IIntegerLiteralExpression, _ILiteralExpression, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression, ILiteralExpression2, ILiteralExpression
	{
		// Token: 0x060005A1 RID: 1441 RVA: 0x0000FC85 File Offset: 0x0000EC85
		public IntegerLiteralExpression()
		{
		}

		// Token: 0x060005A2 RID: 1442 RVA: 0x0000FC95 File Offset: 0x0000EC95
		internal IntegerLiteralExpression(IToken token) : base(token)
		{
		}

		// Token: 0x060005A3 RID: 1443 RVA: 0x0000FCA6 File Offset: 0x0000ECA6
		public IntegerLiteralExpression(long lVal, TypeClass tc, bool negative) : this(lVal, tc)
		{
			this.Negative = negative;
		}

		// Token: 0x060005A4 RID: 1444 RVA: 0x0000FCB7 File Offset: 0x0000ECB7
		public IntegerLiteralExpression(long lVal, TypeClass tc, IToken token, bool negative) : this(lVal, tc, token)
		{
			this.Negative = negative;
		}

		// Token: 0x060005A5 RID: 1445 RVA: 0x0000FCCA File Offset: 0x0000ECCA
		public IntegerLiteralExpression(long lVal, TypeClass tc)
		{
			this.m_lValue = lVal;
			this.m_type = tc;
			this.m_tcOriginal = tc;
		}

		// Token: 0x060005A6 RID: 1446 RVA: 0x0000FCEF File Offset: 0x0000ECEF
		public IntegerLiteralExpression(long lVal, TypeClass tc, IToken token) : base(token)
		{
			this.m_lValue = lVal;
			this.m_type = tc;
			this.m_tcOriginal = tc;
		}

		// Token: 0x060005A7 RID: 1447 RVA: 0x0000FD15 File Offset: 0x0000ED15
		internal IntegerLiteralExpression(ulong ulVal, TypeClass tc)
		{
			this.m_lValue = (long)ulVal;
			this.Negative = false;
			this.m_type = tc;
			this.m_tcOriginal = tc;
		}

		// Token: 0x060005A8 RID: 1448 RVA: 0x0000FD41 File Offset: 0x0000ED41
		internal IntegerLiteralExpression(ulong ulVal, TypeClass tc, IToken token) : base(token)
		{
			this.m_lValue = (long)ulVal;
			this.Negative = false;
			this.m_type = tc;
			this.m_tcOriginal = tc;
		}

		// Token: 0x060005A9 RID: 1449 RVA: 0x0000FD6E File Offset: 0x0000ED6E
		internal IntegerLiteralExpression(long lVal)
		{
			this.m_lValue = lVal;
			this.Negative = (this.m_lValue < 0L);
			this.m_type = TypeClass.AnyInt;
			this.m_tcOriginal = TypeClass.None;
		}

		// Token: 0x060005AA RID: 1450 RVA: 0x0000FDA5 File Offset: 0x0000EDA5
		internal IntegerLiteralExpression(ulong ulVal)
		{
			this.m_lValue = (long)ulVal;
			this.Negative = false;
			this.m_type = TypeClass.AnyInt;
			this.m_tcOriginal = TypeClass.None;
		}

		// Token: 0x060005AB RID: 1451 RVA: 0x00004E6B File Offset: 0x00003E6B
		public override bool IsLValue(IScope scope, bool bWriteToConstants)
		{
			return false;
		}

		// Token: 0x17000138 RID: 312
		// (get) Token: 0x060005AC RID: 1452 RVA: 0x0000FDD3 File Offset: 0x0000EDD3
		// (set) Token: 0x060005AD RID: 1453 RVA: 0x0000FDDB File Offset: 0x0000EDDB
		public override long LongValue
		{
			get
			{
				return this.m_lValue;
			}
			set
			{
				this.m_lValue = value;
			}
		}

		// Token: 0x17000139 RID: 313
		// (get) Token: 0x060005AE RID: 1454 RVA: 0x0000FDE4 File Offset: 0x0000EDE4
		// (set) Token: 0x060005AF RID: 1455 RVA: 0x0000FDEC File Offset: 0x0000EDEC
		public override bool Negative
		{
			get
			{
				return this.m_bNegative;
			}
			set
			{
				this.m_bNegative = value;
			}
		}

		// Token: 0x1700013A RID: 314
		// (get) Token: 0x060005B0 RID: 1456 RVA: 0x0000FDD3 File Offset: 0x0000EDD3
		public override ulong ULongValue
		{
			get
			{
				return (ulong)this.m_lValue;
			}
		}

		// Token: 0x1700013B RID: 315
		// (get) Token: 0x060005B1 RID: 1457 RVA: 0x0000E8C4 File Offset: 0x0000D8C4
		// (set) Token: 0x060005B2 RID: 1458 RVA: 0x00003AE9 File Offset: 0x00002AE9
		[SuppressMessage("Blocker Code Smell", "S3237:\"value\" parameters should be used", Justification = "Setter cannot be changed for compatibility reasons")]
		public override string StringValue
		{
			get
			{
				return string.Empty;
			}
			set
			{
			}
		}

		// Token: 0x1700013C RID: 316
		// (get) Token: 0x060005B3 RID: 1459 RVA: 0x0000FDF5 File Offset: 0x0000EDF5
		// (set) Token: 0x060005B4 RID: 1460 RVA: 0x00003AE9 File Offset: 0x00002AE9
		[SuppressMessage("Blocker Code Smell", "S3237:\"value\" parameters should be used", Justification = "Setter cannot be changed for compatibility reasons")]
		public override double RealValue
		{
			get
			{
				return 0.0;
			}
			set
			{
			}
		}

		// Token: 0x1700013D RID: 317
		// (get) Token: 0x060005B5 RID: 1461 RVA: 0x0000FE00 File Offset: 0x0000EE00
		// (set) Token: 0x060005B6 RID: 1462 RVA: 0x0000FE08 File Offset: 0x0000EE08
		public override TypeClass ConstantType
		{
			get
			{
				return this.m_type;
			}
			set
			{
				this.m_type = value;
			}
		}

		// Token: 0x1700013E RID: 318
		// (get) Token: 0x060005B7 RID: 1463 RVA: 0x00005E58 File Offset: 0x00004E58
		public override bool IsLiteral
		{
			get
			{
				return true;
			}
		}

		// Token: 0x060005B8 RID: 1464 RVA: 0x0000FE14 File Offset: 0x0000EE14
		public override _IExprement Duplicate()
		{
			IntegerLiteralExpression integerLiteralExpression = new IntegerLiteralExpression();
			this.DuplicateCommon(integerLiteralExpression);
			integerLiteralExpression.m_lValue = this.m_lValue;
			integerLiteralExpression.m_type = this.m_type;
			integerLiteralExpression.m_tcOriginal = this.m_tcOriginal;
			integerLiteralExpression.m_bNegative = this.m_bNegative;
			return integerLiteralExpression;
		}

		// Token: 0x040000C8 RID: 200
		[DefaultSerialization("LongValue")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		protected long m_lValue;

		// Token: 0x040000C9 RID: 201
		[DefaultSerialization("TypeClass")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		protected TypeClass m_type = TypeClass.None;

		// Token: 0x040000CA RID: 202
		[DefaultSerialization("Negative")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		protected bool m_bNegative;
	}
}
