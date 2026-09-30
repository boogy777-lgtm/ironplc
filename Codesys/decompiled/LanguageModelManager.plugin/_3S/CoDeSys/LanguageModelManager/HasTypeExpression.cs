using System;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000059 RID: 89
	[TypeGuid("{274662ad-09b0-41d0-95dc-3f2994617b5f}")]
	[StorageVersion("3.3.0.0")]
	public class HasTypeExpression : PragmaExpression, _IHasTypeExpression, _IPragmaExpression, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression, IHasTypeExpression
	{
		// Token: 0x06000546 RID: 1350 RVA: 0x0000C6EA File Offset: 0x0000B6EA
		public HasTypeExpression()
		{
		}

		// Token: 0x06000547 RID: 1351 RVA: 0x0000C6F2 File Offset: 0x0000B6F2
		public HasTypeExpression(IToken token) : base(token)
		{
		}

		// Token: 0x06000548 RID: 1352 RVA: 0x0000F043 File Offset: 0x0000E043
		public HasTypeExpression(IToken token, VariableReference varref, ICompiledType type) : base(token)
		{
			this.m_varref = varref;
			this.m_type = type;
		}

		// Token: 0x1700011F RID: 287
		// (get) Token: 0x06000549 RID: 1353 RVA: 0x0000F05A File Offset: 0x0000E05A
		// (set) Token: 0x0600054A RID: 1354 RVA: 0x0000F070 File Offset: 0x0000E070
		public _IExpression Variable
		{
			get
			{
				if (this.m_varref == null)
				{
					return new NullExpression();
				}
				return this.m_varref;
			}
			set
			{
				this.m_varref = (value as VariableReference);
			}
		}

		// Token: 0x17000120 RID: 288
		// (get) Token: 0x0600054B RID: 1355 RVA: 0x0000F07E File Offset: 0x0000E07E
		public IExpression Instance
		{
			get
			{
				return this.Variable;
			}
		}

		// Token: 0x17000121 RID: 289
		// (get) Token: 0x0600054C RID: 1356 RVA: 0x00005E58 File Offset: 0x00004E58
		public virtual bool Exact
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000122 RID: 290
		// (get) Token: 0x0600054D RID: 1357 RVA: 0x0000F086 File Offset: 0x0000E086
		// (set) Token: 0x0600054E RID: 1358 RVA: 0x0000F08E File Offset: 0x0000E08E
		public ICompiledType ReferencedType
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

		// Token: 0x0600054F RID: 1359 RVA: 0x0000F097 File Offset: 0x0000E097
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x06000550 RID: 1360 RVA: 0x0000F0A0 File Offset: 0x0000E0A0
		public override T Accept<T>(IExprementVisitor<T> visitor)
		{
			return visitor.visit(this);
		}

		// Token: 0x06000551 RID: 1361 RVA: 0x0000F0A9 File Offset: 0x0000E0A9
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x06000552 RID: 1362 RVA: 0x00004E6B File Offset: 0x00003E6B
		public override bool IsLValue(IScope scope, bool bWriteToConstants)
		{
			return false;
		}

		// Token: 0x06000553 RID: 1363 RVA: 0x0000F0B4 File Offset: 0x0000E0B4
		public override _IExprement Duplicate()
		{
			HasTypeExpression hasTypeExpression = new HasTypeExpression();
			if (this.m_varref != null)
			{
				hasTypeExpression.m_varref = (this.m_varref.Duplicate() as VariableReference);
			}
			if (this.m_type != null)
			{
				hasTypeExpression.m_type = ((_IType)this.m_type).Duplicate;
			}
			this.DuplicateCommon(hasTypeExpression);
			return hasTypeExpression;
		}

		// Token: 0x17000123 RID: 291
		// (get) Token: 0x06000554 RID: 1364 RVA: 0x0000F10B File Offset: 0x0000E10B
		public _IExpression VarRef
		{
			get
			{
				return this.m_varref;
			}
		}

		// Token: 0x040000BF RID: 191
		[DefaultSerialization("Reference")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		protected VariableReference m_varref;

		// Token: 0x040000C0 RID: 192
		[DefaultSerialization("ReferenceType")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		protected ICompiledType m_type;
	}
}
