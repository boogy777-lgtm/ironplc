using System;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler35220.TreeConversion
{
	// Token: 0x02000018 RID: 24
	internal interface IGreenTreeVisitor
	{
		// Token: 0x06000464 RID: 1124
		void visit(_ISequenceStatement seq, int nExprementId);

		// Token: 0x06000465 RID: 1125
		void visit(_IWhileStatement whilst, int nExprementId);

		// Token: 0x06000466 RID: 1126
		void visit(_IRepeatStatement repeat, int nExprementId);

		// Token: 0x06000467 RID: 1127
		void visit(_IForStatement forloop, int nExprementId);

		// Token: 0x06000468 RID: 1128
		void visit(_IExitStatement exit, int nExprementId);

		// Token: 0x06000469 RID: 1129
		void visit(_IContinueStatement cont, int nExprementId);

		// Token: 0x0600046A RID: 1130
		void visit(_IIfStatement ifst, int nExprementId);

		// Token: 0x0600046B RID: 1131
		void visit(_IReturnStatement returnst, int nExprementId);

		// Token: 0x0600046C RID: 1132
		void visit(_IJumpStatement gotost, int nExprementId);

		// Token: 0x0600046D RID: 1133
		void visit(_ILabelStatement label, int nExprementId);

		// Token: 0x0600046E RID: 1134
		void visit(_ICommentStatement comment, int nExprementId);

		// Token: 0x0600046F RID: 1135
		void visit(_IPragmaStatement pragma, int nExprementId);

		// Token: 0x06000470 RID: 1136
		void visit(_IExpressionStatement expstat, int nExprementId);

		// Token: 0x06000471 RID: 1137
		void visit(_IAssignmentExpression assign, int nExprementId);

		// Token: 0x06000472 RID: 1138
		void visit(_ICallExpression call, int nExprementId);

		// Token: 0x06000473 RID: 1139
		void visit(_IOperatorExpression op, int nExprementId);

		// Token: 0x06000474 RID: 1140
		void visit(_IConversionExpression conv, int nExprementId);

		// Token: 0x06000475 RID: 1141
		void visit(_IThisExpression thisexp, int nExprementId);

		// Token: 0x06000476 RID: 1142
		void visit(_IBaseExpression baseexp, int nExprementId);

		// Token: 0x06000477 RID: 1143
		void visit(_ILiteralExpression literal, int nExprementId);

		// Token: 0x06000478 RID: 1144
		void visit(_IAddressExpression address, int nExprementId);

		// Token: 0x06000479 RID: 1145
		void visit(_IVariableExpression variable, int nExprementId);

		// Token: 0x0600047A RID: 1146
		void visit(_IIndexAccessExpression indexaccess, int nExprementId);

		// Token: 0x0600047B RID: 1147
		void visit(_ICompoAccessExpression compo, int nExprementId);

		// Token: 0x0600047C RID: 1148
		void visit(_IDeRefAccessExpression deref, int nExprementId);

		// Token: 0x0600047D RID: 1149
		void visit(_IGlobalScopeExpression globexp, int nExprementId);

		// Token: 0x0600047E RID: 1150
		void visit(_ISystemScopeExpression systemscope, int nExprementId);

		// Token: 0x0600047F RID: 1151
		void visit(_IEmptyStatement empty, int nExprementId);

		// Token: 0x06000480 RID: 1152
		void visit(_ICaseRangeExpression caserange, int nExprementId);

		// Token: 0x06000481 RID: 1153
		void visit(_ICaseLabelStatement caselabel, int nExprementId);

		// Token: 0x06000482 RID: 1154
		void visit(_ICaseStatement casest, int nExprementId);

		// Token: 0x06000483 RID: 1155
		void visit(_IErrorExpression errorexp, int nExprementId);

		// Token: 0x06000484 RID: 1156
		void visit(_IErrorStatement errorst, int nExprementId);

		// Token: 0x06000485 RID: 1157
		void visit(_INullExpression errorexp, int nExprementId);

		// Token: 0x06000486 RID: 1158
		void visit(_INullStatement errorst, int nExprementId);

		// Token: 0x06000487 RID: 1159
		void visit(_IMultipleIndexInitialization mix, int nExprementId);

		// Token: 0x06000488 RID: 1160
		void visit(_IArrayInitialization arrayinit, int nExprementId);

		// Token: 0x06000489 RID: 1161
		void visit(_IStructureInitialization structinit, int nExprementId);

		// Token: 0x0600048A RID: 1162
		void visit(_IDefineReference defref, int nExprementId);

		// Token: 0x0600048B RID: 1163
		void visit(_IVariableReference varref, int nExprementId);

		// Token: 0x0600048C RID: 1164
		void visit(_ITypeReference typeref, int nExprementId);

		// Token: 0x0600048D RID: 1165
		void visit(_IPouReference pouref, int nExprementId);

		// Token: 0x0600048E RID: 1166
		void visit(_ITaskReference taskref, int nExprementId);

		// Token: 0x0600048F RID: 1167
		void visit(_IResourceReference resref, int nExprementId);

		// Token: 0x06000490 RID: 1168
		void visit(_IDefinedExpression defexp, int nExprementId);

		// Token: 0x06000491 RID: 1169
		void visit(_IXRefExpression xref, int nExprementId);

		// Token: 0x06000492 RID: 1170
		void visit(_ICompilerVersionExpression compiversionexp, int nExprementId);

		// Token: 0x06000493 RID: 1171
		void visit(_IPragmaOperatorExpression popexp, int nExprementId);

		// Token: 0x06000494 RID: 1172
		void visit(_IPragmaIfStatement pifst, int nExprementId);

		// Token: 0x06000495 RID: 1173
		void visit(_IDefineStatement defstate, int nExprementId);

		// Token: 0x06000496 RID: 1174
		void visit(_IHasCompatibleTypeExpression hastype, int nExprementId);

		// Token: 0x06000497 RID: 1175
		void visit(_IHasTypeExpression hastype, int nExprementId);

		// Token: 0x06000498 RID: 1176
		void visit(_IIsEnumTypeExpression isenumtype, int nExprementId);

		// Token: 0x06000499 RID: 1177
		void visit(_IHasAttributeExpression hasattribute, int nExprementId);

		// Token: 0x0600049A RID: 1178
		void visit(_IHasValueExpression hasvalue, int nExprementId);

		// Token: 0x0600049B RID: 1179
		void visit(_IHasConstantValueExpression hasvalue, int nExprementId);

		// Token: 0x0600049C RID: 1180
		void visit(_IPragmaAssertion assertion, int nExprementId);

		// Token: 0x0600049D RID: 1181
		void visit(_ICastExpression castexp, int nExprementId);

		// Token: 0x0600049E RID: 1182
		void visit(_INewExpression newexp, int nExprementId);

		// Token: 0x0600049F RID: 1183
		void visit(_ITypeExpression typeexp, int nExprementId);

		// Token: 0x060004A0 RID: 1184
		void visit(_INamespaceAccessExpression namespaceaccess, int nExprementId);

		// Token: 0x060004A1 RID: 1185
		void visit(_IRuntimeVersionExpression runtimeversionexp, int nExprementId);

		// Token: 0x060004A2 RID: 1186
		void visit(_ICurrentTaskExpression currentTask, int nExprementId);

		// Token: 0x060004A3 RID: 1187
		void visit(_IPoolScopeExpression poolscope, int nExprementId);

		// Token: 0x060004A4 RID: 1188
		void visit(_ITryCatchStatement trycatchstatement, int nExprementId);

		// Token: 0x060004A5 RID: 1189
		void visit(_IBreakPointStatement bpstatement, int nExprementId);

		// Token: 0x060004A6 RID: 1190
		void visit(_IHasConstantTypeExpression hasConstantTypeExpression, int nExprementId);

		// Token: 0x060004A7 RID: 1191
		void visit(_IPartialAccessExpression partialAccessExpression, int nExprementId);

		// Token: 0x060004A8 RID: 1192
		void visit(_IProjectDefinedExpression projectDefinedExpression, int nExprementId);
	}
}
