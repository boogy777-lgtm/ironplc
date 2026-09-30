using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000050 RID: 80
	[TypeGuid("{182b8553-40b6-4da1-b365-72d137f05911}")]
	[StorageVersion("3.3.0.0")]
	public abstract class Expression : Exprement, _IExpression2, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression
	{
		// Token: 0x0600049D RID: 1181 RVA: 0x0000E573 File Offset: 0x0000D573
		protected Expression()
		{
		}

		// Token: 0x0600049E RID: 1182 RVA: 0x0000E57B File Offset: 0x0000D57B
		protected Expression(IToken token) : base(token)
		{
		}

		// Token: 0x0600049F RID: 1183 RVA: 0x0000E584 File Offset: 0x0000D584
		public virtual bool IsVarInOutInput(IScope scope, bool bWriteToConstants, bool bVarInoutConstant)
		{
			return this.IsLValue(scope, bWriteToConstants);
		}

		// Token: 0x060004A0 RID: 1184
		public abstract bool IsLValue(IScope scope, bool bWriteToConstants);

		// Token: 0x060004A1 RID: 1185 RVA: 0x0000E584 File Offset: 0x0000D584
		public virtual bool IsLValue(IScope scope, bool bWriteToConstants, bool bPassToVarInout)
		{
			return this.IsLValue(scope, bWriteToConstants);
		}

		// Token: 0x060004A2 RID: 1186 RVA: 0x0000E58E File Offset: 0x0000D58E
		public virtual bool IsLValue(IScope scope, bool bWriteToConstants, bool bPassToVarInout, bool bRefAssign)
		{
			return this.IsLValue(scope, bWriteToConstants, bPassToVarInout);
		}

		// Token: 0x170000F1 RID: 241
		// (get) Token: 0x060004A3 RID: 1187 RVA: 0x0000E59C File Offset: 0x0000D59C
		public virtual IIdentifierInfo2 IdentifierInfo
		{
			get
			{
				ISignature6 signatureForPrecompileID = APEnvironmentFacade.Instance.LanguageModelMgr.GetSignatureForPrecompileID(this.PrecompileSignatureId);
				if (signatureForPrecompileID == null)
				{
					return null;
				}
				IVariable variableForPrecompileId = signatureForPrecompileID.GetVariableForPrecompileId(this.PrecompileVariableId);
				string stComment = null;
				IType type = null;
				if (variableForPrecompileId != null)
				{
					stComment = variableForPrecompileId.Comment;
					type = variableForPrecompileId.Type;
				}
				return new IdentifierInfo(variableForPrecompileId, signatureForPrecompileID, this.ToString(), stComment, IdentifierInfoFlag.None, type);
			}
		}

		// Token: 0x170000F2 RID: 242
		// (get) Token: 0x060004A4 RID: 1188 RVA: 0x0000E5F6 File Offset: 0x0000D5F6
		// (set) Token: 0x060004A5 RID: 1189 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public virtual int PrecompileVariableId
		{
			get
			{
				return Common.InvalidID;
			}
			set
			{
			}
		}

		// Token: 0x170000F3 RID: 243
		// (get) Token: 0x060004A6 RID: 1190 RVA: 0x0000E5F6 File Offset: 0x0000D5F6
		// (set) Token: 0x060004A7 RID: 1191 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public virtual int PrecompileSignatureId
		{
			get
			{
				return Common.InvalidID;
			}
			set
			{
			}
		}

		// Token: 0x060004A8 RID: 1192 RVA: 0x00005F0F File Offset: 0x00004F0F
		public virtual IVariable GetVariable(IScope scope)
		{
			return null;
		}

		// Token: 0x060004A9 RID: 1193 RVA: 0x00005F0F File Offset: 0x00004F0F
		public virtual IVariable GetVariable(IPrecompileScope scope)
		{
			return null;
		}

		// Token: 0x170000F4 RID: 244
		// (get) Token: 0x060004AA RID: 1194 RVA: 0x000042F0 File Offset: 0x000032F0
		public static int InvalidId
		{
			get
			{
				return -1;
			}
		}

		// Token: 0x170000F5 RID: 245
		// (get) Token: 0x060004AB RID: 1195 RVA: 0x0000E5FD File Offset: 0x0000D5FD
		// (set) Token: 0x060004AC RID: 1196 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public virtual int VariableId
		{
			get
			{
				return Expression.InvalidId;
			}
			set
			{
			}
		}

		// Token: 0x060004AD RID: 1197 RVA: 0x0000E604 File Offset: 0x0000D604
		public bool IsEqual(IExpression expression)
		{
			return base.IsEqual(expression as Exprement);
		}

		// Token: 0x170000F6 RID: 246
		// (get) Token: 0x060004AE RID: 1198 RVA: 0x00004E6B File Offset: 0x00003E6B
		public virtual bool IsPOUReference
		{
			get
			{
				return false;
			}
		}

		// Token: 0x060004AF RID: 1199 RVA: 0x00005F0F File Offset: 0x00004F0F
		public virtual ISignature GetSignature(IScope scope)
		{
			return null;
		}

		// Token: 0x060004B0 RID: 1200 RVA: 0x0000E612 File Offset: 0x0000D612
		public virtual ISignature GetSignatureEx(IScope scope)
		{
			return scope[this.SignatureId];
		}

		// Token: 0x170000F7 RID: 247
		// (get) Token: 0x060004B1 RID: 1201 RVA: 0x0000E5FD File Offset: 0x0000D5FD
		// (set) Token: 0x060004B2 RID: 1202 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public virtual int SignatureId
		{
			get
			{
				return Expression.InvalidId;
			}
			set
			{
			}
		}

		// Token: 0x170000F8 RID: 248
		// (get) Token: 0x060004B3 RID: 1203 RVA: 0x00004E6B File Offset: 0x00003E6B
		public virtual bool IsLiteral
		{
			get
			{
				return false;
			}
		}

		// Token: 0x060004B4 RID: 1204 RVA: 0x0000E620 File Offset: 0x0000D620
		public virtual ILiteralValue LiteralUnchecked(IScope scope)
		{
			IConstantFolder3 constantFolder = VersionedCompilerFactory._ConstantFolder_OrNull as IConstantFolder3;
			if (constantFolder != null)
			{
				bool flag;
				return constantFolder.GetLiteralValue(this, scope, out flag);
			}
			return this.Literal(scope);
		}

		// Token: 0x060004B5 RID: 1205 RVA: 0x0000E650 File Offset: 0x0000D650
		public ILiteralValue LiteralWithRecursionCheck(IScope scope, IDictionary<IVariable, IVariable> variableStack, bool bAllocatedOK, out bool bRecursionError)
		{
			RecursionGuard recursionGuard = new RecursionGuard();
			foreach (KeyValuePair<IVariable, IVariable> keyValuePair in variableStack)
			{
				recursionGuard.Add(keyValuePair.Key);
			}
			IConstantFolder3 constantFolder = VersionedCompilerFactory._ConstantFolder_OrNull as IConstantFolder3;
			if (constantFolder != null)
			{
				return constantFolder.GetLiteralValue(this, scope, recursionGuard, out bRecursionError);
			}
			return this.LiteralWithRecursionCheck(scope, recursionGuard, bAllocatedOK, out bRecursionError);
		}

		// Token: 0x060004B6 RID: 1206 RVA: 0x0000E6CC File Offset: 0x0000D6CC
		public virtual ILiteralValue LiteralWithRecursionCheck(IScope scope, IRecursionGuard recursionGuard, bool bAllocatedOK, out bool bRecursionError)
		{
			bRecursionError = false;
			return null;
		}

		// Token: 0x060004B7 RID: 1207 RVA: 0x0000E6D4 File Offset: 0x0000D6D4
		public ILiteralValue LiteralWithRecursionCheck(IPrecompileScope scope, IDictionary<IVariable, IVariable> variableStack, bool bAllocatedOK, out bool bRecursionError)
		{
			RecursionGuard recursionGuard = new RecursionGuard();
			foreach (KeyValuePair<IVariable, IVariable> keyValuePair in variableStack)
			{
				recursionGuard.Add(keyValuePair.Key);
			}
			IConstantFolder3 constantFolder = VersionedCompilerFactory._ConstantFolder_OrNull as IConstantFolder3;
			if (constantFolder != null)
			{
				return constantFolder.GetLiteralValue(this, scope, recursionGuard, out bRecursionError);
			}
			return this.LiteralWithRecursionCheck(scope, recursionGuard, bAllocatedOK, out bRecursionError);
		}

		// Token: 0x060004B8 RID: 1208 RVA: 0x0000E6CC File Offset: 0x0000D6CC
		public virtual ILiteralValue LiteralWithRecursionCheck(IPrecompileScope scope, IRecursionGuard recursionGuard, bool bAllocatedOK, out bool bRecursionError)
		{
			bRecursionError = false;
			return null;
		}

		// Token: 0x060004B9 RID: 1209 RVA: 0x00005F0F File Offset: 0x00004F0F
		public virtual ILiteralValue Literal(IScope scope)
		{
			return null;
		}

		// Token: 0x060004BA RID: 1210 RVA: 0x0000E750 File Offset: 0x0000D750
		public virtual ILiteralValue Literal(IScope scope, bool bAllocatedOK)
		{
			return this.Literal(scope);
		}

		// Token: 0x060004BB RID: 1211 RVA: 0x00005F0F File Offset: 0x00004F0F
		public virtual ILiteralValue Literal(IPrecompileScope scope)
		{
			return null;
		}

		// Token: 0x060004BC RID: 1212 RVA: 0x00005F0F File Offset: 0x00004F0F
		public virtual IDataLocation DataLocation(IScope scope)
		{
			return null;
		}

		// Token: 0x170000F9 RID: 249
		// (get) Token: 0x060004BD RID: 1213 RVA: 0x0000E759 File Offset: 0x0000D759
		public bool IsCompiled
		{
			get
			{
				return this._CompiledType != null;
			}
		}

		// Token: 0x170000FA RID: 250
		// (get) Token: 0x060004BE RID: 1214 RVA: 0x0000E764 File Offset: 0x0000D764
		// (set) Token: 0x060004BF RID: 1215 RVA: 0x0000E78D File Offset: 0x0000D78D
		public ICompiledType Type
		{
			get
			{
				_IType itype = this._CompiledType as _IType;
				if (itype == null)
				{
					return this._CompiledType;
				}
				return itype.EffectiveType;
			}
			set
			{
				this._CompiledType = value;
			}
		}

		// Token: 0x060004C0 RID: 1216 RVA: 0x00004E6B File Offset: 0x00003E6B
		public virtual bool IsConstant(IScope scope, bool bAllocatedOK)
		{
			return false;
		}

		// Token: 0x170000FB RID: 251
		// (get) Token: 0x060004C1 RID: 1217 RVA: 0x000042F0 File Offset: 0x000032F0
		// (set) Token: 0x060004C2 RID: 1218 RVA: 0x0000E796 File Offset: 0x0000D796
		public virtual int ScratchOffset
		{
			get
			{
				return -1;
			}
			set
			{
				throw new NotSupportedException("not supported in this type of expression");
			}
		}

		// Token: 0x170000FC RID: 252
		// (get) Token: 0x060004C3 RID: 1219 RVA: 0x00004E6B File Offset: 0x00003E6B
		// (set) Token: 0x060004C4 RID: 1220 RVA: 0x0000E7A2 File Offset: 0x0000D7A2
		[Obsolete("this flag is not supported any more!")]
		[SuppressMessage("Info Code Smell", "S1133:Deprecated code should be removed", Justification = "Property cannot be removed because of released interfaces")]
		public bool IsStatement
		{
			get
			{
				return false;
			}
			set
			{
				throw new NotSupportedException("not supported anymore");
			}
		}

		// Token: 0x060004C5 RID: 1221 RVA: 0x0000E7B0 File Offset: 0x0000D7B0
		public override void DuplicateCommon(Exprement exprem)
		{
			base.DuplicateCommon(exprem);
			Expression expression = exprem as Expression;
			if (expression == null)
			{
				return;
			}
			expression._CompiledType = this._CompiledType;
		}

		// Token: 0x060004C6 RID: 1222 RVA: 0x0000E7DC File Offset: 0x0000D7DC
		public override void AfterDeserialize()
		{
			base.AfterDeserialize();
			if (!APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV33010)
			{
				this._CompiledType = null;
				return;
			}
			if (this._CompiledType != null && this._CompiledType.IsEqual(TypeTable.Get(this._CompiledType.Class)))
			{
				this._CompiledType = TypeTable.GetStaticType(this._CompiledType);
			}
		}

		// Token: 0x170000FD RID: 253
		// (get) Token: 0x060004C7 RID: 1223
		// (set) Token: 0x060004C8 RID: 1224
		[DefaultSerialization("Type")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		public abstract ICompiledType _CompiledType { get; set; }
	}
}
