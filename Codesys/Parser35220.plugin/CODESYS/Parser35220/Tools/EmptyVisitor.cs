using System;
using System.Diagnostics.CodeAnalysis;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace CODESYS.Parser35220.Tools
{
	// Token: 0x0200000D RID: 13
	[ExcludeFromCodeCoverage]
	public abstract class EmptyVisitor : IExprementVisitor352000, IExprementVisitor351900, IExprementVisitor351800, IExprementVisitor351500, IExprementVisitor351400, IExprementVisitor351300, IExprementVisitor3590, IExprementVisitor, IExprementVisitor2
	{
		// Token: 0x06000060 RID: 96 RVA: 0x00002074 File Offset: 0x00000274
		public virtual void visit(_ICompiledPOU cpou)
		{
		}

		// Token: 0x06000061 RID: 97 RVA: 0x00002074 File Offset: 0x00000274
		public virtual void visit(_IWhileStatement whilst)
		{
		}

		// Token: 0x06000062 RID: 98 RVA: 0x00002074 File Offset: 0x00000274
		public virtual void visit(_IRepeatStatement repeat)
		{
		}

		// Token: 0x06000063 RID: 99 RVA: 0x00002074 File Offset: 0x00000274
		public virtual void visit(_IForStatement forloop)
		{
		}

		// Token: 0x06000064 RID: 100 RVA: 0x00002074 File Offset: 0x00000274
		public virtual void visit(_IExitStatement exit)
		{
		}

		// Token: 0x06000065 RID: 101 RVA: 0x00002074 File Offset: 0x00000274
		public virtual void visit(_IContinueStatement cont)
		{
		}

		// Token: 0x06000066 RID: 102 RVA: 0x00002074 File Offset: 0x00000274
		public virtual void visit(_ISequenceStatement seq)
		{
		}

		// Token: 0x06000067 RID: 103 RVA: 0x00002074 File Offset: 0x00000274
		public virtual void visit(_IAssignmentExpression assign)
		{
		}

		// Token: 0x06000068 RID: 104 RVA: 0x00002074 File Offset: 0x00000274
		public virtual void visit(_IIfStatement ifst)
		{
		}

		// Token: 0x06000069 RID: 105 RVA: 0x00002074 File Offset: 0x00000274
		public virtual void visit(_IReturnStatement returnst)
		{
		}

		// Token: 0x0600006A RID: 106 RVA: 0x00002074 File Offset: 0x00000274
		public virtual void visit(_IJumpStatement gotost)
		{
		}

		// Token: 0x0600006B RID: 107 RVA: 0x00002074 File Offset: 0x00000274
		public virtual void visit(_ILabelStatement label)
		{
		}

		// Token: 0x0600006C RID: 108 RVA: 0x00002074 File Offset: 0x00000274
		public virtual void visit(_ICommentStatement comment)
		{
		}

		// Token: 0x0600006D RID: 109 RVA: 0x00002074 File Offset: 0x00000274
		public virtual void visit(_IPragmaStatement pragma)
		{
		}

		// Token: 0x0600006E RID: 110 RVA: 0x00002074 File Offset: 0x00000274
		public virtual void visit(_IExpressionStatement expstat)
		{
		}

		// Token: 0x0600006F RID: 111 RVA: 0x00002074 File Offset: 0x00000274
		public virtual void visit(_ICallExpression call)
		{
		}

		// Token: 0x06000070 RID: 112 RVA: 0x00002074 File Offset: 0x00000274
		public virtual void visit(_IOperatorExpression op)
		{
		}

		// Token: 0x06000071 RID: 113 RVA: 0x00002074 File Offset: 0x00000274
		public virtual void visit(_ICastExpression castexp)
		{
		}

		// Token: 0x06000072 RID: 114 RVA: 0x00002074 File Offset: 0x00000274
		public virtual void visit(_INewExpression typeref)
		{
		}

		// Token: 0x06000073 RID: 115 RVA: 0x00002074 File Offset: 0x00000274
		public virtual void visit(_ITypeExpression typeexp)
		{
		}

		// Token: 0x06000074 RID: 116 RVA: 0x00002074 File Offset: 0x00000274
		public virtual void visit(_IConversionExpression conv)
		{
		}

		// Token: 0x06000075 RID: 117 RVA: 0x00002074 File Offset: 0x00000274
		public virtual void visit(_IThisExpression thisexp)
		{
		}

		// Token: 0x06000076 RID: 118 RVA: 0x00002074 File Offset: 0x00000274
		public virtual void visit(_IBaseExpression baseexp)
		{
		}

		// Token: 0x06000077 RID: 119 RVA: 0x00002074 File Offset: 0x00000274
		public virtual void visit(_ILiteralExpression literal)
		{
		}

		// Token: 0x06000078 RID: 120 RVA: 0x00002074 File Offset: 0x00000274
		public virtual void visit(_IAddressExpression address)
		{
		}

		// Token: 0x06000079 RID: 121 RVA: 0x00002074 File Offset: 0x00000274
		public virtual void visit(_IIndexAccessExpression indexaccess)
		{
		}

		// Token: 0x0600007A RID: 122 RVA: 0x00002074 File Offset: 0x00000274
		public virtual void visit(_IDeRefAccessExpression deref)
		{
		}

		// Token: 0x0600007B RID: 123 RVA: 0x00002074 File Offset: 0x00000274
		public virtual void visit(_ICopyScopeExpression copyexp)
		{
		}

		// Token: 0x0600007C RID: 124 RVA: 0x00002074 File Offset: 0x00000274
		public virtual void visit(_IGlobalScopeExpression globexp)
		{
		}

		// Token: 0x0600007D RID: 125 RVA: 0x00002074 File Offset: 0x00000274
		public virtual void visit(_ISystemScopeExpression systemscope)
		{
		}

		// Token: 0x0600007E RID: 126 RVA: 0x00002074 File Offset: 0x00000274
		public virtual void visit(_IEmptyStatement empty)
		{
		}

		// Token: 0x0600007F RID: 127 RVA: 0x00002074 File Offset: 0x00000274
		public virtual void visit(_ICaseRangeExpression caserange)
		{
		}

		// Token: 0x06000080 RID: 128 RVA: 0x00002074 File Offset: 0x00000274
		public virtual void visit(_ICaseLabelStatement caselabel)
		{
		}

		// Token: 0x06000081 RID: 129 RVA: 0x00002074 File Offset: 0x00000274
		public virtual void visit(_ICaseStatement casest)
		{
		}

		// Token: 0x06000082 RID: 130 RVA: 0x00002074 File Offset: 0x00000274
		public virtual void visit(_IErrorExpression errorexp)
		{
		}

		// Token: 0x06000083 RID: 131 RVA: 0x00002074 File Offset: 0x00000274
		public virtual void visit(_IErrorStatement errorst)
		{
		}

		// Token: 0x06000084 RID: 132 RVA: 0x00002074 File Offset: 0x00000274
		public virtual void visit(_INullExpression errorexp)
		{
		}

		// Token: 0x06000085 RID: 133 RVA: 0x00002074 File Offset: 0x00000274
		public virtual void visit(_INullStatement errorst)
		{
		}

		// Token: 0x06000086 RID: 134 RVA: 0x00002074 File Offset: 0x00000274
		public virtual void visit(_IQualifiedNameExpression qne)
		{
		}

		// Token: 0x06000087 RID: 135 RVA: 0x00002074 File Offset: 0x00000274
		public virtual void visit(_IVariableDeclarationStatement vds)
		{
		}

		// Token: 0x06000088 RID: 136 RVA: 0x00002074 File Offset: 0x00000274
		public virtual void visit(_IVariableDeclarationListStatement vdls)
		{
		}

		// Token: 0x06000089 RID: 137 RVA: 0x00002074 File Offset: 0x00000274
		public virtual void visit(_IPOUDeclarationStatement pds)
		{
		}

		// Token: 0x0600008A RID: 138 RVA: 0x00002074 File Offset: 0x00000274
		public virtual void visit(_ITypeDeclarationStatement tds)
		{
		}

		// Token: 0x0600008B RID: 139 RVA: 0x00002074 File Offset: 0x00000274
		public virtual void visit(_IEnumDeclarationStatement eds)
		{
		}

		// Token: 0x0600008C RID: 140 RVA: 0x00002074 File Offset: 0x00000274
		public virtual void visit(_IEnumDeclarationListStatement eds)
		{
		}

		// Token: 0x0600008D RID: 141 RVA: 0x00002074 File Offset: 0x00000274
		public virtual void visit(_IMultipleIndexInitialization errorst)
		{
		}

		// Token: 0x0600008E RID: 142 RVA: 0x00002074 File Offset: 0x00000274
		public virtual void visit(_IArrayInitialization errorexp)
		{
		}

		// Token: 0x0600008F RID: 143 RVA: 0x00002074 File Offset: 0x00000274
		public virtual void visit(_IStructureInitialization errorst)
		{
		}

		// Token: 0x06000090 RID: 144 RVA: 0x00002074 File Offset: 0x00000274
		public virtual void visit(_IDefineReference defref)
		{
		}

		// Token: 0x06000091 RID: 145 RVA: 0x00002074 File Offset: 0x00000274
		public virtual void visit(_IVariableReference varref)
		{
		}

		// Token: 0x06000092 RID: 146 RVA: 0x00002074 File Offset: 0x00000274
		public virtual void visit(_ITypeReference typeref)
		{
		}

		// Token: 0x06000093 RID: 147 RVA: 0x00002074 File Offset: 0x00000274
		public virtual void visit(_IPouReference pouref)
		{
		}

		// Token: 0x06000094 RID: 148 RVA: 0x00002074 File Offset: 0x00000274
		public virtual void visit(_ITaskReference taskref)
		{
		}

		// Token: 0x06000095 RID: 149 RVA: 0x00002074 File Offset: 0x00000274
		public virtual void visit(_IResourceReference resref)
		{
		}

		// Token: 0x06000096 RID: 150 RVA: 0x00002074 File Offset: 0x00000274
		public virtual void visit(_IDefinedExpression defexp)
		{
		}

		// Token: 0x06000097 RID: 151 RVA: 0x00002074 File Offset: 0x00000274
		public virtual void visit(_IPragmaOperatorExpression popexp)
		{
		}

		// Token: 0x06000098 RID: 152 RVA: 0x00002074 File Offset: 0x00000274
		public virtual void visit(_IPragmaIfStatement pifst)
		{
		}

		// Token: 0x06000099 RID: 153 RVA: 0x00002074 File Offset: 0x00000274
		public virtual void visit(_IBreakPointStatement bpstate)
		{
		}

		// Token: 0x0600009A RID: 154 RVA: 0x00002074 File Offset: 0x00000274
		public virtual void visit(_IDefineStatement defstate)
		{
		}

		// Token: 0x0600009B RID: 155 RVA: 0x00002074 File Offset: 0x00000274
		public virtual void visit(_IXRefExpression xref)
		{
		}

		// Token: 0x0600009C RID: 156 RVA: 0x00002074 File Offset: 0x00000274
		public virtual void visit(_IHasTypeExpression hastype)
		{
		}

		// Token: 0x0600009D RID: 157 RVA: 0x00002074 File Offset: 0x00000274
		public virtual void visit(_IIsEnumTypeExpression isenumtype)
		{
		}

		// Token: 0x0600009E RID: 158 RVA: 0x00002074 File Offset: 0x00000274
		public virtual void visit(_IHasAttributeExpression hasattribute)
		{
		}

		// Token: 0x0600009F RID: 159 RVA: 0x00002074 File Offset: 0x00000274
		public virtual void visit(_IHasValueExpression hasvalue)
		{
		}

		// Token: 0x060000A0 RID: 160 RVA: 0x00002074 File Offset: 0x00000274
		public virtual void visit(_IHasConstantValueExpression hasvalue)
		{
		}

		// Token: 0x060000A1 RID: 161 RVA: 0x00002074 File Offset: 0x00000274
		public virtual void visit(_IPragmaAssertion assertion)
		{
		}

		// Token: 0x060000A2 RID: 162 RVA: 0x00002074 File Offset: 0x00000274
		public virtual void visit(_ICompilerVersionExpression compiversionexp)
		{
		}

		// Token: 0x060000A3 RID: 163 RVA: 0x00002074 File Offset: 0x00000274
		public virtual void visit(_ITryCatchStatement trycatchstatement)
		{
		}

		// Token: 0x060000A4 RID: 164 RVA: 0x00002074 File Offset: 0x00000274
		public virtual void visit(_IPoolScopeExpression poolscope)
		{
		}

		// Token: 0x060000A5 RID: 165 RVA: 0x00002074 File Offset: 0x00000274
		public virtual void visit(_INamespaceAccessExpression namespaceaccess)
		{
		}

		// Token: 0x060000A6 RID: 166 RVA: 0x00002074 File Offset: 0x00000274
		public virtual void visit(_ICurrentTaskExpression currentTask)
		{
		}

		// Token: 0x060000A7 RID: 167 RVA: 0x00002074 File Offset: 0x00000274
		public virtual void visit(_IRuntimeVersionExpression runtimeversionexp)
		{
		}

		// Token: 0x060000A8 RID: 168 RVA: 0x00002074 File Offset: 0x00000274
		public virtual void visit(_IVariableExpression variable)
		{
		}

		// Token: 0x060000A9 RID: 169 RVA: 0x00002074 File Offset: 0x00000274
		public virtual void visit(_ICompoAccessExpression compo)
		{
		}

		// Token: 0x060000AA RID: 170 RVA: 0x00002074 File Offset: 0x00000274
		public virtual void visit(_IHasConstantTypeExpression hasConstantTypeExpression)
		{
		}

		// Token: 0x060000AB RID: 171 RVA: 0x00002074 File Offset: 0x00000274
		public virtual void visit(_IPartialAccessExpression partialAccessExpression)
		{
		}

		// Token: 0x060000AC RID: 172 RVA: 0x00002074 File Offset: 0x00000274
		public virtual void visit(_IProjectDefinedExpression projectDefinedExpression)
		{
		}
	}
}
