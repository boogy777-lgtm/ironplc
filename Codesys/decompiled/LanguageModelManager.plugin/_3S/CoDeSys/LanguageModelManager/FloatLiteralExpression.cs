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
	// Token: 0x02000051 RID: 81
	[TypeGuid("{fbc31a5d-5b51-405e-b986-43250b86c9f7}")]
	[StorageVersion("3.3.0.0")]
	public class FloatLiteralExpression : LiteralExpression, _IFloatLiteralExpression, _ILiteralExpression, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression, ILiteralExpression2, ILiteralExpression
	{
		// Token: 0x060004C9 RID: 1225 RVA: 0x0000E83E File Offset: 0x0000D83E
		public FloatLiteralExpression()
		{
		}

		// Token: 0x060004CA RID: 1226 RVA: 0x0000E84E File Offset: 0x0000D84E
		public FloatLiteralExpression(double dVal, TypeClass tc)
		{
			this.m_dValue = dVal;
			this.m_type = tc;
			this.m_tcOriginal = tc;
		}

		// Token: 0x060004CB RID: 1227 RVA: 0x0000E873 File Offset: 0x0000D873
		public FloatLiteralExpression(double dVal, TypeClass tc, IToken token) : base(token)
		{
			this.m_dValue = dVal;
			this.m_type = tc;
			this.m_tcOriginal = tc;
		}

		// Token: 0x060004CC RID: 1228 RVA: 0x0000E899 File Offset: 0x0000D899
		public FloatLiteralExpression(double dVal)
		{
			this.m_dValue = dVal;
			this.m_type = TypeClass.AnyReal;
			this.m_tcOriginal = TypeClass.None;
		}

		// Token: 0x170000FE RID: 254
		// (get) Token: 0x060004CD RID: 1229 RVA: 0x0000E8C0 File Offset: 0x0000D8C0
		// (set) Token: 0x060004CE RID: 1230 RVA: 0x00003AE9 File Offset: 0x00002AE9
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

		// Token: 0x170000FF RID: 255
		// (get) Token: 0x060004CF RID: 1231 RVA: 0x00004E6B File Offset: 0x00003E6B
		// (set) Token: 0x060004D0 RID: 1232 RVA: 0x00003AE9 File Offset: 0x00002AE9
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

		// Token: 0x17000100 RID: 256
		// (get) Token: 0x060004D1 RID: 1233 RVA: 0x0000E8C0 File Offset: 0x0000D8C0
		public override ulong ULongValue
		{
			get
			{
				return 0UL;
			}
		}

		// Token: 0x17000101 RID: 257
		// (get) Token: 0x060004D2 RID: 1234 RVA: 0x0000E8C4 File Offset: 0x0000D8C4
		// (set) Token: 0x060004D3 RID: 1235 RVA: 0x00003AE9 File Offset: 0x00002AE9
		[SuppressMessage("Blocker Code Smell", "S3237:\"value\" parameters should be used", Justification = "Setter cannot be changed for compatibility reasons")]
		public override string StringValue
		{
			get
			{
				return string.Empty;
			}
			set
			{
			}
		}

		// Token: 0x17000102 RID: 258
		// (get) Token: 0x060004D4 RID: 1236 RVA: 0x0000E8CB File Offset: 0x0000D8CB
		// (set) Token: 0x060004D5 RID: 1237 RVA: 0x0000E8D3 File Offset: 0x0000D8D3
		public override double RealValue
		{
			get
			{
				return this.m_dValue;
			}
			set
			{
				this.m_dValue = value;
			}
		}

		// Token: 0x17000103 RID: 259
		// (get) Token: 0x060004D6 RID: 1238 RVA: 0x0000E8DC File Offset: 0x0000D8DC
		// (set) Token: 0x060004D7 RID: 1239 RVA: 0x0000E8E4 File Offset: 0x0000D8E4
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

		// Token: 0x060004D8 RID: 1240 RVA: 0x0000E8F0 File Offset: 0x0000D8F0
		public override _IExprement Duplicate()
		{
			FloatLiteralExpression floatLiteralExpression = new FloatLiteralExpression();
			this.DuplicateCommon(floatLiteralExpression);
			floatLiteralExpression.m_dValue = this.m_dValue;
			floatLiteralExpression.m_type = this.m_type;
			floatLiteralExpression.m_tcOriginal = this.m_tcOriginal;
			return floatLiteralExpression;
		}

		// Token: 0x040000B2 RID: 178
		[DefaultSerialization("TypeClass")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private TypeClass m_type = TypeClass.None;

		// Token: 0x040000B3 RID: 179
		[DefaultSerialization("FloatValue")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private double m_dValue;
	}
}
