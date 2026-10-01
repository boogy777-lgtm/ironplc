using System;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x0200003D RID: 61
	[TypeGuid("{47c70928-094c-4f67-8ece-7d271841e8d6}")]
	[StorageVersion("3.3.0.0")]
	public class AddressExpression : PositionExpression, _IAddressExpression, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression, IAddressExpression
	{
		// Token: 0x06000303 RID: 771 RVA: 0x0000B408 File Offset: 0x0000A408
		public AddressExpression()
		{
		}

		// Token: 0x06000304 RID: 772 RVA: 0x0000B410 File Offset: 0x0000A410
		internal AddressExpression(IDirectVariable dirvar)
		{
			this.m_dirvar = dirvar;
		}

		// Token: 0x06000305 RID: 773 RVA: 0x0000B41F File Offset: 0x0000A41F
		internal AddressExpression(IDirectVariable dirvar, IToken token) : base(token)
		{
			this.m_dirvar = dirvar;
		}

		// Token: 0x17000082 RID: 130
		// (get) Token: 0x06000306 RID: 774 RVA: 0x0000B42F File Offset: 0x0000A42F
		// (set) Token: 0x06000307 RID: 775 RVA: 0x0000B437 File Offset: 0x0000A437
		public IDirectVariable DirectAddress
		{
			get
			{
				return this.m_dirvar;
			}
			set
			{
				this.m_dirvar = value;
			}
		}

		// Token: 0x06000308 RID: 776 RVA: 0x0000B440 File Offset: 0x0000A440
		public override bool IsLValue(IScope scope, bool bWriteToConstants)
		{
			return this.m_dirvar.Location != DirectVariableLocation.Input;
		}

		// Token: 0x06000309 RID: 777 RVA: 0x0000B453 File Offset: 0x0000A453
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x0600030A RID: 778 RVA: 0x0000B45C File Offset: 0x0000A45C
		public override T Accept<T>(IExprementVisitor<T> visitor)
		{
			return visitor.visit(this);
		}

		// Token: 0x0600030B RID: 779 RVA: 0x0000B465 File Offset: 0x0000A465
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x0600030C RID: 780 RVA: 0x0000B470 File Offset: 0x0000A470
		public override _IExprement Duplicate()
		{
			AddressExpression addressExpression = new AddressExpression();
			this.DuplicateCommon(addressExpression);
			addressExpression.m_dirvar = new DirectVariable(this.m_dirvar.Location, this.m_dirvar.Size, this.m_dirvar.Components);
			return addressExpression;
		}

		// Token: 0x17000083 RID: 131
		// (get) Token: 0x0600030D RID: 781 RVA: 0x0000B4B7 File Offset: 0x0000A4B7
		// (set) Token: 0x0600030E RID: 782 RVA: 0x0000B4BF File Offset: 0x0000A4BF
		public override IExprInfo Info
		{
			get
			{
				return this.m_addressExprInfo;
			}
			set
			{
				this.m_addressExprInfo = (value as IAddressExprInfo);
			}
		}

		// Token: 0x17000084 RID: 132
		// (get) Token: 0x0600030F RID: 783 RVA: 0x0000B4CD File Offset: 0x0000A4CD
		// (set) Token: 0x06000310 RID: 784 RVA: 0x0000B4D5 File Offset: 0x0000A4D5
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

		// Token: 0x04000072 RID: 114
		[Obfuscation(Feature = "rename")]
		[DefaultSerialization("compiledtype")]
		[StorageVersion("3.3.0.0")]
		private ICompiledType m_ctype;

		// Token: 0x04000073 RID: 115
		[DefaultSerialization("DirectVariable")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private IDirectVariable m_dirvar;

		// Token: 0x04000074 RID: 116
		[Obfuscation(Feature = "rename")]
		private IAddressExprInfo m_addressExprInfo;
	}
}
