using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x0200003F RID: 63
	[TypeGuid("{458880e1-4ccc-4f34-af1a-7cd7fe7c8193}")]
	[StorageVersion("3.3.0.0")]
	public class AssignmentExpression : PositionExpression, _IAssignmentExpression, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression, IAssignmentExpression
	{
		// Token: 0x06000325 RID: 805 RVA: 0x0000B7C4 File Offset: 0x0000A7C4
		public AssignmentExpression()
		{
		}

		// Token: 0x06000326 RID: 806 RVA: 0x0000B7D7 File Offset: 0x0000A7D7
		public AssignmentExpression(_IExpression expLValue)
		{
			this.m_expLValue = expLValue;
		}

		// Token: 0x06000327 RID: 807 RVA: 0x0000B7F1 File Offset: 0x0000A7F1
		internal AssignmentExpression(_IExpression expLValue, IToken token) : base(token)
		{
			this.m_expLValue = expLValue;
		}

		// Token: 0x1700008B RID: 139
		// (get) Token: 0x06000328 RID: 808 RVA: 0x0000B80C File Offset: 0x0000A80C
		// (set) Token: 0x06000329 RID: 809 RVA: 0x0000B814 File Offset: 0x0000A814
		public Operator KindOf
		{
			get
			{
				return this.m_op;
			}
			set
			{
				this.m_op = value;
			}
		}

		// Token: 0x1700008C RID: 140
		// (get) Token: 0x0600032A RID: 810 RVA: 0x0000B81D File Offset: 0x0000A81D
		public IExpression LValue
		{
			get
			{
				return this._LValue;
			}
		}

		// Token: 0x1700008D RID: 141
		// (get) Token: 0x0600032B RID: 811 RVA: 0x0000B825 File Offset: 0x0000A825
		// (set) Token: 0x0600032C RID: 812 RVA: 0x0000B83B File Offset: 0x0000A83B
		public _IExpression _LValue
		{
			get
			{
				if (this.m_expLValue != null)
				{
					return this.m_expLValue;
				}
				return new NullExpression();
			}
			set
			{
				this.m_expLValue = value;
			}
		}

		// Token: 0x1700008E RID: 142
		// (get) Token: 0x0600032D RID: 813 RVA: 0x0000B844 File Offset: 0x0000A844
		public IExpression RValue
		{
			get
			{
				return this._RValue;
			}
		}

		// Token: 0x1700008F RID: 143
		// (get) Token: 0x0600032E RID: 814 RVA: 0x0000B84C File Offset: 0x0000A84C
		// (set) Token: 0x0600032F RID: 815 RVA: 0x0000B862 File Offset: 0x0000A862
		public _IExpression _RValue
		{
			get
			{
				if (this.m_expRValue == null)
				{
					return new NullExpression();
				}
				return this.m_expRValue;
			}
			set
			{
				this.m_expRValue = value;
			}
		}

		// Token: 0x06000330 RID: 816 RVA: 0x0000B86B File Offset: 0x0000A86B
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x06000331 RID: 817 RVA: 0x0000B874 File Offset: 0x0000A874
		public override T Accept<T>(IExprementVisitor<T> visitor)
		{
			return visitor.visit(this);
		}

		// Token: 0x06000332 RID: 818 RVA: 0x0000B87D File Offset: 0x0000A87D
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x06000333 RID: 819 RVA: 0x0000B888 File Offset: 0x0000A888
		public override _IExprement Duplicate()
		{
			AssignmentExpression assignmentExpression = new AssignmentExpression();
			this.DuplicateCommon(assignmentExpression);
			if (this.m_expLValue != null)
			{
				assignmentExpression._LValue = (this._LValue.Duplicate() as _IExpression);
			}
			if (this.m_expRValue != null)
			{
				assignmentExpression._RValue = (this._RValue.Duplicate() as _IExpression);
			}
			assignmentExpression.KindOf = this.KindOf;
			return assignmentExpression;
		}

		// Token: 0x06000334 RID: 820 RVA: 0x00004E6B File Offset: 0x00003E6B
		public override bool IsLValue(IScope scope, bool bWriteToConstants)
		{
			return false;
		}

		// Token: 0x17000090 RID: 144
		// (get) Token: 0x06000335 RID: 821 RVA: 0x0000B8EB File Offset: 0x0000A8EB
		// (set) Token: 0x06000336 RID: 822 RVA: 0x0000B8F3 File Offset: 0x0000A8F3
		public override IExprInfo Info
		{
			get
			{
				return this.m_exprinfo;
			}
			set
			{
				this.m_exprinfo = value;
			}
		}

		// Token: 0x17000091 RID: 145
		// (get) Token: 0x06000337 RID: 823 RVA: 0x0000B8FC File Offset: 0x0000A8FC
		// (set) Token: 0x06000338 RID: 824 RVA: 0x00003AE9 File Offset: 0x00002AE9
		[DefaultSerialization("Type")]
		[StorageVersion("3.3.0.10")]
		[StorageIgnorable]
		[Obfuscation(Feature = "rename")]
		[SuppressMessage("Blocker Code Smell", "S3237:\"value\" parameters should be used", Justification = "We have to preserve the behaviour for compatibilty reasons")]
		public override ICompiledType _CompiledType
		{
			get
			{
				if (this.Info is IPropertyAssignmentExprInfo)
				{
					return null;
				}
				if (this.m_expLValue == null)
				{
					return null;
				}
				return this.m_expLValue._CompiledType;
			}
			set
			{
			}
		}

		// Token: 0x04000078 RID: 120
		[Obfuscation(Feature = "rename")]
		private IExprInfo m_exprinfo;

		// Token: 0x04000079 RID: 121
		[DefaultSerialization("LValue")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private _IExpression m_expLValue;

		// Token: 0x0400007A RID: 122
		[DefaultSerialization("RValue")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private _IExpression m_expRValue;

		// Token: 0x0400007B RID: 123
		[DefaultSerialization("Operator")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private Operator m_op = Operator.Assign;
	}
}
