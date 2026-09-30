using System;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler35220.Phase5_Codegeneration.Optimization
{
	// Token: 0x0200027F RID: 639
	public class ReplacementCheckerVisitor : IExprementVisitor352000, IExprementVisitor351900, IExprementVisitor351800, IExprementVisitor351500, IExprementVisitor351400, IExprementVisitor351300, IExprementVisitor3590, IExprementVisitor
	{
		// Token: 0x0600283C RID: 10300 RVA: 0x0008BC14 File Offset: 0x00089E14
		private ReplacementCheckerVisitor(ReplacerController[] controllers)
		{
			this.Controllers = controllers;
			this.Traverser = new SimpleStandardTraverser(this);
		}

		// Token: 0x0600283D RID: 10301 RVA: 0x0008BC30 File Offset: 0x00089E30
		public static void CheckForReplacements(ReplacerController[] controllers, _ICompiledPOU cpou)
		{
			new ReplacementCheckerVisitor(controllers).visit(cpou);
		}

		// Token: 0x17000736 RID: 1846
		// (get) Token: 0x0600283E RID: 10302 RVA: 0x0008BC40 File Offset: 0x00089E40
		private ReplacerController[] Controllers { get; }

		// Token: 0x17000737 RID: 1847
		// (get) Token: 0x0600283F RID: 10303 RVA: 0x0008BC48 File Offset: 0x00089E48
		// (set) Token: 0x06002840 RID: 10304 RVA: 0x0008BC50 File Offset: 0x00089E50
		public SimpleStandardTraverser Traverser { get; set; }

		// Token: 0x17000738 RID: 1848
		// (get) Token: 0x06002841 RID: 10305 RVA: 0x0008BC5C File Offset: 0x00089E5C
		public bool bResolveCompoAccessExpression
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000739 RID: 1849
		// (get) Token: 0x06002842 RID: 10306 RVA: 0x0008BC60 File Offset: 0x00089E60
		public bool DoCallExpression
		{
			get
			{
				return false;
			}
		}

		// Token: 0x1700073A RID: 1850
		// (get) Token: 0x06002843 RID: 10307 RVA: 0x0008BC64 File Offset: 0x00089E64
		public bool DoAssignExpression
		{
			get
			{
				return false;
			}
		}

		// Token: 0x06002844 RID: 10308 RVA: 0x0008BC68 File Offset: 0x00089E68
		public void visit(_ICompiledPOU cpou)
		{
			for (int i = 0; i < this.Controllers.Length; i++)
			{
				if (this.Controllers[i].Checker != null)
				{
					this.Controllers[i].Checker.VisitNecessary = (this.Controllers[i].Checker.ToVisit(cpou) || this.Controllers[i].Checker.VisitNecessary);
				}
			}
			cpou.GetParseTree().Accept(this.Traverser);
		}

		// Token: 0x06002845 RID: 10309 RVA: 0x0008BCE4 File Offset: 0x00089EE4
		public void visit(_IWhileStatement whilst)
		{
			for (int i = 0; i < this.Controllers.Length; i++)
			{
				if (this.Controllers[i].Checker != null && this.Controllers[i].Checker.DoTraversal)
				{
					this.Controllers[i].Checker.VisitNecessary = (this.Controllers[i].Checker.ToVisit(whilst) || this.Controllers[i].Checker.VisitNecessary);
				}
			}
		}

		// Token: 0x06002846 RID: 10310 RVA: 0x0008BD64 File Offset: 0x00089F64
		public void visit(_IRepeatStatement repeat)
		{
			for (int i = 0; i < this.Controllers.Length; i++)
			{
				if (this.Controllers[i].Checker != null && this.Controllers[i].Checker.DoTraversal)
				{
					this.Controllers[i].Checker.VisitNecessary = (this.Controllers[i].Checker.ToVisit(repeat) || this.Controllers[i].Checker.VisitNecessary);
				}
			}
		}

		// Token: 0x06002847 RID: 10311 RVA: 0x0008BDE4 File Offset: 0x00089FE4
		public void visit(_IForStatement forloop)
		{
			for (int i = 0; i < this.Controllers.Length; i++)
			{
				if (this.Controllers[i].Checker != null && this.Controllers[i].Checker.DoTraversal)
				{
					this.Controllers[i].Checker.VisitNecessary = (this.Controllers[i].Checker.ToVisit(forloop) || this.Controllers[i].Checker.VisitNecessary);
				}
			}
		}

		// Token: 0x06002848 RID: 10312 RVA: 0x0008BE64 File Offset: 0x0008A064
		public void visit(_IExitStatement exit)
		{
			for (int i = 0; i < this.Controllers.Length; i++)
			{
				if (this.Controllers[i].Checker != null && this.Controllers[i].Checker.DoTraversal)
				{
					this.Controllers[i].Checker.VisitNecessary = (this.Controllers[i].Checker.ToVisit(exit) || this.Controllers[i].Checker.VisitNecessary);
				}
			}
		}

		// Token: 0x06002849 RID: 10313 RVA: 0x0008BEE4 File Offset: 0x0008A0E4
		public void visit(_IContinueStatement cont)
		{
			for (int i = 0; i < this.Controllers.Length; i++)
			{
				if (this.Controllers[i].Checker != null && this.Controllers[i].Checker.DoTraversal)
				{
					this.Controllers[i].Checker.VisitNecessary = (this.Controllers[i].Checker.ToVisit(cont) || this.Controllers[i].Checker.VisitNecessary);
				}
			}
		}

		// Token: 0x0600284A RID: 10314 RVA: 0x0008BF64 File Offset: 0x0008A164
		public void visit(_ISequenceStatement seq)
		{
			for (int i = 0; i < this.Controllers.Length; i++)
			{
				if (this.Controllers[i].Checker != null && this.Controllers[i].Checker.DoTraversal)
				{
					this.Controllers[i].Checker.VisitNecessary = (this.Controllers[i].Checker.ToVisit(seq) || this.Controllers[i].Checker.VisitNecessary);
				}
			}
		}

		// Token: 0x0600284B RID: 10315 RVA: 0x0008BFE4 File Offset: 0x0008A1E4
		public void visit(_IAssignmentExpression assign)
		{
			for (int i = 0; i < this.Controllers.Length; i++)
			{
				if (this.Controllers[i].Checker != null && this.Controllers[i].Checker.DoTraversal)
				{
					this.Controllers[i].Checker.VisitNecessary = (this.Controllers[i].Checker.ToVisit(assign) || this.Controllers[i].Checker.VisitNecessary);
				}
			}
		}

		// Token: 0x0600284C RID: 10316 RVA: 0x0008C064 File Offset: 0x0008A264
		public void visit(_IIfStatement ifst)
		{
			for (int i = 0; i < this.Controllers.Length; i++)
			{
				if (this.Controllers[i].Checker != null && this.Controllers[i].Checker.DoTraversal)
				{
					this.Controllers[i].Checker.VisitNecessary = (this.Controllers[i].Checker.ToVisit(ifst) || this.Controllers[i].Checker.VisitNecessary);
				}
			}
		}

		// Token: 0x0600284D RID: 10317 RVA: 0x0008C0E4 File Offset: 0x0008A2E4
		public void visit(_IReturnStatement returnst)
		{
			for (int i = 0; i < this.Controllers.Length; i++)
			{
				if (this.Controllers[i].Checker != null && this.Controllers[i].Checker.DoTraversal)
				{
					this.Controllers[i].Checker.VisitNecessary = (this.Controllers[i].Checker.ToVisit(returnst) || this.Controllers[i].Checker.VisitNecessary);
				}
			}
		}

		// Token: 0x0600284E RID: 10318 RVA: 0x0008C164 File Offset: 0x0008A364
		public void visit(_IJumpStatement gotost)
		{
			for (int i = 0; i < this.Controllers.Length; i++)
			{
				if (this.Controllers[i].Checker != null && this.Controllers[i].Checker.DoTraversal)
				{
					this.Controllers[i].Checker.VisitNecessary = (this.Controllers[i].Checker.ToVisit(gotost) || this.Controllers[i].Checker.VisitNecessary);
				}
			}
		}

		// Token: 0x0600284F RID: 10319 RVA: 0x0008C1E4 File Offset: 0x0008A3E4
		public void visit(_ILabelStatement label)
		{
			for (int i = 0; i < this.Controllers.Length; i++)
			{
				if (this.Controllers[i].Checker != null && this.Controllers[i].Checker.DoTraversal)
				{
					this.Controllers[i].Checker.VisitNecessary = (this.Controllers[i].Checker.ToVisit(label) || this.Controllers[i].Checker.VisitNecessary);
				}
			}
		}

		// Token: 0x06002850 RID: 10320 RVA: 0x0008C264 File Offset: 0x0008A464
		public void visit(_ICommentStatement comment)
		{
			for (int i = 0; i < this.Controllers.Length; i++)
			{
				if (this.Controllers[i].Checker != null && this.Controllers[i].Checker.DoTraversal)
				{
					this.Controllers[i].Checker.VisitNecessary = (this.Controllers[i].Checker.ToVisit(comment) || this.Controllers[i].Checker.VisitNecessary);
				}
			}
		}

		// Token: 0x06002851 RID: 10321 RVA: 0x0008C2E4 File Offset: 0x0008A4E4
		public void visit(_IPragmaStatement pragma)
		{
			for (int i = 0; i < this.Controllers.Length; i++)
			{
				if (this.Controllers[i].Checker != null && this.Controllers[i].Checker.DoTraversal)
				{
					this.Controllers[i].Checker.VisitNecessary = (this.Controllers[i].Checker.ToVisit(pragma) || this.Controllers[i].Checker.VisitNecessary);
				}
			}
		}

		// Token: 0x06002852 RID: 10322 RVA: 0x0008C364 File Offset: 0x0008A564
		public void visit(_IExpressionStatement expstat)
		{
			for (int i = 0; i < this.Controllers.Length; i++)
			{
				if (this.Controllers[i].Checker != null && this.Controllers[i].Checker.DoTraversal)
				{
					this.Controllers[i].Checker.VisitNecessary = (this.Controllers[i].Checker.ToVisit(expstat) || this.Controllers[i].Checker.VisitNecessary);
				}
			}
		}

		// Token: 0x06002853 RID: 10323 RVA: 0x0008C3E4 File Offset: 0x0008A5E4
		public void visit(_ICallExpression call)
		{
			for (int i = 0; i < this.Controllers.Length; i++)
			{
				if (this.Controllers[i].Checker != null && this.Controllers[i].Checker.DoTraversal)
				{
					this.Controllers[i].Checker.VisitNecessary = (this.Controllers[i].Checker.ToVisit(call) || this.Controllers[i].Checker.VisitNecessary);
				}
			}
		}

		// Token: 0x06002854 RID: 10324 RVA: 0x0008C464 File Offset: 0x0008A664
		public void visit(_IOperatorExpression op)
		{
			for (int i = 0; i < this.Controllers.Length; i++)
			{
				if (this.Controllers[i].Checker != null && this.Controllers[i].Checker.DoTraversal)
				{
					this.Controllers[i].Checker.VisitNecessary = (this.Controllers[i].Checker.ToVisit(op) || this.Controllers[i].Checker.VisitNecessary);
				}
			}
		}

		// Token: 0x06002855 RID: 10325 RVA: 0x0008C4E4 File Offset: 0x0008A6E4
		public void visit(_IConversionExpression conv)
		{
			for (int i = 0; i < this.Controllers.Length; i++)
			{
				if (this.Controllers[i].Checker != null && this.Controllers[i].Checker.DoTraversal)
				{
					this.Controllers[i].Checker.VisitNecessary = (this.Controllers[i].Checker.ToVisit(conv) || this.Controllers[i].Checker.VisitNecessary);
				}
			}
		}

		// Token: 0x06002856 RID: 10326 RVA: 0x0008C564 File Offset: 0x0008A764
		public void visit(_ICastExpression castexp)
		{
			for (int i = 0; i < this.Controllers.Length; i++)
			{
				if (this.Controllers[i].Checker != null && this.Controllers[i].Checker.DoTraversal)
				{
					this.Controllers[i].Checker.VisitNecessary = (this.Controllers[i].Checker.ToVisit(castexp) || this.Controllers[i].Checker.VisitNecessary);
				}
			}
		}

		// Token: 0x06002857 RID: 10327 RVA: 0x0008C5E4 File Offset: 0x0008A7E4
		public void visit(_IThisExpression thisexp)
		{
			for (int i = 0; i < this.Controllers.Length; i++)
			{
				if (this.Controllers[i].Checker != null && this.Controllers[i].Checker.DoTraversal)
				{
					this.Controllers[i].Checker.VisitNecessary = (this.Controllers[i].Checker.ToVisit(thisexp) || this.Controllers[i].Checker.VisitNecessary);
				}
			}
		}

		// Token: 0x06002858 RID: 10328 RVA: 0x0008C664 File Offset: 0x0008A864
		public void visit(_IBaseExpression baseexp)
		{
			for (int i = 0; i < this.Controllers.Length; i++)
			{
				if (this.Controllers[i].Checker != null && this.Controllers[i].Checker.DoTraversal)
				{
					this.Controllers[i].Checker.VisitNecessary = (this.Controllers[i].Checker.ToVisit(baseexp) || this.Controllers[i].Checker.VisitNecessary);
				}
			}
		}

		// Token: 0x06002859 RID: 10329 RVA: 0x0008C6E4 File Offset: 0x0008A8E4
		public void visit(_ILiteralExpression literal)
		{
			for (int i = 0; i < this.Controllers.Length; i++)
			{
				if (this.Controllers[i].Checker != null && this.Controllers[i].Checker.DoTraversal)
				{
					this.Controllers[i].Checker.VisitNecessary = (this.Controllers[i].Checker.ToVisit(literal) || this.Controllers[i].Checker.VisitNecessary);
				}
			}
		}

		// Token: 0x0600285A RID: 10330 RVA: 0x0008C764 File Offset: 0x0008A964
		public void visit(_IAddressExpression address)
		{
			for (int i = 0; i < this.Controllers.Length; i++)
			{
				if (this.Controllers[i].Checker != null && this.Controllers[i].Checker.DoTraversal)
				{
					this.Controllers[i].Checker.VisitNecessary = (this.Controllers[i].Checker.ToVisit(address) || this.Controllers[i].Checker.VisitNecessary);
				}
			}
		}

		// Token: 0x0600285B RID: 10331 RVA: 0x0008C7E4 File Offset: 0x0008A9E4
		public void visit(_IVariableExpression variable)
		{
			for (int i = 0; i < this.Controllers.Length; i++)
			{
				if (this.Controllers[i].Checker != null && this.Controllers[i].Checker.DoTraversal)
				{
					this.Controllers[i].Checker.VisitNecessary = (this.Controllers[i].Checker.ToVisit(variable) || this.Controllers[i].Checker.VisitNecessary);
				}
			}
		}

		// Token: 0x0600285C RID: 10332 RVA: 0x0008C864 File Offset: 0x0008AA64
		public void visit(_IIndexAccessExpression indexaccess)
		{
			for (int i = 0; i < this.Controllers.Length; i++)
			{
				if (this.Controllers[i].Checker != null && this.Controllers[i].Checker.DoTraversal)
				{
					this.Controllers[i].Checker.VisitNecessary = (this.Controllers[i].Checker.ToVisit(indexaccess) || this.Controllers[i].Checker.VisitNecessary);
				}
			}
		}

		// Token: 0x0600285D RID: 10333 RVA: 0x0008C8E4 File Offset: 0x0008AAE4
		public void visit(_ICompoAccessExpression compo)
		{
			for (int i = 0; i < this.Controllers.Length; i++)
			{
				if (this.Controllers[i].Checker != null && this.Controllers[i].Checker.DoTraversal)
				{
					this.Controllers[i].Checker.VisitNecessary = (this.Controllers[i].Checker.ToVisit(compo) || this.Controllers[i].Checker.VisitNecessary);
				}
			}
		}

		// Token: 0x0600285E RID: 10334 RVA: 0x0008C964 File Offset: 0x0008AB64
		public void visit(_IDeRefAccessExpression deref)
		{
			for (int i = 0; i < this.Controllers.Length; i++)
			{
				if (this.Controllers[i].Checker != null && this.Controllers[i].Checker.DoTraversal)
				{
					this.Controllers[i].Checker.VisitNecessary = (this.Controllers[i].Checker.ToVisit(deref) || this.Controllers[i].Checker.VisitNecessary);
				}
			}
		}

		// Token: 0x0600285F RID: 10335 RVA: 0x0008C9E4 File Offset: 0x0008ABE4
		public void visit(_ICopyScopeExpression copyexp)
		{
			for (int i = 0; i < this.Controllers.Length; i++)
			{
				if (this.Controllers[i].Checker != null && this.Controllers[i].Checker.DoTraversal)
				{
					this.Controllers[i].Checker.VisitNecessary = (this.Controllers[i].Checker.ToVisit(copyexp) || this.Controllers[i].Checker.VisitNecessary);
				}
			}
		}

		// Token: 0x06002860 RID: 10336 RVA: 0x0008CA64 File Offset: 0x0008AC64
		public void visit(_IGlobalScopeExpression globexp)
		{
			for (int i = 0; i < this.Controllers.Length; i++)
			{
				if (this.Controllers[i].Checker != null && this.Controllers[i].Checker.DoTraversal)
				{
					this.Controllers[i].Checker.VisitNecessary = (this.Controllers[i].Checker.ToVisit(globexp) || this.Controllers[i].Checker.VisitNecessary);
				}
			}
		}

		// Token: 0x06002861 RID: 10337 RVA: 0x0008CAE4 File Offset: 0x0008ACE4
		public void visit(_ISystemScopeExpression systemscope)
		{
			for (int i = 0; i < this.Controllers.Length; i++)
			{
				if (this.Controllers[i].Checker != null && this.Controllers[i].Checker.DoTraversal)
				{
					this.Controllers[i].Checker.VisitNecessary = (this.Controllers[i].Checker.ToVisit(systemscope) || this.Controllers[i].Checker.VisitNecessary);
				}
			}
		}

		// Token: 0x06002862 RID: 10338 RVA: 0x0008CB64 File Offset: 0x0008AD64
		public void visit(_IEmptyStatement empty)
		{
			for (int i = 0; i < this.Controllers.Length; i++)
			{
				if (this.Controllers[i].Checker != null && this.Controllers[i].Checker.DoTraversal)
				{
					this.Controllers[i].Checker.VisitNecessary = (this.Controllers[i].Checker.ToVisit(empty) || this.Controllers[i].Checker.VisitNecessary);
				}
			}
		}

		// Token: 0x06002863 RID: 10339 RVA: 0x0008CBE4 File Offset: 0x0008ADE4
		public void visit(_ICaseRangeExpression caserange)
		{
			for (int i = 0; i < this.Controllers.Length; i++)
			{
				if (this.Controllers[i].Checker != null && this.Controllers[i].Checker.DoTraversal)
				{
					this.Controllers[i].Checker.VisitNecessary = (this.Controllers[i].Checker.ToVisit(caserange) || this.Controllers[i].Checker.VisitNecessary);
				}
			}
		}

		// Token: 0x06002864 RID: 10340 RVA: 0x0008CC64 File Offset: 0x0008AE64
		public void visit(_ICaseLabelStatement caselabel)
		{
			for (int i = 0; i < this.Controllers.Length; i++)
			{
				if (this.Controllers[i].Checker != null && this.Controllers[i].Checker.DoTraversal)
				{
					this.Controllers[i].Checker.VisitNecessary = (this.Controllers[i].Checker.ToVisit(caselabel) || this.Controllers[i].Checker.VisitNecessary);
				}
			}
		}

		// Token: 0x06002865 RID: 10341 RVA: 0x0008CCE4 File Offset: 0x0008AEE4
		public void visit(_ICaseStatement casest)
		{
			for (int i = 0; i < this.Controllers.Length; i++)
			{
				if (this.Controllers[i].Checker != null && this.Controllers[i].Checker.DoTraversal)
				{
					this.Controllers[i].Checker.VisitNecessary = (this.Controllers[i].Checker.ToVisit(casest) || this.Controllers[i].Checker.VisitNecessary);
				}
			}
		}

		// Token: 0x06002866 RID: 10342 RVA: 0x0008CD64 File Offset: 0x0008AF64
		public void visit(_IErrorExpression errorexp)
		{
			for (int i = 0; i < this.Controllers.Length; i++)
			{
				if (this.Controllers[i].Checker != null && this.Controllers[i].Checker.DoTraversal)
				{
					this.Controllers[i].Checker.VisitNecessary = (this.Controllers[i].Checker.ToVisit(errorexp) || this.Controllers[i].Checker.VisitNecessary);
				}
			}
		}

		// Token: 0x06002867 RID: 10343 RVA: 0x0008CDE4 File Offset: 0x0008AFE4
		public void visit(_IErrorStatement errorst)
		{
			for (int i = 0; i < this.Controllers.Length; i++)
			{
				if (this.Controllers[i].Checker != null && this.Controllers[i].Checker.DoTraversal)
				{
					this.Controllers[i].Checker.VisitNecessary = (this.Controllers[i].Checker.ToVisit(errorst) || this.Controllers[i].Checker.VisitNecessary);
				}
			}
		}

		// Token: 0x06002868 RID: 10344 RVA: 0x0008CE64 File Offset: 0x0008B064
		public void visit(_INullExpression errorexp)
		{
			for (int i = 0; i < this.Controllers.Length; i++)
			{
				if (this.Controllers[i].Checker != null && this.Controllers[i].Checker.DoTraversal)
				{
					this.Controllers[i].Checker.VisitNecessary = (this.Controllers[i].Checker.ToVisit(errorexp) || this.Controllers[i].Checker.VisitNecessary);
				}
			}
		}

		// Token: 0x06002869 RID: 10345 RVA: 0x0008CEE4 File Offset: 0x0008B0E4
		public void visit(_INullStatement errorst)
		{
			for (int i = 0; i < this.Controllers.Length; i++)
			{
				if (this.Controllers[i].Checker != null && this.Controllers[i].Checker.DoTraversal)
				{
					this.Controllers[i].Checker.VisitNecessary = (this.Controllers[i].Checker.ToVisit(errorst) || this.Controllers[i].Checker.VisitNecessary);
				}
			}
		}

		// Token: 0x0600286A RID: 10346 RVA: 0x0008CF64 File Offset: 0x0008B164
		public void visit(_IQualifiedNameExpression qne)
		{
			for (int i = 0; i < this.Controllers.Length; i++)
			{
				if (this.Controllers[i].Checker != null && this.Controllers[i].Checker.DoTraversal)
				{
					this.Controllers[i].Checker.VisitNecessary = (this.Controllers[i].Checker.ToVisit(qne) || this.Controllers[i].Checker.VisitNecessary);
				}
			}
		}

		// Token: 0x0600286B RID: 10347 RVA: 0x0008CFE4 File Offset: 0x0008B1E4
		public void visit(_IVariableDeclarationStatement vds)
		{
			for (int i = 0; i < this.Controllers.Length; i++)
			{
				if (this.Controllers[i].Checker != null && this.Controllers[i].Checker.DoTraversal)
				{
					this.Controllers[i].Checker.VisitNecessary = (this.Controllers[i].Checker.ToVisit(vds) || this.Controllers[i].Checker.VisitNecessary);
				}
			}
		}

		// Token: 0x0600286C RID: 10348 RVA: 0x0008D064 File Offset: 0x0008B264
		public void visit(_IVariableDeclarationListStatement vdls)
		{
			for (int i = 0; i < this.Controllers.Length; i++)
			{
				if (this.Controllers[i].Checker != null && this.Controllers[i].Checker.DoTraversal)
				{
					this.Controllers[i].Checker.VisitNecessary = (this.Controllers[i].Checker.ToVisit(vdls) || this.Controllers[i].Checker.VisitNecessary);
				}
			}
		}

		// Token: 0x0600286D RID: 10349 RVA: 0x0008D0E4 File Offset: 0x0008B2E4
		public void visit(_IPOUDeclarationStatement pds)
		{
			for (int i = 0; i < this.Controllers.Length; i++)
			{
				if (this.Controllers[i].Checker != null && this.Controllers[i].Checker.DoTraversal)
				{
					this.Controllers[i].Checker.VisitNecessary = (this.Controllers[i].Checker.ToVisit(pds) || this.Controllers[i].Checker.VisitNecessary);
				}
			}
		}

		// Token: 0x0600286E RID: 10350 RVA: 0x0008D164 File Offset: 0x0008B364
		public void visit(_ITypeDeclarationStatement tds)
		{
			for (int i = 0; i < this.Controllers.Length; i++)
			{
				if (this.Controllers[i].Checker != null && this.Controllers[i].Checker.DoTraversal)
				{
					this.Controllers[i].Checker.VisitNecessary = (this.Controllers[i].Checker.ToVisit(tds) || this.Controllers[i].Checker.VisitNecessary);
				}
			}
		}

		// Token: 0x0600286F RID: 10351 RVA: 0x0008D1E4 File Offset: 0x0008B3E4
		public void visit(_IEnumDeclarationStatement eds)
		{
			for (int i = 0; i < this.Controllers.Length; i++)
			{
				if (this.Controllers[i].Checker != null && this.Controllers[i].Checker.DoTraversal)
				{
					this.Controllers[i].Checker.VisitNecessary = (this.Controllers[i].Checker.ToVisit(eds) || this.Controllers[i].Checker.VisitNecessary);
				}
			}
		}

		// Token: 0x06002870 RID: 10352 RVA: 0x0008D264 File Offset: 0x0008B464
		public void visit(_IEnumDeclarationListStatement eds)
		{
			for (int i = 0; i < this.Controllers.Length; i++)
			{
				if (this.Controllers[i].Checker != null && this.Controllers[i].Checker.DoTraversal)
				{
					this.Controllers[i].Checker.VisitNecessary = (this.Controllers[i].Checker.ToVisit(eds) || this.Controllers[i].Checker.VisitNecessary);
				}
			}
		}

		// Token: 0x06002871 RID: 10353 RVA: 0x0008D2E4 File Offset: 0x0008B4E4
		public void visit(_IMultipleIndexInitialization errorst)
		{
			for (int i = 0; i < this.Controllers.Length; i++)
			{
				if (this.Controllers[i].Checker != null && this.Controllers[i].Checker.DoTraversal)
				{
					this.Controllers[i].Checker.VisitNecessary = (this.Controllers[i].Checker.ToVisit(errorst) || this.Controllers[i].Checker.VisitNecessary);
				}
			}
		}

		// Token: 0x06002872 RID: 10354 RVA: 0x0008D364 File Offset: 0x0008B564
		public void visit(_IArrayInitialization errorexp)
		{
			for (int i = 0; i < this.Controllers.Length; i++)
			{
				if (this.Controllers[i].Checker != null && this.Controllers[i].Checker.DoTraversal)
				{
					this.Controllers[i].Checker.VisitNecessary = (this.Controllers[i].Checker.ToVisit(errorexp) || this.Controllers[i].Checker.VisitNecessary);
				}
			}
		}

		// Token: 0x06002873 RID: 10355 RVA: 0x0008D3E4 File Offset: 0x0008B5E4
		public void visit(_IStructureInitialization errorst)
		{
			for (int i = 0; i < this.Controllers.Length; i++)
			{
				if (this.Controllers[i].Checker != null && this.Controllers[i].Checker.DoTraversal)
				{
					this.Controllers[i].Checker.VisitNecessary = (this.Controllers[i].Checker.ToVisit(errorst) || this.Controllers[i].Checker.VisitNecessary);
				}
			}
		}

		// Token: 0x06002874 RID: 10356 RVA: 0x0008D464 File Offset: 0x0008B664
		public void visit(_IDefineReference defref)
		{
			for (int i = 0; i < this.Controllers.Length; i++)
			{
				if (this.Controllers[i].Checker != null && this.Controllers[i].Checker.DoTraversal)
				{
					this.Controllers[i].Checker.VisitNecessary = (this.Controllers[i].Checker.ToVisit(defref) || this.Controllers[i].Checker.VisitNecessary);
				}
			}
		}

		// Token: 0x06002875 RID: 10357 RVA: 0x0008D4E4 File Offset: 0x0008B6E4
		public void visit(_IVariableReference varref)
		{
			for (int i = 0; i < this.Controllers.Length; i++)
			{
				if (this.Controllers[i].Checker != null && this.Controllers[i].Checker.DoTraversal)
				{
					this.Controllers[i].Checker.VisitNecessary = (this.Controllers[i].Checker.ToVisit(varref) || this.Controllers[i].Checker.VisitNecessary);
				}
			}
		}

		// Token: 0x06002876 RID: 10358 RVA: 0x0008D564 File Offset: 0x0008B764
		public void visit(_ITypeReference typeref)
		{
			for (int i = 0; i < this.Controllers.Length; i++)
			{
				if (this.Controllers[i].Checker != null && this.Controllers[i].Checker.DoTraversal)
				{
					this.Controllers[i].Checker.VisitNecessary = (this.Controllers[i].Checker.ToVisit(typeref) || this.Controllers[i].Checker.VisitNecessary);
				}
			}
		}

		// Token: 0x06002877 RID: 10359 RVA: 0x0008D5E4 File Offset: 0x0008B7E4
		public void visit(_IPouReference pouref)
		{
			for (int i = 0; i < this.Controllers.Length; i++)
			{
				if (this.Controllers[i].Checker != null && this.Controllers[i].Checker.DoTraversal)
				{
					this.Controllers[i].Checker.VisitNecessary = (this.Controllers[i].Checker.ToVisit(pouref) || this.Controllers[i].Checker.VisitNecessary);
				}
			}
		}

		// Token: 0x06002878 RID: 10360 RVA: 0x0008D664 File Offset: 0x0008B864
		public void visit(_ITaskReference taskref)
		{
			for (int i = 0; i < this.Controllers.Length; i++)
			{
				if (this.Controllers[i].Checker != null && this.Controllers[i].Checker.DoTraversal)
				{
					this.Controllers[i].Checker.VisitNecessary = (this.Controllers[i].Checker.ToVisit(taskref) || this.Controllers[i].Checker.VisitNecessary);
				}
			}
		}

		// Token: 0x06002879 RID: 10361 RVA: 0x0008D6E4 File Offset: 0x0008B8E4
		public void visit(_IResourceReference resref)
		{
			for (int i = 0; i < this.Controllers.Length; i++)
			{
				if (this.Controllers[i].Checker != null && this.Controllers[i].Checker.DoTraversal)
				{
					this.Controllers[i].Checker.VisitNecessary = (this.Controllers[i].Checker.ToVisit(resref) || this.Controllers[i].Checker.VisitNecessary);
				}
			}
		}

		// Token: 0x0600287A RID: 10362 RVA: 0x0008D764 File Offset: 0x0008B964
		public void visit(_IDefinedExpression defexp)
		{
			for (int i = 0; i < this.Controllers.Length; i++)
			{
				if (this.Controllers[i].Checker != null && this.Controllers[i].Checker.DoTraversal)
				{
					this.Controllers[i].Checker.VisitNecessary = (this.Controllers[i].Checker.ToVisit(defexp) || this.Controllers[i].Checker.VisitNecessary);
				}
			}
		}

		// Token: 0x0600287B RID: 10363 RVA: 0x0008D7E4 File Offset: 0x0008B9E4
		public void visit(_IPragmaOperatorExpression popexp)
		{
			for (int i = 0; i < this.Controllers.Length; i++)
			{
				if (this.Controllers[i].Checker != null && this.Controllers[i].Checker.DoTraversal)
				{
					this.Controllers[i].Checker.VisitNecessary = (this.Controllers[i].Checker.ToVisit(popexp) || this.Controllers[i].Checker.VisitNecessary);
				}
			}
		}

		// Token: 0x0600287C RID: 10364 RVA: 0x0008D864 File Offset: 0x0008BA64
		public void visit(_IPragmaIfStatement pifst)
		{
			for (int i = 0; i < this.Controllers.Length; i++)
			{
				if (this.Controllers[i].Checker != null && this.Controllers[i].Checker.DoTraversal)
				{
					this.Controllers[i].Checker.VisitNecessary = (this.Controllers[i].Checker.ToVisit(pifst) || this.Controllers[i].Checker.VisitNecessary);
				}
			}
		}

		// Token: 0x0600287D RID: 10365 RVA: 0x0008D8E4 File Offset: 0x0008BAE4
		public void visit(_IBreakPointStatement bpstate)
		{
			for (int i = 0; i < this.Controllers.Length; i++)
			{
				if (this.Controllers[i].Checker != null && this.Controllers[i].Checker.DoTraversal)
				{
					this.Controllers[i].Checker.VisitNecessary = (this.Controllers[i].Checker.ToVisit(bpstate) || this.Controllers[i].Checker.VisitNecessary);
				}
			}
		}

		// Token: 0x0600287E RID: 10366 RVA: 0x0008D964 File Offset: 0x0008BB64
		public void visit(_IDefineStatement defstate)
		{
			for (int i = 0; i < this.Controllers.Length; i++)
			{
				if (this.Controllers[i].Checker != null && this.Controllers[i].Checker.DoTraversal)
				{
					this.Controllers[i].Checker.VisitNecessary = (this.Controllers[i].Checker.ToVisit(defstate) || this.Controllers[i].Checker.VisitNecessary);
				}
			}
		}

		// Token: 0x0600287F RID: 10367 RVA: 0x0008D9E4 File Offset: 0x0008BBE4
		public void visit(_IXRefExpression xref)
		{
			for (int i = 0; i < this.Controllers.Length; i++)
			{
				if (this.Controllers[i].Checker != null && this.Controllers[i].Checker.DoTraversal)
				{
					this.Controllers[i].Checker.VisitNecessary = (this.Controllers[i].Checker.ToVisit(xref) || this.Controllers[i].Checker.VisitNecessary);
				}
			}
		}

		// Token: 0x06002880 RID: 10368 RVA: 0x0008DA64 File Offset: 0x0008BC64
		public void visit(_IHasTypeExpression hastype)
		{
			for (int i = 0; i < this.Controllers.Length; i++)
			{
				if (this.Controllers[i].Checker != null && this.Controllers[i].Checker.DoTraversal)
				{
					this.Controllers[i].Checker.VisitNecessary = (this.Controllers[i].Checker.ToVisit(hastype) || this.Controllers[i].Checker.VisitNecessary);
				}
			}
		}

		// Token: 0x06002881 RID: 10369 RVA: 0x0008DAE4 File Offset: 0x0008BCE4
		public void visit(_IIsEnumTypeExpression isenumtype)
		{
			for (int i = 0; i < this.Controllers.Length; i++)
			{
				if (this.Controllers[i].Checker != null && this.Controllers[i].Checker.DoTraversal)
				{
					this.Controllers[i].Checker.VisitNecessary = (this.Controllers[i].Checker.ToVisit(isenumtype) || this.Controllers[i].Checker.VisitNecessary);
				}
			}
		}

		// Token: 0x06002882 RID: 10370 RVA: 0x0008DB64 File Offset: 0x0008BD64
		public void visit(_IHasAttributeExpression hasattribute)
		{
			for (int i = 0; i < this.Controllers.Length; i++)
			{
				if (this.Controllers[i].Checker != null && this.Controllers[i].Checker.DoTraversal)
				{
					this.Controllers[i].Checker.VisitNecessary = (this.Controllers[i].Checker.ToVisit(hasattribute) || this.Controllers[i].Checker.VisitNecessary);
				}
			}
		}

		// Token: 0x06002883 RID: 10371 RVA: 0x0008DBE4 File Offset: 0x0008BDE4
		public void visit(_IHasValueExpression hasvalue)
		{
			for (int i = 0; i < this.Controllers.Length; i++)
			{
				if (this.Controllers[i].Checker != null && this.Controllers[i].Checker.DoTraversal)
				{
					this.Controllers[i].Checker.VisitNecessary = (this.Controllers[i].Checker.ToVisit(hasvalue) || this.Controllers[i].Checker.VisitNecessary);
				}
			}
		}

		// Token: 0x06002884 RID: 10372 RVA: 0x0008DC64 File Offset: 0x0008BE64
		public void visit(_IHasConstantValueExpression hasvalue)
		{
			for (int i = 0; i < this.Controllers.Length; i++)
			{
				if (this.Controllers[i].Checker != null && this.Controllers[i].Checker.DoTraversal)
				{
					this.Controllers[i].Checker.VisitNecessary = (this.Controllers[i].Checker.ToVisit(hasvalue) || this.Controllers[i].Checker.VisitNecessary);
				}
			}
		}

		// Token: 0x06002885 RID: 10373 RVA: 0x0008DCE4 File Offset: 0x0008BEE4
		public void visit(_IPragmaAssertion assertion)
		{
			for (int i = 0; i < this.Controllers.Length; i++)
			{
				if (this.Controllers[i].Checker != null && this.Controllers[i].Checker.DoTraversal)
				{
					this.Controllers[i].Checker.VisitNecessary = (this.Controllers[i].Checker.ToVisit(assertion) || this.Controllers[i].Checker.VisitNecessary);
				}
			}
		}

		// Token: 0x06002886 RID: 10374 RVA: 0x0008DD64 File Offset: 0x0008BF64
		public void visit(_ICompilerVersionExpression compiversionexp)
		{
			for (int i = 0; i < this.Controllers.Length; i++)
			{
				if (this.Controllers[i].Checker != null && this.Controllers[i].Checker.DoTraversal)
				{
					this.Controllers[i].Checker.VisitNecessary = (this.Controllers[i].Checker.ToVisit(compiversionexp) || this.Controllers[i].Checker.VisitNecessary);
				}
			}
		}

		// Token: 0x06002887 RID: 10375 RVA: 0x0008DDE4 File Offset: 0x0008BFE4
		public void visit(_INewExpression typeref)
		{
			for (int i = 0; i < this.Controllers.Length; i++)
			{
				if (this.Controllers[i].Checker != null && this.Controllers[i].Checker.DoTraversal)
				{
					this.Controllers[i].Checker.VisitNecessary = (this.Controllers[i].Checker.ToVisit(typeref) || this.Controllers[i].Checker.VisitNecessary);
				}
			}
		}

		// Token: 0x06002888 RID: 10376 RVA: 0x0008DE64 File Offset: 0x0008C064
		public void visit(_ITypeExpression typeexp)
		{
			for (int i = 0; i < this.Controllers.Length; i++)
			{
				if (this.Controllers[i].Checker != null && this.Controllers[i].Checker.DoTraversal)
				{
					this.Controllers[i].Checker.VisitNecessary = (this.Controllers[i].Checker.ToVisit(typeexp) || this.Controllers[i].Checker.VisitNecessary);
				}
			}
		}

		// Token: 0x06002889 RID: 10377 RVA: 0x0008DEE4 File Offset: 0x0008C0E4
		public void visit(_ITryCatchStatement trycatch)
		{
			for (int i = 0; i < this.Controllers.Length; i++)
			{
				if (this.Controllers[i].Checker != null && this.Controllers[i].Checker.DoTraversal)
				{
					this.Controllers[i].Checker.VisitNecessary = (this.Controllers[i].Checker.ToVisit(trycatch) || this.Controllers[i].Checker.VisitNecessary);
				}
			}
		}

		// Token: 0x0600288A RID: 10378 RVA: 0x0008DF64 File Offset: 0x0008C164
		public void visit(_IPoolScopeExpression poolscope)
		{
			for (int i = 0; i < this.Controllers.Length; i++)
			{
				if (this.Controllers[i].Checker != null && this.Controllers[i].Checker.DoTraversal)
				{
					this.Controllers[i].Checker.VisitNecessary = (this.Controllers[i].Checker.ToVisit(poolscope) || this.Controllers[i].Checker.VisitNecessary);
				}
			}
		}

		// Token: 0x0600288B RID: 10379 RVA: 0x0008DFE4 File Offset: 0x0008C1E4
		public void visit(_ICurrentTaskExpression currentTask)
		{
			for (int i = 0; i < this.Controllers.Length; i++)
			{
				if (this.Controllers[i].Checker != null && this.Controllers[i].Checker.DoTraversal)
				{
					this.Controllers[i].Checker.VisitNecessary = (this.Controllers[i].Checker.ToVisit(currentTask) || this.Controllers[i].Checker.VisitNecessary);
				}
			}
		}

		// Token: 0x0600288C RID: 10380 RVA: 0x0008E064 File Offset: 0x0008C264
		public void visit(_IRuntimeVersionExpression runtimeversionexp)
		{
			for (int i = 0; i < this.Controllers.Length; i++)
			{
				if (this.Controllers[i].Checker != null && this.Controllers[i].Checker.DoTraversal)
				{
					this.Controllers[i].Checker.VisitNecessary = (this.Controllers[i].Checker.ToVisit(runtimeversionexp) || this.Controllers[i].Checker.VisitNecessary);
				}
			}
		}

		// Token: 0x0600288D RID: 10381 RVA: 0x0008E0E4 File Offset: 0x0008C2E4
		public void visit(_INamespaceAccessExpression namespaceaccess)
		{
			for (int i = 0; i < this.Controllers.Length; i++)
			{
				if (this.Controllers[i].Checker != null && this.Controllers[i].Checker.DoTraversal)
				{
					this.Controllers[i].Checker.VisitNecessary = (this.Controllers[i].Checker.ToVisit(namespaceaccess) || this.Controllers[i].Checker.VisitNecessary);
				}
			}
		}

		// Token: 0x0600288E RID: 10382 RVA: 0x0008E164 File Offset: 0x0008C364
		public void visit(_IHasConstantTypeExpression hasConstantTypeExpression)
		{
			for (int i = 0; i < this.Controllers.Length; i++)
			{
				if (this.Controllers[i].Checker != null && this.Controllers[i].Checker.DoTraversal)
				{
					this.Controllers[i].Checker.VisitNecessary = (this.Controllers[i].Checker.ToVisit(hasConstantTypeExpression) || this.Controllers[i].Checker.VisitNecessary);
				}
			}
		}

		// Token: 0x0600288F RID: 10383 RVA: 0x0008E1E4 File Offset: 0x0008C3E4
		public void visit(_IPartialAccessExpression partialAccessExpression)
		{
			for (int i = 0; i < this.Controllers.Length; i++)
			{
				if (this.Controllers[i].Checker != null && this.Controllers[i].Checker.DoTraversal)
				{
					this.Controllers[i].Checker.VisitNecessary = (this.Controllers[i].Checker.ToVisit(partialAccessExpression) || this.Controllers[i].Checker.VisitNecessary);
				}
			}
		}

		// Token: 0x06002890 RID: 10384 RVA: 0x0008E264 File Offset: 0x0008C464
		public void visit(_IProjectDefinedExpression projectDefinedExpression)
		{
			for (int i = 0; i < this.Controllers.Length; i++)
			{
				if (this.Controllers[i].Checker != null && this.Controllers[i].Checker.DoTraversal)
				{
					this.Controllers[i].Checker.VisitNecessary = (this.Controllers[i].Checker.ToVisit(projectDefinedExpression) || this.Controllers[i].Checker.VisitNecessary);
				}
			}
		}

		// Token: 0x04000770 RID: 1904
		[CompilerGenerated]
		private readonly ReplacerController[] \u0001;

		// Token: 0x04000771 RID: 1905
		[CompilerGenerated]
		private SimpleStandardTraverser \u0001;
	}
}
