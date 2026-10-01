using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000064 RID: 100
	[TypeGuid("{42E3D528-9EC1-4a72-908B-1C3B5414A93C}")]
	[StorageVersion("3.3.2.0")]
	public class NewExpression : PositionExpression, _INewExpression, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression, INewExpression
	{
		// Token: 0x06000634 RID: 1588 RVA: 0x0000B408 File Offset: 0x0000A408
		public NewExpression()
		{
		}

		// Token: 0x06000635 RID: 1589 RVA: 0x0001076F File Offset: 0x0000F76F
		public NewExpression(_IType typeIn, _IExpression expCount)
		{
			this.m_typetocast = typeIn;
			this.m_expCount = expCount;
		}

		// Token: 0x06000636 RID: 1590 RVA: 0x00010785 File Offset: 0x0000F785
		public NewExpression(_IType typeIn, _IExpression expCount, IToken token) : base(token)
		{
			this.m_typetocast = typeIn;
			this.m_expCount = expCount;
		}

		// Token: 0x1700015B RID: 347
		// (get) Token: 0x06000637 RID: 1591 RVA: 0x0001079C File Offset: 0x0000F79C
		// (set) Token: 0x06000638 RID: 1592 RVA: 0x000107A4 File Offset: 0x0000F7A4
		public bool PositionOK { get; set; }

		// Token: 0x06000639 RID: 1593 RVA: 0x00004E6B File Offset: 0x00003E6B
		public override bool IsLValue(IScope scope, bool bWriteToConstants)
		{
			return false;
		}

		// Token: 0x0600063A RID: 1594 RVA: 0x000107AD File Offset: 0x0000F7AD
		public int ElementCount(out bool bValid)
		{
			bValid = false;
			if (this.m_expCount is IntegerLiteralExpression)
			{
				return (this.m_expCount as IntegerLiteralExpression).LiteralValue.GetInt(out bValid);
			}
			return 0;
		}

		// Token: 0x0600063B RID: 1595 RVA: 0x000107D7 File Offset: 0x0000F7D7
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x0600063C RID: 1596 RVA: 0x000107E0 File Offset: 0x0000F7E0
		public override T Accept<T>(IExprementVisitor<T> visitor)
		{
			return visitor.visit(this);
		}

		// Token: 0x0600063D RID: 1597 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public override void AcceptVisitor(IExprVisitor visitor)
		{
		}

		// Token: 0x0600063E RID: 1598 RVA: 0x000107E9 File Offset: 0x0000F7E9
		public override ISignature GetSignature(IScope scope)
		{
			if (this.m_typetocast is UserdefType)
			{
				return (this.m_typetocast as UserdefType).GetSignature(scope);
			}
			return base.GetSignature(scope);
		}

		// Token: 0x0600063F RID: 1599 RVA: 0x00010814 File Offset: 0x0000F814
		public override _IExprement Duplicate()
		{
			NewExpression newExpression = new NewExpression(this._TypeToCast.Duplicate, this.m_expCount.Duplicate() as Expression);
			this.DuplicateCommon(newExpression);
			if (this._fbinitparams != null)
			{
				foreach (_IAssignmentExpression iassignmentExpression in this._fbinitparams.OfType<_IAssignmentExpression>())
				{
					newExpression.AddFBInitParam(iassignmentExpression.Duplicate() as _IAssignmentExpression);
				}
			}
			return newExpression;
		}

		// Token: 0x1700015C RID: 348
		// (get) Token: 0x06000640 RID: 1600 RVA: 0x000108A4 File Offset: 0x0000F8A4
		// (set) Token: 0x06000641 RID: 1601 RVA: 0x00003AE9 File Offset: 0x00002AE9
		[SuppressMessage("Blocker Code Smell", "S3237:\"value\" parameters should be used", Justification = "Setter cannot be changed for compatibility reasons")]
		public override ICompiledType _CompiledType
		{
			get
			{
				return new PointerType(this._TypeToCast);
			}
			set
			{
			}
		}

		// Token: 0x1700015D RID: 349
		// (get) Token: 0x06000642 RID: 1602 RVA: 0x000108B1 File Offset: 0x0000F8B1
		// (set) Token: 0x06000643 RID: 1603 RVA: 0x000108CD File Offset: 0x0000F8CD
		public _IType _TypeToCast
		{
			get
			{
				if (this.m_typetocast == null)
				{
					return null;
				}
				return this.m_typetocast.EffectiveType as _IType;
			}
			set
			{
				this.m_typetocast = value;
			}
		}

		// Token: 0x1700015E RID: 350
		// (get) Token: 0x06000644 RID: 1604 RVA: 0x000108D6 File Offset: 0x0000F8D6
		// (set) Token: 0x06000645 RID: 1605 RVA: 0x000108DE File Offset: 0x0000F8DE
		public _IExpression _Count
		{
			get
			{
				return this.m_expCount;
			}
			set
			{
				this.m_expCount = value;
			}
		}

		// Token: 0x1700015F RID: 351
		// (get) Token: 0x06000646 RID: 1606 RVA: 0x000108E7 File Offset: 0x0000F8E7
		public ICompiledType TypeToCreate
		{
			get
			{
				return this.m_typetocast;
			}
		}

		// Token: 0x17000160 RID: 352
		// (get) Token: 0x06000647 RID: 1607 RVA: 0x000108D6 File Offset: 0x0000F8D6
		public IExpression Count
		{
			get
			{
				return this.m_expCount;
			}
		}

		// Token: 0x17000161 RID: 353
		// (get) Token: 0x06000648 RID: 1608 RVA: 0x000108EF File Offset: 0x0000F8EF
		// (set) Token: 0x06000649 RID: 1609 RVA: 0x000108F7 File Offset: 0x0000F8F7
		public IEnumerable<IAssignmentExpression> FBInitParams
		{
			get
			{
				return this._fbinitparams;
			}
			set
			{
				this._fbinitparams = new LList<IAssignmentExpression>(value);
			}
		}

		// Token: 0x17000162 RID: 354
		// (get) Token: 0x0600064A RID: 1610 RVA: 0x000108EF File Offset: 0x0000F8EF
		public IList<IAssignmentExpression> _FBInitParams
		{
			get
			{
				return this._fbinitparams;
			}
		}

		// Token: 0x0600064B RID: 1611 RVA: 0x00010905 File Offset: 0x0000F905
		public void AddFBInitParam(IAssignmentExpression assexp)
		{
			if (this._fbinitparams == null)
			{
				this._fbinitparams = new LList<IAssignmentExpression>();
			}
			this._fbinitparams.Add(assexp);
		}

		// Token: 0x040000D4 RID: 212
		[Obfuscation(Feature = "rename")]
		[DefaultSerialization("type")]
		[StorageVersion("3.3.2.0")]
		private _IType m_typetocast;

		// Token: 0x040000D5 RID: 213
		[Obfuscation(Feature = "rename")]
		[DefaultSerialization("count")]
		[StorageVersion("3.3.2.0")]
		private _IExpression m_expCount;

		// Token: 0x040000D6 RID: 214
		[Obfuscation(Feature = "rename")]
		[DefaultSerialization("fbinitparams")]
		[StorageVersion("3.5.3.40")]
		[StorageIgnorable]
		private LList<IAssignmentExpression> _fbinitparams;
	}
}
