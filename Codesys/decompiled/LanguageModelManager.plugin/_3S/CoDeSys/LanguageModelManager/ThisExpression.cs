using System;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000075 RID: 117
	[TypeGuid("{a40f0567-c9d1-457f-86ff-9de3a1f69954}")]
	[StorageVersion("3.3.0.0")]
	public class ThisExpression : PositionExpression, _IThisExpression, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression, IThisExpression
	{
		// Token: 0x06000787 RID: 1927 RVA: 0x0000B408 File Offset: 0x0000A408
		public ThisExpression()
		{
		}

		// Token: 0x06000788 RID: 1928 RVA: 0x0000B9FB File Offset: 0x0000A9FB
		public ThisExpression(IToken token) : base(token)
		{
		}

		// Token: 0x06000789 RID: 1929 RVA: 0x00004E6B File Offset: 0x00003E6B
		public override bool IsLValue(IScope scope, bool bWriteToConstants)
		{
			return false;
		}

		// Token: 0x0600078A RID: 1930 RVA: 0x00012EE3 File Offset: 0x00011EE3
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x0600078B RID: 1931 RVA: 0x00012EEC File Offset: 0x00011EEC
		public override T Accept<T>(IExprementVisitor<T> visitor)
		{
			return visitor.visit(this);
		}

		// Token: 0x0600078C RID: 1932 RVA: 0x00012EF5 File Offset: 0x00011EF5
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x0600078D RID: 1933 RVA: 0x00012F00 File Offset: 0x00011F00
		public override _IExprement Duplicate()
		{
			ThisExpression thisExpression = new ThisExpression();
			this.DuplicateCommon(thisExpression);
			return thisExpression;
		}

		// Token: 0x170001A8 RID: 424
		// (get) Token: 0x0600078E RID: 1934 RVA: 0x00012F1B File Offset: 0x00011F1B
		// (set) Token: 0x0600078F RID: 1935 RVA: 0x00012F23 File Offset: 0x00011F23
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

		// Token: 0x06000790 RID: 1936 RVA: 0x00012F2C File Offset: 0x00011F2C
		public override ISignature GetSignature(IScope scope)
		{
			return scope.LocalSignature;
		}

		// Token: 0x170001A9 RID: 425
		// (get) Token: 0x06000791 RID: 1937 RVA: 0x00012F34 File Offset: 0x00011F34
		// (set) Token: 0x06000792 RID: 1938 RVA: 0x00012F3C File Offset: 0x00011F3C
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

		// Token: 0x040000FF RID: 255
		[Obfuscation(Feature = "rename")]
		[DefaultSerialization("compiledtype")]
		[StorageVersion("3.3.0.0")]
		private ICompiledType m_ctype;

		// Token: 0x04000100 RID: 256
		[Obfuscation(Feature = "rename")]
		private IExprInfo m_expInfo;
	}
}
