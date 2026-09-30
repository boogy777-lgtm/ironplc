using System;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler35220.Phase5_Codegeneration.Optimization
{
	// Token: 0x0200020C RID: 524
	public class AbstractToVisitchecker : IToVisitchecker
	{
		// Token: 0x17000676 RID: 1654
		// (get) Token: 0x06002252 RID: 8786 RVA: 0x00077808 File Offset: 0x00075A08
		public virtual bool DoTraversal
		{
			get
			{
				return !this.VisitNecessary;
			}
		}

		// Token: 0x17000677 RID: 1655
		// (get) Token: 0x06002253 RID: 8787 RVA: 0x00077814 File Offset: 0x00075A14
		// (set) Token: 0x06002254 RID: 8788 RVA: 0x0007781C File Offset: 0x00075A1C
		public virtual bool VisitNecessary { get; set; }

		// Token: 0x06002255 RID: 8789 RVA: 0x00077828 File Offset: 0x00075A28
		public virtual bool ToVisit(_ICompiledPOU cpou)
		{
			return false;
		}

		// Token: 0x06002256 RID: 8790 RVA: 0x0007782C File Offset: 0x00075A2C
		public virtual bool ToVisit(_IWhileStatement whilst)
		{
			return false;
		}

		// Token: 0x06002257 RID: 8791 RVA: 0x00077830 File Offset: 0x00075A30
		public virtual bool ToVisit(_IRepeatStatement repeat)
		{
			return false;
		}

		// Token: 0x06002258 RID: 8792 RVA: 0x00077834 File Offset: 0x00075A34
		public virtual bool ToVisit(_IForStatement forloop)
		{
			return false;
		}

		// Token: 0x06002259 RID: 8793 RVA: 0x00077838 File Offset: 0x00075A38
		public virtual bool ToVisit(_IExitStatement exit)
		{
			return false;
		}

		// Token: 0x0600225A RID: 8794 RVA: 0x0007783C File Offset: 0x00075A3C
		public virtual bool ToVisit(_IContinueStatement cont)
		{
			return false;
		}

		// Token: 0x0600225B RID: 8795 RVA: 0x00077840 File Offset: 0x00075A40
		public virtual bool ToVisit(_ISequenceStatement seq)
		{
			return false;
		}

		// Token: 0x0600225C RID: 8796 RVA: 0x00077844 File Offset: 0x00075A44
		public virtual bool ToVisit(_IAssignmentExpression assign)
		{
			return false;
		}

		// Token: 0x0600225D RID: 8797 RVA: 0x00077848 File Offset: 0x00075A48
		public virtual bool ToVisit(_IIfStatement ifst)
		{
			return false;
		}

		// Token: 0x0600225E RID: 8798 RVA: 0x0007784C File Offset: 0x00075A4C
		public virtual bool ToVisit(_IReturnStatement returnst)
		{
			return false;
		}

		// Token: 0x0600225F RID: 8799 RVA: 0x00077850 File Offset: 0x00075A50
		public virtual bool ToVisit(_IJumpStatement gotost)
		{
			return false;
		}

		// Token: 0x06002260 RID: 8800 RVA: 0x00077854 File Offset: 0x00075A54
		public virtual bool ToVisit(_ILabelStatement label)
		{
			return false;
		}

		// Token: 0x06002261 RID: 8801 RVA: 0x00077858 File Offset: 0x00075A58
		public virtual bool ToVisit(_ICommentStatement comment)
		{
			return false;
		}

		// Token: 0x06002262 RID: 8802 RVA: 0x0007785C File Offset: 0x00075A5C
		public virtual bool ToVisit(_IPragmaStatement pragma)
		{
			return false;
		}

		// Token: 0x06002263 RID: 8803 RVA: 0x00077860 File Offset: 0x00075A60
		public virtual bool ToVisit(_IExpressionStatement expstat)
		{
			return false;
		}

		// Token: 0x06002264 RID: 8804 RVA: 0x00077864 File Offset: 0x00075A64
		public virtual bool ToVisit(_ITryCatchStatement trycatch)
		{
			return false;
		}

		// Token: 0x06002265 RID: 8805 RVA: 0x00077868 File Offset: 0x00075A68
		public virtual bool ToVisit(_ICallExpression call)
		{
			return false;
		}

		// Token: 0x06002266 RID: 8806 RVA: 0x0007786C File Offset: 0x00075A6C
		public virtual bool ToVisit(_IOperatorExpression op)
		{
			return false;
		}

		// Token: 0x06002267 RID: 8807 RVA: 0x00077870 File Offset: 0x00075A70
		public virtual bool ToVisit(_IConversionExpression conv)
		{
			return false;
		}

		// Token: 0x06002268 RID: 8808 RVA: 0x00077874 File Offset: 0x00075A74
		public virtual bool ToVisit(_IThisExpression thisexp)
		{
			return false;
		}

		// Token: 0x06002269 RID: 8809 RVA: 0x00077878 File Offset: 0x00075A78
		public virtual bool ToVisit(_IBaseExpression baseexp)
		{
			return false;
		}

		// Token: 0x0600226A RID: 8810 RVA: 0x0007787C File Offset: 0x00075A7C
		public virtual bool ToVisit(_ILiteralExpression literal)
		{
			return false;
		}

		// Token: 0x0600226B RID: 8811 RVA: 0x00077880 File Offset: 0x00075A80
		public virtual bool ToVisit(_IAddressExpression address)
		{
			return false;
		}

		// Token: 0x0600226C RID: 8812 RVA: 0x00077884 File Offset: 0x00075A84
		public virtual bool ToVisit(_IVariableExpression variable)
		{
			return false;
		}

		// Token: 0x0600226D RID: 8813 RVA: 0x00077888 File Offset: 0x00075A88
		public virtual bool ToVisit(_IIndexAccessExpression indexaccess)
		{
			return false;
		}

		// Token: 0x0600226E RID: 8814 RVA: 0x0007788C File Offset: 0x00075A8C
		public virtual bool ToVisit(_ICompoAccessExpression compo)
		{
			return false;
		}

		// Token: 0x0600226F RID: 8815 RVA: 0x00077890 File Offset: 0x00075A90
		public virtual bool ToVisit(_IDeRefAccessExpression deref)
		{
			return false;
		}

		// Token: 0x06002270 RID: 8816 RVA: 0x00077894 File Offset: 0x00075A94
		public virtual bool ToVisit(_ICopyScopeExpression copyexp)
		{
			return false;
		}

		// Token: 0x06002271 RID: 8817 RVA: 0x00077898 File Offset: 0x00075A98
		public virtual bool ToVisit(_IGlobalScopeExpression globexp)
		{
			return false;
		}

		// Token: 0x06002272 RID: 8818 RVA: 0x0007789C File Offset: 0x00075A9C
		public virtual bool ToVisit(_ISystemScopeExpression systemscope)
		{
			return false;
		}

		// Token: 0x06002273 RID: 8819 RVA: 0x000778A0 File Offset: 0x00075AA0
		public virtual bool ToVisit(_IEmptyStatement empty)
		{
			return false;
		}

		// Token: 0x06002274 RID: 8820 RVA: 0x000778A4 File Offset: 0x00075AA4
		public virtual bool ToVisit(_ICaseRangeExpression caserange)
		{
			return false;
		}

		// Token: 0x06002275 RID: 8821 RVA: 0x000778A8 File Offset: 0x00075AA8
		public virtual bool ToVisit(_ICaseLabelStatement caselabel)
		{
			return false;
		}

		// Token: 0x06002276 RID: 8822 RVA: 0x000778AC File Offset: 0x00075AAC
		public virtual bool ToVisit(_ICaseStatement casest)
		{
			return false;
		}

		// Token: 0x06002277 RID: 8823 RVA: 0x000778B0 File Offset: 0x00075AB0
		public virtual bool ToVisit(_IErrorExpression errorexp)
		{
			return false;
		}

		// Token: 0x06002278 RID: 8824 RVA: 0x000778B4 File Offset: 0x00075AB4
		public virtual bool ToVisit(_IErrorStatement errorst)
		{
			return false;
		}

		// Token: 0x06002279 RID: 8825 RVA: 0x000778B8 File Offset: 0x00075AB8
		public virtual bool ToVisit(_INullExpression errorexp)
		{
			return false;
		}

		// Token: 0x0600227A RID: 8826 RVA: 0x000778BC File Offset: 0x00075ABC
		public virtual bool ToVisit(_INullStatement errorst)
		{
			return false;
		}

		// Token: 0x0600227B RID: 8827 RVA: 0x000778C0 File Offset: 0x00075AC0
		public virtual bool ToVisit(_IQualifiedNameExpression qne)
		{
			return false;
		}

		// Token: 0x0600227C RID: 8828 RVA: 0x000778C4 File Offset: 0x00075AC4
		public virtual bool ToVisit(_IVariableDeclarationStatement vds)
		{
			return false;
		}

		// Token: 0x0600227D RID: 8829 RVA: 0x000778C8 File Offset: 0x00075AC8
		public virtual bool ToVisit(_IVariableDeclarationListStatement vdls)
		{
			return false;
		}

		// Token: 0x0600227E RID: 8830 RVA: 0x000778CC File Offset: 0x00075ACC
		public virtual bool ToVisit(_IPOUDeclarationStatement pds)
		{
			return false;
		}

		// Token: 0x0600227F RID: 8831 RVA: 0x000778D0 File Offset: 0x00075AD0
		public virtual bool ToVisit(_ITypeDeclarationStatement tds)
		{
			return false;
		}

		// Token: 0x06002280 RID: 8832 RVA: 0x000778D4 File Offset: 0x00075AD4
		public virtual bool ToVisit(_IEnumDeclarationStatement eds)
		{
			return false;
		}

		// Token: 0x06002281 RID: 8833 RVA: 0x000778D8 File Offset: 0x00075AD8
		public virtual bool ToVisit(_IEnumDeclarationListStatement eds)
		{
			return false;
		}

		// Token: 0x06002282 RID: 8834 RVA: 0x000778DC File Offset: 0x00075ADC
		public virtual bool ToVisit(_IMultipleIndexInitialization errorst)
		{
			return false;
		}

		// Token: 0x06002283 RID: 8835 RVA: 0x000778E0 File Offset: 0x00075AE0
		public virtual bool ToVisit(_IArrayInitialization arrayInitialization)
		{
			return false;
		}

		// Token: 0x06002284 RID: 8836 RVA: 0x000778E4 File Offset: 0x00075AE4
		public virtual bool ToVisit(_IStructureInitialization structureInitialization)
		{
			return false;
		}

		// Token: 0x06002285 RID: 8837 RVA: 0x000778E8 File Offset: 0x00075AE8
		public virtual bool ToVisit(_IDefineReference defref)
		{
			return false;
		}

		// Token: 0x06002286 RID: 8838 RVA: 0x000778EC File Offset: 0x00075AEC
		public virtual bool ToVisit(_IVariableReference varref)
		{
			return false;
		}

		// Token: 0x06002287 RID: 8839 RVA: 0x000778F0 File Offset: 0x00075AF0
		public virtual bool ToVisit(_ITypeReference typeref)
		{
			return false;
		}

		// Token: 0x06002288 RID: 8840 RVA: 0x000778F4 File Offset: 0x00075AF4
		public virtual bool ToVisit(_IPouReference pouref)
		{
			return false;
		}

		// Token: 0x06002289 RID: 8841 RVA: 0x000778F8 File Offset: 0x00075AF8
		public virtual bool ToVisit(_ITaskReference taskref)
		{
			return false;
		}

		// Token: 0x0600228A RID: 8842 RVA: 0x000778FC File Offset: 0x00075AFC
		public virtual bool ToVisit(_IResourceReference resref)
		{
			return false;
		}

		// Token: 0x0600228B RID: 8843 RVA: 0x00077900 File Offset: 0x00075B00
		public virtual bool ToVisit(_IDefinedExpression defexp)
		{
			return false;
		}

		// Token: 0x0600228C RID: 8844 RVA: 0x00077904 File Offset: 0x00075B04
		public virtual bool ToVisit(_IPragmaOperatorExpression popexp)
		{
			return false;
		}

		// Token: 0x0600228D RID: 8845 RVA: 0x00077908 File Offset: 0x00075B08
		public virtual bool ToVisit(_IPragmaIfStatement pifst)
		{
			return false;
		}

		// Token: 0x0600228E RID: 8846 RVA: 0x0007790C File Offset: 0x00075B0C
		public virtual bool ToVisit(_IBreakPointStatement bpstate)
		{
			return false;
		}

		// Token: 0x0600228F RID: 8847 RVA: 0x00077910 File Offset: 0x00075B10
		public virtual bool ToVisit(_IDefineStatement defstate)
		{
			return false;
		}

		// Token: 0x06002290 RID: 8848 RVA: 0x00077914 File Offset: 0x00075B14
		public virtual bool ToVisit(_IXRefExpression xref)
		{
			return false;
		}

		// Token: 0x06002291 RID: 8849 RVA: 0x00077918 File Offset: 0x00075B18
		public virtual bool ToVisit(_IHasTypeExpression hastype)
		{
			return false;
		}

		// Token: 0x06002292 RID: 8850 RVA: 0x0007791C File Offset: 0x00075B1C
		public virtual bool ToVisit(_IIsEnumTypeExpression isenumtype)
		{
			return false;
		}

		// Token: 0x06002293 RID: 8851 RVA: 0x00077920 File Offset: 0x00075B20
		public virtual bool ToVisit(_IHasAttributeExpression hasattribute)
		{
			return false;
		}

		// Token: 0x06002294 RID: 8852 RVA: 0x00077924 File Offset: 0x00075B24
		public virtual bool ToVisit(_IHasValueExpression hasvalue)
		{
			return false;
		}

		// Token: 0x06002295 RID: 8853 RVA: 0x00077928 File Offset: 0x00075B28
		public virtual bool ToVisit(_IHasConstantValueExpression hasvalue)
		{
			return false;
		}

		// Token: 0x06002296 RID: 8854 RVA: 0x0007792C File Offset: 0x00075B2C
		public virtual bool ToVisit(_IPragmaAssertion assertion)
		{
			return false;
		}

		// Token: 0x06002297 RID: 8855 RVA: 0x00077930 File Offset: 0x00075B30
		public virtual bool ToVisit(_ICompilerVersionExpression compiversionexp)
		{
			return false;
		}

		// Token: 0x06002298 RID: 8856 RVA: 0x00077934 File Offset: 0x00075B34
		public virtual bool ToVisit(_ICastExpression castexp)
		{
			return false;
		}

		// Token: 0x06002299 RID: 8857 RVA: 0x00077938 File Offset: 0x00075B38
		public virtual bool ToVisit(_INewExpression typeref)
		{
			return false;
		}

		// Token: 0x0600229A RID: 8858 RVA: 0x0007793C File Offset: 0x00075B3C
		public virtual bool ToVisit(_ITypeExpression typeexp)
		{
			return false;
		}

		// Token: 0x0600229B RID: 8859 RVA: 0x00077940 File Offset: 0x00075B40
		public virtual bool ToVisit(_IPoolScopeExpression poolscope)
		{
			return false;
		}

		// Token: 0x0600229C RID: 8860 RVA: 0x00077944 File Offset: 0x00075B44
		public virtual bool ToVisit(_ICurrentTaskExpression currentTask)
		{
			return false;
		}

		// Token: 0x0600229D RID: 8861 RVA: 0x00077948 File Offset: 0x00075B48
		public virtual bool ToVisit(_IRuntimeVersionExpression runtimeversionexp)
		{
			return false;
		}

		// Token: 0x0600229E RID: 8862 RVA: 0x0007794C File Offset: 0x00075B4C
		public virtual bool ToVisit(_INamespaceAccessExpression namespaceaccess)
		{
			return false;
		}

		// Token: 0x0600229F RID: 8863 RVA: 0x00077950 File Offset: 0x00075B50
		public virtual bool ToVisit(_IHasConstantTypeExpression hasConstantTypeExpression)
		{
			return false;
		}

		// Token: 0x060022A0 RID: 8864 RVA: 0x00077954 File Offset: 0x00075B54
		public virtual bool ToVisit(_IPartialAccessExpression partialAccessExpression)
		{
			return false;
		}

		// Token: 0x060022A1 RID: 8865 RVA: 0x00077958 File Offset: 0x00075B58
		public virtual bool ToVisit(_IProjectDefinedExpression projectDefinedExpression)
		{
			return false;
		}

		// Token: 0x04000619 RID: 1561
		[CompilerGenerated]
		private bool \u0001;
	}
}
