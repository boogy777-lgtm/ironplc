using System;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x0200016E RID: 366
	[TypeGuid("{71c4e10e-3e38-473f-b4b9-db1c89cbe452}")]
	[StorageVersion("3.3.0.0")]
	public class BitType : IECType, _IBitType, _IType, ICompiledType5, ICompiledType4, ICompiledType3, ICompiledType2, ICompiledType, IType, IArchivable
	{
		// Token: 0x06001C35 RID: 7221 RVA: 0x0004EE9C File Offset: 0x0004DE9C
		public override string ToString()
		{
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35500 || (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV345110 && !APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35000))
			{
				return CompilerProxy.GetTextOfOperator(Operator.Bit);
			}
			return CompilerProxy.GetTextOfOperator(Operator.Bool);
		}

		// Token: 0x17000788 RID: 1928
		// (get) Token: 0x06001C36 RID: 7222 RVA: 0x00005E58 File Offset: 0x00004E58
		public override TypeClass Class
		{
			get
			{
				return TypeClass.Bit;
			}
		}

		// Token: 0x06001C37 RID: 7223 RVA: 0x0004EEEB File Offset: 0x0004DEEB
		public override void Accept(ITypeVisitor typvis)
		{
			typvis.visit(this);
		}

		// Token: 0x06001C38 RID: 7224 RVA: 0x00005E58 File Offset: 0x00004E58
		public override bool CanConvertRaw(byte[] raw, ByteOrder byteOrder, IScope5 scope)
		{
			return true;
		}

		// Token: 0x06001C39 RID: 7225 RVA: 0x0004EEF4 File Offset: 0x0004DEF4
		public override object ConvertRaw(byte[] raw, ByteOrder byteOrder, IScope5 scope)
		{
			return (raw[0] & 1) == 1;
		}

		// Token: 0x06001C3A RID: 7226 RVA: 0x0004EF08 File Offset: 0x0004DF08
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

		// Token: 0x06001C3B RID: 7227 RVA: 0x0004EF3C File Offset: 0x0004DF3C
		public override byte[] ConvertToRaw(object value, ByteOrder byteOrder, IScope5 scope)
		{
			byte b;
			if (value is bool)
			{
				b = (((bool)value) ? 1 : 0);
			}
			else
			{
				long numericValue = TypeHelper.GetNumericValue(value, scope);
				if (numericValue != 0L)
				{
					if (numericValue != 1L)
					{
						throw new InvalidCastException("Invalid value " + value.ToString() + " for a binary value");
					}
					b = 1;
				}
				else
				{
					b = 0;
				}
			}
			return new byte[]
			{
				b
			};
		}
	}
}
