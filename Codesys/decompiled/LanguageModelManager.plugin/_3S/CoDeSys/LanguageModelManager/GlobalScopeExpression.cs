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
	// Token: 0x02000054 RID: 84
	[TypeGuid("{d0649e38-1446-41c8-95a3-084c23b488d2}")]
	[StorageVersion("3.3.0.0")]
	public class GlobalScopeExpression : Expression, _IGlobalScopeExpression, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression, IGlobalScopeExpression, ILengthExprement
	{
		// Token: 0x060004EE RID: 1262 RVA: 0x0000C3D6 File Offset: 0x0000B3D6
		public GlobalScopeExpression()
		{
		}

		// Token: 0x060004EF RID: 1263 RVA: 0x0000EA1D File Offset: 0x0000DA1D
		public GlobalScopeExpression(_IExpression expBase, IToken token) : base(token)
		{
			this.m_expBase = expBase;
		}

		// Token: 0x060004F0 RID: 1264 RVA: 0x0000EA2D File Offset: 0x0000DA2D
		public GlobalScopeExpression(_IExpression expBase)
		{
			this.m_expBase = expBase;
		}

		// Token: 0x060004F1 RID: 1265 RVA: 0x0000EA3C File Offset: 0x0000DA3C
		public override bool IsVarInOutInput(IScope scope, bool bWriteToConstants, bool bVarInoutConstant)
		{
			return this._Base.IsVarInOutInput(scope, bWriteToConstants, bVarInoutConstant);
		}

		// Token: 0x060004F2 RID: 1266 RVA: 0x0000EA4C File Offset: 0x0000DA4C
		public override bool IsLValue(IScope scope, bool bWriteToConstants)
		{
			return this._Base.IsLValue(scope, bWriteToConstants);
		}

		// Token: 0x060004F3 RID: 1267 RVA: 0x0000EA5B File Offset: 0x0000DA5B
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x060004F4 RID: 1268 RVA: 0x0000EA64 File Offset: 0x0000DA64
		public override T Accept<T>(IExprementVisitor<T> visitor)
		{
			return visitor.visit(this);
		}

		// Token: 0x060004F5 RID: 1269 RVA: 0x0000EA6D File Offset: 0x0000DA6D
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x060004F6 RID: 1270 RVA: 0x0000EA76 File Offset: 0x0000DA76
		public override IVariable GetVariable(IScope scope)
		{
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV3202)
			{
				return this._Base.GetVariable(scope);
			}
			return base.GetVariable(scope);
		}

		// Token: 0x060004F7 RID: 1271 RVA: 0x0000EA9D File Offset: 0x0000DA9D
		public override IVariable GetVariable(IPrecompileScope scope)
		{
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV3202)
			{
				return this._Base.GetVariable(scope);
			}
			return base.GetVariable(scope);
		}

		// Token: 0x060004F8 RID: 1272 RVA: 0x0000EAC4 File Offset: 0x0000DAC4
		public override ISignature GetSignature(IScope scope)
		{
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV34100)
			{
				return this._Base.GetSignature(scope);
			}
			return base.GetSignature(scope);
		}

		// Token: 0x060004F9 RID: 1273 RVA: 0x0000EAEC File Offset: 0x0000DAEC
		public override ISourcePosition GetPosition()
		{
			SourcePosition sourcePosition = this._Base.GetPosition() as SourcePosition;
			if (sourcePosition != null)
			{
				sourcePosition.Length = this.m_sLength;
			}
			return sourcePosition;
		}

		// Token: 0x060004FA RID: 1274 RVA: 0x0000EB1A File Offset: 0x0000DB1A
		public override IDataLocation DataLocation(IScope scope)
		{
			if (!APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV34100)
			{
				return base.DataLocation(scope);
			}
			return this.Base.DataLocation(scope);
		}

		// Token: 0x17000108 RID: 264
		// (get) Token: 0x060004FB RID: 1275 RVA: 0x0000EB41 File Offset: 0x0000DB41
		// (set) Token: 0x060004FC RID: 1276 RVA: 0x00003AE9 File Offset: 0x00002AE9
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

		// Token: 0x060004FD RID: 1277 RVA: 0x0000EB4E File Offset: 0x0000DB4E
		[Obfuscation(Feature = "rename")]
		public override void SetPositionIntern(IMinimalPosition minpos)
		{
			this._Base.PositionIntern = minpos;
		}

		// Token: 0x17000109 RID: 265
		// (get) Token: 0x060004FE RID: 1278 RVA: 0x0000EB5C File Offset: 0x0000DB5C
		// (set) Token: 0x060004FF RID: 1279 RVA: 0x0000EB64 File Offset: 0x0000DB64
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

		// Token: 0x1700010A RID: 266
		// (get) Token: 0x06000500 RID: 1280 RVA: 0x0000EB6D File Offset: 0x0000DB6D
		public IExpression Base
		{
			get
			{
				return this._Base;
			}
		}

		// Token: 0x1700010B RID: 267
		// (get) Token: 0x06000501 RID: 1281 RVA: 0x0000EB75 File Offset: 0x0000DB75
		// (set) Token: 0x06000502 RID: 1282 RVA: 0x0000EB8B File Offset: 0x0000DB8B
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

		// Token: 0x06000503 RID: 1283 RVA: 0x0000EB94 File Offset: 0x0000DB94
		public override _IExprement Duplicate()
		{
			GlobalScopeExpression globalScopeExpression = new GlobalScopeExpression();
			this.DuplicateCommon(globalScopeExpression);
			if (this.m_expBase != null)
			{
				globalScopeExpression.m_expBase = (this.m_expBase.Duplicate() as _IExpression);
			}
			return globalScopeExpression;
		}

		// Token: 0x06000504 RID: 1284 RVA: 0x0000EBCD File Offset: 0x0000DBCD
		public override bool IsConstant(IScope scope, bool bAllocatedOK)
		{
			return this._Base.IsConstant(scope, bAllocatedOK);
		}

		// Token: 0x06000505 RID: 1285 RVA: 0x0000EBDC File Offset: 0x0000DBDC
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

		// Token: 0x06000506 RID: 1286 RVA: 0x0000EC0E File Offset: 0x0000DC0E
		public override ILiteralValue Literal(IScope scope)
		{
			return this._Base.Literal(scope);
		}

		// Token: 0x06000507 RID: 1287 RVA: 0x0000EC1C File Offset: 0x0000DC1C
		public override ILiteralValue Literal(IPrecompileScope scope)
		{
			return this._Base.Literal(scope);
		}

		// Token: 0x06000508 RID: 1288 RVA: 0x0000EC2C File Offset: 0x0000DC2C
		public override ILiteralValue LiteralWithRecursionCheck(IScope scope, IRecursionGuard recursionGuard, bool bAllocatedOK, out bool bRecursionError)
		{
			IConstantFolder3 constantFolder = VersionedCompilerFactory._ConstantFolder_OrNull as IConstantFolder3;
			if (constantFolder != null)
			{
				return constantFolder.GetLiteralValue(this, scope, out bRecursionError);
			}
			return ((_IExpression2)this._Base).LiteralWithRecursionCheck(scope, recursionGuard, bAllocatedOK, out bRecursionError);
		}

		// Token: 0x06000509 RID: 1289 RVA: 0x0000EC68 File Offset: 0x0000DC68
		public override ILiteralValue LiteralWithRecursionCheck(IPrecompileScope scope, IRecursionGuard recursionGuard, bool bAllocatedOK, out bool bRecursionError)
		{
			IConstantFolder3 constantFolder = VersionedCompilerFactory._ConstantFolder_OrNull as IConstantFolder3;
			if (constantFolder != null)
			{
				return constantFolder.GetLiteralValue(this, scope, recursionGuard, out bRecursionError);
			}
			return ((_IExpression2)this._Base).LiteralWithRecursionCheck(scope, recursionGuard, bAllocatedOK, out bRecursionError);
		}

		// Token: 0x1700010C RID: 268
		// (get) Token: 0x0600050A RID: 1290 RVA: 0x0000ECA4 File Offset: 0x0000DCA4
		// (set) Token: 0x0600050B RID: 1291 RVA: 0x0000ECB1 File Offset: 0x0000DCB1
		public override int PrecompileVariableId
		{
			get
			{
				return this._Base.PrecompileVariableId;
			}
			set
			{
				this._Base.PrecompileVariableId = value;
			}
		}

		// Token: 0x1700010D RID: 269
		// (get) Token: 0x0600050C RID: 1292 RVA: 0x0000ECBF File Offset: 0x0000DCBF
		// (set) Token: 0x0600050D RID: 1293 RVA: 0x0000ECCC File Offset: 0x0000DCCC
		public override int PrecompileSignatureId
		{
			get
			{
				return this._Base.PrecompileSignatureId;
			}
			set
			{
				this._Base.PrecompileSignatureId = value;
			}
		}

		// Token: 0x1700010E RID: 270
		// (get) Token: 0x0600050E RID: 1294 RVA: 0x0000ECDA File Offset: 0x0000DCDA
		// (set) Token: 0x0600050F RID: 1295 RVA: 0x00003AE9 File Offset: 0x00002AE9
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

		// Token: 0x040000B6 RID: 182
		[Obfuscation(Feature = "rename")]
		protected short m_sLength;

		// Token: 0x040000B7 RID: 183
		[DefaultSerialization("Base")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private _IExpression m_expBase;
	}
}
