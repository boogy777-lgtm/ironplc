using System;
using System.Collections.Generic;
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
	// Token: 0x0200005D RID: 93
	[TypeGuid("{79a8baad-3f7f-40d2-8250-3037bf5e6631}")]
	[StorageVersion("3.3.0.0")]
	[SuppressMessage("Major Code Smell", "S1200:Classes should not be coupled to too many other classes (Single Responsibility Principle)", Justification = "Class cannot be divided into subclasses because of released interfaces")]
	public class IndexAccessExpression : Expression, _IIndexAccessExpression, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression, IIndexAccessExpression, ILengthExprement
	{
		// Token: 0x06000575 RID: 1397 RVA: 0x0000F37D File Offset: 0x0000E37D
		public IndexAccessExpression()
		{
		}

		// Token: 0x06000576 RID: 1398 RVA: 0x0000F391 File Offset: 0x0000E391
		internal IndexAccessExpression(_IExpression expBase)
		{
			this.m_expVar = expBase;
		}

		// Token: 0x06000577 RID: 1399 RVA: 0x0000F3AC File Offset: 0x0000E3AC
		internal IndexAccessExpression(_IExpression expBase, IToken token) : base(token)
		{
			this.m_expVar = expBase;
		}

		// Token: 0x06000578 RID: 1400 RVA: 0x0000F3C8 File Offset: 0x0000E3C8
		public override bool IsVarInOutInput(IScope scope, bool bWriteToConstants, bool bVarInoutConstant)
		{
			return this._Var.IsVarInOutInput(scope, bWriteToConstants, bVarInoutConstant);
		}

		// Token: 0x06000579 RID: 1401 RVA: 0x0000F3D8 File Offset: 0x0000E3D8
		public override bool IsLValue(IScope scope, bool bWriteToConstants)
		{
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV34400)
			{
				_IVariable ivariable = this._Var.GetVariable(scope) as _IVariable;
				if (ivariable != null && ivariable.IsProperty && ivariable.Type != null && (this._Var.Type != null && this._Var.Type.Class == TypeClass.Reference))
				{
					return true;
				}
			}
			return this._Var.IsLValue(scope, bWriteToConstants);
		}

		// Token: 0x1700012B RID: 299
		// (get) Token: 0x0600057A RID: 1402 RVA: 0x0000F450 File Offset: 0x0000E450
		public IExpression Var
		{
			get
			{
				return this._Var;
			}
		}

		// Token: 0x1700012C RID: 300
		// (get) Token: 0x0600057B RID: 1403 RVA: 0x0000F458 File Offset: 0x0000E458
		// (set) Token: 0x0600057C RID: 1404 RVA: 0x0000F46E File Offset: 0x0000E46E
		public _IExpression _Var
		{
			get
			{
				if (this.m_expVar == null)
				{
					return new NullExpression();
				}
				return this.m_expVar;
			}
			set
			{
				this.m_expVar = value;
			}
		}

		// Token: 0x1700012D RID: 301
		// (get) Token: 0x0600057D RID: 1405 RVA: 0x0000F478 File Offset: 0x0000E478
		public IExpression[] Accesses
		{
			get
			{
				_IExpression[] array = new _IExpression[this.m_expAccesses.Count];
				this.m_expAccesses.CopyTo(array);
				return array;
			}
		}

		// Token: 0x1700012E RID: 302
		// (get) Token: 0x0600057E RID: 1406 RVA: 0x0000F4A5 File Offset: 0x0000E4A5
		public ICollection<_IExpression> _Accesses
		{
			get
			{
				return this.m_expAccesses;
			}
		}

		// Token: 0x1700012F RID: 303
		// (get) Token: 0x0600057F RID: 1407 RVA: 0x0000F4AD File Offset: 0x0000E4AD
		public int NumAccesses
		{
			get
			{
				return this.m_expAccesses.Count;
			}
		}

		// Token: 0x06000580 RID: 1408 RVA: 0x0000F4BA File Offset: 0x0000E4BA
		public _IExpression GetAccess(int i)
		{
			return this.m_expAccesses[i];
		}

		// Token: 0x17000130 RID: 304
		public _IExpression this[int i]
		{
			get
			{
				return this.m_expAccesses[i];
			}
			set
			{
				this.m_expAccesses[i] = value;
			}
		}

		// Token: 0x06000583 RID: 1411 RVA: 0x0000F4D7 File Offset: 0x0000E4D7
		public void AddAccess(_IExpression expAcc)
		{
			this.m_expAccesses.Add(expAcc);
		}

		// Token: 0x06000584 RID: 1412 RVA: 0x0000F4E5 File Offset: 0x0000E4E5
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x06000585 RID: 1413 RVA: 0x0000F4EE File Offset: 0x0000E4EE
		public override T Accept<T>(IExprementVisitor<T> visitor)
		{
			return visitor.visit(this);
		}

		// Token: 0x06000586 RID: 1414 RVA: 0x0000F4F7 File Offset: 0x0000E4F7
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x06000587 RID: 1415 RVA: 0x0000F500 File Offset: 0x0000E500
		public override ISourcePosition GetPosition()
		{
			SourcePosition sourcePosition = this.m_expVar.GetPosition() as SourcePosition;
			if (sourcePosition != null)
			{
				sourcePosition.Length = this.m_sLength;
			}
			return sourcePosition;
		}

		// Token: 0x17000131 RID: 305
		// (get) Token: 0x06000588 RID: 1416 RVA: 0x0000F52E File Offset: 0x0000E52E
		// (set) Token: 0x06000589 RID: 1417 RVA: 0x0000F53B File Offset: 0x0000E53B
		[Obfuscation(Feature = "rename")]
		public override IMinimalPosition PositionIntern
		{
			get
			{
				return this.m_expVar.PositionIntern;
			}
			set
			{
				if (this.m_expVar != null)
				{
					this.m_expVar.PositionIntern = value;
				}
			}
		}

		// Token: 0x0600058A RID: 1418 RVA: 0x0000F551 File Offset: 0x0000E551
		[Obfuscation(Feature = "rename")]
		public override void SetPositionIntern(IMinimalPosition minpos)
		{
			this.m_expVar.PositionIntern = minpos;
		}

		// Token: 0x17000132 RID: 306
		// (get) Token: 0x0600058B RID: 1419 RVA: 0x0000F55F File Offset: 0x0000E55F
		// (set) Token: 0x0600058C RID: 1420 RVA: 0x0000F567 File Offset: 0x0000E567
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

		// Token: 0x0600058D RID: 1421 RVA: 0x0000F570 File Offset: 0x0000E570
		public override IVariable GetVariable(IScope scope)
		{
			return this._Var.GetVariable(scope);
		}

		// Token: 0x0600058E RID: 1422 RVA: 0x0000F57E File Offset: 0x0000E57E
		public override IVariable GetVariable(IPrecompileScope scope)
		{
			return this._Var.GetVariable(scope);
		}

		// Token: 0x0600058F RID: 1423 RVA: 0x0000F58C File Offset: 0x0000E58C
		public override ISignature GetSignature(IScope scope)
		{
			return this._Var.GetSignature(scope);
		}

		// Token: 0x17000133 RID: 307
		// (get) Token: 0x06000590 RID: 1424 RVA: 0x0000F59A File Offset: 0x0000E59A
		// (set) Token: 0x06000591 RID: 1425 RVA: 0x0000F5A7 File Offset: 0x0000E5A7
		public override int SignatureId
		{
			get
			{
				return this._Var.SignatureId;
			}
			set
			{
				throw new NotSupportedException("signature id cant be set in Index Access");
			}
		}

		// Token: 0x06000592 RID: 1426 RVA: 0x0000F5B4 File Offset: 0x0000E5B4
		public override _IExprement Duplicate()
		{
			IndexAccessExpression indexAccessExpression = new IndexAccessExpression();
			this.DuplicateCommon(indexAccessExpression);
			if (this.m_expVar != null)
			{
				indexAccessExpression.m_expVar = (this.m_expVar.Duplicate() as _IExpression);
			}
			foreach (_IExpression iexpression in this.m_expAccesses)
			{
				indexAccessExpression.AddAccess(iexpression.Duplicate() as _IExpression);
			}
			indexAccessExpression.m_expAccesses.TrimExcess();
			return indexAccessExpression;
		}

		// Token: 0x06000593 RID: 1427 RVA: 0x0000F644 File Offset: 0x0000E644
		public override bool IsConstant(IScope scope, bool bAllocatedOK)
		{
			if (!bAllocatedOK)
			{
				return false;
			}
			if (!this._Var.IsConstant(scope, bAllocatedOK))
			{
				return false;
			}
			using (IEnumerator<_IExpression> enumerator = this.m_expAccesses.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (!enumerator.Current.IsConstant(scope, bAllocatedOK))
					{
						return false;
					}
				}
			}
			return true;
		}

		// Token: 0x06000594 RID: 1428 RVA: 0x0000F6B0 File Offset: 0x0000E6B0
		[SuppressMessage("Critical Code Smell", "S3776:Cognitive Complexity of methods should not be too high", Justification = "Will be fixed with CDS-86324")]
		public bool GetConstantIndex(IScope scope, IRecursionGuard recursionGuard, out bool bRecursion, out int nIndex)
		{
			nIndex = 0;
			int num = 1;
			bRecursion = false;
			bool flag = false;
			IVariable variable = this._Var.GetVariable(scope);
			if (variable != null && variable.CompiledType.Class == TypeClass.Array && variable.GetFlag(VarFlag.Constant))
			{
				IArrayType arrayType = variable.CompiledType as ArrayType;
				for (int i = arrayType.Dimensions.Length - 1; i >= 0; i--)
				{
					ILiteralValue literalValue = ((_IExpression2)this.GetAccess(i)).LiteralWithRecursionCheck(scope, recursionGuard, false, out bRecursion);
					if (literalValue == null | bRecursion)
					{
						return false;
					}
					int num2 = arrayType.Dimensions[i].LowerBorderInt(out flag, scope);
					if (!flag)
					{
						return false;
					}
					int num3 = arrayType.Dimensions[i].UpperBorderInt(out flag, scope);
					if (!flag)
					{
						return false;
					}
					int @int = literalValue.GetInt(out flag);
					if (!flag)
					{
						return false;
					}
					if (@int < num2 || @int > num3)
					{
						return false;
					}
					int num4 = arrayType.Dimensions[i].Range(out flag, scope);
					if (!flag)
					{
						return false;
					}
					nIndex += (@int - num2) * num;
					num *= num4;
				}
			}
			return true;
		}

		// Token: 0x06000595 RID: 1429 RVA: 0x0000F7C4 File Offset: 0x0000E7C4
		public override ILiteralValue Literal(IScope scope)
		{
			bool flag;
			return base.LiteralWithRecursionCheck(scope, new LDictionary<IVariable, IVariable>(), false, out flag);
		}

		// Token: 0x06000596 RID: 1430 RVA: 0x0000F7E0 File Offset: 0x0000E7E0
		public override ILiteralValue Literal(IPrecompileScope scope)
		{
			bool greaterEqualV = APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35900;
			bool flag;
			return base.LiteralWithRecursionCheck(scope, new LDictionary<IVariable, IVariable>(), greaterEqualV, out flag);
		}

		// Token: 0x06000597 RID: 1431 RVA: 0x0000F80C File Offset: 0x0000E80C
		[SuppressMessage("Critical Code Smell", "S3776:Cognitive Complexity of methods should not be too high", Justification = "Will be fixed with CDS-86324")]
		public override ILiteralValue LiteralWithRecursionCheck(IScope scope, IRecursionGuard recursionGuard, bool bAllocatedOK, out bool bRecursionError)
		{
			IConstantFolder3 constantFolder = VersionedCompilerFactory._ConstantFolder_OrNull as IConstantFolder3;
			if (constantFolder != null)
			{
				return constantFolder.GetLiteralValue(this, scope, out bRecursionError);
			}
			bRecursionError = false;
			bool flag = false;
			ILiteralValue literalValue = ((_IExpression2)this._Var).LiteralWithRecursionCheck(scope, recursionGuard, bAllocatedOK, out bRecursionError);
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35300 && literalValue == null)
			{
				IVariable variable = this._Var.GetVariable(scope);
				if (variable != null && variable.Initial is IArrayInitialization && variable.CompiledType.Class == TypeClass.Array && variable.GetFlag(VarFlag.Constant))
				{
					int num = 0;
					int num2 = 1;
					ArrayInitialisation arrayInitialisation = variable.Initial as ArrayInitialisation;
					IArrayType arrayType = variable.CompiledType as ArrayType;
					if (arrayInitialisation == null || arrayType.Dimensions.Length != this._Accesses.Count)
					{
						return base.LiteralWithRecursionCheck(scope, recursionGuard, bAllocatedOK, out bRecursionError);
					}
					bool flag2 = false;
					IList<_IExpression> flatList = arrayInitialisation.GetFlatList(scope, out flag2);
					if (flatList == null || !flag2)
					{
						return base.LiteralWithRecursionCheck(scope, recursionGuard, bAllocatedOK, out bRecursionError);
					}
					for (int i = arrayType.Dimensions.Length - 1; i >= 0; i--)
					{
						ILiteralValue literalValue2 = ((_IExpression2)this.GetAccess(i)).LiteralWithRecursionCheck(scope, recursionGuard, bAllocatedOK, out flag);
						if (flag)
						{
							bRecursionError = true;
						}
						if (literalValue2 == null)
						{
							return base.LiteralWithRecursionCheck(scope, recursionGuard, bAllocatedOK, out bRecursionError);
						}
						int num3 = arrayType.Dimensions[i].LowerBorderInt(out flag2, scope);
						if (!flag2)
						{
							return base.LiteralWithRecursionCheck(scope, recursionGuard, bAllocatedOK, out bRecursionError);
						}
						int num4 = arrayType.Dimensions[i].Range(out flag2, scope);
						if (!flag2)
						{
							return base.LiteralWithRecursionCheck(scope, recursionGuard, bAllocatedOK, out bRecursionError);
						}
						int @int = literalValue2.GetInt(out flag2);
						if (!flag2)
						{
							return base.LiteralWithRecursionCheck(scope, recursionGuard, bAllocatedOK, out bRecursionError);
						}
						num += (@int - num3) * num2;
						num2 *= num4;
					}
					if (flatList.Count <= num || num < 0)
					{
						return base.LiteralWithRecursionCheck(scope, recursionGuard, bAllocatedOK, out bRecursionError);
					}
					return (flatList[num] as Expression).LiteralWithRecursionCheck(scope, recursionGuard, bAllocatedOK, out bRecursionError);
				}
			}
			if (!APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV3204 || literalValue == null || this.m_expAccesses.Count != 1)
			{
				return base.LiteralWithRecursionCheck(scope, recursionGuard, bAllocatedOK, out bRecursionError);
			}
			ILiteralValue literalValue3 = ((_IExpression2)this.GetAccess(0)).LiteralWithRecursionCheck(scope, recursionGuard, bAllocatedOK, out flag);
			if (flag)
			{
				bRecursionError = true;
			}
			if (literalValue3 == null)
			{
				return base.LiteralWithRecursionCheck(scope, recursionGuard, bAllocatedOK, out bRecursionError);
			}
			bool flag3;
			int int2 = literalValue3.GetInt(out flag3);
			if (!flag3)
			{
				return base.LiteralWithRecursionCheck(scope, recursionGuard, bAllocatedOK, out bRecursionError);
			}
			string @string = literalValue.GetString(out flag3);
			if (!flag3)
			{
				return base.LiteralWithRecursionCheck(scope, recursionGuard, bAllocatedOK, out bRecursionError);
			}
			return new LiteralValue((long)((ulong)@string[int2]));
		}

		// Token: 0x06000598 RID: 1432 RVA: 0x0000FABC File Offset: 0x0000EABC
		[SuppressMessage("Critical Code Smell", "S3776:Cognitive Complexity of methods should not be too high", Justification = "Will be fixed with CDS-86324")]
		public override IDataLocation DataLocation(IScope scope)
		{
			IDataLocation dataLocation = this._Var.DataLocation(scope);
			if (dataLocation == null)
			{
				return null;
			}
			int num = base.Type.Size(scope);
			int num2 = 0;
			_IArrayType iarrayType = this._Var.Type as _IArrayType;
			_IVectorType ivectorType = this._Var.Type as _IVectorType;
			if (iarrayType == null && ivectorType == null)
			{
				return null;
			}
			if (iarrayType != null)
			{
				IList<_IArrayDimension> dimensions = iarrayType._Dimensions;
				for (int i = this._Accesses.Count - 1; i >= 0; i--)
				{
					_IExpression access = this.GetAccess(i);
					if (!access.IsConstant(scope, false))
					{
						return null;
					}
					ILiteralValue literalValue = access.Literal(scope);
					if (literalValue == null)
					{
						return null;
					}
					bool flag;
					int @int = literalValue.GetInt(out flag);
					if (!flag)
					{
						return null;
					}
					int num3 = dimensions[i].LowerBorderInt(out flag, scope);
					if (!flag)
					{
						return null;
					}
					int num4 = dimensions[i].Range(out flag, scope);
					if (!flag)
					{
						return null;
					}
					num2 += (@int - num3) * num;
					num *= num4;
				}
			}
			else
			{
				_IExpression access2 = this.GetAccess(0);
				if (!access2.IsConstant(scope, false))
				{
					return null;
				}
				ILiteralValue literalValue2 = access2.Literal(scope);
				if (literalValue2 == null)
				{
					return null;
				}
				bool flag;
				int int2 = literalValue2.GetInt(out flag);
				if (!flag)
				{
					return null;
				}
				num2 = int2 * num;
			}
			if (dataLocation.IsRelativ)
			{
				return LanguageModelBuilder.Singleton.CreateRelativeDataLocation(dataLocation.Offset + num2);
			}
			return LanguageModelBuilder.Singleton.CreateDataLocation(dataLocation.Area, dataLocation.Offset + num2);
		}

		// Token: 0x17000134 RID: 308
		// (get) Token: 0x06000599 RID: 1433 RVA: 0x0000FC2D File Offset: 0x0000EC2D
		// (set) Token: 0x0600059A RID: 1434 RVA: 0x0000FC35 File Offset: 0x0000EC35
		public override IExprInfo Info
		{
			get
			{
				return this.m_expInfo;
			}
			set
			{
				this.m_expInfo = value;
			}
		}

		// Token: 0x17000135 RID: 309
		// (get) Token: 0x0600059B RID: 1435 RVA: 0x0000FC3E File Offset: 0x0000EC3E
		// (set) Token: 0x0600059C RID: 1436 RVA: 0x0000FC46 File Offset: 0x0000EC46
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

		// Token: 0x17000136 RID: 310
		// (get) Token: 0x0600059D RID: 1437 RVA: 0x0000FC4F File Offset: 0x0000EC4F
		// (set) Token: 0x0600059E RID: 1438 RVA: 0x0000FC5C File Offset: 0x0000EC5C
		public override int PrecompileVariableId
		{
			get
			{
				return this._Var.PrecompileVariableId;
			}
			set
			{
				this._Var.PrecompileVariableId = value;
			}
		}

		// Token: 0x17000137 RID: 311
		// (get) Token: 0x0600059F RID: 1439 RVA: 0x0000FC6A File Offset: 0x0000EC6A
		// (set) Token: 0x060005A0 RID: 1440 RVA: 0x0000FC77 File Offset: 0x0000EC77
		public override int PrecompileSignatureId
		{
			get
			{
				return this._Var.PrecompileSignatureId;
			}
			set
			{
				this._Var.PrecompileSignatureId = value;
			}
		}

		// Token: 0x040000C3 RID: 195
		[Obfuscation(Feature = "rename")]
		protected short m_sLength;

		// Token: 0x040000C4 RID: 196
		[Obfuscation(Feature = "rename")]
		[DefaultSerialization("compiledtype")]
		[StorageVersion("3.3.0.0")]
		private ICompiledType m_ctype;

		// Token: 0x040000C5 RID: 197
		[Obfuscation(Feature = "rename")]
		private IExprInfo m_expInfo;

		// Token: 0x040000C6 RID: 198
		[DefaultSerialization("Variable")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private _IExpression m_expVar;

		// Token: 0x040000C7 RID: 199
		[DefaultSerialization("Accesses")]
		[StorageVersion("3.3.0.0")]
		[StorageSaveAsNonGenericCollection("3.3.0.0-3.5.6.255")]
		[Obfuscation(Feature = "rename")]
		private LList<_IExpression> m_expAccesses = new LList<_IExpression>(1);
	}
}
