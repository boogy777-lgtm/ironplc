using System;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler35220.Phase5_Codegeneration.Optimization
{
	// Token: 0x0200020D RID: 525
	public interface IToVisitchecker
	{
		// Token: 0x17000678 RID: 1656
		// (get) Token: 0x060022A3 RID: 8867
		bool DoTraversal { get; }

		// Token: 0x17000679 RID: 1657
		// (get) Token: 0x060022A4 RID: 8868
		// (set) Token: 0x060022A5 RID: 8869
		bool VisitNecessary { get; set; }

		// Token: 0x060022A6 RID: 8870
		bool ToVisit(_ICompiledPOU cpou);

		// Token: 0x060022A7 RID: 8871
		bool ToVisit(_IWhileStatement whilst);

		// Token: 0x060022A8 RID: 8872
		bool ToVisit(_IRepeatStatement repeat);

		// Token: 0x060022A9 RID: 8873
		bool ToVisit(_IForStatement forloop);

		// Token: 0x060022AA RID: 8874
		bool ToVisit(_IExitStatement exit);

		// Token: 0x060022AB RID: 8875
		bool ToVisit(_IContinueStatement cont);

		// Token: 0x060022AC RID: 8876
		bool ToVisit(_ISequenceStatement seq);

		// Token: 0x060022AD RID: 8877
		bool ToVisit(_IAssignmentExpression assign);

		// Token: 0x060022AE RID: 8878
		bool ToVisit(_IIfStatement ifst);

		// Token: 0x060022AF RID: 8879
		bool ToVisit(_IReturnStatement returnst);

		// Token: 0x060022B0 RID: 8880
		bool ToVisit(_IJumpStatement gotost);

		// Token: 0x060022B1 RID: 8881
		bool ToVisit(_ILabelStatement label);

		// Token: 0x060022B2 RID: 8882
		bool ToVisit(_ICommentStatement comment);

		// Token: 0x060022B3 RID: 8883
		bool ToVisit(_IPragmaStatement pragma);

		// Token: 0x060022B4 RID: 8884
		bool ToVisit(_IExpressionStatement expstat);

		// Token: 0x060022B5 RID: 8885
		bool ToVisit(_ITryCatchStatement trycatch);

		// Token: 0x060022B6 RID: 8886
		bool ToVisit(_ICallExpression call);

		// Token: 0x060022B7 RID: 8887
		bool ToVisit(_IOperatorExpression op);

		// Token: 0x060022B8 RID: 8888
		bool ToVisit(_IConversionExpression conv);

		// Token: 0x060022B9 RID: 8889
		bool ToVisit(_IThisExpression thisexp);

		// Token: 0x060022BA RID: 8890
		bool ToVisit(_IBaseExpression baseexp);

		// Token: 0x060022BB RID: 8891
		bool ToVisit(_ILiteralExpression literal);

		// Token: 0x060022BC RID: 8892
		bool ToVisit(_IAddressExpression address);

		// Token: 0x060022BD RID: 8893
		bool ToVisit(_IVariableExpression variable);

		// Token: 0x060022BE RID: 8894
		bool ToVisit(_IIndexAccessExpression indexaccess);

		// Token: 0x060022BF RID: 8895
		bool ToVisit(_ICompoAccessExpression compo);

		// Token: 0x060022C0 RID: 8896
		bool ToVisit(_IDeRefAccessExpression deref);

		// Token: 0x060022C1 RID: 8897
		bool ToVisit(_ICopyScopeExpression copyexp);

		// Token: 0x060022C2 RID: 8898
		bool ToVisit(_IGlobalScopeExpression globexp);

		// Token: 0x060022C3 RID: 8899
		bool ToVisit(_ISystemScopeExpression systemscope);

		// Token: 0x060022C4 RID: 8900
		bool ToVisit(_IEmptyStatement empty);

		// Token: 0x060022C5 RID: 8901
		bool ToVisit(_ICaseRangeExpression caserange);

		// Token: 0x060022C6 RID: 8902
		bool ToVisit(_ICaseLabelStatement caselabel);

		// Token: 0x060022C7 RID: 8903
		bool ToVisit(_ICaseStatement casest);

		// Token: 0x060022C8 RID: 8904
		bool ToVisit(_IErrorExpression errorexp);

		// Token: 0x060022C9 RID: 8905
		bool ToVisit(_IErrorStatement errorst);

		// Token: 0x060022CA RID: 8906
		bool ToVisit(_INullExpression errorexp);

		// Token: 0x060022CB RID: 8907
		bool ToVisit(_INullStatement errorst);

		// Token: 0x060022CC RID: 8908
		bool ToVisit(_IQualifiedNameExpression qne);

		// Token: 0x060022CD RID: 8909
		bool ToVisit(_IVariableDeclarationStatement vds);

		// Token: 0x060022CE RID: 8910
		bool ToVisit(_IVariableDeclarationListStatement vdls);

		// Token: 0x060022CF RID: 8911
		bool ToVisit(_IPOUDeclarationStatement pds);

		// Token: 0x060022D0 RID: 8912
		bool ToVisit(_ITypeDeclarationStatement tds);

		// Token: 0x060022D1 RID: 8913
		bool ToVisit(_IEnumDeclarationStatement eds);

		// Token: 0x060022D2 RID: 8914
		bool ToVisit(_IEnumDeclarationListStatement eds);

		// Token: 0x060022D3 RID: 8915
		bool ToVisit(_IMultipleIndexInitialization errorst);

		// Token: 0x060022D4 RID: 8916
		bool ToVisit(_IArrayInitialization arrayInitialization);

		// Token: 0x060022D5 RID: 8917
		bool ToVisit(_IStructureInitialization structureInitialization);

		// Token: 0x060022D6 RID: 8918
		bool ToVisit(_IDefineReference defref);

		// Token: 0x060022D7 RID: 8919
		bool ToVisit(_IVariableReference varref);

		// Token: 0x060022D8 RID: 8920
		bool ToVisit(_ITypeReference typeref);

		// Token: 0x060022D9 RID: 8921
		bool ToVisit(_IPouReference pouref);

		// Token: 0x060022DA RID: 8922
		bool ToVisit(_ITaskReference taskref);

		// Token: 0x060022DB RID: 8923
		bool ToVisit(_IResourceReference resref);

		// Token: 0x060022DC RID: 8924
		bool ToVisit(_IDefinedExpression defexp);

		// Token: 0x060022DD RID: 8925
		bool ToVisit(_IPragmaOperatorExpression popexp);

		// Token: 0x060022DE RID: 8926
		bool ToVisit(_IPragmaIfStatement pifst);

		// Token: 0x060022DF RID: 8927
		bool ToVisit(_IBreakPointStatement bpstate);

		// Token: 0x060022E0 RID: 8928
		bool ToVisit(_IDefineStatement defstate);

		// Token: 0x060022E1 RID: 8929
		bool ToVisit(_IXRefExpression xref);

		// Token: 0x060022E2 RID: 8930
		bool ToVisit(_IHasTypeExpression hastype);

		// Token: 0x060022E3 RID: 8931
		bool ToVisit(_IIsEnumTypeExpression isenumtype);

		// Token: 0x060022E4 RID: 8932
		bool ToVisit(_IHasAttributeExpression hasattribute);

		// Token: 0x060022E5 RID: 8933
		bool ToVisit(_IHasValueExpression hasvalue);

		// Token: 0x060022E6 RID: 8934
		bool ToVisit(_IHasConstantValueExpression hasvalue);

		// Token: 0x060022E7 RID: 8935
		bool ToVisit(_IPragmaAssertion assertion);

		// Token: 0x060022E8 RID: 8936
		bool ToVisit(_ICompilerVersionExpression compiversionexp);

		// Token: 0x060022E9 RID: 8937
		bool ToVisit(_ICastExpression castexp);

		// Token: 0x060022EA RID: 8938
		bool ToVisit(_INewExpression typeref);

		// Token: 0x060022EB RID: 8939
		bool ToVisit(_ITypeExpression typeexp);

		// Token: 0x060022EC RID: 8940
		bool ToVisit(_IPoolScopeExpression poolscope);

		// Token: 0x060022ED RID: 8941
		bool ToVisit(_ICurrentTaskExpression currentTask);

		// Token: 0x060022EE RID: 8942
		bool ToVisit(_IRuntimeVersionExpression runtimeversionexp);

		// Token: 0x060022EF RID: 8943
		bool ToVisit(_INamespaceAccessExpression namespaceaccess);

		// Token: 0x060022F0 RID: 8944
		bool ToVisit(_IHasConstantTypeExpression hasConstantTypeExpression);

		// Token: 0x060022F1 RID: 8945
		bool ToVisit(_IPartialAccessExpression partialAccessExpression);

		// Token: 0x060022F2 RID: 8946
		bool ToVisit(_IProjectDefinedExpression projectDefinedExpression);
	}
}
