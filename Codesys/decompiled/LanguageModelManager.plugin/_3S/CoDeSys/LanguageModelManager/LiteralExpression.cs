using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000061 RID: 97
	public abstract class LiteralExpression : PositionExpression, _ILiteralExpression, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression, ILiteralExpression2, ILiteralExpression
	{
		// Token: 0x060005C8 RID: 1480 RVA: 0x0000FEBD File Offset: 0x0000EEBD
		protected LiteralExpression()
		{
		}

		// Token: 0x060005C9 RID: 1481 RVA: 0x0000FECD File Offset: 0x0000EECD
		protected LiteralExpression(IToken token) : base(token)
		{
		}

		// Token: 0x060005CA RID: 1482 RVA: 0x0000FEE0 File Offset: 0x0000EEE0
		public bool LiteralValueEquals(_ILiteralExpression other)
		{
			bool flag = true;
			if (this.ConstantType != other.ConstantType)
			{
				switch (other.ConstantType)
				{
				case TypeClass.AnyInt:
					flag = TypeTable.IsInteger(this.ConstantType);
					break;
				case TypeClass.AnyNum:
					flag = TypeTable.IsNumber(this.ConstantType);
					break;
				case TypeClass.AnyReal:
					flag = TypeTable.IsReal(this.ConstantType);
					break;
				default:
					flag = (this.ConstantType == other.ConstantType);
					break;
				}
			}
			if (flag)
			{
				flag = (this.LongValue == other.LongValue && this.ULongValue == other.ULongValue && this.StringValue == other.StringValue && this.RealValue == other.RealValue);
			}
			return flag;
		}

		// Token: 0x060005CB RID: 1483 RVA: 0x0000FF9C File Offset: 0x0000EF9C
		public static LiteralExpression CreateDefaultLiteralExpression(TypeClass tc)
		{
			switch (tc)
			{
			case TypeClass.Bool:
			case TypeClass.Bit:
			case TypeClass.BitConst:
				return LiteralExpression.CreateLiteralExpression(false);
			case TypeClass.Byte:
			case TypeClass.Word:
			case TypeClass.DWord:
			case TypeClass.LWord:
			case TypeClass.SInt:
			case TypeClass.Int:
			case TypeClass.DInt:
			case TypeClass.LInt:
			case TypeClass.USInt:
			case TypeClass.UInt:
			case TypeClass.UDInt:
			case TypeClass.ULInt:
			case TypeClass.AnyBit:
				return LiteralExpression.CreateLiteralExpression(0L, tc);
			case TypeClass.Real:
			case TypeClass.LReal:
			case TypeClass.AnyReal:
				return LiteralExpression.CreateLiteralExpression(0.0, tc);
			case TypeClass.String:
			case TypeClass.WString:
				return LiteralExpression.CreateLiteralExpression(string.Empty);
			}
			return null;
		}

		// Token: 0x060005CC RID: 1484 RVA: 0x00010079 File Offset: 0x0000F079
		public static LiteralExpression CreateLiteralExpression(long lVal, TypeClass tc)
		{
			return new IntegerLiteralExpression(lVal, tc);
		}

		// Token: 0x060005CD RID: 1485 RVA: 0x00010082 File Offset: 0x0000F082
		public static LiteralExpression CreateLiteralExpression(long lVal, TypeClass tc, int nBase)
		{
			return LiteralExpression.CreateLiteralExpression(lVal, tc, nBase, false);
		}

		// Token: 0x060005CE RID: 1486 RVA: 0x0001008D File Offset: 0x0000F08D
		public static LiteralExpression CreateLiteralExpression(long lVal, TypeClass tc, int nBase, bool negative)
		{
			if (nBase != 10)
			{
				return new BasedIntegerLiteralExpression(lVal, tc, nBase, negative);
			}
			return new IntegerLiteralExpression(lVal, tc, negative);
		}

		// Token: 0x060005CF RID: 1487 RVA: 0x000100A6 File Offset: 0x0000F0A6
		public static LiteralExpression CreateLiteralExpression(long lVal, TypeClass tc, IToken token)
		{
			return new IntegerLiteralExpression(lVal, tc, token);
		}

		// Token: 0x060005D0 RID: 1488 RVA: 0x000100B0 File Offset: 0x0000F0B0
		public static LiteralExpression CreateLiteralExpression(long lVal, TypeClass tc, IToken token, int nBase)
		{
			return LiteralExpression.CreateLiteralExpression(lVal, tc, token, nBase, false);
		}

		// Token: 0x060005D1 RID: 1489 RVA: 0x000100BC File Offset: 0x0000F0BC
		public static LiteralExpression CreateLiteralExpression(long lVal, TypeClass tc, IToken token, int nBase, bool negative)
		{
			if (nBase != 10)
			{
				return new BasedIntegerLiteralExpression(lVal, tc, token, nBase, negative);
			}
			return new IntegerLiteralExpression(lVal, tc, token, negative);
		}

		// Token: 0x060005D2 RID: 1490 RVA: 0x000100D9 File Offset: 0x0000F0D9
		public static LiteralExpression CreateLiteralExpression(ulong ulVal, TypeClass tc, IToken token)
		{
			return new IntegerLiteralExpression(ulVal, tc, token);
		}

		// Token: 0x060005D3 RID: 1491 RVA: 0x000100E3 File Offset: 0x0000F0E3
		public static LiteralExpression CreateLiteralExpression(string stVal, TypeClass tc, IToken token)
		{
			return new StringLiteralExpression(stVal, tc, token);
		}

		// Token: 0x060005D4 RID: 1492 RVA: 0x000100ED File Offset: 0x0000F0ED
		public static LiteralExpression CreateLiteralExpression(string stVal, TypeClass tc, IToken token, StringEncoding stringEncoding)
		{
			return new StringLiteralExpression(stVal, tc, token, stringEncoding);
		}

		// Token: 0x060005D5 RID: 1493 RVA: 0x000100F8 File Offset: 0x0000F0F8
		public static LiteralExpression CreateLiteralExpression(double dVal, TypeClass tc, IToken token)
		{
			return new FloatLiteralExpression(dVal, tc, token);
		}

		// Token: 0x060005D6 RID: 1494 RVA: 0x00010102 File Offset: 0x0000F102
		public static LiteralExpression CreateLiteralExpression(ulong ulVal, TypeClass tc)
		{
			return new IntegerLiteralExpression(ulVal, tc);
		}

		// Token: 0x060005D7 RID: 1495 RVA: 0x0001010B File Offset: 0x0000F10B
		public static LiteralExpression CreateLiteralExpression(string stVal, TypeClass tc)
		{
			return new StringLiteralExpression(stVal, tc);
		}

		// Token: 0x060005D8 RID: 1496 RVA: 0x00010114 File Offset: 0x0000F114
		public static LiteralExpression CreateLiteralExpression(double dVal, TypeClass tc)
		{
			return new FloatLiteralExpression(dVal, tc);
		}

		// Token: 0x060005D9 RID: 1497 RVA: 0x0001011D File Offset: 0x0000F11D
		public static LiteralExpression CreateLiteralExpression(long lVal)
		{
			return new IntegerLiteralExpression(lVal);
		}

		// Token: 0x060005DA RID: 1498 RVA: 0x00010128 File Offset: 0x0000F128
		public static LiteralExpression CreateLiteralExpression(ILiteralValue2 litVal, TypeClass tc)
		{
			switch (litVal.KindOf)
			{
			case KindOfLiteral.SignedInteger:
				return LiteralExpression.CreateLiteralExpression(litVal.SignedLong, tc);
			case KindOfLiteral.UnsignedInteger:
				return LiteralExpression.CreateLiteralExpression(litVal.UnsignedLong, tc);
			case KindOfLiteral.Float:
				return LiteralExpression.CreateLiteralExpression(litVal.Float, tc);
			case KindOfLiteral.String:
				return LiteralExpression.CreateLiteralExpression(litVal.String);
			case KindOfLiteral.Bool:
				return LiteralExpression.CreateLiteralExpression(litVal.Bool);
			default:
				return null;
			}
		}

		// Token: 0x060005DB RID: 1499 RVA: 0x00010198 File Offset: 0x0000F198
		public static LiteralExpression CreateLiteralExpression(ulong ulVal)
		{
			return new IntegerLiteralExpression(ulVal);
		}

		// Token: 0x060005DC RID: 1500 RVA: 0x000101A0 File Offset: 0x0000F1A0
		public static LiteralExpression CreateLiteralExpression(string stVal)
		{
			return new StringLiteralExpression(stVal);
		}

		// Token: 0x060005DD RID: 1501 RVA: 0x000101A8 File Offset: 0x0000F1A8
		public static LiteralExpression CreateLiteralExpression(double dVal)
		{
			return new FloatLiteralExpression(dVal);
		}

		// Token: 0x060005DE RID: 1502 RVA: 0x000101B0 File Offset: 0x0000F1B0
		internal static LiteralExpression CreateLiteralExpression(bool bVal)
		{
			if (bVal)
			{
				return LiteralExpression.CreateLiteralExpression(1L, TypeClass.Bool);
			}
			return LiteralExpression.CreateLiteralExpression(0L, TypeClass.Bool);
		}

		// Token: 0x060005DF RID: 1503 RVA: 0x000101C6 File Offset: 0x0000F1C6
		public override bool IsLValue(IScope scope, bool bWriteToConstants)
		{
			return this.IsLValue(scope, bWriteToConstants, false);
		}

		// Token: 0x060005E0 RID: 1504 RVA: 0x000101D1 File Offset: 0x0000F1D1
		public override bool IsLValue(IScope scope, bool bWriteToConstants, bool bPassToVarInout)
		{
			return (!APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV34400 || bPassToVarInout) && (this.ConstantType == TypeClass.String || this.ConstantType == TypeClass.WString);
		}

		// Token: 0x17000141 RID: 321
		// (get) Token: 0x060005E1 RID: 1505
		// (set) Token: 0x060005E2 RID: 1506
		public abstract long LongValue { get; set; }

		// Token: 0x17000142 RID: 322
		// (get) Token: 0x060005E3 RID: 1507
		// (set) Token: 0x060005E4 RID: 1508
		public abstract bool Negative { get; set; }

		// Token: 0x17000143 RID: 323
		// (get) Token: 0x060005E5 RID: 1509
		public abstract ulong ULongValue { get; }

		// Token: 0x17000144 RID: 324
		// (get) Token: 0x060005E6 RID: 1510
		// (set) Token: 0x060005E7 RID: 1511
		public abstract string StringValue { get; set; }

		// Token: 0x17000145 RID: 325
		// (get) Token: 0x060005E8 RID: 1512
		// (set) Token: 0x060005E9 RID: 1513
		public abstract double RealValue { get; set; }

		// Token: 0x17000146 RID: 326
		// (get) Token: 0x060005EA RID: 1514
		// (set) Token: 0x060005EB RID: 1515
		public abstract TypeClass ConstantType { get; set; }

		// Token: 0x17000147 RID: 327
		// (get) Token: 0x060005EC RID: 1516 RVA: 0x00010200 File Offset: 0x0000F200
		public TypeClass OriginalType
		{
			get
			{
				return this.m_tcOriginal;
			}
		}

		// Token: 0x17000148 RID: 328
		// (get) Token: 0x060005ED RID: 1517 RVA: 0x00010208 File Offset: 0x0000F208
		// (set) Token: 0x060005EE RID: 1518 RVA: 0x00003AE9 File Offset: 0x00002AE9
		[SuppressMessage("Blocker Code Smell", "S3237:\"value\" parameters should be used", Justification = "Setter cannot be changed for compatibility reasons")]
		public virtual int Base
		{
			get
			{
				return 10;
			}
			set
			{
			}
		}

		// Token: 0x060005EF RID: 1519 RVA: 0x0001020C File Offset: 0x0000F20C
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x060005F0 RID: 1520 RVA: 0x00010215 File Offset: 0x0000F215
		public override T Accept<T>(IExprementVisitor<T> visitor)
		{
			return visitor.visit(this);
		}

		// Token: 0x060005F1 RID: 1521 RVA: 0x0001021E File Offset: 0x0000F21E
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x17000149 RID: 329
		// (get) Token: 0x060005F2 RID: 1522 RVA: 0x00005E58 File Offset: 0x00004E58
		public override bool IsLiteral
		{
			get
			{
				return true;
			}
		}

		// Token: 0x060005F3 RID: 1523 RVA: 0x00010227 File Offset: 0x0000F227
		public override ILiteralValue Literal(IScope scope)
		{
			return this.LiteralValue;
		}

		// Token: 0x060005F4 RID: 1524 RVA: 0x00010227 File Offset: 0x0000F227
		public override ILiteralValue Literal(IPrecompileScope scope)
		{
			return this.LiteralValue;
		}

		// Token: 0x060005F5 RID: 1525 RVA: 0x00010230 File Offset: 0x0000F230
		public override ILiteralValue LiteralWithRecursionCheck(IScope scope, IRecursionGuard recursionGuard, bool bAllocatedOK, out bool bRecursionError)
		{
			IConstantFolder3 constantFolder = VersionedCompilerFactory._ConstantFolder_OrNull as IConstantFolder3;
			if (constantFolder != null)
			{
				return constantFolder.GetLiteralValue(this, scope, out bRecursionError);
			}
			bRecursionError = false;
			return this.LiteralValue;
		}

		// Token: 0x060005F6 RID: 1526 RVA: 0x00010260 File Offset: 0x0000F260
		public override ILiteralValue LiteralWithRecursionCheck(IPrecompileScope scope, IRecursionGuard recursionGuard, bool bAllocatedOK, out bool bRecursionError)
		{
			IConstantFolder3 constantFolder = VersionedCompilerFactory._ConstantFolder_OrNull as IConstantFolder3;
			if (constantFolder != null)
			{
				return constantFolder.GetLiteralValue(this, scope, recursionGuard, out bRecursionError);
			}
			bRecursionError = false;
			return this.LiteralValue;
		}

		// Token: 0x1700014A RID: 330
		// (get) Token: 0x060005F7 RID: 1527 RVA: 0x00010294 File Offset: 0x0000F294
		public ILiteralValue LiteralValue
		{
			get
			{
				TypeClass constantType = this.ConstantType;
				if (constantType > TypeClass.LReal)
				{
					if (constantType - TypeClass.String > 1)
					{
						if (constantType == TypeClass.AnyReal)
						{
							goto IL_2B;
						}
						if (constantType != TypeClass.XString)
						{
							goto IL_62;
						}
					}
					return new LiteralValue(this.StringValue);
				}
				if (constantType == TypeClass.Bool)
				{
					return new LiteralValue(this.LongValue == 1L);
				}
				if (constantType - TypeClass.Real > 1)
				{
					goto IL_62;
				}
				IL_2B:
				return new LiteralValue(this.RealValue);
				IL_62:
				return (base.Type != null && !TypeTable.IsSigned(base.Type.DeRefType.Class) && TypeTable.IsInteger(base.Type.DeRefType.Class)) ? new LiteralValue(this.ULongValue) : new LiteralValue(this.LongValue);
			}
		}

		// Token: 0x060005F8 RID: 1528 RVA: 0x0000BAC0 File Offset: 0x0000AAC0
		public override _IExprement Duplicate()
		{
			Debug.Assert(false);
			return null;
		}

		// Token: 0x060005F9 RID: 1529 RVA: 0x00005E58 File Offset: 0x00004E58
		public override bool IsConstant(IScope scope, bool bAllocatedOK)
		{
			return true;
		}

		// Token: 0x1700014B RID: 331
		// (get) Token: 0x060005FA RID: 1530 RVA: 0x00010359 File Offset: 0x0000F359
		// (set) Token: 0x060005FB RID: 1531 RVA: 0x00010361 File Offset: 0x0000F361
		[DefaultSerialization("Type")]
		[StorageVersion("3.3.0.10")]
		[StorageIgnorable]
		[Obfuscation(Feature = "rename")]
		public override ICompiledType _CompiledType
		{
			get
			{
				return this.m_ctype;
			}
			set
			{
				this.m_ctype = value;
			}
		}

		// Token: 0x040000CD RID: 205
		[Obfuscation(Feature = "rename")]
		[DefaultSerialization("compiledtype")]
		[StorageVersion("3.3.0.0")]
		private ICompiledType m_ctype;

		// Token: 0x040000CE RID: 206
		[DefaultSerialization("OriginalTypeClass")]
		[StorageVersion("3.5.0.0")]
		[StorageIgnorable]
		[Obfuscation(Feature = "rename")]
		protected TypeClass m_tcOriginal = TypeClass.None;
	}
}
