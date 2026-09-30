using System;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler35220.TreeConversion
{
	// Token: 0x02000019 RID: 25
	public abstract class SimpleGreenTreeVisitor : IGreenTreeVisitor
	{
		// Token: 0x060004A9 RID: 1193
		public abstract void HandleExprement(_IExprement exp, int nExprementId);

		// Token: 0x060004AA RID: 1194 RVA: 0x00009340 File Offset: 0x00007540
		public void visit(_IRepeatStatement repeat, int nExprementId)
		{
			this.HandleExprement(repeat, nExprementId);
		}

		// Token: 0x060004AB RID: 1195 RVA: 0x0000934C File Offset: 0x0000754C
		public void visit(_IExitStatement exit, int nExprementId)
		{
			this.HandleExprement(exit, nExprementId);
		}

		// Token: 0x060004AC RID: 1196 RVA: 0x00009358 File Offset: 0x00007558
		public void visit(_IIfStatement ifst, int nExprementId)
		{
			this.HandleExprement(ifst, nExprementId);
		}

		// Token: 0x060004AD RID: 1197 RVA: 0x00009364 File Offset: 0x00007564
		public void visit(_IJumpStatement gotost, int nExprementId)
		{
			this.HandleExprement(gotost, nExprementId);
		}

		// Token: 0x060004AE RID: 1198 RVA: 0x00009370 File Offset: 0x00007570
		public void visit(_ICommentStatement comment, int nExprementId)
		{
			this.HandleExprement(comment, nExprementId);
		}

		// Token: 0x060004AF RID: 1199 RVA: 0x0000937C File Offset: 0x0000757C
		public void visit(_IExpressionStatement expstat, int nExprementId)
		{
			this.HandleExprement(expstat, nExprementId);
		}

		// Token: 0x060004B0 RID: 1200 RVA: 0x00009388 File Offset: 0x00007588
		public void visit(_ICallExpression call, int nExprementId)
		{
			this.HandleExprement(call, nExprementId);
		}

		// Token: 0x060004B1 RID: 1201 RVA: 0x00009394 File Offset: 0x00007594
		public void visit(_IConversionExpression conv, int nExprementId)
		{
			this.HandleExprement(conv, nExprementId);
		}

		// Token: 0x060004B2 RID: 1202 RVA: 0x000093A0 File Offset: 0x000075A0
		public void visit(_IBaseExpression baseexp, int nExprementId)
		{
			this.HandleExprement(baseexp, nExprementId);
		}

		// Token: 0x060004B3 RID: 1203 RVA: 0x000093AC File Offset: 0x000075AC
		public void visit(_IAddressExpression address, int nExprementId)
		{
			this.HandleExprement(address, nExprementId);
		}

		// Token: 0x060004B4 RID: 1204 RVA: 0x000093B8 File Offset: 0x000075B8
		public void visit(_IIndexAccessExpression indexaccess, int nExprementId)
		{
			this.HandleExprement(indexaccess, nExprementId);
		}

		// Token: 0x060004B5 RID: 1205 RVA: 0x000093C4 File Offset: 0x000075C4
		public void visit(_IDeRefAccessExpression deref, int nExprementId)
		{
			this.HandleExprement(deref, nExprementId);
		}

		// Token: 0x060004B6 RID: 1206 RVA: 0x000093D0 File Offset: 0x000075D0
		public void visit(_ISystemScopeExpression systemscope, int nExprementId)
		{
			this.HandleExprement(systemscope, nExprementId);
		}

		// Token: 0x060004B7 RID: 1207 RVA: 0x000093DC File Offset: 0x000075DC
		public void visit(_ICaseRangeExpression caserange, int nExprementId)
		{
			this.HandleExprement(caserange, nExprementId);
		}

		// Token: 0x060004B8 RID: 1208 RVA: 0x000093E8 File Offset: 0x000075E8
		public void visit(_ICaseStatement casest, int nExprementId)
		{
			this.HandleExprement(casest, nExprementId);
		}

		// Token: 0x060004B9 RID: 1209 RVA: 0x000093F4 File Offset: 0x000075F4
		public void visit(_IErrorStatement errorst, int nExprementId)
		{
			this.HandleExprement(errorst, nExprementId);
		}

		// Token: 0x060004BA RID: 1210 RVA: 0x00009400 File Offset: 0x00007600
		public void visit(_INullStatement errorst, int nExprementId)
		{
			this.HandleExprement(errorst, nExprementId);
		}

		// Token: 0x060004BB RID: 1211 RVA: 0x0000940C File Offset: 0x0000760C
		public void visit(_IArrayInitialization arrayinit, int nExprementId)
		{
			this.HandleExprement(arrayinit, nExprementId);
		}

		// Token: 0x060004BC RID: 1212 RVA: 0x00009418 File Offset: 0x00007618
		public void visit(_IDefineReference defref, int nExprementId)
		{
			this.HandleExprement(defref, nExprementId);
		}

		// Token: 0x060004BD RID: 1213 RVA: 0x00009424 File Offset: 0x00007624
		public void visit(_ITypeReference typeref, int nExprementId)
		{
			this.HandleExprement(typeref, nExprementId);
		}

		// Token: 0x060004BE RID: 1214 RVA: 0x00009430 File Offset: 0x00007630
		public void visit(_ITaskReference taskref, int nExprementId)
		{
			this.HandleExprement(taskref, nExprementId);
		}

		// Token: 0x060004BF RID: 1215 RVA: 0x0000943C File Offset: 0x0000763C
		public void visit(_IDefinedExpression defexp, int nExprementId)
		{
			this.HandleExprement(defexp, nExprementId);
		}

		// Token: 0x060004C0 RID: 1216 RVA: 0x00009448 File Offset: 0x00007648
		public void visit(_ICompilerVersionExpression compiversionexp, int nExprementId)
		{
			this.HandleExprement(compiversionexp, nExprementId);
		}

		// Token: 0x060004C1 RID: 1217 RVA: 0x00009454 File Offset: 0x00007654
		public void visit(_IPragmaIfStatement pifst, int nExprementId)
		{
			this.HandleExprement(pifst, nExprementId);
		}

		// Token: 0x060004C2 RID: 1218 RVA: 0x00009460 File Offset: 0x00007660
		public void visit(_IHasCompatibleTypeExpression hastype, int nExprementId)
		{
			this.HandleExprement(hastype, nExprementId);
		}

		// Token: 0x060004C3 RID: 1219 RVA: 0x0000946C File Offset: 0x0000766C
		public void visit(_IIsEnumTypeExpression isenumtype, int nExprementId)
		{
			this.HandleExprement(isenumtype, nExprementId);
		}

		// Token: 0x060004C4 RID: 1220 RVA: 0x00009478 File Offset: 0x00007678
		public void visit(_IHasValueExpression hasvalue, int nExprementId)
		{
			this.HandleExprement(hasvalue, nExprementId);
		}

		// Token: 0x060004C5 RID: 1221 RVA: 0x00009484 File Offset: 0x00007684
		public void visit(_IPragmaAssertion assertion, int nExprementId)
		{
			this.HandleExprement(assertion, nExprementId);
		}

		// Token: 0x060004C6 RID: 1222 RVA: 0x00009490 File Offset: 0x00007690
		public void visit(_INewExpression newexp, int nExprementId)
		{
			this.HandleExprement(newexp, nExprementId);
		}

		// Token: 0x060004C7 RID: 1223 RVA: 0x0000949C File Offset: 0x0000769C
		public void visit(_INamespaceAccessExpression namespaceaccess, int nExprementId)
		{
			this.HandleExprement(namespaceaccess, nExprementId);
		}

		// Token: 0x060004C8 RID: 1224 RVA: 0x000094A8 File Offset: 0x000076A8
		public void visit(_ICurrentTaskExpression currentTask, int nExprementId)
		{
			this.HandleExprement(currentTask, nExprementId);
		}

		// Token: 0x060004C9 RID: 1225 RVA: 0x000094B4 File Offset: 0x000076B4
		public void visit(_ITryCatchStatement trycatchstatement, int nExprementId)
		{
			this.HandleExprement(trycatchstatement, nExprementId);
		}

		// Token: 0x060004CA RID: 1226 RVA: 0x000094C0 File Offset: 0x000076C0
		public void visit(_IPoolScopeExpression poolscope, int nExprementId)
		{
			this.HandleExprement(poolscope, nExprementId);
		}

		// Token: 0x060004CB RID: 1227 RVA: 0x000094CC File Offset: 0x000076CC
		public void visit(_IRuntimeVersionExpression runtimeversionexp, int nExprementId)
		{
			this.HandleExprement(runtimeversionexp, nExprementId);
		}

		// Token: 0x060004CC RID: 1228 RVA: 0x000094D8 File Offset: 0x000076D8
		public void visit(_ITypeExpression typeexp, int nExprementId)
		{
			this.HandleExprement(typeexp, nExprementId);
		}

		// Token: 0x060004CD RID: 1229 RVA: 0x000094E4 File Offset: 0x000076E4
		public void visit(_ICastExpression castexp, int nExprementId)
		{
			this.HandleExprement(castexp, nExprementId);
		}

		// Token: 0x060004CE RID: 1230 RVA: 0x000094F0 File Offset: 0x000076F0
		public void visit(_IHasConstantValueExpression hasvalue, int nExprementId)
		{
			this.HandleExprement(hasvalue, nExprementId);
		}

		// Token: 0x060004CF RID: 1231 RVA: 0x000094FC File Offset: 0x000076FC
		public void visit(_IHasConstantTypeExpression hasConstantTypeExpression, int nExprementId)
		{
			this.HandleExprement(hasConstantTypeExpression, nExprementId);
		}

		// Token: 0x060004D0 RID: 1232 RVA: 0x00009508 File Offset: 0x00007708
		public void visit(_IHasAttributeExpression hasattribute, int nExprementId)
		{
			this.HandleExprement(hasattribute, nExprementId);
		}

		// Token: 0x060004D1 RID: 1233 RVA: 0x00009514 File Offset: 0x00007714
		public void visit(_IHasTypeExpression hastype, int nExprementId)
		{
			this.HandleExprement(hastype, nExprementId);
		}

		// Token: 0x060004D2 RID: 1234 RVA: 0x00009520 File Offset: 0x00007720
		public void visit(_IDefineStatement defstate, int nExprementId)
		{
			this.HandleExprement(defstate, nExprementId);
		}

		// Token: 0x060004D3 RID: 1235 RVA: 0x0000952C File Offset: 0x0000772C
		public void visit(_IPragmaOperatorExpression popexp, int nExprementId)
		{
			this.HandleExprement(popexp, nExprementId);
		}

		// Token: 0x060004D4 RID: 1236 RVA: 0x00009538 File Offset: 0x00007738
		public void visit(_IXRefExpression xref, int nExprementId)
		{
			this.HandleExprement(xref, nExprementId);
		}

		// Token: 0x060004D5 RID: 1237 RVA: 0x00009544 File Offset: 0x00007744
		public void visit(_IResourceReference resref, int nExprementId)
		{
			this.HandleExprement(resref, nExprementId);
		}

		// Token: 0x060004D6 RID: 1238 RVA: 0x00009550 File Offset: 0x00007750
		public void visit(_IPouReference pouref, int nExprementId)
		{
			this.HandleExprement(pouref, nExprementId);
		}

		// Token: 0x060004D7 RID: 1239 RVA: 0x0000955C File Offset: 0x0000775C
		public void visit(_IVariableReference varref, int nExprementId)
		{
			this.HandleExprement(varref, nExprementId);
		}

		// Token: 0x060004D8 RID: 1240 RVA: 0x00009568 File Offset: 0x00007768
		public void visit(_IStructureInitialization structinit, int nExprementId)
		{
			this.HandleExprement(structinit, nExprementId);
		}

		// Token: 0x060004D9 RID: 1241 RVA: 0x00009574 File Offset: 0x00007774
		public void visit(_IMultipleIndexInitialization mix, int nExprementId)
		{
			this.HandleExprement(mix, nExprementId);
		}

		// Token: 0x060004DA RID: 1242 RVA: 0x00009580 File Offset: 0x00007780
		public void visit(_INullExpression errorexp, int nExprementId)
		{
			this.HandleExprement(errorexp, nExprementId);
		}

		// Token: 0x060004DB RID: 1243 RVA: 0x0000958C File Offset: 0x0000778C
		public void visit(_IErrorExpression errorexp, int nExprementId)
		{
			this.HandleExprement(errorexp, nExprementId);
		}

		// Token: 0x060004DC RID: 1244 RVA: 0x00009598 File Offset: 0x00007798
		public void visit(_ICaseLabelStatement caselabel, int nExprementId)
		{
			this.HandleExprement(caselabel, nExprementId);
		}

		// Token: 0x060004DD RID: 1245 RVA: 0x000095A4 File Offset: 0x000077A4
		public void visit(_IEmptyStatement empty, int nExprementId)
		{
			this.HandleExprement(empty, nExprementId);
		}

		// Token: 0x060004DE RID: 1246 RVA: 0x000095B0 File Offset: 0x000077B0
		public void visit(_IGlobalScopeExpression globexp, int nExprementId)
		{
			this.HandleExprement(globexp, nExprementId);
		}

		// Token: 0x060004DF RID: 1247 RVA: 0x000095BC File Offset: 0x000077BC
		public void visit(_ICompoAccessExpression compo, int nExprementId)
		{
			this.HandleExprement(compo, nExprementId);
		}

		// Token: 0x060004E0 RID: 1248 RVA: 0x000095C8 File Offset: 0x000077C8
		public void visit(_IVariableExpression variable, int nExprementId)
		{
			this.HandleExprement(variable, nExprementId);
		}

		// Token: 0x060004E1 RID: 1249 RVA: 0x000095D4 File Offset: 0x000077D4
		public void visit(_ILiteralExpression literal, int nExprementId)
		{
			this.HandleExprement(literal, nExprementId);
		}

		// Token: 0x060004E2 RID: 1250 RVA: 0x000095E0 File Offset: 0x000077E0
		public void visit(_IThisExpression thisexp, int nExprementId)
		{
			this.HandleExprement(thisexp, nExprementId);
		}

		// Token: 0x060004E3 RID: 1251 RVA: 0x000095EC File Offset: 0x000077EC
		public void visit(_IOperatorExpression op, int nExprementId)
		{
			this.HandleExprement(op, nExprementId);
		}

		// Token: 0x060004E4 RID: 1252 RVA: 0x000095F8 File Offset: 0x000077F8
		public void visit(_IAssignmentExpression assign, int nExprementId)
		{
			this.HandleExprement(assign, nExprementId);
		}

		// Token: 0x060004E5 RID: 1253 RVA: 0x00009604 File Offset: 0x00007804
		public void visit(_IPragmaStatement pragma, int nExprementId)
		{
			this.HandleExprement(pragma, nExprementId);
		}

		// Token: 0x060004E6 RID: 1254 RVA: 0x00009610 File Offset: 0x00007810
		public void visit(_ILabelStatement label, int nExprementId)
		{
			this.HandleExprement(label, nExprementId);
		}

		// Token: 0x060004E7 RID: 1255 RVA: 0x0000961C File Offset: 0x0000781C
		public void visit(_IReturnStatement returnst, int nExprementId)
		{
			this.HandleExprement(returnst, nExprementId);
		}

		// Token: 0x060004E8 RID: 1256 RVA: 0x00009628 File Offset: 0x00007828
		public void visit(_IContinueStatement cont, int nExprementId)
		{
			this.HandleExprement(cont, nExprementId);
		}

		// Token: 0x060004E9 RID: 1257 RVA: 0x00009634 File Offset: 0x00007834
		public void visit(_IForStatement forloop, int nExprementId)
		{
			this.HandleExprement(forloop, nExprementId);
		}

		// Token: 0x060004EA RID: 1258 RVA: 0x00009640 File Offset: 0x00007840
		public void visit(_IWhileStatement whilst, int nExprementId)
		{
			this.HandleExprement(whilst, nExprementId);
		}

		// Token: 0x060004EB RID: 1259 RVA: 0x0000964C File Offset: 0x0000784C
		public void visit(_ISequenceStatement seq, int nExprementId)
		{
			this.HandleExprement(seq, nExprementId);
		}

		// Token: 0x060004EC RID: 1260 RVA: 0x00009658 File Offset: 0x00007858
		public void visit(_IBreakPointStatement bpstatement, int nExprementId)
		{
			this.HandleExprement(bpstatement, nExprementId);
		}

		// Token: 0x060004ED RID: 1261 RVA: 0x00009664 File Offset: 0x00007864
		public void visit(_IPartialAccessExpression partialAccessExpression, int nExprementId)
		{
			this.HandleExprement(partialAccessExpression, nExprementId);
		}

		// Token: 0x060004EE RID: 1262 RVA: 0x00009670 File Offset: 0x00007870
		public void visit(_IProjectDefinedExpression projectDefinedExpression, int nExprementId)
		{
			this.HandleExprement(projectDefinedExpression, nExprementId);
		}
	}
}
