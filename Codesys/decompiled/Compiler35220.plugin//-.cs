using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u007F
{
	// Token: 0x02000075 RID: 117
	internal abstract class \u0001 : IExprementVisitor352000, IExprementVisitor351900, IExprementVisitor351800, IExprementVisitor351500, IExprementVisitor351400, IExprementVisitor351300, IExprementVisitor3590, IExprementVisitor, IExprementVisitor2
	{
		// Token: 0x0600097F RID: 2431 RVA: 0x00012B80 File Offset: 0x00010D80
		public virtual void \u0001(_IAddressExpression \u0002)
		{
		}

		// Token: 0x06000980 RID: 2432 RVA: 0x00012B84 File Offset: 0x00010D84
		public virtual void \u0001(_IVariableExpression \u0002)
		{
		}

		// Token: 0x06000981 RID: 2433 RVA: 0x00012B88 File Offset: 0x00010D88
		public virtual void \u0001(_ICompiledPOU \u0002)
		{
		}

		// Token: 0x06000982 RID: 2434 RVA: 0x00012B8C File Offset: 0x00010D8C
		public virtual void \u0001(_ICopyScopeExpression \u0002)
		{
		}

		// Token: 0x06000983 RID: 2435 RVA: 0x00012B90 File Offset: 0x00010D90
		public virtual void \u0001(_INullStatement \u0002)
		{
		}

		// Token: 0x06000984 RID: 2436 RVA: 0x00012B94 File Offset: 0x00010D94
		public virtual void \u0001(_IQualifiedNameExpression \u0002)
		{
		}

		// Token: 0x06000985 RID: 2437 RVA: 0x00012B98 File Offset: 0x00010D98
		public virtual void \u0001(_IXRefExpression \u0002)
		{
		}

		// Token: 0x06000986 RID: 2438 RVA: 0x00012B9C File Offset: 0x00010D9C
		public virtual void \u0001(_IWhileStatement \u0002)
		{
			\u0002._Condition.Accept(this);
			\u0002._Controlled.Accept(this);
		}

		// Token: 0x06000987 RID: 2439 RVA: 0x00012BB8 File Offset: 0x00010DB8
		public virtual void \u0001(_IRepeatStatement \u0002)
		{
			\u0002._Condition.Accept(this);
			\u0002._Controlled.Accept(this);
		}

		// Token: 0x06000988 RID: 2440 RVA: 0x00012BD4 File Offset: 0x00010DD4
		public virtual void \u0001(_IForStatement \u0002)
		{
			\u0002._CounterStart.Accept(this);
			if (\u0002._Condition != null)
			{
				\u0002._Condition.Accept(this);
			}
			\u0002._UpperBound.Accept(this);
			if (\u0002.By != null)
			{
				\u0002._By.Accept(this);
			}
			\u0002._Controlled.Accept(this);
		}

		// Token: 0x06000989 RID: 2441 RVA: 0x00012C30 File Offset: 0x00010E30
		public virtual void \u0001(_ISequenceStatement \u0002)
		{
			for (int i = 0; i < \u0002._StatementList.Count; i++)
			{
				\u0002._StatementList[i].Accept(this);
			}
		}

		// Token: 0x0600098A RID: 2442 RVA: 0x00012C68 File Offset: 0x00010E68
		public virtual void \u0001(_IIfStatement \u0002)
		{
			\u0002._Condition.Accept(this);
			\u0002._IfThen.Accept(this);
			foreach (_IElseIf ielseIf in \u0002._ElseIf)
			{
				ielseIf._Condition.Accept(this);
				ielseIf._Controlled.Accept(this);
			}
			if (\u0002._IfElse != null)
			{
				\u0002._IfElse.Accept(this);
			}
		}

		// Token: 0x0600098B RID: 2443 RVA: 0x00012CF0 File Offset: 0x00010EF0
		public virtual void \u0001(_IExpressionStatement \u0002)
		{
			\u0002._Expr.Accept(this);
		}

		// Token: 0x0600098C RID: 2444 RVA: 0x00012D00 File Offset: 0x00010F00
		public virtual void \u0001(_IAssignmentExpression \u0002)
		{
			\u0002._LValue.Accept(this);
			\u0002._RValue.Accept(this);
		}

		// Token: 0x0600098D RID: 2445 RVA: 0x00012D1C File Offset: 0x00010F1C
		public virtual void \u0001(_ICallExpression \u0002)
		{
			\u0002._Callee.Accept(this);
			if (\u0002._Condition != null)
			{
				\u0002._Condition.Accept(this);
			}
			foreach (_IExpression iexpression in \u0002.ParamExpressions)
			{
				if (iexpression != null)
				{
					iexpression.Accept(this);
				}
			}
			foreach (_IExpression iexpression2 in \u0002.OutputExpressions)
			{
				if (iexpression2 != null)
				{
					iexpression2.Accept(this);
				}
			}
			foreach (_IExpression iexpression3 in \u0002.Inputs)
			{
				if (iexpression3 != null)
				{
					iexpression3.Accept(this);
				}
			}
			foreach (_IExpression iexpression4 in \u0002.Outputs)
			{
				if (iexpression4 != null)
				{
					iexpression4.Accept(this);
				}
			}
		}

		// Token: 0x0600098E RID: 2446 RVA: 0x00012E4C File Offset: 0x0001104C
		public virtual void \u0001(_IOperatorExpression \u0002)
		{
			foreach (_IExpression iexpression in \u0002._OperandsList)
			{
				iexpression.Accept(this);
			}
		}

		// Token: 0x0600098F RID: 2447 RVA: 0x00012E98 File Offset: 0x00011098
		public virtual void \u0001(_ICastExpression \u0002)
		{
			\u0002.BaseExpression.Accept(this);
		}

		// Token: 0x06000990 RID: 2448 RVA: 0x00012EA8 File Offset: 0x000110A8
		public virtual void \u0001(_INewExpression \u0002)
		{
			\u0002._Count.Accept(this);
			if (\u0002._FBInitParams != null)
			{
				foreach (IAssignmentExpression assignmentExpression in \u0002._FBInitParams)
				{
					((_IAssignmentExpression)assignmentExpression).Accept(this);
				}
			}
		}

		// Token: 0x06000991 RID: 2449 RVA: 0x00012F0C File Offset: 0x0001110C
		public virtual void \u0001(_ITypeExpression \u0002)
		{
		}

		// Token: 0x06000992 RID: 2450 RVA: 0x00012F10 File Offset: 0x00011110
		public virtual void \u0001(_IConversionExpression \u0002)
		{
			\u0002._Exp.Accept(this);
		}

		// Token: 0x06000993 RID: 2451 RVA: 0x00012F20 File Offset: 0x00011120
		public virtual void \u0001(_IIndexAccessExpression \u0002)
		{
			for (int i = 0; i < \u0002.NumAccesses; i++)
			{
				\u0002.GetAccess(i).Accept(this);
			}
			\u0002._Var.Accept(this);
		}

		// Token: 0x06000994 RID: 2452 RVA: 0x00012F58 File Offset: 0x00011158
		public virtual void \u0001(_ILiteralExpression \u0002)
		{
		}

		// Token: 0x06000995 RID: 2453 RVA: 0x00012F5C File Offset: 0x0001115C
		public virtual void \u0001(_ICompoAccessExpression \u0002)
		{
			\u0002._Left.Accept(this);
			\u0002._Right.Accept(this);
		}

		// Token: 0x06000996 RID: 2454 RVA: 0x00012F78 File Offset: 0x00011178
		public virtual void \u0001(_IDeRefAccessExpression \u0002)
		{
			\u0002._Base.Accept(this);
		}

		// Token: 0x06000997 RID: 2455 RVA: 0x00012F88 File Offset: 0x00011188
		public virtual void \u0001(_IGlobalScopeExpression \u0002)
		{
			\u0002._Base.Accept(this);
		}

		// Token: 0x06000998 RID: 2456 RVA: 0x00012F98 File Offset: 0x00011198
		public virtual void \u0001(_ISystemScopeExpression \u0002)
		{
			\u0002._Base.Accept(this);
		}

		// Token: 0x06000999 RID: 2457 RVA: 0x00012FA8 File Offset: 0x000111A8
		public virtual void \u0001(_IPoolScopeExpression \u0002)
		{
			\u0002._Base.Accept(this);
		}

		// Token: 0x0600099A RID: 2458 RVA: 0x00012FB8 File Offset: 0x000111B8
		public virtual void \u0001(_INamespaceAccessExpression \u0002)
		{
			\u0002._Namespace.Accept(this);
			\u0002._Access.Accept(this);
		}

		// Token: 0x0600099B RID: 2459 RVA: 0x00012FD4 File Offset: 0x000111D4
		public virtual void \u0001(_ICurrentTaskExpression \u0002)
		{
			\u0002._Base.Accept(this);
		}

		// Token: 0x0600099C RID: 2460 RVA: 0x00012FE4 File Offset: 0x000111E4
		public virtual void \u0001(_ICaseRangeExpression \u0002)
		{
			\u0002._Low.Accept(this);
			\u0002._High.Accept(this);
		}

		// Token: 0x0600099D RID: 2461 RVA: 0x00013000 File Offset: 0x00011200
		public virtual void \u0001(_ICaseLabelStatement \u0002)
		{
			foreach (_IExpression iexpression in \u0002._cases)
			{
				iexpression.Accept(this);
			}
		}

		// Token: 0x0600099E RID: 2462 RVA: 0x0001304C File Offset: 0x0001124C
		public virtual void \u0001(_ICaseStatement \u0002)
		{
			\u0002._Switch.Accept(this);
			foreach (_ICase icase in \u0002._Cases)
			{
				icase._Label.Accept(this);
				icase._Controlled.Accept(this);
			}
			if (\u0002._Else != null)
			{
				\u0002._Else.Accept(this);
			}
		}

		// Token: 0x0600099F RID: 2463 RVA: 0x000130C8 File Offset: 0x000112C8
		public virtual void \u0001(_IExitStatement \u0002)
		{
		}

		// Token: 0x060009A0 RID: 2464 RVA: 0x000130CC File Offset: 0x000112CC
		public virtual void \u0001(_IContinueStatement \u0002)
		{
		}

		// Token: 0x060009A1 RID: 2465 RVA: 0x000130D0 File Offset: 0x000112D0
		public virtual void \u0001(_IThisExpression \u0002)
		{
		}

		// Token: 0x060009A2 RID: 2466 RVA: 0x000130D4 File Offset: 0x000112D4
		public virtual void \u0001(_IBaseExpression \u0002)
		{
		}

		// Token: 0x060009A3 RID: 2467 RVA: 0x000130D8 File Offset: 0x000112D8
		public virtual void \u0001(_IEmptyStatement \u0002)
		{
		}

		// Token: 0x060009A4 RID: 2468 RVA: 0x000130DC File Offset: 0x000112DC
		public virtual void \u0001(_IReturnStatement \u0002)
		{
			if (\u0002._Condition != null)
			{
				\u0002._Condition.Accept(this);
			}
		}

		// Token: 0x060009A5 RID: 2469 RVA: 0x000130F4 File Offset: 0x000112F4
		public virtual void \u0001(_IJumpStatement \u0002)
		{
			if (\u0002._Condition != null)
			{
				\u0002._Condition.Accept(this);
			}
		}

		// Token: 0x060009A6 RID: 2470 RVA: 0x0001310C File Offset: 0x0001130C
		public virtual void \u0001(_ILabelStatement \u0002)
		{
		}

		// Token: 0x060009A7 RID: 2471 RVA: 0x00013110 File Offset: 0x00011310
		public virtual void \u0001(_ICommentStatement \u0002)
		{
		}

		// Token: 0x060009A8 RID: 2472 RVA: 0x00013114 File Offset: 0x00011314
		public virtual void \u0001(_IPragmaStatement \u0002)
		{
		}

		// Token: 0x060009A9 RID: 2473 RVA: 0x00013118 File Offset: 0x00011318
		public virtual void \u0001(_IErrorExpression \u0002)
		{
		}

		// Token: 0x060009AA RID: 2474 RVA: 0x0001311C File Offset: 0x0001131C
		public virtual void \u0001(_IErrorStatement \u0002)
		{
		}

		// Token: 0x060009AB RID: 2475 RVA: 0x00013120 File Offset: 0x00011320
		public virtual void \u0001(_INullExpression \u0002)
		{
		}

		// Token: 0x060009AC RID: 2476 RVA: 0x00013124 File Offset: 0x00011324
		public virtual void \u0001(_IVariableDeclarationStatement \u0002)
		{
			foreach (_IExpression iexpression in \u0002.NameList)
			{
				iexpression.Accept(this);
			}
			if (\u0002.Initial != null)
			{
				\u0002.Initial.Accept(this);
			}
			if (\u0002.InputAssigns != null)
			{
				foreach (_IAssignmentExpression iassignmentExpression in \u0002.InputAssigns)
				{
					iassignmentExpression.Accept(this);
				}
			}
		}

		// Token: 0x060009AD RID: 2477 RVA: 0x000131C8 File Offset: 0x000113C8
		public virtual void \u0001(_IVariableDeclarationListStatement \u0002)
		{
			\u0002.VariableDeclaration.Accept(this);
		}

		// Token: 0x060009AE RID: 2478 RVA: 0x000131D8 File Offset: 0x000113D8
		public virtual void \u0001(_IPOUDeclarationStatement \u0002)
		{
			\u0002.Declarations.Accept(this);
			if (\u0002.Implements != null)
			{
				foreach (_IExpression iexpression in \u0002.Implements)
				{
					iexpression.Accept(this);
				}
			}
			if (\u0002.Extends != null)
			{
				foreach (_IExpression iexpression2 in \u0002.Extends)
				{
					iexpression2.Accept(this);
				}
			}
		}

		// Token: 0x060009AF RID: 2479 RVA: 0x0001327C File Offset: 0x0001147C
		public virtual void \u0001(_ITypeDeclarationStatement \u0002)
		{
			\u0002.Declarations.Accept(this);
			if (\u0002.Initial != null)
			{
				\u0002.Initial.Accept(this);
			}
			if (\u0002.Extends != null)
			{
				\u0002.Extends.Accept(this);
			}
		}

		// Token: 0x060009B0 RID: 2480 RVA: 0x000132B4 File Offset: 0x000114B4
		public virtual void \u0001(_IEnumDeclarationStatement \u0002)
		{
			if (\u0002._Value != null)
			{
				\u0002._Value.Accept(this);
			}
		}

		// Token: 0x060009B1 RID: 2481 RVA: 0x000132CC File Offset: 0x000114CC
		public virtual void \u0001(_IEnumDeclarationListStatement \u0002)
		{
			foreach (_IEnumDeclarationStatement ienumDeclarationStatement in \u0002.Enums)
			{
				ienumDeclarationStatement.Accept(this);
			}
		}

		// Token: 0x060009B2 RID: 2482 RVA: 0x00013318 File Offset: 0x00011518
		public virtual void \u0001(_IMultipleIndexInitialization \u0002)
		{
			\u0002._Value.Accept(this);
			\u0002._Number.Accept(this);
		}

		// Token: 0x060009B3 RID: 2483 RVA: 0x00013334 File Offset: 0x00011534
		public virtual void \u0001(_IArrayInitialization \u0002)
		{
			foreach (_IExpression iexpression in \u0002._InitValues)
			{
				iexpression.Accept(this);
			}
		}

		// Token: 0x060009B4 RID: 2484 RVA: 0x00013380 File Offset: 0x00011580
		public virtual void \u0001(_IStructureInitialization \u0002)
		{
			foreach (_IAssignmentExpression iassignmentExpression in \u0002._CompoInits)
			{
				iassignmentExpression.Accept(this);
			}
		}

		// Token: 0x060009B5 RID: 2485 RVA: 0x000133CC File Offset: 0x000115CC
		public virtual void \u0001(_IDefineReference \u0002)
		{
		}

		// Token: 0x060009B6 RID: 2486 RVA: 0x000133D0 File Offset: 0x000115D0
		public virtual void \u0001(_IVariableReference \u0002)
		{
			if (\u0002.InstancePath != null)
			{
				\u0002.InstancePath.Accept(this);
			}
		}

		// Token: 0x060009B7 RID: 2487 RVA: 0x000133E8 File Offset: 0x000115E8
		public virtual void \u0001(_ITypeReference \u0002)
		{
			if (\u0002.InstancePath != null)
			{
				\u0002.InstancePath.Accept(this);
			}
		}

		// Token: 0x060009B8 RID: 2488 RVA: 0x00013400 File Offset: 0x00011600
		public virtual void \u0001(_IPouReference \u0002)
		{
			if (\u0002.InstancePath != null)
			{
				\u0002.InstancePath.Accept(this);
			}
		}

		// Token: 0x060009B9 RID: 2489 RVA: 0x00013418 File Offset: 0x00011618
		public virtual void \u0001(_ITaskReference \u0002)
		{
		}

		// Token: 0x060009BA RID: 2490 RVA: 0x0001341C File Offset: 0x0001161C
		public virtual void \u0001(_IResourceReference \u0002)
		{
		}

		// Token: 0x060009BB RID: 2491 RVA: 0x00013420 File Offset: 0x00011620
		public virtual void \u0001(_IDefinedExpression \u0002)
		{
			\u0002.ItemReference.Accept(this);
		}

		// Token: 0x060009BC RID: 2492 RVA: 0x00013430 File Offset: 0x00011630
		public virtual void \u0001(_IPragmaOperatorExpression \u0002)
		{
			foreach (_IExpression iexpression in \u0002.Operands)
			{
				iexpression.Accept(this);
			}
		}

		// Token: 0x060009BD RID: 2493 RVA: 0x0001347C File Offset: 0x0001167C
		public virtual void \u0001(_IPragmaIfStatement \u0002)
		{
			\u0002.Condition.Accept(this);
			\u0002.IfThen.Accept(this);
			foreach (_IPragmaElseIf ipragmaElseIf in \u0002.ElseIf)
			{
				ipragmaElseIf.Condition.Accept(this);
				ipragmaElseIf.Controlled.Accept(this);
			}
			if (\u0002.IfElse != null)
			{
				\u0002.IfElse.Accept(this);
			}
		}

		// Token: 0x060009BE RID: 2494 RVA: 0x00013504 File Offset: 0x00011704
		public virtual void \u0001(_IBreakPointStatement \u0002)
		{
		}

		// Token: 0x060009BF RID: 2495 RVA: 0x00013508 File Offset: 0x00011708
		public virtual void \u0001(_IDefineStatement \u0002)
		{
		}

		// Token: 0x060009C0 RID: 2496 RVA: 0x0001350C File Offset: 0x0001170C
		public virtual void \u0001(_IHasTypeExpression \u0002)
		{
			\u0002.Variable.Accept(this);
		}

		// Token: 0x060009C1 RID: 2497 RVA: 0x0001351C File Offset: 0x0001171C
		public virtual void \u0001(_IIsEnumTypeExpression \u0002)
		{
		}

		// Token: 0x060009C2 RID: 2498 RVA: 0x00013520 File Offset: 0x00011720
		public virtual void \u0001(_IHasAttributeExpression \u0002)
		{
			\u0002.ItemReference.Accept(this);
		}

		// Token: 0x060009C3 RID: 2499 RVA: 0x00013530 File Offset: 0x00011730
		public virtual void \u0001(_IHasValueExpression \u0002)
		{
		}

		// Token: 0x060009C4 RID: 2500 RVA: 0x00013534 File Offset: 0x00011734
		public virtual void \u0001(_IHasConstantValueExpression \u0002)
		{
		}

		// Token: 0x060009C5 RID: 2501 RVA: 0x00013538 File Offset: 0x00011738
		public virtual void \u0001(_IHasConstantTypeExpression \u0002)
		{
		}

		// Token: 0x060009C6 RID: 2502 RVA: 0x0001353C File Offset: 0x0001173C
		public virtual void \u0001(_IPragmaAssertion \u0002)
		{
			\u0002.Condition.Accept(this);
		}

		// Token: 0x060009C7 RID: 2503 RVA: 0x0001354C File Offset: 0x0001174C
		public virtual void \u0001(_ICompilerVersionExpression \u0002)
		{
		}

		// Token: 0x060009C8 RID: 2504 RVA: 0x00013550 File Offset: 0x00011750
		public virtual void \u0001(_IRuntimeVersionExpression \u0002)
		{
		}

		// Token: 0x060009C9 RID: 2505 RVA: 0x00013554 File Offset: 0x00011754
		public void \u0001(_ITryCatchStatement \u0002)
		{
			\u0002.DefaultTraverse(this);
		}

		// Token: 0x060009CA RID: 2506 RVA: 0x00013560 File Offset: 0x00011760
		public virtual void \u0001(_IPartialAccessExpression \u0002)
		{
			\u0002._Left.Accept(this);
		}

		// Token: 0x060009CB RID: 2507 RVA: 0x00013570 File Offset: 0x00011770
		public virtual void \u0001(_IProjectDefinedExpression \u0002)
		{
			_IDefineReference defineReference = \u0002.DefineReference;
			if (defineReference == null)
			{
				return;
			}
			defineReference.Accept(this);
		}
	}
}
