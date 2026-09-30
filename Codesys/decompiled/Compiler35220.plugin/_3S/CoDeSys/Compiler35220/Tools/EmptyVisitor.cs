using System;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler35220.Tools
{
	// Token: 0x0200006A RID: 106
	public abstract class EmptyVisitor : IExprementVisitorNoTraversion
	{
		// Token: 0x170003F3 RID: 1011
		// (get) Token: 0x060007EB RID: 2027 RVA: 0x000107A0 File Offset: 0x0000E9A0
		// (set) Token: 0x060007EC RID: 2028 RVA: 0x000107A8 File Offset: 0x0000E9A8
		public IStandardTraverser Traverser { get; set; }

		// Token: 0x060007ED RID: 2029 RVA: 0x000107B4 File Offset: 0x0000E9B4
		public virtual void visit(_IVariableExpression variable, AccessFlag access)
		{
		}

		// Token: 0x060007EE RID: 2030 RVA: 0x000107B8 File Offset: 0x0000E9B8
		public virtual void visit(_ICompiledPOU cpou)
		{
		}

		// Token: 0x060007EF RID: 2031 RVA: 0x000107BC File Offset: 0x0000E9BC
		public virtual void visit(_IWhileStatement whilst)
		{
		}

		// Token: 0x060007F0 RID: 2032 RVA: 0x000107C0 File Offset: 0x0000E9C0
		public virtual void visit(_IRepeatStatement repeat)
		{
		}

		// Token: 0x060007F1 RID: 2033 RVA: 0x000107C4 File Offset: 0x0000E9C4
		public virtual void visit(_IForStatement forloop)
		{
		}

		// Token: 0x060007F2 RID: 2034 RVA: 0x000107C8 File Offset: 0x0000E9C8
		public virtual void visit(_IExitStatement exit)
		{
		}

		// Token: 0x060007F3 RID: 2035 RVA: 0x000107CC File Offset: 0x0000E9CC
		public virtual void visit(_IContinueStatement cont)
		{
		}

		// Token: 0x060007F4 RID: 2036 RVA: 0x000107D0 File Offset: 0x0000E9D0
		public virtual void visit(_ISequenceStatement seq)
		{
		}

		// Token: 0x060007F5 RID: 2037 RVA: 0x000107D4 File Offset: 0x0000E9D4
		public virtual void visit(_IAssignmentExpression assign)
		{
		}

		// Token: 0x060007F6 RID: 2038 RVA: 0x000107D8 File Offset: 0x0000E9D8
		public virtual void visit(_IIfStatement ifst)
		{
		}

		// Token: 0x060007F7 RID: 2039 RVA: 0x000107DC File Offset: 0x0000E9DC
		public virtual void visit(_IReturnStatement returnst)
		{
		}

		// Token: 0x060007F8 RID: 2040 RVA: 0x000107E0 File Offset: 0x0000E9E0
		public virtual void visit(_IJumpStatement gotost)
		{
		}

		// Token: 0x060007F9 RID: 2041 RVA: 0x000107E4 File Offset: 0x0000E9E4
		public virtual void visit(_ILabelStatement label)
		{
		}

		// Token: 0x060007FA RID: 2042 RVA: 0x000107E8 File Offset: 0x0000E9E8
		public virtual void visit(_ICommentStatement comment)
		{
		}

		// Token: 0x060007FB RID: 2043 RVA: 0x000107EC File Offset: 0x0000E9EC
		public virtual void visit(_IPragmaStatement pragma)
		{
		}

		// Token: 0x060007FC RID: 2044 RVA: 0x000107F0 File Offset: 0x0000E9F0
		public virtual void visit(_IExpressionStatement expstat)
		{
		}

		// Token: 0x060007FD RID: 2045 RVA: 0x000107F4 File Offset: 0x0000E9F4
		public virtual void visit(_ICallExpression call)
		{
		}

		// Token: 0x060007FE RID: 2046 RVA: 0x000107F8 File Offset: 0x0000E9F8
		public virtual void visit(_IOperatorExpression op)
		{
		}

		// Token: 0x060007FF RID: 2047 RVA: 0x000107FC File Offset: 0x0000E9FC
		public virtual void visit(_ICastExpression cast)
		{
		}

		// Token: 0x06000800 RID: 2048 RVA: 0x00010800 File Offset: 0x0000EA00
		public virtual void visit(_INewExpression newexp)
		{
		}

		// Token: 0x06000801 RID: 2049 RVA: 0x00010804 File Offset: 0x0000EA04
		public virtual void visit(_ITypeExpression typeexp)
		{
		}

		// Token: 0x06000802 RID: 2050 RVA: 0x00010808 File Offset: 0x0000EA08
		public virtual void visit(_IConversionExpression conv)
		{
		}

		// Token: 0x06000803 RID: 2051 RVA: 0x0001080C File Offset: 0x0000EA0C
		public virtual void visit(_IThisExpression thisexp)
		{
		}

		// Token: 0x06000804 RID: 2052 RVA: 0x00010810 File Offset: 0x0000EA10
		public virtual void visit(_IBaseExpression baseexp)
		{
		}

		// Token: 0x06000805 RID: 2053 RVA: 0x00010814 File Offset: 0x0000EA14
		public virtual void visit(_ILiteralExpression literal)
		{
		}

		// Token: 0x06000806 RID: 2054 RVA: 0x00010818 File Offset: 0x0000EA18
		public virtual void visit(_IAddressExpression address)
		{
		}

		// Token: 0x06000807 RID: 2055 RVA: 0x0001081C File Offset: 0x0000EA1C
		public virtual void visit(_IIndexAccessExpression indexaccess)
		{
		}

		// Token: 0x170003F4 RID: 1012
		// (get) Token: 0x06000808 RID: 2056 RVA: 0x00010820 File Offset: 0x0000EA20
		public virtual bool bResolveCompoAccessExpression
		{
			get
			{
				return true;
			}
		}

		// Token: 0x06000809 RID: 2057 RVA: 0x00010824 File Offset: 0x0000EA24
		public virtual void visit(_ICompoAccessExpression compo, AccessFlag access)
		{
		}

		// Token: 0x0600080A RID: 2058 RVA: 0x00010828 File Offset: 0x0000EA28
		public virtual void visit(_IDeRefAccessExpression deref)
		{
		}

		// Token: 0x0600080B RID: 2059 RVA: 0x0001082C File Offset: 0x0000EA2C
		public virtual void visit(_ICopyScopeExpression copyexp)
		{
		}

		// Token: 0x0600080C RID: 2060 RVA: 0x00010830 File Offset: 0x0000EA30
		public virtual void visit(_IGlobalScopeExpression globexp)
		{
		}

		// Token: 0x0600080D RID: 2061 RVA: 0x00010834 File Offset: 0x0000EA34
		public virtual void visit(_ISystemScopeExpression systemscope)
		{
		}

		// Token: 0x0600080E RID: 2062 RVA: 0x00010838 File Offset: 0x0000EA38
		public virtual void visit(_IEmptyStatement empty)
		{
		}

		// Token: 0x0600080F RID: 2063 RVA: 0x0001083C File Offset: 0x0000EA3C
		public virtual void visit(_ICaseRangeExpression caserange)
		{
		}

		// Token: 0x06000810 RID: 2064 RVA: 0x00010840 File Offset: 0x0000EA40
		public virtual void visit(_ICaseLabelStatement caselabel)
		{
		}

		// Token: 0x06000811 RID: 2065 RVA: 0x00010844 File Offset: 0x0000EA44
		public virtual void visit(_ICaseStatement casest)
		{
		}

		// Token: 0x06000812 RID: 2066 RVA: 0x00010848 File Offset: 0x0000EA48
		public virtual void visit(_IErrorExpression errorexp)
		{
		}

		// Token: 0x06000813 RID: 2067 RVA: 0x0001084C File Offset: 0x0000EA4C
		public virtual void visit(_IErrorStatement errorst)
		{
		}

		// Token: 0x06000814 RID: 2068 RVA: 0x00010850 File Offset: 0x0000EA50
		public virtual void visit(_INullExpression errorexp)
		{
		}

		// Token: 0x06000815 RID: 2069 RVA: 0x00010854 File Offset: 0x0000EA54
		public virtual void visit(_INullStatement errorst)
		{
		}

		// Token: 0x06000816 RID: 2070 RVA: 0x00010858 File Offset: 0x0000EA58
		public virtual void visit(_IQualifiedNameExpression qne)
		{
		}

		// Token: 0x06000817 RID: 2071 RVA: 0x0001085C File Offset: 0x0000EA5C
		public virtual void visit(_IVariableDeclarationStatement vds)
		{
		}

		// Token: 0x06000818 RID: 2072 RVA: 0x00010860 File Offset: 0x0000EA60
		public virtual void visit(_IVariableDeclarationListStatement vdls)
		{
		}

		// Token: 0x06000819 RID: 2073 RVA: 0x00010864 File Offset: 0x0000EA64
		public virtual void visit(_IPOUDeclarationStatement pds)
		{
		}

		// Token: 0x0600081A RID: 2074 RVA: 0x00010868 File Offset: 0x0000EA68
		public virtual void visit(_ITypeDeclarationStatement tds)
		{
		}

		// Token: 0x0600081B RID: 2075 RVA: 0x0001086C File Offset: 0x0000EA6C
		public virtual void visit(_IEnumDeclarationStatement eds)
		{
		}

		// Token: 0x0600081C RID: 2076 RVA: 0x00010870 File Offset: 0x0000EA70
		public virtual void visit(_IEnumDeclarationListStatement eds)
		{
		}

		// Token: 0x0600081D RID: 2077 RVA: 0x00010874 File Offset: 0x0000EA74
		public virtual void visit(_IMultipleIndexInitialization errorst)
		{
		}

		// Token: 0x0600081E RID: 2078 RVA: 0x00010878 File Offset: 0x0000EA78
		public virtual void visit(_IArrayInitialization errorexp)
		{
		}

		// Token: 0x0600081F RID: 2079 RVA: 0x0001087C File Offset: 0x0000EA7C
		public virtual void visit(_IStructureInitialization errorst)
		{
		}

		// Token: 0x06000820 RID: 2080 RVA: 0x00010880 File Offset: 0x0000EA80
		public virtual void visit(_IDefineReference defref)
		{
		}

		// Token: 0x06000821 RID: 2081 RVA: 0x00010884 File Offset: 0x0000EA84
		public virtual void visit(_IVariableReference varref)
		{
		}

		// Token: 0x06000822 RID: 2082 RVA: 0x00010888 File Offset: 0x0000EA88
		public virtual void visit(_ITypeReference typeref)
		{
		}

		// Token: 0x06000823 RID: 2083 RVA: 0x0001088C File Offset: 0x0000EA8C
		public virtual void visit(_IPouReference pouref)
		{
		}

		// Token: 0x06000824 RID: 2084 RVA: 0x00010890 File Offset: 0x0000EA90
		public virtual void visit(_ITaskReference taskref)
		{
		}

		// Token: 0x06000825 RID: 2085 RVA: 0x00010894 File Offset: 0x0000EA94
		public virtual void visit(_IResourceReference resref)
		{
		}

		// Token: 0x06000826 RID: 2086 RVA: 0x00010898 File Offset: 0x0000EA98
		public virtual void visit(_IDefinedExpression defexp)
		{
		}

		// Token: 0x06000827 RID: 2087 RVA: 0x0001089C File Offset: 0x0000EA9C
		public virtual void visit(_IPragmaOperatorExpression popexp)
		{
		}

		// Token: 0x06000828 RID: 2088 RVA: 0x000108A0 File Offset: 0x0000EAA0
		public virtual void visit(_IPragmaIfStatement pifst)
		{
		}

		// Token: 0x06000829 RID: 2089 RVA: 0x000108A4 File Offset: 0x0000EAA4
		public virtual void visit(_IBreakPointStatement bpstate)
		{
		}

		// Token: 0x0600082A RID: 2090 RVA: 0x000108A8 File Offset: 0x0000EAA8
		public virtual void visit(_IDefineStatement defstate)
		{
		}

		// Token: 0x0600082B RID: 2091 RVA: 0x000108AC File Offset: 0x0000EAAC
		public virtual void visit(_IXRefExpression xref)
		{
		}

		// Token: 0x0600082C RID: 2092 RVA: 0x000108B0 File Offset: 0x0000EAB0
		public virtual void visit(_IHasTypeExpression hastype)
		{
		}

		// Token: 0x0600082D RID: 2093 RVA: 0x000108B4 File Offset: 0x0000EAB4
		public virtual void visit(_IIsEnumTypeExpression isenumtype)
		{
		}

		// Token: 0x0600082E RID: 2094 RVA: 0x000108B8 File Offset: 0x0000EAB8
		public virtual void visit(_IHasAttributeExpression hasattribute)
		{
		}

		// Token: 0x0600082F RID: 2095 RVA: 0x000108BC File Offset: 0x0000EABC
		public virtual void visit(_IHasValueExpression hasvalue)
		{
		}

		// Token: 0x06000830 RID: 2096 RVA: 0x000108C0 File Offset: 0x0000EAC0
		public virtual void visit(_IHasConstantValueExpression hasvalue)
		{
		}

		// Token: 0x06000831 RID: 2097 RVA: 0x000108C4 File Offset: 0x0000EAC4
		public virtual void visit(_IPragmaAssertion assertion)
		{
		}

		// Token: 0x06000832 RID: 2098 RVA: 0x000108C8 File Offset: 0x0000EAC8
		public virtual void visit(_ICompilerVersionExpression compversion)
		{
		}

		// Token: 0x170003F5 RID: 1013
		// (get) Token: 0x06000833 RID: 2099 RVA: 0x000108CC File Offset: 0x0000EACC
		public virtual bool DoCallExpression
		{
			get
			{
				return false;
			}
		}

		// Token: 0x170003F6 RID: 1014
		// (get) Token: 0x06000834 RID: 2100 RVA: 0x000108D0 File Offset: 0x0000EAD0
		public virtual bool DoAssignExpression
		{
			get
			{
				return false;
			}
		}

		// Token: 0x06000835 RID: 2101 RVA: 0x000108D4 File Offset: 0x0000EAD4
		public virtual void visit(_ITryCatchStatement trycatch)
		{
		}

		// Token: 0x0400011D RID: 285
		[CompilerGenerated]
		private IStandardTraverser \u0001;
	}
}
