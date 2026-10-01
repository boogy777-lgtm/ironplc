using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Compiler35220.Serialization;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using \u0082;

namespace \u0013
{
	// Token: 0x0200008F RID: 143
	internal abstract class \u0002 : IExprementVisitor352000, IExprementVisitor351900, IExprementVisitor351800, IExprementVisitor351500, IExprementVisitor351400, IExprementVisitor351300, IExprementVisitor3590, IExprementVisitor, IExprementVisitor2
	{
		// Token: 0x17000422 RID: 1058
		// (get) Token: 0x06000BE0 RID: 3040 RVA: 0x0001BE80 File Offset: 0x0001A080
		// (set) Token: 0x06000BE1 RID: 3041 RVA: 0x0001BE88 File Offset: 0x0001A088
		public BinaryWriter Writer { get; set; }

		// Token: 0x06000BE2 RID: 3042 RVA: 0x0001BE94 File Offset: 0x0001A094
		protected \u0002(BinaryWriter \u009C\u0002)
		{
			this.Writer = \u009C\u0002;
		}

		// Token: 0x06000BE3 RID: 3043 RVA: 0x0001BEA4 File Offset: 0x0001A0A4
		protected static void \u0001(BinaryWriter \u0002, ICompactedParseTreeInformation \u0003)
		{
			\u0013.\u0002.\u0001(\u0002, \u0003.SourcePosTable, \u0003.LengthTable);
			\u0013.\u0002.\u0001(\u0002, \u0003.MessageTable);
		}

		// Token: 0x06000BE4 RID: 3044 RVA: 0x0001BEC4 File Offset: 0x0001A0C4
		protected static void \u0001(BinaryWriter \u0002, ICompactedCompiledParseTreeInformation \u0003)
		{
			\u0013.\u0002.\u0001(\u0002, \u0003.SourcePosTable, \u0003.LengthTable);
			\u0013.\u0002.\u0001(\u0002, \u0003.MessageTable);
			\u0013.\u0002.\u0002(\u0002, \u0003);
		}

		// Token: 0x06000BE5 RID: 3045 RVA: 0x0001BEEC File Offset: 0x0001A0EC
		private static void \u0001(BinaryWriter \u0002, IDictionary<int, IList<_ICompilerMessage>> \u0003)
		{
			\u0002.Write(\u0003.Count);
			foreach (KeyValuePair<int, IList<_ICompilerMessage>> keyValuePair in \u0003)
			{
				\u0002.Write(keyValuePair.Key);
				IList<_ICompilerMessage> value = keyValuePair.Value;
				\u0002.Write(value.Count);
				foreach (_ICompilerMessage msg in value)
				{
					CommonSerializer.SerializeMessage(\u0002, msg);
				}
			}
		}

		// Token: 0x06000BE6 RID: 3046 RVA: 0x0001BF94 File Offset: 0x0001A194
		private static void \u0001(BinaryWriter \u0002, IList<long> \u0003, IList<short> \u0004)
		{
			\u0002.Write(\u0003.Count);
			foreach (long value in \u0003)
			{
				\u0002.Write(value);
			}
			foreach (short value2 in \u0004)
			{
				\u0002.Write(value2);
			}
		}

		// Token: 0x06000BE7 RID: 3047 RVA: 0x0001C020 File Offset: 0x0001A220
		private static void \u0002(BinaryWriter \u0002, ICompactedCompiledParseTreeInformation \u0003)
		{
			\u0002.Write(\u0003.TypeInfoTable.Count);
			foreach (KeyValuePair<int, ICompiledExpressionTypeInfo> keyValuePair in \u0003.TypeInfoTable)
			{
				\u0002.Write(keyValuePair.Key);
				ICompiledExpressionTypeInfo value = keyValuePair.Value;
				\u0002.Write(value.SignatureId);
				\u0002.Write(value.VariableId);
				if (!CommonSerializer.SerializeNullabe(\u0002, value.CompiledType == null))
				{
					\u0082.\u0001.Instance.\u0001(\u0002, value.CompiledType);
				}
			}
		}

		// Token: 0x06000BE8 RID: 3048 RVA: 0x0001C0C8 File Offset: 0x0001A2C8
		public virtual void \u0001(_ISequenceStatement \u0002)
		{
			this.Writer.Write(12U);
			this.Writer.Write(\u0002._StatementList.Count);
			for (int i = 0; i < \u0002._StatementList.Count; i++)
			{
				StatementFlag flags = (\u0002._StatementList[i] as _IStatement2).Flags;
				this.Writer.Write((int)flags);
				\u0002._StatementList[i].Accept(this);
			}
		}

