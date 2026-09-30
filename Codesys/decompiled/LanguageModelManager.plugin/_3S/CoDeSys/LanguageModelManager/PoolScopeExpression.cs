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
	// Token: 0x02000068 RID: 104
	[TypeGuid("{9FBAB230-1683-4F1F-AF5B-8026DB1DB07C}")]
	[StorageVersion("3.5.9.0")]
	public class PoolScopeExpression : Expression, _IPoolScopeExpression, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression, IPoolScopeExpression, ILengthExprement
	{
		// Token: 0x060006B2 RID: 1714 RVA: 0x0000C3D6 File Offset: 0x0000B3D6
		public PoolScopeExpression()
		{
		}

		// Token: 0x060006B3 RID: 1715 RVA: 0x00011ED1 File Offset: 0x00010ED1
		internal PoolScopeExpression(_IExpression expBase, IToken token) : base(token)
		{
			this.m_expBase = expBase;
		}

		// Token: 0x060006B4 RID: 1716 RVA: 0x00011EE1 File Offset: 0x00010EE1
		internal PoolScopeExpression(_IExpression expBase)
		{
			this.m_expBase = expBase;
		}

		// Token: 0x060006B5 RID: 1717 RVA: 0x00011EF0 File Offset: 0x00010EF0
		internal PoolScopeExpression(string stBaseName) : this(new VariableExpression(stBaseName))
		{
		}

		// Token: 0x060006B6 RID: 1718 RVA: 0x00011EFE File Offset: 0x00010EFE
		public override bool IsVarInOutInput(IScope scope, bool bWriteToConstants, bool bVarInoutConstant)
		{
			return this._Base.IsVarInOutInput(scope, bWriteToConstants, bVarInoutConstant);
		}

		// Token: 0x060006B7 RID: 1719 RVA: 0x00011F0E File Offset: 0x00010F0E
		public override bool IsLValue(IScope scope, bool bWriteToConstants)
		{
			return this._Base.IsLValue(scope, bWriteToConstants);
		}

		// Token: 0x060006B8 RID: 1720 RVA: 0x00011F1D File Offset: 0x00010F1D
		public override bool IsLValue(IScope scope, bool bWriteToConstants, bool bPassToVarInout, bool bRefAssign)
		{
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351600)
			{
				return this._Base.IsLValue(scope, bWriteToConstants, bPassToVarInout, bRefAssign);
			}
			return base.IsLValue(scope, bWriteToConstants, bPassToVarInout, bRefAssign);
		}

		// Token: 0x17000177 RID: 375
		// (get) Token: 0x060006B9 RID: 1721 RVA: 0x00011F4C File Offset: 0x00010F4C
		public override bool IsPOUReference
		{
			get
			{
				if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351600)
				{
					return this._Base.IsPOUReference;
				}
				return base.IsPOUReference;
			}
		}

		// Token: 0x060006BA RID: 1722 RVA: 0x00011F71 File Offset: 0x00010F71
		public override void Accept(IExprementVisitor visitor)
		{
			if (visitor is IExprementVisitor3590)
			{
				(visitor as IExprementVisitor3590).visit(this);
			}
		}

		// Token: 0x060006BB RID: 1723 RVA: 0x00011F87 File Offset: 0x00010F87
		public override T Accept<T>(IExprementVisitor<T> visitor)
		{
			return visitor.visit(this);
		}

		// Token: 0x060006BC RID: 1724 RVA: 0x00011F90 File Offset: 0x00010F90
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			this.m_expBase.AcceptVisitor(visitor);
		}

		// Token: 0x060006BD RID: 1725 RVA: 0x00011FA0 File Offset: 0x00010FA0
		public override ISourcePosition GetPosition()
		{
			SourcePosition sourcePosition = this._Base.GetPosition() as SourcePosition;
			if (sourcePosition != null)
			{
				sourcePosition.Length = this.m_sLength;
			}
			return sourcePosition;
		}

		// Token: 0x060006BE RID: 1726 RVA: 0x00011FCE File Offset: 0x00010FCE
		public override ISignature GetSignature(IScope scope)
		{
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV33010)
			{
				return this.m_expBase.GetSignature(scope);
			}
			return base.GetSignature(scope);
		}

		// Token: 0x060006BF RID: 1727 RVA: 0x00011FF5 File Offset: 0x00010FF5
		public override ISignature GetSignatureEx(IScope scope)
		{
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351600)
			{
				return this._Base.GetSignatureEx(scope);
			}
			return base.GetSignatureEx(scope);
		}

		// Token: 0x060006C0 RID: 1728 RVA: 0x0001201C File Offset: 0x0001101C
		public override IVariable GetVariable(IScope scope)
		{
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351600)
			{
				return this._Base.GetVariable(scope);
			}
			return base.GetVariable(scope);
		}

		// Token: 0x060006C1 RID: 1729 RVA: 0x00012043 File Offset: 0x00011043
		public override IVariable GetVariable(IPrecompileScope scope)
		{
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351600)
			{
				return this._Base.GetVariable(scope);
			}
			return base.GetVariable(scope);
		}

		// Token: 0x17000178 RID: 376
		// (get) Token: 0x060006C2 RID: 1730 RVA: 0x0001206A File Offset: 0x0001106A
		// (set) Token: 0x060006C3 RID: 1731 RVA: 0x0001208F File Offset: 0x0001108F
		public override int PrecompileVariableId
		{
			get
			{
				if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351600)
				{
					return this._Base.PrecompileVariableId;
				}
				return base.PrecompileVariableId;
			}
			set
			{
				if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351600)
				{
					this._Base.PrecompileVariableId = value;
				}
			}
		}

		// Token: 0x17000179 RID: 377
		// (get) Token: 0x060006C4 RID: 1732 RVA: 0x000120AE File Offset: 0x000110AE
		// (set) Token: 0x060006C5 RID: 1733 RVA: 0x000120D3 File Offset: 0x000110D3
		public override int PrecompileSignatureId
		{
			get
			{
				if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351600)
				{
					return this._Base.PrecompileSignatureId;
				}
				return base.PrecompileSignatureId;
			}
			set
			{
				if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351600)
				{
					this._Base.PrecompileSignatureId = value;
				}
			}
		}

		// Token: 0x1700017A RID: 378
		// (get) Token: 0x060006C6 RID: 1734 RVA: 0x000120F2 File Offset: 0x000110F2
		// (set) Token: 0x060006C7 RID: 1735 RVA: 0x00003AE9 File Offset: 0x00002AE9
		[Obfuscation(Feature = "rename")]
		[SuppressMessage("Blocker Code Smell", "S3237:\"value\" parameters should be used", Justification = "Setter cannot be changed for compatibility reasons")]
		public override IMinimalPosition PositionIntern
		{
			get
			{
				return this._Base.PositionIntern;
			}
			set
			{
			}
		}

		// Token: 0x060006C8 RID: 1736 RVA: 0x000120FF File Offset: 0x000110FF
		[Obfuscation(Feature = "rename")]
		public override void SetPositionIntern(IMinimalPosition minpos)
		{
			this._Base.PositionIntern = minpos;
		}

		// Token: 0x1700017B RID: 379
		// (get) Token: 0x060006C9 RID: 1737 RVA: 0x0001210D File Offset: 0x0001110D
		// (set) Token: 0x060006CA RID: 1738 RVA: 0x00012115 File Offset: 0x00011115
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

		// Token: 0x1700017C RID: 380
		// (get) Token: 0x060006CB RID: 1739 RVA: 0x0001211E File Offset: 0x0001111E
		public IExpression Base
		{
			get
			{
				return this._Base;
			}
		}

		// Token: 0x1700017D RID: 381
		// (get) Token: 0x060006CC RID: 1740 RVA: 0x00012126 File Offset: 0x00011126
		// (set) Token: 0x060006CD RID: 1741 RVA: 0x0001213C File Offset: 0x0001113C
		public _IExpression _Base
		{
			get
			{
				if (this.m_expBase == null)
				{
					return new NullExpression();
				}
				return this.m_expBase;
			}
			set
			{
				this.m_expBase = value;
			}
		}

		// Token: 0x060006CE RID: 1742 RVA: 0x00012148 File Offset: 0x00011148
		public override _IExprement Duplicate()
		{
			PoolScopeExpression poolScopeExpression = new PoolScopeExpression();
			this.DuplicateCommon(poolScopeExpression);
			if (this.m_expBase != null)
			{
				poolScopeExpression.m_expBase = (this.m_expBase.Duplicate() as Expression);
			}
			return poolScopeExpression;
		}

		// Token: 0x060006CF RID: 1743 RVA: 0x00012181 File Offset: 0x00011181
		public override bool IsConstant(IScope scope, bool bAllocatedOK)
		{
			return this._Base.IsConstant(scope, bAllocatedOK);
		}

		// Token: 0x060006D0 RID: 1744 RVA: 0x00012190 File Offset: 0x00011190
		public override ILiteralValue LiteralUnchecked(IScope scope)
		{
			IConstantFolder3 constantFolder = VersionedCompilerFactory._ConstantFolder_OrNull as IConstantFolder3;
			if (constantFolder != null)
			{
				bool flag;
				return constantFolder.GetLiteralValue(this, scope, out flag);
			}
			return this._Base.LiteralUnchecked(scope);
		}

		// Token: 0x060006D1 RID: 1745 RVA: 0x000121C2 File Offset: 0x000111C2
		public override ILiteralValue Literal(IScope scope)
		{
			return this._Base.Literal(scope);
		}

		// Token: 0x060006D2 RID: 1746 RVA: 0x000121D0 File Offset: 0x000111D0
		public override ILiteralValue Literal(IScope scope, bool bAllocatedOK)
		{
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351600)
			{
				bool flag;
				return base.LiteralWithRecursionCheck(scope, new LDictionary<IVariable, IVariable>(), bAllocatedOK, out flag);
			}
			return base.Literal(scope, bAllocatedOK);
		}

		// Token: 0x060006D3 RID: 1747 RVA: 0x00012206 File Offset: 0x00011206
		public override ILiteralValue Literal(IPrecompileScope scope)
		{
			return this._Base.Literal(scope);
		}

		// Token: 0x060006D4 RID: 1748 RVA: 0x00012214 File Offset: 0x00011214
		public override ILiteralValue LiteralWithRecursionCheck(IScope scope, IRecursionGuard recursionGuard, bool bAllocatedOK, out bool bRecursionError)
		{
			IConstantFolder3 constantFolder = VersionedCompilerFactory._ConstantFolder_OrNull as IConstantFolder3;
			if (constantFolder != null)
			{
				return constantFolder.GetLiteralValue(this, scope, out bRecursionError);
			}
			return ((_IExpression2)this._Base).LiteralWithRecursionCheck(scope, recursionGuard, bAllocatedOK, out bRecursionError);
		}

		// Token: 0x060006D5 RID: 1749 RVA: 0x00012250 File Offset: 0x00011250
		public override ILiteralValue LiteralWithRecursionCheck(IPrecompileScope scope, IRecursionGuard recursionGuard, bool bAllocatedOK, out bool bRecursionError)
		{
			IConstantFolder3 constantFolder = VersionedCompilerFactory._ConstantFolder_OrNull as IConstantFolder3;
			if (constantFolder != null)
			{
				return constantFolder.GetLiteralValue(this, scope, recursionGuard, out bRecursionError);
			}
			return ((_IExpression2)this._Base).LiteralWithRecursionCheck(scope, recursionGuard, bAllocatedOK, out bRecursionError);
		}

		// Token: 0x1700017E RID: 382
		// (get) Token: 0x060006D6 RID: 1750 RVA: 0x0001228C File Offset: 0x0001128C
		// (set) Token: 0x060006D7 RID: 1751 RVA: 0x00003AE9 File Offset: 0x00002AE9
		[DefaultSerialization("Type")]
		[StorageVersion("3.5.9.0")]
		[StorageIgnorable]
		[Obfuscation(Feature = "rename")]
		[SuppressMessage("Blocker Code Smell", "S3237:\"value\" parameters should be used", Justification = "Setter cannot be changed for compatibility reasons")]
		public override ICompiledType _CompiledType
		{
			get
			{
				return this.m_expBase._CompiledType;
			}
			set
			{
			}
		}

		// Token: 0x040000E4 RID: 228
		[Obfuscation(Feature = "rename")]
		protected short m_sLength;

		// Token: 0x040000E5 RID: 229
		[DefaultSerialization("Base")]
		[StorageVersion("3.5.9.0")]
		[Obfuscation(Feature = "rename")]
		private _IExpression m_expBase;
	}
}
