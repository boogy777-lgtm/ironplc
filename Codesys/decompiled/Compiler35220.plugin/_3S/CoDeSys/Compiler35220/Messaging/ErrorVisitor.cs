using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using \u0019;
using \u001F;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.Compiler35220.Messaging
{
	// Token: 0x0200038F RID: 911
	internal sealed class ErrorVisitor : IExprementVisitor352000, IExprementVisitor351900, IExprementVisitor351800, IExprementVisitor351500, IExprementVisitor351400, IExprementVisitor351300, IExprementVisitor3590, IExprementVisitor, IExprementVisitor2, \u001F.\u0001, IErrorVisitor
	{
		// Token: 0x170008B2 RID: 2226
		// (get) Token: 0x060034BB RID: 13499 RVA: 0x000D0178 File Offset: 0x000CE378
		// (set) Token: 0x060034BC RID: 13500 RVA: 0x000D0180 File Offset: 0x000CE380
		public bool VisitErrorStatements { get; set; } = true;

		// Token: 0x170008B3 RID: 2227
		// (get) Token: 0x060034BD RID: 13501 RVA: 0x000D018C File Offset: 0x000CE38C
		// (set) Token: 0x060034BE RID: 13502 RVA: 0x000D0194 File Offset: 0x000CE394
		public bool VisitErrorStatementsInConditionalPragmas { get; set; } = true;

		// Token: 0x170008B4 RID: 2228
		// (get) Token: 0x060034C0 RID: 13504 RVA: 0x000D01D8 File Offset: 0x000CE3D8
		// (set) Token: 0x060034C1 RID: 13505 RVA: 0x000D01E0 File Offset: 0x000CE3E0
		public Guid MessageGuid { get; set; } = Guid.Empty;

		// Token: 0x170008B5 RID: 2229
		// (get) Token: 0x060034C2 RID: 13506 RVA: 0x000D01EC File Offset: 0x000CE3EC
		public IList<_ICompilerMessage> MessageList
		{
			get
			{
				return this.\u0001;
			}
		}

		// Token: 0x170008B6 RID: 2230
		// (get) Token: 0x060034C3 RID: 13507 RVA: 0x000D01F4 File Offset: 0x000CE3F4
		public _ICompilerMessage[] _Messages
		{
			get
			{
				_ICompilerMessage[] array = new _ICompilerMessage[this.\u0001.Count];
				this.\u0001.CopyTo(array, 0);
				return array;
			}
		}

		// Token: 0x170008B7 RID: 2231
		// (get) Token: 0x060034C4 RID: 13508 RVA: 0x000D0220 File Offset: 0x000CE420
		public IMessage[] Messages
		{
			get
			{
				_ICompilerMessage[] array = new _ICompilerMessage[this.\u0001.Count];
				this.\u0001.CopyTo(array, 0);
				return array;
			}
		}

		// Token: 0x060034C5 RID: 13509 RVA: 0x000D0250 File Offset: 0x000CE450
		public void \u0001(string \u0002)
		{
			if (!this.\u0001.ContainsKey(\u0002))
			{
				this.\u0001.Add(\u0002, \u0002);
			}
		}

		// Token: 0x060034C6 RID: 13510 RVA: 0x000D0270 File Offset: 0x000CE470
		private void \u0002(string \u0002)
		{
			if (this.\u0001.ContainsKey(\u0002))
			{
				this.\u0001.Remove(\u0002);
			}
		}

		// Token: 0x060034C7 RID: 13511 RVA: 0x000D0290 File Offset: 0x000CE490
		private void \u0001(_IExprement \u0002)
		{
			IList<_ICompilerMessage> messagesList = \u0002.MessagesList;
			if (messagesList == null || messagesList.Count == 0)
			{
				return;
			}
			for (int i = 0; i < messagesList.Count; i++)
			{
				_ICompilerMessage icompilerMessage = messagesList[i];
				icompilerMessage = this.\u0001(icompilerMessage);
				this.\u0001.Add(icompilerMessage);
			}
		}

		// Token: 0x060034C8 RID: 13512 RVA: 0x000D02E0 File Offset: 0x000CE4E0
		internal _ICompilerMessage \u0001(_ICompilerMessage \u0002)
		{
			Guid guid = Guid.Empty;
			if (this.MessageGuid != Guid.Empty && \u0002.ObjectGuid == Guid.Empty)
			{
				guid = this.MessageGuid;
			}
			Severity severity = \u0002.Severity;
			if (severity == Severity.Warning && \u0002.Number != null && this.\u0001.Count > 0)
			{
				string text = string.Format("{0}{1:d4}", \u0002.Prefix, \u0002.Number);
				if (this.\u0001.ContainsKey(text))
				{
					severity = Severity.SuppressedWarning;
				}
			}
			if (guid != Guid.Empty || severity != Severity.Warning)
			{
				_ICompilerMessage icompilerMessage = \u0019.\u0003.\u0001(\u0019.\u0003.\u0001(\u0002.ProjectHandle, guid, \u0002.Position, \u0002.PositionOffset, \u0002.Length), \u0002.Text, severity, \u0002.MessageId);
				icompilerMessage.ShowAttribute = \u0002.ShowAttribute;
				return icompilerMessage;
			}
			return \u0002;
		}

		// Token: 0x060034C9 RID: 13513 RVA: 0x000D03C4 File Offset: 0x000CE5C4
		public void \u0001(_ICompiledPOU \u0002)
		{
			_IStatement parseTree = \u0002.GetParseTree();
			if (parseTree != null)
			{
				parseTree.Accept(this);
			}
		}

		// Token: 0x060034CA RID: 13514 RVA: 0x000D03E4 File Offset: 0x000CE5E4
		public void \u0001(_ISequenceStatement \u0002)
		{
			this.\u0001(\u0002);
			IList<_IStatement> statementList = \u0002._StatementList;
			for (int i = 0; i < statementList.Count; i++)
			{
				statementList[i].Accept(this);
			}
		}

		// Token: 0x060034CB RID: 13515 RVA: 0x000D0420 File Offset: 0x000CE620
		public void \u0001(_IWhileStatement \u0002)
		{
			this.\u0001(\u0002);
			\u0002._Condition.Accept(this);
			\u0002._Controlled.Accept(this);
		}

		// Token: 0x060034CC RID: 13516 RVA: 0x000D0444 File Offset: 0x000CE644
		public void \u0001(_IRepeatStatement \u0002)
		{
			this.\u0001(\u0002);
			\u0002._Controlled.Accept(this);
			\u0002._Condition.Accept(this);
		}

		// Token: 0x060034CD RID: 13517 RVA: 0x000D0468 File Offset: 0x000CE668
		public void \u0001(_IForStatement \u0002)
		{
			string u = string.Format("C{0:D4}", 195);
			this.\u0001(\u0002);
			\u0002._CounterStart.Accept(this);
			\u0002._UpperBound.Accept(this);
			if (\u0002.By != null)
			{
				this.\u0001(u);
				\u0002._By.Accept(this);
				this.\u0002(u);
			}
			\u0002._Controlled.Accept(this);
		}

		// Token: 0x060034CE RID: 13518 RVA: 0x000D04D8 File Offset: 0x000CE6D8
		public void \u0001(_IExitStatement \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x060034CF RID: 13519 RVA: 0x000D04E4 File Offset: 0x000CE6E4
		public void \u0001(_IContinueStatement \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x060034D0 RID: 13520 RVA: 0x000D04F0 File Offset: 0x000CE6F0
		public void \u0001(_IAssignmentExpression \u0002)
		{
			this.\u0001(\u0002);
			\u0002._LValue.Accept(this);
			\u0002._RValue.Accept(this);
		}

		// Token: 0x060034D1 RID: 13521 RVA: 0x000D0514 File Offset: 0x000CE714
		public void \u0001(_IIfStatement \u0002)
		{
			this.\u0001(\u0002);
			\u0002._Condition.Accept(this);
			\u0002._IfThen.Accept(this);
			foreach (_IElseIf ielseIf in \u0002._ElseIf)
			{
				ielseIf._Condition.Accept(this);
				ielseIf._Controlled.Accept(this);
			}
			if (\u0002._IfElse != null)
			{
				\u0002._IfElse.Accept(this);
			}
		}

		// Token: 0x060034D2 RID: 13522 RVA: 0x000D05A4 File Offset: 0x000CE7A4
		public void \u0001(_IReturnStatement \u0002)
		{
			if (\u0002._Condition != null)
			{
				\u0002._Condition.Accept(this);
			}
			this.\u0001(\u0002);
		}

		// Token: 0x060034D3 RID: 13523 RVA: 0x000D05C4 File Offset: 0x000CE7C4
		public void \u0001(_IJumpStatement \u0002)
		{
			if (\u0002._Condition != null)
			{
				\u0002._Condition.Accept(this);
			}
			this.\u0001(\u0002);
		}

		// Token: 0x060034D4 RID: 13524 RVA: 0x000D05E4 File Offset: 0x000CE7E4
		public void \u0001(_ILabelStatement \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x060034D5 RID: 13525 RVA: 0x000D05F0 File Offset: 0x000CE7F0
		public void \u0001(_ICommentStatement \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x060034D6 RID: 13526 RVA: 0x000D05FC File Offset: 0x000CE7FC
		public void \u0001(_IPragmaStatement \u0002)
		{
			if (\u0002 is _IWarningDisableRestorePragmaStatement)
			{
				_IWarningDisableRestorePragmaStatement iwarningDisableRestorePragmaStatement = \u0002 as _IWarningDisableRestorePragmaStatement;
				if (iwarningDisableRestorePragmaStatement.Restore)
				{
					if (this.\u0001.ContainsKey(iwarningDisableRestorePragmaStatement.Id))
					{
						this.\u0001.Remove(iwarningDisableRestorePragmaStatement.Id);
					}
				}
				else if (!this.\u0001.ContainsKey(iwarningDisableRestorePragmaStatement.Id))
				{
					this.\u0001.Add(iwarningDisableRestorePragmaStatement.Id, iwarningDisableRestorePragmaStatement.Id);
				}
			}
			this.\u0001(\u0002);
			if (\u0002 is _IMessageGuidPragmaStatement)
			{
				this.MessageGuid = (\u0002 as _IMessageGuidPragmaStatement).MessageGuid;
			}
		}

		// Token: 0x060034D7 RID: 13527 RVA: 0x000D0694 File Offset: 0x000CE894
		public void \u0001(_IExpressionStatement \u0002)
		{
			this.\u0001(\u0002);
			\u0002._Expr.Accept(this);
		}

		// Token: 0x060034D8 RID: 13528 RVA: 0x000D06AC File Offset: 0x000CE8AC
		public void \u0001(_IVariableDeclarationStatement \u0002)
		{
			this.\u0001(\u0002);
			_IExpression initial = \u0002.Initial;
			if (initial == null)
			{
				return;
			}
			initial.Accept(this);
		}

		// Token: 0x060034D9 RID: 13529 RVA: 0x000D06C8 File Offset: 0x000CE8C8
		public void \u0001(_IVariableDeclarationListStatement \u0002)
		{
			this.\u0001(\u0002);
			\u0002.VariableDeclaration.Accept(this);
		}

		// Token: 0x060034DA RID: 13530 RVA: 0x000D06E0 File Offset: 0x000CE8E0
		public void \u0001(_IPOUDeclarationStatement \u0002)
		{
			this.\u0001(\u0002);
			\u0002.Declarations.Accept(this);
		}

		// Token: 0x060034DB RID: 13531 RVA: 0x000D06F8 File Offset: 0x000CE8F8
		public void \u0001(_ITypeDeclarationStatement \u0002)
		{
			this.\u0001(\u0002);
			\u0002.Declarations.Accept(this);
			if (\u0002.Initial != null)
			{
				\u0002.Initial.Accept(this);
			}
		}

		// Token: 0x060034DC RID: 13532 RVA: 0x000D0724 File Offset: 0x000CE924
		public void \u0001(_IEnumDeclarationStatement \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x060034DD RID: 13533 RVA: 0x000D0730 File Offset: 0x000CE930
		public void \u0001(_IEnumDeclarationListStatement \u0002)
		{
			this.\u0001(\u0002);
			foreach (_IEnumDeclarationStatement ienumDeclarationStatement in \u0002.Enums)
			{
				ienumDeclarationStatement.Accept(this);
			}
		}

		// Token: 0x060034DE RID: 13534 RVA: 0x000D0784 File Offset: 0x000CE984
		public void \u0001(_ICallExpression \u0002)
		{
			this.\u0001(\u0002);
			\u0002._Callee.Accept(this);
			if (\u0002._Condition != null)
			{
				\u0002._Condition.Accept(this);
			}
			foreach (_IExpression iexpression in \u0002.ParamExpressions)
			{
				if (iexpression != null)
				{
					iexpression.Accept(this);
				}
			}
			foreach (_IExpression iexpression2 in \u0002.OutputExpressions)
			{
				if (iexpression2 != null)
				{
					iexpression2.Accept(this);
				}
			}
			foreach (_IExpression iexpression3 in \u0002.Inputs)
			{
				if (iexpression3 != null)
				{
					iexpression3.Accept(this);
				}
			}
			foreach (_IExpression iexpression4 in \u0002.Outputs)
			{
				if (iexpression4 != null)
				{
					iexpression4.Accept(this);
				}
			}
			foreach (_IExpression iexpression5 in \u0002.EmptyAssigns)
			{
				if (iexpression5 != null)
				{
					iexpression5.Accept(this);
				}
			}
		}

		// Token: 0x060034DF RID: 13535 RVA: 0x000D0900 File Offset: 0x000CEB00
		public void \u0001(_IOperatorExpression \u0002)
		{
			this.\u0001(\u0002);
			IList<_IExpression> operandsList = \u0002._OperandsList;
			for (int i = 0; i < operandsList.Count; i++)
			{
				operandsList[i].Accept(this);
			}
		}

		// Token: 0x060034E0 RID: 13536 RVA: 0x000D093C File Offset: 0x000CEB3C
		public void \u0001(_ICastExpression \u0002)
		{
			this.\u0001(\u0002);
			\u0002.BaseExpression.Accept(this);
			if (\u0002.ExpWithType != null)
			{
				\u0002.ExpWithType.Accept(this);
			}
		}

		// Token: 0x060034E1 RID: 13537 RVA: 0x000D0968 File Offset: 0x000CEB68
		public void \u0001(_INewExpression \u0002)
		{
			this.\u0001(\u0002);
			\u0002._Count.Accept(this);
			if (\u0002._FBInitParams != null)
			{
				foreach (IAssignmentExpression assignmentExpression in \u0002._FBInitParams)
				{
					((_IAssignmentExpression)assignmentExpression).Accept(this);
				}
			}
		}

		// Token: 0x060034E2 RID: 13538 RVA: 0x000D09D4 File Offset: 0x000CEBD4
		public void \u0001(_ITypeExpression \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x060034E3 RID: 13539 RVA: 0x000D09E0 File Offset: 0x000CEBE0
		public void \u0001(_IConversionExpression \u0002)
		{
			this.\u0001(\u0002);
			\u0002._Exp.Accept(this);
		}

		// Token: 0x060034E4 RID: 13540 RVA: 0x000D09F8 File Offset: 0x000CEBF8
		public void \u0001(_IThisExpression \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x060034E5 RID: 13541 RVA: 0x000D0A04 File Offset: 0x000CEC04
		public void \u0001(_IBaseExpression \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x060034E6 RID: 13542 RVA: 0x000D0A10 File Offset: 0x000CEC10
		public void \u0001(_ILiteralExpression \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x060034E7 RID: 13543 RVA: 0x000D0A1C File Offset: 0x000CEC1C
		public void \u0001(_IAddressExpression \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x060034E8 RID: 13544 RVA: 0x000D0A28 File Offset: 0x000CEC28
		public void \u0001(_IQualifiedNameExpression \u0002)
		{
		}

		// Token: 0x060034E9 RID: 13545 RVA: 0x000D0A2C File Offset: 0x000CEC2C
		public void \u0001(_IVariableExpression \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x060034EA RID: 13546 RVA: 0x000D0A38 File Offset: 0x000CEC38
		public void \u0001(_IIndexAccessExpression \u0002)
		{
			this.\u0001(\u0002);
			\u0002._Var.Accept(this);
			for (int i = 0; i < \u0002.NumAccesses; i++)
			{
				\u0002.GetAccess(i).Accept(this);
			}
		}

		// Token: 0x060034EB RID: 13547 RVA: 0x000D0A78 File Offset: 0x000CEC78
		public void \u0001(_ICompoAccessExpression \u0002)
		{
			this.\u0001(\u0002);
			\u0002._Left.Accept(this);
			\u0002._Right.Accept(this);
		}

		// Token: 0x060034EC RID: 13548 RVA: 0x000D0A9C File Offset: 0x000CEC9C
		public void \u0001(_IDeRefAccessExpression \u0002)
		{
			this.\u0001(\u0002);
			\u0002._Base.Accept(this);
		}

		// Token: 0x060034ED RID: 13549 RVA: 0x000D0AB4 File Offset: 0x000CECB4
		public void \u0001(_ICopyScopeExpression \u0002)
		{
			this.\u0001(\u0002);
			\u0002._Base.Accept(this);
		}

		// Token: 0x060034EE RID: 13550 RVA: 0x000D0ACC File Offset: 0x000CECCC
		public void \u0001(_IGlobalScopeExpression \u0002)
		{
			this.\u0001(\u0002);
			\u0002._Base.Accept(this);
		}

		// Token: 0x060034EF RID: 13551 RVA: 0x000D0AE4 File Offset: 0x000CECE4
		public void \u0001(_ISystemScopeExpression \u0002)
		{
			this.\u0001(\u0002);
			\u0002._Base.Accept(this);
		}

		// Token: 0x060034F0 RID: 13552 RVA: 0x000D0AFC File Offset: 0x000CECFC
		public void \u0001(_IPoolScopeExpression \u0002)
		{
			this.\u0001(\u0002);
			\u0002._Base.Accept(this);
		}

		// Token: 0x060034F1 RID: 13553 RVA: 0x000D0B14 File Offset: 0x000CED14
		public void \u0001(_INamespaceAccessExpression \u0002)
		{
			this.\u0001(\u0002);
			_IExpression access = \u0002._Access;
			if (access == null)
			{
				return;
			}
			access.Accept(this);
		}

		// Token: 0x060034F2 RID: 13554 RVA: 0x000D0B30 File Offset: 0x000CED30
		public void \u0001(_ICurrentTaskExpression \u0002)
		{
			this.\u0001(\u0002);
			\u0002._Base.Accept(this);
		}

		// Token: 0x060034F3 RID: 13555 RVA: 0x000D0B48 File Offset: 0x000CED48
		public void \u0001(_IEmptyStatement \u0002)
		{
			if (this.VisitErrorStatements)
			{
				this.\u0001(\u0002);
			}
		}

		// Token: 0x060034F4 RID: 13556 RVA: 0x000D0B5C File Offset: 0x000CED5C
		public void \u0001(_ICaseRangeExpression \u0002)
		{
			this.\u0001(\u0002);
			\u0002._Low.Accept(this);
			\u0002._High.Accept(this);
		}

		// Token: 0x060034F5 RID: 13557 RVA: 0x000D0B80 File Offset: 0x000CED80
		public void \u0001(_ICaseLabelStatement \u0002)
		{
			this.\u0001(\u0002);
			foreach (_IExpression iexpression in \u0002._cases)
			{
				iexpression.Accept(this);
			}
		}

		// Token: 0x060034F6 RID: 13558 RVA: 0x000D0BD4 File Offset: 0x000CEDD4
		public void \u0001(_ICaseStatement \u0002)
		{
			this.\u0001(\u0002);
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

		// Token: 0x060034F7 RID: 13559 RVA: 0x000D0C58 File Offset: 0x000CEE58
		public void \u0001(_IErrorExpression \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x060034F8 RID: 13560 RVA: 0x000D0C64 File Offset: 0x000CEE64
		public void \u0001(_IErrorStatement \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x060034F9 RID: 13561 RVA: 0x000D0C70 File Offset: 0x000CEE70
		public void \u0001(_INullExpression \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x060034FA RID: 13562 RVA: 0x000D0C7C File Offset: 0x000CEE7C
		public void \u0001(_INullStatement \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x060034FB RID: 13563 RVA: 0x000D0C88 File Offset: 0x000CEE88
		public void \u0001(_IMultipleIndexInitialization \u0002)
		{
			this.\u0001(\u0002);
			\u0002._Number.Accept(this);
			\u0002._Value.Accept(this);
		}

		// Token: 0x060034FC RID: 13564 RVA: 0x000D0CAC File Offset: 0x000CEEAC
		public void \u0001(_IArrayInitialization \u0002)
		{
			this.\u0001(\u0002);
			foreach (_IExpression iexpression in \u0002._InitValues)
			{
				iexpression.Accept(this);
			}
		}

		// Token: 0x060034FD RID: 13565 RVA: 0x000D0D00 File Offset: 0x000CEF00
		public void \u0001(_IStructureInitialization \u0002)
		{
			this.\u0001(\u0002);
			foreach (_IAssignmentExpression iassignmentExpression in \u0002._CompoInits)
			{
				iassignmentExpression.Accept(this);
			}
		}

		// Token: 0x060034FE RID: 13566 RVA: 0x000D0D54 File Offset: 0x000CEF54
		public void \u0001(_IDefineReference \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x060034FF RID: 13567 RVA: 0x000D0D60 File Offset: 0x000CEF60
		public void \u0001(_IVariableReference \u0002)
		{
			this.\u0001(\u0002);
			_IExpression instancePath = \u0002.InstancePath;
			if (instancePath == null)
			{
				return;
			}
			instancePath.Accept(this);
		}

		// Token: 0x06003500 RID: 13568 RVA: 0x000D0D7C File Offset: 0x000CEF7C
		public void \u0001(_ITypeReference \u0002)
		{
			this.\u0001(\u0002);
			_IExpression instancePath = \u0002.InstancePath;
			if (instancePath == null)
			{
				return;
			}
			instancePath.Accept(this);
		}

		// Token: 0x06003501 RID: 13569 RVA: 0x000D0D98 File Offset: 0x000CEF98
		public void \u0001(_IPouReference \u0002)
		{
			this.\u0001(\u0002);
			_IExpression instancePath = \u0002.InstancePath;
			if (instancePath == null)
			{
				return;
			}
			instancePath.Accept(this);
		}

		// Token: 0x06003502 RID: 13570 RVA: 0x000D0DB4 File Offset: 0x000CEFB4
		public void \u0001(_ITaskReference \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x06003503 RID: 13571 RVA: 0x000D0DC0 File Offset: 0x000CEFC0
		public void \u0001(_IResourceReference \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x06003504 RID: 13572 RVA: 0x000D0DCC File Offset: 0x000CEFCC
		public void \u0001(_IDefinedExpression \u0002)
		{
			this.\u0001(\u0002);
			\u0002.ItemReference.Accept(this);
		}

		// Token: 0x06003505 RID: 13573 RVA: 0x000D0DE4 File Offset: 0x000CEFE4
		public void \u0001(_IPragmaOperatorExpression \u0002)
		{
			this.\u0001(\u0002);
			foreach (_IExpression iexpression in \u0002.Operands)
			{
				iexpression.Accept(this);
			}
		}

		// Token: 0x06003506 RID: 13574 RVA: 0x000D0E38 File Offset: 0x000CF038
		public void \u0001(_IPragmaIfStatement \u0002)
		{
			this.\u0001(\u0002);
			bool u = this.VisitErrorStatements;
			this.VisitErrorStatements = this.VisitErrorStatementsInConditionalPragmas;
			\u0002.Condition.Accept(this);
			\u0002.IfThen.Accept(this);
			foreach (_IPragmaElseIf ipragmaElseIf in \u0002.ElseIf)
			{
				ipragmaElseIf.Condition.Accept(this);
				ipragmaElseIf.Controlled.Accept(this);
			}
			_IStatement ifElse = \u0002.IfElse;
			if (ifElse != null)
			{
				ifElse.Accept(this);
			}
			this.VisitErrorStatements = u;
		}

		// Token: 0x06003507 RID: 13575 RVA: 0x000D0EE0 File Offset: 0x000CF0E0
		public void \u0001(_IPragmaAssertion \u0002)
		{
			this.\u0001(\u0002);
			\u0002.Condition.Accept(this);
		}

		// Token: 0x06003508 RID: 13576 RVA: 0x000D0EF8 File Offset: 0x000CF0F8
		public void \u0001(_IProjectDefinedExpression \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x06003509 RID: 13577 RVA: 0x000D0F04 File Offset: 0x000CF104
		public void \u0001(_ICompilerVersionExpression \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x0600350A RID: 13578 RVA: 0x000D0F10 File Offset: 0x000CF110
		public void \u0001(_IRuntimeVersionExpression \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x0600350B RID: 13579 RVA: 0x000D0F1C File Offset: 0x000CF11C
		public void \u0001(_IBreakPointStatement \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x0600350C RID: 13580 RVA: 0x000D0F28 File Offset: 0x000CF128
		public void \u0001(_IDefineStatement \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x0600350D RID: 13581 RVA: 0x000D0F34 File Offset: 0x000CF134
		public void \u0001(_IXRefExpression \u0002)
		{
			this.\u0001(\u0002);
			if (\u0002.XRef != null)
			{
				\u0002.XRef.Accept(this);
			}
			if (\u0002.XRefFrom != null)
			{
				\u0002.XRefFrom.Accept(this);
			}
		}

		// Token: 0x0600350E RID: 13582 RVA: 0x000D0F68 File Offset: 0x000CF168
		public void \u0001(_IHasTypeExpression \u0002)
		{
			this.\u0001(\u0002);
			if (\u0002.Variable != null)
			{
				\u0002.Variable.Accept(this);
			}
		}

		// Token: 0x0600350F RID: 13583 RVA: 0x000D0F88 File Offset: 0x000CF188
		public void \u0001(_IIsEnumTypeExpression \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x06003510 RID: 13584 RVA: 0x000D0F94 File Offset: 0x000CF194
		public void \u0001(_IHasAttributeExpression \u0002)
		{
			this.\u0001(\u0002);
			if (\u0002.ItemReference != null)
			{
				\u0002.ItemReference.Accept(this);
			}
		}

		// Token: 0x06003511 RID: 13585 RVA: 0x000D0FB4 File Offset: 0x000CF1B4
		public void \u0001(_IHasValueExpression \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x06003512 RID: 13586 RVA: 0x000D0FC0 File Offset: 0x000CF1C0
		public void \u0001(_IHasConstantValueExpression \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x06003513 RID: 13587 RVA: 0x000D0FCC File Offset: 0x000CF1CC
		public void \u0001(_IHasConstantTypeExpression \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x06003514 RID: 13588 RVA: 0x000D0FD8 File Offset: 0x000CF1D8
		public void \u0001(_IPartialAccessExpression \u0002)
		{
			this.\u0001(\u0002);
			\u0002._Left.Accept(this);
		}

		// Token: 0x06003515 RID: 13589 RVA: 0x000D0FF0 File Offset: 0x000CF1F0
		public void \u0001(_ITryCatchStatement \u0002)
		{
			this.\u0001(\u0002);
			\u0002.DefaultTraverse(this);
		}

		// Token: 0x06003516 RID: 13590 RVA: 0x000D1000 File Offset: 0x000CF200
		public void \u0001()
		{
			this.\u0001.Clear();
			this.\u0001.Clear();
			this.MessageGuid = Guid.Empty;
			this.VisitErrorStatements = true;
			this.VisitErrorStatementsInConditionalPragmas = true;
		}

		// Token: 0x04000A4C RID: 2636
		[CompilerGenerated]
		private bool \u0001;

		// Token: 0x04000A4D RID: 2637
		[CompilerGenerated]
		private bool \u0002;

		// Token: 0x04000A4E RID: 2638
		private readonly LDictionary<string, string> \u0001 = new LDictionary<string, string>();

		// Token: 0x04000A4F RID: 2639
		[CompilerGenerated]
		private Guid \u0001;

		// Token: 0x04000A50 RID: 2640
		private readonly LList<_ICompilerMessage> \u0001 = new LList<_ICompilerMessage>();
	}
}
