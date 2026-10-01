using System;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x0200006A RID: 106
	[TypeGuid("{3f8c7ea4-5a17-4473-90f4-c728f72004b9}")]
	[StorageVersion("3.3.0.0")]
	public class PouReference : ItemReference, _IPouReference, _IItemReference, _IPragmaExpression, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression, IPouReference2, IPouReference
	{
		// Token: 0x060006DE RID: 1758 RVA: 0x0000E359 File Offset: 0x0000D359
		public PouReference()
		{
		}

		// Token: 0x060006DF RID: 1759 RVA: 0x0000E361 File Offset: 0x0000D361
		public PouReference(IToken token) : base(token)
		{
		}

		// Token: 0x060006E0 RID: 1760 RVA: 0x000122C4 File Offset: 0x000112C4
		public PouReference(IToken token, _IExpression expPath) : base(token)
		{
			this.m_expPath = expPath;
		}

		// Token: 0x17000181 RID: 385
		// (get) Token: 0x060006E1 RID: 1761 RVA: 0x000122D4 File Offset: 0x000112D4
		// (set) Token: 0x060006E2 RID: 1762 RVA: 0x000122DC File Offset: 0x000112DC
		public _IExpression InstancePath
		{
			get
			{
				return this.m_expPath;
			}
			set
			{
				this.m_expPath = value;
			}
		}

		// Token: 0x17000182 RID: 386
		// (get) Token: 0x060006E3 RID: 1763 RVA: 0x00005F0F File Offset: 0x00004F0F
		public IQualifiedNameExpression Instance
		{
			get
			{
				return null;
			}
		}

		// Token: 0x17000183 RID: 387
		// (get) Token: 0x060006E4 RID: 1764 RVA: 0x000122D4 File Offset: 0x000112D4
		public IExpression InstanceExpression
		{
			get
			{
				return this.m_expPath;
			}
		}

		// Token: 0x060006E5 RID: 1765 RVA: 0x000122E5 File Offset: 0x000112E5
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x060006E6 RID: 1766 RVA: 0x000122EE File Offset: 0x000112EE
		public override T Accept<T>(IExprementVisitor<T> visitor)
		{
			return visitor.visit(this);
		}

		// Token: 0x060006E7 RID: 1767 RVA: 0x000122F7 File Offset: 0x000112F7
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x060006E8 RID: 1768 RVA: 0x00004E6B File Offset: 0x00003E6B
		public override bool IsLValue(IScope scope, bool bWriteToConstants)
		{
			return false;
		}

		// Token: 0x060006E9 RID: 1769 RVA: 0x00012300 File Offset: 0x00011300
		public override ISignature GetSignature(IScope scope)
		{
			ISignature[] array = (scope as IScope2).FindSignature(this.InstanceExpression);
			if (array == null || array.Length != 1)
			{
				return null;
			}
			return array[0];
		}

		// Token: 0x060006EA RID: 1770 RVA: 0x00012330 File Offset: 0x00011330
		public override bool HasAttribute(string stAttribute, IScope scope)
		{
			ISignature signature = this.GetSignature(scope);
			return signature != null && signature.HasAttribute(stAttribute);
		}

		// Token: 0x060006EB RID: 1771 RVA: 0x00012354 File Offset: 0x00011354
		public override bool HasAttribute(string stAttribute, IPrecompileScope2 scope)
		{
			ISignature signature = scope.FindSignatureGlobal(this.InstanceExpression);
			return signature != null && signature.HasAttribute(stAttribute);
		}

		// Token: 0x060006EC RID: 1772 RVA: 0x00012380 File Offset: 0x00011380
		public override _IExprement Duplicate()
		{
			PouReference pouReference = new PouReference();
			if (this.m_expPath != null)
			{
				pouReference.m_expPath = (this.m_expPath.Duplicate() as Expression);
			}
			this.DuplicateCommon(pouReference);
			return pouReference;
		}

		// Token: 0x040000E8 RID: 232
		[DefaultSerialization("Qualiname")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private _IExpression m_expPath;
	}
}
