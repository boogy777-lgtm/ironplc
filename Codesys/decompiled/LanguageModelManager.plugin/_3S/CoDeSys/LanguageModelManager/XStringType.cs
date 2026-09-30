using System;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x0200018D RID: 397
	[TypeGuid("{964B6B19-CD1E-426F-B296-8793B8BF1E49}")]
	[StorageVersion("3.5.5.0")]
	public class XStringType : IECType, _IXStringType, _IStringType, _IType, ICompiledType5, ICompiledType4, ICompiledType3, ICompiledType2, ICompiledType, IType, IArchivable, IStringType, _IWStringType, IWStringType
	{
		// Token: 0x170007A7 RID: 1959
		// (get) Token: 0x06001D08 RID: 7432 RVA: 0x000505DA File Offset: 0x0004F5DA
		public IExpression LengthExpression
		{
			get
			{
				return this.m_expSize;
			}
		}

		// Token: 0x170007A8 RID: 1960
		// (get) Token: 0x06001D09 RID: 7433 RVA: 0x000505DA File Offset: 0x0004F5DA
		// (set) Token: 0x06001D0A RID: 7434 RVA: 0x000505E2 File Offset: 0x0004F5E2
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

		// Token: 0x06001D0B RID: 7435 RVA: 0x000505EB File Offset: 0x0004F5EB
		public override string ToString()
		{
			return CompilerProxy.GetTextOfOperator(Operator.__XString);
		}

		// Token: 0x170007A9 RID: 1961
		// (get) Token: 0x06001D0C RID: 7436 RVA: 0x000505F7 File Offset: 0x0004F5F7
		public override TypeClass Class
		{
			get
			{
				return TypeClass.XString;
			}
		}

		// Token: 0x06001D0D RID: 7437 RVA: 0x000505FB File Offset: 0x0004F5FB
		public override void Accept(ITypeVisitor typvis)
		{
			typvis.visit(this);
		}

		// Token: 0x06001D0E RID: 7438 RVA: 0x00004E6B File Offset: 0x00003E6B
		public override bool CanConvertRaw(byte[] raw, ByteOrder byteOrder, IScope5 scope)
		{
			return false;
		}

		// Token: 0x06001D0F RID: 7439 RVA: 0x00005F0F File Offset: 0x00004F0F
		public override object ConvertRaw(byte[] raw, ByteOrder byteOrder, IScope5 scope)
		{
			return null;
		}

		// Token: 0x06001D10 RID: 7440 RVA: 0x00004E6B File Offset: 0x00003E6B
		public override bool CanConvertToRaw(object value, ByteOrder byteOrder, IScope5 scope)
		{
			return false;
		}

		// Token: 0x06001D11 RID: 7441 RVA: 0x00005F0F File Offset: 0x00004F0F
		public override byte[] ConvertToRaw(object value, ByteOrder byteOrder, IScope5 scope)
		{
			return null;
		}

		// Token: 0x040005E8 RID: 1512
		[DefaultSerialization("SizeExpression")]
		[StorageVersion("3.5.5.0")]
		[Obfuscation(Feature = "rename")]
		private _IExpression m_expSize;
	}
}