		// Token: 0x06000BE9 RID: 3049 RVA: 0x0001C144 File Offset: 0x0001A344
		public void \u0001(_IWhileStatement \u0002)
		{
			this.Writer.Write(3U);
			\u0002._Condition.Accept(this);
			\u0002._Controlled.Accept(this);
		}

		// Token: 0x06000BEA RID: 3050 RVA: 0x0001C16C File Offset: 0x0001A36C
		public void \u0001(_IRepeatStatement \u0002)
		{
			this.Writer.Write(4U);
			\u0002._Condition.Accept(this);
			\u0002._Controlled.Accept(this);
		}

		// Token: 0x06000BEB RID: 3051 RVA: 0x0001C194 File Offset: 0x0001A394
		public void \u0001(_IForStatement \u0002)
		{
			this.Writer.Write(9U);
			\u0002._CounterStart.Accept(this);
			\u0002._UpperBound.Accept(this);
			if (\u0002.By != null)
			{
				\u0002._By.Accept(this);
			}
			else
			{
				this.Writer.Write(0U);
			}
			if (\u0002._Counter != null)
			{
				\u0002._Counter.Accept(this);
			}
			else
			{
				this.Writer.Write(0U);
			}
			if (\u0002._Condition != null)
			{
				\u0002._Condition.Accept(this);
			}
			else
			{
				this.Writer.Write(0U);
			}
			\u0002._Controlled.Accept(this);
		}

		// Token: 0x06000BEC RID: 3052 RVA: 0x0001C238 File Offset: 0x0001A438
		public void \u0001(_IExitStatement \u0002)
		{
			this.Writer.Write(10U);
		}

		// Token: 0x06000BED RID: 3053 RVA: 0x0001C248 File Offset: 0x0001A448
		public void \u0001(_IContinueStatement \u0002)
		{
			this.Writer.Write(11U);
		}

		// Token: 0x06000BEE RID: 3054 RVA: 0x0001C258 File Offset: 0x0001A458
		public void \u0001(_IIfStatement \u0002)
		{
			this.Writer.Write(15U);
			\u0002._Condition.Accept(this);
			\u0002._IfThen.Accept(this);
			this.Writer.Write(\u0002._ElseIf.Count);
			foreach (_IElseIf ielseIf in \u0002._ElseIf)
			{
				ielseIf._Condition.Accept(this);
				ielseIf._Controlled.Accept(this);
			}
			if (\u0002._IfElse != null)
			{
				\u0002._IfElse.Accept(this);
				return;
			}
			this.Writer.Write(0U);
		}

		// Token: 0x06000BEF RID: 3055 RVA: 0x0001C310 File Offset: 0x0001A510
		public void \u0001(_IReturnStatement \u0002)
		{
			this.Writer.Write(17U);
			if (\u0002._Condition != null)
			{
				\u0002._Condition.Accept(this);
				return;
			}
			this.Writer.Write(0U);
		}

		// Token: 0x06000BF0 RID: 3056 RVA: 0x0001C340 File Offset: 0x0001A540
		public void \u0001(_IJumpStatement \u0002)
		{
			this.Writer.Write(18U);
			this.Writer.Write(\u0002.Label);
			if (\u0002._Condition != null)
			{
				\u0002._Condition.Accept(this);
				return;
			}
			this.Writer.Write(0U);
		}

		// Token: 0x06000BF1 RID: 3057 RVA: 0x0001C38C File Offset: 0x0001A58C
		public void \u0001(_ILabelStatement \u0002)
		{
			this.Writer.Write(19U);
			this.Writer.Write(\u0002.OrgText);
		}

		// Token: 0x06000BF2 RID: 3058 RVA: 0x0001C3AC File Offset: 0x0001A5AC
		public void \u0001(_ICommentStatement \u0002)
		{
			this.Writer.Write(20U);
			this.Writer.Write(\u0002.Text);
			this.Writer.Write(\u0002.DocComment);
		}

