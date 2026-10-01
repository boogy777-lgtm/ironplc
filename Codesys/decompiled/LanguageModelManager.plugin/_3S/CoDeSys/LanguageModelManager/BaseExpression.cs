using System;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000041 RID: 65
	[TypeGuid("{c2eeddeb-b307-4edb-87b6-5b334e187ae2}")]
	[StorageVersion("3.3.0.0")]
	public class BaseExpression : PositionExpression, _IBaseExpression, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression, IBaseExpression
	{
		// Token: 0x06000341 RID: 833 RVA: 0x0000B408 File Offset: 0x0000A408
		public BaseExpression()
		{
		}

		// Token: 0x06000342 RID: 834 RVA: 0x0000B9FB File Offset: 0x0000A9FB
		public BaseExpression(IToken token) : base(token)
		{
		}

		// Token: 0x06000343 RID: 835 RVA: 0x00004E6B File Offset: 0x00003E6B
		public override bool IsLValue(IScope scope, bool bWriteToConstants)
		{
			return false;
		}

		// Token: 0x06000344 RID: 836 RVA: 0x0000BA04 File Offset: 0x0000AA04
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x06000345 RID: 837 RVA: 0x0000BA0D File Offset: 0x0000AA0D
		public override T Accept<T>(IExprementVisitor<T> visitor)
		{
			return visitor.visit(this);
		}

		// Token: 0x06000346 RID: 838 RVA: 0x0000BA16 File Offset: 0x0000AA16
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x06000347 RID: 839 RVA: 0x0000BA20 File Offset: 0x0000AA20
		public override _IExprement Duplicate()
		{
			BaseExpression baseExpression = new BaseExpression();
			this.DuplicateCommon(baseExpression);
			return baseExpression;
		}

		// Token: 0x17000093 RID: 147
		// (get) Token: 0x06000348 RID: 840 RVA: 0x0000BA3B File Offset: 0x0000AA3B
		// (set) Token: 0x06000349 RID: 841 RVA: 0x0000BA43 File Offset: 0x0000AA43
		public override IExprInfo Info
		{
			get
			{
				return this.m_expInfo;
			}
			set
			{
				this.m_expInfo = value;
			}
		}

		// Token: 0x0600034A RID: 842 RVA: 0x0000BA4C File Offset: 0x0000AA4C
		public override ISignature GetSignature(IScope scope)
		{
			if (scope.LocalSignature == null)
			{
				return base.GetSignature(scope);
			}
			return scope[scope.LocalSignature.BaseSignatureId];
		}

		// Token: 0x17000094 RID: 148
		// (get) Token: 0x0600034B RID: 843 RVA: 0x0000BA6F File Offset: 0x0000AA6F
		// (set) Token: 0x0600034C RID: 844 RVA: 0x0000BA77 File Offset: 0x0000AA77
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

		// Token: 0x0400007D RID: 125
		[Obfuscation(Feature = "rename")]
		[DefaultSerialization("compiledtype")]
		[StorageVersion("3.3.0.0")]
		private ICompiledType m_ctype;

		// Token: 0x0400007E RID: 126
		[Obfuscation(Feature = "rename")]
		private IExprInfo m_expInfo;
	}
}
