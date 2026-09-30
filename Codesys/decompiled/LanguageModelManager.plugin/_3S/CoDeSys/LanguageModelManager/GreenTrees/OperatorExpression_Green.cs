using System;
using System.Collections.Generic;
using System.Diagnostics;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.GreenTrees
{
	// Token: 0x020001FB RID: 507
	internal class OperatorExpression_Green : Expression_Green, _IOperatorExpression, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression, IOperatorExpression
	{
		// Token: 0x060022B2 RID: 8882 RVA: 0x0005AFEE File Offset: 0x00059FEE
		public OperatorExpression_Green(Operator op, _IExpression[] expoperands)
		{
			this.m_op = op;
			this._operands = OperandList.CreateOperandList(expoperands);
		}

		// Token: 0x170009AE RID: 2478
		// (get) Token: 0x060022B3 RID: 8883 RVA: 0x0005B009 File Offset: 0x0005A009
		// (set) Token: 0x060022B4 RID: 8884 RVA: 0x0005A471 File Offset: 0x00059471
		public Operator Code
		{
			get
			{
				return this.m_op;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		// Token: 0x170009AF RID: 2479
		// (get) Token: 0x060022B5 RID: 8885 RVA: 0x0005B014 File Offset: 0x0005A014
		public IExpression[] Operands
		{
			get
			{
				_IExpression[] array = new _IExpression[this._operands.Count];
				this._operands.CopyTo(array, 0);
				return array;
			}
		}

		// Token: 0x170009B0 RID: 2480
		// (get) Token: 0x060022B6 RID: 8886 RVA: 0x0005B042 File Offset: 0x0005A042
		public IList<_IExpression> _OperandsList
		{
			get
			{
				return this._operands;
			}
		}

		// Token: 0x170009B1 RID: 2481
		public _IExpression this[int i]
		{
			get
			{
				return this._operands[i];
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		// Token: 0x170009B2 RID: 2482
		// (get) Token: 0x060022B9 RID: 8889 RVA: 0x0005B058 File Offset: 0x0005A058
		// (set) Token: 0x060022BA RID: 8890 RVA: 0x0005A471 File Offset: 0x00059471
		public bool PositionOK
		{
			get
			{
				return this.AllPositionsAllowed || this._bPosOK;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		// Token: 0x170009B3 RID: 2483
		// (get) Token: 0x060022BB RID: 8891 RVA: 0x0005B06A File Offset: 0x0005A06A
		public bool AllPositionsAllowed
		{
			get
			{
				return Array.Find<Operator>(OperatorExpression_Green.s_PositionLimitedOperators, (Operator elem) => elem == this.Code) == Operator.None;
			}
		}

		// Token: 0x060022BC RID: 8892 RVA: 0x0005A8D1 File Offset: 0x000598D1
		[Obsolete("no list access")]
		public void AddOperand(_IExpression exp)
		{
			Debug.Assert(false);
		}

		// Token: 0x060022BD RID: 8893 RVA: 0x00010A62 File Offset: 0x0000FA62
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x060022BE RID: 8894 RVA: 0x0005B088 File Offset: 0x0005A088
		public void AcceptOperatorVisitor(IOperatorExpressionVisitor visitor)
		{
			Operator code = this.Code;
			switch (code)
			{
			case Operator.__Reloc:
				visitor.visitReloc(this);
				return;
			case Operator.__Copy:
			case Operator.__Lazy:
			case Operator.Any:
			case Operator.AnyBit:
			case Operator.AnyDate:
			case Operator.AnyInt:
			case Operator.AnyNum:
			case Operator.AnyReal:
			case Operator.Bit:
			case Operator.Bool:
			case Operator.Byte:
			case Operator.Word:
			case Operator.DWord:
			case Operator.LWord:
			case Operator.SInt:
			case Operator.Int:
			case Operator.DInt:
			case Operator.LInt:
			case Operator.USInt:
			case Operator.UInt:
			case Operator.UDInt:
			case Operator.ULInt:
			case Operator.Real:
			case Operator.LReal:
			case Operator.String:
			case Operator.WString:
			case Operator.Date:
			case Operator.DateAndTime:
			case Operator.TimeOfDay:
				goto IL_50B;
			case Operator.Time:
				visitor.visitTime(this);
				return;
			case Operator.LTime:
				visitor.visitLTime(this);
				return;
			case Operator.Adr:
				visitor.visitAdr(this);
				return;
			case Operator.BitAdr:
				visitor.visitBitAdr(this);
				return;
			case Operator.IndexOf:
				visitor.visitIndexOf(this);
				return;
			case Operator.SizeOf:
				visitor.visitSizeOf(this);
				return;
			case Operator.Ini:
				visitor.visitIni(this);
				return;
			case Operator.Abs:
				visitor.visitAbs(this);
				return;
			case Operator.Limit:
			case Operator.Min:
			case Operator.Max:
			case Operator.Mux:
			case Operator.Sel:
				visitor.visitSelection(this);
				return;
			case Operator.Trunc:
				visitor.visitTrunc(this);
				return;
			case Operator.Rol:
			case Operator.Ror:
			case Operator.Shl:
			case Operator.Shr:
				visitor.visitShiftOps(this);
				return;
			case Operator.Exp:
			case Operator.Expt:
			case Operator.Sqrt:
			case Operator.Ln:
			case Operator.Log:
			case Operator.Sin:
			case Operator.Cos:
			case Operator.Tan:
			case Operator.ASin:
			case Operator.ACos:
			case Operator.ATan:
				break;
			default:
				switch (code)
				{
				case Operator.Add:
				case Operator.Sub:
				case Operator.Mul:
				case Operator.Div:
				case Operator.Mod:
				case Operator.Plus:
				case Operator.Minus:
				case Operator.Times:
				case Operator.Divide:
					visitor.visitArithmetics(this);
					return;
				case Operator.And:
				case Operator.AndN:
				case Operator.Or:
				case Operator.OrN:
				case Operator.Xor:
				case Operator.XorN:
				case Operator.Not:
				case Operator.Ampersand:
				case Operator.VerticalLine:
				case Operator.And_Then:
				case Operator.Or_Else:
					visitor.visitBoolOps(this);
					return;
				case Operator.Eq:
				case Operator.Ne:
				case Operator.Ge:
				case Operator.Gt:
				case Operator.Le:
				case Operator.Lt:
				case Operator.Less:
				case Operator.Greater:
				case Operator.LessEqual:
				case Operator.GreaterEqual:
				case Operator.Equal:
				case Operator.NotEqual:
					visitor.visitComparisons(this);
					return;
				case Operator.Cal:
				case Operator.CalC:
				case Operator.CalCN:
				case Operator.Jmp:
				case Operator.JmpC:
				case Operator.JmpCN:
				case Operator.Ret:
				case Operator.RetC:
				case Operator.RetCN:
				case Operator.Ld:
				case Operator.LdN:
				case Operator.St:
				case Operator.StN:
				case Operator.R:
				case Operator.S:
				case Operator.Period:
				case Operator.Colon:
				case Operator.Assign:
				case Operator.SetAssign:
				case Operator.ResetAssign:
				case Operator.LeftParenthesis:
				case Operator.RightParenthesis:
				case Operator.LeftBracket:
				case Operator.RightBracket:
				case Operator.Comma:
				case Operator.Semicolon:
				case Operator.Range:
				case Operator.AssignOut:
				case Operator.DeRef:
				case Operator.Conversion:
				case Operator.RefAssign:
				case Operator.Reference:
				case Operator.Property:
				case Operator.FupAssign:
				case Operator.__SystemScope:
				case Operator.__New:
				case Operator.__Cast:
				case Operator.__Wait:
				case Operator.SafeBool:
				case Operator.SafeByte:
				case Operator.SafeUSInt:
				case Operator.SafeSInt:
				case Operator.SafeWord:
				case Operator.SafeUInt:
				case Operator.SafeInt:
				case Operator.SafeDWord:
				case Operator.SafeUDInt:
				case Operator.SafeTime:
				case Operator.SafeDInt:
				case Operator.SafeLWord:
				case Operator.SafeULInt:
				case Operator.SafeLInt:
				case Operator.Class:
				case Operator.Abstract:
				case Operator.Override:
				case Operator.Public:
				case Operator.Private:
				case Operator.Protected:
				case Operator.Internal:
				case Operator.Final:
				case Operator.__XWord:
				case Operator.__UXInt:
				case Operator.__XInt:
				case Operator.__Try:
				case Operator.__EndTry:
				case Operator.__Catch:
				case Operator.__Finally:
				case Operator.__XString:
				case Operator.VarInst:
				case Operator.AnyString:
				case Operator.__PoolScope:
				case Operator.__Vector:
					goto IL_50B;
				case Operator.Move:
					visitor.visitMove(this);
					return;
				case Operator.TestAndSet:
					visitor.visitTestAndSet(this);
					return;
				case Operator.Power:
					break;
				case Operator.TruncInt:
					visitor.visitTruncInt(this);
					return;
				case Operator.__LocalOffset:
					visitor.visitLocalOffset(this);
					return;
				case Operator.__VarInfo:
					visitor.visitVarInfo(this);
					return;
				case Operator.__TypeOf:
					visitor.visitTypeOf(this);
					return;
				case Operator.__CRC:
					visitor.visitCRC(this);
					return;
				case Operator.__MaxOffset:
					visitor.visitMaxOffset(this);
					return;
				case Operator.__Init:
					visitor.visitInit(this);
					return;
				case Operator.__IsValidRef:
					visitor.visitIsValidRef(this);
					return;
				case Operator.__QueryInterface:
					visitor.visitQueryInterface(this);
					return;
				case Operator.__QueryPointer:
					visitor.visitQueryPointer(this);
					return;
				case Operator.__Delete:
					visitor.visitDelete(this);
					return;
				case Operator.__AdrInst:
					visitor.visitAdrInst(this);
					return;
				case Operator.__RefAdr:
					visitor.visitRefAdr(this);
					return;
				case Operator.__BitOffset:
					visitor.visitBitOffset(this);
					return;
				case Operator.__FCall:
					visitor.visitFCall(this);
					return;
				case Operator.__PropertyInfo:
					visitor.visitPropertyInfo(this);
					return;
				case Operator.__MemorySet:
					visitor.visitMemorySet(this);
					return;
				case Operator.__GetLTick:
					visitor.visitGetLTick(this);
					return;
				case Operator.__Throw:
					visitor.visitThrow(this);
					return;
				case Operator.__CheckLicense:
					visitor.visitCheckLicense(this);
					return;
				case Operator.__CallInitFunction:
					visitor.visitCallInitFunction(this);
					return;
				case Operator.__LateCompiledExpr:
					visitor.visitLateCompiledExpr(this);
					return;
				case Operator.LowerBound:
				case Operator.UpperBound:
					visitor.visitLowerUpperBound(this);
					return;
				case Operator.__CheckLicenseBit:
					(visitor as IOperatorExpressionVisitor2).visitCheckLicenseBit(this);
					return;
				case Operator.__XAdd:
					(visitor as IOperatorExpressionVisitor3).visitXAdd(this);
					return;
				case Operator.__MemoryBarrier:
					(visitor as IOperatorExpressionVisitor4).visitMemoryBarrier(this);
					return;
				case Operator.__CurrentTask:
					visitor.visitCurrentTask(this);
					return;
				case Operator.__CompareAndSwap:
					(visitor as IOperatorExpressionVisitor5).visitCompareAndSwap(this);
					return;
				case Operator.__vcAdd:
				case Operator.__vcSub:
				case Operator.__vcMul:
				case Operator.__vcDiv:
				case Operator.__vcDot:
				case Operator.__vcSqrt:
				case Operator.__vcMin:
				case Operator.__vcMax:
				case Operator.__vcSetReal:
				case Operator.__vcSetLReal:
				case Operator.__vcLoadReal:
				case Operator.__vcLoadLReal:
				case Operator.__vcStore:
					((IOperatorExpressionVisitor4)visitor).visitVector(this);
					return;
				default:
				{
					if (code != Operator.XSizeOf)
					{
						goto IL_50B;
					}
					IOperatorExpressionVisitor6 operatorExpressionVisitor = visitor as IOperatorExpressionVisitor6;
					if (operatorExpressionVisitor != null)
					{
						operatorExpressionVisitor.visitXSizeOf(this);
						return;
					}
					return;
				}
				}
				break;
			}
			visitor.visitTrigonometrics(this);
			return;
			IL_50B:
			Debug.Assert(false);
		}

		// Token: 0x060022BF RID: 8895 RVA: 0x00010F2D File Offset: 0x0000FF2D
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x040006AB RID: 1707
		private static Operator[] s_PositionLimitedOperators = new Operator[]
		{
			Operator.__CheckLicense,
			Operator.__CheckLicenseBit
		};

		// Token: 0x040006AC RID: 1708
		private readonly bool _bPosOK;

		// Token: 0x040006AD RID: 1709
		private readonly OperandList _operands;

		// Token: 0x040006AE RID: 1710
		private readonly Operator m_op;
	}
}
