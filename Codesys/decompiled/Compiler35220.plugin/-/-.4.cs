using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Compiler35220.TreeConversion;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0008
{
	// Token: 0x0200001A RID: 26
	internal sealed class \u0001 : IExprementVisitor352000, IExprementVisitor351900, IExprementVisitor351800, IExprementVisitor351500, IExprementVisitor351400, IExprementVisitor351300, IExprementVisitor3590, IExprementVisitor, IExprementVisitor2
	{
		// Token: 0x17000351 RID: 849
		// (get) Token: 0x060004F0 RID: 1264 RVA: 0x00009684 File Offset: 0x00007884
		// (set) Token: 0x060004F1 RID: 1265 RVA: 0x0000968C File Offset: 0x0000788C
		public IGreenTreeVisitor Visitor { get; set; }

		// Token: 0x060004F2 RID: 1266 RVA: 0x00009698 File Offset: 0x00007898
		public void \u0001(_ISequenceStatement \u0002)
		{
			this.Visitor.visit(\u0002, this.\u0001);
			this.\u0001++;
			for (int i = 0; i < \u0002._StatementList.Count; i++)
			{
				\u0002._StatementList[i].Accept(this);
			}
		}

		// Token: 0x060004F3 RID: 1267 RVA: 0x000096F0 File Offset: 0x000078F0
		public void \u0001(_IWhileStatement \u0002)
		{
			this.Visitor.visit(\u0002, this.\u0001);
			this.\u0001++;
			\u0002._Condition.Accept(this);
			\u0002._Controlled.Accept(this);
		}

		// Token: 0x060004F4 RID: 1268 RVA: 0x0000972C File Offset: 0x0000792C
		public void \u0001(_IRepeatStatement \u0002)
		{
			this.Visitor.visit(\u0002, this.\u0001);
			this.\u0001++;
			\u0002._Condition.Accept(this);
			\u0002._Controlled.Accept(this);
		}

		// Token: 0x060004F5 RID: 1269 RVA: 0x00009768 File Offset: 0x00007968
		public void \u0001(_IForStatement \u0002)
		{
			this.Visitor.visit(\u0002, this.\u0001);
			this.\u0001++;
			\u0002._CounterStart.Accept(this);
			\u0002._UpperBound.Accept(this);
			if (\u0002.By != null)
			{
				\u0002._By.Accept(this);
			}
			_IExpression condition = \u0002._Condition;
			if (condition != null)
			{
				condition.Accept(this);
			}
			_IExpression counter = \u0002._Counter;
			if (counter != null)
			{
				counter.Accept(this);
			}
			\u0002._Controlled.Accept(this);
		}

		// Token: 0x060004F6 RID: 1270 RVA: 0x000097F4 File Offset: 0x000079F4
		public void \u0001(_IExitStatement \u0002)
		{
			this.Visitor.visit(\u0002, this.\u0001);
			this.\u0001++;
		}

		// Token: 0x060004F7 RID: 1271 RVA: 0x00009818 File Offset: 0x00007A18
		public void \u0001(_IContinueStatement \u0002)
		{
			this.Visitor.visit(\u0002, this.\u0001);
			this.\u0001++;
		}

		// Token: 0x060004F8 RID: 1272 RVA: 0x0000983C File Offset: 0x00007A3C
		public void \u0001(_IIfStatement \u0002)
		{
			this.Visitor.visit(\u0002, this.\u0001);
			this.\u0001++;
			\u0002._Condition.Accept(this);
			\u0002._IfThen.Accept(this);
			foreach (_IElseIf ielseIf in \u0002._ElseIf)
			{
				this.\u0001++;
				ielseIf._Condition.Accept(this);
				ielseIf._Controlled.Accept(this);
			}
			if (\u0002._IfElse != null)
			{
				\u0002._IfElse.Accept(this);
			}
		}

		// Token: 0x060004F9 RID: 1273 RVA: 0x000098F4 File Offset: 0x00007AF4
		public void \u0001(_IReturnStatement \u0002)
		{
			this.Visitor.visit(\u0002, this.\u0001);
			this.\u0001++;
			if (\u0002._Condition != null)
			{
				\u0002._Condition.Accept(this);
			}
		}

		// Token: 0x060004FA RID: 1274 RVA: 0x0000992C File Offset: 0x00007B2C
		public void \u0001(_IJumpStatement \u0002)
		{
			this.Visitor.visit(\u0002, this.\u0001);
			this.\u0001++;
			if (\u0002._Condition != null)
			{
				\u0002._Condition.Accept(this);
			}
		}

		// Token: 0x060004FB RID: 1275 RVA: 0x00009964 File Offset: 0x00007B64
		public void \u0001(_ILabelStatement \u0002)
		{
			this.Visitor.visit(\u0002, this.\u0001);
			this.\u0001++;
		}

		// Token: 0x060004FC RID: 1276 RVA: 0x00009988 File Offset: 0x00007B88
		public void \u0001(_ICommentStatement \u0002)
		{
			this.Visitor.visit(\u0002, this.\u0001);
			this.\u0001++;
		}

		// Token: 0x060004FD RID: 1277 RVA: 0x000099AC File Offset: 0x00007BAC
		public void \u0001(_IPragmaStatement \u0002)
		{
			this.Visitor.visit(\u0002, this.\u0001);
			this.\u0001++;
		}

		// Token: 0x060004FE RID: 1278 RVA: 0x000099D0 File Offset: 0x00007BD0
		public void \u0001(_IExpressionStatement \u0002)
		{
			this.Visitor.visit(\u0002, this.\u0001);
			this.\u0001++;
			\u0002._Expr.Accept(this);
		}

		// Token: 0x060004FF RID: 1279 RVA: 0x00009A00 File Offset: 0x00007C00
		public void \u0001(_IAssignmentExpression \u0002)
		{
			this.Visitor.visit(\u0002, this.\u0001);
			this.\u0001++;
			\u0002._LValue.Accept(this);
			\u0002._RValue.Accept(this);
		}

		// Token: 0x06000500 RID: 1280 RVA: 0x00009A3C File Offset: 0x00007C3C
		public void \u0001(_ICallExpression \u0002)
		{
			this.Visitor.visit(\u0002, this.\u0001);
			this.\u0001++;
			IList<_IExpression> inputs = \u0002.Inputs;
			IList<_IExpression> paramExpressions = \u0002.ParamExpressions;
			IList<_IExpression> outputs = \u0002.Outputs;
			IList<_IExpression> outputExpressions = \u0002.OutputExpressions;
			IList<_IExpression> emptyAssigns = \u0002.EmptyAssigns;
			\u0002._Callee.Accept(this);
			if (\u0002._Condition != null)
			{
				\u0002._Condition.Accept(this);
			}
			for (int i = 0; i < paramExpressions.Count; i++)
			{
				paramExpressions[i].Accept(this);
			}
			for (int j = 0; j < inputs.Count; j++)
			{
				_IExpression iexpression = inputs[j];
				if (iexpression != null)
				{
					iexpression.Accept(this);
				}
			}
			for (int k = 0; k < outputs.Count; k++)
			{
				outputs[k].Accept(this);
			}
			for (int l = 0; l < outputExpressions.Count; l++)
			{
				if (\u0002.OutputExpressions[l] != null)
				{
					\u0002.OutputExpressions[l].Accept(this);
				}
			}
			for (int m = 0; m < \u0002.EmptyAssigns.Count; m++)
			{
				\u0002.EmptyAssigns[m].Accept(this);
			}
		}

		// Token: 0x06000501 RID: 1281 RVA: 0x00009B84 File Offset: 0x00007D84
		public void \u0001(_IOperatorExpression \u0002)
		{
			this.Visitor.visit(\u0002, this.\u0001);
			this.\u0001++;
			IList<_IExpression> operandsList = \u0002._OperandsList;
			for (int i = 0; i < operandsList.Count; i++)
			{
				operandsList[i].Accept(this);
			}
		}

		// Token: 0x06000502 RID: 1282 RVA: 0x00009BD8 File Offset: 0x00007DD8
		public void \u0001(_IConversionExpression \u0002)
		{
			this.Visitor.visit(\u0002, this.\u0001);
			this.\u0001++;
			\u0002._Exp.Accept(this);
		}

		// Token: 0x06000503 RID: 1283 RVA: 0x00009C08 File Offset: 0x00007E08
		public void \u0001(_IThisExpression \u0002)
		{
			this.Visitor.visit(\u0002, this.\u0001);
			this.\u0001++;
		}

		// Token: 0x06000504 RID: 1284 RVA: 0x00009C2C File Offset: 0x00007E2C
		public void \u0001(_IBaseExpression \u0002)
		{
			this.Visitor.visit(\u0002, this.\u0001);
			this.\u0001++;
		}

		// Token: 0x06000505 RID: 1285 RVA: 0x00009C50 File Offset: 0x00007E50
		public void \u0001(_ILiteralExpression \u0002)
		{
			this.Visitor.visit(\u0002, this.\u0001);
			this.\u0001++;
		}

		// Token: 0x06000506 RID: 1286 RVA: 0x00009C74 File Offset: 0x00007E74
		public void \u0001(_IAddressExpression \u0002)
		{
			this.Visitor.visit(\u0002, this.\u0001);
			this.\u0001++;
		}

		// Token: 0x06000507 RID: 1287 RVA: 0x00009C98 File Offset: 0x00007E98
		public void \u0001(_IVariableExpression \u0002)
		{
			this.Visitor.visit(\u0002, this.\u0001);
			this.\u0001++;
		}

		// Token: 0x06000508 RID: 1288 RVA: 0x00009CBC File Offset: 0x00007EBC
		public void \u0001(_IIndexAccessExpression \u0002)
		{
			this.Visitor.visit(\u0002, this.\u0001);
			this.\u0001++;
			\u0002._Var.Accept(this);
			foreach (_IExpression iexpression in \u0002._Accesses)
			{
				iexpression.Accept(this);
			}
		}

		// Token: 0x06000509 RID: 1289 RVA: 0x00009D34 File Offset: 0x00007F34
		public void \u0001(_ICompoAccessExpression \u0002)
		{
			this.Visitor.visit(\u0002, this.\u0001);
			this.\u0001++;
			\u0002._Left.Accept(this);
			\u0002._Right.Accept(this);
		}

		// Token: 0x0600050A RID: 1290 RVA: 0x00009D70 File Offset: 0x00007F70
		public void \u0001(_IDeRefAccessExpression \u0002)
		{
			this.Visitor.visit(\u0002, this.\u0001);
			this.\u0001++;
			\u0002._Base.Accept(this);
		}

		// Token: 0x0600050B RID: 1291 RVA: 0x00009DA0 File Offset: 0x00007FA0
		public void \u0001(_IGlobalScopeExpression \u0002)
		{
			this.Visitor.visit(\u0002, this.\u0001);
			this.\u0001++;
			\u0002._Base.Accept(this);
		}

		// Token: 0x0600050C RID: 1292 RVA: 0x00009DD0 File Offset: 0x00007FD0
		public void \u0001(_ISystemScopeExpression \u0002)
		{
			this.Visitor.visit(\u0002, this.\u0001);
			this.\u0001++;
			\u0002._Base.Accept(this);
		}

		// Token: 0x0600050D RID: 1293 RVA: 0x00009E00 File Offset: 0x00008000
		public void \u0001(_IEmptyStatement \u0002)
		{
			this.Visitor.visit(\u0002, this.\u0001);
			this.\u0001++;
		}

		// Token: 0x0600050E RID: 1294 RVA: 0x00009E24 File Offset: 0x00008024
		public void \u0001(_ICaseRangeExpression \u0002)
		{
			this.Visitor.visit(\u0002, this.\u0001);
			this.\u0001++;
			\u0002._High.Accept(this);
			\u0002._Low.Accept(this);
		}

		// Token: 0x0600050F RID: 1295 RVA: 0x00009E60 File Offset: 0x00008060
		public void \u0001(_ICaseLabelStatement \u0002)
		{
			this.Visitor.visit(\u0002, this.\u0001);
			this.\u0001++;
			for (int i = 0; i < \u0002._cases.Count; i++)
			{
				\u0002._cases[i].Accept(this);
			}
		}

		// Token: 0x06000510 RID: 1296 RVA: 0x00009EB8 File Offset: 0x000080B8
		public void \u0001(_ICaseStatement \u0002)
		{
			this.Visitor.visit(\u0002, this.\u0001);
			this.\u0001++;
			\u0002._Switch.Accept(this);
			foreach (_ICase icase in \u0002._Cases)
			{
				icase._Label.Accept(this);
				icase._Controlled.Accept(this);
			}
			if (\u0002._Else != null)
			{
				\u0002._Else.Accept(this);
			}
		}

		// Token: 0x06000511 RID: 1297 RVA: 0x00009F54 File Offset: 0x00008154
		public void \u0001(_IErrorExpression \u0002)
		{
			this.Visitor.visit(\u0002, this.\u0001);
			this.\u0001++;
		}

		// Token: 0x06000512 RID: 1298 RVA: 0x00009F78 File Offset: 0x00008178
		public void \u0001(_IErrorStatement \u0002)
		{
			this.Visitor.visit(\u0002, this.\u0001);
			this.\u0001++;
		}

		// Token: 0x06000513 RID: 1299 RVA: 0x00009F9C File Offset: 0x0000819C
		public void \u0001(_INullExpression \u0002)
		{
			this.Visitor.visit(\u0002, this.\u0001);
			this.\u0001++;
		}

		// Token: 0x06000514 RID: 1300 RVA: 0x00009FC0 File Offset: 0x000081C0
		public void \u0001(_INullStatement \u0002)
		{
			this.Visitor.visit(\u0002, this.\u0001);
			this.\u0001++;
		}

		// Token: 0x06000515 RID: 1301 RVA: 0x00009FE4 File Offset: 0x000081E4
		public void \u0001(_IMultipleIndexInitialization \u0002)
		{
			this.Visitor.visit(\u0002, this.\u0001);
			this.\u0001++;
			\u0002._Number.Accept(this);
			\u0002._Value.Accept(this);
		}

		// Token: 0x06000516 RID: 1302 RVA: 0x0000A020 File Offset: 0x00008220
		public void \u0001(_IArrayInitialization \u0002)
		{
			this.Visitor.visit(\u0002, this.\u0001);
			this.\u0001++;
			foreach (_IExpression iexpression in \u0002._InitValues)
			{
				iexpression.Accept(this);
			}
		}

		// Token: 0x06000517 RID: 1303 RVA: 0x0000A08C File Offset: 0x0000828C
		public void \u0001(_IStructureInitialization \u0002)
		{
			this.Visitor.visit(\u0002, this.\u0001);
			this.\u0001++;
			foreach (_IAssignmentExpression iassignmentExpression in \u0002._CompoInits)
			{
				iassignmentExpression.Accept(this);
			}
		}

		// Token: 0x06000518 RID: 1304 RVA: 0x0000A0F8 File Offset: 0x000082F8
		public void \u0001(_IDefineReference \u0002)
		{
			this.Visitor.visit(\u0002, this.\u0001);
			this.\u0001++;
		}

		// Token: 0x06000519 RID: 1305 RVA: 0x0000A11C File Offset: 0x0000831C
		public void \u0001(_IVariableReference \u0002)
		{
			this.Visitor.visit(\u0002, this.\u0001);
			this.\u0001++;
			\u0002.InstancePath.Accept(this);
		}

		// Token: 0x0600051A RID: 1306 RVA: 0x0000A14C File Offset: 0x0000834C
		public void \u0001(_ITypeReference \u0002)
		{
			this.Visitor.visit(\u0002, this.\u0001);
			this.\u0001++;
			\u0002.InstancePath.Accept(this);
		}

		// Token: 0x0600051B RID: 1307 RVA: 0x0000A17C File Offset: 0x0000837C
		public void \u0001(_IPouReference \u0002)
		{
			this.Visitor.visit(\u0002, this.\u0001);
			this.\u0001++;
			\u0002.InstancePath.Accept(this);
		}

		// Token: 0x0600051C RID: 1308 RVA: 0x0000A1AC File Offset: 0x000083AC
		public void \u0001(_ITaskReference \u0002)
		{
			this.Visitor.visit(\u0002, this.\u0001);
			this.\u0001++;
		}

		// Token: 0x0600051D RID: 1309 RVA: 0x0000A1D0 File Offset: 0x000083D0
		public void \u0001(_IResourceReference \u0002)
		{
			this.Visitor.visit(\u0002, this.\u0001);
			this.\u0001++;
		}

		// Token: 0x0600051E RID: 1310 RVA: 0x0000A1F4 File Offset: 0x000083F4
		public void \u0001(_IDefinedExpression \u0002)
		{
			this.Visitor.visit(\u0002, this.\u0001);
			this.\u0001++;
			\u0002.ItemReference.Accept(this);
		}

		// Token: 0x0600051F RID: 1311 RVA: 0x0000A224 File Offset: 0x00008424
		public void \u0001(_IXRefExpression \u0002)
		{
			this.Visitor.visit(\u0002, this.\u0001);
			this.\u0001++;
			\u0002.XRef.Accept(this);
			\u0002.XRefFrom.Accept(this);
		}

		// Token: 0x06000520 RID: 1312 RVA: 0x0000A260 File Offset: 0x00008460
		public void \u0001(_ICompilerVersionExpression \u0002)
		{
			this.Visitor.visit(\u0002, this.\u0001);
			this.\u0001++;
		}

		// Token: 0x06000521 RID: 1313 RVA: 0x0000A284 File Offset: 0x00008484
		public void \u0001(_IPragmaOperatorExpression \u0002)
		{
			this.Visitor.visit(\u0002, this.\u0001);
			this.\u0001++;
			foreach (_IExpression iexpression in \u0002.Operands)
			{
				iexpression.Accept(this);
			}
		}

		// Token: 0x06000522 RID: 1314 RVA: 0x0000A2F0 File Offset: 0x000084F0
		public void \u0001(_IPragmaIfStatement \u0002)
		{
			this.Visitor.visit(\u0002, this.\u0001);
			this.\u0001++;
			\u0002.Condition.Accept(this);
			\u0002.IfThen.Accept(this);
			foreach (_IPragmaElseIf ipragmaElseIf in \u0002.ElseIf)
			{
				ipragmaElseIf.Condition.Accept(this);
				ipragmaElseIf.Controlled.Accept(this);
			}
			if (\u0002.IfElse != null)
			{
				\u0002.IfElse.Accept(this);
			}
		}

		// Token: 0x06000523 RID: 1315 RVA: 0x0000A398 File Offset: 0x00008598
		public void \u0001(_IDefineStatement \u0002)
		{
			this.Visitor.visit(\u0002, this.\u0001);
			this.\u0001++;
		}

		// Token: 0x06000524 RID: 1316 RVA: 0x0000A3BC File Offset: 0x000085BC
		public void \u0001(_IHasCompatibleTypeExpression \u0002)
		{
			this.Visitor.visit(\u0002, this.\u0001);
			this.\u0001++;
			\u0002.Variable.Accept(this);
		}

		// Token: 0x06000525 RID: 1317 RVA: 0x0000A3EC File Offset: 0x000085EC
		public void \u0001(_IHasTypeExpression \u0002)
		{
			if (\u0002 is _IHasCompatibleTypeExpression)
			{
				this.\u0001((_IHasCompatibleTypeExpression)\u0002);
				return;
			}
			this.Visitor.visit(\u0002, this.\u0001);
			this.\u0001++;
			\u0002.Variable.Accept(this);
		}

		// Token: 0x06000526 RID: 1318 RVA: 0x0000A43C File Offset: 0x0000863C
		public void \u0001(_IIsEnumTypeExpression \u0002)
		{
			this.Visitor.visit(\u0002, this.\u0001);
			this.\u0001++;
		}

		// Token: 0x06000527 RID: 1319 RVA: 0x0000A460 File Offset: 0x00008660
		public void \u0001(_IHasAttributeExpression \u0002)
		{
			this.Visitor.visit(\u0002, this.\u0001);
			this.\u0001++;
			\u0002.ItemReference.Accept(this);
		}

		// Token: 0x06000528 RID: 1320 RVA: 0x0000A490 File Offset: 0x00008690
		public void \u0001(_IHasValueExpression \u0002)
		{
			this.Visitor.visit(\u0002, this.\u0001);
			this.\u0001++;
		}

		// Token: 0x06000529 RID: 1321 RVA: 0x0000A4B4 File Offset: 0x000086B4
		public void \u0001(_IHasConstantValueExpression \u0002)
		{
			this.Visitor.visit(\u0002, this.\u0001);
			this.\u0001++;
			_IExpression constant = \u0002._Constant;
			if (constant != null)
			{
				constant.Accept(this);
			}
			_IExpression constantValue = \u0002._ConstantValue;
			if (constantValue == null)
			{
				return;
			}
			constantValue.Accept(this);
		}

		// Token: 0x0600052A RID: 1322 RVA: 0x0000A504 File Offset: 0x00008704
		public void \u0001(_IHasConstantTypeExpression \u0002)
		{
			this.Visitor.visit(\u0002, this.\u0001);
			this.\u0001++;
			_IExpression constant = \u0002._Constant;
			if (constant == null)
			{
				return;
			}
			constant.Accept(this);
		}

		// Token: 0x0600052B RID: 1323 RVA: 0x0000A538 File Offset: 0x00008738
		public void \u0001(_IPragmaAssertion \u0002)
		{
			this.Visitor.visit(\u0002, this.\u0001);
			this.\u0001++;
			\u0002.Condition.Accept(this);
		}

		// Token: 0x0600052C RID: 1324 RVA: 0x0000A568 File Offset: 0x00008768
		public void \u0001(_ICastExpression \u0002)
		{
			this.Visitor.visit(\u0002, this.\u0001);
			this.\u0001++;
			\u0002.BaseExpression.Accept(this);
			if (\u0002.ExpWithType != null)
			{
				\u0002.ExpWithType.Accept(this);
			}
		}

		// Token: 0x0600052D RID: 1325 RVA: 0x0000A5B8 File Offset: 0x000087B8
		public void \u0001(_INewExpression \u0002)
		{
			this.Visitor.visit(\u0002, this.\u0001);
			this.\u0001++;
			if (\u0002._Count != null)
			{
				\u0002._Count.Accept(this);
			}
			if (\u0002._FBInitParams != null)
			{
				foreach (IAssignmentExpression assignmentExpression in \u0002._FBInitParams)
				{
					((_IAssignmentExpression)assignmentExpression).Accept(this);
				}
			}
		}

		// Token: 0x0600052E RID: 1326 RVA: 0x0000A644 File Offset: 0x00008844
		public void \u0001(_ITypeExpression \u0002)
		{
			this.Visitor.visit(\u0002, this.\u0001);
			this.\u0001++;
		}

		// Token: 0x0600052F RID: 1327 RVA: 0x0000A668 File Offset: 0x00008868
		public void \u0001(_INamespaceAccessExpression \u0002)
		{
			this.Visitor.visit(\u0002, this.\u0001);
			this.\u0001++;
			\u0002._Access.Accept(this);
			\u0002._Namespace.Accept(this);
		}

		// Token: 0x06000530 RID: 1328 RVA: 0x0000A6A4 File Offset: 0x000088A4
		public void \u0001(_IRuntimeVersionExpression \u0002)
		{
			this.Visitor.visit(\u0002, this.\u0001);
			this.\u0001++;
		}

		// Token: 0x06000531 RID: 1329 RVA: 0x0000A6C8 File Offset: 0x000088C8
		public void \u0001(_ICurrentTaskExpression \u0002)
		{
			this.Visitor.visit(\u0002, this.\u0001);
			this.\u0001++;
			\u0002._Base.Accept(this);
		}

		// Token: 0x06000532 RID: 1330 RVA: 0x0000A6F8 File Offset: 0x000088F8
		public void \u0001(_IPoolScopeExpression \u0002)
		{
			this.Visitor.visit(\u0002, this.\u0001);
			this.\u0001++;
			\u0002._Base.Accept(this);
		}

		// Token: 0x06000533 RID: 1331 RVA: 0x0000A728 File Offset: 0x00008928
		public void \u0001(_ITryCatchStatement \u0002)
		{
			this.Visitor.visit(\u0002, this.\u0001);
			this.\u0001++;
			\u0002._Try.Accept(this);
			if (\u0002._Catch != null)
			{
				\u0002._Catch.Accept(this);
			}
			if (\u0002._Exception != null)
			{
				\u0002._Exception.Accept(this);
			}
			if (\u0002._Finally != null)
			{
				\u0002._Finally.Accept(this);
			}
		}

		// Token: 0x06000534 RID: 1332 RVA: 0x0000A7A0 File Offset: 0x000089A0
		public void \u0001(_IBreakPointStatement \u0002)
		{
			this.Visitor.visit(\u0002, this.\u0001);
			this.\u0001++;
		}

		// Token: 0x06000535 RID: 1333 RVA: 0x0000A7C4 File Offset: 0x000089C4
		public void \u0001(_IPartialAccessExpression \u0002)
		{
			this.Visitor.visit(\u0002, this.\u0001);
			this.\u0001++;
			\u0002._Left.Accept(this);
		}

		// Token: 0x06000536 RID: 1334 RVA: 0x0000A7F4 File Offset: 0x000089F4
		public void \u0001(_IProjectDefinedExpression \u0002)
		{
			this.Visitor.visit(\u0002, this.\u0001);
			this.\u0001++;
			_IDefineReference defineReference = \u0002.DefineReference;
			if (defineReference == null)
			{
				return;
			}
			defineReference.Accept(this);
		}

		// Token: 0x06000537 RID: 1335 RVA: 0x0000A828 File Offset: 0x00008A28
		public void \u0001(_IPOUDeclarationStatement \u0002)
		{
		}

		// Token: 0x06000538 RID: 1336 RVA: 0x0000A82C File Offset: 0x00008A2C
		public void \u0001(_ICompiledPOU \u0002)
		{
			throw new NotImplementedException();
		}

		// Token: 0x06000539 RID: 1337 RVA: 0x0000A834 File Offset: 0x00008A34
		public void \u0001(_ICopyScopeExpression \u0002)
		{
			throw new NotImplementedException();
		}

		// Token: 0x0600053A RID: 1338 RVA: 0x0000A83C File Offset: 0x00008A3C
		public void \u0001(_IQualifiedNameExpression \u0002)
		{
			throw new NotImplementedException();
		}

		// Token: 0x0600053B RID: 1339 RVA: 0x0000A844 File Offset: 0x00008A44
		public void \u0001(_IVariableDeclarationStatement \u0002)
		{
			throw new NotImplementedException();
		}

		// Token: 0x0600053C RID: 1340 RVA: 0x0000A84C File Offset: 0x00008A4C
		public void \u0001(_IVariableDeclarationListStatement \u0002)
		{
			throw new NotImplementedException();
		}

		// Token: 0x0600053D RID: 1341 RVA: 0x0000A854 File Offset: 0x00008A54
		public void \u0001(_ITypeDeclarationStatement \u0002)
		{
			throw new NotImplementedException();
		}

		// Token: 0x0600053E RID: 1342 RVA: 0x0000A85C File Offset: 0x00008A5C
		public void \u0001(_IEnumDeclarationStatement \u0002)
		{
			throw new NotImplementedException();
		}

		// Token: 0x0600053F RID: 1343 RVA: 0x0000A864 File Offset: 0x00008A64
		public void \u0001(_IEnumDeclarationListStatement \u0002)
		{
			throw new NotImplementedException();
		}

		// Token: 0x0400003E RID: 62
		private int \u0001;

		// Token: 0x0400003F RID: 63
		[CompilerGenerated]
		private IGreenTreeVisitor \u0001;
	}
}
