using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x020000FA RID: 250
	public class IndexAccessAddressInfoCalculator
	{
		// Token: 0x1700054C RID: 1356
		// (get) Token: 0x0600123A RID: 4666 RVA: 0x000337C0 File Offset: 0x000327C0
		private VarReferenceVisitor.VRStackContent TopOfStack { get; }

		// Token: 0x1700054D RID: 1357
		// (get) Token: 0x0600123B RID: 4667 RVA: 0x000337C8 File Offset: 0x000327C8
		private IScope5 Scope { get; }

		// Token: 0x1700054E RID: 1358
		// (get) Token: 0x0600123C RID: 4668 RVA: 0x000337D0 File Offset: 0x000327D0
		private IVarReferenceGenerator Generator { get; }

		// Token: 0x1700054F RID: 1359
		// (get) Token: 0x0600123D RID: 4669 RVA: 0x000337D8 File Offset: 0x000327D8
		private VarReferenceVisitor Visitor { get; }

		// Token: 0x0600123E RID: 4670 RVA: 0x000337E0 File Offset: 0x000327E0
		internal IndexAccessAddressInfoCalculator(VarReferenceVisitor.VRStackContent topOfStack, IScope5 scope, IVarReferenceGenerator generator, VarReferenceVisitor varReferenceVisitor)
		{
			this.TopOfStack = topOfStack;
			this.Scope = scope;
			this.Generator = generator;
			this.Visitor = varReferenceVisitor;
		}

		// Token: 0x0600123F RID: 4671 RVA: 0x00033808 File Offset: 0x00032808
		public void CalculateIndexAccessAddressInfo(_IIndexAccessExpression indexaccess, bool bIsVarLenArray, IAddressInfo aiBase, int nAreaBase, int nAddressBase)
		{
			this._indexExpressions = indexaccess._Accesses;
			this._aiIndices = null;
			this._aiBounds = null;
			if (indexaccess._Var.Type == null)
			{
				return;
			}
			this._bValid = false;
			this._bDeRef = false;
			this._bVariableIndexAccess = false;
			this._nOffset = 0;
			this._nSize = 0;
			ICompiledType deRefType = this.GetDeRefType(indexaccess, bIsVarLenArray, out this._bForceAddressSize);
			aiBase = this.CreateTypeSpecificBaseAddressInfo(indexaccess, aiBase, deRefType);
			this.Generator.Remove(aiBase);
			if (this._bValid)
			{
				this.CreateAddressInfo(indexaccess, aiBase, nAreaBase, nAddressBase, deRefType);
				return;
			}
			this.TopOfStack.Area = -1;
			this.TopOfStack.Address = 0;
			this.TopOfStack.AddressInfo = null;
		}

		// Token: 0x06001240 RID: 4672 RVA: 0x000338C0 File Offset: 0x000328C0
		private void CreateAddressInfo(_IIndexAccessExpression indexaccess, IAddressInfo aiBase, int nAreaBase, int nAddressBase, ICompiledType ctypeDeRef)
		{
			if (nAreaBase == -1)
			{
				this.TopOfStack.Address = -1;
				this.TopOfStack.Area = -1;
				this.CreateRelativAddressInfo(indexaccess, aiBase);
			}
			else
			{
				this.CreateAddressInfoOnStaticBase(indexaccess, aiBase, nAreaBase, nAddressBase, ctypeDeRef);
			}
			if (this._bForceAddressSize)
			{
				((IMyAddressInfo)this.TopOfStack.AddressInfo).SetSize(this._nSize);
			}
		}

		// Token: 0x06001241 RID: 4673 RVA: 0x00033924 File Offset: 0x00032924
		private void CreateAddressInfoOnStaticBase(_IIndexAccessExpression indexaccess, IAddressInfo aiBase, int nAreaBase, int nAddressBase, ICompiledType ctypeDeRef)
		{
			if (this._bDeRef)
			{
				this.TopOfStack.AddressInfo = this.Generator.GenerateDeRefAccess(indexaccess, aiBase, this._nOffset);
				this.TopOfStack.Address = -1;
				this.TopOfStack.Area = -1;
				return;
			}
			if (!this._bVariableIndexAccess)
			{
				this.TopOfStack.Area = nAreaBase;
				this.TopOfStack.Address = nAddressBase + this._nOffset;
				this.TopOfStack.AddressInfo = this.Generator.GenerateIndexAccess(indexaccess, this.TopOfStack.Area, this.TopOfStack.Address, this._nSize);
				return;
			}
			this.TopOfStack.Address = -1;
			this.TopOfStack.Area = -1;
			if (aiBase != null)
			{
				((IMyAddressInfo)aiBase).SetSize(this._nSize);
				this.TopOfStack.AddressInfo = this.Generator.GenerateVariableIndexAccess(indexaccess, aiBase, this._nSize, this._aiIndices, this._aiBounds, ctypeDeRef);
				return;
			}
			this.TopOfStack.AddressInfo = null;
		}

		// Token: 0x06001242 RID: 4674 RVA: 0x00033A34 File Offset: 0x00032A34
		private void CreateRelativAddressInfo(_IIndexAccessExpression indexaccess, IAddressInfo aiBase)
		{
			if (this._bDeRef)
			{
				this.TopOfStack.AddressInfo = this.Generator.GenerateDeRefAccess(indexaccess, aiBase, this._nOffset);
				return;
			}
			if (!this._bVariableIndexAccess)
			{
				this.TopOfStack.AddressInfo = this.Generator.GenerateCompoAccess(indexaccess, aiBase, this._nOffset);
				return;
			}
			if (aiBase != null)
			{
				((IMyAddressInfo)aiBase).SetSize(this._nSize);
				this.TopOfStack.AddressInfo = this.Generator.GenerateVariableIndexAccess(indexaccess, aiBase, this._nSize, this._aiIndices, this._aiBounds, indexaccess._CompiledType);
				return;
			}
			this.TopOfStack.AddressInfo = null;
		}

		// Token: 0x17000550 RID: 1360
		// (get) Token: 0x06001243 RID: 4675 RVA: 0x00033AE0 File Offset: 0x00032AE0
		internal static _ILanguageModelBuilder2 Builder
		{
			get
			{
				if (IndexAccessAddressInfoCalculator.s_lmbuilder == null)
				{
					IndexAccessAddressInfoCalculator.s_lmbuilder = (APEnvironmentFacade.Instance.LanguageModelMgr.CreateLanguageModelBuilder() as _ILanguageModelBuilder2);
				}
				return IndexAccessAddressInfoCalculator.s_lmbuilder;
			}
		}

		// Token: 0x06001244 RID: 4676 RVA: 0x00033B07 File Offset: 0x00032B07
		internal static _ILiteralExpression CreateLiteralExpression(long lVal)
		{
			return IndexAccessAddressInfoCalculator.Builder.CreateLiteralExpression(null, lVal) as _ILiteralExpression;
		}

		// Token: 0x06001245 RID: 4677 RVA: 0x00033B1C File Offset: 0x00032B1C
		private IAddressInfo CreateTypeSpecificBaseAddressInfo(_IIndexAccessExpression indexaccess, IAddressInfo aiBase, ICompiledType ctypeDeRef)
		{
			TypeClass @class = ctypeDeRef.Class;
			if (@class <= TypeClass.Pointer)
			{
				if (@class - TypeClass.String > 1)
				{
					if (@class == TypeClass.Pointer)
					{
						this._nSize = this.CalculateOffsetForPointerAccess(indexaccess, ctypeDeRef, ref aiBase);
					}
				}
				else
				{
					int num = 0;
					_IArrayType iarrayType;
					if (ctypeDeRef.Class == TypeClass.String)
					{
						_IStringType istringType = ctypeDeRef as _IStringType;
						iarrayType = IndexAccessAddressInfoCalculator.Builder.CreateArrayType(TypeTable.Byte);
						iarrayType.AddDimension(IndexAccessAddressInfoCalculator.CreateLiteralExpression(0L), IndexAccessAddressInfoCalculator.CreateLiteralExpression((long)istringType.Size(this.Scope)));
						num = 1;
					}
					else
					{
						_IWStringType iwstringType = ctypeDeRef as _IWStringType;
						iarrayType = IndexAccessAddressInfoCalculator.Builder.CreateArrayType(TypeTable.Word);
						iarrayType.AddDimension(IndexAccessAddressInfoCalculator.CreateLiteralExpression(0L), IndexAccessAddressInfoCalculator.CreateLiteralExpression((long)(iwstringType.Size(this.Scope) / 2)));
						num = 2;
					}
					this._nSize = this.CalculateOffsetForArrayAccess(indexaccess, iarrayType, iarrayType._Dimensions, ref num);
				}
			}
			else if (@class != TypeClass.Array)
			{
				if (@class == TypeClass.__Vector)
				{
					_IVectorType ivectorType = ctypeDeRef as _IVectorType;
					if (ivectorType != null && ivectorType.BaseType != null)
					{
						this._nSize = this.CalculateOffsetForVectorAccess(indexaccess, ivectorType);
					}
				}
			}
			else
			{
				_IArrayType iarrayType2 = ctypeDeRef as _IArrayType;
				if (iarrayType2 != null && iarrayType2.BaseType != null)
				{
					IList<_IArrayDimension> dimensions = iarrayType2._Dimensions;
					int num2 = iarrayType2.BaseType.Size(this.Scope);
					if (dimensions.Count == this._indexExpressions.Count)
					{
						this._nSize = this.CalculateOffsetForArrayAccess(indexaccess, iarrayType2, dimensions, ref num2);
					}
				}
			}
			return aiBase;
		}

		// Token: 0x06001246 RID: 4678 RVA: 0x00033C9C File Offset: 0x00032C9C
		private int CalculateOffsetForPointerAccess(_IIndexAccessExpression indexAccessExpression, ICompiledType ctypeDeRef, ref IAddressInfo aiBase)
		{
			int num = 0;
			_IExpression iexpression = indexAccessExpression[0];
			if (iexpression.Type == null)
			{
				return 0;
			}
			int nLowerBound = 0;
			int maxValue = int.MaxValue;
			if (iexpression.IsConstant(this.Scope, false))
			{
				num = this.CalculateForConstantAccess(ctypeDeRef, maxValue, num, iexpression, nLowerBound);
			}
			else
			{
				if (ctypeDeRef.Class != TypeClass.Pointer)
				{
					return 0;
				}
				num = this.CalculateForComplexAccess(indexAccessExpression, ctypeDeRef, ref aiBase, nLowerBound, maxValue);
			}
			return num;
		}

		// Token: 0x06001247 RID: 4679 RVA: 0x00033CFC File Offset: 0x00032CFC
		private int CalculateForComplexAccess(_IIndexAccessExpression indexAccessExpression, ICompiledType ctypeDeRef, ref IAddressInfo aiBase, int nLowerBound, int nUpperBound)
		{
			this._bVariableIndexAccess = true;
			this._bValid = (this._indexExpressions.Count > 0);
			int result = ctypeDeRef.BaseType.Size(this.Scope);
			this.Visitor.CreateIndexVarAddressInfo(indexAccessExpression, this._indexExpressions, out this._aiIndices, out this._aiBounds, ref this._bValid);
			if (this._bValid)
			{
				for (int i = 0; i < indexAccessExpression.NumAccesses; i++)
				{
					this._aiBounds[i] = new ArrayAccessBounds((long)nLowerBound, (long)nUpperBound);
				}
				IAddressInfo varrefelement = aiBase;
				IMyAddressInfo myAddressInfo = aiBase as IMyAddressInfo;
				if (myAddressInfo != null)
				{
					varrefelement = myAddressInfo.Duplicate();
				}
				aiBase = this.Generator.GenerateDeRefAccess(indexAccessExpression._Var, varrefelement, this._nOffset);
			}
			else
			{
				this.Generator.ClearDueToInvalidArrayIndex();
			}
			return result;
		}

		// Token: 0x06001248 RID: 4680 RVA: 0x00033DC4 File Offset: 0x00032DC4
		private int CalculateForConstantAccess(ICompiledType ctypeDeRef, int nUpperBound, int nSize, _IExpression expIndexAccess, int nLowerBound)
		{
			TypeClass @class = ctypeDeRef.Class;
			if (@class != TypeClass.String)
			{
				if (@class != TypeClass.WString)
				{
					if (@class == TypeClass.Pointer)
					{
						nSize = ctypeDeRef.BaseType.Size(this.Scope);
						this._bDeRef = true;
					}
				}
				else
				{
					WStringType wstringType = ctypeDeRef as WStringType;
					if (wstringType != null)
					{
						nUpperBound = wstringType.Size(this.Scope) - 1;
					}
					nSize = 2;
				}
			}
			else
			{
				StringType stringType = ctypeDeRef as StringType;
				if (stringType != null)
				{
					nUpperBound = stringType.Size(this.Scope) - 1;
				}
				nSize = 1;
			}
			ILiteralValue literalValue = expIndexAccess.Literal(this.Scope);
			int num;
			if (literalValue != null && literalValue.GetInt(out num) && num >= nLowerBound && num <= nUpperBound)
			{
				this._nOffset = (num - nLowerBound) * nSize;
				this._bValid = true;
			}
			return nSize;
		}

		// Token: 0x06001249 RID: 4681 RVA: 0x00033E7C File Offset: 0x00032E7C
		private ICompiledType GetDeRefType(_IIndexAccessExpression indexaccess, bool bIsVarLenArray, out bool bForceAddressSize)
		{
			ICompiledType result = indexaccess._Var.Type.DeRefType;
			ICompiledType baseType = indexaccess._Var.Type.BaseType;
			_IVariable ivariable = indexaccess._Var.GetVariable(this.Scope) as _IVariable;
			bForceAddressSize = false;
			if (!bIsVarLenArray && indexaccess._Var is IDeRefAccessExpression && ivariable != null && ivariable.GetFlag(VarFlag.Inout))
			{
				result = baseType;
				bForceAddressSize = true;
			}
			return result;
		}

		// Token: 0x0600124A RID: 4682 RVA: 0x00033EE8 File Offset: 0x00032EE8
		private int CalculateOffsetForArrayAccess(_IIndexAccessExpression indexaccess, _IArrayType arrtype, IList<_IArrayDimension> adim, ref int nBaseSize)
		{
			bool flag = true;
			for (int i = 0; i < this._indexExpressions.Count; i++)
			{
				if (!indexaccess.GetAccess(i).IsConstant(this.Scope, false))
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				this.CalculateConstantArrayAddressInfo(indexaccess, adim, ref nBaseSize);
			}
			else
			{
				this.CalculateComplexArrayAddressInfo(indexaccess, adim);
			}
			return arrtype.BaseType.Size(this.Scope);
		}

		// Token: 0x0600124B RID: 4683 RVA: 0x00033F50 File Offset: 0x00032F50
		private void CalculateComplexArrayAddressInfo(_IIndexAccessExpression indexaccess, IList<_IArrayDimension> adim)
		{
			this._bVariableIndexAccess = true;
			this._bValid = (this._indexExpressions.Count > 0);
			this.Visitor.CreateIndexVarAddressInfo(indexaccess, this._indexExpressions, out this._aiIndices, out this._aiBounds, ref this._bValid);
			if (this._bValid)
			{
				for (int i = 0; i < adim.Count; i++)
				{
					bool flag;
					int num = adim[i].LowerBorderInt(out flag, this.Scope);
					if (!flag)
					{
						this._bValid = false;
						return;
					}
					int num2 = adim[i].UpperBorderInt(out flag, this.Scope);
					if (!flag)
					{
						this._bValid = false;
						return;
					}
					this._aiBounds[i] = new ArrayAccessBounds((long)num, (long)num2);
				}
				return;
			}
			this.Generator.ClearDueToInvalidArrayIndex();
		}

		// Token: 0x0600124C RID: 4684 RVA: 0x00034014 File Offset: 0x00033014
		private void CalculateConstantArrayAddressInfo(_IIndexAccessExpression indexaccess, IList<_IArrayDimension> adim, ref int nBaseSize)
		{
			for (int i = adim.Count - 1; i >= 0; i--)
			{
				IExpression access = indexaccess.GetAccess(i);
				this._bValid = false;
				bool flag;
				int @int = access.Literal(this.Scope).GetInt(out flag);
				if (!flag)
				{
					break;
				}
				int num = adim[i].LowerBorderInt(out flag, this.Scope);
				if (!flag)
				{
					break;
				}
				int num2 = adim[i].Range(out flag, this.Scope);
				if (!flag)
				{
					break;
				}
				this._nOffset += (@int - num) * nBaseSize;
				nBaseSize *= num2;
				this._bValid = true;
			}
		}

		// Token: 0x0600124D RID: 4685 RVA: 0x000340AC File Offset: 0x000330AC
		private int CalculateOffsetForVectorAccess(_IIndexAccessExpression indexaccess, _IVectorType vectype)
		{
			int num = vectype.BaseType.Size(this.Scope);
			if (indexaccess.GetAccess(0).IsConstant(this.Scope, false))
			{
				bool flag;
				int @int = indexaccess.GetAccess(0).Literal(this.Scope).GetInt(out flag);
				if (flag)
				{
					this._nOffset = num * @int;
					this._bValid = true;
				}
			}
			else
			{
				this._bVariableIndexAccess = true;
				int num2 = vectype.DimensionInt(this.Scope, out this._bValid);
				if (this._bValid)
				{
					this.Visitor.CreateIndexVarAddressInfo(indexaccess, this._indexExpressions, out this._aiIndices, out this._aiBounds, ref this._bValid);
					if (this._bValid)
					{
						this._aiBounds[0] = new ArrayAccessBounds(0L, (long)(num2 - 1));
					}
					else
					{
						this.Generator.ClearDueToInvalidArrayIndex();
					}
				}
			}
			return vectype.BaseType.Size(this.Scope);
		}

		// Token: 0x0400044B RID: 1099
		private ICollection<_IExpression> _indexExpressions;

		// Token: 0x0400044C RID: 1100
		private IAddressInfo[] _aiIndices;

		// Token: 0x0400044D RID: 1101
		private IArrayBounds[] _aiBounds;

		// Token: 0x0400044E RID: 1102
		private bool _bValid;

		// Token: 0x0400044F RID: 1103
		private bool _bDeRef;

		// Token: 0x04000450 RID: 1104
		private bool _bVariableIndexAccess;

		// Token: 0x04000451 RID: 1105
		private bool _bForceAddressSize;

		// Token: 0x04000452 RID: 1106
		private int _nOffset;

		// Token: 0x04000453 RID: 1107
		private int _nSize;

		// Token: 0x04000458 RID: 1112
		private static _ILanguageModelBuilder2 s_lmbuilder;
	}
}
