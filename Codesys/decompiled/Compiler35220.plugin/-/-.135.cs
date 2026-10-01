using System;
using System.Collections;
using System.Collections.Generic;
using \u0003;
using \u0019;
using CODESYS.Parser;
using _3S.CoDeSys.Compiler35220;
using _3S.CoDeSys.Compiler35220.PreCompile;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;
using \u0082;

namespace \u0004
{
	// Token: 0x02000186 RID: 390
	internal sealed class \u0004 : IExprementVisitor352000, IExprementVisitor351900, IExprementVisitor351800, IExprementVisitor351500, IExprementVisitor351400, IExprementVisitor351300, IExprementVisitor3590, IExprementVisitor
	{
		// Token: 0x06001AF1 RID: 6897 RVA: 0x00058574 File Offset: 0x00056774
		private bool \u0001(TypeClass \u0002, TypeClass \u0003)
		{
			return \u0003 - TypeClass.Any <= 5 || (\u0002 - TypeClass.AnyBit <= 2 || \u0002 == TypeClass.AnyReal);
		}

		// Token: 0x06001AF2 RID: 6898 RVA: 0x00058590 File Offset: 0x00056790
		public \u0004(Guid \u008D\u0003, int \u0096\u0002, _IPreCompCrossReferences \u008E\u0003, _IPreCompCrossReferences \u008F\u0003, _IPreCompCrossReferences \u0090\u0003, bool \u0091\u0003)
		{
			bool u = APEnvironmentFacade.Instance.ExistsPrimaryProject && \u0096\u0002 == APEnvironmentFacade.Instance.PrimaryProjectHandle;
			this.\u0001(\u008D\u0003, \u008E\u0003, \u008F\u0003, this.\u0003, \u0091\u0003, u);
		}

		// Token: 0x06001AF3 RID: 6899 RVA: 0x00058620 File Offset: 0x00056820
		public \u0004(Guid \u008D\u0003, _IPreCompCrossReferences \u008E\u0003, _IPreCompCrossReferences \u008F\u0003, _IPreCompCrossReferences \u0090\u0003, bool \u0091\u0003, bool \u0092\u0003)
		{
			this.\u0001(\u008D\u0003, \u008E\u0003, \u008F\u0003, \u0090\u0003, \u0091\u0003, \u0092\u0003);
		}

		// Token: 0x06001AF4 RID: 6900 RVA: 0x00058690 File Offset: 0x00056890
		private void \u0001(Guid \u0002, _IPreCompCrossReferences \u0003, _IPreCompCrossReferences \u0004, _IPreCompCrossReferences \u0005, bool \u0006, bool \u0007)
		{
			if (\u0007)
			{
				this.\u0001 = \u0003;
				this.\u0002 = \u0004;
				this.\u0003 = \u0005;
			}
			this.\u0001 = \u0002;
			this.\u0001 = \u0006;
		}

		// Token: 0x170005A5 RID: 1445
		// (get) Token: 0x06001AF5 RID: 6901 RVA: 0x000586BC File Offset: 0x000568BC
		// (set) Token: 0x06001AF6 RID: 6902 RVA: 0x000586C4 File Offset: 0x000568C4
		public Guid MessageGuid
		{
			get
			{
				return this.\u0002;
			}
			set
			{
				this.\u0002 = value;
			}
		}

		// Token: 0x170005A6 RID: 1446
		// (get) Token: 0x06001AF7 RID: 6903 RVA: 0x000586D0 File Offset: 0x000568D0
		public Guid ObjectGuid
		{
			get
			{
				return this.\u0001;
			}
		}

		// Token: 0x06001AF8 RID: 6904 RVA: 0x000586D8 File Offset: 0x000568D8
		private void \u0001(string \u0002, Guid \u0003, Guid \u0004)
		{
			if (this.\u0001 != null)
			{
				this.\u0001.Add(\u0002, \u0003, \u0004);
			}
		}

		// Token: 0x06001AF9 RID: 6905 RVA: 0x000586F0 File Offset: 0x000568F0
		private void \u0002(string \u0002, Guid \u0003, Guid \u0004)
		{
			if (this.\u0002 != null)
			{
				this.\u0002.Add(\u0002, \u0003, \u0004);
			}
		}

		// Token: 0x06001AFA RID: 6906 RVA: 0x00058708 File Offset: 0x00056908
		private void \u0003(string \u0002, Guid \u0003, Guid \u0004)
		{
			if (this.\u0003 != null)
			{
				this.\u0003.Add(\u0002, \u0003, \u0004);
			}
		}

		// Token: 0x06001AFB RID: 6907 RVA: 0x00058720 File Offset: 0x00056920
		private void \u0001()
		{
			this.\u0001 = 0;
			this.\u0001 = new ArrayList();
			this.\u0002 = new ArrayList();
			this.\u0001.Clear();
		}

		// Token: 0x06001AFC RID: 6908 RVA: 0x0005874C File Offset: 0x0005694C
		private bool \u0001(_IStatement \u0002)
		{
			if (\u0002 == null)
			{
				return true;
			}
			if (typeof(_INullStatement).IsAssignableFrom(\u0002.GetType()))
			{
				return true;
			}
			_ISequenceStatement isequenceStatement = \u0002 as _ISequenceStatement;
			return isequenceStatement != null && isequenceStatement._StatementList.Count == 0;
		}

		// Token: 0x06001AFD RID: 6909 RVA: 0x00058794 File Offset: 0x00056994
		private void \u0001(_IStatement \u0002, bool \u0003 = true)
		{
			if (this.\u0001(\u0002))
			{
				if (\u0003)
				{
					global::\u0003.\u0006.\u0002(\u0002, MessageId.Err_AtLeastOneExpected, Array.Empty<object>());
					return;
				}
				global::\u0003.\u0006.\u0001(\u0002, MessageId.Wrn_AtLeastOneExpected, Array.Empty<object>());
			}
		}

		// Token: 0x06001AFE RID: 6910 RVA: 0x000587C0 File Offset: 0x000569C0
		private void \u0001(_IExpression \u0002)
		{
			if (typeof(_ICaseRangeExpression).IsAssignableFrom(\u0002.GetType()))
			{
				global::\u0003.\u0006.\u0002(\u0002, MessageId.Err_NoValidCondition, new object[]
				{
					\u0002
				});
				return;
			}
			if (typeof(_INullExpression).IsAssignableFrom(\u0002.GetType()))
			{
				global::\u0003.\u0006.\u0002(\u0002, MessageId.Err_ConditionExpected, Array.Empty<object>());
				return;
			}
		}

		// Token: 0x06001AFF RID: 6911 RVA: 0x0005881C File Offset: 0x00056A1C
		public void \u0001(_ICompiledPOU \u0002)
		{
			this.\u0001();
			\u0002.GetParseTree().Accept(this);
			for (int i = this.\u0001.Count - 1; i >= 1; i--)
			{
				_ILabelStatement ilabelStatement = this.\u0001[i] as _ILabelStatement;
				for (int j = i - 1; j >= 0; j--)
				{
					_ILabelStatement ilabelStatement2 = this.\u0001[j] as _ILabelStatement;
					if (string.Compare(ilabelStatement.Text, ilabelStatement2.Text, StringComparison.OrdinalIgnoreCase) == 0)
					{
						global::\u0003.\u0006.\u0002(ilabelStatement, MessageId.Err_DuplicateLabelDefinition, new object[]
						{
							ilabelStatement.Text
						});
						this.\u0001.RemoveAt(i);
						ilabelStatement = ilabelStatement2;
						i = j;
					}
				}
			}
			bool[] u = this.\u0001();
			this.\u0001(u);
			if (this.\u0001 > 100)
			{
				this.\u0001(\u0002.GetParseTree(), Severity.Warning, MessageId.Wrn_ComplexExpression);
			}
			this.\u0001();
		}

		// Token: 0x06001B00 RID: 6912 RVA: 0x000588FC File Offset: 0x00056AFC
		private void \u0001(bool[] \u0002)
		{
			for (int i = 0; i < \u0002.Length; i++)
			{
				if (!\u0002[i])
				{
					_ILabelStatement ilabelStatement = this.\u0001[i] as _ILabelStatement;
					global::\u0003.\u0006.\u0001(ilabelStatement, Severity.Warning, MessageId.Wrn_LabelNoReference, new object[]
					{
						ilabelStatement.Text
					});
				}
			}
		}