		// Token: 0x06000BF3 RID: 3059 RVA: 0x0001C3E0 File Offset: 0x0001A5E0
		public void \u0001(_IPragmaStatement \u0002)
		{
			_IMessageGuidPragmaStatement imessageGuidPragmaStatement = \u0002 as _IMessageGuidPragmaStatement;
			if (imessageGuidPragmaStatement == null)
			{
				_IImplicitCodeSectionPragma iimplicitCodeSectionPragma = \u0002 as _IImplicitCodeSectionPragma;
				if (iimplicitCodeSectionPragma == null)
				{
					_ILocalSignatureIdPragma ilocalSignatureIdPragma = \u0002 as _ILocalSignatureIdPragma;
					if (ilocalSignatureIdPragma == null)
					{
						_IWarningDisableRestorePragmaStatement iwarningDisableRestorePragmaStatement = \u0002 as _IWarningDisableRestorePragmaStatement;
						if (iwarningDisableRestorePragmaStatement == null)
						{
							this.Writer.Write(21U);
						}
						else
						{
							this.Writer.Write(23U);
							this.Writer.Write(iwarningDisableRestorePragmaStatement.Restore);
							this.Writer.Write(iwarningDisableRestorePragmaStatement.Id);
						}
					}
					else
					{
						this.Writer.Write(81U);
						this.Writer.Write(ilocalSignatureIdPragma.LocalSignatureId);
					}
				}
				else
				{
					this.Writer.Write(80U);
					this.Writer.Write(iimplicitCodeSectionPragma.ImplicitOn);
				}
			}
			else
			{
				this.Writer.Write(22U);
				this.Writer.Write(imessageGuidPragmaStatement.MessageGuid.ToString());
			}
			this.Writer.Write(\u0002.Text);
		}

		// Token: 0x06000BF4 RID: 3060 RVA: 0x0001C4D8 File Offset: 0x0001A6D8
		public void \u0001(_IExpressionStatement \u0002)
		{
			this.Writer.Write(24U);
			\u0002._Expr.Accept(this);
		}

		// Token: 0x06000BF5 RID: 3061 RVA: 0x0001C4F4 File Offset: 0x0001A6F4
		public void \u0001(_ICallExpression \u0002)
		{
			this.Writer.Write(27U);
			IList<_IExpression> inputs = \u0002.Inputs;
			IList<_IExpression> paramExpressions = \u0002.ParamExpressions;
			IList<_IExpression> outputs = \u0002.Outputs;
			IList<_IExpression> outputExpressions = \u0002.OutputExpressions;
			\u0002._Callee.Accept(this);
			if (\u0002._Condition != null)
			{
				\u0002._Condition.Accept(this);
			}
			else
			{
				this.Writer.Write(0U);
			}
			if (\u0002.ExpectedType != null)
			{
				this.Writer.Write(\u0002.ExpectedType.ToString());
			}
			else
			{
				this.Writer.Write(string.Empty);
			}
			this.Writer.Write(paramExpressions.Count);
			for (int i = 0; i < paramExpressions.Count; i++)
			{
				paramExpressions[i].Accept(this);
			}
			this.Writer.Write(inputs.Count);
			for (int j = 0; j < inputs.Count; j++)
			{
				_IExpression iexpression = inputs[j];
				if (iexpression != null)
				{
					iexpression.Accept(this);
				}
				else
				{
					this.Writer.Write(0U);
				}
			}
			this.Writer.Write(outputs.Count);
			for (int k = 0; k < outputs.Count; k++)
			{
				outputs[k].Accept(this);
			}
			this.Writer.Write(outputExpressions.Count);
			for (int l = 0; l < outputExpressions.Count; l++)
			{
				if (\u0002.OutputExpressions[l] != null)
				{
					\u0002.OutputExpressions[l].Accept(this);
				}
				else
				{
					this.Writer.Write(0U);
				}
			}
			this.Writer.Write(\u0002.EmptyAssigns.Count);
			for (int m = 0; m < \u0002.EmptyAssigns.Count; m++)
			{
				\u0002.EmptyAssigns[m].Accept(this);
			}
		}

		// Token: 0x06000BF6 RID: 3062 RVA: 0x0001C6D4 File Offset: 0x0001A8D4
		public void \u0001(_IConversionExpression \u0002)
		{
			if (\u0002 is _IImplicitConversionExpression)
			{
				this.Writer.Write(79U);
				this.Writer.Write((uint)\u0002.From);
				this.Writer.Write((uint)\u0002.To);
				\u0002._Exp.Accept(this);
				return;
			}
			this.Writer.Write(29U);
			this.Writer.Write((uint)\u0002.From);
			this.Writer.Write((uint)\u0002.To);
			\u0002._Exp.Accept(this);
		}

		// Token: 0x06000BF7 RID: 3063 RVA: 0x0001C760 File Offset: 0x0001A960
		public void \u0001(_IThisExpression \u0002)
		{
			this.Writer.Write(32U);
		}

		// Token: 0x06000BF8 RID: 3064 RVA: 0x0001C770 File Offset: 0x0001A970
		public void \u0001(_IBaseExpression \u0002)
		{
			this.Writer.Write(33U);
		}

