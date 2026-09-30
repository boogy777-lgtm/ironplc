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
	// Token: 0x0200004A RID: 74
	[TypeGuid("{9188ea1b-7f87-41fb-8765-86f3f734f653}")]
	[StorageVersion("3.3.0.0")]
	public class CopyScopeExpression : PositionExpression, _ICopyScopeExpression, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression
	{
		// Token: 0x06000430 RID: 1072 RVA: 0x0000B408 File Offset: 0x0000A408
		public CopyScopeExpression()
		{
		}

		// Token: 0x06000431 RID: 1073 RVA: 0x0000DE9A File Offset: 0x0000CE9A
		internal CopyScopeExpression(_IExpression expBase, IToken token) : base(token)
		{
			this.m_expBase = expBase;
		}

		// Token: 0x06000432 RID: 1074 RVA: 0x0000DEAA File Offset: 0x0000CEAA
		internal CopyScopeExpression(_IExpression expBase)
		{
			this.m_expBase = expBase;
		}

		// Token: 0x06000433 RID: 1075 RVA: 0x0000DEB9 File Offset: 0x0000CEB9
		public override bool IsLValue(IScope scope, bool bWriteToConstants)
		{
			return this._Base.IsLValue(scope, bWriteToConstants);
		}

		// Token: 0x06000434 RID: 1076 RVA: 0x0000DEC8 File Offset: 0x0000CEC8
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x06000435 RID: 1077 RVA: 0x0000DED1 File Offset: 0x0000CED1
		public override T Accept<T>(IExprementVisitor<T> visitor)
		{
			return visitor.visit(this);
		}

		// Token: 0x06000436 RID: 1078 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public override void AcceptVisitor(IExprVisitor visitor)
		{
		}

		// Token: 0x06000437 RID: 1079 RVA: 0x0000DEDA File Offset: 0x0000CEDA
		public override ISourcePosition GetPosition()
		{
			return this._Base.GetPosition();
		}

		// Token: 0x170000DC RID: 220
		// (get) Token: 0x06000438 RID: 1080 RVA: 0x0000DEE7 File Offset: 0x0000CEE7
		// (set) Token: 0x06000439 RID: 1081 RVA: 0x00003AE9 File Offset: 0x00002AE9
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

		// Token: 0x0600043A RID: 1082 RVA: 0x0000DEF4 File Offset: 0x0000CEF4
		[Obfuscation(Feature = "rename")]
		public override void SetPositionIntern(IMinimalPosition minpos)
		{
			this._Base.PositionIntern = minpos;
		}

		// Token: 0x170000DD RID: 221
		// (get) Token: 0x0600043B RID: 1083 RVA: 0x0000DF02 File Offset: 0x0000CF02
		// (set) Token: 0x0600043C RID: 1084 RVA: 0x00003AE9 File Offset: 0x00002AE9
		[Obfuscation(Feature = "rename")]
		[SuppressMessage("Blocker Code Smell", "S3237:\"value\" parameters should be used", Justification = "Setter cannot be changed for compatibility reasons")]
		public override short LengthIntern
		{
			get
			{
				if (this._Base == null)
				{
					return 0;
				}
				return (this._Base as Expression).LengthIntern;
			}
			set
			{
			}
		}

		// Token: 0x170000DE RID: 222
		// (get) Token: 0x0600043D RID: 1085 RVA: 0x0000DF1E File Offset: 0x0000CF1E
		public IExpression Base
		{
			get
			{
				return this._Base;
			}
		}

		// Token: 0x170000DF RID: 223
		// (get) Token: 0x0600043E RID: 1086 RVA: 0x0000DF26 File Offset: 0x0000CF26
		// (set) Token: 0x0600043F RID: 1087 RVA: 0x0000DF3C File Offset: 0x0000CF3C
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

		// Token: 0x06000440 RID: 1088 RVA: 0x0000DF48 File Offset: 0x0000CF48
		public override _IExprement Duplicate()
		{
			CopyScopeExpression copyScopeExpression = new CopyScopeExpression();
			this.DuplicateCommon(copyScopeExpression);
			if (this.m_expBase != null)
			{
				copyScopeExpression.m_expBase = (this.m_expBase.Duplicate() as Expression);
			}
			return copyScopeExpression;
		}

		// Token: 0x06000441 RID: 1089 RVA: 0x0000DF81 File Offset: 0x0000CF81
		public override bool IsConstant(IScope scope, bool bAllocatedOK)
		{
			return this._Base.IsConstant(scope, bAllocatedOK);
		}

		// Token: 0x06000442 RID: 1090 RVA: 0x0000DF90 File Offset: 0x0000CF90
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

		// Token: 0x06000443 RID: 1091 RVA: 0x0000DFC2 File Offset: 0x0000CFC2
		public override ILiteralValue Literal(IScope scope)
		{
			return this._Base.Literal(scope);
		}

		// Token: 0x06000444 RID: 1092 RVA: 0x0000DFD0 File Offset: 0x0000CFD0
		public override ILiteralValue Literal(IPrecompileScope scope)
		{
			return this._Base.Literal(scope);
		}

		// Token: 0x06000445 RID: 1093 RVA: 0x0000DFE0 File Offset: 0x0000CFE0
		public override ILiteralValue LiteralWithRecursionCheck(IScope scope, IRecursionGuard recursionGuard, bool bAllocatedOK, out bool bRecursionError)
		{
			IConstantFolder3 constantFolder = VersionedCompilerFactory._ConstantFolder_OrNull as IConstantFolder3;
			if (constantFolder != null)
			{
				return constantFolder.GetLiteralValue(this, scope, out bRecursionError);
			}
			return ((_IExpression2)this._Base).LiteralWithRecursionCheck(scope, recursionGuard, bAllocatedOK, out bRecursionError);
		}

		// Token: 0x06000446 RID: 1094 RVA: 0x0000E01C File Offset: 0x0000D01C
		public override ILiteralValue LiteralWithRecursionCheck(IPrecompileScope scope, IRecursionGuard recursionGuard, bool bAllocatedOK, out bool bRecursionError)
		{
			IConstantFolder3 constantFolder = VersionedCompilerFactory._ConstantFolder_OrNull as IConstantFolder3;
			if (constantFolder != null)
			{
				return constantFolder.GetLiteralValue(this, scope, recursionGuard, out bRecursionError);
			}
			return ((_IExpression2)this._Base).LiteralWithRecursionCheck(scope, recursionGuard, bAllocatedOK, out bRecursionError);
		}

		// Token: 0x170000E0 RID: 224
		// (get) Token: 0x06000447 RID: 1095 RVA: 0x0000E058 File Offset: 0x0000D058
		// (set) Token: 0x06000448 RID: 1096 RVA: 0x0000E07D File Offset: 0x0000D07D
		[DefaultSerialization("Type")]
		[StorageVersion("3.3.0.10")]
		[StorageIgnorable]
		[Obfuscation(Feature = "rename")]
		public override ICompiledType _CompiledType
		{
			get
			{
				if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV352200)
				{
					return this._Base._CompiledType;
				}
				return this.m_ctype;
			}
			set
			{
				if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV352200)
				{
					return;
				}
				this.m_ctype = value;
			}
		}

		// Token: 0x040000A8 RID: 168
		[Obfuscation(Feature = "rename")]
		[DefaultSerialization("compiledtype")]
		[StorageVersion("3.3.0.0")]
		private ICompiledType m_ctype;

		// Token: 0x040000A9 RID: 169
		[DefaultSerialization("Base")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private _IExpression m_expBase;
	}
}
