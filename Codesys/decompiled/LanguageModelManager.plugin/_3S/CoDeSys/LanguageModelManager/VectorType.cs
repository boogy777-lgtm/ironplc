using System;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x020001A0 RID: 416
	[TypeGuid("{CA4A85AB-E505-4BEE-B42B-62154E661C46}")]
	[StorageVersion("3.5.14.0")]
	public class VectorType : IECType, _IVectorType, _IType, ICompiledType5, ICompiledType4, ICompiledType3, ICompiledType2, ICompiledType, IType, IArchivable, IVectorType, ITypeWithRecursiveTypeCheck
	{
		// Token: 0x06001E1D RID: 7709 RVA: 0x0004EC38 File Offset: 0x0004DC38
		public VectorType()
		{
		}

		// Token: 0x06001E1E RID: 7710 RVA: 0x000533B6 File Offset: 0x000523B6
		public VectorType(_IType typeBase, _IExpression dimension)
		{
			this.m_typeBase = typeBase;
			this.m_Dimension = dimension;
		}

		// Token: 0x170007E6 RID: 2022
		// (get) Token: 0x06001E1F RID: 7711 RVA: 0x000533CC File Offset: 0x000523CC
		public IExpression Dimension
		{
			get
			{
				return this.m_Dimension;
			}
		}

		// Token: 0x170007E7 RID: 2023
		// (get) Token: 0x06001E20 RID: 7712 RVA: 0x000533CC File Offset: 0x000523CC
		// (set) Token: 0x06001E21 RID: 7713 RVA: 0x000533D4 File Offset: 0x000523D4
		public _IExpression _Dimension
		{
			get
			{
				return this.m_Dimension;
			}
			set
			{
				this.m_Dimension = value;
			}
		}

		// Token: 0x170007E8 RID: 2024
		// (get) Token: 0x06001E22 RID: 7714 RVA: 0x000533DD File Offset: 0x000523DD
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

		// Token: 0x170007E9 RID: 2025
		// (get) Token: 0x06001E23 RID: 7715 RVA: 0x000533F4 File Offset: 0x000523F4
		// (set) Token: 0x06001E24 RID: 7716 RVA: 0x00053410 File Offset: 0x00052410
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

		// Token: 0x170007EA RID: 2026
		// (get) Token: 0x06001E25 RID: 7717 RVA: 0x000533F4 File Offset: 0x000523F4
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

		// Token: 0x170007EB RID: 2027
		// (get) Token: 0x06001E26 RID: 7718 RVA: 0x00053419 File Offset: 0x00052419
		public _IType OriginalBaseType
		{
			get
			{
				return this.m_typeBase;
			}
		}

		// Token: 0x06001E27 RID: 7719 RVA: 0x00053424 File Offset: 0x00052424
		private bool IsEqualConstantVariable(IVariable var1, IVariable var2)
		{
			return var1 != null && var2 != null && var1.HasFlag(VarFlag.Constant) && var2.HasFlag(VarFlag.Constant) && !(var1.OrgName != var2.OrgName) && var1.OriginalType == var2.OriginalType && var1.Initial == var2.Initial;
		}

		// Token: 0x06001E28 RID: 7720 RVA: 0x0005054F File Offset: 0x0004F54F
		public override bool IsEqualPreCompile(ICompiledType type, IScope scope)
		{
			return this.IsEqual(type, scope);
		}

		// Token: 0x06001E29 RID: 7721 RVA: 0x00053481 File Offset: 0x00052481
		public static int GetVectorBlockSize(IScope scope)
		{
			return ((_ICompileContext)((_IScope)scope).ApplicationContext).VectorBlockSize;
		}

		// Token: 0x06001E2A RID: 7722 RVA: 0x00053498 File Offset: 0x00052498
		public override bool IsEqual(ICompiledType type, IScope scope)
		{
			return this.IsEqual(type, scope, null, null);
		}

		// Token: 0x06001E2B RID: 7723 RVA: 0x000534A4 File Offset: 0x000524A4
		public bool IsEqual(ICompiledType itype, IScope scope, IPrecompileScope scopeThis, IPrecompileScope scopeParameter)
		{
			VectorType vectorType = itype as VectorType;
			if (vectorType == null)
			{
				return false;
			}
			if (!this.BaseType.IsEqual(vectorType.BaseType))
			{
				return false;
			}
			_IExpression dimension = this.m_Dimension;
			_IExpression dimension2 = vectorType.m_Dimension;
			if (scope != null)
			{
				bool bAllocatedOK = true;
				ILiteralValue literalValue = dimension.Literal(scope, bAllocatedOK);
				ILiteralValue literalValue2 = dimension2.Literal(scope, bAllocatedOK);
				if (literalValue == null || literalValue2 == null)
				{
					return false;
				}
				int num;
				int num2;
				if (!literalValue.GetInt(out num) || !literalValue2.GetInt(out num2))
				{
					return false;
				}
				if (num != num2)
				{
					return false;
				}
			}
			else
			{
				string text = dimension.ToString().ToUpperInvariant();
				string value = dimension2.ToString().ToUpperInvariant();
				if (!text.Equals(value))
				{
					return false;
				}
				if (scopeThis == null || scopeParameter == null)
				{
					return false;
				}
				IVariable variable = dimension.GetVariable(scopeThis);
				IVariable variable2 = dimension2.GetVariable(scopeParameter);
				if (!this.IsEqualConstantVariable(variable, variable2))
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x06001E2C RID: 7724 RVA: 0x00053571 File Offset: 0x00052571
		public override string ToString()
		{
			if (this.OriginalBaseType == null)
			{
				return "ERROR";
			}
			string str = "__VECTOR[";
			string str2 = this.m_Dimension.ToString();
			string str3 = "] OF ";
			_IType originalBaseType = this.OriginalBaseType;
			return str + str2 + str3 + ((originalBaseType != null) ? originalBaseType.ToString() : null);
		}

		// Token: 0x06001E2D RID: 7725 RVA: 0x000535B0 File Offset: 0x000525B0
		public override string GetConstantString(IScope scope)
		{
			if (this.OriginalBaseType == null)
			{
				return "ERROR";
			}
			bool flag;
			return "__VECTOR[" + TypeHelper.GetInt(this.m_Dimension, scope as IScope5, out flag).ToString() + "] OF " + this.OriginalBaseType.GetConstantString(scope);
		}

		// Token: 0x170007EC RID: 2028
		// (get) Token: 0x06001E2E RID: 7726 RVA: 0x00053606 File Offset: 0x00052606
		public override TypeClass Class
		{
			get
			{
				return TypeClass.__Vector;
			}
		}

		// Token: 0x06001E2F RID: 7727 RVA: 0x0005360A File Offset: 0x0005260A
		public override void Accept(ITypeVisitor typvis)
		{
			typvis.visit(this);
		}

		// Token: 0x06001E30 RID: 7728 RVA: 0x00053613 File Offset: 0x00052613
		public override _IType _Duplicate(bool bDeep)
		{
			return new VectorType(this.m_typeBase._Duplicate(bDeep), this.m_Dimension.Duplicate() as _IExpression);
		}

		// Token: 0x06001E31 RID: 7729 RVA: 0x00053638 File Offset: 0x00052638
		public override int SizeChecked(IScope scope, out bool bValid)
		{
			int num = this.DimensionInt(scope, null, out bValid);
			if (!bValid)
			{
				return 0;
			}
			int vectorBlockSize = VectorType.GetVectorBlockSize(scope);
			return checked((num * (this.BaseType as _IType).SizeChecked(scope, out bValid) + vectorBlockSize - 1) / vectorBlockSize * vectorBlockSize);
		}

		// Token: 0x06001E32 RID: 7730 RVA: 0x00053678 File Offset: 0x00052678
		public int SizeWithRecursionCheck(IScope scope, IRecursionGuard recursionGuard, out bool bValid)
		{
			if (recursionGuard.Has(this))
			{
				bValid = false;
				return 0;
			}
			int num = this.DimensionInt(scope, recursionGuard, out bValid);
			if (!bValid)
			{
				return 0;
			}
			int vectorBlockSize = VectorType.GetVectorBlockSize(scope);
			return checked((num * (this.BaseType as _IType).SizeChecked(scope, out bValid) + vectorBlockSize - 1) / vectorBlockSize * vectorBlockSize);
		}

		// Token: 0x06001E33 RID: 7731 RVA: 0x000536C6 File Offset: 0x000526C6
		public int DimensionInt(IScope scope, out bool bValid)
		{
			return this.DimensionInt(scope, null, out bValid);
		}

		// Token: 0x06001E34 RID: 7732 RVA: 0x000536D4 File Offset: 0x000526D4
		public int DimensionInt(IScope scope, IRecursionGuard recursionGuard, out bool bValid)
		{
			if (recursionGuard != null)
			{
				recursionGuard = recursionGuard.Duplicate();
				if (recursionGuard.Has(this))
				{
					bValid = false;
					return 0;
				}
				recursionGuard.Add(this);
			}
			int @int = TypeHelper.GetInt(this.m_Dimension, scope as IScope5, true, recursionGuard, out bValid);
			if (!bValid)
			{
				return 0;
			}
			return @int;
		}

		// Token: 0x06001E35 RID: 7733 RVA: 0x00053720 File Offset: 0x00052720
		public override int Size(IScope scope)
		{
			bool flag;
			int num = this.DimensionInt(scope, null, out flag);
			if (!flag)
			{
				return 0;
			}
			int vectorBlockSize = VectorType.GetVectorBlockSize(scope);
			return checked((num * this.BaseType.Size(scope) + vectorBlockSize - 1) / vectorBlockSize * vectorBlockSize);
		}

		// Token: 0x06001E36 RID: 7734 RVA: 0x0005375C File Offset: 0x0005275C
		public override int GetNumOfElements(IScope5 scope)
		{
			bool flag;
			return this.DimensionInt(scope, null, out flag);
		}

		// Token: 0x06001E37 RID: 7735 RVA: 0x00053050 File Offset: 0x00052050
		public override ICompiledType GetComponent(int i, IScope5 scope)
		{
			if (i < 0 || i >= this.GetNumOfElements(scope))
			{
				throw new ArgumentOutOfRangeException("i");
			}
			return this.BaseType;
		}

		// Token: 0x06001E38 RID: 7736 RVA: 0x00053774 File Offset: 0x00052774
		public override string[] GetComponents(IScope5 scope, out bool bValid)
		{
			int numOfElements = this.GetNumOfElements(scope);
			if (numOfElements <= 0)
			{
				bValid = false;
				return new string[0];
			}
			bValid = true;
			string[] array = new string[numOfElements];
			for (int i = 0; i < numOfElements; i++)
			{
				array[i] = string.Format("[{0}]", i);
			}
			return array;
		}

		// Token: 0x06001E39 RID: 7737 RVA: 0x000537C4 File Offset: 0x000527C4
		public override bool CanConvertRaw(byte[] raw, ByteOrder byteOrder, IScope5 scope)
		{
			if (raw.Length != this.Size(scope))
			{
				return false;
			}
			bool result;
			this.DimensionInt(scope, null, out result);
			return result;
		}

		// Token: 0x06001E3A RID: 7738 RVA: 0x00004E6B File Offset: 0x00003E6B
		public override bool CanConvertToRaw(object value, ByteOrder byteOrder, IScope5 scope)
		{
			return false;
		}

		// Token: 0x06001E3B RID: 7739 RVA: 0x000537F0 File Offset: 0x000527F0
		public override object ConvertRaw(byte[] raw, ByteOrder byteOrder, IScope5 scope)
		{
			bool flag;
			int num = this.DimensionInt(scope, null, out flag);
			if (!flag)
			{
				return null;
			}
			object[] array = new object[num];
			int num2 = this.BaseType.Size(scope);
			byte[] array2 = new byte[num2];
			for (int i = 0; i < array.Length; i++)
			{
				Array.Copy(raw, i * num2, array2, 0, num2);
				array[i] = this._Base.ConvertRaw(array2, byteOrder, scope);
			}
			return array;
		}

		// Token: 0x06001E3C RID: 7740 RVA: 0x0000677E File Offset: 0x0000577E
		public override byte[] ConvertToRaw(object value, ByteOrder byteOrder, IScope5 scope)
		{
			throw new NotImplementedException();
		}

		// Token: 0x040005F9 RID: 1529
		[DefaultSerialization("Dimension")]
		[StorageVersion("3.5.14.0")]
		[StorageDefaultValue(null)]
		[Obfuscation(Feature = "rename")]
		private _IExpression m_Dimension;

		// Token: 0x040005FA RID: 1530
		[DefaultSerialization("BaseType")]
		[StorageVersion("3.5.14.0")]
		[StorageDefaultValue(null)]
		[Obfuscation(Feature = "rename")]
		private _IType m_typeBase;
	}
}
