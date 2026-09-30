using System;
using System.Diagnostics.CodeAnalysis;
using \u0003;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0082
{
	// Token: 0x02000292 RID: 658
	[ExcludeFromCodeCoverage]
	internal class \u0011
	{
		// Token: 0x06002976 RID: 10614 RVA: 0x00091310 File Offset: 0x0008F510
		public void visit(_ICastExpression cast)
		{
		}

		// Token: 0x06002977 RID: 10615 RVA: 0x00091314 File Offset: 0x0008F514
		public void visit(_INewExpression typeref)
		{
		}

		// Token: 0x06002978 RID: 10616 RVA: 0x00091318 File Offset: 0x0008F518
		public void visit(_ITypeExpression typeexp)
		{
		}

		// Token: 0x06002979 RID: 10617 RVA: 0x0009131C File Offset: 0x0008F51C
		public void visit(_IAddressExpression address)
		{
			this.\u0002(address);
		}

		// Token: 0x0600297A RID: 10618 RVA: 0x00091328 File Offset: 0x0008F528
		public void visit(_IDeRefAccessExpression deref)
		{
			this.\u0002(deref);
		}

		// Token: 0x0600297B RID: 10619 RVA: 0x00091334 File Offset: 0x0008F534
		public void visit(_ICopyScopeExpression copyexp)
		{
			this.\u0002(copyexp);
		}

		// Token: 0x0600297C RID: 10620 RVA: 0x00091340 File Offset: 0x0008F540
		public void visit(_ICompiledPOU cpou)
		{
		}

		// Token: 0x0600297D RID: 10621 RVA: 0x00091344 File Offset: 0x0008F544
		public void visit(_IWhileStatement whilst)
		{
		}

		// Token: 0x0600297E RID: 10622 RVA: 0x00091348 File Offset: 0x0008F548
		public void visit(_IRepeatStatement repeat)
		{
		}

		// Token: 0x0600297F RID: 10623 RVA: 0x0009134C File Offset: 0x0008F54C
		public void visit(_IForStatement forloop)
		{
		}

		// Token: 0x06002980 RID: 10624 RVA: 0x00091350 File Offset: 0x0008F550
		public void visit(_IExitStatement exit)
		{
		}

		// Token: 0x06002981 RID: 10625 RVA: 0x00091354 File Offset: 0x0008F554
		public void visit(_IContinueStatement cont)
		{
		}

		// Token: 0x06002982 RID: 10626 RVA: 0x00091358 File Offset: 0x0008F558
		public void visit(_ISequenceStatement seq)
		{
		}

		// Token: 0x06002983 RID: 10627 RVA: 0x0009135C File Offset: 0x0008F55C
		public void visit(_IAssignmentExpression assign)
		{
			this.\u0002(assign);
		}

		// Token: 0x06002984 RID: 10628 RVA: 0x00091368 File Offset: 0x0008F568
		public void visit(_IIfStatement ifst)
		{
		}

		// Token: 0x06002985 RID: 10629 RVA: 0x0009136C File Offset: 0x0008F56C
		public void visit(_IReturnStatement returnst)
		{
		}

		// Token: 0x06002986 RID: 10630 RVA: 0x00091370 File Offset: 0x0008F570
		public void visit(_IJumpStatement gotost)
		{
		}

		// Token: 0x06002987 RID: 10631 RVA: 0x00091374 File Offset: 0x0008F574
		public void visit(_ILabelStatement label)
		{
		}

		// Token: 0x06002988 RID: 10632 RVA: 0x00091378 File Offset: 0x0008F578
		public void visit(_ICommentStatement comment)
		{
		}

		// Token: 0x06002989 RID: 10633 RVA: 0x0009137C File Offset: 0x0008F57C
		public void visit(_IPragmaStatement pragma)
		{
		}

		// Token: 0x0600298A RID: 10634 RVA: 0x00091380 File Offset: 0x0008F580
		public void visit(_IExpressionStatement expstat)
		{
		}

		// Token: 0x0600298B RID: 10635 RVA: 0x00091384 File Offset: 0x0008F584
		public void visit(_ICallExpression call)
		{
			this.\u0002(call);
		}

		// Token: 0x0600298C RID: 10636 RVA: 0x00091390 File Offset: 0x0008F590
		public void visit(_IThisExpression thisexp)
		{
			this.\u0002(thisexp);
		}

		// Token: 0x0600298D RID: 10637 RVA: 0x0009139C File Offset: 0x0008F59C
		public void visit(_IBaseExpression baseexp)
		{
			this.\u0002(baseexp);
		}

		// Token: 0x0600298E RID: 10638 RVA: 0x000913A8 File Offset: 0x0008F5A8
		public void visit(_ISystemScopeExpression systemscope)
		{
			this.\u0002(systemscope);
		}

		// Token: 0x0600298F RID: 10639 RVA: 0x000913B4 File Offset: 0x0008F5B4
		public void visit(_IEmptyStatement empty)
		{
		}

		// Token: 0x06002990 RID: 10640 RVA: 0x000913B8 File Offset: 0x0008F5B8
		public void visit(_ICaseRangeExpression caserange)
		{
		}

		// Token: 0x06002991 RID: 10641 RVA: 0x000913BC File Offset: 0x0008F5BC
		public void visit(_ICaseLabelStatement caselabel)
		{
		}

		// Token: 0x06002992 RID: 10642 RVA: 0x000913C0 File Offset: 0x0008F5C0
		public void visit(_ICaseStatement casest)
		{
		}

		// Token: 0x06002993 RID: 10643 RVA: 0x000913C4 File Offset: 0x0008F5C4
		public void visit(_IErrorExpression errorexp)
		{
		}

		// Token: 0x06002994 RID: 10644 RVA: 0x000913C8 File Offset: 0x0008F5C8
		public void visit(_IErrorStatement errorst)
		{
		}

		// Token: 0x06002995 RID: 10645 RVA: 0x000913CC File Offset: 0x0008F5CC
		public void visit(_INullExpression errorexp)
		{
		}

		// Token: 0x06002996 RID: 10646 RVA: 0x000913D0 File Offset: 0x0008F5D0
		public void visit(_INullStatement errorst)
		{
		}

		// Token: 0x06002997 RID: 10647 RVA: 0x000913D4 File Offset: 0x0008F5D4
		public void visit(_IQualifiedNameExpression qne)
		{
		}

		// Token: 0x06002998 RID: 10648 RVA: 0x000913D8 File Offset: 0x0008F5D8
		public void visit(_IVariableDeclarationStatement vds)
		{
		}

		// Token: 0x06002999 RID: 10649 RVA: 0x000913DC File Offset: 0x0008F5DC
		public void visit(_IVariableDeclarationListStatement vdls)
		{
		}

		// Token: 0x0600299A RID: 10650 RVA: 0x000913E0 File Offset: 0x0008F5E0
		public void visit(_IPOUDeclarationStatement pds)
		{
		}

		// Token: 0x0600299B RID: 10651 RVA: 0x000913E4 File Offset: 0x0008F5E4
		public void visit(_ITypeDeclarationStatement tds)
		{
		}

		// Token: 0x0600299C RID: 10652 RVA: 0x000913E8 File Offset: 0x0008F5E8
		public void visit(_IEnumDeclarationStatement eds)
		{
		}

		// Token: 0x0600299D RID: 10653 RVA: 0x000913EC File Offset: 0x0008F5EC
		public void visit(_IEnumDeclarationListStatement eds)
		{
		}

		// Token: 0x0600299E RID: 10654 RVA: 0x000913F0 File Offset: 0x0008F5F0
		public void visit(_IDefineReference defref)
		{
		}

		// Token: 0x0600299F RID: 10655 RVA: 0x000913F4 File Offset: 0x0008F5F4
		public void visit(_IVariableReference varref)
		{
		}

		// Token: 0x060029A0 RID: 10656 RVA: 0x000913F8 File Offset: 0x0008F5F8
		public void visit(_ITypeReference typeref)
		{
		}

		// Token: 0x060029A1 RID: 10657 RVA: 0x000913FC File Offset: 0x0008F5FC
		public void visit(_IPouReference pouref)
		{
		}

		// Token: 0x060029A2 RID: 10658 RVA: 0x00091400 File Offset: 0x0008F600
		public void visit(_ITaskReference taskref)
		{
		}

		// Token: 0x060029A3 RID: 10659 RVA: 0x00091404 File Offset: 0x0008F604
		public void visit(_IResourceReference resref)
		{
		}

		// Token: 0x060029A4 RID: 10660 RVA: 0x00091408 File Offset: 0x0008F608
		public void visit(_IDefinedExpression defexp)
		{
		}

		// Token: 0x060029A5 RID: 10661 RVA: 0x0009140C File Offset: 0x0008F60C
		public void visit(_IPragmaOperatorExpression popexp)
		{
		}

		// Token: 0x060029A6 RID: 10662 RVA: 0x00091410 File Offset: 0x0008F610
		public void visit(_IPragmaIfStatement pifst)
		{
		}

		// Token: 0x060029A7 RID: 10663 RVA: 0x00091414 File Offset: 0x0008F614
		public void visit(_IBreakPointStatement bpstate)
		{
		}

		// Token: 0x060029A8 RID: 10664 RVA: 0x00091418 File Offset: 0x0008F618
		public void visit(_IDefineStatement defstate)
		{
		}

		// Token: 0x060029A9 RID: 10665 RVA: 0x0009141C File Offset: 0x0008F61C
		public void visit(_IXRefExpression xref)
		{
		}

		// Token: 0x060029AA RID: 10666 RVA: 0x00091420 File Offset: 0x0008F620
		public void visit(_IHasTypeExpression hastype)
		{
		}

		// Token: 0x060029AB RID: 10667 RVA: 0x00091424 File Offset: 0x0008F624
		public void visit(_IIsEnumTypeExpression isenumtype)
		{
		}

		// Token: 0x060029AC RID: 10668 RVA: 0x00091428 File Offset: 0x0008F628
		public void visit(_IHasAttributeExpression hasattribute)
		{
		}

		// Token: 0x060029AD RID: 10669 RVA: 0x0009142C File Offset: 0x0008F62C
		public void visit(_IHasValueExpression hasvalue)
		{
		}

		// Token: 0x060029AE RID: 10670 RVA: 0x00091430 File Offset: 0x0008F630
		public void visit(_IHasConstantValueExpression hasvalue)
		{
		}

		// Token: 0x060029AF RID: 10671 RVA: 0x00091434 File Offset: 0x0008F634
		public void visit(_IPragmaAssertion assertion)
		{
		}

		// Token: 0x060029B0 RID: 10672 RVA: 0x00091438 File Offset: 0x0008F638
		public void visit(_ICompilerVersionExpression compversion)
		{
		}

		// Token: 0x060029B1 RID: 10673 RVA: 0x0009143C File Offset: 0x0008F63C
		protected void \u0002(_IExpression \u0002)
		{
			\u0002.AddError(\u0006.\u0001(MessageId.Err_BlobInitError, new object[]
			{
				\u0002.ToString()
			}), MessageId.Err_BlobInitError);
		}
	}
}
