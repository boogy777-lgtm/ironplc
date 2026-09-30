using System;
using System.Collections.Generic;
using System.Linq;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler35220.PreCompile
{
	// Token: 0x02000157 RID: 343
	public abstract class ChildVisitor : IExprementVisitor352000, IExprementVisitor351900, IExprementVisitor351800, IExprementVisitor351500, IExprementVisitor351400, IExprementVisitor351300, IExprementVisitor3590, IExprementVisitor
	{
		// Token: 0x060017C1 RID: 6081
		protected abstract void VisitChild(_IExprement exprement);

		// Token: 0x060017C2 RID: 6082 RVA: 0x00049D78 File Offset: 0x00047F78
		public static void GetChildren(_IExprement exprement, ICollection<_IExprement> targetChildren)
		{
			exprement.Accept(new ChildVisitor.\u0001(targetChildren));
		}

		// Token: 0x060017C3 RID: 6083 RVA: 0x00049D88 File Offset: 0x00047F88
		public static IReadOnlyList<_IExprement> GetChildren(_IExprement exprement)
		{
			List<_IExprement> list = new List<_IExprement>();
			ChildVisitor.GetChildren(exprement, list);
			return list;
		}

		// Token: 0x060017C4 RID: 6084 RVA: 0x00049DA4 File Offset: 0x00047FA4
		public void visit(_IWhileStatement whilst)
		{
			this.VisitChild(whilst._Condition);
			this.VisitChild(whilst._Controlled);
		}

		// Token: 0x060017C5 RID: 6085 RVA: 0x00049DC0 File Offset: 0x00047FC0
		public void visit(_IRepeatStatement repeat)
		{
			this.VisitChild(repeat._Condition);
			this.VisitChild(repeat._Controlled);
		}

		// Token: 0x060017C6 RID: 6086 RVA: 0x00049DDC File Offset: 0x00047FDC
		public void visit(_IForStatement forloop)
		{
			this.VisitChild(forloop._CounterStart);
			this.VisitChild(forloop._UpperBound);
			if (forloop.By != null)
			{
				this.VisitChild(forloop._By);
			}
			this.VisitChild(forloop._Controlled);
		}

		// Token: 0x060017C7 RID: 6087 RVA: 0x00049E18 File Offset: 0x00048018
		public void visit(_ISequenceStatement seq)
		{
			foreach (_IStatement exprement in seq._StatementList)
			{
				this.VisitChild(exprement);
			}
		}

		// Token: 0x060017C8 RID: 6088 RVA: 0x00049E68 File Offset: 0x00048068
		public void visit(_IIfStatement ifst)
		{
			this.VisitChild(ifst._Condition);
			this.VisitChild(ifst._IfThen);
			foreach (_IElseIf ielseIf in ifst._ElseIf)
			{
				this.VisitChild(ielseIf._Condition);
				this.VisitChild(ielseIf._Controlled);
			}
			if (ifst._IfElse != null)
			{
				this.VisitChild(ifst._IfElse);
			}
		}

		// Token: 0x060017C9 RID: 6089 RVA: 0x00049EF4 File Offset: 0x000480F4
		public void visit(_IExpressionStatement expstat)
		{
			this.VisitChild(expstat._Expr);
		}

		// Token: 0x060017CA RID: 6090 RVA: 0x00049F04 File Offset: 0x00048104
		public void visit(_IAssignmentExpression assign)
		{
			this.VisitChild(assign._LValue);
			this.VisitChild(assign._RValue);
		}

		// Token: 0x060017CB RID: 6091 RVA: 0x00049F20 File Offset: 0x00048120
		public void visit(_ICallExpression call)
		{
			this.VisitChild(call._Callee);
			if (call._Condition != null)
			{
				this.VisitChild(call._Condition);
			}
			this.\u0001(call);
			foreach (_IExpression iexpression in call.OutputExpressions)
			{
				if (iexpression != null)
				{
					this.VisitChild(iexpression);
				}
			}
			foreach (_IExpression iexpression2 in call.Inputs)
			{
				if (iexpression2 != null)
				{
					this.VisitChild(iexpression2);
				}
			}
			foreach (_IExpression iexpression3 in call.Outputs)
			{
				if (iexpression3 != null)
				{
					this.VisitChild(iexpression3);
				}
			}
		}

		// Token: 0x060017CC RID: 6092 RVA: 0x0004A018 File Offset: 0x00048218
		public void visit(_IOperatorExpression op)
		{
			foreach (_IExpression exprement in op._OperandsList)
			{
				this.VisitChild(exprement);
			}
		}

		// Token: 0x060017CD RID: 6093 RVA: 0x0004A068 File Offset: 0x00048268
		public void visit(_ICastExpression castexp)
		{
			this.VisitChild(castexp.BaseExpression);
		}

		// Token: 0x060017CE RID: 6094 RVA: 0x0004A078 File Offset: 0x00048278
		public void visit(_INewExpression typeref)
		{
			this.VisitChild(typeref._Count);
			if (typeref._FBInitParams != null)
			{
				foreach (_IAssignmentExpression exprement in typeref._FBInitParams.OfType<_IAssignmentExpression>())
				{
					this.VisitChild(exprement);
				}
			}
		}

		// Token: 0x060017CF RID: 6095 RVA: 0x0004A0E0 File Offset: 0x000482E0
		public void visit(_IConversionExpression conv)
		{
			this.VisitChild(conv._Exp);
		}

		// Token: 0x060017D0 RID: 6096 RVA: 0x0004A0F0 File Offset: 0x000482F0
		public void visit(_IIndexAccessExpression indexaccess)
		{
			for (int i = 0; i < indexaccess.NumAccesses; i++)
			{
				this.VisitChild(indexaccess.GetAccess(i));
			}
			this.VisitChild(indexaccess._Var);
		}

		// Token: 0x060017D1 RID: 6097 RVA: 0x0004A128 File Offset: 0x00048328
		public void visit(_ICompoAccessExpression compo)
		{
			this.VisitChild(compo._Right);
			this.VisitChild(compo._Left);
		}

		// Token: 0x060017D2 RID: 6098 RVA: 0x0004A144 File Offset: 0x00048344
		public void visit(_IDeRefAccessExpression deref)
		{
			this.VisitChild(deref._Base);
		}

		// Token: 0x060017D3 RID: 6099 RVA: 0x0004A154 File Offset: 0x00048354
		public void visit(_ICopyScopeExpression copyexp)
		{
			this.VisitChild(copyexp._Base);
		}

		// Token: 0x060017D4 RID: 6100 RVA: 0x0004A164 File Offset: 0x00048364
		public void visit(_IGlobalScopeExpression globexp)
		{
			this.VisitChild(globexp._Base);
		}

		// Token: 0x060017D5 RID: 6101 RVA: 0x0004A174 File Offset: 0x00048374
		public void visit(_ISystemScopeExpression systemscope)
		{
			this.VisitChild(systemscope._Base);
		}

		// Token: 0x060017D6 RID: 6102 RVA: 0x0004A184 File Offset: 0x00048384
		public void visit(_IPoolScopeExpression poolscope)
		{
			this.VisitChild(poolscope._Base);
		}

		// Token: 0x060017D7 RID: 6103 RVA: 0x0004A194 File Offset: 0x00048394
		public void visit(_INamespaceAccessExpression namespaceaccess)
		{
			this.VisitChild(namespaceaccess._Namespace);
			if (namespaceaccess._Access != null)
			{
				this.VisitChild(namespaceaccess._Access);
			}
		}

		// Token: 0x060017D8 RID: 6104 RVA: 0x0004A1B8 File Offset: 0x000483B8
		public void visit(_ICurrentTaskExpression currentTask)
		{
			this.VisitChild(currentTask._Base);
		}

		// Token: 0x060017D9 RID: 6105 RVA: 0x0004A1C8 File Offset: 0x000483C8
		public void visit(_ICaseRangeExpression caserange)
		{
			this.VisitChild(caserange._Low);
			this.VisitChild(caserange._High);
		}

		// Token: 0x060017DA RID: 6106 RVA: 0x0004A1E4 File Offset: 0x000483E4
		public void visit(_ICaseLabelStatement caselabel)
		{
			foreach (_IExpression exprement in caselabel._cases)
			{
				this.VisitChild(exprement);
			}
		}

		// Token: 0x060017DB RID: 6107 RVA: 0x0004A234 File Offset: 0x00048434
		public void visit(_ICaseStatement casest)
		{
			this.VisitChild(casest._Switch);
			foreach (_ICase icase in casest._Cases)
			{
				this.VisitChild(icase._Label);
				this.VisitChild(icase._Controlled);
			}
			if (casest._Else != null)
			{
				this.VisitChild(casest._Else);
			}
		}

		// Token: 0x060017DC RID: 6108 RVA: 0x0004A2B4 File Offset: 0x000484B4
		public void visit(_IReturnStatement returnst)
		{
			if (returnst._Condition != null)
			{
				this.VisitChild(returnst._Condition);
			}
		}

		// Token: 0x060017DD RID: 6109 RVA: 0x0004A2CC File Offset: 0x000484CC
		public void visit(_IJumpStatement gotost)
		{
			if (gotost._Condition != null)
			{
				this.VisitChild(gotost._Condition);
			}
		}

		// Token: 0x060017DE RID: 6110 RVA: 0x0004A2E4 File Offset: 0x000484E4
		public void visit(_IVariableDeclarationStatement vds)
		{
			foreach (_IExpression exprement in vds.NameList)
			{
				this.VisitChild(exprement);
			}
			this.VisitChild(vds.Initial);
			if (vds.InputAssigns != null)
			{
				foreach (_IAssignmentExpression exprement2 in vds.InputAssigns)
				{
					this.VisitChild(exprement2);
				}
			}
		}

		// Token: 0x060017DF RID: 6111 RVA: 0x0004A384 File Offset: 0x00048584
		public void visit(_IVariableDeclarationListStatement vdls)
		{
			this.VisitChild(vdls.VariableDeclaration);
		}

		// Token: 0x060017E0 RID: 6112 RVA: 0x0004A394 File Offset: 0x00048594
		public void visit(_IPOUDeclarationStatement pds)
		{
			this.VisitChild(pds.Declarations);
			if (pds.Implements != null)
			{
				foreach (_IExpression exprement in pds.Implements)
				{
					this.VisitChild(exprement);
				}
			}
			if (pds.Extends != null)
			{
				foreach (_IExpression exprement2 in pds.Extends)
				{
					this.VisitChild(exprement2);
				}
			}
		}

		// Token: 0x060017E1 RID: 6113 RVA: 0x0004A43C File Offset: 0x0004863C
		public void visit(_ITypeDeclarationStatement tds)
		{
			this.VisitChild(tds.Declarations);
			if (tds.Initial != null)
			{
				this.VisitChild(tds.Initial);
			}
			if (tds.Extends != null)
			{
				this.VisitChild(tds.Extends);
			}
		}

		// Token: 0x060017E2 RID: 6114 RVA: 0x0004A474 File Offset: 0x00048674
		public void visit(_IEnumDeclarationStatement eds)
		{
			if (eds._Value != null)
			{
				this.VisitChild(eds._Value);
			}
		}

		// Token: 0x060017E3 RID: 6115 RVA: 0x0004A48C File Offset: 0x0004868C
		public void visit(_IEnumDeclarationListStatement eds)
		{
			foreach (_IEnumDeclarationStatement exprement in eds.Enums)
			{
				this.VisitChild(exprement);
			}
		}

		// Token: 0x060017E4 RID: 6116 RVA: 0x0004A4DC File Offset: 0x000486DC
		public void visit(_IMultipleIndexInitialization errorst)
		{
			this.VisitChild(errorst._Value);
			this.VisitChild(errorst._Number);
		}

		// Token: 0x060017E5 RID: 6117 RVA: 0x0004A4F8 File Offset: 0x000486F8
		public void visit(_IArrayInitialization errorexp)
		{
			foreach (_IExpression exprement in errorexp._InitValues)
			{
				this.VisitChild(exprement);
			}
		}

		// Token: 0x060017E6 RID: 6118 RVA: 0x0004A548 File Offset: 0x00048748
		public void visit(_IStructureInitialization errorst)
		{
			foreach (_IAssignmentExpression exprement in errorst._CompoInits)
			{
				this.VisitChild(exprement);
			}
		}

		// Token: 0x060017E7 RID: 6119 RVA: 0x0004A598 File Offset: 0x00048798
		public void visit(_IVariableReference varref)
		{
			if (varref.InstancePath != null)
			{
				this.VisitChild(varref.InstancePath);
			}
		}

		// Token: 0x060017E8 RID: 6120 RVA: 0x0004A5B0 File Offset: 0x000487B0
		public void visit(_ITypeReference typeref)
		{
			if (typeref.InstancePath != null)
			{
				this.VisitChild(typeref.InstancePath);
			}
		}

		// Token: 0x060017E9 RID: 6121 RVA: 0x0004A5C8 File Offset: 0x000487C8
		public void visit(_IPouReference pouref)
		{
			if (pouref.InstancePath != null)
			{
				this.VisitChild(pouref.InstancePath);
			}
		}

		// Token: 0x060017EA RID: 6122 RVA: 0x0004A5E0 File Offset: 0x000487E0
		public void visit(_IDefinedExpression defexp)
		{
			this.VisitChild(defexp.ItemReference);
		}

		// Token: 0x060017EB RID: 6123 RVA: 0x0004A5F0 File Offset: 0x000487F0
		public void visit(_IPragmaOperatorExpression popexp)
		{
			foreach (_IExpression exprement in popexp.Operands)
			{
				this.VisitChild(exprement);
			}
		}

		// Token: 0x060017EC RID: 6124 RVA: 0x0004A640 File Offset: 0x00048840
		public void visit(_IPragmaIfStatement pifst)
		{
			this.VisitChild(pifst.Condition);
			this.VisitChild(pifst.IfThen);
			foreach (_IPragmaElseIf ipragmaElseIf in pifst.ElseIf)
			{
				this.VisitChild(ipragmaElseIf.Condition);
				this.VisitChild(ipragmaElseIf.Controlled);
			}
			if (pifst.IfElse != null)
			{
				this.VisitChild(pifst.IfElse);
			}
		}

		// Token: 0x060017ED RID: 6125 RVA: 0x0004A6CC File Offset: 0x000488CC
		public void visit(_IXRefExpression xref)
		{
			this.VisitChild(xref.XRef);
			this.VisitChild(xref.XRefFrom);
		}

		// Token: 0x060017EE RID: 6126 RVA: 0x0004A6E8 File Offset: 0x000488E8
		public void visit(_IHasTypeExpression hastype)
		{
			this.VisitChild(hastype.Variable);
		}

		// Token: 0x060017EF RID: 6127 RVA: 0x0004A6F8 File Offset: 0x000488F8
		public void visit(_IHasAttributeExpression hasattribute)
		{
			this.VisitChild(hasattribute.ItemReference);
		}

		// Token: 0x060017F0 RID: 6128 RVA: 0x0004A708 File Offset: 0x00048908
		public void visit(_IHasConstantValueExpression hasvalue)
		{
			this.VisitChild(hasvalue._Constant);
		}

		// Token: 0x060017F1 RID: 6129 RVA: 0x0004A718 File Offset: 0x00048918
		public void visit(_IHasConstantTypeExpression hasConstantTypeExpression)
		{
			this.VisitChild(hasConstantTypeExpression._Constant);
		}

		// Token: 0x060017F2 RID: 6130 RVA: 0x0004A728 File Offset: 0x00048928
		public void visit(_IPragmaAssertion assertion)
		{
			this.VisitChild(assertion.Condition);
		}

		// Token: 0x060017F3 RID: 6131 RVA: 0x0004A738 File Offset: 0x00048938
		public void visit(_ITryCatchStatement trycatchstatement)
		{
			trycatchstatement.DefaultTraverse(this);
		}

		// Token: 0x060017F4 RID: 6132 RVA: 0x0004A744 File Offset: 0x00048944
		public void visit(_IPartialAccessExpression partialAccessExpression)
		{
			this.VisitChild(partialAccessExpression._Left);
		}

		// Token: 0x060017F5 RID: 6133 RVA: 0x0004A754 File Offset: 0x00048954
		public void visit(_IProjectDefinedExpression projectDefinedExpression)
		{
			if (projectDefinedExpression.DefineReference != null)
			{
				this.VisitChild(projectDefinedExpression.DefineReference);
			}
		}

		// Token: 0x060017F6 RID: 6134 RVA: 0x0004A76C File Offset: 0x0004896C
		private void \u0001(_ICallExpression \u0002)
		{
			if (\u0002.InputAssigns != null)
			{
				this.\u0003(\u0002);
				return;
			}
			this.\u0002(\u0002);
		}

		// Token: 0x060017F7 RID: 6135 RVA: 0x0004A788 File Offset: 0x00048988
		private void \u0002(_ICallExpression \u0002)
		{
			foreach (_IExpression iexpression in \u0002.ParamExpressions)
			{
				if (iexpression != null)
				{
					this.VisitChild(iexpression);
				}
			}
		}

		// Token: 0x060017F8 RID: 6136 RVA: 0x0004A7D8 File Offset: 0x000489D8
		private void \u0003(_ICallExpression \u0002)
		{
			IAssignmentExpression[] inputAssigns = \u0002.InputAssigns;
			for (int i = 0; i < inputAssigns.Length; i++)
			{
				_IAssignmentExpression iassignmentExpression = inputAssigns[i] as _IAssignmentExpression;
				if (iassignmentExpression != null)
				{
					if (iassignmentExpression.LValue is INullExpression)
					{
						this.VisitChild(iassignmentExpression._RValue);
					}
					else
					{
						this.VisitChild(iassignmentExpression);
					}
				}
			}
		}

		// Token: 0x060017F9 RID: 6137 RVA: 0x0004A828 File Offset: 0x00048A28
		public void visit(_ITypeExpression typeexp)
		{
		}

		// Token: 0x060017FA RID: 6138 RVA: 0x0004A82C File Offset: 0x00048A2C
		public void visit(_IAddressExpression address)
		{
		}

		// Token: 0x060017FB RID: 6139 RVA: 0x0004A830 File Offset: 0x00048A30
		public void visit(_IVariableExpression variable)
		{
		}

		// Token: 0x060017FC RID: 6140 RVA: 0x0004A834 File Offset: 0x00048A34
		public void visit(_ICompiledPOU cpou)
		{
		}

		// Token: 0x060017FD RID: 6141 RVA: 0x0004A838 File Offset: 0x00048A38
		public void visit(_ILiteralExpression literal)
		{
		}

		// Token: 0x060017FE RID: 6142 RVA: 0x0004A83C File Offset: 0x00048A3C
		public void visit(_IExitStatement exit)
		{
		}

		// Token: 0x060017FF RID: 6143 RVA: 0x0004A840 File Offset: 0x00048A40
		public void visit(_IContinueStatement cont)
		{
		}

		// Token: 0x06001800 RID: 6144 RVA: 0x0004A844 File Offset: 0x00048A44
		public void visit(_IThisExpression thisexp)
		{
		}

		// Token: 0x06001801 RID: 6145 RVA: 0x0004A848 File Offset: 0x00048A48
		public void visit(_IBaseExpression baseexp)
		{
		}

		// Token: 0x06001802 RID: 6146 RVA: 0x0004A84C File Offset: 0x00048A4C
		public void visit(_IEmptyStatement empty)
		{
		}

		// Token: 0x06001803 RID: 6147 RVA: 0x0004A850 File Offset: 0x00048A50
		public void visit(_ILabelStatement label)
		{
		}

		// Token: 0x06001804 RID: 6148 RVA: 0x0004A854 File Offset: 0x00048A54
		public void visit(_ICommentStatement comment)
		{
		}

		// Token: 0x06001805 RID: 6149 RVA: 0x0004A858 File Offset: 0x00048A58
		public void visit(_IPragmaStatement pragma)
		{
		}

		// Token: 0x06001806 RID: 6150 RVA: 0x0004A85C File Offset: 0x00048A5C
		public void visit(_IErrorExpression errorexp)
		{
		}

		// Token: 0x06001807 RID: 6151 RVA: 0x0004A860 File Offset: 0x00048A60
		public void visit(_IErrorStatement errorst)
		{
		}

		// Token: 0x06001808 RID: 6152 RVA: 0x0004A864 File Offset: 0x00048A64
		public void visit(_INullExpression errorexp)
		{
		}

		// Token: 0x06001809 RID: 6153 RVA: 0x0004A868 File Offset: 0x00048A68
		public void visit(_INullStatement errorst)
		{
		}

		// Token: 0x0600180A RID: 6154 RVA: 0x0004A86C File Offset: 0x00048A6C
		public void visit(_IQualifiedNameExpression qne)
		{
		}

		// Token: 0x0600180B RID: 6155 RVA: 0x0004A870 File Offset: 0x00048A70
		public void visit(_ICompilerVersionExpression compiversionexp)
		{
		}

		// Token: 0x0600180C RID: 6156 RVA: 0x0004A874 File Offset: 0x00048A74
		public void visit(_IRuntimeVersionExpression runtimeversionexp)
		{
		}

		// Token: 0x0600180D RID: 6157 RVA: 0x0004A878 File Offset: 0x00048A78
		public void visit(_IIsEnumTypeExpression isenumtype)
		{
		}

		// Token: 0x0600180E RID: 6158 RVA: 0x0004A87C File Offset: 0x00048A7C
		public void visit(_IHasValueExpression hasvalue)
		{
		}

		// Token: 0x0600180F RID: 6159 RVA: 0x0004A880 File Offset: 0x00048A80
		public void visit(_IBreakPointStatement bpstate)
		{
		}

		// Token: 0x06001810 RID: 6160 RVA: 0x0004A884 File Offset: 0x00048A84
		public void visit(_IDefineStatement defstate)
		{
		}

		// Token: 0x06001811 RID: 6161 RVA: 0x0004A888 File Offset: 0x00048A88
		public void visit(_ITaskReference taskref)
		{
		}

		// Token: 0x06001812 RID: 6162 RVA: 0x0004A88C File Offset: 0x00048A8C
		public void visit(_IResourceReference resref)
		{
		}

		// Token: 0x06001813 RID: 6163 RVA: 0x0004A890 File Offset: 0x00048A90
		public void visit(_IDefineReference defref)
		{
		}

		// Token: 0x02000158 RID: 344
		private sealed class \u0001 : ChildVisitor
		{
			// Token: 0x06001815 RID: 6165 RVA: 0x0004A89C File Offset: 0x00048A9C
			public \u0001(ICollection<_IExprement> \u0012\u0008)
			{
				this.\u0001 = \u0012\u0008;
			}

			// Token: 0x06001816 RID: 6166 RVA: 0x0004A8AC File Offset: 0x00048AAC
			protected override void VisitChild(_IExprement exprement)
			{
				this.\u0001.Add(exprement);
			}

			// Token: 0x04000441 RID: 1089
			private readonly ICollection<_IExprement> \u0001;
		}
	}
}
