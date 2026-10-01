using System;
using System.Collections.Generic;
using System.Diagnostics;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.GreenTrees
{
	// Token: 0x0200022E RID: 558
	internal class StructureInitialisation_Green : Expression_Green, _IStructureInitialization, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression, IStructureInitialization
	{
		// Token: 0x06002479 RID: 9337 RVA: 0x0005C4E1 File Offset: 0x0005B4E1
		public StructureInitialisation_Green(IList<_IAssignmentExpression> explist)
		{
			this.m_initlist = new _IAssignmentExpression[explist.Count];
			explist.CopyTo(this.m_initlist, 0);
		}

		// Token: 0x0600247A RID: 9338 RVA: 0x00012A6D File Offset: 0x00011A6D
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x0600247B RID: 9339 RVA: 0x0005C508 File Offset: 0x0005B508
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			IExprVisitor3 exprVisitor = visitor as IExprVisitor3;
			if (exprVisitor != null)
			{
				exprVisitor.visit(this);
			}
		}

		// Token: 0x17000A57 RID: 2647
		// (get) Token: 0x0600247C RID: 9340 RVA: 0x0005C528 File Offset: 0x0005B528
		public IAssignmentExpression[] CompoInits
		{
			get
			{
				return this.m_initlist;
			}
		}

		// Token: 0x17000A58 RID: 2648
		// (get) Token: 0x0600247D RID: 9341 RVA: 0x0005C53D File Offset: 0x0005B53D
		public IList<_IAssignmentExpression> _CompoInits
		{
			get
			{
				return this.m_initlist;
			}
		}

		// Token: 0x0600247E RID: 9342 RVA: 0x0005A8D1 File Offset: 0x000598D1
		[Obsolete("no list access")]
		public void AddInitValue(_IAssignmentExpression assign)
		{
			Debug.Assert(false);
		}

		// Token: 0x17000A59 RID: 2649
		// (get) Token: 0x0600247F RID: 9343 RVA: 0x00004E6B File Offset: 0x00003E6B
		// (set) Token: 0x06002480 RID: 9344 RVA: 0x0005A471 File Offset: 0x00059471
		[Obsolete("not used for compile")]
		public bool DefaultInitializationDone
		{
			get
			{
				return false;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		// Token: 0x040006FC RID: 1788
		private readonly _IAssignmentExpression[] m_initlist;
	}
}
