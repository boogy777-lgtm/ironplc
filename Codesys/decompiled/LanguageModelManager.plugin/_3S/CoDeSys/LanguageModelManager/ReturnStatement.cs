using System;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x020000A0 RID: 160
	[TypeGuid("{7b1448ab-19e4-43b7-9990-5d25309be891}")]
	[StorageVersion("3.3.0.0")]
	public class ReturnStatement : PositionStatement, _IReturnStatement, _IStatement, _IExprement, IExprement3, IExprement2, IExprement, IStatement, IReturnStatement
	{
		// Token: 0x060009A3 RID: 2467 RVA: 0x000149DB File Offset: 0x000139DB
		public ReturnStatement()
		{
		}

		// Token: 0x060009A4 RID: 2468 RVA: 0x000149E3 File Offset: 0x000139E3
		public ReturnStatement(IToken token) : base(token)
		{
		}

		// Token: 0x060009A5 RID: 2469 RVA: 0x000164AB File Offset: 0x000154AB
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x060009A6 RID: 2470 RVA: 0x000164B4 File Offset: 0x000154B4
		public override T Accept<T>(IExprementVisitor<T> visitor)
		{
			return visitor.visit(this);
		}

		// Token: 0x060009A7 RID: 2471 RVA: 0x000164BD File Offset: 0x000154BD
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x1700023A RID: 570
		// (get) Token: 0x060009A8 RID: 2472 RVA: 0x000164C6 File Offset: 0x000154C6
		public IExpression Condition
		{
			get
			{
				return this._Condition;
			}
		}

		// Token: 0x1700023B RID: 571
		// (get) Token: 0x060009A9 RID: 2473 RVA: 0x000164CE File Offset: 0x000154CE
		// (set) Token: 0x060009AA RID: 2474 RVA: 0x000164D6 File Offset: 0x000154D6
		public _IExpression _Condition
		{
			get
			{
				return this.m_expCondition;
			}
			set
			{
				this.m_expCondition = value;
			}
		}

		// Token: 0x060009AB RID: 2475 RVA: 0x000164E0 File Offset: 0x000154E0
		public override _IExprement Duplicate()
		{
			ReturnStatement returnStatement = new ReturnStatement();
			this.DuplicateCommon(returnStatement);
			if (this._Condition != null)
			{
				returnStatement._Condition = (this._Condition.Duplicate() as _IExpression);
			}
			return returnStatement;
		}

		// Token: 0x04000158 RID: 344
		[DefaultSerialization("Condition")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private _IExpression m_expCondition;
	}
}
