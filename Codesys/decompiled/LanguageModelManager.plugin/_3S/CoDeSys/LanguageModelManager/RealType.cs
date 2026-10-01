using System;
using System.Diagnostics;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x0200018A RID: 394
	[TypeGuid("{4c33f62d-2ba9-4951-b27c-3cedc666c650}")]
	[StorageVersion("3.3.0.0")]
	public class RealType : IECType, _IRealType, _IType, ICompiledType5, ICompiledType4, ICompiledType3, ICompiledType2, ICompiledType, IType, IArchivable
	{
		// Token: 0x06001CE5 RID: 7397 RVA: 0x0005011D File Offset: 0x0004F11D
		public override string ToString()
		{
			return CompilerProxy.GetTextOfOperator(Operator.Real);
		}

		// Token: 0x170007A1 RID: 1953
		// (get) Token: 0x06001CE6 RID: 7398 RVA: 0x00050126 File Offset: 0x0004F126
		public override TypeClass Class
		{
			get
			{
				return TypeClass.Real;
			}
		}

		// Token: 0x06001CE7 RID: 7399 RVA: 0x0005012A File Offset: 0x0004F12A
		public override void Accept(ITypeVisitor typvis)
		{
			typvis.visit(this);
		}

		// Token: 0x06001CE8 RID: 7400 RVA: 0x0004F4D4 File Offset: 0x0004E4D4
		public override bool CanConvertRaw(byte[] raw, ByteOrder byteOrder, IScope5 scope)
		{
			return raw.Length == 4;
		}

		// Token: 0x06001CE9 RID: 7401 RVA: 0x00050134 File Offset: 0x0004F134
		public override object ConvertRaw(byte[] raw, ByteOrder byteOrder, IScope5 scope)
		{
			if (byteOrder == ByteOrder.Motorola)
			{
				byte[] array = new byte[4];
				raw.CopyTo(array, 0);
				Array.Reverse(array);
				return BitConverter.ToSingle(array, 0);
			}
			return BitConverter.ToSingle(raw, 0);
		}

		// Token: 0x06001CEA RID: 7402 RVA: 0x00050174 File Offset: 0x0004F174
		public override bool CanConvertToRaw(object value, ByteOrder byteOrder, IScope5 scope)
		{
			bool result;
			try
			{
				this.ConvertToRaw(value, byteOrder, scope);
				result = true;
			}
			catch (Exception)
			{
				result = false;
			}
			return result;
		}

		// Token: 0x06001CEB RID: 7403 RVA: 0x000501A8 File Offset: 0x0004F1A8
		public override byte[] ConvertToRaw(object value, ByteOrder byteOrder, IScope5 scope)
		{
			float value2;
			if (value is float)
			{
				value2 = (float)value;
			}
			else if (value is double)
			{
				double num = (double)value;
				if (num > 3.4028234663852886E+38 || num < -3.4028234663852886E+38)
				{
					throw new ArgumentException("value");
				}
				value2 = (float)num;
			}
			else
			{
				value2 = (float)TypeHelper.GetNumericValue(value, scope);
			}
			byte[] bytes = BitConverter.GetBytes(value2);
			Debug.Assert(bytes.Length == 4, "Raw value of a float must be 4 bytes");
			if (byteOrder == ByteOrder.Motorola)
			{
				Array.Reverse(bytes);
			}
			return bytes;
		}
	}
}