		// Token: 0x06000BF9 RID: 3065 RVA: 0x0001C780 File Offset: 0x0001A980
		public void \u0001(_IAddressExpression \u0002)
		{
			this.Writer.Write(40U);
			this.Writer.Write(\u0002.DirectAddress.ToString());
		}

		// Token: 0x06000BFA RID: 3066 RVA: 0x0001C7A8 File Offset: 0x0001A9A8
		public virtual void \u0001(_ILiteralExpression \u0002)
		{
			bool flag = false;
			bool flag2 = false;
			TypeClass constantType = \u0002.ConstantType;
			if (constantType <= TypeClass.WString)
			{
				if (constantType - TypeClass.Real > 1)
				{
					if (constantType - TypeClass.String > 1)
					{
						goto IL_32;
					}
					goto IL_30;
				}
			}
			else if (constantType != TypeClass.AnyReal)
			{
				if (constantType != TypeClass.XString)
				{
					goto IL_32;
				}
				goto IL_30;
			}
			flag = true;
			goto IL_32;
			IL_30:
			flag2 = true;
			IL_32:
			if (!flag && !flag2)
			{
				if (\u0002.Base != 10)
				{
					this.Writer.Write(36U);
					this.Writer.Write((uint)\u0002.ConstantType);
					this.Writer.Write(\u0002.LongValue);
					this.Writer.Write(\u0002.Base);
					this.Writer.Write(\u0002.Negative);
					return;
				}
				this.Writer.Write(35U);
				this.Writer.Write((uint)\u0002.ConstantType);
				this.Writer.Write(\u0002.LongValue);
				this.Writer.Write(\u0002.Negative);
				return;
			}
			else
			{
				if (flag2)
				{
					this.Writer.Write(37U);
					this.Writer.Write((uint)\u0002.ConstantType);
					this.Writer.Write(\u0002.StringValue);
					this.Writer.Write((uint)(\u0002 as _IStringLiteralExpression2).StringEncoding);
					return;
				}
				if (flag)
				{
					this.Writer.Write(38U);
					this.Writer.Write((uint)\u0002.ConstantType);
					this.Writer.Write(\u0002.RealValue);
				}
				return;
			}
		}

		// Token: 0x06000BFB RID: 3067 RVA: 0x0001C90C File Offset: 0x0001AB0C
		public virtual void \u0001(_IAssignmentExpression \u0002)
		{
			this.Writer.Write(13U);
			this.Writer.Write((uint)\u0002.KindOf);
			\u0002._LValue.Accept(this);
			\u0002._RValue.Accept(this);
		}

		// Token: 0x06000BFC RID: 3068 RVA: 0x0001C944 File Offset: 0x0001AB44
		public virtual void \u0001(_IOperatorExpression \u0002)
		{
			this.Writer.Write(28U);
			this.Writer.Write((uint)\u0002.Code);
			IList<_IExpression> operandsList = \u0002._OperandsList;
			new _IExpression[operandsList.Count];
			this.Writer.Write(operandsList.Count);
			for (int i = 0; i < operandsList.Count; i++)
			{
				operandsList[i].Accept(this);
			}
		}

		// Token: 0x06000BFD RID: 3069 RVA: 0x0001C9B4 File Offset: 0x0001ABB4
		public virtual void \u0001(_IVariableExpression \u0002)
		{
			this.Writer.Write(41U);
			this.Writer.Write(\u0002.Name);
		}

		// Token: 0x06000BFE RID: 3070 RVA: 0x0001C9D4 File Offset: 0x0001ABD4
		public virtual void \u0001(_IIndexAccessExpression \u0002)
		{
			this.Writer.Write(42U);
			\u0002._Var.Accept(this);
			this.Writer.Write(\u0002._Accesses.Count);
			foreach (_IExpression iexpression in \u0002._Accesses)
			{
				iexpression.Accept(this);
			}
		}

		// Token: 0x06000BFF RID: 3071 RVA: 0x0001CA50 File Offset: 0x0001AC50
		public virtual void \u0001(_ICompoAccessExpression \u0002)
		{
			this.Writer.Write(43U);
			\u0002._Left.Accept(this);
			\u0002._Right.Accept(this);
		}

		// Token: 0x06000C00 RID: 3072 RVA: 0x0001CA78 File Offset: 0x0001AC78
		public virtual void \u0001(_IDeRefAccessExpression \u0002)
		{
			this.Writer.Write(44U);
			\u0002._Base.Accept(this);
		}

