using System;
using System.Reflection;
using System.Text;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x020001A2 RID: 418
	[TypeGuid("{FABC1445-524D-44F0-B6EC-2C81F6235E3E}")]
	[StorageVersion("3.5.8.0")]
	public class VariableLengthArrayType : IECType, _IVariableLengthArrayType, _IType, ICompiledType5, ICompiledType4, ICompiledType3, ICompiledType2, ICompiledType, IType, IArchivable
	{
		// Token: 0x06001E53 RID: 7763 RVA: 0x0004EC38 File Offset: 0x0004DC38
		public VariableLengthArrayType()
		{
		}

		// Token: 0x06001E54 RID: 7764 RVA: 0x00053B24 File Offset: 0x00052B24
		public VariableLengthArrayType(_IType typeBase, int nDimensions)
		{
			this.m_typeBase = typeBase;
			this.m_nDimensions = nDimensions;
		}

		// Token: 0x170007F4 RID: 2036
		// (get) Token: 0x06001E55 RID: 7765 RVA: 0x00053B3A File Offset: 0x00052B3A
		public IType Base
		{
			get
			{
				return this.m_typeBase;
			}
		}

		// Token: 0x170007F5 RID: 2037
		// (get) Token: 0x06001E56 RID: 7766 RVA: 0x00053B3A File Offset: 0x00052B3A
		// (set) Token: 0x06001E57 RID: 7767 RVA: 0x00053B42 File Offset: 0x00052B42
		public _IType _Base
		{
			get
			{
				return this.m_typeBase;
			}
			set
			{
				this.m_typeBase = value;
			}
		}

		// Token: 0x170007F6 RID: 2038
		// (get) Token: 0x06001E58 RID: 7768 RVA: 0x00053B4B File Offset: 0x00052B4B
		// (set) Token: 0x06001E59 RID: 7769 RVA: 0x00053B53 File Offset: 0x00052B53
		public int Dimensions
		{
			get
			{
				return this.m_nDimensions;
			}
			set
			{
				this.m_nDimensions = value;
			}
		}

		// Token: 0x170007F7 RID: 2039
		// (get) Token: 0x06001E5A RID: 7770 RVA: 0x00053B3A File Offset: 0x00052B3A
		public override ICompiledType BaseType
		{
			get
			{
				return this.m_typeBase;
			}
		}

		// Token: 0x06001E5B RID: 7771 RVA: 0x00053B5C File Offset: 0x00052B5C
		public override string ToString()
		{
			if (this.m_typeBase == null)
			{
				return "ERROR";
			}
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("ARRAY[");
			for (int i = 0; i < this.Dimensions; i++)
			{
				stringBuilder.Append("*");
				if (i < this.Dimensions - 1)
				{
					stringBuilder.Append(",");
				}
			}
			stringBuilder.Append("] OF " + this.m_typeBase.ToString());
			return stringBuilder.ToString();
		}

		// Token: 0x06001E5C RID: 7772 RVA: 0x00051202 File Offset: 0x00050202
		public override string ToUpperString()
		{
			return this.ToString().ToUpperInvariant();
		}

		// Token: 0x06001E5D RID: 7773 RVA: 0x00053BE0 File Offset: 0x00052BE0
		public override string GetConstantString(IScope scope)
		{
			if (this.m_typeBase == null)
			{
				return "ERROR";
			}
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("ARRAY[");
			for (int i = 0; i < this.Dimensions; i++)
			{
				stringBuilder.Append("*");
				if (i < this.Dimensions - 1)
				{
					stringBuilder.Append(",");
				}
			}
			stringBuilder.Append("] OF " + this.m_typeBase.GetConstantString(scope));
			return stringBuilder.ToString();
		}

		// Token: 0x170007F8 RID: 2040
		// (get) Token: 0x06001E5E RID: 7774 RVA: 0x00053C64 File Offset: 0x00052C64
		public override TypeClass Class
		{
			get
			{
				return TypeClass.VarLenArray;
			}
		}

		// Token: 0x06001E5F RID: 7775 RVA: 0x00053C68 File Offset: 0x00052C68
		public override void Accept(ITypeVisitor typvis)
		{
			typvis.visit(this);
		}

		// Token: 0x06001E60 RID: 7776 RVA: 0x00053C71 File Offset: 0x00052C71
		public override _IType _Duplicate(bool bDeep)
		{
			return new VariableLengthArrayType
			{
				_Base = this.m_typeBase._Duplicate(bDeep),
				Dimensions = this.Dimensions
			};
		}

		// Token: 0x06001E61 RID: 7777 RVA: 0x00053C96 File Offset: 0x00052C96
		public override int SizeChecked(IScope scope, out bool bValid)
		{
			return this.m_typeBase.SizeChecked(scope, out bValid);
		}

		// Token: 0x06001E62 RID: 7778 RVA: 0x00053CA5 File Offset: 0x00052CA5
		public override int Size(IScope scope)
		{
			return this.m_typeBase.Size(scope);
		}

		// Token: 0x06001E63 RID: 7779 RVA: 0x00004E6B File Offset: 0x00003E6B
		public override bool CanConvertRaw(byte[] raw, ByteOrder byteOrder, IScope5 scope)
		{
			return false;
		}

		// Token: 0x06001E64 RID: 7780 RVA: 0x0005392A File Offset: 0x0005292A
		public override object ConvertRaw(byte[] raw, ByteOrder byteOrder, IScope5 scope)
		{
			throw new NotImplementedException("ArrayType.ConvertRaw not implemented yet");
		}

		// Token: 0x06001E65 RID: 7781 RVA: 0x00004E6B File Offset: 0x00003E6B
		public override bool CanConvertToRaw(object value, ByteOrder byteOrder, IScope5 scope)
		{
			return false;
		}

		// Token: 0x06001E66 RID: 7782 RVA: 0x00053936 File Offset: 0x00052936
		public override byte[] ConvertToRaw(object value, ByteOrder byteOrder, IScope5 scope)
		{
			throw new NotImplementedException("ArrayType.ConvertToRaw not implemented yet");
		}

		// Token: 0x040005FD RID: 1533
		[DefaultSerialization("BaseType")]
		[StorageVersion("3.5.8.0")]
		[Obfuscation(Feature = "rename")]
		private _IType m_typeBase;

		// Token: 0x040005FE RID: 1534
		[DefaultSerialization("Dimensions")]
		[StorageVersion("3.5.8.0")]
		[Obfuscation(Feature = "rename")]
		private int m_nDimensions;
	}
}
