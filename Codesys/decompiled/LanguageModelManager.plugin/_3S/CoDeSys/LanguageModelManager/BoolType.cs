using System;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x0200015D RID: 349
	[TypeGuid("{fd373d9e-533d-4d69-ad04-82ccf513ff10}")]
	[StorageVersion("3.3.0.0")]
	public class BoolType : IECType, _IBoolType, _IType, ICompiledType5, ICompiledType4, ICompiledType3, ICompiledType2, ICompiledType, IType, IArchivable
	{
		// Token: 0x06001C0D RID: 7181 RVA: 0x0004EC40 File Offset: 0x0004DC40
		public override string ToString()
		{
			return CompilerProxy.GetTextOfOperator(Operator.Bool);
		}

		// Token: 0x17000787 RID: 1927
		// (get) Token: 0x06001C0E RID: 7182 RVA: 0x00004E6B File Offset: 0x00003E6B
		public override TypeClass Class
		{
			get
			{
				return TypeClass.Bool;
			}
		}

		// Token: 0x06001C0F RID: 7183 RVA: 0x0004EC49 File Offset: 0x0004DC49
		public override void Accept(ITypeVisitor typvis)
		{
			typvis.visit(this);
		}

		// Token: 0x06001C10 RID: 7184 RVA: 0x0004EC52 File Offset: 0x0004DC52
		public override bool CanConvertRaw(byte[] raw, ByteOrder byteOrder, IScope5 scope)
		{
			return raw.Length == 1 && (raw[0] == 1 || raw[0] == 0);
		}

		// Token: 0x06001C11 RID: 7185 RVA: 0x0004EC6A File Offset: 0x0004DC6A
		public override object ConvertRaw(byte[] raw, ByteOrder byteOrder, IScope5 scope)
		{
			if (raw.Length != 1)
			{
				throw new InvalidCastException("Invalid boolean value");
			}
			if (raw[0] == 0)
			{
				return false;
			}
			if (raw[0] == 1)
			{
				return true;
			}
			return raw[0];
		}

		// Token: 0x06001C12 RID: 7186 RVA: 0x0004EC9E File Offset: 0x0004DC9E
		public override bool CanConvertToRaw(object value, ByteOrder byteOrder, IScope5 scope)
		{
			return value is bool;
		}

		// Token: 0x06001C13 RID: 7187 RVA: 0x0004ECAC File Offset: 0x0004DCAC
		public override byte[] ConvertToRaw(object value, ByteOrder byteOrder, IScope5 scope)
		{
			if (value is bool)
			{
				if ((bool)value)
				{
					return new byte[]
					{
						1
					};
				}
				return new byte[1];
			}
			else
			{
				string text = value.ToString().ToLowerInvariant();
				if (text == "true" || text == "bool#1" || text == "bool#true")
				{
					return new byte[]
					{
						1
					};
				}
				if (text == "false" || text == "bool#0" || text == "bool#false")
				{
					return new byte[1];
				}
				if (bool.Parse(text))
				{
					return new byte[]
					{
						1
					};
				}
				return new byte[1];
			}
		}
	}
}