		// Token: 0x06000C01 RID: 3073 RVA: 0x0001CA94 File Offset: 0x0001AC94
		public void \u0001(_IGlobalScopeExpression \u0002)
		{
			this.Writer.Write(45U);
			\u0002._Base.Accept(this);
		}

		// Token: 0x06000C02 RID: 3074 RVA: 0x0001CAB0 File Offset: 0x0001ACB0
		public void \u0001(_ISystemScopeExpression \u0002)
		{
			this.Writer.Write(46U);
			\u0002._Base.Accept(this);
		}

		// Token: 0x06000C03 RID: 3075 RVA: 0x0001CACC File Offset: 0x0001ACCC
		public void \u0001(_IEmptyStatement \u0002)
		{
			this.Writer.Write(2U);
		}

		// Token: 0x06000C04 RID: 3076 RVA: 0x0001CADC File Offset: 0x0001ACDC
		public void \u0001(_ICaseRangeExpression \u0002)
		{
			this.Writer.Write(5U);
			\u0002._High.Accept(this);
			\u0002._Low.Accept(this);
		}

		// Token: 0x06000C05 RID: 3077 RVA: 0x0001CB04 File Offset: 0x0001AD04
		public void \u0001(_ICaseLabelStatement \u0002)
		{
			this.Writer.Write(6U);
			this.Writer.Write(\u0002._cases.Count);
			for (int i = 0; i < \u0002._cases.Count; i++)
			{
				\u0002._cases[i].Accept(this);
			}
		}

		// Token: 0x06000C06 RID: 3078 RVA: 0x0001CB5C File Offset: 0x0001AD5C
		public void \u0001(_ICaseStatement \u0002)
		{
			this.Writer.Write(8U);
			\u0002._Switch.Accept(this);
			this.Writer.Write(\u0002._Cases.Count);
			foreach (_ICase icase in \u0002._Cases)
			{
				icase._Label.Accept(this);
				icase._Controlled.Accept(this);
			}
			if (\u0002._Else != null)
			{
				\u0002._Else.Accept(this);
				return;
			}
			this.Writer.Write(0U);
		}

		// Token: 0x06000C07 RID: 3079 RVA: 0x0001CC08 File Offset: 0x0001AE08
		public void \u0001(_IErrorExpression \u0002)
		{
			this.Writer.Write(25U);
		}

		// Token: 0x06000C08 RID: 3080 RVA: 0x0001CC18 File Offset: 0x0001AE18
		public void \u0001(_IErrorStatement \u0002)
		{
			this.Writer.Write(1U);
		}

		// Token: 0x06000C09 RID: 3081 RVA: 0x0001CC28 File Offset: 0x0001AE28
		public void \u0001(_INullExpression \u0002)
		{
			this.Writer.Write(49U);
		}

		// Token: 0x06000C0A RID: 3082 RVA: 0x0001CC38 File Offset: 0x0001AE38
		public void \u0001(_INullStatement \u0002)
		{
			this.Writer.Write(50U);
		}

		// Token: 0x06000C0B RID: 3083 RVA: 0x0001CC48 File Offset: 0x0001AE48
		public void \u0001(_IMultipleIndexInitialization \u0002)
		{
			this.Writer.Write(59U);
			\u0002._Number.Accept(this);
			\u0002._Value.Accept(this);
		}

		// Token: 0x06000C0C RID: 3084 RVA: 0x0001CC70 File Offset: 0x0001AE70
		public void \u0001(_IArrayInitialization \u0002)
		{
			this.Writer.Write(57U);
			this.Writer.Write(\u0002._InitValues.Count);
			foreach (_IExpression iexpression in \u0002._InitValues)
			{
				iexpression.Accept(this);
			}
		}

		// Token: 0x06000C0D RID: 3085 RVA: 0x0001CCE0 File Offset: 0x0001AEE0
		public void \u0001(_IStructureInitialization \u0002)
		{
			this.Writer.Write(58U);
			this.Writer.Write(\u0002._CompoInits.Count);
			foreach (_IAssignmentExpression iassignmentExpression in \u0002._CompoInits)
			{
				iassignmentExpression.Accept(this);
			}
		}

		// Token: 0x06000C0E RID: 3086 RVA: 0x0001CD50 File Offset: 0x0001AF50
		public void \u0001(_IDefineReference \u0002)
		{
			this.Writer.Write(60U);
			this.Writer.Write(\u0002.Define);
		}

