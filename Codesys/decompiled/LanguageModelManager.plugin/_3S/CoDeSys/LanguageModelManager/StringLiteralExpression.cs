using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000071 RID: 113
	[TypeGuid("{187803f1-6b2c-486c-bcf0-b6f913f59ce3}")]
	[StorageVersion("3.3.0.0")]
	public class StringLiteralExpression : LiteralExpression, _IStringLiteralExpression2, _IStringLiteralExpression, _ILiteralExpression, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression, ILiteralExpression2, ILiteralExpression
	{
		// Token: 0x0600073B RID: 1851 RVA: 0x0001285B File Offset: 0x0001185B
		public StringLiteralExpression()
		{
		}

		// Token: 0x0600073C RID: 1852 RVA: 0x00012878 File Offset: 0x00011878
		public StringLiteralExpression(string stVal, TypeClass tc)
		{
			this.m_stValue = stVal;
			this.m_type = tc;
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351600)
			{
				this.m_tcOriginal = tc;
			}
		}

		// Token: 0x0600073D RID: 1853 RVA: 0x000128C4 File Offset: 0x000118C4
		public StringLiteralExpression(string stVal, TypeClass tc, StringEncoding stringEncoding)
		{
			this.m_stValue = stVal;
			this.m_type = tc;
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351600)
			{
				this.m_tcOriginal = tc;
			}
			this.m_stringEncoding = stringEncoding;
		}

		// Token: 0x0600073E RID: 1854 RVA: 0x00012917 File Offset: 0x00011917
		public StringLiteralExpression(string stVal, TypeClass tc, IToken token) : base(token)
		{
			this.m_stValue = stVal;
			this.m_type = tc;
			this.m_tcOriginal = tc;
		}

		// Token: 0x0600073F RID: 1855 RVA: 0x00012948 File Offset: 0x00011948
		public StringLiteralExpression(string stVal, TypeClass tc, IToken token, StringEncoding stringEncoding) : base(token)
		{
			this.m_stValue = stVal;
			this.m_type = tc;
			this.m_tcOriginal = tc;
			this.m_stringEncoding = stringEncoding;
		}

		// Token: 0x06000740 RID: 1856 RVA: 0x00012981 File Offset: 0x00011981
		public StringLiteralExpression(string stVal)
		{
			this.m_stValue = stVal;
			this.m_type = TypeClass.String;
		}

		// Token: 0x06000741 RID: 1857 RVA: 0x000129AB File Offset: 0x000119AB
		public override bool IsVarInOutInput(IScope scope, bool bWriteToConstants, bool bVarInoutConstant)
		{
			return !APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35800 || bVarInoutConstant;
		}

		// Token: 0x17000196 RID: 406
		// (get) Token: 0x06000742 RID: 1858 RVA: 0x0000E8C0 File Offset: 0x0000D8C0
		// (set) Token: 0x06000743 RID: 1859 RVA: 0x00003AE9 File Offset: 0x00002AE9
		[SuppressMessage("Blocker Code Smell", "S3237:\"value\" parameters should be used", Justification = "Setter cannot be changed for compatibility reasons")]
		public override long LongValue
		{
			get
			{
				return 0L;
			}
			set
			{
			}
		}

		// Token: 0x17000197 RID: 407
		// (get) Token: 0x06000744 RID: 1860 RVA: 0x00004E6B File Offset: 0x00003E6B
		// (set) Token: 0x06000745 RID: 1861 RVA: 0x00003AE9 File Offset: 0x00002AE9
		[SuppressMessage("Blocker Code Smell", "S3237:\"value\" parameters should be used", Justification = "Setter cannot be changed for compatibility reasons")]
		public override bool Negative
		{
			get
			{
				return false;
			}
			set
			{
			}
		}

		// Token: 0x17000198 RID: 408
		// (get) Token: 0x06000746 RID: 1862 RVA: 0x0000E8C0 File Offset: 0x0000D8C0
		public override ulong ULongValue
		{
			get
			{
				return 0UL;
			}
		}

		// Token: 0x17000199 RID: 409
		// (get) Token: 0x06000747 RID: 1863 RVA: 0x000129C6 File Offset: 0x000119C6
		// (set) Token: 0x06000748 RID: 1864 RVA: 0x000129CE File Offset: 0x000119CE
		public override string StringValue
		{
			get
			{
				return this.m_stValue;
			}
			set
			{
				this.m_stValue = value;
			}
		}

		// Token: 0x1700019A RID: 410
		// (get) Token: 0x06000749 RID: 1865 RVA: 0x0000FDF5 File Offset: 0x0000EDF5
		// (set) Token: 0x0600074A RID: 1866 RVA: 0x00003AE9 File Offset: 0x00002AE9
		[SuppressMessage("Blocker Code Smell", "S3237:\"value\" parameters should be used", Justification = "Setter cannot be changed for compatibility reasons")]
		public override double RealValue
		{
			get
			{
				return 0.0;
			}
			set
			{
			}
		}

		// Token: 0x1700019B RID: 411
		// (get) Token: 0x0600074B RID: 1867 RVA: 0x000129D7 File Offset: 0x000119D7
		// (set) Token: 0x0600074C RID: 1868 RVA: 0x000129DF File Offset: 0x000119DF
		public override TypeClass ConstantType
		{
			get
			{
				return this.m_type;
			}
			set
			{
				this.m_type = value;
			}
		}

		// Token: 0x0600074D RID: 1869 RVA: 0x000129E8 File Offset: 0x000119E8
		public override _IExprement Duplicate()
		{
			StringLiteralExpression stringLiteralExpression = new StringLiteralExpression();
			this.DuplicateCommon(stringLiteralExpression);
			stringLiteralExpression.m_stValue = this.m_stValue;
			stringLiteralExpression.m_type = this.m_type;
			stringLiteralExpression.m_tcOriginal = this.m_tcOriginal;
			stringLiteralExpression.m_stringEncoding = this.m_stringEncoding;
			return stringLiteralExpression;
		}

		// Token: 0x1700019C RID: 412
		// (get) Token: 0x0600074E RID: 1870 RVA: 0x00012A33 File Offset: 0x00011A33
		// (set) Token: 0x0600074F RID: 1871 RVA: 0x00012A3B File Offset: 0x00011A3B
		public StringEncoding StringEncoding
		{
			get
			{
				return this.m_stringEncoding;
			}
			set
			{
				this.m_stringEncoding = value;
			}
		}

		// Token: 0x040000F6 RID: 246
		[DefaultSerialization("TypeClass")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private TypeClass m_type = TypeClass.None;

		// Token: 0x040000F7 RID: 247
		[DefaultSerialization("StringValue")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private string m_stValue = string.Empty;

		// Token: 0x040000F8 RID: 248
		[DefaultSerialization("StringEncoding")]
		[StorageVersion("3.5.18.0")]
		[StorageDefaultValue(StringEncoding.Default)]
		[Obfuscation(Feature = "rename")]
		private StringEncoding m_stringEncoding;
	}
}
