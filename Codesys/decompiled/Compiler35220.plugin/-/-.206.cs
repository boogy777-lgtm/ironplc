using System;
using System.Collections.Generic;
using \u0019;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0005
{
	// Token: 0x0200023B RID: 571
	internal sealed class \u0005 : StandardTraverser
	{
		// Token: 0x170006F0 RID: 1776
		// (get) Token: 0x060025A1 RID: 9633 RVA: 0x000820D4 File Offset: 0x000802D4
		private IFlowPosVisitor351300 FlowPosVis
		{
			get
			{
				return this._expCalledForAll as IFlowPosVisitor351300;
			}
		}

		// Token: 0x060025A2 RID: 9634 RVA: 0x000820E4 File Offset: 0x000802E4
		public \u0005(IFlowPosVisitor351300 \u000F\u0004, _IBreakpointList \u0010\u0004) : base(\u000F\u0004)
		{
			this.\u0001 = \u0010\u0004;
		}

		// Token: 0x060025A3 RID: 9635 RVA: 0x000820F4 File Offset: 0x000802F4
		public bool \u0001(ISourcePosition \u0002)
		{
			IBreakpoint breakpoint = this.\u0001.FindBySourcePosition(\u0002);
			if (breakpoint == null)
			{
				return false;
			}
			this.FlowPosVis.SetCurrentBreakpoint(breakpoint);
			return true;
		}

		// Token: 0x060025A4 RID: 9636 RVA: 0x00082120 File Offset: 0x00080320
		public override void visit(_IIfStatement ifst)
		{
			base.Push(AccessFlag.Read);
			ifst._Condition.Accept(this);
			base.Pop();
			ifst._IfThen.Accept(this);
			foreach (_IElseIf ielseIf in ifst._ElseIf)
			{
				this.\u0001(ielseIf.Position);
				base.Push(AccessFlag.Read);
				ielseIf._Condition.Accept(this);
				base.Pop();
				ielseIf._Controlled.Accept(this);
			}
			if (ifst._IfElse != null)
			{
				ifst._IfElse.Accept(this);
			}
			this._expCalledForAll.visit(ifst);
		}

		// Token: 0x060025A5 RID: 9637 RVA: 0x000821E0 File Offset: 0x000803E0
		public override void visit(_ISequenceStatement seq)
		{
			if (base.Abort)
			{
				return;
			}
			base.Push(AccessFlag.Unknown);
			foreach (_IStatement istatement in seq._StatementList)
			{
				if (istatement.GetFlag(StatementFlag.GenerateFlow) && istatement.Position != null && !(istatement is _ICommentStatement) && (this.\u0001(istatement.Position) || istatement is _ISequenceStatement || istatement is _IRepeatStatement))
				{
					istatement.Accept(this);
					if (base.Abort)
					{
						break;
					}
					if (!(istatement is _ISequenceStatement))
					{
						IBreakpoint bp = this.\u0001.FindBySourcePosition(istatement.Position);
						this.FlowPosVis.GenerateDummyFlow(bp);
					}
				}
			}
			base.Pop();
			this._expCalledForAll.visit(seq);
		}

		// Token: 0x060025A6 RID: 9638 RVA: 0x000822BC File Offset: 0x000804BC
		public override void visit(_ICallExpression call)
		{
			IUserdefType userdefType = (IUserdefType)call._Callee._CompiledType;
			ISignature u = this.FlowPosVis.IScope[userdefType.SignatureId];
			base.Push(AccessFlag.Call);
			call._Callee.Accept(this);
			base.Pop();
			this.\u0001(call._Condition, AccessFlag.Read);
			this.\u0001(call.ParamExpressions, AccessFlag.Read);
			this.\u0001(call.OutputExpressions, AccessFlag.Write);
			this.\u0001(call, u);
			IBreakpoint currentBreakpoint = this.\u0001(call, u);
			this._expCalledForAll.visit(call);
			this.FlowPosVis.SetCurrentBreakpoint(currentBreakpoint);
		}

		// Token: 0x060025A7 RID: 9639 RVA: 0x0008235C File Offset: 0x0008055C
		public override void visit(_IBreakPointStatement bpstate)
		{
			this.\u0001(bpstate.Position);
		}

		// Token: 0x060025A8 RID: 9640 RVA: 0x0008236C File Offset: 0x0008056C
		public override void visit(_ICompoAccessExpression compo)
		{
			this.FlowPosVis.visitCompoAccess(compo, base.TopOfStack);
			compo._Left.Accept(this);
		}

		// Token: 0x060025A9 RID: 9641 RVA: 0x0008238C File Offset: 0x0008058C
		public override void visit(_IIndexAccessExpression indexaccess)
		{
			this.FlowPosVis.visitIndexAccess(indexaccess, base.TopOfStack);
			foreach (_IExpression iexpression in indexaccess._Accesses)
			{
				iexpression.Accept(this);
			}
			indexaccess._Var.Accept(this);
		}

		// Token: 0x060025AA RID: 9642 RVA: 0x000823F8 File Offset: 0x000805F8
		public override void visit(_IDeRefAccessExpression deref)
		{
			this.FlowPosVis.visitDeRefAccess(deref, base.TopOfStack);
			deref._Base.Accept(this);
		}

		// Token: 0x060025AB RID: 9643 RVA: 0x00082418 File Offset: 0x00080618
		private void \u0001(_ICallExpression \u0002, ISignature \u0003)
		{
			if (\u0003 != null && (\u0003.POUType == Operator.FunctionBlock || \u0003.POUType == Operator.Program))
			{
				foreach (_IExpression u in \u0002.Inputs)
				{
					this.\u0001(\u0002, u, AccessFlag.Write);
				}
				foreach (_IExpression u2 in \u0002.Outputs)
				{
					this.\u0001(\u0002, u2, AccessFlag.Read);
				}
				foreach (_IExpression u3 in \u0002.EmptyAssigns)
				{
					this.\u0001(\u0002, u3, AccessFlag.Read);
				}
			}
		}

		// Token: 0x060025AC RID: 9644 RVA: 0x00082500 File Offset: 0x00080700
		private IBreakpoint \u0001(_ICallExpression \u0002, ISignature \u0003)
		{
			IBreakpoint currentBreakpoint = this.FlowPosVis.GetCurrentBreakpoint();
			if (currentBreakpoint != null && currentBreakpoint.StepInSuccessors != null && \u0003 != null)
			{
				foreach (IStepInPosition stepInPosition in currentBreakpoint.StepInSuccessors)
				{
					if (stepInPosition.StepOutBreakpoint != null && stepInPosition.StepOutBreakpoint.Position.Position == \u0002.Position.Position && stepInPosition.StepOutBreakpoint.Position.PositionOffset == \u0002.Position.PositionOffset && \u0003.Id == stepInPosition.SignatureId)
					{
						this.FlowPosVis.SetCurrentBreakpoint(stepInPosition.StepOutBreakpoint);
						this.FlowPosVis.LastListedBreakpoint = currentBreakpoint;
						break;
					}
				}
			}
			return currentBreakpoint;
		}

		// Token: 0x060025AD RID: 9645 RVA: 0x000825BC File Offset: 0x000807BC
		private void \u0001(_ICallExpression \u0002, _IExpression \u0003, AccessFlag \u0004)
		{
			_IVariableExpression ivariableExpression = \u0003 as _IVariableExpression;
			if (ivariableExpression != null)
			{
				_IExpression iexpression = (_IExpression)\u0002._Callee.Duplicate();
				iexpression._Position = \u0003.\u0001(ivariableExpression._Position.EditorPosition, ivariableExpression._Position.PositionOffset + ivariableExpression.LengthIntern);
				_ICompoAccessExpression icompoAccessExpression = \u0003.\u0001(iexpression, ivariableExpression);
				icompoAccessExpression._Position = ivariableExpression._Position;
				icompoAccessExpression.LengthIntern = ivariableExpression.LengthIntern;
				base.Push(\u0004);
				icompoAccessExpression.Accept(this);
				base.Pop();
			}
		}

		// Token: 0x060025AE RID: 9646 RVA: 0x00082640 File Offset: 0x00080840
		private void \u0001(IEnumerable<_IExpression> \u0002, AccessFlag \u0003)
		{
			foreach (_IExpression u in \u0002)
			{
				this.\u0001(u, \u0003);
			}
		}

		// Token: 0x060025AF RID: 9647 RVA: 0x0008268C File Offset: 0x0008088C
		private void \u0001(_IExpression \u0002, AccessFlag \u0003)
		{
			if (\u0002 != null)
			{
				this.\u0002(\u0002, \u0003);
			}
		}

		// Token: 0x060025B0 RID: 9648 RVA: 0x0008269C File Offset: 0x0008089C
		private void \u0002(_IExpression \u0002, AccessFlag \u0003)
		{
			base.Push(\u0003);
			\u0002.Accept(this);
			base.Pop();
		}

		// Token: 0x040006CF RID: 1743
		private readonly _IBreakpointList \u0001;
	}
}