		// Token: 0x06000C0F RID: 3087 RVA: 0x0001CD70 File Offset: 0x0001AF70
		public void \u0001(_IVariableReference \u0002)
		{
			this.Writer.Write(61U);
			\u0002.InstancePath.Accept(this);
		}

		// Token: 0x06000C10 RID: 3088 RVA: 0x0001CD8C File Offset: 0x0001AF8C
		public void \u0001(_ITypeReference \u0002)
		{
			this.Writer.Write(62U);
			\u0002.InstancePath.Accept(this);
		}

		// Token: 0x06000C11 RID: 3089 RVA: 0x0001CDA8 File Offset: 0x0001AFA8
		public void \u0001(_IPouReference \u0002)
		{
			this.Writer.Write(63U);
			\u0002.InstancePath.Accept(this);
		}

		// Token: 0x06000C12 RID: 3090 RVA: 0x0001CDC4 File Offset: 0x0001AFC4
		public void \u0001(_ITaskReference \u0002)
		{
			this.Writer.Write(64U);
			this.Writer.Write(\u0002.TaskName);
		}

		// Token: 0x06000C13 RID: 3091 RVA: 0x0001CDE4 File Offset: 0x0001AFE4
		public void \u0001(_IResourceReference \u0002)
		{
			this.Writer.Write(65U);
			this.Writer.Write(\u0002.ResourceName);
		}

		// Token: 0x06000C14 RID: 3092 RVA: 0x0001CE04 File Offset: 0x0001B004
		public void \u0001(_IDefinedExpression \u0002)
		{
			this.Writer.Write(66U);
			\u0002.ItemReference.Accept(this);
		}

		// Token: 0x06000C15 RID: 3093 RVA: 0x0001CE20 File Offset: 0x0001B020
		public void \u0001(_IXRefExpression \u0002)
		{
			this.Writer.Write(67U);
			\u0002.XRef.Accept(this);
			\u0002.XRefFrom.Accept(this);
		}

		// Token: 0x06000C16 RID: 3094 RVA: 0x0001CE48 File Offset: 0x0001B048
		public void \u0001(_ICompilerVersionExpression \u0002)
		{
			this.Writer.Write(68U);
			this.Writer.Write(\u0002.VersionToTest.ToString());
			this.Writer.Write((uint)\u0002.OpComparison);
		}

		// Token: 0x06000C17 RID: 3095 RVA: 0x0001CE80 File Offset: 0x0001B080
		public void \u0001(_IPragmaOperatorExpression \u0002)
		{
			this.Writer.Write(53U);
			this.Writer.Write((uint)\u0002.Code);
			this.Writer.Write(\u0002.Operands.Count);
			foreach (_IExpression iexpression in \u0002.Operands)
			{
				iexpression.Accept(this);
			}
		}

		// Token: 0x06000C18 RID: 3096 RVA: 0x0001CF00 File Offset: 0x0001B100
		public void \u0001(_IPragmaIfStatement \u0002)
		{
			this.Writer.Write(69U);
			\u0002.Condition.Accept(this);
			\u0002.IfThen.Accept(this);
			this.Writer.Write(\u0002.ElseIf.Count);
			foreach (_IPragmaElseIf ipragmaElseIf in \u0002.ElseIf)
			{
				ipragmaElseIf.Condition.Accept(this);
				ipragmaElseIf.Controlled.Accept(this);
			}
			if (\u0002.IfElse != null)
			{
				\u0002.IfElse.Accept(this);
				return;
			}
			this.Writer.Write(0U);
		}

		// Token: 0x06000C19 RID: 3097 RVA: 0x0001CFB8 File Offset: 0x0001B1B8
		public void \u0001(_IDefineStatement \u0002)
		{
			this.Writer.Write(56U);
			this.Writer.Write(\u0002.Define);
			this.Writer.Write(\u0002.Ident);
			this.Writer.Write(\u0002.Value);
		}

		// Token: 0x06000C1A RID: 3098 RVA: 0x0001D008 File Offset: 0x0001B208
		public void \u0001(_IHasCompatibleTypeExpression \u0002)
		{
			this.Writer.Write(70U);
			this.Writer.Write(\u0002.ReferencedType.ToString());
			\u0002.Variable.Accept(this);
		}

		// Token: 0x06000C1B RID: 3099 RVA: 0x0001D03C File Offset: 0x0001B23C
		public void \u0001(_IHasTypeExpression \u0002)
		{
			if (\u0002 is _IHasCompatibleTypeExpression)
			{
				this.\u0001((_IHasCompatibleTypeExpression)\u0002);
				return;
			}
			this.Writer.Write(71U);
			this.Writer.Write(\u0002.ReferencedType.ToString());
			\u0002.Variable.Accept(this);
		}

