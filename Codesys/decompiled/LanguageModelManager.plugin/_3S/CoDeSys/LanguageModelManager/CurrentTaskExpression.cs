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
	// Token: 0x0200004B RID: 75
	[TypeGuid("{163E8F1B-44D1-4951-B752-4261C40B8BEF}")]
	[StorageVersion("3.5.13.0")]
	public class CurrentTaskExpression : Expression, _ICurrentTaskExpression, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression, ICurrentTaskExpression, ILengthExprement
	{
		// Token: 0x06000449 RID: 1097 RVA: 0x0000C3D6 File Offset: 0x0000B3D6
		public CurrentTaskExpression()
		{
		}

		// Token: 0x0600044A RID: 1098 RVA: 0x0000E098 File Offset: 0x0000D098
		internal CurrentTaskExpression(_IExpression expBase, IToken token) : base(token)
		{
			this.m_expBase = expBase;
		}

		// Token: 0x0600044B RID: 1099 RVA: 0x0000E0A8 File Offset: 0x0000D0A8
		internal CurrentTaskExpression(_IExpression expBase)
		{
			this.m_expBase = expBase;
		}

		// Token: 0x0600044C RID: 1100 RVA: 0x0000E0B7 File Offset: 0x0000D0B7
		internal CurrentTaskExpression(string stBaseName) : this(new VariableExpression(stBaseName))
		{
		}

		// Token: 0x0600044D RID: 1101 RVA: 0x0000E0C5 File Offset: 0x0000D0C5
		public override bool IsVarInOutInput(IScope scope, bool bWriteToConstants, bool bVarInoutConstant)
		{
			return this._Base.IsVarInOutInput(scope, bWriteToConstants, bVarInoutConstant);
		}

		// Token: 0x0600044E RID: 1102 RVA: 0x0000E0D5 File Offset: 0x0000D0D5
		public override bool IsLValue(IScope scope, bool bWriteToConstants)
		{
			return this._Base.IsLValue(scope, bWriteToConstants);
		}

		// Token: 0x0600044F RID: 1103 RVA: 0x0000E0E4 File Offset: 0x0000D0E4
		public override void Accept(IExprementVisitor visitor)
		{
			if (visitor is IExprementVisitor351300)
			{
				(visitor as IExprementVisitor351300).visit(this);
			}
		}

		// Token: 0x06000450 RID: 1104 RVA: 0x0000E0FA File Offset: 0x0000D0FA
		public override T Accept<T>(IExprementVisitor<T> visitor)
		{
			return visitor.visit(this);
		}

		// Token: 0x06000451 RID: 1105 RVA: 0x0000E103 File Offset: 0x0000D103
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			this.m_expBase.AcceptVisitor(visitor);
		}

		// Token: 0x06000452 RID: 1106 RVA: 0x0000E114 File Offset: 0x0000D114
		public override ISourcePosition GetPosition()
		{
			SourcePosition sourcePosition = this._Base.GetPosition() as SourcePosition;
			if (sourcePosition != null)
			{
				sourcePosition.Length = this.m_sLength;
			}
			return sourcePosition;
		}

		// Token: 0x06000453 RID: 1107 RVA: 0x0000E142 File Offset: 0x0000D142
		public override ISignature GetSignature(IScope scope)
		{
			return this.m_expBase.GetSignature(scope);
		}

		// Token: 0x170000E1 RID: 225
		// (get) Token: 0x06000454 RID: 1108 RVA: 0x0000E150 File Offset: 0x0000D150
		// (set) Token: 0x06000455 RID: 1109 RVA: 0x00003AE9 File Offset: 0x00002AE9
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

		// Token: 0x06000456 RID: 1110 RVA: 0x0000E15D File Offset: 0x0000D15D
		[Obfuscation(Feature = "rename")]
		public override void SetPositionIntern(IMinimalPosition minpos)
		{
			this._Base.PositionIntern = minpos;
		}

		// Token: 0x170000E2 RID: 226
		// (get) Token: 0x06000457 RID: 1111 RVA: 0x0000E16B File Offset: 0x0000D16B
		// (set) Token: 0x06000458 RID: 1112 RVA: 0x0000E173 File Offset: 0x0000D173
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

		// Token: 0x170000E3 RID: 227
		// (get) Token: 0x06000459 RID: 1113 RVA: 0x0000E17C File Offset: 0x0000D17C
		public IExpression Base
		{
			get
			{
				return this._Base;
			}
		}

		// Token: 0x170000E4 RID: 228
		// (get) Token: 0x0600045A RID: 1114 RVA: 0x0000E184 File Offset: 0x0000D184
		// (set) Token: 0x0600045B RID: 1115 RVA: 0x0000E19A File Offset: 0x0000D19A
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

		// Token: 0x0600045C RID: 1116 RVA: 0x0000E1A4 File Offset: 0x0000D1A4
		public override _IExprement Duplicate()
		{
			CurrentTaskExpression currentTaskExpression = new CurrentTaskExpression();
			this.DuplicateCommon(currentTaskExpression);
			if (this.m_expBase != null)
			{
				currentTaskExpression.m_expBase = (this.m_expBase.Duplicate() as Expression);
			}
			return currentTaskExpression;
		}

		// Token: 0x0600045D RID: 1117 RVA: 0x0000E1DD File Offset: 0x0000D1DD
		public override bool IsConstant(IScope scope, bool bAllocatedOK)
		{
			return this._Base.IsConstant(scope, bAllocatedOK);
		}

		// Token: 0x0600045E RID: 1118 RVA: 0x0000E1EC File Offset: 0x0000D1EC
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

		// Token: 0x0600045F RID: 1119 RVA: 0x0000E21E File Offset: 0x0000D21E
		public override ILiteralValue Literal(IScope scope)
		{
			return this._Base.Literal(scope);
		}

		// Token: 0x06000460 RID: 1120 RVA: 0x0000E22C File Offset: 0x0000D22C
		public override ILiteralValue Literal(IPrecompileScope scope)
		{
			return this._Base.Literal(scope);
		}

		// Token: 0x06000461 RID: 1121 RVA: 0x0000E23C File Offset: 0x0000D23C
		public override ILiteralValue LiteralWithRecursionCheck(IScope scope, IRecursionGuard recursionGuard, bool bAllocatedOK, out bool bRecursionError)
		{
			IConstantFolder3 constantFolder = VersionedCompilerFactory._ConstantFolder_OrNull as IConstantFolder3;
			if (constantFolder != null)
			{
				return constantFolder.GetLiteralValue(this, scope, out bRecursionError);
			}
			return ((_IExpression2)this._Base).LiteralWithRecursionCheck(scope, recursionGuard, bAllocatedOK, out bRecursionError);
		}

		// Token: 0x06000462 RID: 1122 RVA: 0x0000E278 File Offset: 0x0000D278
		public override ILiteralValue LiteralWithRecursionCheck(IPrecompileScope scope, IRecursionGuard recursionGuard, bool bAllocatedOK, out bool bRecursionError)
		{
			IConstantFolder3 constantFolder = VersionedCompilerFactory._ConstantFolder_OrNull as IConstantFolder3;
			if (constantFolder != null)
			{
				return constantFolder.GetLiteralValue(this, scope, recursionGuard, out bRecursionError);
			}
			return ((_IExpression2)this._Base).LiteralWithRecursionCheck(scope, recursionGuard, bAllocatedOK, out bRecursionError);
		}

		// Token: 0x170000E5 RID: 229
		// (get) Token: 0x06000463 RID: 1123 RVA: 0x0000E2B4 File Offset: 0x0000D2B4
		// (set) Token: 0x06000464 RID: 1124 RVA: 0x00003AE9 File Offset: 0x00002AE9
		[DefaultSerialization("Type")]
		[StorageVersion("3.5.13.0")]
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

		// Token: 0x040000AA RID: 170
		[Obfuscation(Feature = "rename")]
		protected short m_sLength;

		// Token: 0x040000AB RID: 171
		[DefaultSerialization("Base")]
		[StorageVersion("3.5.13.0")]
		[Obfuscation(Feature = "rename")]
		private _IExpression m_expBase;
	}
}
