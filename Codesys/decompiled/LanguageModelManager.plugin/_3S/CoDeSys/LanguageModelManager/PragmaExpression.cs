using System;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x0200006B RID: 107
	public abstract class PragmaExpression : PositionExpression, _IPragmaExpression, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression
	{
		// Token: 0x060006ED RID: 1773 RVA: 0x0000B408 File Offset: 0x0000A408
		protected PragmaExpression()
		{
		}

		// Token: 0x060006EE RID: 1774 RVA: 0x0000B9FB File Offset: 0x0000A9FB
		protected PragmaExpression(IToken token) : base(token)
		{
		}

		// Token: 0x17000184 RID: 388
		// (get) Token: 0x060006EF RID: 1775 RVA: 0x000123B9 File Offset: 0x000113B9
		// (set) Token: 0x060006F0 RID: 1776 RVA: 0x000123C1 File Offset: 0x000113C1
		public bool Value
		{
			get
			{
				return this.m_bValue;
			}
			set
			{
				this.m_bValue = value;
			}
		}

		// Token: 0x17000185 RID: 389
		// (get) Token: 0x060006F1 RID: 1777 RVA: 0x000123CA File Offset: 0x000113CA
		// (set) Token: 0x060006F2 RID: 1778 RVA: 0x000123D2 File Offset: 0x000113D2
		public bool ValueStillUndecided
		{
			get
			{
				return this.m_bValueStillUndecided;
			}
			set
			{
				this.m_bValueStillUndecided = value;
			}
		}

		// Token: 0x17000186 RID: 390
		// (get) Token: 0x060006F3 RID: 1779 RVA: 0x000123DB File Offset: 0x000113DB
		// (set) Token: 0x060006F4 RID: 1780 RVA: 0x000123E3 File Offset: 0x000113E3
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

		// Token: 0x040000E9 RID: 233
		[DefaultSerialization("Value")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private bool m_bValue;

		// Token: 0x040000EA RID: 234
		[DefaultSerialization("ValueStillUndecided")]
		[StorageVersion("3.5.9.0")]
		[StorageDefaultValue(false)]
		[Obfuscation(Feature = "rename")]
		private bool m_bValueStillUndecided;

		// Token: 0x040000EB RID: 235
		[Obfuscation(Feature = "rename")]
		[DefaultSerialization("compiledtype")]
		[StorageVersion("3.3.0.0")]
		private ICompiledType m_ctype;
	}
}
