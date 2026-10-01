using System;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0013
{
	// Token: 0x02000072 RID: 114
	internal class \u0001 : IExprementVisitor, IExprementVisitor2, IStatementTraverser
	{
		// Token: 0x060008E6 RID: 2278 RVA: 0x00012394 File Offset: 0x00010594
		internal \u0001(IStatementVisitorNoTraversion \u009F)
		{
			this.\u0001 = \u009F;
		}

		// Token: 0x060008E7 RID: 2279 RVA: 0x000123A4 File Offset: 0x000105A4
		public virtual void visit(_ICompiledPOU cpou)
		{
			cpou.GetParseTree().Accept(this);
			this.\u0001.visit(cpou);
		}

		// Token: 0x060008E8 RID: 2280 RVA: 0x000123C0 File Offset: 0x000105C0
		public virtual void visit(_IWhileStatement whilst)
		{
			whilst._Controlled.Accept(this);
			this.\u0001.visit(whilst);
		}

		// Token: 0x060008E9 RID: 2281 RVA: 0x000123DC File Offset: 0x000105DC
		public virtual void visit(_IRepeatStatement repeat)
		{
			repeat._Controlled.Accept(this);
			this.\u0001.visit(repeat);
		}

		// Token: 0x060008EA RID: 2282 RVA: 0x000123F8 File Offset: 0x000105F8
		public virtual void visit(_IForStatement forloop)
		{
			forloop._CounterStart.Accept(this);
			forloop._Controlled.Accept(this);
			this.\u0001.visit(forloop);
		}

		// Token: 0x060008EB RID: 2283 RVA: 0x00012420 File Offset: 0x00010620
		public virtual void visit(_ISequenceStatement seq)
		{
			for (int i = 0; i < seq._StatementList.Count; i++)
			{
				seq._StatementList[i].Accept(this);
			}
			this.\u0001.visit(seq);
		}

		// Token: 0x060008EC RID: 2284 RVA: 0x00012464 File Offset: 0x00010664
		public virtual void visit(_IIfStatement ifst)
		{
			ifst._IfThen.Accept(this);
			foreach (_IElseIf ielseIf in ifst._ElseIf)
			{
				ielseIf._Controlled.Accept(this);
			}
			if (ifst._IfElse != null)
			{
				ifst._IfElse.Accept(this);
			}
			this.\u0001.visit(ifst);
		}

		// Token: 0x060008ED RID: 2285 RVA: 0x000124E0 File Offset: 0x000106E0
		public virtual void visit(_IExpressionStatement expstat)
		{
			this.\u0001.visit(expstat);
		}

		// Token: 0x060008EE RID: 2286 RVA: 0x000124F0 File Offset: 0x000106F0
		public virtual void visit(_ICaseLabelStatement caselabel)
		{
			this.\u0001.visit(caselabel);
		}

		// Token: 0x060008EF RID: 2287 RVA: 0x00012500 File Offset: 0x00010700
		public virtual void visit(_ICaseStatement casest)
		{
			foreach (_ICase icase in casest._Cases)
			{
				icase._Label.Accept(this);
				icase._Controlled.Accept(this);
			}
			if (casest._Else != null)
			{
				casest._Else.Accept(this);
			}
			this.\u0001.visit(casest);
		}

		// Token: 0x060008F0 RID: 2288 RVA: 0x0001257C File Offset: 0x0001077C
		public virtual void visit(_IExitStatement exit)
		{
			this.\u0001.visit(exit);
		}

		// Token: 0x060008F1 RID: 2289 RVA: 0x0001258C File Offset: 0x0001078C
		public virtual void visit(_IContinueStatement cont)
		{
			this.\u0001.visit(cont);
		}

		// Token: 0x060008F2 RID: 2290 RVA: 0x0001259C File Offset: 0x0001079C
		public virtual void visit(_IEmptyStatement empty)
		{
			this.\u0001.visit(empty);
		}

		// Token: 0x060008F3 RID: 2291 RVA: 0x000125AC File Offset: 0x000107AC
		public virtual void visit(_IReturnStatement returnst)
		{
			this.\u0001.visit(returnst);
		}

		// Token: 0x060008F4 RID: 2292 RVA: 0x000125BC File Offset: 0x000107BC
		public virtual void visit(_IJumpStatement gotost)
		{
			this.\u0001.visit(gotost);
		}

		// Token: 0x060008F5 RID: 2293 RVA: 0x000125CC File Offset: 0x000107CC
		public virtual void visit(_ILabelStatement label)
		{
			this.\u0001.visit(label);
		}

		// Token: 0x060008F6 RID: 2294 RVA: 0x000125DC File Offset: 0x000107DC
		public virtual void visit(_ICommentStatement comment)
		{
			this.\u0001.visit(comment);
		}

		// Token: 0x060008F7 RID: 2295 RVA: 0x000125EC File Offset: 0x000107EC
		public virtual void visit(_IPragmaStatement pragma)
		{
			this.\u0001.visit(pragma);
		}

		// Token: 0x060008F8 RID: 2296 RVA: 0x000125FC File Offset: 0x000107FC
		public virtual void visit(_IPragmaIfStatement pifst)
		{
			pifst.IfThen.Accept(this);
			foreach (_IPragmaElseIf ipragmaElseIf in pifst.ElseIf)
			{
				ipragmaElseIf.Controlled.Accept(this);
			}
			if (pifst.IfElse != null)
			{
				pifst.IfElse.Accept(this);
			}
			this.\u0001.visit(pifst);
		}

		// Token: 0x060008F9 RID: 2297 RVA: 0x00012678 File Offset: 0x00010878
		public virtual void visit(_IBreakPointStatement bpstate)
		{
			this.\u0001.visit(bpstate);
		}

		// Token: 0x060008FA RID: 2298 RVA: 0x00012688 File Offset: 0x00010888
		public virtual void visit(_IDefineStatement defstate)
		{
			this.\u0001.visit(defstate);
		}

		// Token: 0x060008FB RID: 2299 RVA: 0x00012698 File Offset: 0x00010898
		public virtual void visit(_IPragmaAssertion assertion)
		{
			this.\u0001.visit(assertion);
		}

		// Token: 0x060008FC RID: 2300 RVA: 0x000126A8 File Offset: 0x000108A8
		public virtual void visit(_IErrorStatement errorst)
		{
			this.\u0001.visit(errorst);
		}

		// Token: 0x060008FD RID: 2301 RVA: 0x000126B8 File Offset: 0x000108B8
		public virtual void visit(_INullStatement errorst)
		{
			this.\u0001.visit(errorst);
		}

		// Token: 0x060008FE RID: 2302 RVA: 0x000126C8 File Offset: 0x000108C8
		public virtual void visit(_IVariableDeclarationListStatement vdls)
		{
			vdls.VariableDeclaration.Accept(this);
		}

		// Token: 0x060008FF RID: 2303 RVA: 0x000126D8 File Offset: 0x000108D8
		public virtual void visit(_IEnumDeclarationListStatement eds)
		{
		}

		// Token: 0x06000900 RID: 2304 RVA: 0x000126DC File Offset: 0x000108DC
		public virtual void visit(_IPOUDeclarationStatement pds)
		{
			pds.Declarations.Accept(this);
		}

		// Token: 0x06000901 RID: 2305 RVA: 0x000126EC File Offset: 0x000108EC
		public virtual void visit(_ITypeDeclarationStatement tds)
		{
			tds.Declarations.Accept(this);
		}

		// Token: 0x06000902 RID: 2306 RVA: 0x000126FC File Offset: 0x000108FC
		public virtual void visit(_IAssignmentExpression assign)
		{
		}

		// Token: 0x06000903 RID: 2307 RVA: 0x00012700 File Offset: 0x00010900
		public virtual void visit(_ICallExpression call)
		{
		}

		// Token: 0x06000904 RID: 2308 RVA: 0x00012704 File Offset: 0x00010904
		public virtual void visit(_IOperatorExpression op)
		{
		}

		// Token: 0x06000905 RID: 2309 RVA: 0x00012708 File Offset: 0x00010908
		public virtual void visit(_ICastExpression cast)
		{
		}

		// Token: 0x06000906 RID: 2310 RVA: 0x0001270C File Offset: 0x0001090C
		public virtual void visit(_INewExpression newexp)
		{
		}

		// Token: 0x06000907 RID: 2311 RVA: 0x00012710 File Offset: 0x00010910
		public virtual void visit(_ITypeExpression typeexp)
		{
		}

		// Token: 0x06000908 RID: 2312 RVA: 0x00012714 File Offset: 0x00010914
		public virtual void visit(_IConversionExpression conv)
		{
		}

		// Token: 0x06000909 RID: 2313 RVA: 0x00012718 File Offset: 0x00010918
		public virtual void visit(_IIndexAccessExpression indexaccess)
		{
		}

		// Token: 0x0600090A RID: 2314 RVA: 0x0001271C File Offset: 0x0001091C
		public virtual void visit(_ILiteralExpression literal)
		{
		}

		// Token: 0x0600090B RID: 2315 RVA: 0x00012720 File Offset: 0x00010920
		public virtual void visit(_ICompoAccessExpression compo)
		{
		}

		// Token: 0x0600090C RID: 2316 RVA: 0x00012724 File Offset: 0x00010924
		public virtual void visit(_IDeRefAccessExpression deref)
		{
		}

		// Token: 0x0600090D RID: 2317 RVA: 0x00012728 File Offset: 0x00010928
		public virtual void visit(_ICopyScopeExpression copyexp)
		{
		}

		// Token: 0x0600090E RID: 2318 RVA: 0x0001272C File Offset: 0x0001092C
		public virtual void visit(_IGlobalScopeExpression globexp)
		{
		}

		// Token: 0x0600090F RID: 2319 RVA: 0x00012730 File Offset: 0x00010930
		public virtual void visit(_ISystemScopeExpression systemscope)
		{
		}

		// Token: 0x06000910 RID: 2320 RVA: 0x00012734 File Offset: 0x00010934
		public virtual void visit(_ICaseRangeExpression caserange)
		{
		}

		// Token: 0x06000911 RID: 2321 RVA: 0x00012738 File Offset: 0x00010938
		public virtual void visit(_IThisExpression thisexp)
		{
		}

		// Token: 0x06000912 RID: 2322 RVA: 0x0001273C File Offset: 0x0001093C
		public virtual void visit(_IBaseExpression baseexp)
		{
		}

		// Token: 0x06000913 RID: 2323 RVA: 0x00012740 File Offset: 0x00010940
		public virtual void visit(_IErrorExpression errorexp)
		{
		}

		// Token: 0x06000914 RID: 2324 RVA: 0x00012744 File Offset: 0x00010944
		public virtual void visit(_INullExpression errorexp)
		{
		}

		// Token: 0x06000915 RID: 2325 RVA: 0x00012748 File Offset: 0x00010948
		public virtual void visit(_IQualifiedNameExpression qne)
		{
		}

		// Token: 0x06000916 RID: 2326 RVA: 0x0001274C File Offset: 0x0001094C
		public virtual void visit(_IVariableDeclarationStatement vds)
		{
		}

		// Token: 0x06000917 RID: 2327 RVA: 0x00012750 File Offset: 0x00010950
		public virtual void visit(_IEnumDeclarationStatement eds)
		{
		}

		// Token: 0x06000918 RID: 2328 RVA: 0x00012754 File Offset: 0x00010954
		public virtual void visit(_IMultipleIndexInitialization errorst)
		{
		}

		// Token: 0x06000919 RID: 2329 RVA: 0x00012758 File Offset: 0x00010958
		public virtual void visit(_IArrayInitialization errorexp)
		{
		}

		// Token: 0x0600091A RID: 2330 RVA: 0x0001275C File Offset: 0x0001095C
		public virtual void visit(_IStructureInitialization errorst)
		{
		}

		// Token: 0x0600091B RID: 2331 RVA: 0x00012760 File Offset: 0x00010960
		public virtual void visit(_IDefineReference defref)
		{
		}

		// Token: 0x0600091C RID: 2332 RVA: 0x00012764 File Offset: 0x00010964
		public virtual void visit(_IVariableReference varref)
		{
		}

		// Token: 0x0600091D RID: 2333 RVA: 0x00012768 File Offset: 0x00010968
		public virtual void visit(_ITypeReference typeref)
		{
		}

		// Token: 0x0600091E RID: 2334 RVA: 0x0001276C File Offset: 0x0001096C
		public virtual void visit(_IPouReference pouref)
		{
		}

		// Token: 0x0600091F RID: 2335 RVA: 0x00012770 File Offset: 0x00010970
		public virtual void visit(_ITaskReference taskref)
		{
		}

		// Token: 0x06000920 RID: 2336 RVA: 0x00012774 File Offset: 0x00010974
		public virtual void visit(_IResourceReference resref)
		{
		}

		// Token: 0x06000921 RID: 2337 RVA: 0x00012778 File Offset: 0x00010978
		public virtual void visit(_IDefinedExpression defexp)
		{
		}

		// Token: 0x06000922 RID: 2338 RVA: 0x0001277C File Offset: 0x0001097C
		public virtual void visit(_IPragmaOperatorExpression popexp)
		{
		}

		// Token: 0x06000923 RID: 2339 RVA: 0x00012780 File Offset: 0x00010980
		public virtual void visit(_IXRefExpression xref)
		{
		}

		// Token: 0x06000924 RID: 2340 RVA: 0x00012784 File Offset: 0x00010984
		public virtual void visit(_IHasTypeExpression hastype)
		{
		}

		// Token: 0x06000925 RID: 2341 RVA: 0x00012788 File Offset: 0x00010988
		public virtual void visit(_IIsEnumTypeExpression isenumtype)
		{
		}

		// Token: 0x06000926 RID: 2342 RVA: 0x0001278C File Offset: 0x0001098C
		public virtual void visit(_IHasAttributeExpression hasattribute)
		{
		}

		// Token: 0x06000927 RID: 2343 RVA: 0x00012790 File Offset: 0x00010990
		public virtual void visit(_IHasValueExpression hasvalue)
		{
		}

		// Token: 0x06000928 RID: 2344 RVA: 0x00012794 File Offset: 0x00010994
		public virtual void visit(_IHasConstantValueExpression hasvalue)
		{
		}

		// Token: 0x06000929 RID: 2345 RVA: 0x00012798 File Offset: 0x00010998
		public virtual void visit(_IHasConstantTypeExpression hasConstantTypeExpression)
		{
		}

		// Token: 0x0600092A RID: 2346 RVA: 0x0001279C File Offset: 0x0001099C
		public virtual void visit(_ICompilerVersionExpression compversion)
		{
		}

		// Token: 0x0600092B RID: 2347 RVA: 0x000127A0 File Offset: 0x000109A0
		public void visit(_IAddressExpression address)
		{
		}

		// Token: 0x0600092C RID: 2348 RVA: 0x000127A4 File Offset: 0x000109A4
		public void visit(_IVariableExpression variable)
		{
		}

		// Token: 0x0600092D RID: 2349 RVA: 0x000127A8 File Offset: 0x000109A8
		public void \u0001(_ITryCatchStatement \u0002)
		{
			this.\u0001.visit(\u0002);
			\u0002.DefaultTraverse(this);
		}

		// Token: 0x04000130 RID: 304
		private IStatementVisitorNoTraversion \u0001;
	}
}