		// Token: 0x06001B01 RID: 6913 RVA: 0x00058948 File Offset: 0x00056B48
		private bool[] \u0001()
		{
			bool[] array = new bool[this.\u0001.Count];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = false;
			}
			foreach (object obj in this.\u0002)
			{
				_IJumpStatement ijumpStatement = (_IJumpStatement)obj;
				bool flag = false;
				for (int j = 0; j < this.\u0001.Count; j++)
				{
					_ILabelStatement ilabelStatement = this.\u0001[j] as _ILabelStatement;
					if (string.Compare(ijumpStatement.Label, ilabelStatement.Text, StringComparison.OrdinalIgnoreCase) == 0)
					{
						flag = true;
						array[j] = true;
						break;
					}
				}
				if (!flag)
				{
					global::\u0003.\u0006.\u0002(ijumpStatement, MessageId.Err_NoSuchLabel, new object[]
					{
						ijumpStatement.Label
					});
				}
			}
			return array;
		}

		// Token: 0x06001B02 RID: 6914 RVA: 0x00058A30 File Offset: 0x00056C30
		private void \u0001(_IStatement \u0002, Severity \u0003, MessageId \u0004)
		{
			if (this.\u0001(\u0004))
			{
				\u0003 = Severity.SuppressedWarning;
			}
			global::\u0003.\u0006.\u0001(\u0002, \u0003, \u0004, Array.Empty<object>());
		}

		// Token: 0x06001B03 RID: 6915 RVA: 0x00058A4C File Offset: 0x00056C4C
		public bool \u0001(MessageId \u0002)
		{
			string text = string.Format("C{0:D4}", (int)\u0002);
			return this.\u0001.ContainsKey(text);
		}

		// Token: 0x06001B04 RID: 6916 RVA: 0x00058A7C File Offset: 0x00056C7C
		public void \u0001(_ISequenceStatement \u0002)
		{
			foreach (_IStatement istatement in \u0002._StatementList)
			{
				try
				{
					istatement.Accept(this);
				}
				catch
				{
				}
			}
		}

		// Token: 0x06001B05 RID: 6917 RVA: 0x00058ADC File Offset: 0x00056CDC
		public void \u0001(_IWhileStatement \u0002)
		{
			this.\u0001(\u0002._Condition);
			this.\u0001(\u0002._Controlled, true);
			this.\u0001(AccessModeFlags.Read);
			\u0002._Condition.Accept(this);
			this.\u0002();
			this.\u0001(true);
			\u0002._Controlled.Accept(this);
			this.\u0002();
		}

		// Token: 0x06001B06 RID: 6918 RVA: 0x00058B34 File Offset: 0x00056D34
		public void \u0001(_IRepeatStatement \u0002)
		{
			this.\u0001(\u0002._Condition);
			this.\u0001(\u0002._Controlled, true);
			this.\u0001(AccessModeFlags.Read);
			\u0002._Condition.Accept(this);
			this.\u0002();
			this.\u0001(true);
			\u0002._Controlled.Accept(this);
			this.\u0002();
		}

		// Token: 0x06001B07 RID: 6919 RVA: 0x00058B8C File Offset: 0x00056D8C
		private void \u0002(_IExpression \u0002)
		{
			if (!(\u0002 is _IAssignmentExpression))
			{
				global::\u0003.\u0006.\u0002(\u0002, MessageId.Err_CounterStartExpected, Array.Empty<object>());
				return;
			}
		}

		// Token: 0x06001B08 RID: 6920 RVA: 0x00058BA4 File Offset: 0x00056DA4
		private void \u0001(_IExpression \u0002, bool \u0003)
		{
			if (\u0002 == null)
			{
				return;
			}
			Type type = \u0002.GetType();
			if (typeof(_INullExpression).IsAssignableFrom(type) || typeof(_ICaseRangeExpression).IsAssignableFrom(type))
			{
				if (\u0003)
				{
					global::\u0003.\u0006.\u0002(\u0002, MessageId.Err_UpperBoundExpected, Array.Empty<object>());
					return;
				}
				global::\u0003.\u0006.\u0002(\u0002, MessageId.Err_InvalidLoopIncrement, Array.Empty<object>());
			}
		}

		// Token: 0x06001B09 RID: 6921 RVA: 0x00058C00 File Offset: 0x00056E00
		public void \u0001(_IForStatement \u0002)
		{
			this.\u0002(\u0002._CounterStart);
			this.\u0001(\u0002._UpperBound, true);
			this.\u0001(\u0002._By, false);
			this.\u0001(\u0002._Controlled, true);
			\u0002._CounterStart.Accept(this);
			this.\u0001(AccessModeFlags.Read);
			\u0002._UpperBound.Accept(this);
			if (\u0002.By != null)
			{
				\u0002._By.Accept(this);
			}
			this.\u0002();
			this.\u0001(true);
			\u0002._Controlled.Accept(this);
			this.\u0002();
		}

		// Token: 0x06001B0A RID: 6922 RVA: 0x00058C94 File Offset: 0x00056E94
		public void \u0001(_IExitStatement \u0002)
		{
			if (this.TopOfStack == null || !this.TopOfStack.\u0001)
			{
				global::\u0003.\u0006.\u0002(\u0002, MessageId.Err_NoEnclosingLoopExit, new object[]
				{
					Scanner.GetTextOfOperator(Operator.Exit).ToLowerInvariant()
				});
			}
		}

		// Token: 0x06001B0B RID: 6923 RVA: 0x00058CCC File Offset: 0x00056ECC
		public void \u0001(_IContinueStatement \u0002)
		{
			if (this.TopOfStack == null || !this.TopOfStack.\u0001)
			{
				global::\u0003.\u0006.\u0002(\u0002, MessageId.Err_NoEnclosingLoopExit, new object[]
				{
					Scanner.GetTextOfOperator(Operator.Continue).ToLowerInvariant()
				});
			}
		}

		// Token: 0x06001B0C RID: 6924 RVA: 0x00058D04 File Offset: 0x00056F04
		private void \u0003(_IExpression \u0002)
		{
		}

		// Token: 0x06001B0D RID: 6925 RVA: 0x00058D08 File Offset: 0x00056F08
		private void \u0004(_IExpression \u0002)
		{
			Type type = \u0002.GetType();
			if (typeof(INullExpression).IsAssignableFrom(type) || typeof(_ICaseRangeExpression).IsAssignableFrom(type))
			{
				global::\u0003.\u0006.\u0002(\u0002, MessageId.Err_RValueRequired, Array.Empty<object>());
			}
		}

		// Token: 0x06001B0E RID: 6926 RVA: 0x00058D50 File Offset: 0x00056F50
		public void \u0001(_IAssignmentExpression \u0002)
		{
			this.\u0003(\u0002._LValue);
			this.\u0004(\u0002._RValue);
			this.\u0001(AccessModeFlags.Write);
			\u0002._LValue.Accept(this);
			this.\u0002();
			this.\u0001(AccessModeFlags.Read);
			\u0002._RValue.Accept(this);
			this.\u0002();
		}

		// Token: 0x06001B0F RID: 6927 RVA: 0x00058DA8 File Offset: 0x00056FA8
		public void \u0001(_IIfStatement \u0002)
		{
			this.\u0001(\u0002._Condition);
			this.\u0001(\u0002._IfThen, true);
			this.\u0001(AccessModeFlags.Read);
			\u0002._Condition.Accept(this);
			this.\u0002();
			\u0002._IfThen.Accept(this);
			foreach (_IElseIf ielseIf in \u0002._ElseIf)
			{
				this.\u0001(ielseIf._Condition);
				this.\u0001(ielseIf._Controlled, true);
				this.\u0001(AccessModeFlags.Read);
				ielseIf._Condition.Accept(this);
				this.\u0002();
				ielseIf._Controlled.Accept(this);
			}
			if (\u0002.IfElse != null)
			{
				this.\u0001(\u0002._IfElse, true);
				\u0002._IfElse.Accept(this);
			}
		}

		// Token: 0x06001B10 RID: 6928 RVA: 0x00058E8C File Offset: 0x0005708C
		public void \u0001(_IReturnStatement \u0002)
		{
			if (\u0002._Condition != null)
			{
				this.\u0001(AccessModeFlags.Read);
				\u0002._Condition.Accept(this);
				this.\u0002();
				this.\u0001(\u0002._Condition);
			}
		}

		// Token: 0x06001B11 RID: 6929 RVA: 0x00058EBC File Offset: 0x000570BC
		public void \u0001(_IJumpStatement \u0002)
		{
			if (\u0002._Condition != null)
			{
				this.\u0001(AccessModeFlags.Read);
				\u0002._Condition.Accept(this);
				this.\u0002();
				this.\u0001(\u0002._Condition);
			}
			this.\u0002.Add(\u0002);
		}

		// Token: 0x06001B12 RID: 6930 RVA: 0x00058EF8 File Offset: 0x000570F8
		public void \u0001(_ILabelStatement \u0002)
		{
			this.\u0001.Add(\u0002);
		}

		// Token: 0x06001B13 RID: 6931 RVA: 0x00058F08 File Offset: 0x00057108
		public void \u0001(_ICommentStatement \u0002)
		{
		}

