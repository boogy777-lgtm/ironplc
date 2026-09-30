using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000049 RID: 73
	[TypeGuid("{2b7266d6-1783-4e2a-8690-e35219f7fc36}")]
	[StorageVersion("3.3.0.0")]
	public class ConversionExpression : Expression, _IConversionExpression, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression, IConversionExpression, IPositionExprement, ILengthExprement
	{
		// Token: 0x0600040F RID: 1039 RVA: 0x0000D681 File Offset: 0x0000C681
		public ConversionExpression()
		{
		}

		// Token: 0x06000410 RID: 1040 RVA: 0x0000D699 File Offset: 0x0000C699
		internal ConversionExpression(TypeClass tcFrom, TypeClass tcTo)
		{
			this.m_tcFrom = tcFrom;
			this.m_tcTo = tcTo;
		}

		// Token: 0x06000411 RID: 1041 RVA: 0x0000D6BF File Offset: 0x0000C6BF
		internal ConversionExpression(TypeClass tcFrom, TypeClass tcTo, IToken token) : base(token)
		{
			this.m_tcFrom = tcFrom;
			this.m_tcTo = tcTo;
		}

		// Token: 0x06000412 RID: 1042 RVA: 0x00004E6B File Offset: 0x00003E6B
		public override bool IsLValue(IScope scope, bool bWriteToConstants)
		{
			return false;
		}

		// Token: 0x170000D4 RID: 212
		// (get) Token: 0x06000413 RID: 1043 RVA: 0x00004E6B File Offset: 0x00003E6B
		public virtual bool Implicit
		{
			get
			{
				return false;
			}
		}

		// Token: 0x170000D5 RID: 213
		// (get) Token: 0x06000414 RID: 1044 RVA: 0x0000D6E6 File Offset: 0x0000C6E6
		// (set) Token: 0x06000415 RID: 1045 RVA: 0x0000D6EE File Offset: 0x0000C6EE
		public TypeClass From
		{
			get
			{
				return this.m_tcFrom;
			}
			set
			{
				this.m_tcFrom = value;
			}
		}

		// Token: 0x170000D6 RID: 214
		// (get) Token: 0x06000416 RID: 1046 RVA: 0x0000D6F7 File Offset: 0x0000C6F7
		// (set) Token: 0x06000417 RID: 1047 RVA: 0x0000D6FF File Offset: 0x0000C6FF
		public TypeClass To
		{
			get
			{
				return this.m_tcTo;
			}
			set
			{
				this.m_tcTo = value;
			}
		}

		// Token: 0x170000D7 RID: 215
		// (get) Token: 0x06000418 RID: 1048 RVA: 0x0000D708 File Offset: 0x0000C708
		public IExpression Exp
		{
			get
			{
				return this._Exp;
			}
		}

		// Token: 0x170000D8 RID: 216
		// (get) Token: 0x06000419 RID: 1049 RVA: 0x0000D710 File Offset: 0x0000C710
		// (set) Token: 0x0600041A RID: 1050 RVA: 0x0000D726 File Offset: 0x0000C726
		public _IExpression _Exp
		{
			get
			{
				if (this.m_exp == null)
				{
					return new NullExpression();
				}
				return this.m_exp;
			}
			set
			{
				this.m_exp = value;
			}
		}

		// Token: 0x0600041B RID: 1051 RVA: 0x0000D72F File Offset: 0x0000C72F
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x0600041C RID: 1052 RVA: 0x0000D738 File Offset: 0x0000C738
		public override T Accept<T>(IExprementVisitor<T> visitor)
		{
			return visitor.visit(this);
		}

		// Token: 0x0600041D RID: 1053 RVA: 0x0000D741 File Offset: 0x0000C741
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x0600041E RID: 1054 RVA: 0x0000D74C File Offset: 0x0000C74C
		public override _IExprement Duplicate()
		{
			ConversionExpression conversionExpression = new ConversionExpression(this.m_tcFrom, this.m_tcTo);
			this.DuplicateCommon(conversionExpression);
			conversionExpression._Exp = (this._Exp.Duplicate() as Expression);
			return conversionExpression;
		}

		// Token: 0x0600041F RID: 1055 RVA: 0x0000D789 File Offset: 0x0000C789
		public override bool IsConstant(IScope scope, bool bAllocatedOK)
		{
			return this._Exp.IsConstant(scope, bAllocatedOK);
		}

		// Token: 0x06000420 RID: 1056 RVA: 0x0000D798 File Offset: 0x0000C798
		private ILiteralValue CreateUnsignedValue(bool bulongInvalid, ulong ul)
		{
			if (bulongInvalid)
			{
				return null;
			}
			return new LiteralValue(ul);
		}

		// Token: 0x06000421 RID: 1057 RVA: 0x0000D7AA File Offset: 0x0000C7AA
		private ILiteralValue CreateSignedValue(bool blongInvalid, long l)
		{
			if (blongInvalid)
			{
				return null;
			}
			return new LiteralValue(l);
		}

		// Token: 0x06000422 RID: 1058 RVA: 0x0000D7BC File Offset: 0x0000C7BC
		private ILiteralValue CreateDoubleValue(bool bdoubleInvalid, double d)
		{
			if (bdoubleInvalid)
			{
				return null;
			}
			return new LiteralValue(d);
		}

		// Token: 0x06000423 RID: 1059 RVA: 0x0000D7D0 File Offset: 0x0000C7D0
		[SuppressMessage("Critical Code Smell", "S3776:Cognitive Complexity of methods should not be too high", Justification = "Will be fixed with CDS-86324")]
		protected virtual ILiteralValue GetConversionValue(ILiteralValue litvalExp)
		{
			if (litvalExp == null)
			{
				return null;
			}
			string text = null;
			double num = 0.0;
			bool flag = false;
			long num2 = 0L;
			ulong num3 = 0UL;
			bool bdoubleInvalid = false;
			bool blongInvalid = false;
			bool bulongInvalid = false;
			switch (litvalExp.KindOf)
			{
			case KindOfLiteral.SignedInteger:
				num2 = litvalExp.SignedLong;
				num = (double)num2;
				flag = (num2 != 0L);
				text = num2.ToString();
				num3 = (ulong)num2;
				break;
			case KindOfLiteral.UnsignedInteger:
				num3 = litvalExp.UnsignedLong;
				num = num3;
				flag = (num3 > 0UL);
				text = num3.ToString();
				num2 = (long)num3;
				break;
			case KindOfLiteral.Float:
				num = litvalExp.Float;
				flag = (num != 0.0);
				text = num.ToString();
				if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV34400)
				{
					if (num >= 0.0)
					{
						num2 = (long)(num + 0.5);
						num3 = (ulong)(num + 0.5);
					}
					else
					{
						num2 = (long)(num - 0.5);
						num3 = (ulong)(num - 0.5);
					}
				}
				else
				{
					num2 = (long)num;
					num3 = (ulong)num;
				}
				break;
			case KindOfLiteral.String:
				text = litvalExp.String;
				if (!double.TryParse(text, out num))
				{
					bdoubleInvalid = true;
				}
				flag = (text == "TRUE" || text == "1");
				if (!long.TryParse(text, out num2))
				{
					blongInvalid = true;
				}
				if (!ulong.TryParse(text, out num3))
				{
					bulongInvalid = true;
				}
				break;
			case KindOfLiteral.Bool:
				flag = litvalExp.Bool;
				num = (double)(flag ? 1 : 0);
				text = (flag ? "TRUE" : "FALSE");
				num2 = (flag ? 1L : 0L);
				num3 = (flag ? 1UL : 0UL);
				break;
			}
			switch (this.m_tcTo)
			{
			case TypeClass.Bool:
				return new LiteralValue(flag);
			case TypeClass.Byte:
			case TypeClass.USInt:
				num3 = (ulong)((byte)num3);
				return this.CreateUnsignedValue(bulongInvalid, num3);
			case TypeClass.Word:
			case TypeClass.UInt:
				num3 = (ulong)((ushort)num3);
				return this.CreateUnsignedValue(bulongInvalid, num3);
			case TypeClass.DWord:
			case TypeClass.UDInt:
			case TypeClass.Time:
			case TypeClass.DateAndTime:
			case TypeClass.TimeOfDay:
				num3 = (ulong)((uint)num3);
				return this.CreateUnsignedValue(bulongInvalid, num3);
			case TypeClass.LWord:
			case TypeClass.ULInt:
				return this.CreateUnsignedValue(bulongInvalid, num3);
			case TypeClass.SInt:
				num2 = (long)((sbyte)num2);
				return this.CreateSignedValue(blongInvalid, num2);
			case TypeClass.Int:
				num2 = (long)((short)num2);
				return this.CreateSignedValue(blongInvalid, num2);
			case TypeClass.DInt:
				num2 = (long)((int)num2);
				return this.CreateSignedValue(blongInvalid, num2);
			case TypeClass.LInt:
			case TypeClass.LTime:
			case TypeClass.LDateAndTime:
			case TypeClass.LTimeOfDay:
				return this.CreateSignedValue(blongInvalid, num2);
			case TypeClass.Real:
				num = (double)((float)num);
				return this.CreateDoubleValue(bdoubleInvalid, num);
			case TypeClass.LReal:
				return this.CreateDoubleValue(bdoubleInvalid, num);
			case TypeClass.String:
			case TypeClass.WString:
				return new LiteralValue(text);
			case TypeClass.Date:
				num3 = (ulong)((uint)num3);
				if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351800)
				{
					num3 -= num3 % 86400UL;
				}
				return this.CreateUnsignedValue(bulongInvalid, num3);
			case TypeClass.LDate:
				if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351800)
				{
					num2 -= num2 % 86400000000000L;
				}
				return this.CreateSignedValue(blongInvalid, num2);
			}
			return litvalExp;
		}

		// Token: 0x06000424 RID: 1060 RVA: 0x0000DB48 File Offset: 0x0000CB48
		public override ILiteralValue LiteralUnchecked(IScope scope)
		{
			IConstantFolder3 constantFolder = VersionedCompilerFactory._ConstantFolder_OrNull as IConstantFolder3;
			if (constantFolder != null)
			{
				bool flag;
				return constantFolder.GetLiteralValue(this, scope, out flag);
			}
			return this.GetConversionValue(this._Exp.LiteralUnchecked(scope));
		}

		// Token: 0x06000425 RID: 1061 RVA: 0x0000DB80 File Offset: 0x0000CB80
		public override ILiteralValue Literal(IScope scope, bool bAllocatedOK)
		{
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351600)
			{
				bool flag;
				return base.LiteralWithRecursionCheck(scope, new LDictionary<IVariable, IVariable>(), bAllocatedOK, out flag);
			}
			return base.Literal(scope, bAllocatedOK);
		}

		// Token: 0x06000426 RID: 1062 RVA: 0x0000DBB8 File Offset: 0x0000CBB8
		public override ILiteralValue Literal(IScope scope)
		{
			bool flag;
			return base.LiteralWithRecursionCheck(scope, new LDictionary<IVariable, IVariable>(), false, out flag);
		}

		// Token: 0x06000427 RID: 1063 RVA: 0x0000DBD4 File Offset: 0x0000CBD4
		public override ILiteralValue Literal(IPrecompileScope scope)
		{
			bool greaterEqualV = APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35900;
			bool flag;
			return base.LiteralWithRecursionCheck(scope, new LDictionary<IVariable, IVariable>(), greaterEqualV, out flag);
		}

		// Token: 0x06000428 RID: 1064 RVA: 0x0000DC00 File Offset: 0x0000CC00
		[SuppressMessage("Critical Code Smell", "S3776:Cognitive Complexity of methods should not be too high", Justification = "Will be fixed with CDS-86324")]
		public override ILiteralValue LiteralWithRecursionCheck(IScope scope, IRecursionGuard recursionGuard, bool bAllocatedOK, out bool bRecursionError)
		{
			IConstantFolder3 constantFolder = VersionedCompilerFactory._ConstantFolder_OrNull as IConstantFolder3;
			if (constantFolder != null)
			{
				return constantFolder.GetLiteralValue(this, scope, out bRecursionError);
			}
			bRecursionError = false;
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351000 && ((this.From == TypeClass.Time && this.To == TypeClass.LTime) || (this.From == TypeClass.LTime && this.To == TypeClass.Time) || (this.From == TypeClass.DateAndTime && this.To == TypeClass.Time) || (this.From == TypeClass.DateAndTime && this.To == TypeClass.LTime) || (this.From == TypeClass.Date && this.To == TypeClass.Time) || (this.From == TypeClass.Date && this.To == TypeClass.LTime) || (this.From == TypeClass.Time && this.To == TypeClass.Date) || (this.From == TypeClass.LTime && this.To == TypeClass.Date) || (this.From == TypeClass.Time && this.To == TypeClass.DateAndTime) || (this.From == TypeClass.LTime && this.To == TypeClass.DateAndTime) || (this.From == TypeClass.TimeOfDay && this.To == TypeClass.DateAndTime) || (this.From == TypeClass.Date && this.To == TypeClass.TimeOfDay) || (this.From == TypeClass.TimeOfDay && this.To == TypeClass.Date) || (this.From == TypeClass.DateAndTime && this.To == TypeClass.TimeOfDay) || (this.From == TypeClass.DateAndTime && this.To == TypeClass.Date)))
			{
				return null;
			}
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351600)
			{
				if ((this.From == TypeClass.LDate || this.From == TypeClass.LDateAndTime || this.From == TypeClass.LTimeOfDay) && TypeTable.IsTimeOrDateType(this.To))
				{
					return null;
				}
				if (TypeTable.IsTimeOrDateType(this.From) && (this.To == TypeClass.LDate || this.To == TypeClass.LDateAndTime || this.To == TypeClass.LTimeOfDay))
				{
					return null;
				}
			}
			return this.GetConversionValue(((_IExpression2)this._Exp).LiteralWithRecursionCheck(scope, recursionGuard, bAllocatedOK, out bRecursionError));
		}

		// Token: 0x06000429 RID: 1065 RVA: 0x0000DE0C File Offset: 0x0000CE0C
		public override ILiteralValue LiteralWithRecursionCheck(IPrecompileScope scope, IRecursionGuard recursionGuard, bool bAllocatedOK, out bool bRecursionError)
		{
			IConstantFolder3 constantFolder = VersionedCompilerFactory._ConstantFolder_OrNull as IConstantFolder3;
			if (constantFolder != null)
			{
				return constantFolder.GetLiteralValue(this, scope, recursionGuard, out bRecursionError);
			}
			return this.GetConversionValue(((_IExpression2)this._Exp).LiteralWithRecursionCheck(scope, recursionGuard, bAllocatedOK, out bRecursionError));
		}

		// Token: 0x170000D9 RID: 217
		// (get) Token: 0x0600042A RID: 1066 RVA: 0x0000DE4E File Offset: 0x0000CE4E
		// (set) Token: 0x0600042B RID: 1067 RVA: 0x0000DE6F File Offset: 0x0000CE6F
		[DefaultSerialization("Type")]
		[StorageVersion("3.3.0.10")]
		[StorageIgnorable]
		[Obfuscation(Feature = "rename")]
		public override ICompiledType _CompiledType
		{
			get
			{
				if (this.m_CompiledType == null)
				{
					this.m_CompiledType = TypeTable.Get(this.m_tcTo);
				}
				return this.m_CompiledType;
			}
			set
			{
				this.m_CompiledType = value;
			}
		}

		// Token: 0x170000DA RID: 218
		// (get) Token: 0x0600042C RID: 1068 RVA: 0x0000DE78 File Offset: 0x0000CE78
		// (set) Token: 0x0600042D RID: 1069 RVA: 0x0000DE80 File Offset: 0x0000CE80
		[Obfuscation(Feature = "rename")]
		public override IMinimalPosition PositionIntern
		{
			get
			{
				return this.m_position;
			}
			set
			{
				this.m_position = value;
			}
		}

		// Token: 0x170000DB RID: 219
		// (get) Token: 0x0600042E RID: 1070 RVA: 0x0000DE89 File Offset: 0x0000CE89
		// (set) Token: 0x0600042F RID: 1071 RVA: 0x0000DE91 File Offset: 0x0000CE91
		[Obfuscation(Feature = "rename")]
		public override short LengthIntern
		{
			get
			{
				return this.m_sLength;
			}
			set
			{
				this.m_sLength = value;
			}
		}

		// Token: 0x040000A0 RID: 160
		private const uint SECONDS_PER_DAY = 86400U;

		// Token: 0x040000A1 RID: 161
		private const long NANOSECONDS_PER_DAY = 86400000000000L;

		// Token: 0x040000A2 RID: 162
		[DefaultSerialization("CompiledType")]
		[StorageVersion("3.5.15.0")]
		[StorageDefaultValue(null)]
		[Obfuscation(Feature = "rename")]
		private ICompiledType m_CompiledType;

		// Token: 0x040000A3 RID: 163
		[Obfuscation(Feature = "rename")]
		private IMinimalPosition m_position;

		// Token: 0x040000A4 RID: 164
		[Obfuscation(Feature = "rename")]
		protected short m_sLength;

		// Token: 0x040000A5 RID: 165
		[DefaultSerialization("From")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		protected TypeClass m_tcFrom = TypeClass.None;

		// Token: 0x040000A6 RID: 166
		[DefaultSerialization("To")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		protected TypeClass m_tcTo = TypeClass.None;

		// Token: 0x040000A7 RID: 167
		[DefaultSerialization("Expression")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		protected _IExpression m_exp;
	}
}
