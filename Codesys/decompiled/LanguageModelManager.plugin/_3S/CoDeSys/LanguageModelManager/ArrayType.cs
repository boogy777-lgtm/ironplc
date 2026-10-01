using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x0200019F RID: 415
	[TypeGuid("{1cdc270d-d608-42d9-b269-87457e40fd7d}")]
	[StorageVersion("3.3.0.0")]
	public class ArrayType : IECType, _IArrayType2, _IArrayType, _IType, ICompiledType5, ICompiledType4, ICompiledType3, ICompiledType2, ICompiledType, IType, IArchivable, IArrayType2, IArrayType, ITypeWithRecursiveTypeCheck, IHasEnumerableComponents
	{
		// Token: 0x06001DFA RID: 7674 RVA: 0x00052494 File Offset: 0x00051494
		public ArrayType()
		{
		}

		// Token: 0x06001DFB RID: 7675 RVA: 0x000524A7 File Offset: 0x000514A7
		public ArrayType(_IType typeBase)
		{
			this.m_typeBase = typeBase;
		}

		// Token: 0x170007DF RID: 2015
		// (get) Token: 0x06001DFC RID: 7676 RVA: 0x000524C1 File Offset: 0x000514C1
		public IType Base
		{
			get
			{
				if (this.m_typeBase != null)
				{
					return this.m_typeBase.EffectiveType;
				}
				return null;
			}
		}

		// Token: 0x170007E0 RID: 2016
		// (get) Token: 0x06001DFD RID: 7677 RVA: 0x000524D8 File Offset: 0x000514D8
		// (set) Token: 0x06001DFE RID: 7678 RVA: 0x000524F4 File Offset: 0x000514F4
		public _IType _Base
		{
			get
			{
				if (this.m_typeBase != null)
				{
					return this.m_typeBase.EffectiveType as _IType;
				}
				return null;
			}
			set
			{
				this.m_typeBase = value;
			}
		}

		// Token: 0x170007E1 RID: 2017
		// (get) Token: 0x06001DFF RID: 7679 RVA: 0x000524D8 File Offset: 0x000514D8
		public override ICompiledType BaseType
		{
			get
			{
				if (this.m_typeBase != null)
				{
					return this.m_typeBase.EffectiveType as _IType;
				}
				return null;
			}
		}

		// Token: 0x170007E2 RID: 2018
		// (get) Token: 0x06001E00 RID: 7680 RVA: 0x000524FD File Offset: 0x000514FD
		public _IType OriginalBaseType
		{
			get
			{
				return this.m_typeBase;
			}
		}

		// Token: 0x170007E3 RID: 2019
		// (get) Token: 0x06001E01 RID: 7681 RVA: 0x00052508 File Offset: 0x00051508
		public IArrayDimension[] Dimensions
		{
			get
			{
				_IArrayDimension[] array = new _IArrayDimension[this.m_arraydims.Count];
				this.m_arraydims.CopyTo(array);
				return array;
			}
		}

		// Token: 0x06001E02 RID: 7682 RVA: 0x00052538 File Offset: 0x00051538
		private bool IsEqualArrayDimensions(ArrayType arrtypein, IScope scope, IPrecompileScope scopeThis, IPrecompileScope scopeParameter, ref bool bConstantArrayLimitOnlyQualifiedChanged)
		{
			for (int i = 0; i < this.m_arraydims.Count; i++)
			{
				ArrayDim arrayDim = this.m_arraydims[i] as ArrayDim;
				ArrayDim arrayDim2 = arrtypein.m_arraydims[i] as ArrayDim;
				if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35600 && scope != null)
				{
					bool bAllocatedOK = false;
					if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35700)
					{
						bAllocatedOK = true;
					}
					ILiteralValue literalValue = arrayDim._LowerBorder.Literal(scope, bAllocatedOK);
					ILiteralValue literalValue2 = arrayDim._UpperBorder.Literal(scope, bAllocatedOK);
					ILiteralValue literalValue3 = arrayDim2._LowerBorder.Literal(scope, bAllocatedOK);
					ILiteralValue literalValue4 = arrayDim2._UpperBorder.Literal(scope, bAllocatedOK);
					if (literalValue == null || literalValue2 == null || literalValue3 == null || literalValue4 == null)
					{
						return false;
					}
					int num;
					int num2;
					int num3;
					int num4;
					if (!literalValue.GetInt(out num) || !literalValue2.GetInt(out num2) || !literalValue3.GetInt(out num3) || !literalValue4.GetInt(out num4))
					{
						return false;
					}
					if (num2 != num4 || num != num3)
					{
						return false;
					}
				}
				else
				{
					string text = arrayDim._LowerBorder.ToString();
					string text2 = arrayDim2._LowerBorder.ToString();
					string text3 = arrayDim._UpperBorder.ToString();
					string text4 = arrayDim2._UpperBorder.ToString();
					if (text.ToUpperInvariant() != text2.ToUpperInvariant() || text3.ToUpperInvariant() != text4.ToUpperInvariant())
					{
						if (scopeThis == null || scopeParameter == null || !APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351000)
						{
							return false;
						}
						bConstantArrayLimitOnlyQualifiedChanged = true;
						if (text.ToUpperInvariant() != text2.ToUpperInvariant())
						{
							IVariable variable = arrayDim._LowerBorder.GetVariable(scopeThis);
							IVariable variable2 = arrayDim2._LowerBorder.GetVariable(scopeParameter);
							if (!this.IsEqualConstantVariable(variable, variable2))
							{
								return false;
							}
						}
						if (text3.ToUpperInvariant() != text4.ToUpperInvariant())
						{
							IVariable variable3 = arrayDim._UpperBorder.GetVariable(scopeThis);
							IVariable variable4 = arrayDim2._UpperBorder.GetVariable(scopeParameter);
							if (!this.IsEqualConstantVariable(variable3, variable4))
							{
								return false;
							}
						}
					}
				}
			}
			return true;
		}

		// Token: 0x06001E03 RID: 7683 RVA: 0x0005274C File Offset: 0x0005174C
		public override bool IsEqual(ICompiledType type, IScope scope)
		{
			bool flag = false;
			return this.IsEqual(type, scope, true, null, null, ref flag);
		}

		// Token: 0x06001E04 RID: 7684 RVA: 0x00052768 File Offset: 0x00051768
		public bool IsEqual(ICompiledType itype, IScope scope, bool bCompiled, IPrecompileScope scopeThis, IPrecompileScope scopeParameter, ref bool bConstantArrayLimitOnlyQualifiedChanged)
		{
			ArrayType arrayType = itype as ArrayType;
			if (arrayType == null)
			{
				return false;
			}
			if (arrayType.m_arraydims.Count != this.m_arraydims.Count)
			{
				return false;
			}
			if (!this.IsEqualArrayDimensions(arrayType, scope, scopeThis, scopeParameter, ref bConstantArrayLimitOnlyQualifiedChanged))
			{
				return false;
			}
			if (!APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351400)
			{
				return this.m_typeBase.IsEqual(arrayType.m_typeBase);
			}
			if (bCompiled)
			{
				return this.m_typeBase.IsEqual(arrayType.m_typeBase);
			}
			return this.m_typeBase.IsEqualPreCompile(arrayType.m_typeBase, scope);
		}

		// Token: 0x06001E05 RID: 7685 RVA: 0x000527F8 File Offset: 0x000517F8
		private bool IsEqualConstantVariable(IVariable var1, IVariable var2)
		{
			return var1 != null && var2 != null && var1.HasFlag(VarFlag.Constant) && var2.HasFlag(VarFlag.Constant) && !(var1.OrgName != var2.OrgName) && var1.OriginalType == var2.OriginalType && var1.Initial == var2.Initial;
		}

		// Token: 0x06001E06 RID: 7686 RVA: 0x00052858 File Offset: 0x00051858
		public override bool IsEqualPreCompile(ICompiledType type, IScope scope)
		{
			bool flag = false;
			return this.IsEqual(type, scope, false, null, null, ref flag);
		}

		// Token: 0x170007E4 RID: 2020
		// (get) Token: 0x06001E07 RID: 7687 RVA: 0x00052874 File Offset: 0x00051874
		public IList<_IArrayDimension> _Dimensions
		{
			get
			{
				return this.m_arraydims.AsReadOnly();
			}
		}

		// Token: 0x06001E08 RID: 7688 RVA: 0x00052884 File Offset: 0x00051884
		public void AddDimension(_IExpression expLower, _IExpression expUpper)
		{
			ArrayDim arrayDim = new ArrayDim(expLower, expUpper);
			this.m_arraydims.Add(arrayDim);
		}

		// Token: 0x06001E09 RID: 7689 RVA: 0x000528A8 File Offset: 0x000518A8
		public override string ToString()
		{
			if (this.m_typeBase == null || this.m_arraydims.Count == 0)
			{
				return "ERROR";
			}
			string text = "ARRAY [";
			bool flag = true;
			foreach (_IArrayDimension iarrayDimension in this._Dimensions)
			{
				if (!flag)
				{
					text += ", ";
				}
				if (iarrayDimension.LowerBorder == null)
				{
					text += "null";
				}
				else
				{
					string str = text;
					IExpression lowerBorder = iarrayDimension.LowerBorder;
					text = str + ((lowerBorder != null) ? lowerBorder.ToString() : null);
				}
				text += "..";
				if (iarrayDimension.UpperBorder == null)
				{
					text += "null";
				}
				else
				{
					string str2 = text;
					IExpression upperBorder = iarrayDimension.UpperBorder;
					text = str2 + ((upperBorder != null) ? upperBorder.ToString() : null);
				}
				flag = false;
			}
			string str3 = text;
			string str4 = "] OF ";
			_IType typeBase = this.m_typeBase;
			text = str3 + str4 + ((typeBase != null) ? typeBase.ToString() : null);
			return text;
		}

		// Token: 0x06001E0A RID: 7690 RVA: 0x00051202 File Offset: 0x00050202
		public override string ToUpperString()
		{
			return this.ToString().ToUpperInvariant();
		}

		// Token: 0x06001E0B RID: 7691 RVA: 0x000529B4 File Offset: 0x000519B4
		public override string GetConstantString(IScope scope)
		{
			if (this.m_typeBase == null || this.m_arraydims.Count == 0)
			{
				return "ERROR";
			}
			string text = "ARRAY [";
			bool flag = true;
			foreach (_IArrayDimension iarrayDimension in this._Dimensions)
			{
				if (!flag)
				{
					text += ", ";
				}
				if (iarrayDimension.LowerBorder == null)
				{
					text += "null";
				}
				else
				{
					bool flag2;
					text += TypeHelper.GetInt(iarrayDimension.LowerBorder, scope as IScope5, out flag2).ToString();
				}
				text += "..";
				if (iarrayDimension.UpperBorder == null)
				{
					text += "null";
				}
				else
				{
					bool flag3;
					text += TypeHelper.GetInt(iarrayDimension.UpperBorder, scope as IScope5, out flag3).ToString();
				}
				flag = false;
			}
			text = text + "] OF " + this.m_typeBase.GetConstantString(scope);
			return text;
		}

		// Token: 0x170007E5 RID: 2021
		// (get) Token: 0x06001E0C RID: 7692 RVA: 0x00052ACC File Offset: 0x00051ACC
		public override TypeClass Class
		{
			get
			{
				return TypeClass.Array;
			}
		}

		// Token: 0x06001E0D RID: 7693 RVA: 0x00052AD0 File Offset: 0x00051AD0
		public override void Accept(ITypeVisitor typvis)
		{
			typvis.visit(this);
		}

		// Token: 0x06001E0E RID: 7694 RVA: 0x00052ADC File Offset: 0x00051ADC
		public override _IType _Duplicate(bool bDeep)
		{
			ArrayType arrayType = new ArrayType();
			arrayType._Base = this.m_typeBase._Duplicate(bDeep);
			foreach (_IArrayDimension iarrayDimension in this.m_arraydims)
			{
				arrayType.AddDimension(iarrayDimension._LowerBorder.Duplicate() as _IExpression, iarrayDimension._UpperBorder.Duplicate() as _IExpression);
			}
			return arrayType;
		}

		// Token: 0x06001E0F RID: 7695 RVA: 0x00052B64 File Offset: 0x00051B64
		public int SizeWithRecursionCheck(IScope scope, IRecursionGuard recursionGuard, out bool bValid)
		{
			checked
			{
				int result;
				try
				{
					int num = 1;
					if (recursionGuard != null)
					{
						recursionGuard = recursionGuard.Duplicate();
						if (recursionGuard.Has(this))
						{
							bValid = false;
							return -1;
						}
						recursionGuard.Add(this);
					}
					foreach (_IArrayDimension iarrayDimension in this.m_arraydims)
					{
						num *= (iarrayDimension as ArrayDim).Range(out bValid, scope as IScope5, recursionGuard);
						if (!bValid)
						{
							return 0;
						}
					}
					result = num * (this.m_typeBase.EffectiveType as _IType).SizeChecked(scope, out bValid);
				}
				catch (OverflowException)
				{
					bValid = false;
					result = int.MinValue;
				}
				return result;
			}
		}

		// Token: 0x06001E10 RID: 7696 RVA: 0x00052C24 File Offset: 0x00051C24
		public override int SizeChecked(IScope scope, out bool bValid)
		{
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351400)
			{
				return this.SizeWithRecursionCheck(scope, null, out bValid);
			}
			int num = 1;
			foreach (_IArrayDimension iarrayDimension in this.m_arraydims)
			{
				num *= iarrayDimension.Range(out bValid, scope as IScope5);
				if (!bValid)
				{
					return 0;
				}
			}
			return num * (this.m_typeBase.EffectiveType as _IType).SizeChecked(scope, out bValid);
		}

		// Token: 0x06001E11 RID: 7697 RVA: 0x00052CBC File Offset: 0x00051CBC
		public override int Size(IScope scope)
		{
			int num = 1;
			foreach (_IArrayDimension iarrayDimension in this.m_arraydims)
			{
				bool flag;
				num *= iarrayDimension.Range(out flag, scope as IScope5);
				if (!flag)
				{
					return 0;
				}
			}
			return this.CheckedSize(scope, num);
		}

		// Token: 0x06001E12 RID: 7698 RVA: 0x00052D28 File Offset: 0x00051D28
		private int CheckedSize(IScope scope, int iRange)
		{
			checked
			{
				if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351500)
				{
					try
					{
						if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV352010)
						{
							return iRange * this.m_typeBase.EffectiveType.Size(scope);
						}
						return unchecked(iRange * this.m_typeBase.EffectiveType.Size(scope));
					}
					catch (OverflowException)
					{
						throw new OverflowException();
					}
					catch
					{
						if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV352010)
						{
							return -1;
						}
						return iRange * this.m_typeBase.EffectiveType.Size(scope);
					}
				}
				if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV3300)
				{
					return iRange * this.m_typeBase.EffectiveType.Size(scope);
				}
			}
			return iRange * this.m_typeBase.EffectiveType.Size(scope);
		}

		// Token: 0x06001E13 RID: 7699 RVA: 0x00052E10 File Offset: 0x00051E10
		public override bool CanConvertRaw(byte[] raw, ByteOrder byteOrder, IScope5 scope)
		{
			if (raw.Length != this.Size(scope))
			{
				return false;
			}
			try
			{
				this.ConvertRaw(raw, byteOrder, scope);
			}
			catch
			{
				return false;
			}
			return true;
		}

		// Token: 0x06001E14 RID: 7700 RVA: 0x00052E50 File Offset: 0x00051E50
		public override object ConvertRaw(byte[] raw, ByteOrder byteOrder, IScope5 scope)
		{
			int num = 1;
			foreach (_IArrayDimension iarrayDimension in this.m_arraydims)
			{
				bool flag;
				num *= iarrayDimension.Range(out flag, scope);
				if (!flag)
				{
					return null;
				}
			}
			object[] array = new object[num];
			int num2 = this.m_typeBase.EffectiveType.Size(scope);
			byte[] array2 = new byte[num2];
			for (int i = 0; i < array.Length; i++)
			{
				Array.Copy(raw, i * num2, array2, 0, num2);
				array[i] = this._Base.ConvertRaw(array2, byteOrder, scope);
			}
			return array;
		}

		// Token: 0x06001E15 RID: 7701 RVA: 0x00052F0C File Offset: 0x00051F0C
		public override bool CanConvertToRaw(object value, ByteOrder byteOrder, IScope5 scope)
		{
			try
			{
				if (this.ConvertToRaw(value, byteOrder, scope) == null)
				{
					return false;
				}
			}
			catch
			{
				return false;
			}
			return true;
		}

		// Token: 0x06001E16 RID: 7702 RVA: 0x00052F44 File Offset: 0x00051F44
		public override byte[] ConvertToRaw(object value, ByteOrder byteOrder, IScope5 scope)
		{
			byte[] array = new byte[this.Size(scope)];
			try
			{
				object[] array2 = (object[])value;
				int num = this.m_typeBase.EffectiveType.Size(scope);
				for (int i = 0; i < array2.Length; i++)
				{
					byte[] array3 = this._Base.ConvertToRaw(array2[i], byteOrder, scope);
					Array.Copy(array3, 0, array, i * num, (array3.Length < num) ? array3.Length : num);
				}
			}
			catch
			{
				return null;
			}
			return array;
		}

		// Token: 0x06001E17 RID: 7703 RVA: 0x00052FD4 File Offset: 0x00051FD4
		public override int GetNumOfElements(IScope5 scope)
		{
			bool flag;
			return this.GetNumOfElements(scope, out flag);
		}

		// Token: 0x06001E18 RID: 7704 RVA: 0x00052FEC File Offset: 0x00051FEC
		public int GetNumOfElements(IScope5 scope, out bool bValid)
		{
			int num = 1;
			bValid = false;
			foreach (_IArrayDimension iarrayDimension in this.m_arraydims)
			{
				num = iarrayDimension.Range(out bValid, scope) * num;
				if (!bValid)
				{
					break;
				}
			}
			if (!bValid)
			{
				return 0;
			}
			return num;
		}

		// Token: 0x06001E19 RID: 7705 RVA: 0x00053050 File Offset: 0x00052050
		public override ICompiledType GetComponent(int i, IScope5 scope)
		{
			if (i < 0 || i >= this.GetNumOfElements(scope))
			{
				throw new ArgumentOutOfRangeException("i");
			}
			return this.BaseType;
		}

		// Token: 0x06001E1A RID: 7706 RVA: 0x00053074 File Offset: 0x00052074
		IEnumerable<string> IHasEnumerableComponents.GetComponents(IScope scope, out bool bValid)
		{
			bValid = true;
			if (this.m_arraydims.Count == 0)
			{
				return Enumerable.Empty<string>();
			}
			int[] array = new int[this.m_arraydims.Count];
			int[] array2 = new int[this.m_arraydims.Count];
			for (int i = 0; i < this.m_arraydims.Count; i++)
			{
				ArrayDim arrayDim = this.m_arraydims[i] as ArrayDim;
				int num = arrayDim.LowerBorderInt(out bValid, scope);
				if (!bValid)
				{
					return Enumerable.Empty<string>();
				}
				int num2 = arrayDim.UpperBorderInt(out bValid, scope);
				if (!bValid)
				{
					return Enumerable.Empty<string>();
				}
				array[i] = num;
				array2[i] = num2;
			}
			return new ArrayType.ComponentEnumerable(array, array2);
		}

		// Token: 0x06001E1B RID: 7707 RVA: 0x0005311C File Offset: 0x0005211C
		public override string[] GetComponents(IScope5 scope, out bool bValid)
		{
			bValid = true;
			if (this.m_arraydims.Count == 0)
			{
				return new string[0];
			}
			int[] array = new int[this.m_arraydims.Count];
			int[] array2 = new int[this.m_arraydims.Count];
			int[] array3 = new int[this.m_arraydims.Count];
			for (int i = 0; i < this.m_arraydims.Count; i++)
			{
				ArrayDim arrayDim = this.m_arraydims[i] as ArrayDim;
				int num = arrayDim.LowerBorderInt(out bValid, scope);
				if (!bValid)
				{
					return new string[0];
				}
				int num2 = arrayDim.UpperBorderInt(out bValid, scope);
				if (!bValid)
				{
					return new string[0];
				}
				array2[i] = num;
				array[i] = num;
				array3[i] = num2;
			}
			ArrayList arrayList = new ArrayList();
			int num3 = this.GetNumOfElements(scope);
			bool flag = num3 <= 0;
			num3 = 0;
			LStringBuilder lstringBuilder = new LStringBuilder();
			while (!flag)
			{
				lstringBuilder.Clear();
				lstringBuilder.Append("[");
				for (int j = 0; j < array.Length; j++)
				{
					lstringBuilder.Append(array[j]);
					if (j < array.Length - 1)
					{
						lstringBuilder.Append(", ");
					}
				}
				lstringBuilder.Append("]");
				arrayList.Add(lstringBuilder.ToString());
				flag = true;
				for (int k = array.Length - 1; k >= 0; k--)
				{
					if (array[k] < array3[k])
					{
						array[k]++;
						for (int l = array.Length - 1; l > k; l--)
						{
							array[l] = array2[l];
						}
						flag = false;
						break;
					}
				}
				if (num3 > MemorySentinel.MAX_MONITORING_ELEMENTS && num3 % MemorySentinel.MAX_MONITORING_ELEMENTS == 0 && !MemorySentinel.CheckMinimumFreeMemory(150))
				{
					bValid = false;
					return new string[0];
				}
				num3++;
			}
			Debug.Assert(arrayList.Count == this.GetNumOfElements(scope));
			string[] array4 = new string[arrayList.Count];
			arrayList.CopyTo(array4);
			return array4;
		}

		// Token: 0x06001E1C RID: 7708 RVA: 0x0005331C File Offset: 0x0005231C
		public int[] ToDimensionIndexes(int elementIndex, IScope5 scope, out bool bValid)
		{
			int[] result = new int[0];
			int numOfElements = this.GetNumOfElements(scope, out bValid);
			bValid = (bValid && elementIndex >= 0 && elementIndex < numOfElements);
			if (!bValid)
			{
				return result;
			}
			int num = this.Dimensions.Count<IArrayDimension>();
			int[] array = new int[num];
			int num2 = elementIndex;
			for (int i = num - 1; i >= 0; i--)
			{
				IArrayDimension arrayDimension = this.Dimensions[i];
				int num3 = arrayDimension.Range(out bValid, scope);
				if (!bValid)
				{
					return result;
				}
				int num4 = num2 % num3;
				array[i] = num4 + arrayDimension.LowerBorderInt(out bValid, scope);
				if (!bValid)
				{
					return result;
				}
				num2 = (num2 - num4) / num3;
			}
			return array;
		}

		// Token: 0x040005F7 RID: 1527
		[DefaultSerialization("Dimensions")]
		[StorageVersion("3.3.0.0")]
		[StorageSaveAsNonGenericCollection("3.3.0.0-3.5.6.255")]
		[Obfuscation(Feature = "rename")]
		private LList<_IArrayDimension> m_arraydims = new LList<_IArrayDimension>();

		// Token: 0x040005F8 RID: 1528
		[DefaultSerialization("BaseType")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private _IType m_typeBase;

		// Token: 0x020002BB RID: 699
		private sealed class ComponentEnumerable : IEnumerable<string>, IEnumerable
		{
			// Token: 0x06002C08 RID: 11272 RVA: 0x00074597 File Offset: 0x00073597
			public ComponentEnumerable(int[] indicesLow, int[] indicesHigh)
			{
				this.nIndicesLow = indicesLow;
				this.nIndicesHigh = indicesHigh;
			}

			// Token: 0x06002C09 RID: 11273 RVA: 0x000745AD File Offset: 0x000735AD
			public IEnumerator<string> GetEnumerator()
			{
				return new ArrayType.ComponentEnumerable.ComponentEnumerator(this);
			}

			// Token: 0x06002C0A RID: 11274 RVA: 0x000745B5 File Offset: 0x000735B5
			IEnumerator IEnumerable.GetEnumerator()
			{
				return this.GetEnumerator();
			}

			// Token: 0x040008D1 RID: 2257
			private readonly int[] nIndicesLow;

			// Token: 0x040008D2 RID: 2258
			private readonly int[] nIndicesHigh;

			// Token: 0x020002E6 RID: 742
			private sealed class ComponentEnumerator : IEnumerator<string>, IDisposable, IEnumerator
			{
				// Token: 0x06002CB5 RID: 11445 RVA: 0x000752BA File Offset: 0x000742BA
				public ComponentEnumerator(ArrayType.ComponentEnumerable componentInfo)
				{
					this.ComponentInfo = componentInfo;
					this.Indices = new int[componentInfo.nIndicesLow.Length];
					this.Reset();
				}

				// Token: 0x17000C3B RID: 3131
				// (get) Token: 0x06002CB6 RID: 11446 RVA: 0x000752E4 File Offset: 0x000742E4
				public string Current
				{
					get
					{
						if (this.StringBuilder == null)
						{
							this.StringBuilder = new LStringBuilder();
						}
						this.StringBuilder.Clear();
						this.StringBuilder.Append("[");
						for (int i = 0; i < this.Indices.Length; i++)
						{
							this.StringBuilder.Append(this.Indices[i]);
							if (i < this.Indices.Length - 1)
							{
								this.StringBuilder.Append(", ");
							}
						}
						this.StringBuilder.Append("]");
						return this.StringBuilder.ToString();
					}
				}

				// Token: 0x17000C3C RID: 3132
				// (get) Token: 0x06002CB7 RID: 11447 RVA: 0x00075382 File Offset: 0x00074382
				object IEnumerator.Current
				{
					get
					{
						return this.Current;
					}
				}

				// Token: 0x06002CB8 RID: 11448 RVA: 0x00003AE9 File Offset: 0x00002AE9
				public void Dispose()
				{
				}

				// Token: 0x06002CB9 RID: 11449 RVA: 0x0007538C File Offset: 0x0007438C
				public bool MoveNext()
				{
					for (int i = this.Indices.Length - 1; i >= 0; i--)
					{
						if (this.Indices[i] < this.ComponentInfo.nIndicesHigh[i])
						{
							this.Indices[i]++;
							for (int j = this.Indices.Length - 1; j > i; j--)
							{
								this.Indices[j] = this.ComponentInfo.nIndicesLow[j];
							}
							return true;
						}
					}
					return false;
				}

				// Token: 0x06002CBA RID: 11450 RVA: 0x00075403 File Offset: 0x00074403
				public void Reset()
				{
					this.ComponentInfo.nIndicesLow.CopyTo(this.Indices, 0);
					this.Indices[this.Indices.Length - 1]--;
				}

				// Token: 0x0400092B RID: 2347
				private readonly int[] Indices;

				// Token: 0x0400092C RID: 2348
				private readonly ArrayType.ComponentEnumerable ComponentInfo;

				// Token: 0x0400092D RID: 2349
				private LStringBuilder StringBuilder;
			}
		}
	}
}
