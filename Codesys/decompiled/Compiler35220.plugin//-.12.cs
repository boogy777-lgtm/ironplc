using System;
using System.Diagnostics.CodeAnalysis;
using _3S.CoDeSys.Compiler35220.Phase5_Codegeneration;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0081
{
	// Token: 0x02000295 RID: 661
	[ExcludeFromCodeCoverage]
	internal class \u0011
	{
		// Token: 0x060029CD RID: 10701 RVA: 0x00091DE4 File Offset: 0x0008FFE4
		internal \u0011(_IVariable \u001A\u0002)
		{
			this.\u0001 = \u001A\u0002;
		}

		// Token: 0x060029CE RID: 10702 RVA: 0x00091DF4 File Offset: 0x0008FFF4
		public void visit(_INewExpression typeref)
		{
		}

		// Token: 0x060029CF RID: 10703 RVA: 0x00091DF8 File Offset: 0x0008FFF8
		public void visit(_ITypeExpression typeexp)
		{
		}

		// Token: 0x060029D0 RID: 10704 RVA: 0x00091DFC File Offset: 0x0008FFFC
		public void visit(_ICurrentTaskExpression currentTask)
		{
		}

		// Token: 0x060029D1 RID: 10705 RVA: 0x00091E00 File Offset: 0x00090000
		public void visit(_IRuntimeVersionExpression runtimeversionexp)
		{
		}

		// Token: 0x060029D2 RID: 10706 RVA: 0x00091E04 File Offset: 0x00090004
		public void visit(_IAddressExpression address)
		{
			throw new BlobInitException(address, this.\u0001);
		}

		// Token: 0x060029D3 RID: 10707 RVA: 0x00091E14 File Offset: 0x00090014
		public void visit(_IDeRefAccessExpression deref)
		{
			throw new BlobInitException(deref, this.\u0001);
		}

		// Token: 0x060029D4 RID: 10708 RVA: 0x00091E24 File Offset: 0x00090024
		public void visit(_ICopyScopeExpression copyexp)
		{
			throw new BlobInitException(copyexp, this.\u0001);
		}

		// Token: 0x060029D5 RID: 10709 RVA: 0x00091E34 File Offset: 0x00090034
		public void visit(_ICompiledPOU cpou)
		{
			throw new BlobInitException(null, this.\u0001);
		}

		// Token: 0x060029D6 RID: 10710 RVA: 0x00091E44 File Offset: 0x00090044
		public void visit(_IWhileStatement whilst)
		{
			throw new BlobInitException(whilst, this.\u0001);
		}

		// Token: 0x060029D7 RID: 10711 RVA: 0x00091E54 File Offset: 0x00090054
		public void visit(_IRepeatStatement repeat)
		{
			throw new BlobInitException(repeat, this.\u0001);
		}

		// Token: 0x060029D8 RID: 10712 RVA: 0x00091E64 File Offset: 0x00090064
		public void visit(_IForStatement forloop)
		{
			throw new BlobInitException(forloop, this.\u0001);
		}

		// Token: 0x060029D9 RID: 10713 RVA: 0x00091E74 File Offset: 0x00090074
		public void visit(_IExitStatement exit)
		{
			throw new BlobInitException(exit, this.\u0001);
		}

		// Token: 0x060029DA RID: 10714 RVA: 0x00091E84 File Offset: 0x00090084
		public void visit(_IContinueStatement cont)
		{
			throw new BlobInitException(cont, this.\u0001);
		}

		// Token: 0x060029DB RID: 10715 RVA: 0x00091E94 File Offset: 0x00090094
		public void visit(_ISequenceStatement seq)
		{
			throw new BlobInitException(seq, this.\u0001);
		}

		// Token: 0x060029DC RID: 10716 RVA: 0x00091EA4 File Offset: 0x000900A4
		public void visit(_IAssignmentExpression assign)
		{
			throw new BlobInitException(assign, this.\u0001);
		}

		// Token: 0x060029DD RID: 10717 RVA: 0x00091EB4 File Offset: 0x000900B4
		public void visit(_IIfStatement ifst)
		{
			throw new BlobInitException(ifst, this.\u0001);
		}

		// Token: 0x060029DE RID: 10718 RVA: 0x00091EC4 File Offset: 0x000900C4
		public void visit(_IReturnStatement returnst)
		{
			throw new BlobInitException(returnst, this.\u0001);
		}

		// Token: 0x060029DF RID: 10719 RVA: 0x00091ED4 File Offset: 0x000900D4
		public void visit(_IJumpStatement gotost)
		{
			throw new BlobInitException(gotost, this.\u0001);
		}

		// Token: 0x060029E0 RID: 10720 RVA: 0x00091EE4 File Offset: 0x000900E4
		public void visit(_ILabelStatement label)
		{
			throw new BlobInitException(label, this.\u0001);
		}

		// Token: 0x060029E1 RID: 10721 RVA: 0x00091EF4 File Offset: 0x000900F4
		public void visit(_ICommentStatement comment)
		{
			throw new BlobInitException(comment, this.\u0001);
		}

		// Token: 0x060029E2 RID: 10722 RVA: 0x00091F04 File Offset: 0x00090104
		public void visit(_IPragmaStatement pragma)
		{
			throw new BlobInitException(pragma, this.\u0001);
		}

		// Token: 0x060029E3 RID: 10723 RVA: 0x00091F14 File Offset: 0x00090114
		public void visit(_IExpressionStatement expstat)
		{
			throw new BlobInitException(expstat, this.\u0001);
		}

		// Token: 0x060029E4 RID: 10724 RVA: 0x00091F24 File Offset: 0x00090124
		public void visit(_ICallExpression call)
		{
			throw new BlobInitException(call, this.\u0001);
		}

		// Token: 0x060029E5 RID: 10725 RVA: 0x00091F34 File Offset: 0x00090134
		public void visit(_IThisExpression thisexp)
		{
			throw new BlobInitException(thisexp, this.\u0001);
		}

		// Token: 0x060029E6 RID: 10726 RVA: 0x00091F44 File Offset: 0x00090144
		public void visit(_IBaseExpression baseexp)
		{
			throw new BlobInitException(baseexp, this.\u0001);
		}

		// Token: 0x060029E7 RID: 10727 RVA: 0x00091F54 File Offset: 0x00090154
		public void visit(_ISystemScopeExpression systemscope)
		{
			throw new BlobInitException(systemscope, this.\u0001);
		}

		// Token: 0x060029E8 RID: 10728 RVA: 0x00091F64 File Offset: 0x00090164
		public void visit(_IEmptyStatement empty)
		{
			throw new BlobInitException(empty, this.\u0001);
		}

		// Token: 0x060029E9 RID: 10729 RVA: 0x00091F74 File Offset: 0x00090174
		public void visit(_ICaseRangeExpression caserange)
		{
			throw new BlobInitException(caserange, this.\u0001);
		}

		// Token: 0x060029EA RID: 10730 RVA: 0x00091F84 File Offset: 0x00090184
		public void visit(_ICaseLabelStatement caselabel)
		{
			throw new BlobInitException(caselabel, this.\u0001);
		}

		// Token: 0x060029EB RID: 10731 RVA: 0x00091F94 File Offset: 0x00090194
		public void visit(_ICaseStatement casest)
		{
			throw new BlobInitException(casest, this.\u0001);
		}

		// Token: 0x060029EC RID: 10732 RVA: 0x00091FA4 File Offset: 0x000901A4
		public void visit(_IErrorExpression errorexp)
		{
			throw new BlobInitException(errorexp, this.\u0001);
		}

		// Token: 0x060029ED RID: 10733 RVA: 0x00091FB4 File Offset: 0x000901B4
		public void visit(_IErrorStatement errorst)
		{
			throw new BlobInitException(errorst, this.\u0001);
		}

		// Token: 0x060029EE RID: 10734 RVA: 0x00091FC4 File Offset: 0x000901C4
		public void visit(_INullExpression errorexp)
		{
			throw new BlobInitException(errorexp, this.\u0001);
		}

		// Token: 0x060029EF RID: 10735 RVA: 0x00091FD4 File Offset: 0x000901D4
		public void visit(_INullStatement errorst)
		{
			throw new BlobInitException(errorst, this.\u0001);
		}

		// Token: 0x060029F0 RID: 10736 RVA: 0x00091FE4 File Offset: 0x000901E4
		public void visit(_IQualifiedNameExpression qne)
		{
			throw new BlobInitException(qne, this.\u0001);
		}

		// Token: 0x060029F1 RID: 10737 RVA: 0x00091FF4 File Offset: 0x000901F4
		public void visit(_IVariableDeclarationStatement vds)
		{
			throw new BlobInitException(vds, this.\u0001);
		}

		// Token: 0x060029F2 RID: 10738 RVA: 0x00092004 File Offset: 0x00090204
		public void visit(_IVariableDeclarationListStatement vdls)
		{
			throw new BlobInitException(vdls, this.\u0001);
		}

		// Token: 0x060029F3 RID: 10739 RVA: 0x00092014 File Offset: 0x00090214
		public void visit(_IPOUDeclarationStatement pds)
		{
			throw new BlobInitException(pds, this.\u0001);
		}

		// Token: 0x060029F4 RID: 10740 RVA: 0x00092024 File Offset: 0x00090224
		public void visit(_ITypeDeclarationStatement tds)
		{
			throw new BlobInitException(tds, this.\u0001);
		}

		// Token: 0x060029F5 RID: 10741 RVA: 0x00092034 File Offset: 0x00090234
		public void visit(_IEnumDeclarationStatement eds)
		{
			throw new BlobInitException(eds, this.\u0001);
		}

		// Token: 0x060029F6 RID: 10742 RVA: 0x00092044 File Offset: 0x00090244
		public void visit(_IEnumDeclarationListStatement eds)
		{
			throw new BlobInitException(eds, this.\u0001);
		}

		// Token: 0x060029F7 RID: 10743 RVA: 0x00092054 File Offset: 0x00090254
		public void visit(_IDefineReference defref)
		{
			throw new BlobInitException(defref, this.\u0001);
		}

		// Token: 0x060029F8 RID: 10744 RVA: 0x00092064 File Offset: 0x00090264
		public void visit(_IVariableReference varref)
		{
			throw new BlobInitException(varref, this.\u0001);
		}

		// Token: 0x060029F9 RID: 10745 RVA: 0x00092074 File Offset: 0x00090274
		public void visit(_ITypeReference typeref)
		{
			throw new BlobInitException(typeref, this.\u0001);
		}

		// Token: 0x060029FA RID: 10746 RVA: 0x00092084 File Offset: 0x00090284
		public void visit(_IPouReference pouref)
		{
			throw new BlobInitException(pouref, this.\u0001);
		}

		// Token: 0x060029FB RID: 10747 RVA: 0x00092094 File Offset: 0x00090294
		public void visit(_ITaskReference taskref)
		{
			throw new BlobInitException(taskref, this.\u0001);
		}

		// Token: 0x060029FC RID: 10748 RVA: 0x000920A4 File Offset: 0x000902A4
		public void visit(_IResourceReference resref)
		{
			throw new BlobInitException(resref, this.\u0001);
		}

		// Token: 0x060029FD RID: 10749 RVA: 0x000920B4 File Offset: 0x000902B4
		public void visit(_IDefinedExpression defexp)
		{
			throw new BlobInitException(defexp, this.\u0001);
		}

		// Token: 0x060029FE RID: 10750 RVA: 0x000920C4 File Offset: 0x000902C4
		public void visit(_IPragmaOperatorExpression popexp)
		{
			throw new BlobInitException(popexp, this.\u0001);
		}

		// Token: 0x060029FF RID: 10751 RVA: 0x000920D4 File Offset: 0x000902D4
		public void visit(_IPragmaIfStatement pifst)
		{
			throw new BlobInitException(pifst, this.\u0001);
		}

		// Token: 0x06002A00 RID: 10752 RVA: 0x000920E4 File Offset: 0x000902E4
		public void visit(_IBreakPointStatement bpstate)
		{
			throw new BlobInitException(bpstate, this.\u0001);
		}

		// Token: 0x06002A01 RID: 10753 RVA: 0x000920F4 File Offset: 0x000902F4
		public void visit(_IDefineStatement defstate)
		{
			throw new BlobInitException(defstate, this.\u0001);
		}

		// Token: 0x06002A02 RID: 10754 RVA: 0x00092104 File Offset: 0x00090304
		public void visit(_IXRefExpression xref)
		{
			throw new BlobInitException(xref, this.\u0001);
		}

		// Token: 0x06002A03 RID: 10755 RVA: 0x00092114 File Offset: 0x00090314
		public void visit(_IHasTypeExpression hastype)
		{
			throw new BlobInitException(hastype, this.\u0001);
		}

		// Token: 0x06002A04 RID: 10756 RVA: 0x00092124 File Offset: 0x00090324
		public void visit(_IIsEnumTypeExpression isenumtype)
		{
			throw new BlobInitException(isenumtype, this.\u0001);
		}

		// Token: 0x06002A05 RID: 10757 RVA: 0x00092134 File Offset: 0x00090334
		public void visit(_IHasAttributeExpression hasattribute)
		{
			throw new BlobInitException(hasattribute, this.\u0001);
		}

		// Token: 0x06002A06 RID: 10758 RVA: 0x00092144 File Offset: 0x00090344
		public void visit(_IHasValueExpression hasvalue)
		{
			throw new BlobInitException(hasvalue, this.\u0001);
		}

		// Token: 0x06002A07 RID: 10759 RVA: 0x00092154 File Offset: 0x00090354
		public void visit(_IHasConstantValueExpression hasvalue)
		{
			throw new BlobInitException(hasvalue, this.\u0001);
		}

		// Token: 0x06002A08 RID: 10760 RVA: 0x00092164 File Offset: 0x00090364
		public void visit(_IPragmaAssertion assertion)
		{
			throw new BlobInitException(assertion, this.\u0001);
		}

		// Token: 0x06002A09 RID: 10761 RVA: 0x00092174 File Offset: 0x00090374
		public void visit(_ICompilerVersionExpression compversion)
		{
			throw new BlobInitException(compversion, this.\u0001);
		}

		// Token: 0x040007AC RID: 1964
		protected readonly _IVariable \u0001;
	}
}
