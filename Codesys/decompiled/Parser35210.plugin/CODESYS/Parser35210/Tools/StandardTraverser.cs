using System.Diagnostics.CodeAnalysis;
using System.Linq;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace CODESYS.Parser35210.Tools
{
	[ExcludeFromCodeCoverage]
	public sealed class StandardTraverser : IExprementVisitor352000, IExprementVisitor351900, IExprementVisitor351800, IExprementVisitor351500, IExprementVisitor351400, IExprementVisitor351300, IExprementVisitor3590, IExprementVisitor
	{
		private readonly IExprementVisitor352000 _expCalledForAll;

		public StandardTraverser(IExprementVisitor352000 expCalledForAll)
		{
			_expCalledForAll = expCalledForAll;
		}

		public void visit(_IAddressExpression address)
		{
			_expCalledForAll.visit(address);
		}

		public void visit(_IVariableExpression variable)
		{
			_expCalledForAll.visit(variable);
		}

		public void visit(_ICompiledPOU cpou)
		{
			if (cpou.GetFlag(CompiledPOUFlags.ContainsDirVarAccess))
			{
				cpou.GetParseTree().Accept(this);
			}
			_expCalledForAll.visit(cpou);
		}

		public void visit(_IWhileStatement whilst)
		{
			whilst._Condition.Accept(this);
			whilst._Controlled.Accept(this);
			_expCalledForAll.visit(whilst);
		}

		public void visit(_IRepeatStatement repeat)
		{
			repeat._Condition.Accept(this);
			repeat._Controlled.Accept(this);
			_expCalledForAll.visit(repeat);
		}

		public void visit(_IForStatement forloop)
		{
			forloop._CounterStart.Accept(this);
			forloop._UpperBound.Accept(this);
			if (forloop.By != null)
			{
				forloop._By.Accept(this);
			}
			forloop._Controlled.Accept(this);
			_expCalledForAll.visit(forloop);
		}

		public void visit(_ISequenceStatement seq)
		{
			foreach (_IStatement statement in seq._StatementList)
			{
				statement.Accept(this);
			}
			_expCalledForAll.visit(seq);
		}

		public void visit(_IIfStatement ifst)
		{
			ifst._Condition.Accept(this);
			ifst._IfThen.Accept(this);
			foreach (_IElseIf item in ifst._ElseIf)
			{
				item._Condition.Accept(this);
				item._Controlled.Accept(this);
			}
			ifst._IfElse?.Accept(this);
			_expCalledForAll.visit(ifst);
		}

		public void visit(_IExpressionStatement expstat)
		{
			expstat._Expr.Accept(this);
			_expCalledForAll.visit(expstat);
		}

		public void visit(_IAssignmentExpression assign)
		{
			assign._LValue.Accept(this);
			assign._RValue.Accept(this);
			_expCalledForAll.visit(assign);
		}

		public void visit(_ICallExpression call)
		{
			call._Callee.Accept(this);
			if (call._Condition != null)
			{
				call._Condition.Accept(this);
			}
			VisitInputs(call);
			foreach (_IExpression outputExpression in call.OutputExpressions)
			{
				outputExpression?.Accept(this);
			}
			foreach (_IExpression input in call.Inputs)
			{
				input?.Accept(this);
			}
			foreach (_IExpression output in call.Outputs)
			{
				output?.Accept(this);
			}
			_expCalledForAll.visit(call);
		}

		public void visit(_IOperatorExpression op)
		{
			foreach (_IExpression operands in op._OperandsList)
			{
				operands.Accept(this);
			}
			_expCalledForAll.visit(op);
		}

		public void visit(_ICastExpression castexp)
		{
			castexp.BaseExpression.Accept(this);
			_expCalledForAll.visit(castexp);
		}

		public void visit(_INewExpression typeref)
		{
			typeref._Count.Accept(this);
			if (typeref._FBInitParams != null)
			{
				foreach (_IAssignmentExpression item in typeref._FBInitParams.OfType<_IAssignmentExpression>())
				{
					item.Accept(this);
				}
			}
			_expCalledForAll.visit(typeref);
		}

		public void visit(_ITypeExpression typeexp)
		{
			_expCalledForAll.visit(typeexp);
		}

		public void visit(_IConversionExpression conv)
		{
			conv._Exp.Accept(this);
			_expCalledForAll.visit(conv);
		}

		public void visit(_IIndexAccessExpression indexaccess)
		{
			for (int i = 0; i < indexaccess.NumAccesses; i++)
			{
				indexaccess.GetAccess(i).Accept(this);
			}
			indexaccess._Var.Accept(this);
			_expCalledForAll.visit(indexaccess);
		}

		public void visit(_ILiteralExpression literal)
		{
			_expCalledForAll.visit(literal);
		}

		public void visit(_ICompoAccessExpression compo)
		{
			compo._Right.Accept(this);
			compo._Left.Accept(this);
			_expCalledForAll.visit(compo);
		}

		public void visit(_IDeRefAccessExpression deref)
		{
			deref._Base.Accept(this);
			_expCalledForAll.visit(deref);
		}

		public void visit(_ICopyScopeExpression copyexp)
		{
			copyexp._Base.Accept(this);
			_expCalledForAll.visit(copyexp);
		}

		public void visit(_IGlobalScopeExpression globexp)
		{
			globexp._Base.Accept(this);
			_expCalledForAll.visit(globexp);
		}

		public void visit(_ISystemScopeExpression systemscope)
		{
			systemscope._Base.Accept(this);
			_expCalledForAll.visit(systemscope);
		}

		public void visit(_IPoolScopeExpression poolscope)
		{
			poolscope._Base.Accept(this);
			_expCalledForAll.visit(poolscope);
		}

		public void visit(_INamespaceAccessExpression namespaceaccess)
		{
			namespaceaccess._Namespace.Accept(this);
			namespaceaccess._Access?.Accept(this);
			(_expCalledForAll as IExprementVisitorNoTraversion351500)?.visit(namespaceaccess);
		}

		public void visit(_ICurrentTaskExpression currentTask)
		{
			currentTask._Base.Accept(this);
			_expCalledForAll.visit(currentTask);
		}

		public void visit(_ICaseRangeExpression caserange)
		{
			caserange._Low.Accept(this);
			caserange._High.Accept(this);
			_expCalledForAll.visit(caserange);
		}

		public void visit(_ICaseLabelStatement caselabel)
		{
			foreach (_IExpression @case in caselabel._cases)
			{
				@case.Accept(this);
			}
			_expCalledForAll.visit(caselabel);
		}

		public void visit(_ICaseStatement casest)
		{
			casest._Switch.Accept(this);
			foreach (_ICase @case in casest._Cases)
			{
				@case._Label.Accept(this);
				@case._Controlled.Accept(this);
			}
			if (casest._Else != null)
			{
				casest._Else.Accept(this);
			}
			_expCalledForAll.visit(casest);
		}

		public void visit(_IExitStatement exit)
		{
			_expCalledForAll.visit(exit);
		}

		public void visit(_IContinueStatement cont)
		{
			_expCalledForAll.visit(cont);
		}

		public void visit(_IThisExpression thisexp)
		{
			_expCalledForAll.visit(thisexp);
		}

		public void visit(_IBaseExpression baseexp)
		{
			_expCalledForAll.visit(baseexp);
		}

		public void visit(_IEmptyStatement empty)
		{
			_expCalledForAll.visit(empty);
		}

		public void visit(_IReturnStatement returnst)
		{
			if (returnst._Condition != null)
			{
				returnst._Condition.Accept(this);
			}
			_expCalledForAll.visit(returnst);
		}

		public void visit(_IJumpStatement gotost)
		{
			if (gotost._Condition != null)
			{
				gotost._Condition.Accept(this);
			}
			_expCalledForAll.visit(gotost);
		}

		public void visit(_ILabelStatement label)
		{
			_expCalledForAll.visit(label);
		}

		public void visit(_ICommentStatement comment)
		{
			_expCalledForAll.visit(comment);
		}

		public void visit(_IPragmaStatement pragma)
		{
			_expCalledForAll.visit(pragma);
		}

		public void visit(_IErrorExpression errorexp)
		{
			_expCalledForAll.visit(errorexp);
		}

		public void visit(_IErrorStatement errorst)
		{
			_expCalledForAll.visit(errorst);
		}

		public void visit(_INullExpression errorexp)
		{
			_expCalledForAll.visit(errorexp);
		}

		public void visit(_INullStatement errorst)
		{
			_expCalledForAll.visit(errorst);
		}

		public void visit(_IQualifiedNameExpression qne)
		{
			_expCalledForAll.visit(qne);
		}

		public void visit(_IVariableDeclarationStatement vds)
		{
			foreach (_IExpression name in vds.NameList)
			{
				name.Accept(this);
			}
			vds.Initial.Accept(this);
			if (vds.InputAssigns != null)
			{
				foreach (_IAssignmentExpression inputAssign in vds.InputAssigns)
				{
					inputAssign.Accept(this);
				}
			}
			_expCalledForAll.visit(vds);
		}

		public void visit(_IVariableDeclarationListStatement vdls)
		{
			vdls.VariableDeclaration.Accept(this);
			_expCalledForAll.visit(vdls);
		}

		public void visit(_IPOUDeclarationStatement pds)
		{
			pds.Declarations.Accept(this);
			if (pds.Implements != null)
			{
				foreach (_IExpression implement in pds.Implements)
				{
					implement.Accept(this);
				}
			}
			if (pds.Extends != null)
			{
				foreach (_IExpression extend in pds.Extends)
				{
					extend.Accept(this);
				}
			}
			_expCalledForAll.visit(pds);
		}

		public void visit(_ITypeDeclarationStatement tds)
		{
			tds.Declarations.Accept(this);
			if (tds.Initial != null)
			{
				tds.Initial.Accept(this);
			}
			if (tds.Extends != null)
			{
				tds.Extends.Accept(this);
			}
			_expCalledForAll.visit(tds);
		}

		public void visit(_IEnumDeclarationStatement eds)
		{
			if (eds._Value != null)
			{
				eds._Value.Accept(this);
			}
			_expCalledForAll.visit(eds);
		}

		public void visit(_IEnumDeclarationListStatement eds)
		{
			foreach (_IEnumDeclarationStatement @enum in eds.Enums)
			{
				@enum.Accept(this);
			}
			_expCalledForAll.visit(eds);
		}

		public void visit(_IMultipleIndexInitialization errorst)
		{
			errorst._Value.Accept(this);
			errorst._Number.Accept(this);
			_expCalledForAll.visit(errorst);
		}

		public void visit(_IArrayInitialization errorexp)
		{
			foreach (_IExpression initValue in errorexp._InitValues)
			{
				initValue.Accept(this);
			}
			_expCalledForAll.visit(errorexp);
		}

		public void visit(_IStructureInitialization errorst)
		{
			foreach (_IAssignmentExpression compoInit in errorst._CompoInits)
			{
				compoInit.Accept(this);
			}
			_expCalledForAll.visit(errorst);
		}

		public void visit(_IDefineReference defref)
		{
			_expCalledForAll.visit(defref);
		}

		public void visit(_IVariableReference varref)
		{
			if (varref.InstancePath != null)
			{
				varref.InstancePath.Accept(this);
			}
			_expCalledForAll.visit(varref);
		}

		public void visit(_ITypeReference typeref)
		{
			if (typeref.InstancePath != null)
			{
				typeref.InstancePath.Accept(this);
			}
			_expCalledForAll.visit(typeref);
		}

		public void visit(_IPouReference pouref)
		{
			if (pouref.InstancePath != null)
			{
				pouref.InstancePath.Accept(this);
			}
			_expCalledForAll.visit(pouref);
		}

		public void visit(_ITaskReference taskref)
		{
			_expCalledForAll.visit(taskref);
		}

		public void visit(_IResourceReference resref)
		{
			_expCalledForAll.visit(resref);
		}

		public void visit(_IDefinedExpression defexp)
		{
			defexp.ItemReference.Accept(this);
			_expCalledForAll.visit(defexp);
		}

		public void visit(_IPragmaOperatorExpression popexp)
		{
			foreach (_IExpression operand in popexp.Operands)
			{
				operand.Accept(this);
			}
			_expCalledForAll.visit(popexp);
		}

		public void visit(_IPragmaIfStatement pifst)
		{
			pifst.Condition.Accept(this);
			pifst.IfThen.Accept(this);
			foreach (_IPragmaElseIf item in pifst.ElseIf)
			{
				item.Condition.Accept(this);
				item.Controlled.Accept(this);
			}
			if (pifst.IfElse != null)
			{
				pifst.IfElse.Accept(this);
			}
			_expCalledForAll.visit(pifst);
		}

		public void visit(_IBreakPointStatement bpstate)
		{
			_expCalledForAll.visit(bpstate);
		}

		public void visit(_IDefineStatement defstate)
		{
			_expCalledForAll.visit(defstate);
		}

		public void visit(_IXRefExpression xref)
		{
			xref.XRef.Accept(this);
			xref.XRefFrom.Accept(this);
			_expCalledForAll.visit(xref);
		}

		public void visit(_IHasTypeExpression hastype)
		{
			hastype.Variable.Accept(this);
			_expCalledForAll.visit(hastype);
		}

		public void visit(_IIsEnumTypeExpression isenumtype)
		{
			_expCalledForAll.visit(isenumtype);
		}

		public void visit(_IHasAttributeExpression hasattribute)
		{
			hasattribute.ItemReference.Accept(this);
			_expCalledForAll.visit(hasattribute);
		}

		public void visit(_IHasValueExpression hasvalue)
		{
			_expCalledForAll.visit(hasvalue);
		}

		public void visit(_IHasConstantValueExpression hasvalue)
		{
			hasvalue._Constant.Accept(this);
			_expCalledForAll.visit(hasvalue);
		}

		public void visit(_IHasConstantTypeExpression hasConstantTypeExpression)
		{
			hasConstantTypeExpression._Constant.Accept(this);
			(_expCalledForAll as IExprementVisitorNoTraversion351800)?.visit(hasConstantTypeExpression);
		}

		public void visit(_IPragmaAssertion assertion)
		{
			assertion.Condition.Accept(this);
			_expCalledForAll.visit(assertion);
		}

		public void visit(_ICompilerVersionExpression compiversionexp)
		{
			_expCalledForAll.visit(compiversionexp);
		}

		public void visit(_IRuntimeVersionExpression runtimeversionexp)
		{
			(_expCalledForAll as IExprementVisitorNoTraversion351400)?.visit(runtimeversionexp);
		}

		public void visit(_ITryCatchStatement trycatchstatement)
		{
			trycatchstatement.DefaultTraverse(this);
			if (_expCalledForAll is IExprementVisitor2 exprementVisitor)
			{
				exprementVisitor.visit(trycatchstatement);
			}
		}

		public void visit(_IPartialAccessExpression partialAccessExpression)
		{
			_expCalledForAll.visit(partialAccessExpression);
			partialAccessExpression._Left.Accept(this);
		}

		public void visit(_IProjectDefinedExpression projectDefinedExpression)
		{
			(_expCalledForAll as IExprementVisitorNoTraversion352000)?.visit(projectDefinedExpression);
			projectDefinedExpression.DefineReference?.Accept(this);
		}

		private void VisitInputs(_ICallExpression call)
		{
			if (call.InputAssigns != null)
			{
				VisitInputAssignments(call);
			}
			else
			{
				VisitInputExpressions(call);
			}
		}

		private void VisitInputExpressions(_ICallExpression call)
		{
			foreach (_IExpression paramExpression in call.ParamExpressions)
			{
				paramExpression?.Accept(this);
			}
		}

		private void VisitInputAssignments(_ICallExpression call)
		{
			IAssignmentExpression[] inputAssigns = call.InputAssigns;
			for (int i = 0; i < inputAssigns.Length; i++)
			{
				if (inputAssigns[i] is _IAssignmentExpression iAssignmentExpression)
				{
					if (iAssignmentExpression.LValue is INullExpression)
					{
						iAssignmentExpression._RValue.Accept(this);
					}
					else
					{
						iAssignmentExpression.Accept(this);
					}
				}
			}
		}
	}
}