		// Token: 0x06001B14 RID: 6932 RVA: 0x00058F0C File Offset: 0x0005710C
		public void \u0001(_IPragmaStatement \u0002)
		{
			string text = \u0002.Text;
			IPragmaScanner pragmaScanner = \u0082.\u0005.Singleton.Create(text);
			IPragmaToken pragmaToken;
			if (pragmaScanner.GetNext(out pragmaToken) == PragmaTokenType.Operator && pragmaToken.Operator == PragmaOperator.attribute && pragmaScanner.GetNext(out pragmaToken) == PragmaTokenType.SingleByteString)
			{
				string @string = pragmaToken.String;
				string u = string.Empty;
				if (pragmaScanner.GetNext(out pragmaToken) == PragmaTokenType.Operator && pragmaToken.Operator == PragmaOperator.assign && pragmaScanner.GetNext(out pragmaToken) == PragmaTokenType.SingleByteString)
				{
					u = pragmaToken.String;
				}
				this.\u0001.Add(\u0019.\u0003.\u0001(@string, u));
			}
			else if (\u0002 is _IMessageGuidPragmaStatement)
			{
				this.MessageGuid = (\u0002 as _IMessageGuidPragmaStatement).MessageGuid;
			}
			this.\u0002(\u0002);
		}

		// Token: 0x06001B15 RID: 6933 RVA: 0x00058FB8 File Offset: 0x000571B8
		private void \u0002(_IPragmaStatement \u0002)
		{
			if (\u0002 is _IWarningDisableRestorePragmaStatement)
			{
				_IWarningDisableRestorePragmaStatement iwarningDisableRestorePragmaStatement = \u0002 as _IWarningDisableRestorePragmaStatement;
				if (iwarningDisableRestorePragmaStatement.Restore)
				{
					if (this.\u0001.ContainsKey(iwarningDisableRestorePragmaStatement.Id))
					{
						this.\u0001.Remove(iwarningDisableRestorePragmaStatement.Id);
						return;
					}
				}
				else if (!this.\u0001.ContainsKey(iwarningDisableRestorePragmaStatement.Id))
				{
					this.\u0001.Add(iwarningDisableRestorePragmaStatement.Id, iwarningDisableRestorePragmaStatement.Id);
				}
			}
		}

		// Token: 0x06001B16 RID: 6934 RVA: 0x0005902C File Offset: 0x0005722C
		public static bool \u0001(_IExpressionStatement \u0002)
		{
			_IExpression expr = \u0002._Expr;
			return !(expr is ICaseRangeExpression) && !(expr is INullExpression) && !(expr is IOperatorExpression) && !(expr is IConversionExpression) && !(expr is ICallExpression) && !(expr is IAssignmentExpression);
		}

		// Token: 0x06001B17 RID: 6935 RVA: 0x00059074 File Offset: 0x00057274
		private static bool \u0002(_IExpressionStatement \u0002)
		{
			_IExpression expr = \u0002._Expr;
			IOperatorExpression operatorExpression = expr as IOperatorExpression;
			if (operatorExpression == null)
			{
				return expr is ICaseRangeExpression || expr is INullExpression || expr is IConversionExpression;
			}
			return global::\u0004.\u0004.\u0001(operatorExpression);
		}

		// Token: 0x06001B18 RID: 6936 RVA: 0x000590B8 File Offset: 0x000572B8
		private static bool \u0001(IOperatorExpression \u0002)
		{
			Operator code = \u0002.Code;
			if (code <= Operator.__Throw)
			{
				if (code <= Operator.__Delete)
				{
					if (code != Operator.__Init && code - Operator.__QueryInterface > 3)
					{
						return true;
					}
				}
				else if (code - Operator.__FCall > 1 && code != Operator.__Throw)
				{
					return true;
				}
			}
			else if (code <= Operator.__CheckLicenseBit)
			{
				if (code - Operator.__CheckLicense > 2 && code != Operator.__CheckLicenseBit)
				{
					return true;
				}
			}
			else if (code != Operator.__MemoryBarrier && code != Operator.__vcStore)
			{
				return true;
			}
			return false;
		}

		// Token: 0x06001B19 RID: 6937 RVA: 0x00059134 File Offset: 0x00057334
		public void \u0001(_IExpressionStatement \u0002)
		{
			if (global::\u0004.\u0004.\u0002(\u0002))
			{
				global::\u0003.\u0006.\u0002(\u0002, MessageId.Err_NoValidStatement, new object[]
				{
					\u0002
				});
			}
			else if (global::\u0004.\u0004.\u0001(\u0002))
			{
				global::\u0003.\u0006.\u0001(\u0002, Severity.Warning, MessageId.Wrn_StatementNoEffect, new object[]
				{
					\u0002
				});
			}
			this.\u0001(AccessModeFlags.Read, true);
			\u0002._Expr.Accept(this);
			this.\u0002();
		}

