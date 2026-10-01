using System;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000171 RID: 369
	[TypeGuid("{95D415C0-DEB2-489b-A8D2-B3C6E3D89680}")]
	[StorageVersion("3.3.1.0")]
	public class RetainBoolType : BoolType, _IRetainBoolType, _IType, ICompiledType5, ICompiledType4, ICompiledType3, ICompiledType2, ICompiledType, IType, IArchivable, ISpecialSizeType
	{
		// Token: 0x1700078B RID: 1931
		// (get) Token: 0x06001C48 RID: 7240 RVA: 0x0004F044 File Offset: 0x0004E044
		public int CompatibilitySize
		{
			get
			{
				return TypeTable.GetSize(TypeClass.Bool, null);
			}
		}

		// Token: 0x1700078C RID: 1932
		// (get) Token: 0x06001C49 RID: 7241 RVA: 0x0004F04D File Offset: 0x0004E04D
		public ICompiledType CodegeneratorType
		{
			get
			{
				return TypeTable.Word;
			}
		}

		// Token: 0x06001C4A RID: 7242 RVA: 0x0004F054 File Offset: 0x0004E054
		public override int Size(IScope scope)
		{
			return TypeTable.GetSize(TypeClass.Word, scope);
		}

		// Token: 0x06001C4B RID: 7243 RVA: 0x0004F05D File Offset: 0x0004E05D
		public override bool CanConvertRaw(byte[] raw, ByteOrder byteOrder, IScope5 scope)
		{
			if (raw.Length != 2)
			{
				return false;
			}
			if (byteOrder == ByteOrder.Intel)
			{
				return raw[0] == 1 || raw[0] == 0;
			}
			return raw[1] == 1 || raw[1] == 0;
		}

		// Token: 0x06001C4C RID: 7244 RVA: 0x0004F088 File Offset: 0x0004E088
		public override object ConvertRaw(byte[] raw, ByteOrder byteOrder, IScope5 scope)
		{
			if (raw.Length != 2)
			{
				throw new InvalidCastException("Invalid boolean value");
			}
			byte b = raw[0];
			if (byteOrder == ByteOrder.Motorola)
			{
				b = raw[1];
			}
			if (b == 0)
			{
				return false;
			}
			if (b == 1)
			{
				return true;
			}
			return raw[0];
		}

		// Token: 0x06001C4D RID: 7245 RVA: 0x0004F0D0 File Offset: 0x0004E0D0
		public override byte[] ConvertToRaw(object value, ByteOrder byteOrder, IScope5 scope)
		{
			if (value is bool)
			{
				if (!(bool)value)
				{
					return new byte[2];
				}
				if (byteOrder == ByteOrder.Intel)
				{
					byte[] array = new byte[2];
					array[0] = 1;
					return array;
				}
				return new byte[]
				{
					0,
					1
				};
			}
			else
			{
				string text = value.ToString().ToLowerInvariant();
				if (!(text == "true") && !(text == "bool#1") && !(text == "bool#true"))
				{
					if (text == "false" || text == "bool#0" || text == "bool#false")
					{
						return new byte[2];
					}
					if (!bool.Parse(text))
					{
						return new byte[2];
					}
					if (byteOrder == ByteOrder.Intel)
					{
						byte[] array2 = new byte[2];
						array2[0] = 1;
						return array2;
					}
					return new byte[]
					{
						0,
						1
					};
				}
				else
				{
					if (byteOrder == ByteOrder.Intel)
					{
						byte[] array3 = new byte[2];
						array3[0] = 1;
						return array3;
					}
					return new byte[]
					{
						0,
						1
					};
				}
			}
		}

		// Token: 0x06001C4E RID: 7246 RVA: 0x0004EC9E File Offset: 0x0004DC9E
		public override bool CanConvertToRaw(object value, ByteOrder byteOrder, IScope5 scope)
		{
			return value is bool;
		}
	}
}
