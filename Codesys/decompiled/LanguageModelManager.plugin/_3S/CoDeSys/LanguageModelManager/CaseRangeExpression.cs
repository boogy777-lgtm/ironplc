using System;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000045 RID: 69
	[TypeGuid("{f99ece66-24f7-416b-bbf8-0a776dae6d28}")]
	[StorageVersion("3.3.0.0")]
	public class CaseRangeExpression : PositionExpression, _ICaseRangeExpression, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression, ICaseRangeExpression
	{
		// Token: 0x060003AC RID: 940 RVA: 0x0000B408 File Offset: 0x0000A408
		public CaseRangeExpression()
		{
		}

		// Token: 0x060003AD RID: 941 RVA: 0x0000C47A File Offset: 0x0000B47A
		internal CaseRangeExpression(_IExpression expLow, _IExpression expHigh)
		{
			this.m_expLow = expLow;
			this.m_expHigh = expHigh;
		}

		// Token: 0x060003AE RID: 942 RVA: 0x0000C490 File Offset: 0x0000B490
		internal CaseRangeExpression(_IExpression expLow, _IExpression expHigh, IToken token) : base(token)
		{
			this.m_expLow = expLow;
			this.m_expHigh = expHigh;
		}

		// Token: 0x060003AF RID: 943 RVA: 0x00004E6B File Offset: 0x00003E6B
		public override bool IsLValue(IScope scope, bool bWriteToConstants)
		{
			return false;
		}

		// Token: 0x170000B8 RID: 184
		// (get) Token: 0x060003B0 RID: 944 RVA: 0x0000C4A7 File Offset: 0x0000B4A7
		public IExpression Low
		{
			get
			{
				return this._Low;
			}
		}

		// Token: 0x170000B9 RID: 185
		// (get) Token: 0x060003B1 RID: 945 RVA: 0x0000C4AF File Offset: 0x0000B4AF
		// (set) Token: 0x060003B2 RID: 946 RVA: 0x0000C4C5 File Offset: 0x0000B4C5
		public _IExpression _Low
		{
			get
			{
				if (this.m_expLow == null)
				{
					return new NullExpression();
				}
				return this.m_expLow;
			}
			set
			{
				this.m_expLow = value;
			}
		}

		// Token: 0x170000BA RID: 186
		// (get) Token: 0x060003B3 RID: 947 RVA: 0x0000C4CE File Offset: 0x0000B4CE
		public IExpression High
		{
			get
			{
				return this._High;
			}
		}

		// Token: 0x170000BB RID: 187
		// (get) Token: 0x060003B4 RID: 948 RVA: 0x0000C4D6 File Offset: 0x0000B4D6
		// (set) Token: 0x060003B5 RID: 949 RVA: 0x0000C4EC File Offset: 0x0000B4EC
		public _IExpression _High
		{
			get
			{
				if (this.m_expHigh == null)
				{
					return new NullExpression();
				}
				return this.m_expHigh;
			}
			set
			{
				this.m_expHigh = value;
			}
		}

		// Token: 0x060003B6 RID: 950 RVA: 0x0000C4F5 File Offset: 0x0000B4F5
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x060003B7 RID: 951 RVA: 0x0000C4FE File Offset: 0x0000B4FE
		public override T Accept<T>(IExprementVisitor<T> visitor)
		{
			return visitor.visit(this);
		}

		// Token: 0x060003B8 RID: 952 RVA: 0x0000C507 File Offset: 0x0000B507
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x060003B9 RID: 953 RVA: 0x0000C510 File Offset: 0x0000B510
		public override ISourcePosition GetPosition()
		{
			return this._Low.GetPosition();
		}

		// Token: 0x060003BA RID: 954 RVA: 0x0000C520 File Offset: 0x0000B520
		public override _IExprement Duplicate()
		{
			CaseRangeExpression caseRangeExpression = new CaseRangeExpression();
			this.DuplicateCommon(caseRangeExpression);
			if (this.m_expLow != null)
			{
				caseRangeExpression.m_expLow = (this.m_expLow.Duplicate() as Expression);
			}
			if (this.m_expHigh != null)
			{
				caseRangeExpression.m_expHigh = (this.m_expHigh.Duplicate() as Expression);
			}
			return caseRangeExpression;
		}

		// Token: 0x170000BC RID: 188
		// (get) Token: 0x060003BB RID: 955 RVA: 0x0000C577 File Offset: 0x0000B577
		// (set) Token: 0x060003BC RID: 956 RVA: 0x0000C57F File Offset: 0x0000B57F
		[DefaultSerialization("Type")]
		[StorageVersion("3.3.0.10")]
		[StorageIgnorable]
		[Obfuscation(Feature = "rename")]
		public override ICompiledType _CompiledType
		{
			get
			{
				return this.m_ctype;
			}
			set
			{
				this.m_ctype = value;
			}
		}

		// Token: 0x04000092 RID: 146
		[DefaultSerialization("Low")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private _IExpression m_expLow;

		// Token: 0x04000093 RID: 147
		[DefaultSerialization("High")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private _IExpression m_expHigh;

		// Token: 0x04000094 RID: 148
		[Obfuscation(Feature = "rename")]
		[DefaultSerialization("compiledtype")]
		[StorageVersion("3.3.0.0")]
		private ICompiledType m_ctype;
	}
}