		// Token: 0x06001B1A RID: 6938 RVA: 0x00059198 File Offset: 0x00057398
		private void \u0001(_IType \u0002)
		{
			if (\u0002 == null)
			{
				return;
			}
			if (\u0002.Class == TypeClass.Userdef)
			{
				_IUserdefType iuserdefType = \u0002 as _IUserdefType;
				this.\u0001(AccessModeFlags.Read);
				(iuserdefType.NameExpression as _IExpression).Accept(this);
				this.\u0002();
				return;
			}
			if (\u0002.Class == TypeClass.Pointer)
			{
				_IPointerType ipointerType = \u0002 as _IPointerType;
				this.\u0001(ipointerType._Base);
				return;
			}
			if (\u0002.Class == TypeClass.Reference)
			{
				_IReferenceType ireferenceType = \u0002 as _IReferenceType;
				this.\u0001(ireferenceType._Base);
				return;
			}
			if (\u0002.Class == TypeClass.Array)
			{
				_IArrayType iarrayType = \u0002 as _IArrayType;
				this.\u0001(iarrayType._Base);
				using (IEnumerator<_IArrayDimension> enumerator = iarrayType._Dimensions.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						_IArrayDimension iarrayDimension = enumerator.Current;
						if (iarrayDimension._LowerBorder is IVariableExpression)
						{
							this.\u0001(AccessModeFlags.Read);
							iarrayDimension._LowerBorder.Accept(this);
							this.\u0002();
						}
						if (iarrayDimension._UpperBorder is IVariableExpression)
						{
							this.\u0001(AccessModeFlags.Read);
							iarrayDimension._UpperBorder.Accept(this);
							this.\u0002();
						}
					}
					return;
				}
			}
			if (\u0002.Class == TypeClass.String)
			{
				_IStringType istringType = \u0002 as _IStringType;
				if (istringType.Length != null)
				{
					this.\u0001(AccessModeFlags.Read);
					istringType.Length.Accept(this);
					this.\u0002();
					return;
				}
			}
			else if (\u0002.Class == TypeClass.WString)
			{
				_IWStringType iwstringType = \u0002 as _IWStringType;
				if (iwstringType.Length != null)
				{
					this.\u0001(AccessModeFlags.Read);
					iwstringType.Length.Accept(this);
					this.\u0002();
					return;
				}
			}
			else if (\u0002.Class == TypeClass.Subrange)
			{
				_ISubrangeType isubrangeType = \u0002 as _ISubrangeType;
				if (isubrangeType._LowerBorder is IVariableExpression)
				{
					this.\u0001(AccessModeFlags.Read);
					isubrangeType._LowerBorder.Accept(this);
					this.\u0002();
				}
				if (isubrangeType._UpperBorder is IVariableExpression)
				{
					this.\u0001(AccessModeFlags.Read);
					isubrangeType._UpperBorder.Accept(this);
					this.\u0002();
				}
			}
		}

		// Token: 0x06001B1B RID: 6939 RVA: 0x00059394 File Offset: 0x00057594
		public void \u0001(_IVariableDeclarationStatement \u0002)
		{
			foreach (_IExprement iexprement in \u0002.NameList)
			{
				this.\u0001(AccessModeFlags.Declaration);
				iexprement.Accept(this);
				this.\u0002();
				this.\u0001(\u0002.Type);
				if (\u0002.Initial != null)
				{
					this.\u0001(AccessModeFlags.Read);
					\u0002.Initial.Accept(this);
					this.\u0002();
				}
				if (\u0002.InputAssigns != null)
				{
					foreach (_IAssignmentExpression iassignmentExpression in \u0002.InputAssigns)
					{
						iassignmentExpression.Accept(this);
					}
				}
			}
		}

		// Token: 0x06001B1C RID: 6940 RVA: 0x00059468 File Offset: 0x00057668
		public void \u0001(_IVariableDeclarationListStatement \u0002)
		{
			\u0002.VariableDeclaration.Accept(this);
		}

		// Token: 0x06001B1D RID: 6941 RVA: 0x00059478 File Offset: 0x00057678
		public void \u0001(_IPOUDeclarationStatement \u0002)
		{
			IType type = \u0002.ReturnType;
			if (type is _IReferenceType)
			{
				type = (type as _IReferenceType)._Base;
			}
			if (type is _IPointerType)
			{
				type = (type as _IPointerType).Base;
			}
			if (type != null && type is _IUserdefType)
			{
				_IVariableExpression ivariableExpression = (type as _IUserdefType).NameExpression as _IVariableExpression;
				if (ivariableExpression != null)
				{
					this.\u0001(AccessModeFlags.Type);
					this.\u0001(ivariableExpression);
					this.\u0002();
				}
			}
			this.\u0001(AccessModeFlags.Declaration);
			\u0002.Declarations.Accept(this);
			this.\u0002();
		}

		// Token: 0x06001B1E RID: 6942 RVA: 0x00059508 File Offset: 0x00057708
		public void \u0001(_ITypeDeclarationStatement \u0002)
		{
			this.\u0001(AccessModeFlags.Declaration);
			\u0002.Declarations.Accept(this);
			this.\u0002();
		}

		// Token: 0x06001B1F RID: 6943 RVA: 0x00059528 File Offset: 0x00057728
		public void \u0001(_IEnumDeclarationStatement \u0002)
		{
			this.\u0001(\u0002.Name, this.ObjectGuid, this.MessageGuid);
		}

		// Token: 0x06001B20 RID: 6944 RVA: 0x00059544 File Offset: 0x00057744
		public void \u0001(_IEnumDeclarationListStatement \u0002)
		{
			foreach (_IEnumDeclarationStatement ienumDeclarationStatement in \u0002.Enums)
			{
				ienumDeclarationStatement.Accept(this);
			}
		}

		// Token: 0x06001B21 RID: 6945 RVA: 0x00059590 File Offset: 0x00057790
		private void \u0005(_IExpression \u0002)
		{
			Type type = \u0002.GetType();
			if (!typeof(_ICallExpression).IsAssignableFrom(type) && !typeof(_IThisExpression).IsAssignableFrom(type) && !typeof(_IVariableExpression).IsAssignableFrom(type) && !typeof(_IIndexAccessExpression).IsAssignableFrom(type) && !typeof(_ICompoAccessExpression).IsAssignableFrom(type) && !typeof(_IDeRefAccessExpression).IsAssignableFrom(type) && !typeof(_ICopyScopeExpression).IsAssignableFrom(type) && !typeof(_INamespaceAccessExpression).IsAssignableFrom(type) && !typeof(_IGlobalScopeExpression).IsAssignableFrom(type) && !typeof(_IPoolScopeExpression).IsAssignableFrom(type))
			{
				global::\u0003.\u0006.\u0002(\u0002, MessageId.Err_CalleeInvalidType, new object[]
				{
					\u0002
				});
			}
		}

		// Token: 0x06001B22 RID: 6946 RVA: 0x00059674 File Offset: 0x00057874
		public void \u0001(_ICallExpression \u0002)
		{
			this.\u0005(\u0002._Callee);
			this.\u0001(AccessModeFlags.Call);
			\u0002._Callee.Accept(this);
			this.\u0002();
			if (\u0002._Condition != null)
			{
				this.\u0001(AccessModeFlags.Read);
				\u0002._Condition.Accept(this);
				this.\u0002();
				this.\u0001(\u0002._Condition);
			}
			foreach (_IExpression iexpression in \u0002.ParamExpressions)
			{
				if (iexpression != null)
				{
					this.\u0004(iexpression);
					this.\u0001(AccessModeFlags.Read);
					iexpression.Accept(this);
					this.\u0002();
				}
			}
			foreach (_IExpression iexpression2 in \u0002.OutputExpressions)
			{
				if (iexpression2 != null)
				{
					this.\u0003(iexpression2);
					this.\u0001(AccessModeFlags.Write);
					iexpression2.Accept(this);
					this.\u0002();
				}
			}
			foreach (_IExpression iexpression3 in \u0002.Inputs)
			{
				_IVariableExpression ivariableExpression = (_IVariableExpression)iexpression3;
				if (ivariableExpression != null)
				{
					this.\u0001(AccessModeFlags.Write);
					ivariableExpression.Accept(this);
					this.\u0002();
				}
			}
			foreach (_IExpression iexpression4 in \u0002.Outputs)
			{
				_IVariableExpression ivariableExpression2 = (_IVariableExpression)iexpression4;
				if (ivariableExpression2 != null)
				{
					this.\u0001(AccessModeFlags.Read);
					ivariableExpression2.Accept(this);
					this.\u0002();
				}
			}
		}

		// Token: 0x06001B23 RID: 6947 RVA: 0x00059824 File Offset: 0x00057A24
		private void \u0006(_IExpression \u0002)
		{
			Type type = \u0002.GetType();
			if (type == typeof(_INullExpression) || type == typeof(_ICaseRangeExpression))
			{
				global::\u0003.\u0006.\u0002(\u0002, MessageId.Err_NoValidOperand, new object[]
				{
					\u0002
				});
			}
		}

		// Token: 0x06001B24 RID: 6948 RVA: 0x00059870 File Offset: 0x00057A70
		private bool \u0001(_IOperatorExpression \u0002)
		{
			bool result = true;
			Operator code = \u0002.Code;
			if (code <= Operator.__AdrInst)
			{
				if (code - Operator.Time > 1 && code != Operator.__AdrInst)
				{
					goto IL_71;
				}
			}
			else if (code != Operator.__GetLTick && code != Operator.__MemoryBarrier && code - Operator.__PouName > 1)
			{
				goto IL_71;
			}
			if (\u0002._OperandsList.Count > 0)
			{
				global::\u0003.\u0006.\u0002(\u0002, MessageId.Err_OpNeedsAtLeastInputs, new object[]
				{
					Scanner.GetTextOfOperator(\u0002.Code),
					0
				});
				return result;
			}
			return result;
			IL_71:
			result = false;
			return result;
		}

		// Token: 0x06001B25 RID: 6949 RVA: 0x000598F4 File Offset: 0x00057AF4
		private bool \u0002(_IOperatorExpression \u0002)
		{
			bool result = true;
			Operator code = \u0002.Code;
			if (code <= Operator.TestAndSet)
			{
				switch (code)
				{
				case Operator.Adr:
				case Operator.BitAdr:
				case Operator.SizeOf:
				case Operator.Abs:
				case Operator.Trunc:
				case Operator.Exp:
				case Operator.Sqrt:
				case Operator.Ln:
				case Operator.Log:
				case Operator.Sin:
				case Operator.Cos:
				case Operator.Tan:
				case Operator.ASin:
				case Operator.ACos:
				case Operator.ATan:
					break;
				case Operator.IndexOf:
				case Operator.Ini:
				case Operator.Limit:
				case Operator.Min:
				case Operator.Max:
				case Operator.Mux:
				case Operator.Sel:
				case Operator.Rol:
				case Operator.Ror:
				case Operator.Shl:
				case Operator.Shr:
				case Operator.Expt:
					goto IL_14A;
				default:
					if (code != Operator.Not && code - Operator.Move > 1)
					{
						goto IL_14A;
					}
					break;
				}
			}
			else if (code <= Operator.__LateCompiledExpr)
			{
				switch (code)
				{
				case Operator.TruncInt:
				case Operator.__LocalOffset:
				case Operator.__TypeOf:
				case Operator.__CRC:
				case Operator.__MaxOffset:
				case Operator.__Init:
				case Operator.__IsValidRef:
				case Operator.__Delete:
				case Operator.__RefAdr:
					break;
				case Operator.FupAssign:
				case Operator.__VarInfo:
				case Operator.__SystemScope:
				case Operator.__QueryInterface:
				case Operator.__QueryPointer:
				case Operator.__New:
				case Operator.__Cast:
				case Operator.__AdrInst:
					goto IL_14A;
				default:
					if (code != Operator.__LateCompiledExpr)
					{
						goto IL_14A;
					}
					break;
				}
			}
			else if (code != Operator.__vcSqrt && code != Operator.XSizeOf)
			{
				goto IL_14A;
			}
			if (\u0002._OperandsList.Count != 1)
			{
				global::\u0003.\u0006.\u0002(\u0002, MessageId.Err_OpNeedsExactInputs, new object[]
				{
					Scanner.GetTextOfOperator(\u0002.Code),
					1
				});
				return result;
			}
			return result;
			IL_14A:
			result = false;
			return result;
		}

		// Token: 0x06001B26 RID: 6950 RVA: 0x00059A50 File Offset: 0x00057C50
		private bool \u0003(_IOperatorExpression \u0002)
		{
			bool result = true;
			Operator code = \u0002.Code;
			if (code <= Operator.Lt)
			{
				if (code <= Operator.Ini)
				{
					if (code != Operator.__Reloc && code != Operator.Ini)
					{
						goto IL_171;
					}
				}
				else if (code - Operator.Rol > 3 && code != Operator.Expt)
				{
					switch (code)
					{
					case Operator.Sub:
					case Operator.Div:
					case Operator.Mod:
					case Operator.Eq:
					case Operator.Ne:
					case Operator.Ge:
					case Operator.Gt:
					case Operator.Le:
					case Operator.Lt:
						break;
					case Operator.Mul:
					case Operator.And:
					case Operator.AndN:
					case Operator.Or:
					case Operator.OrN:
					case Operator.Xor:
					case Operator.XorN:
					case Operator.Not:
						goto IL_171;
					default:
						goto IL_171;
					}
				}
			}
			else if (code <= Operator.NotEqual)
			{
				if (code != Operator.Minus && code - Operator.Power > 1 && code - Operator.Less > 5)
				{
					goto IL_171;
				}
			}
			else if (code - Operator.__QueryInterface > 1 && code != Operator.__PropertyInfo)
			{
				switch (code)
				{
				case Operator.LowerBound:
				case Operator.UpperBound:
				case Operator.__XAdd:
				case Operator.__vcAdd:
				case Operator.__vcSub:
				case Operator.__vcMul:
				case Operator.__vcDiv:
				case Operator.__vcDot:
				case Operator.__vcMin:
				case Operator.__vcMax:
				case Operator.__vcLoadReal:
				case Operator.__vcLoadLReal:
				case Operator.__vcStore:
					break;
				case Operator.AnyString:
				case Operator.__PoolScope:
				case Operator.__CheckLicenseBit:
				case Operator.__MemoryBarrier:
				case Operator.__CurrentTask:
				case Operator.__CompareAndSwap:
				case Operator.__Vector:
				case Operator.__vcSqrt:
				case Operator.__vcSetReal:
				case Operator.__vcSetLReal:
					goto IL_171;
				default:
					goto IL_171;
				}
			}
			if (\u0002._OperandsList.Count != 2)
			{
				global::\u0003.\u0006.\u0002(\u0002, MessageId.Err_OpNeedsExactInputs, new object[]
				{
					Scanner.GetTextOfOperator(\u0002.Code),
					2
				});
				return result;
			}
			return result;
			IL_171:
			result = false;
			return result;
		}

		// Token: 0x06001B27 RID: 6951 RVA: 0x00059BD4 File Offset: 0x00057DD4
		private bool \u0004(_IOperatorExpression \u0002)
		{
			bool result = true;
			Operator code = \u0002.Code;
			if (code <= Operator.Plus)
			{
				if (code - Operator.Min > 1)
				{
					switch (code)
					{
					case Operator.Add:
					case Operator.Mul:
					case Operator.And:
					case Operator.AndN:
					case Operator.Or:
					case Operator.OrN:
					case Operator.Xor:
					case Operator.XorN:
						break;
					case Operator.Sub:
					case Operator.Div:
					case Operator.Mod:
						goto IL_BA;
					default:
						if (code != Operator.Plus)
						{
							goto IL_BA;
						}
						break;
					}
				}
			}
			else if (code <= Operator.VerticalLine)
			{
				if (code != Operator.Times && code - Operator.Ampersand > 1)
				{
					goto IL_BA;
				}
			}
			else if (code != Operator.__FCall && code - Operator.And_Then > 1)
			{
				goto IL_BA;
			}
			if (\u0002._OperandsList.Count < 2)
			{
				global::\u0003.\u0006.\u0002(\u0002, MessageId.Err_OpNeedsAtLeastInputs, new object[]
				{
					Scanner.GetTextOfOperator(\u0002.Code),
					2
				});
				return result;
			}
			return result;
			IL_BA:
			result = false;
			return result;
		}

		// Token: 0x06001B28 RID: 6952 RVA: 0x00059CA0 File Offset: 0x00057EA0
		private bool \u0005(_IOperatorExpression \u0002)
		{
			bool result = true;
			Operator code = \u0002.Code;
			if (code == Operator.Limit || code == Operator.Sel || code == Operator.__CompareAndSwap)
			{
				if (\u0002._OperandsList.Count != 3)
				{
					global::\u0003.\u0006.\u0002(\u0002, MessageId.Err_OpNeedsExactInputs, new object[]
					{
						Scanner.GetTextOfOperator(\u0002.Code),
						3
					});
				}
			}
			else
			{
				result = false;
			}
			return result;
		}

		// Token: 0x06001B29 RID: 6953 RVA: 0x00059D00 File Offset: 0x00057F00
		private bool \u0006(_IOperatorExpression \u0002)
		{
			bool result = true;
			Operator code = \u0002.Code;
			if (code <= Operator.__MemorySet)
			{
				if (code <= Operator.__New)
				{
					if (code != Operator.Mux)
					{
						if (code == Operator.__New)
						{
							if (\u0002._OperandsList.Count < 1 || \u0002._OperandsList.Count > 2)
							{
								global::\u0003.\u0006.\u0002(\u0002, MessageId.Err_OpNeedsExactInputs, new object[]
								{
									Scanner.GetTextOfOperator(\u0002.Code),
									1
								});
								return result;
							}
							return result;
						}
					}
					else
					{
						if (\u0002._OperandsList.Count < 3)
						{
							global::\u0003.\u0006.\u0002(\u0002, MessageId.Err_OpNeedsAtLeastInputs, new object[]
							{
								Scanner.GetTextOfOperator(\u0002.Code),
								3
							});
							return result;
						}
						return result;
					}
				}
				else if (code != Operator.__BitOffset)
				{
					if (code == Operator.__MemorySet)
					{
						if (\u0002._OperandsList.Count != 3)
						{
							global::\u0003.\u0006.\u0002(\u0002, MessageId.Err_OpNeedsExactInputs, new object[]
							{
								Scanner.GetTextOfOperator(\u0002.Code),
								3
							});
							return result;
						}
						return result;
					}
				}
				else
				{
					if (\u0002._OperandsList.Count > 3 || \u0002._OperandsList.Count == 2)
					{
						global::\u0003.\u0006.\u0002(\u0002, MessageId.Err_OpTakesAtMostInputs, new object[]
						{
							Scanner.GetTextOfOperator(\u0002.Code),
							3
						});
						return result;
					}
					return result;
				}
			}
			else if (code <= Operator.__CallInitFunction)
			{
				if (code != Operator.__Throw)
				{
					if (code - Operator.__CheckLicense <= 1)
					{
						if (\u0002._OperandsList.Count != 2)
						{
							global::\u0003.\u0006.\u0002(\u0002, MessageId.Err_OpNeedsExactInputs, new object[]
							{
								Scanner.GetTextOfOperator(\u0002.Code),
								2
							});
							return result;
						}
						return result;
					}
				}
				else
				{
					if (\u0002._OperandsList.Count != 1)
					{
						global::\u0003.\u0006.\u0002(\u0002, MessageId.Err_OpNeedsExactInputs, new object[]
						{
							Scanner.GetTextOfOperator(\u0002.Code),
							1
						});
						return result;
					}
					return result;
				}
			}
			else if (code != Operator.__CheckLicenseBit)
			{
				if (code - Operator.__vcSetReal <= 1)
				{
					if (\u0002._OperandsList.Count < 1)
					{
						global::\u0003.\u0006.\u0002(\u0002, MessageId.Err_OpNeedsAtLeastInputs, new object[]
						{
							Scanner.GetTextOfOperator(\u0002.Code),
							1
						});
						return result;
					}
					return result;
				}
			}
			else
			{
				if (\u0002._OperandsList.Count != 3)
				{
					global::\u0003.\u0006.\u0002(\u0002, MessageId.Err_OpNeedsExactInputs, new object[]
					{
						Scanner.GetTextOfOperator(\u0002.Code),
						3
					});
					return result;
				}
				return result;
			}
			result = false;
			return result;
		}

		// Token: 0x06001B2A RID: 6954 RVA: 0x00059F80 File Offset: 0x00058180
		private void \u0001(_IOperatorExpression \u0002)
		{
			if (\u0002._OperandsList.Count == 1)
			{
				bool flag = false;
				_IExprement iexprement = \u0002._OperandsList[0];
				if (iexprement != null)
				{
					if (iexprement is ILiteralExpression || iexprement is IOperatorExpression)
					{
						flag = true;
					}
					else if (iexprement is IThisExpression || iexprement is IBaseExpression)
					{
						flag = true;
					}
					else if (iexprement is IDeRefAccessExpression)
					{
						flag = true;
					}
				}
				if (flag)
				{
					global::\u0003.\u0006.\u0002(iexprement, MessageId.Err_UnexpectedOperandForIndexOf, new object[]
					{
						iexprement,
						Scanner.GetTextOfOperator(\u0002.Code)
					});
					return;
				}
			}
			else
			{
				global::\u0003.\u0006.\u0002(\u0002, MessageId.Err_OpNeedsExactInputs, new object[]
				{
					Scanner.GetTextOfOperator(\u0002.Code),
					1
				});
			}
		}

		// Token: 0x06001B2B RID: 6955 RVA: 0x0005A028 File Offset: 0x00058228
		public void \u0002(_IOperatorExpression \u0002)
		{
			Operator code = \u0002.Code;
			if ((code == Operator.__Init || code == Operator.__Delete) && !this.TopOfStack.\u0002)
			{
				global::\u0003.\u0006.\u0002(\u0002, MessageId.Err_OperatorNotAllowedAtPosition, new object[]
				{
					\u0002.Code.ToString()
				});
			}
			bool flag = true;
			if (!this.\u0003(\u0002) && !this.\u0002(\u0002) && !this.\u0004(\u0002) && !this.\u0005(\u0002) && !this.\u0001(\u0002) && !this.\u0006(\u0002))
			{
				flag = false;
			}
			if (!flag)
			{
				code = \u0002.Code;
				if (code != Operator.IndexOf)
				{
					if (code != Operator.__VarInfo)
					{
						global::\u0003.\u0006.\u0002(\u0002, MessageId.Err_IllegalOperator, new object[]
						{
							Scanner.GetTextOfOperator(\u0002.Code)
						});
					}
					else
					{
						this.\u0001(\u0002);
					}
				}
				else
				{
					if (\u0002._OperandsList.Count != 1)
					{
						global::\u0003.\u0006.\u0002(\u0002, MessageId.Err_OpNeedsExactInputs, new object[]
						{
							Scanner.GetTextOfOperator(\u0002.Code),
							1
						});
					}
					global::\u0003.\u0006.\u0002(\u0002, MessageId.Err_IndexOfNotSupported, Array.Empty<object>());
				}
			}
			foreach (_IExpression iexpression in \u0002._OperandsList)
			{
				this.\u0006(iexpression);
				this.\u0001(AccessModeFlags.Read);
				iexpression.Accept(this);
				this.\u0002();
			}
		}

		// Token: 0x06001B2C RID: 6956 RVA: 0x0005A198 File Offset: 0x00058398
		public void \u0001(_ICastExpression \u0002)
		{
		}

		// Token: 0x06001B2D RID: 6957 RVA: 0x0005A19C File Offset: 0x0005839C
		public void \u0001(_INewExpression \u0002)
		{
			this.\u0001(AccessModeFlags.Read);
			\u0002._Count.Accept(this);
			this.\u0002();
			if (\u0002._FBInitParams != null)
			{
				foreach (IAssignmentExpression assignmentExpression in \u0002._FBInitParams)
				{
					((_IAssignmentExpression)assignmentExpression).Accept(this);
				}
			}
		}

		// Token: 0x06001B2E RID: 6958 RVA: 0x0005A210 File Offset: 0x00058410
		public void \u0001(_ITypeExpression \u0002)
		{
		}

		// Token: 0x06001B2F RID: 6959 RVA: 0x0005A214 File Offset: 0x00058414
		public void \u0001(_IConversionExpression \u0002)
		{
			this.\u0001(AccessModeFlags.Read);
			\u0002._Exp.Accept(this);
			this.\u0002();
			if (this.\u0001(\u0002.From, \u0002.To))
			{
				global::\u0003.\u0006.\u0002(\u0002, MessageId.Err_IllegalOperator, new object[]
				{
					\u0002.ToString()
				});
			}
		}

		// Token: 0x06001B30 RID: 6960 RVA: 0x0005A268 File Offset: 0x00058468
		public void \u0001(_IThisExpression \u0002)
		{
		}

		// Token: 0x06001B31 RID: 6961 RVA: 0x0005A26C File Offset: 0x0005846C
		public void \u0001(_IBaseExpression \u0002)
		{
		}

		// Token: 0x06001B32 RID: 6962 RVA: 0x0005A270 File Offset: 0x00058470
		public void \u0001(_ILiteralExpression \u0002)
		{
		}

		// Token: 0x06001B33 RID: 6963 RVA: 0x0005A274 File Offset: 0x00058474
		public void \u0001(_IAddressExpression \u0002)
		{
			if (this.TopOfStack == null)
			{
				this.\u0003(\u0002.ToString(), \u0002.Position.ObjectGuid, this.MessageGuid);
				return;
			}
			if ((this.TopOfStack.\u0001 & AccessModeFlags.Read) == AccessModeFlags.Read)
			{
				this.\u0003(\u0002.ToString(), this.ObjectGuid, this.MessageGuid);
				return;
			}
			if ((this.TopOfStack.\u0001 & AccessModeFlags.Write) == AccessModeFlags.Write)
			{
				this.\u0003(\u0002.ToString(), this.ObjectGuid, this.MessageGuid);
				return;
			}
			this.\u0003(\u0002.ToString(), this.ObjectGuid, this.MessageGuid);
		}

		// Token: 0x06001B34 RID: 6964 RVA: 0x0005A314 File Offset: 0x00058514
		public void \u0001(_IQualifiedNameExpression \u0002)
		{
		}

		// Token: 0x06001B35 RID: 6965 RVA: 0x0005A318 File Offset: 0x00058518
		public void \u0001(_IVariableExpression \u0002)
		{
			if (this.TopOfStack == null)
			{
				this.\u0001(\u0002.Name, \u0002.Position.ObjectGuid, this.MessageGuid);
				return;
			}
			if ((this.TopOfStack.\u0001 & AccessModeFlags.Read) == AccessModeFlags.Read)
			{
				this.\u0001(\u0002.Name, this.ObjectGuid, this.MessageGuid);
			}
			else if ((this.TopOfStack.\u0001 & AccessModeFlags.Write) == AccessModeFlags.Write)
			{
				this.\u0001(\u0002.Name, this.ObjectGuid, this.MessageGuid);
			}
			else if ((this.TopOfStack.\u0001 & AccessModeFlags.Declaration) == AccessModeFlags.Declaration)
			{
				this.\u0001(\u0002.Name, this.ObjectGuid, this.MessageGuid);
			}
			else if ((this.TopOfStack.\u0001 & AccessModeFlags.Call) == AccessModeFlags.Call)
			{
				this.\u0002(\u0002.Name, this.ObjectGuid, this.MessageGuid);
			}
			else
			{
				this.\u0001(\u0002.Name, this.ObjectGuid, this.MessageGuid);
			}
			foreach (_ICompilerAttribute icompilerAttribute in this.\u0001)
			{
				if (icompilerAttribute.Name == "map_to")
				{
					string value = icompilerAttribute.Value;
					this.\u0001.Clear();
					IScanner scanner = APEnvironmentFacade.Instance.LanguageModelMgr.CreateScanner(value, false, false, false, false);
					_IExpression iexpression = (APEnvironmentFacade.Instance.LanguageModelMgr.CreateParser(scanner) as IParser4).ParseExpression() as _IExpression;
					if (iexpression != null)
					{
						iexpression.Accept(this);
						break;
					}
					break;
				}
			}
			this.\u0001.Clear();
		}

		// Token: 0x06001B36 RID: 6966 RVA: 0x0005A4C8 File Offset: 0x000586C8
		public void \u0001(_IIndexAccessExpression \u0002)
		{
			\u0002._Var.Accept(this);
			this.\u0001(AccessModeFlags.Read);
			for (int i = 0; i < \u0002.NumAccesses; i++)
			{
				\u0002.GetAccess(i).Accept(this);
			}
			this.\u0002();
		}

		// Token: 0x06001B37 RID: 6967 RVA: 0x0005A50C File Offset: 0x0005870C
		public void \u0001(_ICompoAccessExpression \u0002)
		{
			\u0002._Left.Accept(this);
			\u0002._Right.Accept(this);
		}

		// Token: 0x06001B38 RID: 6968 RVA: 0x0005A528 File Offset: 0x00058728
		public void \u0001(_IDeRefAccessExpression \u0002)
		{
			\u0002._Base.Accept(this);
		}

		// Token: 0x06001B39 RID: 6969 RVA: 0x0005A538 File Offset: 0x00058738
		public void \u0001(_ICopyScopeExpression \u0002)
		{
			\u0002._Base.Accept(this);
		}

		// Token: 0x06001B3A RID: 6970 RVA: 0x0005A548 File Offset: 0x00058748
		public void \u0001(_IGlobalScopeExpression \u0002)
		{
			if (!(\u0002._Base is IVariableExpression) && !(\u0002._Base is ICompoAccessExpression) && !(\u0002._Base is ICallExpression) && !(\u0002._Base is IIndexAccessExpression) && !(\u0002._Base is IDeRefAccessExpression))
			{
				global::\u0003.\u0006.\u0002(\u0002, MessageId.Err_InvalidBaseForGlobalScopeExpression, new object[]
				{
					\u0002._Base
				});
			}
			\u0002._Base.Accept(this);
		}

		// Token: 0x06001B3B RID: 6971 RVA: 0x0005A5BC File Offset: 0x000587BC
		public void \u0001(_ISystemScopeExpression \u0002)
		{
			if (!(\u0002._Base is IVariableExpression) && !(\u0002._Base is ICompoAccessExpression) && !(\u0002._Base is ICallExpression) && !(\u0002._Base is IIndexAccessExpression) && !(\u0002._Base is IDeRefAccessExpression))
			{
				global::\u0003.\u0006.\u0002(\u0002, MessageId.Err_InvalidBaseForGlobalScopeExpression, new object[]
				{
					\u0002._Base
				});
			}
			\u0002._Base.Accept(this);
		}

		// Token: 0x06001B3C RID: 6972 RVA: 0x0005A630 File Offset: 0x00058830
		public void \u0001(_IPoolScopeExpression \u0002)
		{
			if (!(\u0002._Base is IVariableExpression) && !(\u0002._Base is ICompoAccessExpression) && !(\u0002._Base is ICallExpression) && !(\u0002._Base is IIndexAccessExpression) && !(\u0002._Base is IDeRefAccessExpression))
			{
				global::\u0003.\u0006.\u0002(\u0002, MessageId.Err_InvalidBaseForGlobalScopeExpression, new object[]
				{
					\u0002._Base
				});
			}
			\u0002._Base.Accept(this);
		}

		// Token: 0x06001B3D RID: 6973 RVA: 0x0005A6A4 File Offset: 0x000588A4
		public void \u0001(_INamespaceAccessExpression \u0002)
		{
			if (!(\u0002._Namespace is IVariableExpression) && !(\u0002._Namespace is _INamespaceAccessExpression) && !(\u0002._Namespace is ICompoAccessExpression))
			{
				global::\u0003.\u0006.\u0002(\u0002._Namespace, MessageId.Err_InvalidNamespaceForNamespaceAccess, new object[]
				{
					\u0002._Namespace
				});
			}
			\u0002._Namespace.Accept(this);
			_IExpression access = \u0002._Access;
			if (access == null)
			{
				return;
			}
			access.Accept(this);
		}

		// Token: 0x06001B3E RID: 6974 RVA: 0x0005A714 File Offset: 0x00058914
		public void \u0001(_IEmptyStatement \u0002)
		{
		}

		// Token: 0x06001B3F RID: 6975 RVA: 0x0005A718 File Offset: 0x00058918
		public void \u0001(_ICaseRangeExpression \u0002)
		{
			this.\u0001(AccessModeFlags.Read);
			\u0002._Low.Accept(this);
			\u0002._High.Accept(this);
			this.\u0002();
		}

		// Token: 0x06001B40 RID: 6976 RVA: 0x0005A740 File Offset: 0x00058940
		public void \u0001(_ICaseLabelStatement \u0002)
		{
			this.\u0001(AccessModeFlags.Read);
			foreach (_IExpression iexpression in \u0002._cases)
			{
				iexpression.Accept(this);
			}
			this.\u0002();
		}

		// Token: 0x06001B41 RID: 6977 RVA: 0x0005A798 File Offset: 0x00058998
		public void \u0001(_ICaseStatement \u0002)
		{
			this.\u0001(AccessModeFlags.Read);
			\u0002._Switch.Accept(this);
			this.\u0002();
			foreach (_ICase icase in \u0002._Cases)
			{
				icase._Label.Accept(this);
				icase._Controlled.Accept(this);
				this.\u0001(icase._Controlled, false);
			}
			if (\u0002._Else != null)
			{
				\u0002._Else.Accept(this);
			}
		}

		// Token: 0x06001B42 RID: 6978 RVA: 0x0005A830 File Offset: 0x00058A30
		public void \u0001(_IErrorExpression \u0002)
		{
		}

		// Token: 0x06001B43 RID: 6979 RVA: 0x0005A834 File Offset: 0x00058A34
		public void \u0001(_IErrorStatement \u0002)
		{
		}

		// Token: 0x06001B44 RID: 6980 RVA: 0x0005A838 File Offset: 0x00058A38
		public void \u0001(_INullExpression \u0002)
		{
		}

		// Token: 0x06001B45 RID: 6981 RVA: 0x0005A83C File Offset: 0x00058A3C
		public void \u0001(_INullStatement \u0002)
		{
		}

		// Token: 0x06001B46 RID: 6982 RVA: 0x0005A840 File Offset: 0x00058A40
		public void \u0001(_IMultipleIndexInitialization \u0002)
		{
			this.\u0001(AccessModeFlags.Read);
			\u0002._Number.Accept(this);
			\u0002._Value.Accept(this);
			this.\u0002();
		}

		// Token: 0x06001B47 RID: 6983 RVA: 0x0005A868 File Offset: 0x00058A68
		public void \u0001(_IArrayInitialization \u0002)
		{
			this.\u0001(AccessModeFlags.Read);
			foreach (_IExpression iexpression in \u0002._InitValues)
			{
				iexpression.Accept(this);
			}
			this.\u0002();
			if (!this.\u0001)
			{
				global::\u0003.\u0006.\u0002(\u0002, MessageId.Err_UnexpectedArrayInitialisation, Array.Empty<object>());
			}
		}

		// Token: 0x06001B48 RID: 6984 RVA: 0x0005A8D8 File Offset: 0x00058AD8
		public void \u0001(_IStructureInitialization \u0002)
		{
			foreach (_IAssignmentExpression iassignmentExpression in \u0002._CompoInits)
			{
				iassignmentExpression.Accept(this);
			}
		}

		// Token: 0x06001B49 RID: 6985 RVA: 0x0005A924 File Offset: 0x00058B24
		public void \u0001(_IDefineReference \u0002)
		{
			string define = \u0002.Define;
		}

		// Token: 0x06001B4A RID: 6986 RVA: 0x0005A930 File Offset: 0x00058B30
		public void \u0001(_IVariableReference \u0002)
		{
			if (\u0002.InstancePath != null)
			{
				this.\u0001(AccessModeFlags.Read);
				\u0002.InstancePath.Accept(this);
				this.\u0002();
			}
		}

		// Token: 0x06001B4B RID: 6987 RVA: 0x0005A954 File Offset: 0x00058B54
		public void \u0001(_ITypeReference \u0002)
		{
			if (\u0002.InstancePath != null)
			{
				this.\u0001(AccessModeFlags.Read);
				\u0002.InstancePath.Accept(this);
				this.\u0002();
			}
		}

		// Token: 0x06001B4C RID: 6988 RVA: 0x0005A978 File Offset: 0x00058B78
		public void \u0001(_IPouReference \u0002)
		{
			if (\u0002.InstancePath != null)
			{
				this.\u0001(AccessModeFlags.Read);
				\u0002.InstancePath.Accept(this);
				this.\u0002();
			}
		}

		// Token: 0x06001B4D RID: 6989 RVA: 0x0005A99C File Offset: 0x00058B9C
		public void \u0001(_ITaskReference \u0002)
		{
			string taskName = \u0002.TaskName;
		}

		// Token: 0x06001B4E RID: 6990 RVA: 0x0005A9A8 File Offset: 0x00058BA8
		public void \u0001(_IResourceReference \u0002)
		{
			string resourceName = \u0002.ResourceName;
		}

		// Token: 0x06001B4F RID: 6991 RVA: 0x0005A9B4 File Offset: 0x00058BB4
		public void \u0001(_IDefinedExpression \u0002)
		{
			if (typeof(_INullExpression).IsAssignableFrom(\u0002.ItemReference.GetType()))
			{
				global::\u0003.\u0006.\u0002(\u0002, MessageId.Err_NoValidOperandforPragma, new object[]
				{
					\u0002
				});
				return;
			}
			\u0002.ItemReference.Accept(this);
		}

		// Token: 0x06001B50 RID: 6992 RVA: 0x0005A9F4 File Offset: 0x00058BF4
		public void \u0001(_IPragmaOperatorExpression \u0002)
		{
			foreach (_IExpression iexpression in \u0002.Operands)
			{
				if (!typeof(_IPragmaExpression).IsAssignableFrom(iexpression.GetType()))
				{
					global::\u0003.\u0006.\u0002(iexpression, MessageId.Err_NoValidOperandforPragma, new object[]
					{
						iexpression
					});
				}
				iexpression.Accept(this);
			}
		}

		// Token: 0x06001B51 RID: 6993 RVA: 0x0005AA6C File Offset: 0x00058C6C
		private void \u0007(_IExpression \u0002)
		{
			if (\u0002 is IErrorExpression)
			{
				IList<_ICompilerMessage> messagesList = \u0002.MessagesList;
				if (messagesList != null && messagesList.Count > 0)
				{
					return;
				}
			}
			if (\u0002 is _INullExpression)
			{
				global::\u0003.\u0006.\u0002(\u0002, MessageId.Err_ConditionExpected, Array.Empty<object>());
				return;
			}
			if (!(\u0002 is _IPragmaExpression))
			{
				global::\u0003.\u0006.\u0002(\u0002, MessageId.Err_NoValidConditionforPragma, new object[]
				{
					\u0002
				});
				return;
			}
		}

		// Token: 0x06001B52 RID: 6994 RVA: 0x0005AACC File Offset: 0x00058CCC
		public void \u0001(_IPragmaAssertion \u0002)
		{
			this.\u0007(\u0002.Condition);
			\u0002.Condition.Accept(this);
		}

		// Token: 0x06001B53 RID: 6995 RVA: 0x0005AAE8 File Offset: 0x00058CE8
		public void \u0001(_ICompilerVersionExpression \u0002)
		{
		}

		// Token: 0x06001B54 RID: 6996 RVA: 0x0005AAEC File Offset: 0x00058CEC
		public void \u0001(_IPragmaIfStatement \u0002)
		{
			this.\u0007(\u0002.Condition);
			\u0002.Condition.Accept(this);
			\u0002.IfThen.Accept(this);
			foreach (_IPragmaElseIf ipragmaElseIf in \u0002.ElseIf)
			{
				this.\u0007(ipragmaElseIf.Condition);
				ipragmaElseIf.Condition.Accept(this);
				ipragmaElseIf.Controlled.Accept(this);
			}
			if (\u0002.IfElse != null)
			{
				\u0002.IfElse.Accept(this);
			}
		}

		// Token: 0x06001B55 RID: 6997 RVA: 0x0005AB90 File Offset: 0x00058D90
		public void \u0001(_IBreakPointStatement \u0002)
		{
		}

		// Token: 0x06001B56 RID: 6998 RVA: 0x0005AB94 File Offset: 0x00058D94
		public void \u0001(_IDefineStatement \u0002)
		{
		}

		// Token: 0x06001B57 RID: 6999 RVA: 0x0005AB98 File Offset: 0x00058D98
		public void \u0001(_IXRefExpression \u0002)
		{
			if (\u0002.XRef != null)
			{
				\u0002.XRef.Accept(this);
			}
			if (\u0002.XRefFrom != null)
			{
				\u0002.XRefFrom.Accept(this);
			}
		}

		// Token: 0x06001B58 RID: 7000 RVA: 0x0005ABC4 File Offset: 0x00058DC4
		public void \u0001(_IHasTypeExpression \u0002)
		{
			if (\u0002.Variable != null)
			{
				\u0002.Variable.Accept(this);
			}
		}

		// Token: 0x06001B59 RID: 7001 RVA: 0x0005ABDC File Offset: 0x00058DDC
		public void \u0001(_IIsEnumTypeExpression \u0002)
		{
		}

		// Token: 0x06001B5A RID: 7002 RVA: 0x0005ABE0 File Offset: 0x00058DE0
		public void \u0001(_IHasAttributeExpression \u0002)
		{
			if (\u0002.ItemReference != null)
			{
				\u0002.ItemReference.Accept(this);
			}
		}

		// Token: 0x06001B5B RID: 7003 RVA: 0x0005ABF8 File Offset: 0x00058DF8
		public void \u0001(_IHasValueExpression \u0002)
		{
		}

		// Token: 0x06001B5C RID: 7004 RVA: 0x0005ABFC File Offset: 0x00058DFC
		public void \u0001(_IHasConstantValueExpression \u0002)
		{
		}

		// Token: 0x06001B5D RID: 7005 RVA: 0x0005AC00 File Offset: 0x00058E00
		public void \u0001(_IHasConstantTypeExpression \u0002)
		{
		}

		// Token: 0x170005A7 RID: 1447
		// (get) Token: 0x06001B5E RID: 7006 RVA: 0x0005AC04 File Offset: 0x00058E04
		private global::\u0004.\u0004.\u0001 TopOfStack
		{
			get
			{
				if (this.\u0001.Count == 0)
				{
					return null;
				}
				return this.\u0001.Peek() as global::\u0004.\u0004.\u0001;
			}
		}

		// Token: 0x06001B5F RID: 7007 RVA: 0x0005AC28 File Offset: 0x00058E28
		private void \u0001(AccessModeFlags \u0002)
		{
			global::\u0004.\u0004.\u0001 u = new global::\u0004.\u0004.\u0001();
			u.\u0001 = \u0002;
			if (this.TopOfStack != null)
			{
				u.\u0001 = this.TopOfStack.\u0001;
			}
			this.\u0001.Push(u);
			if (this.\u0001.Count > this.\u0001)
			{
				this.\u0001 = this.\u0001.Count;
			}
		}

		// Token: 0x06001B60 RID: 7008 RVA: 0x0005AC8C File Offset: 0x00058E8C
		private void \u0001(AccessModeFlags \u0002, bool \u0003)
		{
			global::\u0004.\u0004.\u0001 u = new global::\u0004.\u0004.\u0001();
			u.\u0001 = \u0002;
			u.\u0002 = \u0003;
			if (this.TopOfStack != null)
			{
				u.\u0001 = this.TopOfStack.\u0001;
			}
			this.\u0001.Push(u);
			if (this.\u0001.Count > this.\u0001)
			{
				this.\u0001 = this.\u0001.Count;
			}
		}

		// Token: 0x06001B61 RID: 7009 RVA: 0x0005ACF8 File Offset: 0x00058EF8
		private void \u0001(bool \u0002)
		{
			global::\u0004.\u0004.\u0001 u = new global::\u0004.\u0004.\u0001();
			u.\u0001 = AccessModeFlags.Unknown;
			u.\u0001 = \u0002;
			this.\u0001.Push(u);
			if (this.\u0001.Count > this.\u0001)
			{
				this.\u0001 = this.\u0001.Count;
			}
		}

		// Token: 0x06001B62 RID: 7010 RVA: 0x0005AD4C File Offset: 0x00058F4C
		private void \u0002()
		{
			this.\u0001.Pop();
		}

		// Token: 0x06001B63 RID: 7011 RVA: 0x0005AD5C File Offset: 0x00058F5C
		public void \u0001(_IRuntimeVersionExpression \u0002)
		{
		}

		// Token: 0x06001B64 RID: 7012 RVA: 0x0005AD60 File Offset: 0x00058F60
		public void \u0001(_ICurrentTaskExpression \u0002)
		{
		}

		// Token: 0x06001B65 RID: 7013 RVA: 0x0005AD64 File Offset: 0x00058F64
		public void \u0001(_IPartialAccessExpression \u0002)
		{
			\u0002._Left.Accept(this);
		}

		// Token: 0x06001B66 RID: 7014 RVA: 0x0005AD74 File Offset: 0x00058F74
		public void \u0001(_IProjectDefinedExpression \u0002)
		{
			_IDefineReference defineReference = \u0002.DefineReference;
			if (defineReference == null)
			{
				return;
			}
			defineReference.Accept(this);
		}

		// Token: 0x040004AD RID: 1197
		private _IPreCompCrossReferences \u0001;

		// Token: 0x040004AE RID: 1198
		private _IPreCompCrossReferences \u0002;

		// Token: 0x040004AF RID: 1199
		private _IPreCompCrossReferences \u0003;

		// Token: 0x040004B0 RID: 1200
		private ArrayList \u0001 = new ArrayList();

		// Token: 0x040004B1 RID: 1201
		private ArrayList \u0002 = new ArrayList();

		// Token: 0x040004B2 RID: 1202
		private readonly LList<_ICompilerAttribute> \u0001 = new LList<_ICompilerAttribute>();

		// Token: 0x040004B3 RID: 1203
		private int \u0001;

		// Token: 0x040004B4 RID: 1204
		private Guid \u0001 = Guid.Empty;

		// Token: 0x040004B5 RID: 1205
		private Guid \u0002 = Guid.Empty;

		// Token: 0x040004B6 RID: 1206
		private bool \u0001;

		// Token: 0x040004B7 RID: 1207
		private readonly LDictionary<string, string> \u0001 = new LDictionary<string, string>();

		// Token: 0x040004B8 RID: 1208
		private Stack \u0001 = new Stack();

		// Token: 0x02000187 RID: 391
		private sealed class \u0001
		{
			// Token: 0x040004B9 RID: 1209
			public AccessModeFlags \u0001;

			// Token: 0x040004BA RID: 1210
			public bool \u0001;

			// Token: 0x040004BB RID: 1211
			public bool \u0002;
		}
	}
}
