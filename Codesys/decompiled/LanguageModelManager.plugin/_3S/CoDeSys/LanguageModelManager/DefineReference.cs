using System;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x0200004D RID: 77
	[TypeGuid("{681c77be-49a6-4fa6-94e0-f6ecb3adc8cd}")]
	[StorageVersion("3.3.0.0")]
	public class DefineReference : ItemReference, _IDefineReference, _IItemReference, _IPragmaExpression, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression, IDefineReference
	{
		// Token: 0x06000471 RID: 1137 RVA: 0x0000E359 File Offset: 0x0000D359
		public DefineReference()
		{
		}

		// Token: 0x06000472 RID: 1138 RVA: 0x0000E361 File Offset: 0x0000D361
		public DefineReference(IToken token) : base(token)
		{
		}

		// Token: 0x06000473 RID: 1139 RVA: 0x0000E36A File Offset: 0x0000D36A
		public DefineReference(IToken token, string stDefine) : base(token)
		{
			this.m_stDefine = stDefine;
		}

		// Token: 0x170000E8 RID: 232
		// (get) Token: 0x06000474 RID: 1140 RVA: 0x0000E37A File Offset: 0x0000D37A
		// (set) Token: 0x06000475 RID: 1141 RVA: 0x0000E382 File Offset: 0x0000D382
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

		// Token: 0x06000476 RID: 1142 RVA: 0x0000E38B File Offset: 0x0000D38B
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x06000477 RID: 1143 RVA: 0x0000E394 File Offset: 0x0000D394
		public override T Accept<T>(IExprementVisitor<T> visitor)
		{
			return visitor.visit(this);
		}

		// Token: 0x06000478 RID: 1144 RVA: 0x0000E39D File Offset: 0x0000D39D
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x06000479 RID: 1145 RVA: 0x00004E6B File Offset: 0x00003E6B
		public override bool IsLValue(IScope scope, bool bWriteToConstants)
		{
			return false;
		}

		// Token: 0x0600047A RID: 1146 RVA: 0x0000E3A8 File Offset: 0x0000D3A8
		public override _IExprement Duplicate()
		{
			DefineReference defineReference = new DefineReference();
			defineReference.m_stDefine = this.m_stDefine;
			this.DuplicateCommon(defineReference);
			return defineReference;
		}

		// Token: 0x040000AD RID: 173
		[DefaultSerialization("Define")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private string m_stDefine;
	}
}
