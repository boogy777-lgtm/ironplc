using System;
using System.Diagnostics.CodeAnalysis;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000063 RID: 99
	[TypeGuid("{8928BBD2-10C4-4FBB-8C81-B8942E34DCCA}")]
	[StorageVersion("3.5.15.0")]
	internal class NamespaceAccessExpression : Expression, _INamespaceAccessExpression, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression, INamespaceAccessExpression
	{
		// Token: 0x0600060E RID: 1550 RVA: 0x0000C3D6 File Offset: 0x0000B3D6
		public NamespaceAccessExpression()
		{
		}

		// Token: 0x0600060F RID: 1551 RVA: 0x0001049A File Offset: 0x0000F49A
		internal NamespaceAccessExpression(_IExpression expNamespace, _IExpression expAccess)
		{
			this._expNamespace = expNamespace;
			this._expAccess = expAccess;
		}

		// Token: 0x06000610 RID: 1552 RVA: 0x000104B0 File Offset: 0x0000F4B0
		internal NamespaceAccessExpression(_IExpression expNamespace, _IExpression expAccess, IToken token) : base(token)
		{
			this._expNamespace = expNamespace;
			this._expAccess = expAccess;
		}

		// Token: 0x17000151 RID: 337
		// (get) Token: 0x06000611 RID: 1553 RVA: 0x000104C7 File Offset: 0x0000F4C7
		public IExpression Access
		{
			get
			{
				return this._expAccess;
			}
		}

		// Token: 0x17000152 RID: 338
		// (get) Token: 0x06000612 RID: 1554 RVA: 0x000104CF File Offset: 0x0000F4CF
		public IExpression Namespace
		{
			get
			{
				return this._expNamespace;
			}
		}

		// Token: 0x17000153 RID: 339
		// (get) Token: 0x06000613 RID: 1555 RVA: 0x000104C7 File Offset: 0x0000F4C7
		// (set) Token: 0x06000614 RID: 1556 RVA: 0x000104D7 File Offset: 0x0000F4D7
		public _IExpression _Access
		{
			get
			{
				return this._expAccess;
			}
			set
			{
				this._expAccess = value;
			}
		}

		// Token: 0x17000154 RID: 340
		// (get) Token: 0x06000615 RID: 1557 RVA: 0x000104CF File Offset: 0x0000F4CF
		// (set) Token: 0x06000616 RID: 1558 RVA: 0x000104E0 File Offset: 0x0000F4E0
		public _IExpression _Namespace
		{
			get
			{
				return this._expNamespace;
			}
			set
			{
				this._expNamespace = value;
			}
		}

		// Token: 0x17000155 RID: 341
		// (get) Token: 0x06000617 RID: 1559 RVA: 0x000104E9 File Offset: 0x0000F4E9
		// (set) Token: 0x06000618 RID: 1560 RVA: 0x000104F6 File Offset: 0x0000F4F6
		public override ICompiledType _CompiledType
		{
			get
			{
				return this._expAccess._CompiledType;
			}
			set
			{
				this._expAccess._CompiledType = value;
			}
		}

		// Token: 0x17000156 RID: 342
		// (get) Token: 0x06000619 RID: 1561 RVA: 0x00005F0F File Offset: 0x00004F0F
		// (set) Token: 0x0600061A RID: 1562 RVA: 0x00003AE9 File Offset: 0x00002AE9
		[SuppressMessage("Blocker Code Smell", "S3237:\"value\" parameters should be used", Justification = "Setter cannot be changed for compatibility reasons")]
		public override IMinimalPosition PositionIntern
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

		// Token: 0x17000157 RID: 343
		// (get) Token: 0x0600061B RID: 1563 RVA: 0x00004E6B File Offset: 0x00003E6B
		// (set) Token: 0x0600061C RID: 1564 RVA: 0x00003AE9 File Offset: 0x00002AE9
		[SuppressMessage("Blocker Code Smell", "S3237:\"value\" parameters should be used", Justification = "Setter cannot be changed for compatibility reasons")]
		public override short LengthIntern
		{
			get
			{
				return 0;
			}
			set
			{
			}
		}

		// Token: 0x0600061D RID: 1565 RVA: 0x00010504 File Offset: 0x0000F504
		public override bool IsLValue(IScope scope, bool bWriteToConstants)
		{
			return this._expAccess.IsLValue(scope, bWriteToConstants);
		}

		// Token: 0x0600061E RID: 1566 RVA: 0x00010513 File Offset: 0x0000F513
		public override bool IsLValue(IScope scope, bool bWriteToConstants, bool bPassToVarInout, bool bRefAssign)
		{
			return this._expAccess.IsLValue(scope, bWriteToConstants, bPassToVarInout, bRefAssign);
		}

		// Token: 0x17000158 RID: 344
		// (get) Token: 0x0600061F RID: 1567 RVA: 0x00010525 File Offset: 0x0000F525
		public override bool IsPOUReference
		{
			get
			{
				return this._expAccess.IsPOUReference;
			}
		}

		// Token: 0x06000620 RID: 1568 RVA: 0x00010532 File Offset: 0x0000F532
		public override ISignature GetSignature(IScope scope)
		{
			return this._expAccess.GetSignature(scope);
		}

		// Token: 0x06000621 RID: 1569 RVA: 0x00010540 File Offset: 0x0000F540
		public override ISignature GetSignatureEx(IScope scope)
		{
			return this._expAccess.GetSignatureEx(scope);
		}

		// Token: 0x06000622 RID: 1570 RVA: 0x0001054E File Offset: 0x0000F54E
		public override IVariable GetVariable(IScope scope)
		{
			return this._expAccess.GetVariable(scope);
		}

		// Token: 0x06000623 RID: 1571 RVA: 0x0001055C File Offset: 0x0000F55C
		public override IVariable GetVariable(IPrecompileScope scope)
		{
			return this._expAccess.GetVariable(scope);
		}

		// Token: 0x17000159 RID: 345
		// (get) Token: 0x06000624 RID: 1572 RVA: 0x0001056A File Offset: 0x0000F56A
		// (set) Token: 0x06000625 RID: 1573 RVA: 0x00010577 File Offset: 0x0000F577
		public override int PrecompileVariableId
		{
			get
			{
				return this._expAccess.PrecompileVariableId;
			}
			set
			{
				this._expAccess.PrecompileVariableId = value;
			}
		}

		// Token: 0x1700015A RID: 346
		// (get) Token: 0x06000626 RID: 1574 RVA: 0x00010585 File Offset: 0x0000F585
		// (set) Token: 0x06000627 RID: 1575 RVA: 0x00010592 File Offset: 0x0000F592
		public override int PrecompileSignatureId
		{
			get
			{
				return this._expAccess.PrecompileSignatureId;
			}
			set
			{
				this._expAccess.PrecompileSignatureId = value;
			}
		}

		// Token: 0x06000628 RID: 1576 RVA: 0x000105A0 File Offset: 0x0000F5A0
		public override void Accept(IExprementVisitor visitor)
		{
			IExprementVisitor351500 exprementVisitor = visitor as IExprementVisitor351500;
			if (exprementVisitor == null)
			{
				return;
			}
			exprementVisitor.visit(this);
		}

		// Token: 0x06000629 RID: 1577 RVA: 0x000105B3 File Offset: 0x0000F5B3
		public override T Accept<T>(IExprementVisitor<T> visitor)
		{
			return visitor.visit(this);
		}

		// Token: 0x0600062A RID: 1578 RVA: 0x000105BC File Offset: 0x0000F5BC
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			IExprVisitor5 exprVisitor = visitor as IExprVisitor5;
			if (exprVisitor == null)
			{
				return;
			}
			exprVisitor.visit(this);
		}

		// Token: 0x0600062B RID: 1579 RVA: 0x000105CF File Offset: 0x0000F5CF
		public override bool IsConstant(IScope scope, bool bAllocatedOK)
		{
			return this._expAccess.IsConstant(scope, bAllocatedOK);
		}

		// Token: 0x0600062C RID: 1580 RVA: 0x000105E0 File Offset: 0x0000F5E0
		public override ILiteralValue LiteralUnchecked(IScope scope)
		{
			IConstantFolder3 constantFolder = VersionedCompilerFactory._ConstantFolder_OrNull as IConstantFolder3;
			if (constantFolder != null)
			{
				bool flag;
				return constantFolder.GetLiteralValue(this, scope, out flag);
			}
			return this._expAccess.LiteralUnchecked(scope);
		}

		// Token: 0x0600062D RID: 1581 RVA: 0x00010614 File Offset: 0x0000F614
		public override ILiteralValue Literal(IScope scope, bool bAllocatedOK)
		{
			bool flag;
			return base.LiteralWithRecursionCheck(scope, new LDictionary<IVariable, IVariable>(), bAllocatedOK, out flag);
		}

		// Token: 0x0600062E RID: 1582 RVA: 0x00010630 File Offset: 0x0000F630
		public override ILiteralValue Literal(IScope scope)
		{
			bool flag;
			return base.LiteralWithRecursionCheck(scope, new LDictionary<IVariable, IVariable>(), false, out flag);
		}

		// Token: 0x0600062F RID: 1583 RVA: 0x0001064C File Offset: 0x0000F64C
		public override ILiteralValue Literal(IPrecompileScope scope)
		{
			bool flag;
			return base.LiteralWithRecursionCheck(scope, new LDictionary<IVariable, IVariable>(), true, out flag);
		}

		// Token: 0x06000630 RID: 1584 RVA: 0x00010668 File Offset: 0x0000F668
		public override ILiteralValue LiteralWithRecursionCheck(IScope scope, IRecursionGuard recursionGuard, bool bAllocatedOK, out bool bRecursionError)
		{
			IConstantFolder3 constantFolder = VersionedCompilerFactory._ConstantFolder_OrNull as IConstantFolder3;
			if (constantFolder != null)
			{
				return constantFolder.GetLiteralValue(this, scope, recursionGuard, out bRecursionError);
			}
			return ((_IExpression2)this._expAccess).LiteralWithRecursionCheck(scope, recursionGuard, bAllocatedOK, out bRecursionError);
		}

		// Token: 0x06000631 RID: 1585 RVA: 0x000106A4 File Offset: 0x0000F6A4
		public override ILiteralValue LiteralWithRecursionCheck(IPrecompileScope scope, IRecursionGuard recursionGuard, bool bAllocatedOK, out bool bRecursionError)
		{
			IConstantFolder3 constantFolder = VersionedCompilerFactory._ConstantFolder_OrNull as IConstantFolder3;
			if (constantFolder != null)
			{
				return constantFolder.GetLiteralValue(this, scope, recursionGuard, out bRecursionError);
			}
			bRecursionError = false;
			IVariable variable;
			ISignature signature;
			IPrecompileScope precompileScope;
			scope.FindDeclaration(this._expNamespace.ToString(), out variable, out signature, out precompileScope);
			if (variable == null && signature == null && precompileScope != null)
			{
				IPrecompileScope scope2 = precompileScope;
				return ((_IExpression2)this._expAccess).LiteralWithRecursionCheck(scope2, recursionGuard, bAllocatedOK, out bRecursionError);
			}
			return null;
		}

		// Token: 0x06000632 RID: 1586 RVA: 0x0001070C File Offset: 0x0000F70C
		public override IDataLocation DataLocation(IScope scope)
		{
			IDataLocation dataLocation = this._expAccess.DataLocation(scope);
			if (dataLocation == null)
			{
				return base.DataLocation(scope);
			}
			return dataLocation;
		}

		// Token: 0x06000633 RID: 1587 RVA: 0x00010734 File Offset: 0x0000F734
		public override _IExprement Duplicate()
		{
			NamespaceAccessExpression namespaceAccessExpression = new NamespaceAccessExpression(this._Namespace.Duplicate() as _IExpression, this._Access.Duplicate() as _IExpression);
			this.DuplicateCommon(namespaceAccessExpression);
			return namespaceAccessExpression;
		}

		// Token: 0x040000D2 RID: 210
		[DefaultSerialization("namespace")]
		[StorageVersion("3.5.15.0")]
		private _IExpression _expNamespace;

		// Token: 0x040000D3 RID: 211
		[DefaultSerialization("access")]
		[StorageVersion("3.5.15.0")]
		private _IExpression _expAccess;
	}
}
