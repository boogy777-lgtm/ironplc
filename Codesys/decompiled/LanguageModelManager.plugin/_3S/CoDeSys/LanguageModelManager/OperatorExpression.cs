using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.LanguageModelManager.Legacy;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000066 RID: 102
	[TypeGuid("{c8e3c9b1-32ff-4f6a-ac31-84a9afd1157f}")]
	[StorageVersion("3.3.0.0")]
	[SuppressMessage("Major Code Smell", "S1200:Classes should not be coupled to too many other classes (Single Responsibility Principle)", Justification = "Class cannot be divided into subclasses because of released interfaces")]
	public class OperatorExpression : PositionExpression, _IOperatorExpression, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression, IOperatorExpression, _IOperatorExpressionWithDetailedLiteralCheck
	{
		// Token: 0x06000655 RID: 1621 RVA: 0x00010953 File Offset: 0x0000F953
		public OperatorExpression()
		{
		}

		// Token: 0x06000656 RID: 1622 RVA: 0x0001096D File Offset: 0x0000F96D
		public OperatorExpression(Operator op)
		{
			this.m_op = op;
		}

		// Token: 0x06000657 RID: 1623 RVA: 0x0001098E File Offset: 0x0000F98E
		public OperatorExpression(Operator op, IToken token) : base(token)
		{
			this.m_op = op;
		}

		// Token: 0x06000658 RID: 1624 RVA: 0x00004E6B File Offset: 0x00003E6B
		public override bool IsLValue(IScope scope, bool bWriteToConstants)
		{
			return false;
		}

		// Token: 0x17000164 RID: 356
		// (get) Token: 0x06000659 RID: 1625 RVA: 0x000109B0 File Offset: 0x0000F9B0
		// (set) Token: 0x0600065A RID: 1626 RVA: 0x000109B8 File Offset: 0x0000F9B8
		public Operator Code
		{
			get
			{
				return this.m_op;
			}
			set
			{
				this.m_op = value;
			}
		}

		// Token: 0x17000165 RID: 357
		// (get) Token: 0x0600065B RID: 1627 RVA: 0x000109C4 File Offset: 0x0000F9C4
		public IExpression[] Operands
		{
			get
			{
				Expression[] array = new Expression[this.m_alOperands.Count];
				LList<_IExpression> alOperands = this.m_alOperands;
				_IExpression[] array2 = array;
				alOperands.CopyTo(array2, 0);
				return array;
			}
		}

		// Token: 0x17000166 RID: 358
		// (get) Token: 0x0600065C RID: 1628 RVA: 0x000109F4 File Offset: 0x0000F9F4
		public IList<_IExpression> _OperandsList
		{
			get
			{
				return this.m_alOperands;
			}
		}

		// Token: 0x17000167 RID: 359
		public _IExpression this[int i]
		{
			get
			{
				return this.m_alOperands[i] as Expression;
			}
			set
			{
				this.m_alOperands[i] = value;
			}
		}

		// Token: 0x17000168 RID: 360
		// (get) Token: 0x0600065F RID: 1631 RVA: 0x00010A1E File Offset: 0x0000FA1E
		// (set) Token: 0x06000660 RID: 1632 RVA: 0x00010A30 File Offset: 0x0000FA30
		public bool PositionOK
		{
			get
			{
				return this.AllPositionsAllowed || this._bPosOK;
			}
			set
			{
				this._bPosOK = value;
			}
		}

		// Token: 0x17000169 RID: 361
		// (get) Token: 0x06000661 RID: 1633 RVA: 0x00010A39 File Offset: 0x0000FA39
		public bool AllPositionsAllowed
		{
			get
			{
				return Array.Find<Operator>(OperatorExpression.s_PositionLimitedOperators, (Operator elem) => elem == this.Code) == Operator.None;
			}
		}

		// Token: 0x06000662 RID: 1634 RVA: 0x00010A54 File Offset: 0x0000FA54
		public void AddOperand(_IExpression exp)
		{
			this.m_alOperands.Add(exp);
		}

		// Token: 0x06000663 RID: 1635 RVA: 0x00010A62 File Offset: 0x0000FA62
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x06000664 RID: 1636 RVA: 0x00010A6B File Offset: 0x0000FA6B
		public override T Accept<T>(IExprementVisitor<T> visitor)
		{
			return visitor.visit(this);
		}

		// Token: 0x06000665 RID: 1637 RVA: 0x00010A74 File Offset: 0x0000FA74
		private bool VisitTrigonometrics(IOperatorExpressionVisitor visitor)
		{
			Operator code = this.Code;
			if (code - Operator.Exp <= 10 || code == Operator.Power)
			{
				visitor.visitTrigonometrics(this);
				return true;
			}
			return false;
		}

		// Token: 0x06000666 RID: 1638 RVA: 0x00010AA4 File Offset: 0x0000FAA4
		private bool VisitComparisons(IOperatorExpressionVisitor visitor)
		{
			Operator code = this.Code;
			if (code - Operator.Eq <= 5 || code - Operator.Less <= 5)
			{
				visitor.visitComparisons(this);
				return true;
			}
			return false;
		}

		// Token: 0x06000667 RID: 1639 RVA: 0x00010AD8 File Offset: 0x0000FAD8
		private bool VisitBoolOps(IOperatorExpressionVisitor visitor)
		{
			Operator code = this.Code;
			if (code - Operator.And <= 6 || code - Operator.Ampersand <= 1 || code - Operator.And_Then <= 1)
			{
				visitor.visitBoolOps(this);
				return true;
			}
			return false;
		}

		// Token: 0x06000668 RID: 1640 RVA: 0x00010B14 File Offset: 0x0000FB14
		private bool VisitArithmetics(IOperatorExpressionVisitor visitor)
		{
			Operator code = this.Code;
			if (code - Operator.Add <= 4 || code - Operator.Plus <= 2 || code == Operator.Divide)
			{
				visitor.visitArithmetics(this);
				return true;
			}
			return false;
		}

		// Token: 0x06000669 RID: 1641 RVA: 0x00010B4C File Offset: 0x0000FB4C
		private bool VisitVector(IOperatorExpressionVisitor visitor)
		{
			Operator code = this.Code;
			if (code - Operator.__vcAdd <= 12)
			{
				((IOperatorExpressionVisitor4)visitor).visitVector(this);
				return true;
			}
			return false;
		}

		// Token: 0x0600066A RID: 1642 RVA: 0x00010B7C File Offset: 0x0000FB7C
		private bool VisitBitShift(IOperatorExpressionVisitor visitor)
		{
			Operator code = this.Code;
			if (code - Operator.Rol <= 3)
			{
				visitor.visitShiftOps(this);
				return true;
			}
			return false;
		}

		// Token: 0x0600066B RID: 1643 RVA: 0x00010BA4 File Offset: 0x0000FBA4
		private bool VisitSelection(IOperatorExpressionVisitor visitor)
		{
			Operator code = this.Code;
			if (code - Operator.Limit <= 2 || code - Operator.Mux <= 1)
			{
				visitor.visitSelection(this);
				return true;
			}
			return false;
		}

		// Token: 0x0600066C RID: 1644 RVA: 0x00010BD0 File Offset: 0x0000FBD0
		private bool TryVisitOperatorGroups(IOperatorExpressionVisitor visitor)
		{
			return this.VisitArithmetics(visitor) || this.VisitComparisons(visitor) || this.VisitBoolOps(visitor) || this.VisitTrigonometrics(visitor) || this.VisitBitShift(visitor) || this.VisitSelection(visitor) || this.VisitVector(visitor);
		}

		// Token: 0x0600066D RID: 1645 RVA: 0x00010C2C File Offset: 0x0000FC2C
		public void AcceptOperatorVisitor(IOperatorExpressionVisitor visitor)
		{
			if (this.TryVisitOperatorGroups(visitor))
			{
				return;
			}
			Operator code = this.Code;
			if (code <= Operator.TestAndSet)
			{
				if (code <= Operator.Trunc)
				{
					if (code == Operator.__Reloc)
					{
						visitor.visitReloc(this);
						return;
					}
					switch (code)
					{
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
					case Operator.Trunc:
						visitor.visitTrunc(this);
						return;
					}
				}
				else
				{
					if (code == Operator.Move)
					{
						visitor.visitMove(this);
						return;
					}
					if (code == Operator.TestAndSet)
					{
						visitor.visitTestAndSet(this);
						return;
					}
				}
			}
			else if (code <= Operator.__PropertyInfo)
			{
				switch (code)
				{
				case Operator.TruncInt:
					visitor.visitTruncInt(this);
					return;
				case Operator.FupAssign:
				case Operator.__SystemScope:
				case Operator.__New:
				case Operator.__Cast:
					break;
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
				default:
					switch (code)
					{
					case Operator.__BitOffset:
						visitor.visitBitOffset(this);
						return;
					case Operator.__FCall:
						visitor.visitFCall(this);
						return;
					case Operator.__PropertyInfo:
						visitor.visitPropertyInfo(this);
						return;
					}
					break;
				}
			}
			else
			{
				switch (code)
				{
				case Operator.__MemorySet:
					visitor.visitMemorySet(this);
					return;
				case Operator.And_Then:
				case Operator.Or_Else:
				case Operator.__XInt:
				case Operator.__Try:
				case Operator.__EndTry:
				case Operator.__Catch:
				case Operator.__Finally:
				case Operator.__XString:
				case Operator.VarInst:
				case Operator.AnyString:
				case Operator.__PoolScope:
					break;
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
				default:
					if (code != Operator.XSizeOf)
					{
						if (code - Operator.__PouName <= 1)
						{
							return;
						}
					}
					else
					{
						IOperatorExpressionVisitor6 operatorExpressionVisitor = visitor as IOperatorExpressionVisitor6;
						if (operatorExpressionVisitor != null)
						{
							operatorExpressionVisitor.visitXSizeOf(this);
							return;
						}
						return;
					}
					break;
				}
			}
			Debug.Assert(false);
		}

		// Token: 0x0600066E RID: 1646 RVA: 0x00010F2D File Offset: 0x0000FF2D
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x0600066F RID: 1647 RVA: 0x00010F38 File Offset: 0x0000FF38
		public override _IExprement Duplicate()
		{
			OperatorExpression operatorExpression = new OperatorExpression();
			operatorExpression.m_op = this.m_op;
			this.DuplicateCommon(operatorExpression);
			if (this.m_alOperands != null)
			{
				foreach (_IExpression iexpression in this.m_alOperands)
				{
					operatorExpression.m_alOperands.Add(iexpression.Duplicate() as _IExpression);
				}
			}
			operatorExpression.ScratchOffset = this.ScratchOffset;
			return operatorExpression;
		}

		// Token: 0x06000670 RID: 1648 RVA: 0x00010FC4 File Offset: 0x0000FFC4
		public override bool IsConstant(IScope scope, bool bAllocatedOK)
		{
			Operator code = this.Code;
			if (code <= Operator.__LocalOffset)
			{
				if (code - Operator.IndexOf > 1 && code != Operator.__LocalOffset)
				{
					goto IL_34;
				}
			}
			else if (code - Operator.__TypeOf > 2 && code != Operator.XSizeOf)
			{
				goto IL_34;
			}
			return true;
			IL_34:
			for (int i = 0; i < this.m_alOperands.Count; i++)
			{
				if (!this.m_alOperands[i].IsConstant(scope, bAllocatedOK))
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x06000671 RID: 1649 RVA: 0x00011033 File Offset: 0x00010033
		private ILiteralValue GetLiteral(ILiteralValue[] litvalOps, bool bPrecompile)
		{
			if (VersionedCompilerFactory._ConstantFolder_OrNull != null)
			{
				return VersionedCompilerFactory._ConstantFolder_OrNull.GetLiteral(this, litvalOps, bPrecompile);
			}
			return ConstantFolding.GetLiteral(base.Type, this.Code, litvalOps, bPrecompile);
		}

		// Token: 0x06000672 RID: 1650 RVA: 0x00011060 File Offset: 0x00010060
		[SuppressMessage("Critical Code Smell", "S3776:Cognitive Complexity of methods should not be too high", Justification = "Will be fixed with CDS-86324")]
		public override ILiteralValue LiteralUnchecked(IScope scope)
		{
			IConstantFolder2 constantFolder = VersionedCompilerFactory._ConstantFolder_OrNull as IConstantFolder2;
			if (constantFolder != null)
			{
				return constantFolder.LiteralUnchecked(this, scope);
			}
			if (this.Code == Operator.SizeOf || Operator.XSizeOf == this.Code)
			{
				if (this.m_alOperands.Count != 1)
				{
					return null;
				}
				Expression expression = this.m_alOperands[0] as Expression;
				if (expression.Type == null)
				{
					return null;
				}
				int num = (expression.Type.DeRefType as _IType).Size(scope);
				if (num > 1)
				{
					IScope5 scope2 = scope as IScope5;
					if (scope2 != null && scope2.Codegenerator != null)
					{
						ICodegenerator3 codegenerator = scope2.Codegenerator as ICodegenerator3;
						if (codegenerator != null && codegenerator.GetProperty(CodegeneratorProperties.WordAddressing))
						{
							num /= 2;
						}
					}
				}
				return new LiteralValue((long)num);
			}
			else
			{
				bool greaterEqualV = APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35400;
				if (!this.IsConstant(scope, greaterEqualV) || base.Type == null || this.m_alOperands.Count == 0)
				{
					return base.LiteralUnchecked(scope);
				}
				ILiteralValue[] array = new ILiteralValue[this.m_alOperands.Count];
				for (int i = 0; i < this.m_alOperands.Count; i++)
				{
					Expression expression2 = this.m_alOperands[i] as Expression;
					array[i] = expression2.LiteralUnchecked(scope);
					if (array[i] == null)
					{
						return base.LiteralUnchecked(scope);
					}
				}
				ILiteralValue literal = this.GetLiteral(array, false);
				if (literal == null)
				{
					return base.LiteralUnchecked(scope);
				}
				return literal;
			}
		}

		// Token: 0x06000673 RID: 1651 RVA: 0x000111D4 File Offset: 0x000101D4
		[SuppressMessage("Critical Code Smell", "S3776:Cognitive Complexity of methods should not be too high", Justification = "Will be fixed with CDS-86324")]
		public override ILiteralValue LiteralWithRecursionCheck(IPrecompileScope scope, IRecursionGuard recursionGuard, bool bAllocatedOK, out bool bRecursionError)
		{
			IConstantFolder2 constantFolder = VersionedCompilerFactory._ConstantFolder_OrNull as IConstantFolder2;
			if (constantFolder != null)
			{
				return constantFolder.LiteralWithRecursionCheck(this, scope, recursionGuard, bAllocatedOK, out bRecursionError);
			}
			if (this.m_alOperands.Count == 0)
			{
				return base.LiteralWithRecursionCheck(scope, recursionGuard, bAllocatedOK, out bRecursionError);
			}
			bRecursionError = false;
			if ((this.Code == Operator.SizeOf || Operator.XSizeOf == this.Code) && scope is ICommonScope && APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35600)
			{
				if (this.m_alOperands.Count != 1)
				{
					return null;
				}
				Expression expression = this.m_alOperands[0] as Expression;
				ICompiledType compiledType = expression.Type;
				if (expression.Type == null)
				{
					ISignature[] array = ((ICommonScope)scope).FindSignature(expression);
					if (array != null && array.Length == 1)
					{
						compiledType = new UserdefType(expression);
					}
					else if (array == null || array.Length == 0)
					{
						IVariable variable;
						ISignature signature;
						IPrecompileScope precompileScope;
						scope.FindDeclaration(expression.ToString(), out variable, out signature, out precompileScope);
						if (variable != null)
						{
							compiledType = (variable.Type as ICompiledType);
						}
					}
					if (compiledType == null)
					{
						return null;
					}
				}
				ICompiledType type = TypeComparerProxy.EvaluateAliasAndEnumType(compiledType.DeRefType, scope as ICommonScope);
				ICommonScope2 commonScope = scope as ICommonScope2;
				int size;
				if (recursionGuard != null && commonScope != null)
				{
					size = commonScope.GetSize(type, recursionGuard);
				}
				else
				{
					size = ((ICommonScope)scope).GetSize(type);
				}
				return new LiteralValue((long)size);
			}
			else
			{
				ILiteralValue[] array2 = new ILiteralValue[this.m_alOperands.Count];
				for (int i = 0; i < this.m_alOperands.Count; i++)
				{
					Expression expression2 = this.m_alOperands[i] as Expression;
					array2[i] = expression2.LiteralWithRecursionCheck(scope, recursionGuard, bAllocatedOK, out bRecursionError);
					if (array2[i] == null)
					{
						return base.LiteralWithRecursionCheck(scope, recursionGuard, bAllocatedOK, out bRecursionError);
					}
				}
				ILiteralValue literal = this.GetLiteral(array2, true);
				if (literal == null)
				{
					return base.LiteralWithRecursionCheck(scope, recursionGuard, bAllocatedOK, out bRecursionError);
				}
				return literal;
			}
		}

		// Token: 0x06000674 RID: 1652 RVA: 0x000113A4 File Offset: 0x000103A4
		public override ILiteralValue LiteralWithRecursionCheck(IScope scope, IRecursionGuard recursionGuard, bool bAllocatedOK, out bool bRecursionError)
		{
			bool flag;
			return this.LiteralWithRecursionCheck(scope, recursionGuard, bAllocatedOK, out bRecursionError, out flag);
		}

		// Token: 0x06000675 RID: 1653 RVA: 0x000113C0 File Offset: 0x000103C0
		[SuppressMessage("Critical Code Smell", "S3776:Cognitive Complexity of methods should not be too high", Justification = "Will be fixed with CDS-86324")]
		private ILiteralValue LiteralWithRecursionCheck(IScope scope, IRecursionGuard recursionGuard, bool bAllocatedOK, out bool bRecursionError, out bool bOverflow)
		{
			bOverflow = false;
			IConstantFolder3 constantFolder = VersionedCompilerFactory._ConstantFolder_OrNull as IConstantFolder3;
			if (constantFolder != null)
			{
				EConstantFoldingResult econstantFoldingResult;
				ILiteralValue result = constantFolder.LiteralWithRecursionCheck(this, scope, recursionGuard, bAllocatedOK, out econstantFoldingResult);
				bRecursionError = ((EConstantFoldingResult.RecursionError & econstantFoldingResult) > EConstantFoldingResult.None);
				bOverflow = ((EConstantFoldingResult.Overflow & econstantFoldingResult) > EConstantFoldingResult.None);
				return result;
			}
			IConstantFolder2 constantFolder2 = VersionedCompilerFactory._ConstantFolder_OrNull as IConstantFolder2;
			if (constantFolder2 != null)
			{
				return constantFolder2.LiteralWithRecursionCheck(this, scope, recursionGuard, bAllocatedOK, out bRecursionError);
			}
			bool bAllocatedOK2 = bAllocatedOK;
			if (!APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35300)
			{
				bAllocatedOK2 = false;
			}
			bRecursionError = false;
			if (this.Code == Operator.SizeOf || Operator.XSizeOf == this.Code)
			{
				return OperatorExpression.SizeOf_LiteralValue(this.m_alOperands, scope, recursionGuard);
			}
			if (this.Code == Operator.__CRC)
			{
				return OperatorExpression.CRC_LiteralValue(this.m_alOperands, scope);
			}
			if (this.Code == Operator.__MaxOffset)
			{
				return OperatorExpression.MaxOffset_LiteralValue(this.m_alOperands, scope);
			}
			if (this.Code == Operator.__LocalOffset)
			{
				return OperatorExpression.LocalOffset_LiteralValue(this.m_alOperands, scope);
			}
			if (this.Code == Operator.__TypeOf)
			{
				return OperatorExpression.TypeOf_LiteralValue(this.m_alOperands);
			}
			if (!this.IsConstant(scope, bAllocatedOK) || base.Type == null || this.m_alOperands.Count == 0)
			{
				return base.LiteralWithRecursionCheck(scope, recursionGuard, bAllocatedOK2, out bRecursionError);
			}
			ILiteralValue[] array = new ILiteralValue[this.m_alOperands.Count];
			bool flag = false;
			for (int i = 0; i < this.m_alOperands.Count; i++)
			{
				Expression expression = this.m_alOperands[i] as Expression;
				array[i] = expression.LiteralWithRecursionCheck(scope, recursionGuard, bAllocatedOK2, out flag);
				if (flag)
				{
					bRecursionError = true;
					return null;
				}
				if (array[i] == null)
				{
					return base.LiteralWithRecursionCheck(scope, recursionGuard, bAllocatedOK2, out bRecursionError);
				}
			}
			ILiteralValue literal = this.GetLiteral(array, false);
			if (literal == null)
			{
				return base.LiteralWithRecursionCheck(scope, recursionGuard, bAllocatedOK2, out bRecursionError);
			}
			return literal;
		}

		// Token: 0x06000676 RID: 1654 RVA: 0x00011578 File Offset: 0x00010578
		private static ILiteralValue TypeOf_LiteralValue(LList<_IExpression> operands)
		{
			if (operands.Count != 1)
			{
				return new LiteralValue(29L);
			}
			Expression expression = operands[0] as Expression;
			if (expression.Type == null)
			{
				return new LiteralValue(29L);
			}
			return new LiteralValue((long)expression.Type.DeRefType.Class);
		}

		// Token: 0x06000677 RID: 1655 RVA: 0x000115DC File Offset: 0x000105DC
		private static ILiteralValue LocalOffset_LiteralValue(LList<_IExpression> operands, IScope scope)
		{
			if (operands.Count != 1)
			{
				return null;
			}
			IVariable variable = (operands[0] as Expression).GetVariable(scope);
			if (variable == null)
			{
				return null;
			}
			IDataLocation dataLocation = variable.DataLocation;
			if (dataLocation == null)
			{
				return null;
			}
			if (variable.GetFlag(VarFlag.RelativeStack))
			{
				bool flag = false;
				IScope5 scope2 = (IScope5)scope;
				if (scope2 != null && scope2.Codegenerator != null)
				{
					ICodegenerator3 codegenerator = scope2.Codegenerator as ICodegenerator3;
					if (codegenerator != null)
					{
						flag = codegenerator.GetProperty(CodegeneratorProperties.PositiveStackGrow);
					}
				}
				if ((!flag && dataLocation.Offset > 0) || (flag && dataLocation.Offset < 0))
				{
					return new LiteralValue(((long)dataLocation.Offset + (long)(scope as IScope5).Codegenerator.StackDisplacement) * 8L);
				}
			}
			return new LiteralValue((long)dataLocation.Offset * 8L);
		}

		// Token: 0x06000678 RID: 1656 RVA: 0x000116A8 File Offset: 0x000106A8
		private static ILiteralValue MaxOffset_LiteralValue(LList<_IExpression> operands, IScope scope)
		{
			if (operands.Count != 1)
			{
				return null;
			}
			ISignature signature = (operands[0] as Expression).GetSignature(scope);
			if (signature == null)
			{
				return null;
			}
			if (!signature.GetFlag(SignatureFlag.Located) && APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV352000)
			{
				return null;
			}
			int num = 0;
			foreach (IVariable variable in signature.All)
			{
				if (variable != null)
				{
					IDataLocation dataLocation = variable.DataLocation;
					if (dataLocation != null)
					{
						int num2 = variable.CompiledType.Size(scope);
						if (dataLocation.Offset + num2 > num)
						{
							num = dataLocation.Offset + num2;
						}
					}
				}
			}
			return new LiteralValue((ulong)((long)num));
		}

		// Token: 0x06000679 RID: 1657 RVA: 0x0001175C File Offset: 0x0001075C
		private static ILiteralValue CRC_LiteralValue(LList<_IExpression> operands, IScope scope)
		{
			if (operands.Count != 1)
			{
				return null;
			}
			ISignature signature = (operands[0] as Expression).GetSignature(scope);
			if (signature == null || !signature.HasAttribute(CompileAttributes.ATTRIBUTE_SIGNATURE_CRC))
			{
				return null;
			}
			string attributeValue = signature.GetAttributeValue(CompileAttributes.ATTRIBUTE_SIGNATURE_CRC);
			uint num = 0U;
			try
			{
				num = uint.Parse(attributeValue);
			}
			catch
			{
				return null;
			}
			return new LiteralValue((ulong)num);
		}

		// Token: 0x0600067A RID: 1658 RVA: 0x000117D4 File Offset: 0x000107D4
		private static ILiteralValue SizeOf_LiteralValue(LList<_IExpression> operands, IScope scope, IRecursionGuard recursionGuard)
		{
			if (operands.Count != 1)
			{
				return null;
			}
			Expression expression = operands[0] as Expression;
			if (expression.Type == null)
			{
				return null;
			}
			ICompiledType compiledType = TypeComparerProxy.EvaluateAliasAndEnumType(expression.Type.DeRefType, scope as ICommonScope);
			ITypeWithRecursiveTypeCheck typeWithRecursiveTypeCheck = compiledType as ITypeWithRecursiveTypeCheck;
			bool flag;
			int num;
			if (typeWithRecursiveTypeCheck != null && APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351600)
			{
				num = typeWithRecursiveTypeCheck.SizeWithRecursionCheck(scope, recursionGuard, out flag);
			}
			else
			{
				num = ((_IType)compiledType).SizeChecked(scope, out flag);
			}
			if (!flag)
			{
				return null;
			}
			if (num > 1)
			{
				IScope5 scope2 = (IScope5)scope;
				if (scope2 != null && scope2.Codegenerator != null)
				{
					ICodegenerator3 codegenerator = scope2.Codegenerator as ICodegenerator3;
					if (codegenerator != null && codegenerator.GetProperty(CodegeneratorProperties.WordAddressing))
					{
						num /= 2;
					}
				}
			}
			return new LiteralValue((long)num);
		}

		// Token: 0x0600067B RID: 1659 RVA: 0x000118A4 File Offset: 0x000108A4
		public override ILiteralValue Literal(IScope scope)
		{
			bool flag;
			return base.LiteralWithRecursionCheck(scope, new LDictionary<IVariable, IVariable>(), false, out flag);
		}

		// Token: 0x0600067C RID: 1660 RVA: 0x000118C0 File Offset: 0x000108C0
		public override ILiteralValue Literal(IScope scope, bool bAllocatedOK)
		{
			bool flag;
			return base.LiteralWithRecursionCheck(scope, new LDictionary<IVariable, IVariable>(), bAllocatedOK, out flag);
		}

		// Token: 0x0600067D RID: 1661 RVA: 0x000118DC File Offset: 0x000108DC
		public ILiteralValue Literal(IScope scope, bool bAllocatedOK, out bool bOverflow)
		{
			bool flag;
			return this.LiteralWithRecursionCheck(scope, new RecursionGuard(), bAllocatedOK, out flag, out bOverflow);
		}

		// Token: 0x0600067E RID: 1662 RVA: 0x000118FC File Offset: 0x000108FC
		public override ILiteralValue Literal(IPrecompileScope scope)
		{
			bool greaterEqualV = APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35900;
			bool flag;
			return base.LiteralWithRecursionCheck(scope, new LDictionary<IVariable, IVariable>(), greaterEqualV, out flag);
		}

		// Token: 0x1700016A RID: 362
		// (get) Token: 0x0600067F RID: 1663 RVA: 0x00011928 File Offset: 0x00010928
		// (set) Token: 0x06000680 RID: 1664 RVA: 0x00011930 File Offset: 0x00010930
		public override int ScratchOffset
		{
			get
			{
				return this.m_nScratchOffset;
			}
			set
			{
				this.m_nScratchOffset = value;
			}
		}

		// Token: 0x1700016B RID: 363
		// (get) Token: 0x06000681 RID: 1665 RVA: 0x00011939 File Offset: 0x00010939
		// (set) Token: 0x06000682 RID: 1666 RVA: 0x00011941 File Offset: 0x00010941
		public override IExprInfo Info
		{
			get
			{
				return this.m_exprinfo;
			}
			set
			{
				this.m_exprinfo = value;
			}
		}

		// Token: 0x1700016C RID: 364
		// (get) Token: 0x06000683 RID: 1667 RVA: 0x0001194A File Offset: 0x0001094A
		// (set) Token: 0x06000684 RID: 1668 RVA: 0x00011952 File Offset: 0x00010952
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

		// Token: 0x040000D8 RID: 216
		private static Operator[] s_PositionLimitedOperators = new Operator[]
		{
			Operator.__CheckLicense,
			Operator.__CheckLicenseBit
		};

		// Token: 0x040000D9 RID: 217
		private bool _bPosOK;

		// Token: 0x040000DA RID: 218
		[Obfuscation(Feature = "rename")]
		[DefaultSerialization("compiledtype")]
		[StorageVersion("3.3.0.0")]
		private ICompiledType m_ctype;

		// Token: 0x040000DB RID: 219
		[Obfuscation(Feature = "rename")]
		private IExprInfo m_exprinfo;

		// Token: 0x040000DC RID: 220
		[DefaultSerialization("Operands")]
		[StorageVersion("3.3.0.0")]
		[StorageSaveAsNonGenericCollection("3.3.0.0-3.5.3.255")]
		[Obfuscation(Feature = "rename")]
		private LList<_IExpression> m_alOperands = new LList<_IExpression>();

		// Token: 0x040000DD RID: 221
		[DefaultSerialization("ScratchOffset")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private int m_nScratchOffset = -1;

		// Token: 0x040000DE RID: 222
		[DefaultSerialization("Operator")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private Operator m_op;
	}
}
