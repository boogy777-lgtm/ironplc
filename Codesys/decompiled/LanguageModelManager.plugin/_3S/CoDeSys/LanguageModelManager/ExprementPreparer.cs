using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000030 RID: 48
	[SuppressMessage("Major Code Smell", "S1200:Classes should not be coupled to too many other classes (Single Responsibility Principle)", Justification = "Visitor pattern")]
	internal class ExprementPreparer : IExprementVisitor352000, IExprementVisitor351900, IExprementVisitor351800, IExprementVisitor351500, IExprementVisitor351400, IExprementVisitor351300, IExprementVisitor3590, IExprementVisitor
	{
		// Token: 0x060001FF RID: 511 RVA: 0x000072B4 File Offset: 0x000062B4
		private void RemoveSuppressedWarnings(_IExprement expr)
		{
			if (expr.MessagesList == null || expr.MessagesList.Count == 0)
			{
				return;
			}
			expr.MessagesList = (from m in expr.MessagesList
			where !m.IsSuppressed()
			select m).ToList<_ICompilerMessage>();
		}

		// Token: 0x06000200 RID: 512 RVA: 0x0000730C File Offset: 0x0000630C
		public void visit(_ICompiledPOU cpou)
		{
			_IStatement parseTree = cpou.GetParseTree();
			if (parseTree != null)
			{
				parseTree.Accept(this);
			}
		}

		// Token: 0x06000201 RID: 513 RVA: 0x0000732C File Offset: 0x0000632C
		public void visit(_ISequenceStatement seq)
		{
			this.RemoveSuppressedWarnings(seq);
			IList<_IStatement> statementList = seq._StatementList;
			for (int i = 0; i < statementList.Count; i++)
			{
				_IImplicitCodeSectionPragma iimplicitCodeSectionPragma = statementList[i] as _IImplicitCodeSectionPragma;
				if (iimplicitCodeSectionPragma != null)
				{
					PragmaStatement pragmaStatement = new PragmaStatement
					{
						Text = iimplicitCodeSectionPragma.Text
					};
					pragmaStatement.DuplicateCommon((Exprement)iimplicitCodeSectionPragma);
					statementList[i] = pragmaStatement;
				}
				MethodDeclarationStatement methodDeclarationStatement = statementList[i] as MethodDeclarationStatement;
				if (methodDeclarationStatement != null)
				{
					POUDeclarationStatement poudeclarationStatement = new POUDeclarationStatement();
					methodDeclarationStatement.CopyContentTo(poudeclarationStatement);
					statementList[i] = poudeclarationStatement;
				}
				POUDeclarationStatement poudeclarationStatement2 = statementList[i] as POUDeclarationStatement;
				if (poudeclarationStatement2 != null && (poudeclarationStatement2.Class == Operator.PropertyGet || poudeclarationStatement2.Class == Operator.PropertySet))
				{
					PragmaStatement item = new PragmaStatement
					{
						Text = "attribute 'property'"
					};
					statementList.Insert(i, item);
					i++;
				}
				statementList[i].Accept(this);
			}
		}

		// Token: 0x06000202 RID: 514 RVA: 0x00007419 File Offset: 0x00006419
		public void visit(_IWhileStatement whilst)
		{
			this.RemoveSuppressedWarnings(whilst);
			whilst._Condition.Accept(this);
			whilst._Controlled.Accept(this);
		}

		// Token: 0x06000203 RID: 515 RVA: 0x0000743A File Offset: 0x0000643A
		public void visit(_IRepeatStatement repeat)
		{
			this.RemoveSuppressedWarnings(repeat);
			repeat._Controlled.Accept(this);
			repeat._Condition.Accept(this);
		}

		// Token: 0x06000204 RID: 516 RVA: 0x0000745C File Offset: 0x0000645C
		public void visit(_IForStatement forloop)
		{
			this.RemoveSuppressedWarnings(forloop);
			forloop._CounterStart.Accept(this);
			forloop._UpperBound.Accept(this);
			if (forloop.By != null)
			{
				forloop._By.Accept(this);
			}
			forloop._Controlled.Accept(this);
		}

		// Token: 0x06000205 RID: 517 RVA: 0x000074A8 File Offset: 0x000064A8
		public void visit(_IExitStatement exit)
		{
			this.RemoveSuppressedWarnings(exit);
		}

		// Token: 0x06000206 RID: 518 RVA: 0x000074A8 File Offset: 0x000064A8
		public void visit(_IContinueStatement cont)
		{
			this.RemoveSuppressedWarnings(cont);
		}

		// Token: 0x06000207 RID: 519 RVA: 0x000074B1 File Offset: 0x000064B1
		public void visit(_IAssignmentExpression assign)
		{
			this.RemoveSuppressedWarnings(assign);
			assign._LValue.Accept(this);
			assign._RValue.Accept(this);
		}

		// Token: 0x06000208 RID: 520 RVA: 0x000074D4 File Offset: 0x000064D4
		public void visit(_IIfStatement ifst)
		{
			this.RemoveSuppressedWarnings(ifst);
			ifst._Condition.Accept(this);
			ifst._IfThen.Accept(this);
			foreach (_IElseIf ielseIf in ifst._ElseIf)
			{
				ielseIf._Condition.Accept(this);
				ielseIf._Controlled.Accept(this);
			}
			if (ifst._IfElse != null)
			{
				ifst._IfElse.Accept(this);
			}
		}

		// Token: 0x06000209 RID: 521 RVA: 0x00007564 File Offset: 0x00006564
		public void visit(_IReturnStatement returnst)
		{
			if (returnst._Condition != null)
			{
				returnst._Condition.Accept(this);
			}
			this.RemoveSuppressedWarnings(returnst);
		}

		// Token: 0x0600020A RID: 522 RVA: 0x00007581 File Offset: 0x00006581
		public void visit(_IJumpStatement gotost)
		{
			if (gotost._Condition != null)
			{
				gotost._Condition.Accept(this);
			}
			this.RemoveSuppressedWarnings(gotost);
		}

		// Token: 0x0600020B RID: 523 RVA: 0x000074A8 File Offset: 0x000064A8
		public void visit(_ILabelStatement label)
		{
			this.RemoveSuppressedWarnings(label);
		}

		// Token: 0x0600020C RID: 524 RVA: 0x000074A8 File Offset: 0x000064A8
		public void visit(_ICommentStatement comment)
		{
			this.RemoveSuppressedWarnings(comment);
		}

		// Token: 0x0600020D RID: 525 RVA: 0x000074A8 File Offset: 0x000064A8
		public void visit(_IPragmaStatement pragma)
		{
			this.RemoveSuppressedWarnings(pragma);
		}

		// Token: 0x0600020E RID: 526 RVA: 0x0000759E File Offset: 0x0000659E
		public void visit(_IExpressionStatement expstat)
		{
			this.RemoveSuppressedWarnings(expstat);
			expstat._Expr.Accept(this);
		}

		// Token: 0x0600020F RID: 527 RVA: 0x000075B3 File Offset: 0x000065B3
		public void visit(_IVariableDeclarationStatement vds)
		{
			this.RemoveSuppressedWarnings(vds);
			_IExpression initial = vds.Initial;
			if (initial == null)
			{
				return;
			}
			initial.Accept(this);
		}

		// Token: 0x06000210 RID: 528 RVA: 0x000075CD File Offset: 0x000065CD
		public void visit(_IVariableDeclarationListStatement vdls)
		{
			this.RemoveSuppressedWarnings(vdls);
			vdls.VariableDeclaration.Accept(this);
		}

		// Token: 0x06000211 RID: 529 RVA: 0x000075E4 File Offset: 0x000065E4
		private void InsertPropertyVariable(_IPOUDeclarationStatement pds)
		{
			VariableDeclarationStatement variableDeclarationStatement = new VariableDeclarationStatement();
			variableDeclarationStatement.AddName(new VariableExpression(pds.Name));
			variableDeclarationStatement.Type = pds.Type;
			if (pds.DeclarationLists == null)
			{
				pds.DeclarationLists = new List<IVariableDeclarationListStatement>();
			}
			bool flag = false;
			foreach (_IStatement istatement in pds.DeclarationLists.OfType<_IStatement>())
			{
				VariableDeclarationListStatement variableDeclarationListStatement = istatement as VariableDeclarationListStatement;
				if (variableDeclarationListStatement != null && variableDeclarationListStatement.Flags == VarFlag.Local)
				{
					variableDeclarationListStatement.InsertVariableDeclaration(0, variableDeclarationStatement);
					flag = true;
				}
			}
			if (flag)
			{
				return;
			}
			VariableDeclarationListStatement variableDeclarationListStatement2 = new VariableDeclarationListStatement
			{
				Flags = VarFlag.Local
			};
			variableDeclarationListStatement2.AddVariableDeclaration(variableDeclarationStatement);
			pds.DeclarationLists.Add(variableDeclarationListStatement2);
		}

		// Token: 0x06000212 RID: 530 RVA: 0x000076AC File Offset: 0x000066AC
		public void visit(_IPOUDeclarationStatement pds)
		{
			this.RemoveSuppressedWarnings(pds);
			pds.Declarations.Accept(this);
			if (pds.Class == Operator.PropertyGet)
			{
				if (!this.InterfaceProperty)
				{
					this.InsertPropertyVariable(pds);
				}
				pds.Name = "__get" + pds.Name;
				pds.NameExpression = null;
				pds.Class = Operator.Method;
			}
			if (pds.Class == Operator.PropertySet)
			{
				pds.AddVariableDeclaration(VarFlag.Input, pds.Name, pds.Type, null);
				pds.Name = "__set" + pds.Name;
				pds.NameExpression = null;
				pds.Class = Operator.Method;
				pds.Type = TypeTable.Bool;
			}
		}

		// Token: 0x06000213 RID: 531 RVA: 0x0000775F File Offset: 0x0000675F
		public void visit(_ITypeDeclarationStatement tds)
		{
			this.RemoveSuppressedWarnings(tds);
			tds.Declarations.Accept(this);
			if (tds.Initial != null)
			{
				tds.Initial.Accept(this);
			}
		}

		// Token: 0x06000214 RID: 532 RVA: 0x000074A8 File Offset: 0x000064A8
		public void visit(_IEnumDeclarationStatement eds)
		{
			this.RemoveSuppressedWarnings(eds);
		}

		// Token: 0x06000215 RID: 533 RVA: 0x00007788 File Offset: 0x00006788
		public void visit(_IEnumDeclarationListStatement eds)
		{
			this.RemoveSuppressedWarnings(eds);
			foreach (_IEnumDeclarationStatement ienumDeclarationStatement in eds.Enums)
			{
				ienumDeclarationStatement.Accept(this);
			}
		}

		// Token: 0x06000216 RID: 534 RVA: 0x000077DC File Offset: 0x000067DC
		[SuppressMessage("Critical Code Smell", "S3776:Cognitive Complexity of methods should not be too high", Justification = "Close enough")]
		public void visit(_ICallExpression call)
		{
			this.RemoveSuppressedWarnings(call);
			call._Callee.Accept(this);
			if (call._Condition != null)
			{
				call._Condition.Accept(this);
			}
			foreach (_IExpression iexpression in call.ParamExpressions)
			{
				if (iexpression != null)
				{
					iexpression.Accept(this);
				}
			}
			foreach (_IExpression iexpression2 in call.OutputExpressions)
			{
				if (iexpression2 != null)
				{
					iexpression2.Accept(this);
				}
			}
			foreach (_IExpression iexpression3 in call.Inputs)
			{
				if (iexpression3 != null)
				{
					iexpression3.Accept(this);
				}
			}
			foreach (_IExpression iexpression4 in call.Outputs)
			{
				if (iexpression4 != null)
				{
					iexpression4.Accept(this);
				}
			}
			foreach (_IExpression iexpression5 in call.EmptyAssigns)
			{
				if (iexpression5 != null)
				{
					iexpression5.Accept(this);
				}
			}
		}

		// Token: 0x06000217 RID: 535 RVA: 0x00007958 File Offset: 0x00006958
		public void visit(_IOperatorExpression op)
		{
			this.RemoveSuppressedWarnings(op);
			foreach (_IExpression iexpression in op._OperandsList)
			{
				iexpression.Accept(this);
			}
		}

		// Token: 0x06000218 RID: 536 RVA: 0x000079AC File Offset: 0x000069AC
		public void visit(_ICastExpression castexp)
		{
			this.RemoveSuppressedWarnings(castexp);
			castexp.BaseExpression.Accept(this);
			if (castexp.ExpWithType != null)
			{
				castexp.ExpWithType.Accept(this);
			}
		}

		// Token: 0x06000219 RID: 537 RVA: 0x000079D8 File Offset: 0x000069D8
		public void visit(_INewExpression typeref)
		{
			this.RemoveSuppressedWarnings(typeref);
			typeref._Count.Accept(this);
			if (typeref._FBInitParams != null)
			{
				foreach (_IAssignmentExpression iassignmentExpression in typeref._FBInitParams.OfType<_IAssignmentExpression>())
				{
					iassignmentExpression.Accept(this);
				}
			}
		}

		// Token: 0x0600021A RID: 538 RVA: 0x000074A8 File Offset: 0x000064A8
		public void visit(_ITypeExpression typeexp)
		{
			this.RemoveSuppressedWarnings(typeexp);
		}

		// Token: 0x0600021B RID: 539 RVA: 0x00007A44 File Offset: 0x00006A44
		public void visit(_IConversionExpression conv)
		{
			this.RemoveSuppressedWarnings(conv);
			conv._Exp.Accept(this);
		}

		// Token: 0x0600021C RID: 540 RVA: 0x000074A8 File Offset: 0x000064A8
		public void visit(_IThisExpression thisexp)
		{
			this.RemoveSuppressedWarnings(thisexp);
		}

		// Token: 0x0600021D RID: 541 RVA: 0x000074A8 File Offset: 0x000064A8
		public void visit(_IBaseExpression baseexp)
		{
			this.RemoveSuppressedWarnings(baseexp);
		}

		// Token: 0x0600021E RID: 542 RVA: 0x000074A8 File Offset: 0x000064A8
		public void visit(_ILiteralExpression literal)
		{
			this.RemoveSuppressedWarnings(literal);
		}

		// Token: 0x0600021F RID: 543 RVA: 0x000074A8 File Offset: 0x000064A8
		public void visit(_IAddressExpression address)
		{
			this.RemoveSuppressedWarnings(address);
		}

		// Token: 0x06000220 RID: 544 RVA: 0x000074A8 File Offset: 0x000064A8
		public void visit(_IQualifiedNameExpression qne)
		{
			this.RemoveSuppressedWarnings(qne);
		}

		// Token: 0x06000221 RID: 545 RVA: 0x000074A8 File Offset: 0x000064A8
		public void visit(_IVariableExpression variable)
		{
			this.RemoveSuppressedWarnings(variable);
		}

		// Token: 0x06000222 RID: 546 RVA: 0x00007A5C File Offset: 0x00006A5C
		public void visit(_IIndexAccessExpression indexaccess)
		{
			this.RemoveSuppressedWarnings(indexaccess);
			indexaccess._Var.Accept(this);
			for (int i = 0; i < indexaccess.NumAccesses; i++)
			{
				indexaccess.GetAccess(i).Accept(this);
			}
		}

		// Token: 0x06000223 RID: 547 RVA: 0x00007A9A File Offset: 0x00006A9A
		public void visit(_ICompoAccessExpression compo)
		{
			this.RemoveSuppressedWarnings(compo);
			compo._Left.Accept(this);
			compo._Right.Accept(this);
		}

		// Token: 0x06000224 RID: 548 RVA: 0x00007ABB File Offset: 0x00006ABB
		public void visit(_IDeRefAccessExpression deref)
		{
			this.RemoveSuppressedWarnings(deref);
			deref._Base.Accept(this);
		}

		// Token: 0x06000225 RID: 549 RVA: 0x00007AD0 File Offset: 0x00006AD0
		public void visit(_ICopyScopeExpression copyexp)
		{
			this.RemoveSuppressedWarnings(copyexp);
			copyexp._Base.Accept(this);
		}

		// Token: 0x06000226 RID: 550 RVA: 0x00007AE5 File Offset: 0x00006AE5
		public void visit(_IGlobalScopeExpression globexp)
		{
			this.RemoveSuppressedWarnings(globexp);
			globexp._Base.Accept(this);
		}

		// Token: 0x06000227 RID: 551 RVA: 0x00007AFA File Offset: 0x00006AFA
		public void visit(_ISystemScopeExpression systemscope)
		{
			this.RemoveSuppressedWarnings(systemscope);
			systemscope._Base.Accept(this);
		}

		// Token: 0x06000228 RID: 552 RVA: 0x00007B0F File Offset: 0x00006B0F
		public void visit(_IPoolScopeExpression poolscope)
		{
			this.RemoveSuppressedWarnings(poolscope);
			poolscope._Base.Accept(this);
		}

		// Token: 0x06000229 RID: 553 RVA: 0x00007B24 File Offset: 0x00006B24
		public void visit(_INamespaceAccessExpression namespaceaccess)
		{
			this.RemoveSuppressedWarnings(namespaceaccess);
			_IExpression access = namespaceaccess._Access;
			if (access == null)
			{
				return;
			}
			access.Accept(this);
		}

		// Token: 0x0600022A RID: 554 RVA: 0x00007B3E File Offset: 0x00006B3E
		public void visit(_ICurrentTaskExpression currentTask)
		{
			this.RemoveSuppressedWarnings(currentTask);
			currentTask._Base.Accept(this);
		}

		// Token: 0x0600022B RID: 555 RVA: 0x000074A8 File Offset: 0x000064A8
		public void visit(_IEmptyStatement empty)
		{
			this.RemoveSuppressedWarnings(empty);
		}

		// Token: 0x0600022C RID: 556 RVA: 0x00007B53 File Offset: 0x00006B53
		public void visit(_ICaseRangeExpression caserange)
		{
			this.RemoveSuppressedWarnings(caserange);
			caserange._Low.Accept(this);
			caserange._High.Accept(this);
		}

		// Token: 0x0600022D RID: 557 RVA: 0x00007B74 File Offset: 0x00006B74
		public void visit(_ICaseLabelStatement caselabel)
		{
			this.RemoveSuppressedWarnings(caselabel);
			foreach (_IExpression iexpression in caselabel._cases)
			{
				iexpression.Accept(this);
			}
		}

		// Token: 0x0600022E RID: 558 RVA: 0x00007BC8 File Offset: 0x00006BC8
		public void visit(_ICaseStatement casest)
		{
			this.RemoveSuppressedWarnings(casest);
			casest._Switch.Accept(this);
			foreach (_ICase icase in casest._Cases)
			{
				icase._Label.Accept(this);
				icase._Controlled.Accept(this);
			}
			if (casest._Else != null)
			{
				casest._Else.Accept(this);
			}
		}

		// Token: 0x0600022F RID: 559 RVA: 0x000074A8 File Offset: 0x000064A8
		public void visit(_IErrorExpression errorexp)
		{
			this.RemoveSuppressedWarnings(errorexp);
		}

		// Token: 0x06000230 RID: 560 RVA: 0x000074A8 File Offset: 0x000064A8
		public void visit(_IErrorStatement errorst)
		{
			this.RemoveSuppressedWarnings(errorst);
		}

		// Token: 0x06000231 RID: 561 RVA: 0x000074A8 File Offset: 0x000064A8
		public void visit(_INullExpression errorexp)
		{
			this.RemoveSuppressedWarnings(errorexp);
		}

		// Token: 0x06000232 RID: 562 RVA: 0x000074A8 File Offset: 0x000064A8
		public void visit(_INullStatement errorst)
		{
			this.RemoveSuppressedWarnings(errorst);
		}

		// Token: 0x06000233 RID: 563 RVA: 0x00007C4C File Offset: 0x00006C4C
		public void visit(_IMultipleIndexInitialization errorst)
		{
			this.RemoveSuppressedWarnings(errorst);
			errorst._Number.Accept(this);
			errorst._Value.Accept(this);
		}

		// Token: 0x06000234 RID: 564 RVA: 0x00007C70 File Offset: 0x00006C70
		public void visit(_IArrayInitialization errorexp)
		{
			this.RemoveSuppressedWarnings(errorexp);
			foreach (_IExpression iexpression in errorexp._InitValues)
			{
				iexpression.Accept(this);
			}
		}

		// Token: 0x06000235 RID: 565 RVA: 0x00007CC4 File Offset: 0x00006CC4
		public void visit(_IStructureInitialization errorst)
		{
			this.RemoveSuppressedWarnings(errorst);
			foreach (_IAssignmentExpression iassignmentExpression in errorst._CompoInits)
			{
				iassignmentExpression.Accept(this);
			}
		}

		// Token: 0x06000236 RID: 566 RVA: 0x000074A8 File Offset: 0x000064A8
		public void visit(_IDefineReference defref)
		{
			this.RemoveSuppressedWarnings(defref);
		}

		// Token: 0x06000237 RID: 567 RVA: 0x00007D18 File Offset: 0x00006D18
		public void visit(_IVariableReference varref)
		{
			this.RemoveSuppressedWarnings(varref);
			_IExpression instancePath = varref.InstancePath;
			if (instancePath == null)
			{
				return;
			}
			instancePath.Accept(this);
		}

		// Token: 0x06000238 RID: 568 RVA: 0x00007D32 File Offset: 0x00006D32
		public void visit(_ITypeReference typeref)
		{
			this.RemoveSuppressedWarnings(typeref);
			_IExpression instancePath = typeref.InstancePath;
			if (instancePath == null)
			{
				return;
			}
			instancePath.Accept(this);
		}

		// Token: 0x06000239 RID: 569 RVA: 0x00007D4C File Offset: 0x00006D4C
		public void visit(_IPouReference pouref)
		{
			this.RemoveSuppressedWarnings(pouref);
			_IExpression instancePath = pouref.InstancePath;
			if (instancePath == null)
			{
				return;
			}
			instancePath.Accept(this);
		}

		// Token: 0x0600023A RID: 570 RVA: 0x000074A8 File Offset: 0x000064A8
		public void visit(_ITaskReference taskref)
		{
			this.RemoveSuppressedWarnings(taskref);
		}

		// Token: 0x0600023B RID: 571 RVA: 0x000074A8 File Offset: 0x000064A8
		public void visit(_IResourceReference resref)
		{
			this.RemoveSuppressedWarnings(resref);
		}

		// Token: 0x0600023C RID: 572 RVA: 0x00007D66 File Offset: 0x00006D66
		public void visit(_IDefinedExpression defexp)
		{
			this.RemoveSuppressedWarnings(defexp);
			defexp.ItemReference.Accept(this);
		}

		// Token: 0x0600023D RID: 573 RVA: 0x00007D7C File Offset: 0x00006D7C
		public void visit(_IPragmaOperatorExpression popexp)
		{
			this.RemoveSuppressedWarnings(popexp);
			foreach (_IExpression iexpression in popexp.Operands)
			{
				iexpression.Accept(this);
			}
		}

		// Token: 0x0600023E RID: 574 RVA: 0x00007DD0 File Offset: 0x00006DD0
		public void visit(_IPragmaIfStatement pifst)
		{
			this.RemoveSuppressedWarnings(pifst);
			pifst.Condition.Accept(this);
			pifst.IfThen.Accept(this);
			foreach (_IPragmaElseIf ipragmaElseIf in pifst.ElseIf)
			{
				ipragmaElseIf.Condition.Accept(this);
				ipragmaElseIf.Controlled.Accept(this);
			}
			_IStatement ifElse = pifst.IfElse;
			if (ifElse == null)
			{
				return;
			}
			ifElse.Accept(this);
		}

		// Token: 0x0600023F RID: 575 RVA: 0x00007E5C File Offset: 0x00006E5C
		public void visit(_IPragmaAssertion assertion)
		{
			this.RemoveSuppressedWarnings(assertion);
			assertion.Condition.Accept(this);
		}

		// Token: 0x06000240 RID: 576 RVA: 0x000074A8 File Offset: 0x000064A8
		public void visit(_IProjectDefinedExpression projectDefinedExpression)
		{
			this.RemoveSuppressedWarnings(projectDefinedExpression);
		}

		// Token: 0x06000241 RID: 577 RVA: 0x000074A8 File Offset: 0x000064A8
		public void visit(_ICompilerVersionExpression compiversionexp)
		{
			this.RemoveSuppressedWarnings(compiversionexp);
		}

		// Token: 0x06000242 RID: 578 RVA: 0x000074A8 File Offset: 0x000064A8
		public void visit(_IRuntimeVersionExpression runtimeversionexp)
		{
			this.RemoveSuppressedWarnings(runtimeversionexp);
		}

		// Token: 0x06000243 RID: 579 RVA: 0x000074A8 File Offset: 0x000064A8
		public void visit(_IBreakPointStatement bpstate)
		{
			this.RemoveSuppressedWarnings(bpstate);
		}

		// Token: 0x06000244 RID: 580 RVA: 0x000074A8 File Offset: 0x000064A8
		public void visit(_IDefineStatement defstate)
		{
			this.RemoveSuppressedWarnings(defstate);
		}

		// Token: 0x06000245 RID: 581 RVA: 0x00007E71 File Offset: 0x00006E71
		public void visit(_IXRefExpression xref)
		{
			this.RemoveSuppressedWarnings(xref);
			if (xref.XRef != null)
			{
				xref.XRef.Accept(this);
			}
			if (xref.XRefFrom != null)
			{
				xref.XRefFrom.Accept(this);
			}
		}

		// Token: 0x06000246 RID: 582 RVA: 0x00007EA2 File Offset: 0x00006EA2
		public void visit(_IHasTypeExpression hastype)
		{
			this.RemoveSuppressedWarnings(hastype);
			if (hastype.Variable != null)
			{
				hastype.Variable.Accept(this);
			}
		}

		// Token: 0x06000247 RID: 583 RVA: 0x000074A8 File Offset: 0x000064A8
		public void visit(_IIsEnumTypeExpression isenumtype)
		{
			this.RemoveSuppressedWarnings(isenumtype);
		}

		// Token: 0x06000248 RID: 584 RVA: 0x00007EBF File Offset: 0x00006EBF
		public void visit(_IHasAttributeExpression hasattribute)
		{
			this.RemoveSuppressedWarnings(hasattribute);
			if (hasattribute.ItemReference != null)
			{
				hasattribute.ItemReference.Accept(this);
			}
		}

		// Token: 0x06000249 RID: 585 RVA: 0x000074A8 File Offset: 0x000064A8
		public void visit(_IHasValueExpression hasvalue)
		{
			this.RemoveSuppressedWarnings(hasvalue);
		}

		// Token: 0x0600024A RID: 586 RVA: 0x000074A8 File Offset: 0x000064A8
		public void visit(_IHasConstantValueExpression hasvalue)
		{
			this.RemoveSuppressedWarnings(hasvalue);
		}

		// Token: 0x0600024B RID: 587 RVA: 0x000074A8 File Offset: 0x000064A8
		public void visit(_IHasConstantTypeExpression hasConstantTypeExpression)
		{
			this.RemoveSuppressedWarnings(hasConstantTypeExpression);
		}

		// Token: 0x0600024C RID: 588 RVA: 0x00007EDC File Offset: 0x00006EDC
		public void visit(_IPartialAccessExpression partialAccessExpression)
		{
			this.RemoveSuppressedWarnings(partialAccessExpression);
			partialAccessExpression._Left.Accept(this);
		}

		// Token: 0x0600024D RID: 589 RVA: 0x00007EF1 File Offset: 0x00006EF1
		public void visit(_ITryCatchStatement trycatchstatement)
		{
			this.RemoveSuppressedWarnings(trycatchstatement);
			trycatchstatement.DefaultTraverse(this);
		}

		// Token: 0x17000064 RID: 100
		// (get) Token: 0x0600024E RID: 590 RVA: 0x00007F01 File Offset: 0x00006F01
		// (set) Token: 0x0600024F RID: 591 RVA: 0x00007F09 File Offset: 0x00006F09
		public bool InterfaceProperty { get; set; }
	}
}
