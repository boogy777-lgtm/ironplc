using System;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x0200018E RID: 398
	[TypeGuid("{00014fd3-8bc4-4e77-981b-095b9c869fc2}")]
	[StorageVersion("3.3.0.0")]
	public class WStringType : IECType, _IWStringType, _IType, ICompiledType5, ICompiledType4, ICompiledType3, ICompiledType2, ICompiledType, IType, IArchivable, IWStringType, ITypeWithRecursiveTypeCheck
	{
		// Token: 0x170007AA RID: 1962
		// (get) Token: 0x06001D13 RID: 7443 RVA: 0x00050604 File Offset: 0x0004F604
		public IExpression LengthExpression
		{
			get
			{
				return this.m_expSize;
			}
		}

		// Token: 0x170007AB RID: 1963
		// (get) Token: 0x06001D14 RID: 7444 RVA: 0x00050604 File Offset: 0x0004F604
		// (set) Token: 0x06001D15 RID: 7445 RVA: 0x0005060C File Offset: 0x0004F60C
		public _IExpression Length
		{
			get
			{
				return this.m_expSize;
			}
			set
			{
				this.m_expSize = value;
			}
		}

		// Token: 0x06001D16 RID: 7446 RVA: 0x00050618 File Offset: 0x0004F618
		public override string ToString()
		{
			string text = CompilerProxy.GetTextOfOperator(Operator.WString);
			if (this.Length != null)
			{
				string str = text;
				string str2 = "(";
				_IExpression length = this.Length;
				text = str + str2 + ((length != null) ? length.ToString() : null) + ")";
			}
			return text;
		}

		// Token: 0x06001D17 RID: 7447 RVA: 0x0005065C File Offset: 0x0004F65C
		public override string GetConstantString(IScope scope)
		{
			if (this.m_expSize == null)
			{
				return CompilerProxy.GetTextOfOperator(Operator.WString);
			}
			bool flag;
			int num = TypeHelper.GetInt(this.m_expSize, scope as IScope5, out flag);
			if (!flag || num <= 0)
			{
				num = WStringType.DefaultSize;
			}
			return CompilerProxy.GetTextOfOperator(Operator.WString) + "(" + num.ToString() + ")";
		}

		// Token: 0x170007AC RID: 1964
		// (get) Token: 0x06001D18 RID: 7448 RVA: 0x000506B7 File Offset: 0x0004F6B7
		public override TypeClass Class
		{
			get
			{
				return TypeClass.WString;
			}
		}

		// Token: 0x06001D19 RID: 7449 RVA: 0x000506BB File Offset: 0x0004F6BB
		public override void Accept(ITypeVisitor typvis)
		{
			typvis.visit(this);
		}

		// Token: 0x06001D1A RID: 7450 RVA: 0x000506C4 File Offset: 0x0004F6C4
		public override _IType _Duplicate(bool bDeep)
		{
			WStringType wstringType = new WStringType();
			if (this.Length != null)
			{
				wstringType.Length = (this.Length.Duplicate() as _IExpression);
			}
			return wstringType;
		}

		// Token: 0x170007AD RID: 1965
		// (get) Token: 0x06001D1B RID: 7451 RVA: 0x000506F6 File Offset: 0x0004F6F6
		public static int DefaultSize
		{
			get
			{
				return CompilerConstants.WStringTypeDefaultSize;
			}
		}

		// Token: 0x06001D1C RID: 7452 RVA: 0x00050700 File Offset: 0x0004F700
		public int SizeWithRecursionCheck(IScope scope, IRecursionGuard recursionGuard, out bool bValid)
		{
			bValid = true;
			if (this.m_expSize == null)
			{
				if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35350)
				{
					return WStringType.DefaultSize * 2;
				}
				return WStringType.DefaultSize;
			}
			else
			{
				if (recursionGuard != null)
				{
					recursionGuard = recursionGuard.Duplicate();
					if (recursionGuard.Has(this))
					{
						bValid = false;
						return WStringType.DefaultSize;
					}
					recursionGuard.Add(this);
				}
				int @int = TypeHelper.GetInt(this.m_expSize, scope as IScope5, true, recursionGuard, out bValid);
				if (!bValid || @int <= 0)
				{
					return WStringType.DefaultSize * 2;
				}
				return (@int + 1) * 2;
			}
		}

		// Token: 0x06001D1D RID: 7453 RVA: 0x00050788 File Offset: 0x0004F788
		public override int SizeChecked(IScope scope, out bool bValid)
		{
			bValid = true;
			if (this.m_expSize == null)
			{
				if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35350)
				{
					return WStringType.DefaultSize * 2;
				}
				return WStringType.DefaultSize;
			}
			else
			{
				int @int = TypeHelper.GetInt(this.m_expSize, scope as IScope5, out bValid);
				if (!bValid || @int <= 0)
				{
					return WStringType.DefaultSize * 2;
				}
				return (@int + 1) * 2;
			}
		}

		// Token: 0x06001D1E RID: 7454 RVA: 0x000507E8 File Offset: 0x0004F7E8
		public override int Size(IScope scope)
		{
			bool flag;
			int @int = TypeHelper.GetInt(this.m_expSize, scope as IScope5, out flag);
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351500)
			{
				if (!flag || @int < 0)
				{
					return WStringType.DefaultSize * 2;
				}
			}
			else if (!flag || @int <= 0)
			{
				return WStringType.DefaultSize * 2;
			}
			return (@int + 1) * 2;
		}

		// Token: 0x06001D1F RID: 7455 RVA: 0x00050840 File Offset: 0x0004F840
		private int StringLength(byte[] raw)
		{
			if ((raw.Length & 1) != 0)
			{
				return -1;
			}
			for (int i = 0; i < raw.Length; i += 2)
			{
				if (raw[i] == 0 && raw[i + 1] == 0)
				{
					return i;
				}
			}
			return -1;
		}

		// Token: 0x06001D20 RID: 7456 RVA: 0x00050873 File Offset: 0x0004F873
		public override bool CanConvertRaw(byte[] raw, ByteOrder byteOrder, IScope5 scope)
		{
			return raw.Length <= this.Size(scope) && this.StringLength(raw) != -1;
		}

		// Token: 0x06001D21 RID: 7457 RVA: 0x00050890 File Offset: 0x0004F890
		public override object ConvertRaw(byte[] raw, ByteOrder byteOrder, IScope5 scope)
		{
			if (raw.Length > this.Size(scope))
			{
				throw new InvalidCastException("Invalid length of the WSTRING value");
			}
			if (this.StringLength(raw) == -1)
			{
				throw new InvalidCastException("Invalid WSTRING value");
			}
			return CompilerProxy.StringEncodingService.ConvertBytesToString(raw, byteOrder, TypeClass.WString, StringEncoding.Default);
		}

		// Token: 0x06001D22 RID: 7458 RVA: 0x000508D0 File Offset: 0x0004F8D0
		public override bool IsEqual(ICompiledType type, IScope scope)
		{
			if (!APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35900)
			{
				return base.IsEqual(type, scope);
			}
			if (scope == null)
			{
				return base.IsEqual(type, scope);
			}
			WStringType wstringType = type as WStringType;
			return wstringType != null && wstringType.Size(scope) == this.Size(scope);
		}

		// Token: 0x06001D23 RID: 7459 RVA: 0x0005054F File Offset: 0x0004F54F
		public override bool IsEqualPreCompile(ICompiledType type, IScope scope)
		{
			return this.IsEqual(type, scope);
		}

		// Token: 0x06001D24 RID: 7460 RVA: 0x0005091F File Offset: 0x0004F91F
		public override bool CanConvertToRaw(object value, ByteOrder byteOrder, IScope5 scope)
		{
			return value is string && CompilerProxy.StringEncodingService.GetNumberOfStringBytes((string)value, byteOrder, TypeClass.WString, StringEncoding.Default) + 2L <= (long)this.Size(scope);
		}

		// Token: 0x06001D25 RID: 7461 RVA: 0x00050950 File Offset: 0x0004F950
		public override byte[] ConvertToRaw(object value, ByteOrder byteOrder, IScope5 scope)
		{
			string stValue = (string)value;
			byte[] stringBytes = CompilerProxy.StringEncodingService.GetStringBytes(byteOrder, TypeClass.WString, 2, stValue, StringEncoding.Default);
			if (stringBytes.Length > this.Size(scope))
			{
				throw new InvalidCastException(string.Format("String to long (max. {0} characters)", this.Size(scope) - 1));
			}
			string value2 = CompilerProxy.StringEncodingService.ConvertBytesToString(stringBytes, byteOrder, TypeClass.WString, StringEncoding.Default);
			if (!WStringType.ReplaceEscapeSequences(stValue).Equals(value2))
			{
				throw new InvalidCastException("The string contains characters that are no valid unicode characters.");
			}
			return stringBytes;
		}

		// Token: 0x06001D26 RID: 7462 RVA: 0x000509CC File Offset: 0x0004F9CC
		private static string ReplaceEscapeSequences(string stValue)
		{
			return stValue.Replace("$", "$$").Replace("'", "$'").Replace("\"", "$\"").Replace("\n", "$N").Replace("\f", "$P").Replace("\r", "$R").Replace("\t", "$T");
		}

		// Token: 0x040005E9 RID: 1513
		[DefaultSerialization("SizeExpression")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private _IExpression m_expSize;
	}
}
