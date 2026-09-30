using System;
using System.Diagnostics;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x0200018B RID: 395
	[TypeGuid("{ac2d6c97-8b55-498b-83df-2059efb47336}")]
	[StorageVersion("3.3.0.0")]
	public class LRealType : IECType, _ILRealType, _IType, ICompiledType5, ICompiledType4, ICompiledType3, ICompiledType2, ICompiledType, IType, IArchivable
	{
		// Token: 0x06001CED RID: 7405 RVA: 0x00050228 File Offset: 0x0004F228
		public override string ToString()
		{
			return CompilerProxy.GetTextOfOperator(Operator.LReal);
		}

		// Token: 0x170007A2 RID: 1954
		// (get) Token: 0x06001CEE RID: 7406 RVA: 0x00050231 File Offset: 0x0004F231
		public override TypeClass Class
		{
			get
			{
				return TypeClass.LReal;
			}
		}

		// Token: 0x06001CEF RID: 7407 RVA: 0x00050235 File Offset: 0x0004F235
		public override void Accept(ITypeVisitor typvis)
		{
			typvis.visit(this);
		}

		// Token: 0x06001CF0 RID: 7408 RVA: 0x0004F603 File Offset: 0x0004E603
		public override bool CanConvertRaw(byte[] raw, ByteOrder byteOrder, IScope5 scope)
		{
			return raw.Length == 8;
		}

		// Token: 0x06001CF1 RID: 7409 RVA: 0x00050240 File Offset: 0x0004F240
		public override object ConvertRaw(byte[] raw, ByteOrder byteOrder, IScope5 scope)
		{
			if (byteOrder == ByteOrder.Motorola)
			{
				byte[] array = new byte[8];
				raw.CopyTo(array, 0);
				Array.Reverse(array);
				return BitConverter.ToDouble(array, 0);
			}
			return BitConverter.ToDouble(raw, 0);
		}

		// Token: 0x06001CF2 RID: 7410 RVA: 0x00050280 File Offset: 0x0004F280
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

		// Token: 0x06001CF3 RID: 7411 RVA: 0x000502B4 File Offset: 0x0004F2B4
		public override byte[] ConvertToRaw(object value, ByteOrder byteOrder, IScope5 scope)
		{
			double value2;
			if (value is float)
			{
				value2 = (double)((float)value);
			}
			else if (value is double)
			{
				value2 = (double)value;
			}
			else
			{
				value2 = (double)TypeHelper.GetNumericValue(value, scope);
			}
			byte[] bytes = BitConverter.GetBytes(value2);
			Debug.Assert(bytes.Length == 8, "Raw value of a float must be 4 bytes");
			if (byteOrder == ByteOrder.Motorola)
			{
				Array.Reverse(bytes);
			}
			return bytes;
		}
	}
}