		// Token: 0x06000C1C RID: 3100 RVA: 0x0001D090 File Offset: 0x0001B290
		public void \u0001(_IIsEnumTypeExpression \u0002)
		{
			this.Writer.Write(72U);
			this.Writer.Write(\u0002.ReferencedType.ToString());
		}

		// Token: 0x06000C1D RID: 3101 RVA: 0x0001D0B8 File Offset: 0x0001B2B8
		public void \u0001(_IHasAttributeExpression \u0002)
		{
			this.Writer.Write(73U);
			this.Writer.Write(\u0002.Attribute);
			\u0002.ItemReference.Accept(this);
		}

		// Token: 0x06000C1E RID: 3102 RVA: 0x0001D0E4 File Offset: 0x0001B2E4
		public void \u0001(_IHasValueExpression \u0002)
		{
			this.Writer.Write(51U);
			this.Writer.Write(\u0002.Define);
			this.Writer.Write(\u0002.DefineValue);
		}

		// Token: 0x06000C1F RID: 3103 RVA: 0x0001D118 File Offset: 0x0001B318
		public void \u0001(_IHasConstantValueExpression \u0002)
		{
			this.Writer.Write(52U);
			this.Writer.Write((uint)\u0002._OpComparison);
			\u0002._Constant.Accept(this);
			\u0002._ConstantValue.Accept(this);
		}

		// Token: 0x06000C20 RID: 3104 RVA: 0x0001D150 File Offset: 0x0001B350
		public void \u0001(_IHasConstantTypeExpression \u0002)
		{
			this.Writer.Write(75U);
			\u0002._Constant.Accept(this);
			this.Writer.Write(\u0002._ConstantTypeReplaced);
		}

		// Token: 0x06000C21 RID: 3105 RVA: 0x0001D17C File Offset: 0x0001B37C
		public void \u0001(_IPragmaAssertion \u0002)
		{
			this.Writer.Write(54U);
			this.Writer.Write(\u0002.ErrorOutput);
			\u0002.Condition.Accept(this);
		}

		// Token: 0x06000C22 RID: 3106 RVA: 0x0001D1A8 File Offset: 0x0001B3A8
		public void \u0001(_ICastExpression \u0002)
		{
			this.Writer.Write(31U);
			if (\u0002.ExplicitelySpecifiedType == null)
			{
				this.Writer.Write(string.Empty);
			}
			else
			{
				this.Writer.Write(\u0002.ExplicitelySpecifiedType.ToString());
			}
			\u0002.BaseExpression.Accept(this);
			if (\u0002.ExpWithType == null)
			{
				this.Writer.Write(0U);
				return;
			}
			\u0002.ExpWithType.Accept(this);
		}

		// Token: 0x06000C23 RID: 3107 RVA: 0x0001D220 File Offset: 0x0001B420
		public void \u0001(_INewExpression \u0002)
		{
			this.Writer.Write(30U);
			this.Writer.Write(\u0002._TypeToCast.ToString());
			if (\u0002._Count != null)
			{
				\u0002._Count.Accept(this);
			}
			else
			{
				this.Writer.Write(0U);
			}
			if (\u0002._FBInitParams != null)
			{
				this.Writer.Write(\u0002._FBInitParams.Count);
				using (IEnumerator<IAssignmentExpression> enumerator = \u0002._FBInitParams.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						IAssignmentExpression assignmentExpression = enumerator.Current;
						((_IAssignmentExpression)assignmentExpression).Accept(this);
					}
					return;
				}
			}
			this.Writer.Write(0);
		}

		// Token: 0x06000C24 RID: 3108 RVA: 0x0001D2E0 File Offset: 0x0001B4E0
		public void \u0001(_ITypeExpression \u0002)
		{
			this.Writer.Write(39U);
			this.Writer.Write(\u0002.ExpressionType.ToString());
		}

		// Token: 0x06000C25 RID: 3109 RVA: 0x0001D308 File Offset: 0x0001B508
		public void \u0001(_INamespaceAccessExpression \u0002)
		{
			this.Writer.Write(26U);
			\u0002._Access.Accept(this);
			\u0002._Namespace.Accept(this);
		}

		// Token: 0x06000C26 RID: 3110 RVA: 0x0001D330 File Offset: 0x0001B530
		public void \u0001(_IRuntimeVersionExpression \u0002)
		{
			this.Writer.Write(74U);
			this.Writer.Write(\u0002.VersionToTest.ToString());
			this.Writer.Write((uint)\u0002.OpComparison);
		}

