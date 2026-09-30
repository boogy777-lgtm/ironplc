using System;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x0200018C RID: 396
	[TypeGuid("{ab09737e-b891-4fc6-9c8c-246eb8cdb046}")]
	[StorageVersion("3.3.0.0")]
	public class StringType : IECType, _IStringType, _IType, ICompiledType5, ICompiledType4, ICompiledType3, ICompiledType2, ICompiledType, IType, IArchivable, IStringType, ITypeWithRecursiveTypeCheck
	{
		// Token: 0x170007A3 RID: 1955
		// (get) Token: 0x06001CF5 RID: 7413 RVA: 0x0005030F File Offset: 0x0004F30F
		public IExpression LengthExpression
		{
			get
			{
				return this.m_expSize;
			}
		}

		// Token: 0x170007A4 RID: 1956
		// (get) Token: 0x06001CF6 RID: 7414 RVA: 0x0005030F File Offset: 0x0004F30F
		// (set) Token: 0x06001CF7 RID: 7415 RVA: 0x00050317 File Offset: 0x0004F317
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

		// Token: 0x06001CF8 RID: 7416 RVA: 0x00050320 File Offset: 0x0004F320
		public override string ToString()
		{
			string text = CompilerProxy.GetTextOfOperator(Operator.String);
			if (this.Length != null)
			{
				string str = text;
				string str2 = "(";
				_IExpression length = this.Length;
				text = str + str2 + ((length != null) ? length.ToString() : null) + ")";
			}
			return text;
		}

		// Token: 0x06001CF9 RID: 7417 RVA: 0x00050364 File Offset: 0x0004F364
		public override string GetConstantString(IScope scope)
		{
			string text = CompilerProxy.GetTextOfOperator(Operator.String);
			if (this.Length != null)
			{
				text = text + "(" + (this.Size(scope) - 1).ToString() + ")";
			}
			return text;
		}

		// Token: 0x170007A5 RID: 1957
		// (get) Token: 0x06001CFA RID: 7418 RVA: 0x000503A4 File Offset: 0x0004F3A4
		public override TypeClass Class
		{
			get
			{
				return TypeClass.String;
			}
		}

		// Token: 0x06001CFB RID: 7419 RVA: 0x000503A8 File Offset: 0x0004F3A8
		public override void Accept(ITypeVisitor typvis)
		{
			typvis.visit(this);
		}

		// Token: 0x06001CFC RID: 7420 RVA: 0x000503B4 File Offset: 0x0004F3B4
		public override _IType _Duplicate(bool bDeep)
		{
			StringType stringType = new StringType();
			if (this.Length != null)
			{
				stringType.Length = (this.Length.Duplicate() as _IExpression);
			}
			return stringType;
		}

		// Token: 0x170007A6 RID: 1958
		// (get) Token: 0x06001CFD RID: 7421 RVA: 0x000503E6 File Offset: 0x0004F3E6
		public static int DefaultSize
		{
			get
			{
				return CompilerConstants.StringTypeDefaultSize;
			}
		}

		// Token: 0x06001CFE RID: 7422 RVA: 0x000503ED File Offset: 0x0004F3ED
		public override int SizeChecked(IScope scope, out bool bValid)
		{
			bValid = true;
			return this.Size(scope);
		}

		// Token: 0x06001CFF RID: 7423 RVA: 0x000503FC File Offset: 0x0004F3FC
		public override int Size(IScope scope)
		{
			bool flag;
			return this.SizeWithRecursionCheck(scope, null, out flag);
		}

		// Token: 0x06001D00 RID: 7424 RVA: 0x00050414 File Offset: 0x0004F414
		public int SizeWithRecursionCheck(IScope scope, IRecursionGuard recursionGuard, out bool bValid)
		{
			bValid = true;
			if (this.m_expSize == null)
			{
				return StringType.DefaultSize;
			}
			int @int;
			if (recursionGuard != null)
			{
				recursionGuard = recursionGuard.Duplicate();
				if (recursionGuard.Has(this))
				{
					bValid = false;
					return StringType.DefaultSize;
				}
				recursionGuard.Add(this);
				@int = TypeHelper.GetInt(this.m_expSize, scope as IScope5, true, recursionGuard, out bValid);
			}
			else if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35700)
			{
				@int = TypeHelper.GetInt(this.m_expSize, scope as IScope5, true, out bValid);
			}
			else
			{
				@int = TypeHelper.GetInt(this.m_expSize, scope as IScope5, out bValid);
			}
			if (!bValid || @int < 0)
			{
				return StringType.DefaultSize;
			}
			return @int + 1;
		}

		// Token: 0x06001D01 RID: 7425 RVA: 0x000504BA File Offset: 0x0004F4BA
		public override bool CanConvertRaw(byte[] raw, ByteOrder byteOrder, IScope5 scope)
		{
			return raw.Length <= this.Size(scope) && Array.IndexOf<byte>(raw, 0) != -1;
		}

		// Token: 0x06001D02 RID: 7426 RVA: 0x000504D7 File Offset: 0x0004F4D7
		public override object ConvertRaw(byte[] raw, ByteOrder byteOrder, IScope5 scope)
		{
			if (raw.Length > this.Size(scope))
			{
				throw new InvalidCastException("Invalid length of the STRING value");
			}
			return CompilerProxy.StringEncodingService.ConvertBytesToString(raw, byteOrder, TypeClass.String, StringEncoding.Default);
		}

		// Token: 0x06001D03 RID: 7427 RVA: 0x00050500 File Offset: 0x0004F500
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
			StringType stringType = type as StringType;
			return stringType != null && stringType.Size(scope) == this.Size(scope);
		}

		// Token: 0x06001D04 RID: 7428 RVA: 0x0005054F File Offset: 0x0004F54F
		public override bool IsEqualPreCompile(ICompiledType type, IScope scope)
		{
			return this.IsEqual(type, scope);
		}

		// Token: 0x06001D05 RID: 7429 RVA: 0x00050559 File Offset: 0x0004F559
		public override bool CanConvertToRaw(object value, ByteOrder byteOrder, IScope5 scope)
		{
			return value is string && CompilerProxy.StringEncodingService.GetNumberOfStringBytes((string)value, byteOrder, TypeClass.String, StringEncoding.Default) + 1L <= (long)this.Size(scope);
		}

		// Token: 0x06001D06 RID: 7430 RVA: 0x0005058C File Offset: 0x0004F58C
		public override byte[] ConvertToRaw(object value, ByteOrder byteOrder, IScope5 scope)
		{
			string stValue = (string)value;
			byte[] stringBytes = CompilerProxy.StringEncodingService.GetStringBytes(byteOrder, TypeClass.String, 1, stValue, StringEncoding.Default);
			if (stringBytes.Length > this.Size(scope))
			{
				throw new InvalidCastException(string.Format("String to long (max. {0} characters)", this.Size(scope) - 1));
			}
			return stringBytes;
		}

		// Token: 0x040005E7 RID: 1511
		[DefaultSerialization("SizeExpression")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private _IExpression m_expSize;
	}
}
