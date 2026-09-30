using System;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000040 RID: 64
	[TypeGuid("{b5bdc032-52f5-48d3-9c8c-8c79ab5a416d}")]
	[StorageVersion("3.3.0.0")]
	public class BasedIntegerLiteralExpression : IntegerLiteralExpression
	{
		// Token: 0x06000339 RID: 825 RVA: 0x0000B922 File Offset: 0x0000A922
		public BasedIntegerLiteralExpression()
		{
		}

		// Token: 0x0600033A RID: 826 RVA: 0x0000B932 File Offset: 0x0000A932
		internal BasedIntegerLiteralExpression(long lVal, TypeClass tc, int nBase) : this(lVal, tc, nBase, false)
		{
		}

		// Token: 0x0600033B RID: 827 RVA: 0x0000B93E File Offset: 0x0000A93E
		internal BasedIntegerLiteralExpression(long lVal, TypeClass tc, int nBase, bool negative) : base(lVal, tc)
		{
			this.m_nBase = nBase;
			this.Negative = negative;
		}

		// Token: 0x0600033C RID: 828 RVA: 0x0000B95F File Offset: 0x0000A95F
		internal BasedIntegerLiteralExpression(long lVal, TypeClass tc, IToken token, int nBase) : this(lVal, tc, token, nBase, false)
		{
		}

		// Token: 0x0600033D RID: 829 RVA: 0x0000B96D File Offset: 0x0000A96D
		internal BasedIntegerLiteralExpression(long lVal, TypeClass tc, IToken token, int nBase, bool negative) : base(lVal, tc, token)
		{
			this.m_nBase = nBase;
			this.Negative = negative;
		}

		// Token: 0x17000092 RID: 146
		// (get) Token: 0x0600033E RID: 830 RVA: 0x0000B990 File Offset: 0x0000A990
		// (set) Token: 0x0600033F RID: 831 RVA: 0x0000B998 File Offset: 0x0000A998
		public override int Base
		{
			get
			{
				return this.m_nBase;
			}
			set
			{
				this.m_nBase = value;
			}
		}

		// Token: 0x06000340 RID: 832 RVA: 0x0000B9A4 File Offset: 0x0000A9A4
		public override _IExprement Duplicate()
		{
			BasedIntegerLiteralExpression basedIntegerLiteralExpression = new BasedIntegerLiteralExpression();
			this.DuplicateCommon(basedIntegerLiteralExpression);
			basedIntegerLiteralExpression.m_lValue = this.m_lValue;
			basedIntegerLiteralExpression.m_type = this.m_type;
			basedIntegerLiteralExpression.m_tcOriginal = this.m_tcOriginal;
			basedIntegerLiteralExpression.m_bNegative = this.m_bNegative;
			basedIntegerLiteralExpression.m_nBase = this.m_nBase;
			return basedIntegerLiteralExpression;
		}

		// Token: 0x0400007C RID: 124
		[DefaultSerialization("Base")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		protected int m_nBase = 10;
	}
}
