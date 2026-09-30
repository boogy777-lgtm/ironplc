using System;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x0200015B RID: 347
	[TypeGuid("{22616e05-4c32-44fe-abcd-34b4797489c6}")]
	[StorageVersion("3.3.0.0")]
	public abstract class IECType : GenericObject2, _IType, ICompiledType5, ICompiledType4, ICompiledType3, ICompiledType2, ICompiledType, IType, IArchivable
	{
		// Token: 0x17000780 RID: 1920
		// (get) Token: 0x06001BEE RID: 7150 RVA: 0x0004E9B0 File Offset: 0x0004D9B0
		// (set) Token: 0x06001BEF RID: 7151 RVA: 0x0004E9B8 File Offset: 0x0004D9B8
		public bool IsCompiled
		{
			get
			{
				return this.m_bCompiled;
			}
			set
			{
				this.m_bCompiled = value;
			}
		}

		// Token: 0x06001BF0 RID: 7152
		public abstract override string ToString();

		// Token: 0x06001BF1 RID: 7153 RVA: 0x0004E9C1 File Offset: 0x0004D9C1
		public virtual string ToUpperString()
		{
			return this.ToString();
		}

		// Token: 0x06001BF2 RID: 7154 RVA: 0x0004E9C1 File Offset: 0x0004D9C1
		public virtual string GetConstantString(IScope scope)
		{
			return this.ToString();
		}

		// Token: 0x06001BF3 RID: 7155 RVA: 0x0004E9C9 File Offset: 0x0004D9C9
		public virtual _IType _Duplicate(bool bDeep)
		{
			return this;
		}

		// Token: 0x17000781 RID: 1921
		// (get) Token: 0x06001BF4 RID: 7156 RVA: 0x0004E9CC File Offset: 0x0004D9CC
		public _IType Duplicate
		{
			get
			{
				return this._Duplicate(false);
			}
		}

		// Token: 0x17000782 RID: 1922
		// (get) Token: 0x06001BF5 RID: 7157 RVA: 0x0004E9C9 File Offset: 0x0004D9C9
		public virtual ICompiledType BaseType
		{
			get
			{
				return this;
			}
		}

		// Token: 0x17000783 RID: 1923
		// (get) Token: 0x06001BF6 RID: 7158 RVA: 0x0004E9C9 File Offset: 0x0004D9C9
		public virtual ICompiledType DeRefType
		{
			get
			{
				return this;
			}
		}

		// Token: 0x06001BF7 RID: 7159 RVA: 0x00005F0F File Offset: 0x00004F0F
		public virtual ICompiledType GetComponent(int i, IScope5 scope)
		{
			return null;
		}

		// Token: 0x06001BF8 RID: 7160 RVA: 0x0004E9D5 File Offset: 0x0004D9D5
		public virtual string[] GetComponents(IScope5 scope, out bool bValid)
		{
			bValid = true;
			return new string[0];
		}

		// Token: 0x06001BF9 RID: 7161 RVA: 0x0004E9E0 File Offset: 0x0004D9E0
		public virtual bool IsCompatible(ICompiledType type, IScope2 scope)
		{
			return TypeComparerProxy.IsImplicitConvertable(this, type, scope as ICommonScope);
		}

		// Token: 0x06001BFA RID: 7162 RVA: 0x0004E9EF File Offset: 0x0004D9EF
		[Obsolete("Use IsCompatible(ICompiledType type, IScope scope) instead.")]
		public virtual bool IsCompatible(ICompiledType type)
		{
			return TypeComparerProxy.IsImplicitConvertable(this, type, null);
		}

		// Token: 0x06001BFB RID: 7163
		public abstract void Accept(ITypeVisitor typvis);

		// Token: 0x17000784 RID: 1924
		// (get) Token: 0x06001BFC RID: 7164 RVA: 0x0004E9F9 File Offset: 0x0004D9F9
		public virtual bool IsInteger
		{
			get
			{
				if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV34200)
				{
					return TypeTable.IsInteger(this.Class);
				}
				return TypeTable.IsNumber(this.Class);
			}
		}

		// Token: 0x06001BFD RID: 7165 RVA: 0x00004E6B File Offset: 0x00003E6B
		public virtual int GetNumOfElements(IScope5 scope)
		{
			return 0;
		}

		// Token: 0x06001BFE RID: 7166 RVA: 0x0004EA24 File Offset: 0x0004DA24
		public virtual int SizeChecked(IScope scope, out bool bValid)
		{
			bValid = true;
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV34000)
			{
				ISpecialSizeType specialSizeType = this as ISpecialSizeType;
				if (specialSizeType != null)
				{
					return TypeTable.GetSize(specialSizeType.CodegeneratorType.Class, scope);
				}
			}
			return TypeTable.GetSize(this.Class, scope);
		}

		// Token: 0x06001BFF RID: 7167 RVA: 0x0004EA70 File Offset: 0x0004DA70
		public virtual int Size(IScope scope)
		{
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV34000)
			{
				ISpecialSizeType specialSizeType = this as ISpecialSizeType;
				if (specialSizeType != null)
				{
					return TypeTable.GetSize(specialSizeType.CodegeneratorType.Class, scope);
				}
			}
			return TypeTable.GetSize(this.Class, scope);
		}

		// Token: 0x17000785 RID: 1925
		// (get) Token: 0x06001C00 RID: 7168 RVA: 0x0004E9C9 File Offset: 0x0004D9C9
		public virtual ICompiledType EffectiveType
		{
			get
			{
				return this;
			}
		}

		// Token: 0x17000786 RID: 1926
		// (get) Token: 0x06001C01 RID: 7169
		public abstract TypeClass Class { get; }

		// Token: 0x06001C02 RID: 7170 RVA: 0x0004EAB6 File Offset: 0x0004DAB6
		public bool IsEqual(ICompiledType type)
		{
			return this.IsEqual(type, null);
		}

		// Token: 0x06001C03 RID: 7171 RVA: 0x0004EAC0 File Offset: 0x0004DAC0
		public virtual bool IsEqual(ICompiledType type, IScope scope)
		{
			if (type == null)
			{
				return false;
			}
			if (this.Class != type.Class)
			{
				return false;
			}
			if (this.Class != TypeClass.Array && this.Class != TypeClass.Userdef && this.Class != TypeClass.Pointer && this.Class != TypeClass.Enum && this.Class != TypeClass.Reference && this.Class != TypeClass.String && this.Class != TypeClass.WString && this.Class != TypeClass.Userdef && this.Class != TypeClass.__Vector)
			{
				return true;
			}
			string text = this.ToString();
			string text2 = type.ToString();
			return text.ToUpperInvariant() == text2.ToUpperInvariant();
		}

		// Token: 0x06001C04 RID: 7172 RVA: 0x0004EB5C File Offset: 0x0004DB5C
		public virtual bool IsEqualPreCompile(ICompiledType type, IScope scope)
		{
			if (type == null)
			{
				return false;
			}
			if (type.Class == TypeClass.Enum && APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351300)
			{
				if (this.Class != TypeClass.Userdef && this.Class != TypeClass.Enum)
				{
					return false;
				}
			}
			else if (this.Class != type.Class)
			{
				return false;
			}
			if (this.Class != TypeClass.Array && this.Class != TypeClass.Userdef && this.Class != TypeClass.Pointer && this.Class != TypeClass.Enum && this.Class != TypeClass.Reference && this.Class != TypeClass.String && this.Class != TypeClass.WString && this.Class != TypeClass.Userdef && this.Class != TypeClass.__Vector)
			{
				return true;
			}
			string text = this.ToString();
			string text2 = type.ToString();
			return text.ToUpperInvariant() == text2.ToUpperInvariant();
		}

		// Token: 0x06001C05 RID: 7173
		public abstract bool CanConvertRaw(byte[] raw, ByteOrder byteOrder, IScope5 scope);

		// Token: 0x06001C06 RID: 7174
		public abstract object ConvertRaw(byte[] raw, ByteOrder byteOrder, IScope5 scope);

		// Token: 0x06001C07 RID: 7175
		public abstract bool CanConvertToRaw(object value, ByteOrder byteOrder, IScope5 scope);

		// Token: 0x06001C08 RID: 7176
		public abstract byte[] ConvertToRaw(object value, ByteOrder byteOrder, IScope5 scope);

		// Token: 0x06001C09 RID: 7177 RVA: 0x0004EC28 File Offset: 0x0004DC28
		public override void AfterDeserialize()
		{
			base.AfterDeserialize();
		}

		// Token: 0x040005E6 RID: 1510
		[DefaultSerialization("Compiled")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		protected bool m_bCompiled;
	}
}
