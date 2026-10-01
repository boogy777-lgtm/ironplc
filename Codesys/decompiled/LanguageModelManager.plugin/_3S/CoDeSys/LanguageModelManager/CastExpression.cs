using System;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000046 RID: 70
	[TypeGuid("{1EDCDC92-5054-47e9-B1D3-BF78BAF29A3E}")]
	[StorageVersion("3.4.0.0")]
	public class CastExpression : PositionExpression, _ICastExpression, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression, ICastExpression
	{
		// Token: 0x060003BD RID: 957 RVA: 0x0000B408 File Offset: 0x0000A408
		public CastExpression()
		{
		}

		// Token: 0x060003BE RID: 958 RVA: 0x0000C588 File Offset: 0x0000B588
		public CastExpression(_IExpression expWithType, _IExpression expBase)
		{
			this.m_expWithType = expWithType;
			this.m_expBase = expBase;
			this.PositionIntern = MinimalPosition.CreateMinimalPosition(0L, 0);
		}

		// Token: 0x060003BF RID: 959 RVA: 0x0000C5AC File Offset: 0x0000B5AC
		public override bool IsLValue(IScope scope, bool bWriteToConstants)
		{
			return this.m_expBase.IsLValue(scope, bWriteToConstants);
		}

		// Token: 0x060003C0 RID: 960 RVA: 0x0000C5BB File Offset: 0x0000B5BB
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x060003C1 RID: 961 RVA: 0x0000C5C4 File Offset: 0x0000B5C4
		public override T Accept<T>(IExprementVisitor<T> visitor)
		{
			return visitor.visit(this);
		}

		// Token: 0x060003C2 RID: 962 RVA: 0x0000C5CD File Offset: 0x0000B5CD
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			IExprVisitor4 exprVisitor = visitor as IExprVisitor4;
			if (exprVisitor == null)
			{
				return;
			}
			exprVisitor.visit(this);
		}

		// Token: 0x060003C3 RID: 963 RVA: 0x0000C5E0 File Offset: 0x0000B5E0
		public override IVariable GetVariable(IScope scope)
		{
			return this.m_expBase.GetVariable(scope);
		}

		// Token: 0x060003C4 RID: 964 RVA: 0x0000C5F0 File Offset: 0x0000B5F0
		public override ISignature GetSignature(IScope scope)
		{
			UserdefType userdefType;
			if (this.m_expWithType == null)
			{
				userdefType = (this.m_type as UserdefType);
			}
			else
			{
				userdefType = (this.m_expWithType._CompiledType as UserdefType);
			}
			if (userdefType == null)
			{
				return base.GetSignature(scope);
			}
			return userdefType.GetSignature(scope);
		}

		// Token: 0x060003C5 RID: 965 RVA: 0x0000C638 File Offset: 0x0000B638
		public override _IExprement Duplicate()
		{
			_IExpression expWithType = null;
			if (this.m_expWithType != null)
			{
				expWithType = (this.m_expWithType.Duplicate() as _IExpression);
			}
			CastExpression castExpression = new CastExpression(expWithType, this.m_expBase.Duplicate() as _IExpression);
			castExpression.m_type = this.m_type;
			this.DuplicateCommon(castExpression);
			return castExpression;
		}

		// Token: 0x170000BD RID: 189
		// (get) Token: 0x060003C6 RID: 966 RVA: 0x0000C68B File Offset: 0x0000B68B
		// (set) Token: 0x060003C7 RID: 967 RVA: 0x0000C693 File Offset: 0x0000B693
		[DefaultSerialization("Type")]
		[StorageVersion("3.4.0.0")]
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

		// Token: 0x170000BE RID: 190
		// (get) Token: 0x060003C8 RID: 968 RVA: 0x0000C69C File Offset: 0x0000B69C
		// (set) Token: 0x060003C9 RID: 969 RVA: 0x0000C6A4 File Offset: 0x0000B6A4
		public _IExpression ExpWithType
		{
			get
			{
				return this.m_expWithType;
			}
			set
			{
				this.m_expWithType = value;
			}
		}

		// Token: 0x170000BF RID: 191
		// (get) Token: 0x060003CA RID: 970 RVA: 0x0000C6AD File Offset: 0x0000B6AD
		// (set) Token: 0x060003CB RID: 971 RVA: 0x0000C6B5 File Offset: 0x0000B6B5
		public _IExpression BaseExpression
		{
			get
			{
				return this.m_expBase;
			}
			set
			{
				this.m_expBase = value;
			}
		}

		// Token: 0x170000C0 RID: 192
		// (get) Token: 0x060003CC RID: 972 RVA: 0x0000C69C File Offset: 0x0000B69C
		public IExpression ExprWithType
		{
			get
			{
				return this.m_expWithType;
			}
		}

		// Token: 0x170000C1 RID: 193
		// (get) Token: 0x060003CD RID: 973 RVA: 0x0000C6AD File Offset: 0x0000B6AD
		public IExpression Base
		{
			get
			{
				return this.m_expBase;
			}
		}

		// Token: 0x170000C2 RID: 194
		// (get) Token: 0x060003CE RID: 974 RVA: 0x0000C6BE File Offset: 0x0000B6BE
		// (set) Token: 0x060003CF RID: 975 RVA: 0x0000C6C6 File Offset: 0x0000B6C6
		public ICompiledType ExplicitelySpecifiedType
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

		// Token: 0x04000095 RID: 149
		[Obfuscation(Feature = "rename")]
		[DefaultSerialization("type")]
		[StorageVersion("3.4.0.0")]
		private _IExpression m_expWithType;

		// Token: 0x04000096 RID: 150
		[Obfuscation(Feature = "rename")]
		[DefaultSerialization("base")]
		[StorageVersion("3.4.0.0")]
		private _IExpression m_expBase;

		// Token: 0x04000097 RID: 151
		[Obfuscation(Feature = "rename")]
		[DefaultSerialization("ExplicitType")]
		[StorageVersion("3.4.0.0")]
		private ICompiledType m_type;

		// Token: 0x04000098 RID: 152
		[Obfuscation(Feature = "rename")]
		private ICompiledType m_ctype;
	}
}
