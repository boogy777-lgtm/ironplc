using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0008
{
	// Token: 0x02000020 RID: 32
	internal sealed class \u0002 : IExprementVisitor352000, IExprementVisitor351900, IExprementVisitor351800, IExprementVisitor351500, IExprementVisitor351400, IExprementVisitor351300, IExprementVisitor3590, IExprementVisitor
	{
		// Token: 0x0600055C RID: 1372 RVA: 0x0000B0B4 File Offset: 0x000092B4
		internal static bool \u0001(_IExprement \u0002)
		{
			if (\u0002 == null)
			{
				return true;
			}
			bool result;
			try
			{
				\u0002.Accept(new global::\u0008.\u0002());
				result = true;
			}
			catch (MaximumNestingDepthExceededException)
			{
				result = false;
			}
			return result;
		}

		// Token: 0x0600055D RID: 1373 RVA: 0x0000B0EC File Offset: 0x000092EC
		private void \u0001(Action \u0002)
		{
			this.\u0002++;
			if (this.\u0002 > this.\u0001)
			{
				throw new MaximumNestingDepthExceededException();
			}
			\u0002();
			this.\u0002--;
		}

		// Token: 0x0600055E RID: 1374 RVA: 0x0000B124 File Offset: 0x00009324
		public void \u0001(_IAddressExpression \u0002)
		{
		}

		// Token: 0x0600055F RID: 1375 RVA: 0x0000B128 File Offset: 0x00009328
		public void \u0001(_IVariableExpression \u0002)
		{
		}

		// Token: 0x06000560 RID: 1376 RVA: 0x0000B12C File Offset: 0x0000932C
		public void \u0001(_ITypeExpression \u0002)
		{
		}

		// Token: 0x06000561 RID: 1377 RVA: 0x0000B130 File Offset: 0x00009330
		public void \u0001(_ILiteralExpression \u0002)
		{
		}

		// Token: 0x06000562 RID: 1378 RVA: 0x0000B134 File Offset: 0x00009334
		public void \u0001(_IExitStatement \u0002)
		{
		}

		// Token: 0x06000563 RID: 1379 RVA: 0x0000B138 File Offset: 0x00009338
		public void \u0001(_IContinueStatement \u0002)
		{
		}

		// Token: 0x06000564 RID: 1380 RVA: 0x0000B13C File Offset: 0x0000933C
		public void \u0001(_IThisExpression \u0002)
		{
		}

		// Token: 0x06000565 RID: 1381 RVA: 0x0000B140 File Offset: 0x00009340
		public void \u0001(_IBaseExpression \u0002)
		{
		}

		// Token: 0x06000566 RID: 1382 RVA: 0x0000B144 File Offset: 0x00009344
		public void \u0001(_IEmptyStatement \u0002)
		{
		}

		// Token: 0x06000567 RID: 1383 RVA: 0x0000B148 File Offset: 0x00009348
		public void \u0001(_ILabelStatement \u0002)
		{
		}

		// Token: 0x06000568 RID: 1384 RVA: 0x0000B14C File Offset: 0x0000934C
		public void \u0001(_ICommentStatement \u0002)
		{
		}

		// Token: 0x06000569 RID: 1385 RVA: 0x0000B150 File Offset: 0x00009350
		public void \u0001(_IPragmaStatement \u0002)
		{
		}

		// Token: 0x0600056A RID: 1386 RVA: 0x0000B154 File Offset: 0x00009354
		public void \u0001(_IErrorExpression \u0002)
		{
		}

		// Token: 0x0600056B RID: 1387 RVA: 0x0000B158 File Offset: 0x00009358
		public void \u0001(_IErrorStatement \u0002)
		{
		}

		// Token: 0x0600056C RID: 1388 RVA: 0x0000B15C File Offset: 0x0000935C
		public void \u0001(_INullExpression \u0002)
		{
		}

		// Token: 0x0600056D RID: 1389 RVA: 0x0000B160 File Offset: 0x00009360
		public void \u0001(_INullStatement \u0002)
		{
		}

		// Token: 0x0600056E RID: 1390 RVA: 0x0000B164 File Offset: 0x00009364
		public void \u0001(_IQualifiedNameExpression \u0002)
		{
		}

		// Token: 0x0600056F RID: 1391 RVA: 0x0000B168 File Offset: 0x00009368
		public void \u0001(_IDefineReference \u0002)
		{
		}

		// Token: 0x06000570 RID: 1392 RVA: 0x0000B16C File Offset: 0x0000936C
		public void \u0001(_ITaskReference \u0002)
		{
		}

		// Token: 0x06000571 RID: 1393 RVA: 0x0000B170 File Offset: 0x00009370
		public void \u0001(_IResourceReference \u0002)
		{
		}

		// Token: 0x06000572 RID: 1394 RVA: 0x0000B174 File Offset: 0x00009374
		public void \u0001(_IBreakPointStatement \u0002)
		{
		}

		// Token: 0x06000573 RID: 1395 RVA: 0x0000B178 File Offset: 0x00009378
		public void \u0001(_IDefineStatement \u0002)
		{
		}

		// Token: 0x06000574 RID: 1396 RVA: 0x0000B17C File Offset: 0x0000937C
		public void \u0001(_IIsEnumTypeExpression \u0002)
		{
		}

		// Token: 0x06000575 RID: 1397 RVA: 0x0000B180 File Offset: 0x00009380
		public void \u0001(_IHasValueExpression \u0002)
		{
		}

		// Token: 0x06000576 RID: 1398 RVA: 0x0000B184 File Offset: 0x00009384
		public void \u0001(_ICompilerVersionExpression \u0002)
		{
		}

		// Token: 0x06000577 RID: 1399 RVA: 0x0000B188 File Offset: 0x00009388
		public void \u0001(_IRuntimeVersionExpression \u0002)
		{
		}

		// Token: 0x06000578 RID: 1400 RVA: 0x0000B18C File Offset: 0x0000938C
		public void \u0001(_ICompiledPOU \u0002)
		{
			global::\u0008.\u0002.\u0001 u = new global::\u0008.\u0002.\u0001();
			u.\u0001 = \u0002;
			u.\u0001 = this;
			this.\u0001(new Action(u.\u0001));
		}

		// Token: 0x06000579 RID: 1401 RVA: 0x0000B1C0 File Offset: 0x000093C0
		public void \u0001(_IWhileStatement \u0002)
		{
			global::\u0008.\u0002.\u0002 u = new global::\u0008.\u0002.\u0002();
			u.\u0001 = \u0002;
			u.\u0001 = this;
			this.\u0001(new Action(u.\u0001));
		}

		// Token: 0x0600057A RID: 1402 RVA: 0x0000B1F4 File Offset: 0x000093F4
		public void \u0001(_IRepeatStatement \u0002)
		{
			global::\u0008.\u0002.\u0003 u = new global::\u0008.\u0002.\u0003();
			u.\u0001 = \u0002;
			u.\u0001 = this;
			this.\u0001(new Action(u.\u0001));
		}

		// Token: 0x0600057B RID: 1403 RVA: 0x0000B228 File Offset: 0x00009428
		public void \u0001(_IForStatement \u0002)
		{
			global::\u0008.\u0002.\u0004 u = new global::\u0008.\u0002.\u0004();
			u.\u0001 = \u0002;
			u.\u0001 = this;
			this.\u0001(new Action(u.\u0001));
		}

		// Token: 0x0600057C RID: 1404 RVA: 0x0000B25C File Offset: 0x0000945C
		public void \u0001(_ISequenceStatement \u0002)
		{
			global::\u0008.\u0002.\u0005 u = new global::\u0008.\u0002.\u0005();
			u.\u0001 = \u0002;
			u.\u0001 = this;
			this.\u0001(new Action(u.\u0001));
		}

		// Token: 0x0600057D RID: 1405 RVA: 0x0000B290 File Offset: 0x00009490
		public void \u0001(_IIfStatement \u0002)
		{
			global::\u0008.\u0002.\u0006 u = new global::\u0008.\u0002.\u0006();
			u.\u0001 = \u0002;
			u.\u0001 = this;
			this.\u0001(new Action(u.\u0001));
		}

		// Token: 0x0600057E RID: 1406 RVA: 0x0000B2C4 File Offset: 0x000094C4
		public void \u0001(_IExpressionStatement \u0002)
		{
			global::\u0008.\u0002.\u0007 u = new global::\u0008.\u0002.\u0007();
			u.\u0001 = \u0002;
			u.\u0001 = this;
			this.\u0001(new Action(u.\u0001));
		}

		// Token: 0x0600057F RID: 1407 RVA: 0x0000B2F8 File Offset: 0x000094F8
		public void \u0001(_IAssignmentExpression \u0002)
		{
			global::\u0008.\u0002.\u0008 u = new global::\u0008.\u0002.\u0008();
			u.\u0001 = \u0002;
			u.\u0001 = this;
			this.\u0001(new Action(u.\u0001));
		}

		// Token: 0x06000580 RID: 1408 RVA: 0x0000B32C File Offset: 0x0000952C
		public void \u0001(_ICallExpression \u0002)
		{
			global::\u0008.\u0002.\u000E u000E = new global::\u0008.\u0002.\u000E();
			u000E.\u0001 = \u0002;
			u000E.\u0001 = this;
			this.\u0001(new Action(u000E.\u0001));
		}

		// Token: 0x06000581 RID: 1409 RVA: 0x0000B360 File Offset: 0x00009560
		public void \u0001(_IOperatorExpression \u0002)
		{
			global::\u0008.\u0002.\u000F u000F = new global::\u0008.\u0002.\u000F();
			u000F.\u0001 = \u0002;
			u000F.\u0001 = this;
			this.\u0001(new Action(u000F.\u0001));
		}

		// Token: 0x06000582 RID: 1410 RVA: 0x0000B394 File Offset: 0x00009594
		public void \u0001(_ICastExpression \u0002)
		{
			global::\u0008.\u0002.\u0010 u = new global::\u0008.\u0002.\u0010();
			u.\u0001 = \u0002;
			u.\u0001 = this;
			this.\u0001(new Action(u.\u0001));
		}

		// Token: 0x06000583 RID: 1411 RVA: 0x0000B3C8 File Offset: 0x000095C8
		public void \u0001(_INewExpression \u0002)
		{
			global::\u0008.\u0002.\u0011 u = new global::\u0008.\u0002.\u0011();
			u.\u0001 = \u0002;
			u.\u0001 = this;
			this.\u0001(new Action(u.\u0001));
		}

		// Token: 0x06000584 RID: 1412 RVA: 0x0000B3FC File Offset: 0x000095FC
		public void \u0001(_IConversionExpression \u0002)
		{
			global::\u0008.\u0002.\u0012 u = new global::\u0008.\u0002.\u0012();
			u.\u0001 = \u0002;
			u.\u0001 = this;
			this.\u0001(new Action(u.\u0001));
		}

		// Token: 0x06000585 RID: 1413 RVA: 0x0000B430 File Offset: 0x00009630
		public void \u0001(_IIndexAccessExpression \u0002)
		{
			global::\u0008.\u0002.\u0013 u = new global::\u0008.\u0002.\u0013();
			u.\u0001 = \u0002;
			u.\u0001 = this;
			this.\u0001(new Action(u.\u0001));
		}

		// Token: 0x06000586 RID: 1414 RVA: 0x0000B464 File Offset: 0x00009664
		public void \u0001(_ICompoAccessExpression \u0002)
		{
			global::\u0008.\u0002.\u0014 u = new global::\u0008.\u0002.\u0014();
			u.\u0001 = \u0002;
			u.\u0001 = this;
			this.\u0001(new Action(u.\u0001));
		}

		// Token: 0x06000587 RID: 1415 RVA: 0x0000B498 File Offset: 0x00009698
		public void \u0001(_IDeRefAccessExpression \u0002)
		{
			global::\u0008.\u0002.\u0015 u = new global::\u0008.\u0002.\u0015();
			u.\u0001 = \u0002;
			u.\u0001 = this;
			this.\u0001(new Action(u.\u0001));
		}

		// Token: 0x06000588 RID: 1416 RVA: 0x0000B4CC File Offset: 0x000096CC
		public void \u0001(_ICopyScopeExpression \u0002)
		{
			global::\u0008.\u0002.\u0016 u = new global::\u0008.\u0002.\u0016();
			u.\u0001 = \u0002;
			u.\u0001 = this;
			this.\u0001(new Action(u.\u0001));
		}

		// Token: 0x06000589 RID: 1417 RVA: 0x0000B500 File Offset: 0x00009700
		public void \u0001(_IGlobalScopeExpression \u0002)
		{
			global::\u0008.\u0002.\u0017 u = new global::\u0008.\u0002.\u0017();
			u.\u0001 = \u0002;
			u.\u0001 = this;
			this.\u0001(new Action(u.\u0001));
		}

		// Token: 0x0600058A RID: 1418 RVA: 0x0000B534 File Offset: 0x00009734
		public void \u0001(_ISystemScopeExpression \u0002)
		{
			global::\u0008.\u0002.\u0018 u = new global::\u0008.\u0002.\u0018();
			u.\u0001 = \u0002;
			u.\u0001 = this;
			this.\u0001(new Action(u.\u0001));
		}

		// Token: 0x0600058B RID: 1419 RVA: 0x0000B568 File Offset: 0x00009768
		public void \u0001(_IPoolScopeExpression \u0002)
		{
			global::\u0008.\u0002.\u0019 u = new global::\u0008.\u0002.\u0019();
			u.\u0001 = \u0002;
			u.\u0001 = this;
			this.\u0001(new Action(u.\u0001));
		}

		// Token: 0x0600058C RID: 1420 RVA: 0x0000B59C File Offset: 0x0000979C
		public void \u0001(_INamespaceAccessExpression \u0002)
		{
			global::\u0008.\u0002.\u001A u001A = new global::\u0008.\u0002.\u001A();
			u001A.\u0001 = \u0002;
			u001A.\u0001 = this;
			this.\u0001(new Action(u001A.\u0001));
		}

		// Token: 0x0600058D RID: 1421 RVA: 0x0000B5D0 File Offset: 0x000097D0
		public void \u0001(_ICurrentTaskExpression \u0002)
		{
			global::\u0008.\u0002.\u001B u001B = new global::\u0008.\u0002.\u001B();
			u001B.\u0001 = \u0002;
			u001B.\u0001 = this;
			this.\u0001(new Action(u001B.\u0001));
		}

		// Token: 0x0600058E RID: 1422 RVA: 0x0000B604 File Offset: 0x00009804
		public void \u0001(_ICaseRangeExpression \u0002)
		{
			global::\u0008.\u0002.\u001C u001C = new global::\u0008.\u0002.\u001C();
			u001C.\u0001 = \u0002;
			u001C.\u0001 = this;
			this.\u0001(new Action(u001C.\u0001));
		}

		// Token: 0x0600058F RID: 1423 RVA: 0x0000B638 File Offset: 0x00009838
		public void \u0001(_ICaseLabelStatement \u0002)
		{
			global::\u0008.\u0002.\u001D u001D = new global::\u0008.\u0002.\u001D();
			u001D.\u0001 = \u0002;
			u001D.\u0001 = this;
			this.\u0001(new Action(u001D.\u0001));
		}

		// Token: 0x06000590 RID: 1424 RVA: 0x0000B66C File Offset: 0x0000986C
		public void \u0001(_ICaseStatement \u0002)
		{
			global::\u0008.\u0002.\u001E u001E = new global::\u0008.\u0002.\u001E();
			u001E.\u0001 = \u0002;
			u001E.\u0001 = this;
			this.\u0001(new Action(u001E.\u0001));
		}

		// Token: 0x06000591 RID: 1425 RVA: 0x0000B6A0 File Offset: 0x000098A0
		public void \u0001(_IReturnStatement \u0002)
		{
			global::\u0008.\u0002.\u001F u001F = new global::\u0008.\u0002.\u001F();
			u001F.\u0001 = \u0002;
			u001F.\u0001 = this;
			this.\u0001(new Action(u001F.\u0001));
		}

		// Token: 0x06000592 RID: 1426 RVA: 0x0000B6D4 File Offset: 0x000098D4
		public void \u0001(_IJumpStatement \u0002)
		{
			global::\u0008.\u0002.\u007F u007F = new global::\u0008.\u0002.\u007F();
			u007F.\u0001 = \u0002;
			u007F.\u0001 = this;
			this.\u0001(new Action(u007F.\u0001));
		}

		// Token: 0x06000593 RID: 1427 RVA: 0x0000B708 File Offset: 0x00009908
		public void \u0001(_IVariableDeclarationStatement \u0002)
		{
			global::\u0008.\u0002.\u0080 u = new global::\u0008.\u0002.\u0080();
			u.\u0001 = \u0002;
			u.\u0001 = this;
			this.\u0001(new Action(u.\u0001));
		}

		// Token: 0x06000594 RID: 1428 RVA: 0x0000B73C File Offset: 0x0000993C
		public void \u0001(_IVariableDeclarationListStatement \u0002)
		{
			global::\u0008.\u0002.\u0081 u = new global::\u0008.\u0002.\u0081();
			u.\u0001 = \u0002;
			u.\u0001 = this;
			this.\u0001(new Action(u.\u0001));
		}

		// Token: 0x06000595 RID: 1429 RVA: 0x0000B770 File Offset: 0x00009970
		public void \u0001(_IPOUDeclarationStatement \u0002)
		{
			global::\u0008.\u0002.\u0082 u = new global::\u0008.\u0002.\u0082();
			u.\u0001 = \u0002;
			u.\u0001 = this;
			this.\u0001(new Action(u.\u0001));
		}

		// Token: 0x06000596 RID: 1430 RVA: 0x0000B7A4 File Offset: 0x000099A4
		public void \u0001(_ITypeDeclarationStatement \u0002)
		{
			global::\u0008.\u0002.\u0083 u = new global::\u0008.\u0002.\u0083();
			u.\u0001 = \u0002;
			u.\u0001 = this;
			this.\u0001(new Action(u.\u0001));
		}

		// Token: 0x06000597 RID: 1431 RVA: 0x0000B7D8 File Offset: 0x000099D8
		public void \u0001(_IEnumDeclarationStatement \u0002)
		{
			global::\u0008.\u0002.\u0084 u = new global::\u0008.\u0002.\u0084();
			u.\u0001 = \u0002;
			u.\u0001 = this;
			this.\u0001(new Action(u.\u0001));
		}

		// Token: 0x06000598 RID: 1432 RVA: 0x0000B80C File Offset: 0x00009A0C
		public void \u0001(_IEnumDeclarationListStatement \u0002)
		{
			global::\u0008.\u0002.\u0086 u = new global::\u0008.\u0002.\u0086();
			u.\u0001 = \u0002;
			u.\u0001 = this;
			this.\u0001(new Action(u.\u0001));
		}

		// Token: 0x06000599 RID: 1433 RVA: 0x0000B840 File Offset: 0x00009A40
		public void \u0001(_IMultipleIndexInitialization \u0002)
		{
			global::\u0008.\u0002.\u0087 u = new global::\u0008.\u0002.\u0087();
			u.\u0001 = \u0002;
			u.\u0001 = this;
			this.\u0001(new Action(u.\u0001));
		}

		// Token: 0x0600059A RID: 1434 RVA: 0x0000B874 File Offset: 0x00009A74
		public void \u0001(_IArrayInitialization \u0002)
		{
			global::\u0008.\u0002.\u0088 u = new global::\u0008.\u0002.\u0088();
			u.\u0001 = \u0002;
			u.\u0001 = this;
			this.\u0001(new Action(u.\u0001));
		}

		// Token: 0x0600059B RID: 1435 RVA: 0x0000B8A8 File Offset: 0x00009AA8
		public void \u0001(_IStructureInitialization \u0002)
		{
			global::\u0008.\u0002.\u0089 u = new global::\u0008.\u0002.\u0089();
			u.\u0001 = \u0002;
			u.\u0001 = this;
			this.\u0001(new Action(u.\u0001));
		}

		// Token: 0x0600059C RID: 1436 RVA: 0x0000B8DC File Offset: 0x00009ADC
		public void \u0001(_IVariableReference \u0002)
		{
			global::\u0008.\u0002.\u008A u008A = new global::\u0008.\u0002.\u008A();
			u008A.\u0001 = \u0002;
			u008A.\u0001 = this;
			this.\u0001(new Action(u008A.\u0001));
		}

		// Token: 0x0600059D RID: 1437 RVA: 0x0000B910 File Offset: 0x00009B10
		public void \u0001(_ITypeReference \u0002)
		{
			global::\u0008.\u0002.\u008B u008B = new global::\u0008.\u0002.\u008B();
			u008B.\u0001 = \u0002;
			u008B.\u0001 = this;
			this.\u0001(new Action(u008B.\u0001));
		}

		// Token: 0x0600059E RID: 1438 RVA: 0x0000B944 File Offset: 0x00009B44
		public void \u0001(_IPouReference \u0002)
		{
			global::\u0008.\u0002.\u008C u008C = new global::\u0008.\u0002.\u008C();
			u008C.\u0001 = \u0002;
			u008C.\u0001 = this;
			this.\u0001(new Action(u008C.\u0001));
		}

		// Token: 0x0600059F RID: 1439 RVA: 0x0000B978 File Offset: 0x00009B78
		public void \u0001(_IDefinedExpression \u0002)
		{
			global::\u0008.\u0002.\u008D u008D = new global::\u0008.\u0002.\u008D();
			u008D.\u0001 = \u0002;
			u008D.\u0001 = this;
			this.\u0001(new Action(u008D.\u0001));
		}

		// Token: 0x060005A0 RID: 1440 RVA: 0x0000B9AC File Offset: 0x00009BAC
		public void \u0001(_IPragmaOperatorExpression \u0002)
		{
			global::\u0008.\u0002.\u008E u008E = new global::\u0008.\u0002.\u008E();
			u008E.\u0001 = \u0002;
			u008E.\u0001 = this;
			this.\u0001(new Action(u008E.\u0001));
		}

		// Token: 0x060005A1 RID: 1441 RVA: 0x0000B9E0 File Offset: 0x00009BE0
		public void \u0001(_IPragmaIfStatement \u0002)
		{
			global::\u0008.\u0002.\u008F u008F = new global::\u0008.\u0002.\u008F();
			u008F.\u0001 = \u0002;
			u008F.\u0001 = this;
			this.\u0001(new Action(u008F.\u0001));
		}

		// Token: 0x060005A2 RID: 1442 RVA: 0x0000BA14 File Offset: 0x00009C14
		public void \u0001(_IXRefExpression \u0002)
		{
			global::\u0008.\u0002.\u0090 u = new global::\u0008.\u0002.\u0090();
			u.\u0001 = \u0002;
			u.\u0001 = this;
			this.\u0001(new Action(u.\u0001));
		}

		// Token: 0x060005A3 RID: 1443 RVA: 0x0000BA48 File Offset: 0x00009C48
		public void \u0001(_IHasTypeExpression \u0002)
		{
			global::\u0008.\u0002.\u0091 u = new global::\u0008.\u0002.\u0091();
			u.\u0001 = \u0002;
			u.\u0001 = this;
			this.\u0001(new Action(u.\u0001));
		}

		// Token: 0x060005A4 RID: 1444 RVA: 0x0000BA7C File Offset: 0x00009C7C
		public void \u0001(_IHasAttributeExpression \u0002)
		{
			global::\u0008.\u0002.\u0092 u = new global::\u0008.\u0002.\u0092();
			u.\u0001 = \u0002;
			u.\u0001 = this;
			this.\u0001(new Action(u.\u0001));
		}

		// Token: 0x060005A5 RID: 1445 RVA: 0x0000BAB0 File Offset: 0x00009CB0
		public void \u0001(_IHasConstantValueExpression \u0002)
		{
			global::\u0008.\u0002.\u0093 u = new global::\u0008.\u0002.\u0093();
			u.\u0001 = \u0002;
			u.\u0001 = this;
			this.\u0001(new Action(u.\u0001));
		}

		// Token: 0x060005A6 RID: 1446 RVA: 0x0000BAE4 File Offset: 0x00009CE4
		public void \u0001(_IHasConstantTypeExpression \u0002)
		{
			global::\u0008.\u0002.\u0094 u = new global::\u0008.\u0002.\u0094();
			u.\u0001 = \u0002;
			u.\u0001 = this;
			this.\u0001(new Action(u.\u0001));
		}

		// Token: 0x060005A7 RID: 1447 RVA: 0x0000BB18 File Offset: 0x00009D18
		public void \u0001(_IPragmaAssertion \u0002)
		{
			global::\u0008.\u0002.\u0095 u = new global::\u0008.\u0002.\u0095();
			u.\u0001 = \u0002;
			u.\u0001 = this;
			this.\u0001(new Action(u.\u0001));
		}

		// Token: 0x060005A8 RID: 1448 RVA: 0x0000BB4C File Offset: 0x00009D4C
		public void \u0001(_IPartialAccessExpression \u0002)
		{
			global::\u0008.\u0002.\u0096 u = new global::\u0008.\u0002.\u0096();
			u.\u0001 = \u0002;
			u.\u0001 = this;
			this.\u0001(new Action(u.\u0001));
		}

		// Token: 0x060005A9 RID: 1449 RVA: 0x0000BB80 File Offset: 0x00009D80
		public void \u0001(_IProjectDefinedExpression \u0002)
		{
			global::\u0008.\u0002.\u0097 u = new global::\u0008.\u0002.\u0097();
			u.\u0001 = \u0002;
			u.\u0001 = this;
			this.\u0001(new Action(u.\u0001));
		}

		// Token: 0x060005AA RID: 1450 RVA: 0x0000BBB4 File Offset: 0x00009DB4
		private void \u0002(_ICallExpression \u0002)
		{
			if (\u0002.InputAssigns != null)
			{
				for (int i = 0; i < \u0002.InputAssigns.Length; i++)
				{
					_IAssignmentExpression iassignmentExpression = \u0002.InputAssigns[i] as _IAssignmentExpression;
					if (iassignmentExpression != null)
					{
						iassignmentExpression.Accept(this);
					}
				}
				return;
			}
			foreach (_IExpression iexpression in \u0002.ParamExpressions)
			{
				if (iexpression != null)
				{
					iexpression.Accept(this);
				}
			}
		}

		// Token: 0x04000048 RID: 72
		private readonly int \u0001 = 5000;

		// Token: 0x04000049 RID: 73
		private int \u0002;

		// Token: 0x02000021 RID: 33
		[CompilerGenerated]
		private sealed class \u0001
		{
			// Token: 0x060005AD RID: 1453 RVA: 0x0000BC58 File Offset: 0x00009E58
			internal void \u0001()
			{
				if (this.\u0001.GetFlag(CompiledPOUFlags.ContainsDirVarAccess))
				{
					this.\u0001.GetParseTree().Accept(this.\u0001);
				}
			}

			// Token: 0x0400004A RID: 74
			public _ICompiledPOU \u0001;

			// Token: 0x0400004B RID: 75
			public global::\u0008.\u0002 \u0001;
		}

		// Token: 0x02000022 RID: 34
		[CompilerGenerated]
		private sealed class \u0002
		{
			// Token: 0x060005AF RID: 1455 RVA: 0x0000BC8C File Offset: 0x00009E8C
			internal void \u0001()
			{
				this.\u0001._Condition.Accept(this.\u0001);
				this.\u0001._Controlled.Accept(this.\u0001);
			}

			// Token: 0x0400004C RID: 76
			public _IWhileStatement \u0001;

			// Token: 0x0400004D RID: 77
			public global::\u0008.\u0002 \u0001;
		}

		// Token: 0x02000023 RID: 35
		[CompilerGenerated]
		private sealed class \u0003
		{
			// Token: 0x060005B1 RID: 1457 RVA: 0x0000BCC4 File Offset: 0x00009EC4
			internal void \u0001()
			{
				this.\u0001._Condition.Accept(this.\u0001);
				this.\u0001._Controlled.Accept(this.\u0001);
			}

			// Token: 0x0400004E RID: 78
			public _IRepeatStatement \u0001;

			// Token: 0x0400004F RID: 79
			public global::\u0008.\u0002 \u0001;
		}

		// Token: 0x02000024 RID: 36
		[CompilerGenerated]
		private sealed class \u0004
		{
			// Token: 0x060005B3 RID: 1459 RVA: 0x0000BCFC File Offset: 0x00009EFC
			internal void \u0001()
			{
				this.\u0001._CounterStart.Accept(this.\u0001);
				this.\u0001._UpperBound.Accept(this.\u0001);
				_IExpression by = this.\u0001._By;
				if (by != null)
				{
					by.Accept(this.\u0001);
				}
				this.\u0001._Controlled.Accept(this.\u0001);
			}

			// Token: 0x04000050 RID: 80
			public _IForStatement \u0001;

			// Token: 0x04000051 RID: 81
			public global::\u0008.\u0002 \u0001;
		}

		// Token: 0x02000025 RID: 37
		[CompilerGenerated]
		private sealed class \u0005
		{
			// Token: 0x060005B5 RID: 1461 RVA: 0x0000BD70 File Offset: 0x00009F70
			internal void \u0001()
			{
				IList<_IStatement> statementList = this.\u0001._StatementList;
				for (int i = 0; i < statementList.Count; i++)
				{
					statementList[i].Accept(this.\u0001);
				}
			}

			// Token: 0x04000052 RID: 82
			public _ISequenceStatement \u0001;

			// Token: 0x04000053 RID: 83
			public global::\u0008.\u0002 \u0001;
		}

		// Token: 0x02000026 RID: 38
		[CompilerGenerated]
		private sealed class \u0006
		{
			// Token: 0x060005B7 RID: 1463 RVA: 0x0000BDB4 File Offset: 0x00009FB4
			internal void \u0001()
			{
				this.\u0001._Condition.Accept(this.\u0001);
				this.\u0001._IfThen.Accept(this.\u0001);
				foreach (_IElseIf ielseIf in this.\u0001._ElseIf)
				{
					ielseIf._Condition.Accept(this.\u0001);
					ielseIf._Controlled.Accept(this.\u0001);
				}
				_IStatement ifElse = this.\u0001._IfElse;
				if (ifElse == null)
				{
					return;
				}
				ifElse.Accept(this.\u0001);
			}

			// Token: 0x04000054 RID: 84
			public _IIfStatement \u0001;

			// Token: 0x04000055 RID: 85
			public global::\u0008.\u0002 \u0001;
		}

		// Token: 0x02000027 RID: 39
		[CompilerGenerated]
		private sealed class \u0007
		{
			// Token: 0x060005B9 RID: 1465 RVA: 0x0000BE70 File Offset: 0x0000A070
			internal void \u0001()
			{
				this.\u0001._Expr.Accept(this.\u0001);
			}

			// Token: 0x04000056 RID: 86
			public _IExpressionStatement \u0001;

			// Token: 0x04000057 RID: 87
			public global::\u0008.\u0002 \u0001;
		}

		// Token: 0x02000028 RID: 40
		[CompilerGenerated]
		private sealed class \u0008
		{
			// Token: 0x060005BB RID: 1467 RVA: 0x0000BE90 File Offset: 0x0000A090
			internal void \u0001()
			{
				this.\u0001._LValue.Accept(this.\u0001);
				this.\u0001._RValue.Accept(this.\u0001);
			}

			// Token: 0x04000058 RID: 88
			public _IAssignmentExpression \u0001;

			// Token: 0x04000059 RID: 89
			public global::\u0008.\u0002 \u0001;
		}

		// Token: 0x02000029 RID: 41
		[CompilerGenerated]
		private sealed class \u000E
		{
			// Token: 0x060005BD RID: 1469 RVA: 0x0000BEC8 File Offset: 0x0000A0C8
			internal void \u0001()
			{
				this.\u0001._Callee.Accept(this.\u0001);
				_IExpression condition = this.\u0001._Condition;
				if (condition != null)
				{
					condition.Accept(this.\u0001);
				}
				this.\u0001.\u0002(this.\u0001);
				foreach (_IExpression iexpression in this.\u0001.OutputExpressions)
				{
					if (iexpression != null)
					{
						iexpression.Accept(this.\u0001);
					}
				}
				foreach (_IExpression iexpression2 in this.\u0001.Inputs)
				{
					if (iexpression2 != null)
					{
						iexpression2.Accept(this.\u0001);
					}
				}
				foreach (_IExpression iexpression3 in this.\u0001.Outputs)
				{
					if (iexpression3 != null)
					{
						iexpression3.Accept(this.\u0001);
					}
				}
			}

			// Token: 0x0400005A RID: 90
			public _ICallExpression \u0001;

			// Token: 0x0400005B RID: 91
			public global::\u0008.\u0002 \u0001;
		}

		// Token: 0x0200002A RID: 42
		[CompilerGenerated]
		private sealed class \u000F
		{
			// Token: 0x060005BF RID: 1471 RVA: 0x0000C004 File Offset: 0x0000A204
			internal void \u0001()
			{
				foreach (_IExpression iexpression in this.\u0001._OperandsList)
				{
					iexpression.Accept(this.\u0001);
				}
			}

			// Token: 0x0400005C RID: 92
			public _IOperatorExpression \u0001;

			// Token: 0x0400005D RID: 93
			public global::\u0008.\u0002 \u0001;
		}

		// Token: 0x0200002B RID: 43
		[CompilerGenerated]
		private sealed class \u0010
		{
			// Token: 0x060005C1 RID: 1473 RVA: 0x0000C064 File Offset: 0x0000A264
			internal void \u0001()
			{
				this.\u0001.BaseExpression.Accept(this.\u0001);
			}

			// Token: 0x0400005E RID: 94
			public _ICastExpression \u0001;

			// Token: 0x0400005F RID: 95
			public global::\u0008.\u0002 \u0001;
		}

		// Token: 0x0200002C RID: 44
		[CompilerGenerated]
		private sealed class \u0011
		{
			// Token: 0x060005C3 RID: 1475 RVA: 0x0000C084 File Offset: 0x0000A284
			internal void \u0001()
			{
				this.\u0001._Count.Accept(this.\u0001);
				if (this.\u0001._FBInitParams != null)
				{
					foreach (_IAssignmentExpression iassignmentExpression in this.\u0001._FBInitParams.OfType<_IAssignmentExpression>())
					{
						iassignmentExpression.Accept(this.\u0001);
					}
				}
			}

			// Token: 0x04000060 RID: 96
			public _INewExpression \u0001;

			// Token: 0x04000061 RID: 97
			public global::\u0008.\u0002 \u0001;
		}

		// Token: 0x0200002D RID: 45
		[CompilerGenerated]
		private sealed class \u0012
		{
			// Token: 0x060005C5 RID: 1477 RVA: 0x0000C10C File Offset: 0x0000A30C
			internal void \u0001()
			{
				this.\u0001._Exp.Accept(this.\u0001);
			}

			// Token: 0x04000062 RID: 98
			public _IConversionExpression \u0001;

			// Token: 0x04000063 RID: 99
			public global::\u0008.\u0002 \u0001;
		}

		// Token: 0x0200002E RID: 46
		[CompilerGenerated]
		private sealed class \u0013
		{
			// Token: 0x060005C7 RID: 1479 RVA: 0x0000C12C File Offset: 0x0000A32C
			internal void \u0001()
			{
				for (int i = 0; i < this.\u0001.NumAccesses; i++)
				{
					this.\u0001.GetAccess(i).Accept(this.\u0001);
				}
				this.\u0001._Var.Accept(this.\u0001);
			}

			// Token: 0x04000064 RID: 100
			public _IIndexAccessExpression \u0001;

			// Token: 0x04000065 RID: 101
			public global::\u0008.\u0002 \u0001;
		}

		// Token: 0x0200002F RID: 47
		[CompilerGenerated]
		private sealed class \u0014
		{
			// Token: 0x060005C9 RID: 1481 RVA: 0x0000C184 File Offset: 0x0000A384
			internal void \u0001()
			{
				this.\u0001._Right.Accept(this.\u0001);
				this.\u0001._Left.Accept(this.\u0001);
			}

			// Token: 0x04000066 RID: 102
			public _ICompoAccessExpression \u0001;

			// Token: 0x04000067 RID: 103
			public global::\u0008.\u0002 \u0001;
		}

		// Token: 0x02000030 RID: 48
		[CompilerGenerated]
		private sealed class \u0015
		{
			// Token: 0x060005CB RID: 1483 RVA: 0x0000C1BC File Offset: 0x0000A3BC
			internal void \u0001()
			{
				this.\u0001._Base.Accept(this.\u0001);
			}

			// Token: 0x04000068 RID: 104
			public _IDeRefAccessExpression \u0001;

			// Token: 0x04000069 RID: 105
			public global::\u0008.\u0002 \u0001;
		}

		// Token: 0x02000031 RID: 49
		[CompilerGenerated]
		private sealed class \u0016
		{
			// Token: 0x060005CD RID: 1485 RVA: 0x0000C1DC File Offset: 0x0000A3DC
			internal void \u0001()
			{
				this.\u0001._Base.Accept(this.\u0001);
			}

			// Token: 0x0400006A RID: 106
			public _ICopyScopeExpression \u0001;

			// Token: 0x0400006B RID: 107
			public global::\u0008.\u0002 \u0001;
		}

		// Token: 0x02000032 RID: 50
		[CompilerGenerated]
		private sealed class \u0017
		{
			// Token: 0x060005CF RID: 1487 RVA: 0x0000C1FC File Offset: 0x0000A3FC
			internal void \u0001()
			{
				this.\u0001._Base.Accept(this.\u0001);
			}

			// Token: 0x0400006C RID: 108
			public _IGlobalScopeExpression \u0001;

			// Token: 0x0400006D RID: 109
			public global::\u0008.\u0002 \u0001;
		}

		// Token: 0x02000033 RID: 51
		[CompilerGenerated]
		private sealed class \u0018
		{
			// Token: 0x060005D1 RID: 1489 RVA: 0x0000C21C File Offset: 0x0000A41C
			internal void \u0001()
			{
				this.\u0001._Base.Accept(this.\u0001);
			}

			// Token: 0x0400006E RID: 110
			public _ISystemScopeExpression \u0001;

			// Token: 0x0400006F RID: 111
			public global::\u0008.\u0002 \u0001;
		}

		// Token: 0x02000034 RID: 52
		[CompilerGenerated]
		private sealed class \u0019
		{
			// Token: 0x060005D3 RID: 1491 RVA: 0x0000C23C File Offset: 0x0000A43C
			internal void \u0001()
			{
				this.\u0001._Base.Accept(this.\u0001);
			}

			// Token: 0x04000070 RID: 112
			public _IPoolScopeExpression \u0001;

			// Token: 0x04000071 RID: 113
			public global::\u0008.\u0002 \u0001;
		}

		// Token: 0x02000035 RID: 53
		[CompilerGenerated]
		private sealed class \u001A
		{
			// Token: 0x060005D5 RID: 1493 RVA: 0x0000C25C File Offset: 0x0000A45C
			internal void \u0001()
			{
				this.\u0001._Namespace.Accept(this.\u0001);
				_IExpression access = this.\u0001._Access;
				if (access == null)
				{
					return;
				}
				access.Accept(this.\u0001);
			}

			// Token: 0x04000072 RID: 114
			public _INamespaceAccessExpression \u0001;

			// Token: 0x04000073 RID: 115
			public global::\u0008.\u0002 \u0001;
		}

		// Token: 0x02000036 RID: 54
		[CompilerGenerated]
		private sealed class \u001B
		{
			// Token: 0x060005D7 RID: 1495 RVA: 0x0000C298 File Offset: 0x0000A498
			internal void \u0001()
			{
				this.\u0001._Base.Accept(this.\u0001);
			}

			// Token: 0x04000074 RID: 116
			public _ICurrentTaskExpression \u0001;

			// Token: 0x04000075 RID: 117
			public global::\u0008.\u0002 \u0001;
		}

		// Token: 0x02000037 RID: 55
		[CompilerGenerated]
		private sealed class \u001C
		{
			// Token: 0x060005D9 RID: 1497 RVA: 0x0000C2B8 File Offset: 0x0000A4B8
			internal void \u0001()
			{
				this.\u0001._Low.Accept(this.\u0001);
				this.\u0001._High.Accept(this.\u0001);
			}

			// Token: 0x04000076 RID: 118
			public _ICaseRangeExpression \u0001;

			// Token: 0x04000077 RID: 119
			public global::\u0008.\u0002 \u0001;
		}

		// Token: 0x02000038 RID: 56
		[CompilerGenerated]
		private sealed class \u001D
		{
			// Token: 0x060005DB RID: 1499 RVA: 0x0000C2F0 File Offset: 0x0000A4F0
			internal void \u0001()
			{
				foreach (_IExpression iexpression in this.\u0001._cases)
				{
					iexpression.Accept(this.\u0001);
				}
			}

			// Token: 0x04000078 RID: 120
			public _ICaseLabelStatement \u0001;

			// Token: 0x04000079 RID: 121
			public global::\u0008.\u0002 \u0001;
		}

		// Token: 0x02000039 RID: 57
		[CompilerGenerated]
		private sealed class \u001E
		{
			// Token: 0x060005DD RID: 1501 RVA: 0x0000C350 File Offset: 0x0000A550
			internal void \u0001()
			{
				this.\u0001._Switch.Accept(this.\u0001);
				foreach (_ICase icase in this.\u0001._Cases)
				{
					icase._Label.Accept(this.\u0001);
					icase._Controlled.Accept(this.\u0001);
				}
				_IStatement @else = this.\u0001._Else;
				if (@else == null)
				{
					return;
				}
				@else.Accept(this.\u0001);
			}

			// Token: 0x0400007A RID: 122
			public _ICaseStatement \u0001;

			// Token: 0x0400007B RID: 123
			public global::\u0008.\u0002 \u0001;
		}

		// Token: 0x0200003A RID: 58
		[CompilerGenerated]
		private sealed class \u001F
		{
			// Token: 0x060005DF RID: 1503 RVA: 0x0000C3F4 File Offset: 0x0000A5F4
			internal void \u0001()
			{
				_IExpression condition = this.\u0001._Condition;
				if (condition == null)
				{
					return;
				}
				condition.Accept(this.\u0001);
			}

			// Token: 0x0400007C RID: 124
			public _IReturnStatement \u0001;

			// Token: 0x0400007D RID: 125
			public global::\u0008.\u0002 \u0001;
		}

		// Token: 0x0200003B RID: 59
		[CompilerGenerated]
		private sealed class \u007F
		{
			// Token: 0x060005E1 RID: 1505 RVA: 0x0000C41C File Offset: 0x0000A61C
			internal void \u0001()
			{
				_IExpression condition = this.\u0001._Condition;
				if (condition == null)
				{
					return;
				}
				condition.Accept(this.\u0001);
			}

			// Token: 0x0400007E RID: 126
			public _IJumpStatement \u0001;

			// Token: 0x0400007F RID: 127
			public global::\u0008.\u0002 \u0001;
		}

		// Token: 0x0200003C RID: 60
		[CompilerGenerated]
		private sealed class \u0080
		{
			// Token: 0x060005E3 RID: 1507 RVA: 0x0000C444 File Offset: 0x0000A644
			internal void \u0001()
			{
				foreach (_IExpression iexpression in this.\u0001.NameList)
				{
					iexpression.Accept(this.\u0001);
				}
				_IExpression initial = this.\u0001.Initial;
				if (initial != null)
				{
					initial.Accept(this.\u0001);
				}
				if (this.\u0001.InputAssigns != null)
				{
					foreach (_IAssignmentExpression iassignmentExpression in this.\u0001.InputAssigns)
					{
						iassignmentExpression.Accept(this.\u0001);
					}
				}
			}

			// Token: 0x04000080 RID: 128
			public _IVariableDeclarationStatement \u0001;

			// Token: 0x04000081 RID: 129
			public global::\u0008.\u0002 \u0001;
		}

		// Token: 0x0200003D RID: 61
		[CompilerGenerated]
		private sealed class \u0081
		{
			// Token: 0x060005E5 RID: 1509 RVA: 0x0000C510 File Offset: 0x0000A710
			internal void \u0001()
			{
				this.\u0001.VariableDeclaration.Accept(this.\u0001);
			}

			// Token: 0x04000082 RID: 130
			public _IVariableDeclarationListStatement \u0001;

			// Token: 0x04000083 RID: 131
			public global::\u0008.\u0002 \u0001;
		}

		// Token: 0x0200003E RID: 62
		[CompilerGenerated]
		private sealed class \u0082
		{
			// Token: 0x060005E7 RID: 1511 RVA: 0x0000C530 File Offset: 0x0000A730
			internal void \u0001()
			{
				this.\u0001.Declarations.Accept(this.\u0001);
				if (this.\u0001.Implements != null)
				{
					foreach (_IExpression iexpression in this.\u0001.Implements)
					{
						iexpression.Accept(this.\u0001);
					}
				}
				if (this.\u0001.Extends != null)
				{
					foreach (_IExpression iexpression2 in this.\u0001.Extends)
					{
						iexpression2.Accept(this.\u0001);
					}
				}
			}

			// Token: 0x04000084 RID: 132
			public _IPOUDeclarationStatement \u0001;

			// Token: 0x04000085 RID: 133
			public global::\u0008.\u0002 \u0001;
		}

		// Token: 0x0200003F RID: 63
		[CompilerGenerated]
		private sealed class \u0083
		{
			// Token: 0x060005E9 RID: 1513 RVA: 0x0000C604 File Offset: 0x0000A804
			internal void \u0001()
			{
				this.\u0001.Declarations.Accept(this.\u0001);
				_IExpression initial = this.\u0001.Initial;
				if (initial != null)
				{
					initial.Accept(this.\u0001);
				}
				_IExpression extends = this.\u0001.Extends;
				if (extends == null)
				{
					return;
				}
				extends.Accept(this.\u0001);
			}

			// Token: 0x04000086 RID: 134
			public _ITypeDeclarationStatement \u0001;

			// Token: 0x04000087 RID: 135
			public global::\u0008.\u0002 \u0001;
		}

		// Token: 0x02000040 RID: 64
		[CompilerGenerated]
		private sealed class \u0084
		{
			// Token: 0x060005EB RID: 1515 RVA: 0x0000C668 File Offset: 0x0000A868
			internal void \u0001()
			{
				_IExpression value = this.\u0001._Value;
				if (value == null)
				{
					return;
				}
				value.Accept(this.\u0001);
			}

			// Token: 0x04000088 RID: 136
			public _IEnumDeclarationStatement \u0001;

			// Token: 0x04000089 RID: 137
			public global::\u0008.\u0002 \u0001;
		}

		// Token: 0x02000041 RID: 65
		[CompilerGenerated]
		private sealed class \u0086
		{
			// Token: 0x060005ED RID: 1517 RVA: 0x0000C690 File Offset: 0x0000A890
			internal void \u0001()
			{
				foreach (_IEnumDeclarationStatement ienumDeclarationStatement in this.\u0001.Enums)
				{
					ienumDeclarationStatement.Accept(this.\u0001);
				}
			}

			// Token: 0x0400008A RID: 138
			public _IEnumDeclarationListStatement \u0001;

			// Token: 0x0400008B RID: 139
			public global::\u0008.\u0002 \u0001;
		}

		// Token: 0x02000042 RID: 66
		[CompilerGenerated]
		private sealed class \u0087
		{
			// Token: 0x060005EF RID: 1519 RVA: 0x0000C6F0 File Offset: 0x0000A8F0
			internal void \u0001()
			{
				this.\u0001._Value.Accept(this.\u0001);
				this.\u0001._Number.Accept(this.\u0001);
			}

			// Token: 0x0400008C RID: 140
			public _IMultipleIndexInitialization \u0001;

			// Token: 0x0400008D RID: 141
			public global::\u0008.\u0002 \u0001;
		}

		// Token: 0x02000043 RID: 67
		[CompilerGenerated]
		private sealed class \u0088
		{
			// Token: 0x060005F1 RID: 1521 RVA: 0x0000C728 File Offset: 0x0000A928
			internal void \u0001()
			{
				foreach (_IExpression iexpression in this.\u0001._InitValues)
				{
					iexpression.Accept(this.\u0001);
				}
			}

			// Token: 0x0400008E RID: 142
			public _IArrayInitialization \u0001;

			// Token: 0x0400008F RID: 143
			public global::\u0008.\u0002 \u0001;
		}

		// Token: 0x02000044 RID: 68
		[CompilerGenerated]
		private sealed class \u0089
		{
			// Token: 0x060005F3 RID: 1523 RVA: 0x0000C788 File Offset: 0x0000A988
			internal void \u0001()
			{
				foreach (_IAssignmentExpression iassignmentExpression in this.\u0001._CompoInits)
				{
					iassignmentExpression.Accept(this.\u0001);
				}
			}

			// Token: 0x04000090 RID: 144
			public _IStructureInitialization \u0001;

			// Token: 0x04000091 RID: 145
			public global::\u0008.\u0002 \u0001;
		}

		// Token: 0x02000045 RID: 69
		[CompilerGenerated]
		private sealed class \u008A
		{
			// Token: 0x060005F5 RID: 1525 RVA: 0x0000C7E8 File Offset: 0x0000A9E8
			internal void \u0001()
			{
				_IExpression instancePath = this.\u0001.InstancePath;
				if (instancePath == null)
				{
					return;
				}
				instancePath.Accept(this.\u0001);
			}

			// Token: 0x04000092 RID: 146
			public _IVariableReference \u0001;

			// Token: 0x04000093 RID: 147
			public global::\u0008.\u0002 \u0001;
		}

		// Token: 0x02000046 RID: 70
		[CompilerGenerated]
		private sealed class \u008B
		{
			// Token: 0x060005F7 RID: 1527 RVA: 0x0000C810 File Offset: 0x0000AA10
			internal void \u0001()
			{
				_IExpression instancePath = this.\u0001.InstancePath;
				if (instancePath == null)
				{
					return;
				}
				instancePath.Accept(this.\u0001);
			}

			// Token: 0x04000094 RID: 148
			public _ITypeReference \u0001;

			// Token: 0x04000095 RID: 149
			public global::\u0008.\u0002 \u0001;
		}

		// Token: 0x02000047 RID: 71
		[CompilerGenerated]
		private sealed class \u008C
		{
			// Token: 0x060005F9 RID: 1529 RVA: 0x0000C838 File Offset: 0x0000AA38
			internal void \u0001()
			{
				_IExpression instancePath = this.\u0001.InstancePath;
				if (instancePath == null)
				{
					return;
				}
				instancePath.Accept(this.\u0001);
			}

			// Token: 0x04000096 RID: 150
			public _IPouReference \u0001;

			// Token: 0x04000097 RID: 151
			public global::\u0008.\u0002 \u0001;
		}

		// Token: 0x02000048 RID: 72
		[CompilerGenerated]
		private sealed class \u008D
		{
			// Token: 0x060005FB RID: 1531 RVA: 0x0000C860 File Offset: 0x0000AA60
			internal void \u0001()
			{
				this.\u0001.ItemReference.Accept(this.\u0001);
			}

			// Token: 0x04000098 RID: 152
			public _IDefinedExpression \u0001;

			// Token: 0x04000099 RID: 153
			public global::\u0008.\u0002 \u0001;
		}

		// Token: 0x02000049 RID: 73
		[CompilerGenerated]
		private sealed class \u008E
		{
			// Token: 0x060005FD RID: 1533 RVA: 0x0000C880 File Offset: 0x0000AA80
			internal void \u0001()
			{
				foreach (_IExpression iexpression in this.\u0001.Operands)
				{
					iexpression.Accept(this.\u0001);
				}
			}

			// Token: 0x0400009A RID: 154
			public _IPragmaOperatorExpression \u0001;

			// Token: 0x0400009B RID: 155
			public global::\u0008.\u0002 \u0001;
		}

		// Token: 0x0200004A RID: 74
		[CompilerGenerated]
		private sealed class \u008F
		{
			// Token: 0x060005FF RID: 1535 RVA: 0x0000C8E0 File Offset: 0x0000AAE0
			internal void \u0001()
			{
				this.\u0001.Condition.Accept(this.\u0001);
				this.\u0001.IfThen.Accept(this.\u0001);
				foreach (_IPragmaElseIf ipragmaElseIf in this.\u0001.ElseIf)
				{
					ipragmaElseIf.Condition.Accept(this.\u0001);
					ipragmaElseIf.Controlled.Accept(this.\u0001);
				}
				_IStatement ifElse = this.\u0001.IfElse;
				if (ifElse == null)
				{
					return;
				}
				ifElse.Accept(this.\u0001);
			}

			// Token: 0x0400009C RID: 156
			public _IPragmaIfStatement \u0001;

			// Token: 0x0400009D RID: 157
			public global::\u0008.\u0002 \u0001;
		}

		// Token: 0x0200004B RID: 75
		[CompilerGenerated]
		private sealed class \u0090
		{
			// Token: 0x06000601 RID: 1537 RVA: 0x0000C99C File Offset: 0x0000AB9C
			internal void \u0001()
			{
				this.\u0001.XRef.Accept(this.\u0001);
				this.\u0001.XRefFrom.Accept(this.\u0001);
			}

			// Token: 0x0400009E RID: 158
			public _IXRefExpression \u0001;

			// Token: 0x0400009F RID: 159
			public global::\u0008.\u0002 \u0001;
		}

		// Token: 0x0200004C RID: 76
		[CompilerGenerated]
		private sealed class \u0091
		{
			// Token: 0x06000603 RID: 1539 RVA: 0x0000C9D4 File Offset: 0x0000ABD4
			internal void \u0001()
			{
				this.\u0001.Variable.Accept(this.\u0001);
			}

			// Token: 0x040000A0 RID: 160
			public _IHasTypeExpression \u0001;

			// Token: 0x040000A1 RID: 161
			public global::\u0008.\u0002 \u0001;
		}

		// Token: 0x0200004D RID: 77
		[CompilerGenerated]
		private sealed class \u0092
		{
			// Token: 0x06000605 RID: 1541 RVA: 0x0000C9F4 File Offset: 0x0000ABF4
			internal void \u0001()
			{
				this.\u0001.ItemReference.Accept(this.\u0001);
			}

			// Token: 0x040000A2 RID: 162
			public _IHasAttributeExpression \u0001;

			// Token: 0x040000A3 RID: 163
			public global::\u0008.\u0002 \u0001;
		}

		// Token: 0x0200004E RID: 78
		[CompilerGenerated]
		private sealed class \u0093
		{
			// Token: 0x06000607 RID: 1543 RVA: 0x0000CA14 File Offset: 0x0000AC14
			internal void \u0001()
			{
				this.\u0001._Constant.Accept(this.\u0001);
			}

			// Token: 0x040000A4 RID: 164
			public _IHasConstantValueExpression \u0001;

			// Token: 0x040000A5 RID: 165
			public global::\u0008.\u0002 \u0001;
		}

		// Token: 0x0200004F RID: 79
		[CompilerGenerated]
		private sealed class \u0094
		{
			// Token: 0x06000609 RID: 1545 RVA: 0x0000CA34 File Offset: 0x0000AC34
			internal void \u0001()
			{
				this.\u0001._Constant.Accept(this.\u0001);
			}

			// Token: 0x040000A6 RID: 166
			public _IHasConstantTypeExpression \u0001;

			// Token: 0x040000A7 RID: 167
			public global::\u0008.\u0002 \u0001;
		}

		// Token: 0x02000050 RID: 80
		[CompilerGenerated]
		private sealed class \u0095
		{
			// Token: 0x0600060B RID: 1547 RVA: 0x0000CA54 File Offset: 0x0000AC54
			internal void \u0001()
			{
				this.\u0001.Condition.Accept(this.\u0001);
			}

			// Token: 0x040000A8 RID: 168
			public _IPragmaAssertion \u0001;

			// Token: 0x040000A9 RID: 169
			public global::\u0008.\u0002 \u0001;
		}

		// Token: 0x02000051 RID: 81
		[CompilerGenerated]
		private sealed class \u0096
		{
			// Token: 0x0600060D RID: 1549 RVA: 0x0000CA74 File Offset: 0x0000AC74
			internal void \u0001()
			{
				this.\u0001._Left.Accept(this.\u0001);
			}

			// Token: 0x040000AA RID: 170
			public _IPartialAccessExpression \u0001;

			// Token: 0x040000AB RID: 171
			public global::\u0008.\u0002 \u0001;
		}

		// Token: 0x02000052 RID: 82
		[CompilerGenerated]
		private sealed class \u0097
		{
			// Token: 0x0600060F RID: 1551 RVA: 0x0000CA94 File Offset: 0x0000AC94
			internal void \u0001()
			{
				_IDefineReference defineReference = this.\u0001.DefineReference;
				if (defineReference == null)
				{
					return;
				}
				defineReference.Accept(this.\u0001);
			}

			// Token: 0x040000AC RID: 172
			public _IProjectDefinedExpression \u0001;

			// Token: 0x040000AD RID: 173
			public global::\u0008.\u0002 \u0001;
		}
	}
}
