using System;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x0200005A RID: 90
	[TypeGuid("{a2602515-7c10-47cd-a6b3-8e2421319a44}")]
	[StorageVersion("3.3.0.0")]
	public class HasValueExpression : PragmaExpression, _IHasValueExpression, _IPragmaExpression, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression, IHasValueExpression
	{
		// Token: 0x17000124 RID: 292
		// (get) Token: 0x06000555 RID: 1365 RVA: 0x0000F113 File Offset: 0x0000E113
		// (set) Token: 0x06000556 RID: 1366 RVA: 0x0000F11B File Offset: 0x0000E11B
		[DefaultSerialization("ValueString")]
		[StorageVersion("3.5.20.0")]
		[Obfuscation(Feature = "rename")]
		[StorageDefaultValue(null)]
		private string ValueString
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

		// Token: 0x06000557 RID: 1367 RVA: 0x0000C6EA File Offset: 0x0000B6EA
		public HasValueExpression()
		{
		}

		// Token: 0x06000558 RID: 1368 RVA: 0x0000C6F2 File Offset: 0x0000B6F2
		public HasValueExpression(IToken token) : base(token)
		{
		}

		// Token: 0x06000559 RID: 1369 RVA: 0x0000F124 File Offset: 0x0000E124
		public HasValueExpression(IToken token, string stDefine, string stValue) : base(token)
		{
			this.m_stDefine = stDefine;
			this.m_stValue = stValue;
		}

		// Token: 0x17000125 RID: 293
		// (get) Token: 0x0600055A RID: 1370 RVA: 0x0000F13B File Offset: 0x0000E13B
		// (set) Token: 0x0600055B RID: 1371 RVA: 0x0000F143 File Offset: 0x0000E143
		public string Define
		{
			get
			{
				return this.m_stDefine;
			}
			set
			{
				this.m_stDefine = value;
			}
		}

		// Token: 0x17000126 RID: 294
		// (get) Token: 0x0600055C RID: 1372 RVA: 0x0000F113 File Offset: 0x0000E113
		// (set) Token: 0x0600055D RID: 1373 RVA: 0x0000F11B File Offset: 0x0000E11B
		public string DefineValue
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

		// Token: 0x0600055E RID: 1374 RVA: 0x0000F14C File Offset: 0x0000E14C
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x0600055F RID: 1375 RVA: 0x0000F155 File Offset: 0x0000E155
		public override T Accept<T>(IExprementVisitor<T> visitor)
		{
			return visitor.visit(this);
		}

		// Token: 0x06000560 RID: 1376 RVA: 0x0000F15E File Offset: 0x0000E15E
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x06000561 RID: 1377 RVA: 0x00004E6B File Offset: 0x00003E6B
		public override bool IsLValue(IScope scope, bool bWriteToConstants)
		{
			return false;
		}

		// Token: 0x06000562 RID: 1378 RVA: 0x0000F168 File Offset: 0x0000E168
		public override _IExprement Duplicate()
		{
			HasValueExpression hasValueExpression = new HasValueExpression();
			this.DuplicateCommon(hasValueExpression);
			hasValueExpression.m_stDefine = this.m_stDefine;
			hasValueExpression.m_stValue = this.m_stValue;
			return hasValueExpression;
		}

		// Token: 0x040000C1 RID: 193
		[DefaultSerialization("Define")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private string m_stDefine;

		// Token: 0x040000C2 RID: 194
		[DefaultSerialization("Value")]
		[StorageVersion("3.3.0.0-3.5.19.99")]
		[Obfuscation(Feature = "rename")]
		[StorageDefaultValue(null)]
		private string m_stValue;
	}
}