		// Token: 0x06000C27 RID: 3111 RVA: 0x0001D368 File Offset: 0x0001B568
		public void \u0001(_ICurrentTaskExpression \u0002)
		{
			this.Writer.Write(48U);
			\u0002._Base.Accept(this);
		}

		// Token: 0x06000C28 RID: 3112 RVA: 0x0001D384 File Offset: 0x0001B584
		public void \u0001(_IPoolScopeExpression \u0002)
		{
			this.Writer.Write(47U);
			\u0002._Base.Accept(this);
		}

		// Token: 0x06000C29 RID: 3113 RVA: 0x0001D3A0 File Offset: 0x0001B5A0
		public void \u0001(_ITryCatchStatement \u0002)
		{
			this.Writer.Write(16U);
			\u0002._Try.Accept(this);
			if (\u0002._Catch != null)
			{
				\u0002._Catch.Accept(this);
			}
			else
			{
				this.Writer.Write(0U);
			}
			if (\u0002._Exception != null)
			{
				\u0002._Exception.Accept(this);
			}
			else
			{
				this.Writer.Write(0U);
			}
			if (\u0002._Finally != null)
			{
				\u0002._Finally.Accept(this);
				return;
			}
			this.Writer.Write(0U);
		}

		// Token: 0x06000C2A RID: 3114 RVA: 0x0001D42C File Offset: 0x0001B62C
		public void \u0001(_IPartialAccessExpression \u0002)
		{
			this.Writer.Write(76U);
			\u0002._Left.Accept(this);
			this.Writer.Write((int)\u0002.PartSize);
			this.Writer.Write(\u0002.PartOffset);
		}

		// Token: 0x06000C2B RID: 3115 RVA: 0x0001D46C File Offset: 0x0001B66C
		public void \u0001(_IProjectDefinedExpression \u0002)
		{
			this.Writer.Write(77U);
			_IDefineReference defineReference = \u0002.DefineReference;
			if (defineReference == null)
			{
				return;
			}
			defineReference.Accept(this);
		}

		// Token: 0x06000C2C RID: 3116 RVA: 0x0001D48C File Offset: 0x0001B68C
		public void \u0001(_ICompiledPOU \u0002)
		{
			throw new NotImplementedException();
		}

		// Token: 0x06000C2D RID: 3117 RVA: 0x0001D494 File Offset: 0x0001B694
		public void \u0001(_ICopyScopeExpression \u0002)
		{
			throw new NotImplementedException();
		}

		// Token: 0x06000C2E RID: 3118 RVA: 0x0001D49C File Offset: 0x0001B69C
		public void \u0001(_IQualifiedNameExpression \u0002)
		{
			throw new NotImplementedException();
		}

		// Token: 0x06000C2F RID: 3119 RVA: 0x0001D4A4 File Offset: 0x0001B6A4
		public void \u0001(_IVariableDeclarationStatement \u0002)
		{
			throw new NotImplementedException();
		}

		// Token: 0x06000C30 RID: 3120 RVA: 0x0001D4AC File Offset: 0x0001B6AC
		public void \u0001(_IVariableDeclarationListStatement \u0002)
		{
			throw new NotImplementedException();
		}

		// Token: 0x06000C31 RID: 3121 RVA: 0x0001D4B4 File Offset: 0x0001B6B4
		public void \u0001(_IPOUDeclarationStatement \u0002)
		{
			throw new NotImplementedException();
		}

		// Token: 0x06000C32 RID: 3122 RVA: 0x0001D4BC File Offset: 0x0001B6BC
		public void \u0001(_ITypeDeclarationStatement \u0002)
		{
			throw new NotImplementedException();
		}

		// Token: 0x06000C33 RID: 3123 RVA: 0x0001D4C4 File Offset: 0x0001B6C4
		public void \u0001(_IEnumDeclarationStatement \u0002)
		{
			throw new NotImplementedException();
		}

		// Token: 0x06000C34 RID: 3124 RVA: 0x0001D4CC File Offset: 0x0001B6CC
		public void \u0001(_IEnumDeclarationListStatement \u0002)
		{
			throw new NotImplementedException();
		}

		// Token: 0x06000C35 RID: 3125 RVA: 0x0001D4D4 File Offset: 0x0001B6D4
		public void \u0001(_IBreakPointStatement \u0002)
		{
			throw new NotImplementedException();
		}

		// Token: 0x0400020F RID: 527
		[CompilerGenerated]
		private BinaryWriter \u0001;
	}
}
