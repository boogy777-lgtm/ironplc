using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace CODESYS.Parser35220.Tools
{
	// Token: 0x0200000E RID: 14
	[ExcludeFromCodeCoverage]
	public sealed class StandardTraverser : IExprementVisitor352000, IExprementVisitor351900, IExprementVisitor351800, IExprementVisitor351500, IExprementVisitor351400, IExprementVisitor351300, IExprementVisitor3590, IExprementVisitor
	{
		// Token: 0x060000AE RID: 174 RVA: 0x00003067 File Offset: 0x00001267
		public StandardTraverser(IExprementVisitor352000 expCalledForAll)
		{
			this._expCalledForAll = expCalledForAll;
		}

		// Token: 0x060000AF RID: 175 RVA: 0x00003076 File Offset: 0x00001276
		public void visit(_IAddressExpression address)
		{
			this._expCalledForAll.visit(address);
		}

		// Token: 0x060000B0 RID: 176 RVA: 0x00003084 File Offset: 0x00001284
		public void visit(_IVariableExpression variable)
		{
			this._expCalledForAll.visit(variable);
		}

		// Token: 0x060000B1 RID: 177 RVA: 0x00003092 File Offset: 0x00001292
		public void visit(_ICompiledPOU cpou)
		{
			if (cpou.GetFlag(1024))
			{
				cpou.GetParseTree().Accept(this);
			}
			this._expCalledForAll.visit(cpou);
		}

		// Token: 0x060000B2 RID: 178 RVA: 0x000030B9 File Offset: 0x000012B9
		public void visit(_IWhileStatement whilst)
		{
			whilst._Condition.Accept(this);
			whilst._Controlled.Accept(this);
			this._expCalledForAll.visit(whilst);
		}

		// Token: 0x060000B3 RID: 179 RVA: 0x000030DF File Offset: 0x000012DF
		public void visit(_IRepeatStatement repeat)
		{
			repeat._Condition.Accept(this);
			repeat._Controlled.Accept(this);
			this._expCalledForAll.visit(repeat);
		}

		// Token: 0x060000B4 RID: 180 RVA: 0x00003108 File Offset: 0x00001308
		public void visit(_IForStatement forloop)
		{
			forloop._CounterStart.Accept(this);
			forloop._UpperBound.Accept(this);
			if (forloop.By != null)
			{
				forloop._By.Accept(this);
			}
			forloop._Controlled.Accept(this);
			this._expCalledForAll.visit(forloop);
		}

		// Token: 0x060000B5 RID: 181 RVA: 0x0000315C File Offset: 0x0000135C
		public void visit(_ISequenceStatement seq)
		{
			foreach (_IStatement istatement in seq._StatementList)
			{
				istatement.Accept(this);
			}
			this._expCalledForAll.visit(seq);
		}

		// Token: 0x060000B6 RID: 182 RVA: 0x000031B4 File Offset: 0x000013B4
		public void visit(_IIfStatement ifst)
		{
			ifst._Condition.Accept(this);
			ifst._IfThen.Accept(this);
			foreach (_IElseIf ielseIf in ifst._ElseIf)
			{
				ielseIf._Condition.Accept(this);
				ielseIf._Controlled.Accept(this);
			}
			_IStatement ifElse = ifst._IfElse;
			if (ifElse != null)
			{
				ifElse.Accept(this);
			}
			this._expCalledForAll.visit(ifst);
		}

		// Token: 0x060000B7 RID: 183 RVA: 0x00003248 File Offset: 0x00001448
		public void visit(_IExpressionStatement expstat)
		{
			expstat._Expr.Accept(this);
			this._expCalledForAll.visit(expstat);
		}

		// Token: 0x060000B8 RID: 184 RVA: 0x00003262 File Offset: 0x00001462
		public void visit(_IAssignmentExpression assign)
		{
			assign._LValue.Accept(this);
			assign._RValue.Accept(this);
			this._expCalledForAll.visit(assign);
		}

		// Token: 0x060000B9 RID: 185 RVA: 0x00003288 File Offset: 0x00001488
		public void visit(_ICallExpression call)
		{
			call._Callee.Accept(this);
			if (call._Condition != null)
			{
				call._Condition.Accept(this);
			}
			this.VisitInputs(call);
			foreach (_IExpression iexpression in call.OutputExpressions)
			{
				if (iexpression != null)
				{
					iexpression.Accept(this);
				}
			}
			foreach (_IExpression iexpression2 in call.Inputs)
			{
				if (iexpression2 != null)
				{
					iexpression2.Accept(this);
				}
			}
			foreach (_IExpression iexpression3 in call.Outputs)
			{
				if (iexpression3 != null)
				{
					iexpression3.Accept(this);
				}
			}
			this._expCalledForAll.visit(call);
		}

		// Token: 0x060000BA RID: 186 RVA: 0x0000338C File Offset: 0x0000158C
		public void visit(_IOperatorExpression op)
		{
			foreach (_IExpression iexpression in op._OperandsList)
			{
				iexpression.Accept(this);
			}
			this._expCalledForAll.visit(op);
		}

		// Token: 0x060000BB RID: 187 RVA: 0x000033E4 File Offset: 0x000015E4
		public void visit(_ICastExpression castexp)
		{
			castexp.BaseExpression.Accept(this);
			this._expCalledForAll.visit(castexp);
		}

		// Token: 0x060000BC RID: 188 RVA: 0x00003400 File Offset: 0x00001600
		public void visit(_INewExpression typeref)
		{
			typeref._Count.Accept(this);
			if (typeref._FBInitParams != null)
			{
				foreach (_IAssignmentExpression iassignmentExpression in typeref._FBInitParams.OfType<_IAssignmentExpression>())
				{
					iassignmentExpression.Accept(this);
				}
			}
			this._expCalledForAll.visit(typeref);
		}

		// Token: 0x060000BD RID: 189 RVA: 0x00003470 File Offset: 0x00001670
		public void visit(_ITypeExpression typeexp)
		{
			this._expCalledForAll.visit(typeexp);
		}

		// Token: 0x060000BE RID: 190 RVA: 0x0000347E File Offset: 0x0000167E
		public void visit(_IConversionExpression conv)
		{
			conv._Exp.Accept(this);
			this._expCalledForAll.visit(conv);
		}

		// Token: 0x060000BF RID: 191 RVA: 0x00003498 File Offset: 0x00001698
		public void visit(_IIndexAccessExpression indexaccess)
		{
			for (int i = 0; i < indexaccess.NumAccesses; i++)
			{
				indexaccess.GetAccess(i).Accept(this);
			}
			indexaccess._Var.Accept(this);
			this._expCalledForAll.visit(indexaccess);
		}

		// Token: 0x060000C0 RID: 192 RVA: 0x000034DB File Offset: 0x000016DB
		public void visit(_ILiteralExpression literal)
		{
			this._expCalledForAll.visit(literal);
		}

		// Token: 0x060000C1 RID: 193 RVA: 0x000034E9 File Offset: 0x000016E9
		public void visit(_ICompoAccessExpression compo)
		{
			compo._Right.Accept(this);
			compo._Left.Accept(this);
			this._expCalledForAll.visit(compo);
		}

		// Token: 0x060000C2 RID: 194 RVA: 0x0000350F File Offset: 0x0000170F
		public void visit(_IDeRefAccessExpression deref)
		{
			deref._Base.Accept(this);
			this._expCalledForAll.visit(deref);
		}

		// Token: 0x060000C3 RID: 195 RVA: 0x00003529 File Offset: 0x00001729
		public void visit(_ICopyScopeExpression copyexp)
		{
			copyexp._Base.Accept(this);
			this._expCalledForAll.visit(copyexp);
		}

		// Token: 0x060000C4 RID: 196 RVA: 0x00003543 File Offset: 0x00001743
		public void visit(_IGlobalScopeExpression globexp)
		{
			globexp._Base.Accept(this);
			this._expCalledForAll.visit(globexp);
		}

		// Token: 0x060000C5 RID: 197 RVA: 0x0000355D File Offset: 0x0000175D
		public void visit(_ISystemScopeExpression systemscope)
		{
			systemscope._Base.Accept(this);
			this._expCalledForAll.visit(systemscope);
		}

		// Token: 0x060000C6 RID: 198 RVA: 0x00003577 File Offset: 0x00001777
		public void visit(_IPoolScopeExpression poolscope)
		{
			poolscope._Base.Accept(this);
			this._expCalledForAll.visit(poolscope);
		}

		// Token: 0x060000C7 RID: 199 RVA: 0x00003591 File Offset: 0x00001791
		public void visit(_INamespaceAccessExpression namespaceaccess)
		{
			namespaceaccess._Namespace.Accept(this);
			_IExpression access = namespaceaccess._Access;
			if (access != null)
			{
				access.Accept(this);
			}
			IExprementVisitorNoTraversion351500 exprementVisitorNoTraversion = this._expCalledForAll as IExprementVisitorNoTraversion351500;
			if (exprementVisitorNoTraversion == null)
			{
				return;
			}
			exprementVisitorNoTraversion.visit(namespaceaccess);
		}

		// Token: 0x060000C8 RID: 200 RVA: 0x000035C7 File Offset: 0x000017C7
		public void visit(_ICurrentTaskExpression currentTask)
		{
			currentTask._Base.Accept(this);
			this._expCalledForAll.visit(currentTask);
		}

		// Token: 0x060000C9 RID: 201 RVA: 0x000035E1 File Offset: 0x000017E1
		public void visit(_ICaseRangeExpression caserange)
		{
			caserange._Low.Accept(this);
			caserange._High.Accept(this);
			this._expCalledForAll.visit(caserange);
		}

		// Token: 0x060000CA RID: 202 RVA: 0x00003608 File Offset: 0x00001808
		public void visit(_ICaseLabelStatement caselabel)
		{
			foreach (_IExpression iexpression in caselabel._cases)
			{
				iexpression.Accept(this);
			}
			this._expCalledForAll.visit(caselabel);
		}

		// Token: 0x060000CB RID: 203 RVA: 0x00003660 File Offset: 0x00001860
		public void visit(_ICaseStatement casest)
		{
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
			this._expCalledForAll.visit(casest);
		}

		// Token: 0x060000CC RID: 204 RVA: 0x000036E8 File Offset: 0x000018E8
		public void visit(_IExitStatement exit)
		{
			this._expCalledForAll.visit(exit);
		}

		// Token: 0x060000CD RID: 205 RVA: 0x000036F6 File Offset: 0x000018F6
		public void visit(_IContinueStatement cont)
		{
			this._expCalledForAll.visit(cont);
		}

		// Token: 0x060000CE RID: 206 RVA: 0x00003704 File Offset: 0x00001904
		public void visit(_IThisExpression thisexp)
		{
			this._expCalledForAll.visit(thisexp);
		}

		// Token: 0x060000CF RID: 207 RVA: 0x00003712 File Offset: 0x00001912
		public void visit(_IBaseExpression baseexp)
		{
			this._expCalledForAll.visit(baseexp);
		}

		// Token: 0x060000D0 RID: 208 RVA: 0x00003720 File Offset: 0x00001920
		public void visit(_IEmptyStatement empty)
		{
			this._expCalledForAll.visit(empty);
		}

		// Token: 0x060000D1 RID: 209 RVA: 0x0000372E File Offset: 0x0000192E
		public void visit(_IReturnStatement returnst)
		{
			if (returnst._Condition != null)
			{
				returnst._Condition.Accept(this);
			}
			this._expCalledForAll.visit(returnst);
		}

		// Token: 0x060000D2 RID: 210 RVA: 0x00003750 File Offset: 0x00001950
		public void visit(_IJumpStatement gotost)
		{
			if (gotost._Condition != null)
			{
				gotost._Condition.Accept(this);
			}
			this._expCalledForAll.visit(gotost);
		}

		// Token: 0x060000D3 RID: 211 RVA: 0x00003772 File Offset: 0x00001972
		public void visit(_ILabelStatement label)
		{
			this._expCalledForAll.visit(label);
		}

		// Token: 0x060000D4 RID: 212 RVA: 0x00003780 File Offset: 0x00001980
		public void visit(_ICommentStatement comment)
		{
			this._expCalledForAll.visit(comment);
		}

		// Token: 0x060000D5 RID: 213 RVA: 0x0000378E File Offset: 0x0000198E
		public void visit(_IPragmaStatement pragma)
		{
			this._expCalledForAll.visit(pragma);
		}

		// Token: 0x060000D6 RID: 214 RVA: 0x0000379C File Offset: 0x0000199C
		public void visit(_IErrorExpression errorexp)
		{
			this._expCalledForAll.visit(errorexp);
		}

		// Token: 0x060000D7 RID: 215 RVA: 0x000037AA File Offset: 0x000019AA
		public void visit(_IErrorStatement errorst)
		{
			this._expCalledForAll.visit(errorst);
		}

		// Token: 0x060000D8 RID: 216 RVA: 0x000037B8 File Offset: 0x000019B8
		public void visit(_INullExpression errorexp)
		{
			this._expCalledForAll.visit(errorexp);
		}

		// Token: 0x060000D9 RID: 217 RVA: 0x000037C6 File Offset: 0x000019C6
		public void visit(_INullStatement errorst)
		{
			this._expCalledForAll.visit(errorst);
		}

		// Token: 0x060000DA RID: 218 RVA: 0x000037D4 File Offset: 0x000019D4
		public void visit(_IQualifiedNameExpression qne)
		{
			this._expCalledForAll.visit(qne);
		}

		// Token: 0x060000DB RID: 219 RVA: 0x000037E4 File Offset: 0x000019E4
		public void visit(_IVariableDeclarationStatement vds)
		{
			foreach (_IExpression iexpression in vds.NameList)
			{
				iexpression.Accept(this);
			}
			vds.Initial.Accept(this);
			if (vds.InputAssigns != null)
			{
				foreach (_IAssignmentExpression iassignmentExpression in vds.InputAssigns)
				{
					iassignmentExpression.Accept(this);
				}
			}
			this._expCalledForAll.visit(vds);
		}

		// Token: 0x060000DC RID: 220 RVA: 0x0000388C File Offset: 0x00001A8C
		public void visit(_IVariableDeclarationListStatement vdls)
		{
			vdls.VariableDeclaration.Accept(this);
			this._expCalledForAll.visit(vdls);
		}

		// Token: 0x060000DD RID: 221 RVA: 0x000038A8 File Offset: 0x00001AA8
		public void visit(_IPOUDeclarationStatement pds)
		{
			pds.Declarations.Accept(this);
			if (pds.Implements != null)
			{
				foreach (_IExpression iexpression in pds.Implements)
				{
					iexpression.Accept(this);
				}
			}
			if (pds.Extends != null)
			{
				foreach (_IExpression iexpression2 in pds.Extends)
				{
					iexpression2.Accept(this);
				}
			}
			this._expCalledForAll.visit(pds);
		}

		// Token: 0x060000DE RID: 222 RVA: 0x00003958 File Offset: 0x00001B58
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
			this._expCalledForAll.visit(tds);
		}

		// Token: 0x060000DF RID: 223 RVA: 0x000039A5 File Offset: 0x00001BA5
		public void visit(_IEnumDeclarationStatement eds)
		{
			if (eds._Value != null)
			{
				eds._Value.Accept(this);
			}
			this._expCalledForAll.visit(eds);
		}

		// Token: 0x060000E0 RID: 224 RVA: 0x000039C8 File Offset: 0x00001BC8
		public void visit(_IEnumDeclarationListStatement eds)
		{
			foreach (_IEnumDeclarationStatement ienumDeclarationStatement in eds.Enums)
			{
				ienumDeclarationStatement.Accept(this);
			}
			this._expCalledForAll.visit(eds);
		}

		// Token: 0x060000E1 RID: 225 RVA: 0x00003A20 File Offset: 0x00001C20
		public void visit(_IMultipleIndexInitialization errorst)
		{
			errorst._Value.Accept(this);
			errorst._Number.Accept(this);
			this._expCalledForAll.visit(errorst);
		}

		// Token: 0x060000E2 RID: 226 RVA: 0x00003A48 File Offset: 0x00001C48
		public void visit(_IArrayInitialization errorexp)
		{
			foreach (_IExpression iexpression in errorexp._InitValues)
			{
				iexpression.Accept(this);
			}
			this._expCalledForAll.visit(errorexp);
		}

		// Token: 0x060000E3 RID: 227 RVA: 0x00003AA0 File Offset: 0x00001CA0
		public void visit(_IStructureInitialization errorst)
		{
			foreach (_IAssignmentExpression iassignmentExpression in errorst._CompoInits)
			{
				iassignmentExpression.Accept(this);
			}
			this._expCalledForAll.visit(errorst);
		}

		// Token: 0x060000E4 RID: 228 RVA: 0x00003AF8 File Offset: 0x00001CF8
		public void visit(_IDefineReference defref)
		{
			this._expCalledForAll.visit(defref);
		}

		// Token: 0x060000E5 RID: 229 RVA: 0x00003B06 File Offset: 0x00001D06
		public void visit(_IVariableReference varref)
		{
			if (varref.InstancePath != null)
			{
				varref.InstancePath.Accept(this);
			}
			this._expCalledForAll.visit(varref);
		}

		// Token: 0x060000E6 RID: 230 RVA: 0x00003B28 File Offset: 0x00001D28
		public void visit(_ITypeReference typeref)
		{
			if (typeref.InstancePath != null)
			{
				typeref.InstancePath.Accept(this);
			}
			this._expCalledForAll.visit(typeref);
		}

		// Token: 0x060000E7 RID: 231 RVA: 0x00003B4A File Offset: 0x00001D4A
		public void visit(_IPouReference pouref)
		{
			if (pouref.InstancePath != null)
			{
				pouref.InstancePath.Accept(this);
			}
			this._expCalledForAll.visit(pouref);
		}

		// Token: 0x060000E8 RID: 232 RVA: 0x00003B6C File Offset: 0x00001D6C
		public void visit(_ITaskReference taskref)
		{
			this._expCalledForAll.visit(taskref);
		}

		// Token: 0x060000E9 RID: 233 RVA: 0x00003B7A File Offset: 0x00001D7A
		public void visit(_IResourceReference resref)
		{
			this._expCalledForAll.visit(resref);
		}

		// Token: 0x060000EA RID: 234 RVA: 0x00003B88 File Offset: 0x00001D88
		public void visit(_IDefinedExpression defexp)
		{
			defexp.ItemReference.Accept(this);
			this._expCalledForAll.visit(defexp);
		}

		// Token: 0x060000EB RID: 235 RVA: 0x00003BA4 File Offset: 0x00001DA4
		public void visit(_IPragmaOperatorExpression popexp)
		{
			foreach (_IExpression iexpression in popexp.Operands)
			{
				iexpression.Accept(this);
			}
			this._expCalledForAll.visit(popexp);
		}

		// Token: 0x060000EC RID: 236 RVA: 0x00003BFC File Offset: 0x00001DFC
		public void visit(_IPragmaIfStatement pifst)
		{
			pifst.Condition.Accept(this);
			pifst.IfThen.Accept(this);
			foreach (_IPragmaElseIf ipragmaElseIf in pifst.ElseIf)
			{
				ipragmaElseIf.Condition.Accept(this);
				ipragmaElseIf.Controlled.Accept(this);
			}
			if (pifst.IfElse != null)
			{
				pifst.IfElse.Accept(this);
			}
			this._expCalledForAll.visit(pifst);
		}

		// Token: 0x060000ED RID: 237 RVA: 0x00003C90 File Offset: 0x00001E90
		public void visit(_IBreakPointStatement bpstate)
		{
			this._expCalledForAll.visit(bpstate);
		}

		// Token: 0x060000EE RID: 238 RVA: 0x00003C9E File Offset: 0x00001E9E
		public void visit(_IDefineStatement defstate)
		{
			this._expCalledForAll.visit(defstate);
		}

		// Token: 0x060000EF RID: 239 RVA: 0x00003CAC File Offset: 0x00001EAC
		public void visit(_IXRefExpression xref)
		{
			xref.XRef.Accept(this);
			xref.XRefFrom.Accept(this);
			this._expCalledForAll.visit(xref);
		}

		// Token: 0x060000F0 RID: 240 RVA: 0x00003CD2 File Offset: 0x00001ED2
		public void visit(_IHasTypeExpression hastype)
		{
			hastype.Variable.Accept(this);
			this._expCalledForAll.visit(hastype);
		}

		// Token: 0x060000F1 RID: 241 RVA: 0x00003CEC File Offset: 0x00001EEC
		public void visit(_IIsEnumTypeExpression isenumtype)
		{
			this._expCalledForAll.visit(isenumtype);
		}

		// Token: 0x060000F2 RID: 242 RVA: 0x00003CFA File Offset: 0x00001EFA
		public void visit(_IHasAttributeExpression hasattribute)
		{
			hasattribute.ItemReference.Accept(this);
			this._expCalledForAll.visit(hasattribute);
		}

		// Token: 0x060000F3 RID: 243 RVA: 0x00003D14 File Offset: 0x00001F14
		public void visit(_IHasValueExpression hasvalue)
		{
			this._expCalledForAll.visit(hasvalue);
		}

		// Token: 0x060000F4 RID: 244 RVA: 0x00003D22 File Offset: 0x00001F22
		public void visit(_IHasConstantValueExpression hasvalue)
		{
			hasvalue._Constant.Accept(this);
			this._expCalledForAll.visit(hasvalue);
		}

		// Token: 0x060000F5 RID: 245 RVA: 0x00003D3C File Offset: 0x00001F3C
		public void visit(_IHasConstantTypeExpression hasConstantTypeExpression)
		{
			hasConstantTypeExpression._Constant.Accept(this);
			IExprementVisitorNoTraversion351800 exprementVisitorNoTraversion = this._expCalledForAll as IExprementVisitorNoTraversion351800;
			if (exprementVisitorNoTraversion == null)
			{
				return;
			}
			exprementVisitorNoTraversion.visit(hasConstantTypeExpression);
		}

		// Token: 0x060000F6 RID: 246 RVA: 0x00003D60 File Offset: 0x00001F60
		public void visit(_IPragmaAssertion assertion)
		{
			assertion.Condition.Accept(this);
			this._expCalledForAll.visit(assertion);
		}

		// Token: 0x060000F7 RID: 247 RVA: 0x00003D7A File Offset: 0x00001F7A
		public void visit(_ICompilerVersionExpression compiversionexp)
		{
			this._expCalledForAll.visit(compiversionexp);
		}

		// Token: 0x060000F8 RID: 248 RVA: 0x00003D88 File Offset: 0x00001F88
		public void visit(_IRuntimeVersionExpression runtimeversionexp)
		{
			IExprementVisitorNoTraversion351400 exprementVisitorNoTraversion = this._expCalledForAll as IExprementVisitorNoTraversion351400;
			if (exprementVisitorNoTraversion == null)
			{
				return;
			}
			exprementVisitorNoTraversion.visit(runtimeversionexp);
		}

		// Token: 0x060000F9 RID: 249 RVA: 0x00003DA0 File Offset: 0x00001FA0
		public void visit(_ITryCatchStatement trycatchstatement)
		{
			trycatchstatement.DefaultTraverse(this);
			IExprementVisitor2 exprementVisitor = this._expCalledForAll as IExprementVisitor2;
			if (exprementVisitor != null)
			{
				exprementVisitor.visit(trycatchstatement);
			}
		}

		// Token: 0x060000FA RID: 250 RVA: 0x00003DCA File Offset: 0x00001FCA
		public void visit(_IPartialAccessExpression partialAccessExpression)
		{
			this._expCalledForAll.visit(partialAccessExpression);
			partialAccessExpression._Left.Accept(this);
		}

		// Token: 0x060000FB RID: 251 RVA: 0x00003DE4 File Offset: 0x00001FE4
		public void visit(_IProjectDefinedExpression projectDefinedExpression)
		{
			IExprementVisitorNoTraversion352000 exprementVisitorNoTraversion = this._expCalledForAll as IExprementVisitorNoTraversion352000;
			if (exprementVisitorNoTraversion != null)
			{
				exprementVisitorNoTraversion.visit(projectDefinedExpression);
			}
			_IDefineReference defineReference = projectDefinedExpression.DefineReference;
			if (defineReference == null)
			{
				return;
			}
			defineReference.Accept(this);
		}

		// Token: 0x060000FC RID: 252 RVA: 0x00003E0E File Offset: 0x0000200E
		private void VisitInputs(_ICallExpression call)
		{
			if (call.InputAssigns != null)
			{
				this.VisitInputAssignments(call);
				return;
			}
			this.VisitInputExpressions(call);
		}

		// Token: 0x060000FD RID: 253 RVA: 0x00003E28 File Offset: 0x00002028
		private void VisitInputExpressions(_ICallExpression call)
		{
			foreach (_IExpression iexpression in call.ParamExpressions)
			{
				if (iexpression != null)
				{
					iexpression.Accept(this);
				}
			}
		}

		// Token: 0x060000FE RID: 254 RVA: 0x00003E78 File Offset: 0x00002078
		private void VisitInputAssignments(_ICallExpression call)
		{
			IAssignmentExpression[] inputAssigns = call.InputAssigns;
			for (int i = 0; i < inputAssigns.Length; i++)
			{
				_IAssignmentExpression iassignmentExpression = inputAssigns[i] as _IAssignmentExpression;
				if (iassignmentExpression != null)
				{
					if (iassignmentExpression.LValue is INullExpression)
					{
						iassignmentExpression._RValue.Accept(this);
					}
					else
					{
						iassignmentExpression.Accept(this);
					}
				}
			}
		}

		// Token: 0x04000009 RID: 9
		private readonly IExprementVisitor352000 _expCalledForAll;
	}
}
