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
	// Token: 0x02000073 RID: 115
	[TypeGuid("{944d7731-22cd-4544-8e6a-e23be2ec4010}")]
	[StorageVersion("3.3.0.0")]
	public class SystemScopeExpression : Expression, _ISystemScopeExpression, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression, ISystemScopeExpression, ILengthExprement
	{
		// Token: 0x06000761 RID: 1889 RVA: 0x0000C3D6 File Offset: 0x0000B3D6
		public SystemScopeExpression()
		{
		}

		// Token: 0x06000762 RID: 1890 RVA: 0x00012C58 File Offset: 0x00011C58
		internal SystemScopeExpression(_IExpression expBase, IToken token) : base(token)
		{
			this.m_expBase = expBase;
		}

		// Token: 0x06000763 RID: 1891 RVA: 0x00012C68 File Offset: 0x00011C68
		internal SystemScopeExpression(_IExpression expBase)
		{
			this.m_expBase = expBase;
		}

		// Token: 0x06000764 RID: 1892 RVA: 0x00012C77 File Offset: 0x00011C77
		internal SystemScopeExpression(string stBaseName) : this(new VariableExpression(stBaseName))
		{
		}

		// Token: 0x06000765 RID: 1893 RVA: 0x00012C85 File Offset: 0x00011C85
		public override bool IsVarInOutInput(IScope scope, bool bWriteToConstants, bool bVarInoutConstant)
		{
			return this._Base.IsVarInOutInput(scope, bWriteToConstants, bVarInoutConstant);
		}

		// Token: 0x06000766 RID: 1894 RVA: 0x00012C95 File Offset: 0x00011C95
		public override bool IsLValue(IScope scope, bool bWriteToConstants)
		{
			return this._Base.IsLValue(scope, bWriteToConstants);
		}

		// Token: 0x06000767 RID: 1895 RVA: 0x00012CA4 File Offset: 0x00011CA4
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x06000768 RID: 1896 RVA: 0x00012CAD File Offset: 0x00011CAD
		public override T Accept<T>(IExprementVisitor<T> visitor)
		{
			return visitor.visit(this);
		}

		// Token: 0x06000769 RID: 1897 RVA: 0x00012CB6 File Offset: 0x00011CB6
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			this.m_expBase.AcceptVisitor(visitor);
		}

		// Token: 0x0600076A RID: 1898 RVA: 0x00012CC4 File Offset: 0x00011CC4
		public override ISourcePosition GetPosition()
		{
			SourcePosition sourcePosition = this._Base.GetPosition() as SourcePosition;
			if (sourcePosition != null)
			{
				sourcePosition.Length = this.m_sLength;
			}
			return sourcePosition;
		}

		// Token: 0x0600076B RID: 1899 RVA: 0x00012CF2 File Offset: 0x00011CF2
		public override ISignature GetSignature(IScope scope)
		{
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV33010)
			{
				return this.m_expBase.GetSignature(scope);
			}
			return base.GetSignature(scope);
		}

		// Token: 0x170001A2 RID: 418
		// (get) Token: 0x0600076C RID: 1900 RVA: 0x00012D19 File Offset: 0x00011D19
		// (set) Token: 0x0600076D RID: 1901 RVA: 0x00003AE9 File Offset: 0x00002AE9
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

		// Token: 0x0600076E RID: 1902 RVA: 0x00012D26 File Offset: 0x00011D26
		[Obfuscation(Feature = "rename")]
		public override void SetPositionIntern(IMinimalPosition minpos)
		{
			this._Base.PositionIntern = minpos;
		}

		// Token: 0x170001A3 RID: 419
		// (get) Token: 0x0600076F RID: 1903 RVA: 0x00012D34 File Offset: 0x00011D34
		// (set) Token: 0x06000770 RID: 1904 RVA: 0x00012D3C File Offset: 0x00011D3C
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

		// Token: 0x170001A4 RID: 420
		// (get) Token: 0x06000771 RID: 1905 RVA: 0x00012D45 File Offset: 0x00011D45
		public IExpression Base
		{
			get
			{
				return this._Base;
			}
		}

		// Token: 0x170001A5 RID: 421
		// (get) Token: 0x06000772 RID: 1906 RVA: 0x00012D4D File Offset: 0x00011D4D
		// (set) Token: 0x06000773 RID: 1907 RVA: 0x00012D63 File Offset: 0x00011D63
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

		// Token: 0x06000774 RID: 1908 RVA: 0x00012D6C File Offset: 0x00011D6C
		public override _IExprement Duplicate()
		{
			SystemScopeExpression systemScopeExpression = new SystemScopeExpression();
			this.DuplicateCommon(systemScopeExpression);
			if (this.m_expBase != null)
			{
				systemScopeExpression.m_expBase = (this.m_expBase.Duplicate() as Expression);
			}
			return systemScopeExpression;
		}

		// Token: 0x06000775 RID: 1909 RVA: 0x00012DA5 File Offset: 0x00011DA5
		public override bool IsConstant(IScope scope, bool bAllocatedOK)
		{
			return this._Base.IsConstant(scope, bAllocatedOK);
		}

		// Token: 0x06000776 RID: 1910 RVA: 0x00012DB4 File Offset: 0x00011DB4
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

		// Token: 0x06000777 RID: 1911 RVA: 0x00012DE6 File Offset: 0x00011DE6
		public override ILiteralValue Literal(IScope scope)
		{
			return this._Base.Literal(scope);
		}

		// Token: 0x06000778 RID: 1912 RVA: 0x00012DF4 File Offset: 0x00011DF4
		public override ILiteralValue Literal(IPrecompileScope scope)
		{
			return this._Base.Literal(scope);
		}

		// Token: 0x06000779 RID: 1913 RVA: 0x00012E04 File Offset: 0x00011E04
		public override ILiteralValue LiteralWithRecursionCheck(IScope scope, IRecursionGuard recursionGuard, bool bAllocatedOK, out bool bRecursionError)
		{
			IConstantFolder3 constantFolder = VersionedCompilerFactory._ConstantFolder_OrNull as IConstantFolder3;
			if (constantFolder != null)
			{
				return constantFolder.GetLiteralValue(this, scope, out bRecursionError);
			}
			return ((_IExpression2)this._Base).LiteralWithRecursionCheck(scope, recursionGuard, bAllocatedOK, out bRecursionError);
		}

		// Token: 0x0600077A RID: 1914 RVA: 0x00012E40 File Offset: 0x00011E40
		public override ILiteralValue LiteralWithRecursionCheck(IPrecompileScope scope, IRecursionGuard recursionGuard, bool bAllocatedOK, out bool bRecursionError)
		{
			IConstantFolder3 constantFolder = VersionedCompilerFactory._ConstantFolder_OrNull as IConstantFolder3;
			if (constantFolder != null)
			{
				return constantFolder.GetLiteralValue(this, scope, recursionGuard, out bRecursionError);
			}
			return ((_IExpression2)this._Base).LiteralWithRecursionCheck(scope, recursionGuard, bAllocatedOK, out bRecursionError);
		}

		// Token: 0x170001A6 RID: 422
		// (get) Token: 0x0600077B RID: 1915 RVA: 0x00012E7C File Offset: 0x00011E7C
		// (set) Token: 0x0600077C RID: 1916 RVA: 0x00003AE9 File Offset: 0x00002AE9
		[DefaultSerialization("Type")]
		[StorageVersion("3.3.0.10")]
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

		// Token: 0x040000FC RID: 252
		[Obfuscation(Feature = "rename")]
		protected short m_sLength;

		// Token: 0x040000FD RID: 253
		[DefaultSerialization("Base")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private _IExpression m_expBase;
	}
}
