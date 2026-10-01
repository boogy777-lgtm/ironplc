using System;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000077 RID: 119
	[TypeGuid("{f4babf9e-33d0-43da-9e6b-d20d349b9868}")]
	[StorageVersion("3.3.0.0")]
	public class TypeReference : ItemReference, _ITypeReference, _IItemReference, _IPragmaExpression, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression, ITypeReference2, ITypeReference
	{
		// Token: 0x0600079E RID: 1950 RVA: 0x0000E359 File Offset: 0x0000D359
		public TypeReference()
		{
		}

		// Token: 0x0600079F RID: 1951 RVA: 0x0000E361 File Offset: 0x0000D361
		public TypeReference(IToken token) : base(token)
		{
		}

		// Token: 0x060007A0 RID: 1952 RVA: 0x00012FDF File Offset: 0x00011FDF
		public TypeReference(IToken token, _IExpression expPath) : base(token)
		{
			this.m_expPath = expPath;
		}

		// Token: 0x170001AC RID: 428
		// (get) Token: 0x060007A1 RID: 1953 RVA: 0x00012FEF File Offset: 0x00011FEF
		// (set) Token: 0x060007A2 RID: 1954 RVA: 0x00012FF7 File Offset: 0x00011FF7
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

		// Token: 0x170001AD RID: 429
		// (get) Token: 0x060007A3 RID: 1955 RVA: 0x00005F0F File Offset: 0x00004F0F
		public IQualifiedNameExpression Instance
		{
			get
			{
				return null;
			}
		}

		// Token: 0x170001AE RID: 430
		// (get) Token: 0x060007A4 RID: 1956 RVA: 0x00012FEF File Offset: 0x00011FEF
		public IExpression InstanceExpression
		{
			get
			{
				return this.m_expPath;
			}
		}

		// Token: 0x060007A5 RID: 1957 RVA: 0x00013000 File Offset: 0x00012000
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x060007A6 RID: 1958 RVA: 0x00013009 File Offset: 0x00012009
		public override T Accept<T>(IExprementVisitor<T> visitor)
		{
			return visitor.visit(this);
		}

		// Token: 0x060007A7 RID: 1959 RVA: 0x00013012 File Offset: 0x00012012
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x060007A8 RID: 1960 RVA: 0x00004E6B File Offset: 0x00003E6B
		public override bool IsLValue(IScope scope, bool bWriteToConstants)
		{
			return false;
		}

		// Token: 0x060007A9 RID: 1961 RVA: 0x0001301C File Offset: 0x0001201C
		public override ISignature GetSignature(IScope scope)
		{
			ISignature[] array = (scope as IScope2).FindSignature(this.InstanceExpression);
			if (array == null || array.Length != 1)
			{
				return null;
			}
			return array[0];
		}

		// Token: 0x060007AA RID: 1962 RVA: 0x0001304C File Offset: 0x0001204C
		public override bool HasAttribute(string stAttribute, IScope scope)
		{
			ISignature signature = this.GetSignature(scope);
			return signature != null && signature.HasAttribute(stAttribute);
		}

		// Token: 0x060007AB RID: 1963 RVA: 0x00013070 File Offset: 0x00012070
		public override bool HasAttribute(string stAttribute, IPrecompileScope2 scope)
		{
			ISignature signature = scope.FindSignatureGlobal(this.InstanceExpression);
			return signature != null && signature.HasAttribute(stAttribute);
		}

		// Token: 0x060007AC RID: 1964 RVA: 0x0001309C File Offset: 0x0001209C
		public override _IExprement Duplicate()
		{
			TypeReference typeReference = new TypeReference();
			if (this.m_expPath != null)
			{
				typeReference.m_expPath = (this.m_expPath.Duplicate() as Expression);
			}
			this.DuplicateCommon(typeReference);
			return typeReference;
		}

		// Token: 0x04000102 RID: 258
		[DefaultSerialization("Qualiname")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private _IExpression m_expPath;
	}
}
