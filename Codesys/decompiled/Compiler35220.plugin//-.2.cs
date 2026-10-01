using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using \u000F;
using _3S.CoDeSys.Compiler35220.PreCompile;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace \u0084
{
	// Token: 0x02000079 RID: 121
	internal sealed class \u0002 : IExprementVisitor352000, IExprementVisitor351900, IExprementVisitor351800, IExprementVisitor351500, IExprementVisitor351400, IExprementVisitor351300, IExprementVisitor3590, IExprementVisitor, IExprementVisitor2, IExprementVisitorAdapter
	{
		// Token: 0x06000A3B RID: 2619 RVA: 0x00014628 File Offset: 0x00012828
		public \u0002(bool \u0004\u0002, bool \u0005\u0002, bool \u0006\u0002)
		{
			this.\u0001(Operator.None);
			this.\u0001 = \u0004\u0002;
			this.\u0002 = \u0005\u0002;
			this.\u0003 = \u0006\u0002;
		}

		// Token: 0x17000414 RID: 1044
		// (get) Token: 0x06000A3C RID: 2620 RVA: 0x00014664 File Offset: 0x00012864
		private Operator OperatorContext
		{
			get
			{
				return this.\u0001.Peek();
			}
		}

		// Token: 0x06000A3D RID: 2621 RVA: 0x00014674 File Offset: 0x00012874
		private void \u0001(Operator \u0002)
		{
			this.\u0001.Push(\u0002);
		}

		// Token: 0x06000A3E RID: 2622 RVA: 0x00014684 File Offset: 0x00012884
		private void \u0001()
		{
			this.\u0001.Pop();
		}

		// Token: 0x17000415 RID: 1045
		// (get) Token: 0x06000A3F RID: 2623 RVA: 0x00014694 File Offset: 0x00012894
		public string Output
		{
			get
			{
				return this.\u0001.ToString();
			}
		}

		// Token: 0x17000416 RID: 1046
		// (get) Token: 0x06000A40 RID: 2624 RVA: 0x000146A4 File Offset: 0x000128A4
		internal string Indent
		{
			get
			{
				return new string('\t', this.\u0001);
			}
		}

		// Token: 0x06000A41 RID: 2625 RVA: 0x000146B4 File Offset: 0x000128B4
		private void \u0001(_IExpression \u0002)
		{
			if (!this.\u0001)
			{
				return;
			}
			if (\u0002.Type == null)
			{
				this.\u0001.Append("{unknown}");
				return;
			}
			this.\u0001.Append("{");
			this.\u0001.Append(\u0002.Type);
			this.\u0001.Append("}");
		}

		// Token: 0x06000A42 RID: 2626 RVA: 0x00014718 File Offset: 0x00012918
		[ExcludeFromCodeCoverage]
		internal static bool \u0001(Operator \u0002)
		{
			if (\u0002 <= Operator.__RefAdr)
			{
				if (\u0002 <= Operator.Not)
				{
					switch (\u0002)
					{
					case Operator.__Reloc:
					case Operator.Time:
					case Operator.LTime:
					case Operator.Adr:
					case Operator.BitAdr:
					case Operator.IndexOf:
					case Operator.SizeOf:
					case Operator.Ini:
					case Operator.Abs:
					case Operator.Limit:
					case Operator.Min:
					case Operator.Max:
					case Operator.Trunc:
					case Operator.Mux:
					case Operator.Sel:
					case Operator.Rol:
					case Operator.Ror:
					case Operator.Shl:
					case Operator.Shr:
					case Operator.Exp:
					case Operator.Expt:
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
					case Operator.__Copy:
					case Operator.__Lazy:
					case Operator.Any:
					case Operator.AnyBit:
					case Operator.AnyDate:
					case Operator.AnyInt:
					case Operator.AnyNum:
					case Operator.AnyReal:
					case Operator.Bit:
					case Operator.Bool:
					case Operator.Byte:
					case Operator.Word:
					case Operator.DWord:
					case Operator.LWord:
					case Operator.SInt:
					case Operator.Int:
					case Operator.DInt:
					case Operator.LInt:
					case Operator.USInt:
					case Operator.UInt:
					case Operator.UDInt:
					case Operator.ULInt:
					case Operator.Real:
					case Operator.LReal:
					case Operator.String:
					case Operator.WString:
					case Operator.Date:
					case Operator.DateAndTime:
					case Operator.TimeOfDay:
						return false;
					default:
						if (\u0002 != Operator.Not)
						{
							return false;
						}
						break;
					}
				}
				else if (\u0002 - Operator.Move > 1)
				{
					switch (\u0002)
					{
					case Operator.TruncInt:
					case Operator.__LocalOffset:
					case Operator.__TypeOf:
					case Operator.__CRC:
					case Operator.__MaxOffset:
					case Operator.__Init:
					case Operator.__IsValidRef:
					case Operator.__QueryInterface:
					case Operator.__QueryPointer:
					case Operator.__Delete:
					case Operator.__AdrInst:
					case Operator.__RefAdr:
						break;
					case Operator.FupAssign:
					case Operator.__VarInfo:
					case Operator.__SystemScope:
					case Operator.__New:
					case Operator.__Cast:
						return false;
					default:
						return false;
					}
				}
			}
			else if (\u0002 <= Operator.__CompareAndSwap)
			{
				if (\u0002 - Operator.__BitOffset > 2)
				{
					switch (\u0002)
					{
					case Operator.__MemorySet:
					case Operator.__GetLTick:
					case Operator.__Throw:
					case Operator.__CheckLicense:
					case Operator.__CallInitFunction:
					case Operator.__LateCompiledExpr:
					case Operator.LowerBound:
					case Operator.UpperBound:
					case Operator.__CheckLicenseBit:
					case Operator.__XAdd:
					case Operator.__MemoryBarrier:
					case Operator.__CurrentTask:
					case Operator.__CompareAndSwap:
						break;
					case Operator.And_Then:
					case Operator.Or_Else:
					case Operator.__XInt:
					case Operator.__Try:
					case Operator.__EndTry:
					case Operator.__Catch:
					case Operator.__Finally:
					case Operator.__XString:
					case Operator.VarInst:
					case Operator.AnyString:
					case Operator.__PoolScope:
						return false;
					default:
						return false;
					}
				}
			}
			else if (\u0002 - Operator.__vcSetReal > 4 && \u0002 - Operator.XSizeOf > 2)
			{
				return false;
			}
			return true;
		}

		// Token: 0x06000A43 RID: 2627 RVA: 0x0001493C File Offset: 0x00012B3C
		private static int \u0001(Operator \u0002, Operator \u0003)
		{
			int maxValue;
			if (!\u0084.\u0002.\u0001.TryGetValue(\u0002, out maxValue))
			{
				maxValue = int.MaxValue;
			}
			int maxValue2;
			if (!\u0084.\u0002.\u0001.TryGetValue(\u0003, out maxValue2))
			{
				maxValue2 = int.MaxValue;
			}
			return maxValue.CompareTo(maxValue2);
		}

		// Token: 0x06000A44 RID: 2628 RVA: 0x0001497C File Offset: 0x00012B7C
		private static string \u0001(TypeClass \u0002)
		{
			if (\u0002 <= TypeClass.TimeOfDay)
			{
				if (\u0002 == TypeClass.DateAndTime)
				{
					return "DT";
				}
				if (\u0002 == TypeClass.TimeOfDay)
				{
					return "TOD";
				}
			}
			else
			{
				switch (\u0002)
				{
				case TypeClass.UXInt:
					return "__UXINT";
				case TypeClass.XWord:
					return "__XWORD";
				case TypeClass.XInt:
					return "__XINT";
				default:
					if (\u0002 == TypeClass.LTimeOfDay)
					{
						return "LTOD";
					}
					break;
				}
			}
			return \u0002.ToString().ToUpperInvariant();
		}

		// Token: 0x06000A45 RID: 2629 RVA: 0x000149EC File Offset: 0x00012BEC
		private void \u0001(string \u0002, IEnumerable<IExprement> \u0003)
		{
			bool flag = true;
			foreach (IExprement exprement in \u0003)
			{
				_IExprement iexprement = (_IExprement)exprement;
				if (flag)
				{
					flag = false;
				}
				else
				{
					this.\u0001.Append(\u0002);
				}
				iexprement.Accept(this);
			}
		}

		// Token: 0x06000A46 RID: 2630 RVA: 0x00014A50 File Offset: 0x00012C50
		public void \u0001(_ICompiledPOU \u0002)
		{
			\u0002.GetParseTree().Accept(this);
		}

		// Token: 0x06000A47 RID: 2631 RVA: 0x00014A60 File Offset: 0x00012C60
		public void \u0001(_IEmptyStatement \u0002)
		{
			this.\u0001.AppendLine(";");
		}

		// Token: 0x06000A48 RID: 2632 RVA: 0x00014A74 File Offset: 0x00012C74
		public void \u0001(_ISequenceStatement \u0002)
		{
			foreach (_IStatement istatement in \u0002._StatementList)
			{
				if (!istatement.GetFlag(StatementFlag.GenerateBP))
				{
					this.\u0001.Append("{nobp_flag}");
				}
				istatement.Accept(this);
			}
		}

		// Token: 0x06000A49 RID: 2633 RVA: 0x00014ADC File Offset: 0x00012CDC
		public void \u0001(_IWhileStatement \u0002)
		{
			this.\u0001.Append(this.Indent + "WHILE (");
			\u0002._Condition.Accept(this);
			this.\u0001.AppendLine(") DO");
			this.\u0001++;
			\u0002._Controlled.Accept(this);
			this.\u0001--;
			this.\u0001.AppendLine(this.Indent + "END_WHILE;");
			this.\u0001.AppendLine();
		}

		// Token: 0x06000A4A RID: 2634 RVA: 0x00014B74 File Offset: 0x00012D74
		public void \u0001(_IRepeatStatement \u0002)
		{
			this.\u0001.AppendLine(this.Indent + "REPEAT");
			this.\u0001++;
			\u0002._Controlled.Accept(this);
			this.\u0001--;
			this.\u0001.Append(this.Indent + "UNTIL (");
			\u0002._Condition.Accept(this);
			this.\u0001.AppendLine(")");
			this.\u0001.AppendLine(this.Indent + "END_REPEAT;");
			this.\u0001.AppendLine();
		}

		// Token: 0x06000A4B RID: 2635 RVA: 0x00014C28 File Offset: 0x00012E28
		public void \u0001(_IForStatement \u0002)
		{
			this.\u0001.Append(this.Indent + "FOR ");
			if (\u0002.CounterStart != null)
			{
				_IAssignmentExpression iassignmentExpression = \u0002.CounterStart as _IAssignmentExpression;
				if (iassignmentExpression != null)
				{
					iassignmentExpression._LValue.Accept(this);
					this.\u0001.Append(" := ");
					iassignmentExpression._RValue.Accept(this);
				}
				else
				{
					\u0002._CounterStart.Accept(this);
				}
			}
			this.\u0001.Append(" TO ");
			\u0002._UpperBound.Accept(this);
			if (\u0002.By != null)
			{
				bool flag = \u0002._By is IOperatorExpression;
				this.\u0001.Append(" BY ");
				if (flag)
				{
					this.\u0001.Append("(");
				}
				\u0002._By.Accept(this);
				if (flag)
				{
					this.\u0001.Append(")");
				}
			}
			this.\u0001.AppendLine(" DO");
			this.\u0001++;
			\u0002._Controlled.Accept(this);
			this.\u0001--;
			this.\u0001.AppendLine(this.Indent + "END_FOR;");
		}

		// Token: 0x06000A4C RID: 2636 RVA: 0x00014D6C File Offset: 0x00012F6C
		public void \u0001(_IExitStatement \u0002)
		{
			this.\u0001.AppendLine(this.Indent + "EXIT;");
		}

		// Token: 0x06000A4D RID: 2637 RVA: 0x00014D8C File Offset: 0x00012F8C
		public void \u0001(_IContinueStatement \u0002)
		{
			this.\u0001.AppendLine(this.Indent + "CONTINUE;");
		}

		// Token: 0x06000A4E RID: 2638 RVA: 0x00014DAC File Offset: 0x00012FAC
		public void \u0001(_IAssignmentExpression \u0002)
		{
			bool flag = this.OperatorContext > Operator.None;
			if (flag)
			{
				this.\u0001.Append("(");
			}
			this.\u0001(Operator.None);
			if (\u0002.KindOf == Operator.AssignOut)
			{
				\u0002._RValue.Accept(this);
				this.\u0001.Append(" " + Scanner.GetTextOfOperator(\u0002.KindOf) + " ");
				\u0002._LValue.Accept(this);
			}
			else if (\u0002.KindOf == Operator.FupAssign)
			{
				\u0002._LValue.Accept(this);
				this.\u0001.Append(" := ");
				\u0002._RValue.Accept(this);
			}
			else
			{
				\u0002._LValue.Accept(this);
				this.\u0001.Append(" " + Scanner.GetTextOfOperator(\u0002.KindOf) + " ");
				\u0002._RValue.Accept(this);
			}
			this.\u0001();
			if (flag)
			{
				this.\u0001.Append(")");
			}
		}

		// Token: 0x06000A4F RID: 2639 RVA: 0x00014EBC File Offset: 0x000130BC
		public void \u0001(_ITryCatchStatement \u0002)
		{
			if (\u0002._ReplacedSequence != null)
			{
				\u0002._ReplacedSequence.Accept(this);
				return;
			}
			this.\u0001.AppendLine("__TRY");
			if (\u0002._Try != null)
			{
				this.\u0001++;
				\u0002._Try.Accept(this);
				this.\u0001--;
			}
			if (\u0002._Catch != null)
			{
				this.\u0001.AppendLine("__CATCH");
				this.\u0001++;
				\u0002._Catch.Accept(this);
				this.\u0001--;
			}
			if (\u0002._Finally != null)
			{
				this.\u0001.AppendLine("__FINALLY");
				this.\u0001++;
				\u0002._Finally.Accept(this);
				this.\u0001--;
			}
			this.\u0001.Append("__ENDTRY");
		}

		// Token: 0x06000A50 RID: 2640 RVA: 0x00014FB4 File Offset: 0x000131B4
		public void \u0001(_IIfStatement \u0002)
		{
			this.\u0001.Append(this.Indent + "IF ");
			\u0002._Condition.Accept(this);
			this.\u0001.AppendLine(" THEN");
			this.\u0001++;
			\u0002._IfThen.Accept(this);
			this.\u0001--;
			if (\u0002._ElseIf != null)
			{
				foreach (_IElseIf ielseIf in \u0002._ElseIf)
				{
					this.\u0001.Append(this.Indent + "ELSIF ");
					ielseIf._Condition.Accept(this);
					this.\u0001.AppendLine(" THEN");
					this.\u0001++;
					ielseIf._Controlled.Accept(this);
					this.\u0001--;
				}
			}
			if (\u0002._IfElse != null)
			{
				this.\u0001.AppendLine(this.Indent + "ELSE");
				this.\u0001++;
				\u0002._IfElse.Accept(this);
				this.\u0001--;
			}
			this.\u0001.AppendLine(this.Indent + "END_IF;");
			this.\u0001.AppendLine();
		}

		// Token: 0x06000A51 RID: 2641 RVA: 0x0001513C File Offset: 0x0001333C
		public void \u0001(_IReturnStatement \u0002)
		{
			this.\u0001.Append(this.Indent + "RETURN");
			if (\u0002._Condition != null)
			{
				this.\u0001.Append("(");
				\u0002._Condition.Accept(this);
				this.\u0001.Append(")");
			}
			this.\u0001.AppendLine(";");
		}

		// Token: 0x06000A52 RID: 2642 RVA: 0x000151AC File Offset: 0x000133AC
		public void \u0001(_IJumpStatement \u0002)
		{
			this.\u0001.Append(this.Indent + "JMP ");
			if (\u0002._Condition != null)
			{
				this.\u0001.Append("(");
				\u0002._Condition.Accept(this);
				this.\u0001.Append(")");
			}
			this.\u0001.AppendLine(\u0002.Label + ";");
		}

		// Token: 0x06000A53 RID: 2643 RVA: 0x00015228 File Offset: 0x00013428
		public void \u0001(_ILabelStatement \u0002)
		{
			this.\u0001.AppendLine(this.Indent + \u0002.Text + ":");
		}

		// Token: 0x06000A54 RID: 2644 RVA: 0x0001524C File Offset: 0x0001344C
		public void \u0001(_ICommentStatement \u0002)
		{
			this.\u0001.AppendLine(this.Indent + "(*" + \u0002.Text + "*)");
		}

		// Token: 0x06000A55 RID: 2645 RVA: 0x00015278 File Offset: 0x00013478
		public void \u0001(_IPragmaStatement \u0002)
		{
			_IWarningDisableRestorePragmaStatement iwarningDisableRestorePragmaStatement = \u0002 as _IWarningDisableRestorePragmaStatement;
			if (iwarningDisableRestorePragmaStatement != null)
			{
				string text = iwarningDisableRestorePragmaStatement.Restore ? "restore" : "disable";
				this.\u0001.AppendLine(string.Concat(new string[]
				{
					"{warning ",
					text,
					" ",
					iwarningDisableRestorePragmaStatement.Id,
					"}"
				}));
				return;
			}
			if (\u0002 is _IMessageGuidPragmaStatement)
			{
				this.\u0001.AppendLine(\u0002.Text);
				return;
			}
			this.\u0001.AppendLine("{" + \u0002.Text + "}");
		}

		// Token: 0x06000A56 RID: 2646 RVA: 0x00015320 File Offset: 0x00013520
		public void \u0001(_IExpressionStatement \u0002)
		{
			this.\u0001.Append(this.Indent);
			\u0002._Expr.Accept(this);
			this.\u0001.AppendLine(";");
		}

		// Token: 0x06000A57 RID: 2647 RVA: 0x00015354 File Offset: 0x00013554
		public void \u0001(_IVariableDeclarationStatement \u0002)
		{
			this.\u0001.Append(this.Indent);
			for (int i = 0; i < \u0002.NameList.Count; i++)
			{
				\u0002.NameList[i].Accept(this);
				if (i < \u0002.NameList.Count - 1)
				{
					this.\u0001.Append(", ");
				}
			}
			if (\u0002.Address != null)
			{
				this.\u0001.Append(" AT ");
				this.\u0001.Append(\u0002.Address.ToString());
			}
			this.\u0001.Append(":");
			this.\u0001.Append(\u0002.Type);
			if (\u0002.Type != null && \u0002.Type.Class == TypeClass.Array && \u0002.InputAssigns != null)
			{
				this.\u0002(\u0002);
			}
			else if (\u0002.InputAssigns != null)
			{
				this.\u0001.Append("(");
				this.\u0001(", ", \u0002.InputAssigns);
				this.\u0001.Append(")");
			}
			if (\u0002.Initial != null)
			{
				this.\u0001.Append(" := ");
				\u0002.Initial.Accept(this);
			}
			this.\u0001.AppendLine(";");
		}

		// Token: 0x06000A58 RID: 2648 RVA: 0x000154A8 File Offset: 0x000136A8
		private void \u0002(_IVariableDeclarationStatement \u0002)
		{
			this.\u0001.Append("[");
			_IArrayType iarrayType = \u0002.Type.DeRefType as _IArrayType;
			if (iarrayType != null)
			{
				int count = iarrayType._Dimensions.Count;
				int num = \u0002.InputAssigns.Count / count;
				IList<_IAssignmentExpression> list = (IList<_IAssignmentExpression>)\u0002.InputAssigns;
				int num2 = 0;
				for (int i = 0; i < num; i++)
				{
					this.\u0001.Append("(");
					for (int j = 0; j < count; j++)
					{
						list[num2].Accept(this);
						num2++;
						if (j < count - 1)
						{
							this.\u0001.Append(", ");
						}
					}
					if (i < num - 1)
					{
						this.\u0001.Append("), ");
					}
					else
					{
						this.\u0001.Append(")");
					}
				}
			}
			this.\u0001.Append("]");
		}

		// Token: 0x06000A59 RID: 2649 RVA: 0x000155A4 File Offset: 0x000137A4
		public void \u0001(_IVariableDeclarationListStatement \u0002)
		{
			this.\u0001.Append(this.Indent);
			if (\u0002.GetFlag(VarFlag.Union))
			{
				this.\u0001.Append("UNION");
			}
			else if (\u0002.GetFlag(VarFlag.Structure))
			{
				this.\u0001.Append("STRUCT");
			}
			else if (\u0002.GetFlag(VarFlag.Local))
			{
				this.\u0001.Append("VAR");
			}
			else if (\u0002.GetFlag(VarFlag.Global))
			{
				this.\u0001.Append("VAR_GLOBAL");
			}
			else if (\u0002.GetFlag(VarFlag.Static))
			{
				this.\u0001.Append("VAR_STAT");
			}
			else if (\u0002.GetFlag(VarFlag.Input))
			{
				this.\u0001.Append("VAR_INPUT");
			}
			else if (\u0002.GetFlag(VarFlag.Output))
			{
				this.\u0001.Append("VAR_OUTPUT");
			}
			else if (\u0002.GetFlag(VarFlag.Inout))
			{
				this.\u0001.Append("VAR_IN_OUT");
			}
			else if (\u0002.GetFlag(VarFlag.External))
			{
				this.\u0001.Append("VAR_EXTERNAL");
			}
			else if (\u0002.GetFlag(VarFlag.Temp))
			{
				this.\u0001.Append("VAR_TEMP");
			}
			else if (\u0002.GetFlag(VarFlag.Enum))
			{
				this.\u0001.Append("(");
			}
			if (\u0002.GetFlag(VarFlag.Constant))
			{
				this.\u0001.Append(" CONSTANT");
			}
			if (\u0002.GetFlag(VarFlag.Retain))
			{
				this.\u0001.Append(" RETAIN");
			}
			if (\u0002.GetFlag(VarFlag.Persistent) || \u0002.GetFlag(VarFlag.LocalPersistent))
			{
				this.\u0001.Append(" PERSISTENT");
			}
			this.\u0001.AppendLine();
			this.\u0001++;
			\u0002.VariableDeclaration.Accept(this);
			this.\u0001--;
			if (\u0002.GetFlag(VarFlag.Union))
			{
				this.\u0001.Append("END_UNION");
				return;
			}
			if (\u0002.GetFlag(VarFlag.Structure))
			{
				this.\u0001.Append("END_STRUCT");
				return;
			}
			if (\u0002.GetFlag(VarFlag.Enum))
			{
				this.\u0001.Append(")");
				return;
			}
			this.\u0001.AppendLine("END_VAR");
		}

		// Token: 0x06000A5A RID: 2650 RVA: 0x0001583C File Offset: 0x00013A3C
		private void \u0001(SignatureFlag \u0002)
		{
			if (\u0002.HasFlag(SignatureFlag.Private))
			{
				this.\u0001.Append("PRIVATE ");
			}
			if (\u0002.HasFlag(SignatureFlag.Protected))
			{
				this.\u0001.Append("PROTECTED ");
			}
			if (\u0002.HasFlag(SignatureFlag.Internal))
			{
				this.\u0001.Append("INTERNAL ");
			}
			if (\u0002.HasFlag(SignatureFlag.Abstract))
			{
				this.\u0001.Append("ABSTRACT ");
			}
			if (\u0002.HasFlag(SignatureFlag.Final))
			{
				this.\u0001.Append("FINAL ");
			}
		}

		// Token: 0x06000A5B RID: 2651 RVA: 0x00015928 File Offset: 0x00013B28
		public void \u0001(_IPOUDeclarationStatement \u0002)
		{
			this.\u0001.Append(Scanner.GetTextOfOperator(\u0002.Class));
			this.\u0001.Append(" ");
			this.\u0001(\u0002.Access);
			this.\u0001.Append(\u0002.Name);
			if (\u0002.Extends != null && \u0002.Extends.Count > 0)
			{
				this.\u0001.Append(" EXTENDS ");
				this.\u0001(", ", \u0002.Extends);
			}
			if (\u0002.Implements != null && \u0002.Implements.Count > 0)
			{
				this.\u0001.Append(" IMPLEMENTS ");
				this.\u0001(", ", \u0002.Implements);
			}
			if (\u0002.Type != null)
			{
				LStringBuilder u = this.\u0001;
				string str = ": ";
				_IType type = \u0002.Type;
				u.Append(str + ((type != null) ? type.ToString() : null));
			}
			this.\u0001.AppendLine();
			\u0002.Declarations.Accept(this);
		}

		// Token: 0x06000A5C RID: 2652 RVA: 0x00015A34 File Offset: 0x00013C34
		public void \u0001(_ITypeDeclarationStatement \u0002)
		{
			this.\u0001.AppendLine("TYPE " + \u0002.Name + ":");
			if ((\u0002.Flags & SignatureFlag.Alias) == SignatureFlag.Alias)
			{
				this.\u0001.Append(\u0002.Type.ToString());
			}
			else if (\u0002.Declarations != null)
			{
				\u0002.Declarations.Accept(this);
			}
			else if (\u0002.Type != null)
			{
				this.\u0001.Append(\u0002.Type.ToString());
			}
			if (\u0002.Initial != null)
			{
				LStringBuilder u = this.\u0001;
				string str = " := ";
				_IExpression initial = \u0002.Initial;
				u.Append(str + ((initial != null) ? initial.ToString() : null));
			}
			if ((\u0002.Flags & SignatureFlag.Enum) == SignatureFlag.Enum || (\u0002.Flags & SignatureFlag.Alias) == SignatureFlag.Alias)
			{
				this.\u0001.Append(";");
			}
			this.\u0001.AppendLine();
			this.\u0001.AppendLine("END_TYPE");
		}

		// Token: 0x06000A5D RID: 2653 RVA: 0x00015B38 File Offset: 0x00013D38
		public void \u0001(_IEnumDeclarationStatement \u0002)
		{
			this.\u0001.Append(this.Indent);
			this.\u0001.Append(\u0002.Name);
			if (\u0002.Value != null)
			{
				this.\u0001.AppendFormat(" := {0}", new object[]
				{
					\u0002.Value.ToString()
				});
			}
		}

		// Token: 0x06000A5E RID: 2654 RVA: 0x00015B98 File Offset: 0x00013D98
		public void \u0001(_IEnumDeclarationListStatement \u0002)
		{
			this.\u0001.Append("(");
			this.\u0001.AppendLine();
			this.\u0001.Append("\t");
			this.\u0001("," + Environment.NewLine + "\t", \u0002.Enums);
			this.\u0001.AppendLine();
			this.\u0001.Append(")");
		}

		// Token: 0x06000A5F RID: 2655 RVA: 0x00015C10 File Offset: 0x00013E10
		public void \u0001(_ICallExpression \u0002)
		{
			this.\u0001(Operator.None);
			if (\u0002._Condition != null)
			{
				this.\u0001.Append("CALC(");
				\u0002._Condition.Accept(this);
				this.\u0001.Append(", ");
			}
			\u0002._Callee.Accept(this);
			this.\u0001.Append("(");
			bool flag;
			if (\u0002.Inputs.Count > 0 && \u0002.Inputs[0] == null)
			{
				flag = (\u0002.ParamExpressions.Count > 0);
				this.\u0001(", ", \u0002.ParamExpressions);
			}
			else
			{
				flag = (\u0002._InputAssigns.Count > 0);
				this.\u0001(", ", \u0002._InputAssigns);
			}
			IList<_IAssignmentExpression> outputAssigns = \u0002._OutputAssigns;
			if (flag && outputAssigns.Count > 0)
			{
				this.\u0001.Append(", ");
			}
			this.\u0001(", ", \u0002._OutputAssigns);
			this.\u0001.Append(")");
			if (\u0002._Condition != null)
			{
				this.\u0001.Append(")");
			}
			this.\u0001(\u0002);
			this.\u0001();
		}

		// Token: 0x06000A60 RID: 2656 RVA: 0x00015D44 File Offset: 0x00013F44
		public void \u0001(_ICallInstanceExpression \u0002)
		{
			this.\u0001.Append("__@RegPINST");
		}

		// Token: 0x06000A61 RID: 2657 RVA: 0x00015D58 File Offset: 0x00013F58
		public void \u0001(_IFramePointerExpression \u0002)
		{
			this.\u0001.Append("__@RegFP");
		}

		// Token: 0x06000A62 RID: 2658 RVA: 0x00015D6C File Offset: 0x00013F6C
		public void \u0001(_IProgramCounterExpression \u0002)
		{
			this.\u0001.Append("__@RegPC");
		}

		// Token: 0x06000A63 RID: 2659 RVA: 0x00015D80 File Offset: 0x00013F80
		public void \u0001(_IOperatorExpression \u0002)
		{
			if (\u0084.\u0002.\u0001(\u0002.Code))
			{
				this.\u0001.Append(Scanner.GetTextOfOperator(\u0002.Code));
				this.\u0001.Append("(");
				this.\u0001(Operator.None);
				this.\u0001(", ", \u0002._OperandsList);
				this.\u0001();
				this.\u0001.Append(")");
			}
			else
			{
				string textOfOperator = Scanner.GetTextOfOperator(\u0002.Code);
				bool flag = this.\u0003 && \u0084.\u0002.\u0001(this.OperatorContext, \u0002.Code) <= 0;
				if (!flag)
				{
					this.\u0001.Append("(");
				}
				this.\u0001(\u0002.Code);
				this.\u0001(" " + textOfOperator + " ", \u0002._OperandsList);
				this.\u0001();
				if (!flag)
				{
					this.\u0001.Append(")");
				}
			}
			this.\u0001(\u0002);
		}

		// Token: 0x06000A64 RID: 2660 RVA: 0x00015E80 File Offset: 0x00014080
		public void \u0001(_ICastExpression \u0002)
		{
			this.\u0001.Append("__CAST(");
			\u0002.BaseExpression.Accept(this);
			this.\u0001.Append(",");
			if (\u0002.ExpWithType != null)
			{
				\u0002.ExpWithType.Accept(this);
			}
			if (\u0002.ExplicitelySpecifiedType != null)
			{
				this.\u0001.Append(\u0002.ExplicitelySpecifiedType.ToString());
			}
			this.\u0001.Append(")");
		}

		// Token: 0x06000A65 RID: 2661 RVA: 0x00015F00 File Offset: 0x00014100
		public void \u0001(_INewExpression \u0002)
		{
			this.\u0001.Append("__NEW(");
			this.\u0001.Append(\u0002._TypeToCast.ToString());
			if (\u0002._FBInitParams != null)
			{
				this.\u0001.Append("(");
				this.\u0001(",", \u0002._FBInitParams);
				this.\u0001.Append(")");
			}
			this.\u0001.Append(",");
			\u0002._Count.Accept(this);
			this.\u0001.Append(")");
		}

		// Token: 0x06000A66 RID: 2662 RVA: 0x00015FA0 File Offset: 0x000141A0
		public void \u0001(_ITypeExpression \u0002)
		{
			if (\u0002._CompiledType != null)
			{
				this.\u0001.Append(\u0002._CompiledType.ToString());
			}
		}

		// Token: 0x06000A67 RID: 2663 RVA: 0x00015FC4 File Offset: 0x000141C4
		public void \u0001(_IConversionExpression \u0002)
		{
			if (\u0002.From == \u0002.To)
			{
				\u0002._Exp.Accept(this);
			}
			else
			{
				string text = \u0084.\u0002.\u0001(\u0002.From);
				string text2 = \u0084.\u0002.\u0001(\u0002.To);
				this.\u0001.Append(text);
				this.\u0001.Append("_TO_");
				this.\u0001.Append(text2);
				this.\u0001.Append("(");
				this.\u0001(Operator.None);
				\u0002._Exp.Accept(this);
				this.\u0001();
				this.\u0001.Append(")");
			}
			this.\u0001(\u0002);
		}

		// Token: 0x06000A68 RID: 2664 RVA: 0x00016074 File Offset: 0x00014274
		public void \u0001(_IThisExpression \u0002)
		{
			this.\u0001.Append("THIS");
			this.\u0001(\u0002);
		}

		// Token: 0x06000A69 RID: 2665 RVA: 0x00016090 File Offset: 0x00014290
		public void \u0001(_IBaseExpression \u0002)
		{
			this.\u0001.Append("SUPER");
			this.\u0001(\u0002);
		}

		// Token: 0x06000A6A RID: 2666 RVA: 0x000160AC File Offset: 0x000142AC
		public void \u0001(_ILiteralExpression \u0002)
		{
			TypeClass constantType = \u0002.ConstantType;
			if (constantType <= TypeClass.Enum)
			{
				if (constantType != TypeClass.Bool)
				{
					switch (constantType)
					{
					case TypeClass.LWord:
					case TypeClass.ULInt:
					{
						LStringBuilder u = this.\u0001;
						_IType itype = TypeTable.Get(\u0002.ConstantType);
						u.Append(((itype != null) ? itype.ToString() : null) + "#" + \u0002.ULongValue.ToString());
						goto IL_58B;
					}
					case TypeClass.SInt:
					case TypeClass.Int:
					case TypeClass.DInt:
					case TypeClass.LInt:
					case TypeClass.USInt:
					case TypeClass.UInt:
					case TypeClass.UDInt:
					case TypeClass.Pointer:
					case TypeClass.Reference:
					case TypeClass.Subrange:
						goto IL_456;
					case TypeClass.Real:
					case TypeClass.LReal:
					{
						LStringBuilder u2 = this.\u0001;
						_IType itype2 = TypeTable.Get(\u0002.ConstantType);
						u2.Append(((itype2 != null) ? itype2.ToString() : null) + "#" + \u0002.RealValue.ToString("R", NumberFormatInfo.InvariantInfo));
						goto IL_58B;
					}
					case TypeClass.String:
						this.\u0001.Append("'" + \u0084.\u0002.\u0001(\u0002.StringValue, false) + "'");
						goto IL_58B;
					case TypeClass.WString:
						break;
					case TypeClass.Time:
					{
						LStringBuilder u3 = this.\u0001;
						_IType itype3 = TypeTable.Get(\u0002.ConstantType);
						u3.Append(((itype3 != null) ? itype3.ToString() : null) + "#" + \u000F.\u0001.\u0008(\u0002.ULongValue));
						goto IL_58B;
					}
					case TypeClass.Date:
					{
						LStringBuilder u4 = this.\u0001;
						_IType itype4 = TypeTable.Get(\u0002.ConstantType);
						u4.Append(((itype4 != null) ? itype4.ToString() : null) + "#" + \u000F.\u0001.\u0001(\u0002.ULongValue));
						goto IL_58B;
					}
					case TypeClass.DateAndTime:
					{
						LStringBuilder u5 = this.\u0001;
						_IType itype5 = TypeTable.Get(\u0002.ConstantType);
						u5.Append(((itype5 != null) ? itype5.ToString() : null) + "#" + \u000F.\u0001.\u0005(\u0002.ULongValue));
						goto IL_58B;
					}
					case TypeClass.TimeOfDay:
					{
						LStringBuilder u6 = this.\u0001;
						_IType itype6 = TypeTable.Get(\u0002.ConstantType);
						u6.Append(((itype6 != null) ? itype6.ToString() : null) + "#" + \u000F.\u0001.\u0003(\u0002.ULongValue));
						goto IL_58B;
					}
					case TypeClass.Enum:
					{
						LStringBuilder u7 = this.\u0001;
						_IType itype7 = TypeTable.Get(TypeClass.Int);
						u7.Append(((itype7 != null) ? itype7.ToString() : null) + "#" + \u0002.LongValue.ToString());
						goto IL_58B;
					}
					default:
						goto IL_456;
					}
				}
				else
				{
					if (\u0002.LongValue == 0L)
					{
						this.\u0001.Append("FALSE");
						goto IL_58B;
					}
					this.\u0001.Append("TRUE");
					goto IL_58B;
				}
			}
			else
			{
				switch (constantType)
				{
				case TypeClass.AnyInt:
					\u0084.\u0002.\u0001(\u0002, this.\u0001);
					goto IL_58B;
				case TypeClass.AnyNum:
				case TypeClass.Lazy:
					goto IL_456;
				case TypeClass.AnyReal:
				{
					string text = \u0002.RealValue.ToString("R", NumberFormatInfo.InvariantInfo);
					long num;
					if (long.TryParse(text, out num))
					{
						string text2 = text + ".0";
						double num2;
						if (double.TryParse(text2, out num2))
						{
							text = text2;
						}
					}
					this.\u0001.Append(text);
					goto IL_58B;
				}
				case TypeClass.LTime:
				{
					LStringBuilder u8 = this.\u0001;
					_IType itype8 = TypeTable.Get(\u0002.ConstantType);
					u8.Append(((itype8 != null) ? itype8.ToString() : null) + "#" + \u000F.\u0001.\u0007(\u0002.ULongValue));
					goto IL_58B;
				}
				default:
					switch (constantType)
					{
					case TypeClass.XString:
						break;
					case TypeClass.VarLenArray:
					case TypeClass.AnyString:
					case TypeClass.__Vector:
						goto IL_456;
					case TypeClass.LDate:
					{
						LStringBuilder u9 = this.\u0001;
						_IType itype9 = TypeTable.Get(\u0002.ConstantType);
						u9.Append(((itype9 != null) ? itype9.ToString() : null) + "#" + \u000F.\u0001.\u0002(\u0002.ULongValue));
						goto IL_58B;
					}
					case TypeClass.LDateAndTime:
					{
						LStringBuilder u10 = this.\u0001;
						_IType itype10 = TypeTable.Get(\u0002.ConstantType);
						u10.Append(((itype10 != null) ? itype10.ToString() : null) + "#" + \u000F.\u0001.\u0006(\u0002.ULongValue));
						goto IL_58B;
					}
					case TypeClass.LTimeOfDay:
					{
						LStringBuilder u11 = this.\u0001;
						_IType itype11 = TypeTable.Get(\u0002.ConstantType);
						u11.Append(((itype11 != null) ? itype11.ToString() : null) + "#" + \u000F.\u0001.\u0004(\u0002.ULongValue));
						goto IL_58B;
					}
					default:
						goto IL_456;
					}
					break;
				}
			}
			this.\u0001.Append("\"" + \u0084.\u0002.\u0001(\u0002.StringValue, true) + "\"");
			goto IL_58B;
			IL_456:
			if (TypeTable.Get(\u0002.ConstantType) == null)
			{
				if (\u0002.Negative)
				{
					this.\u0001.Append(\u0002.LongValue);
				}
				else
				{
					this.\u0001.Append(\u0002.ULongValue);
				}
			}
			else if (\u0002.Base != 10)
			{
				if (\u0002.Negative)
				{
					this.\u0001.Append("-" + TypeTable.Get(\u0002.ConstantType).ToString() + "#");
					this.\u0001.AppendFormat("16#{0:X}", new object[]
					{
						0L - \u0002.LongValue
					});
				}
				else
				{
					this.\u0001.Append(TypeTable.Get(\u0002.ConstantType).ToString() + "#");
					this.\u0001.AppendFormat("16#{0:X}", new object[]
					{
						\u0002.ULongValue
					});
				}
			}
			else
			{
				this.\u0001.Append(TypeTable.Get(\u0002.ConstantType).ToString() + "#");
				this.\u0001.Append(\u0002.LongValue);
			}
			IL_58B:
			this.\u0001(\u0002);
		}

		// Token: 0x06000A6B RID: 2667 RVA: 0x0001664C File Offset: 0x0001484C
		private static void \u0001(_ILiteralExpression \u0002, LStringBuilder \u0003)
		{
			if (\u0002.Base == 16)
			{
				if (\u0002.Negative)
				{
					\u0003.AppendFormat("-16#{0:X}", new object[]
					{
						0L - \u0002.LongValue
					});
					return;
				}
				\u0003.AppendFormat("16#{0:X}", new object[]
				{
					\u0002.ULongValue
				});
				return;
			}
			else
			{
				if (\u0002.Base == 8 || \u0002.Base == 2)
				{
					ulong num = \u0002.ULongValue;
					if (\u0002.Negative)
					{
						num = (ulong)(0L - \u0002.LongValue);
						\u0003.Append("-");
					}
					\u0003.AppendFormat("{0}#", new object[]
					{
						\u0002.Base
					});
					int num2 = 0;
					ulong num3 = 1UL;
					while (num3 * (ulong)((long)\u0002.Base) <= num)
					{
						checked
						{
							try
							{
								num3 *= (ulong)\u0002.Base;
							}
							catch (OverflowException)
							{
								break;
							}
						}
						num2++;
					}
					do
					{
						\u0003.AppendFormat("{0}", new object[]
						{
							num / num3
						});
						num %= num3;
						num3 /= (ulong)((long)\u0002.Base);
					}
					while (num3 != 0UL);
					return;
				}
				if (\u0002.Negative)
				{
					\u0003.Append(\u0002.LongValue);
					return;
				}
				\u0003.Append(\u0002.ULongValue);
				return;
			}
		}

		// Token: 0x06000A6C RID: 2668 RVA: 0x00016794 File Offset: 0x00014994
		public static string \u0001(string \u0002, bool \u0003)
		{
			LStringBuilder lstringBuilder = new LStringBuilder();
			int i = 0;
			while (i < \u0002.Length)
			{
				char c = \u0002[i];
				if (c <= '\r')
				{
					if (c != '\0')
					{
						switch (c)
						{
						case '\t':
							lstringBuilder.Append("$T");
							break;
						case '\n':
							lstringBuilder.Append("$N");
							break;
						case '\v':
							goto IL_102;
						case '\f':
							lstringBuilder.Append("$P");
							break;
						case '\r':
							lstringBuilder.Append("$R");
							break;
						default:
							goto IL_102;
						}
					}
					else if (\u0003)
					{
						lstringBuilder.Append("$0000");
					}
					else
					{
						lstringBuilder.Append("$00");
					}
				}
				else if (c != '"')
				{
					if (c != '$')
					{
						if (c != '\'')
						{
							goto IL_102;
						}
						lstringBuilder.Append(\u0003 ? "'" : "$'");
					}
					else
					{
						lstringBuilder.Append("$$");
					}
				}
				else
				{
					lstringBuilder.Append(\u0003 ? "$\"" : "\"");
				}
				IL_150:
				i++;
				continue;
				IL_102:
				if (c >= ' ' || c < '\0')
				{
					lstringBuilder.Append(c);
					goto IL_150;
				}
				if (\u0003)
				{
					lstringBuilder.AppendFormat("{0:X4}", new object[]
					{
						c
					});
					goto IL_150;
				}
				lstringBuilder.AppendFormat("{0:X2}", new object[]
				{
					c
				});
				goto IL_150;
			}
			return lstringBuilder.ToString();
		}

		// Token: 0x06000A6D RID: 2669 RVA: 0x00016908 File Offset: 0x00014B08
		public void \u0001(_IAddressExpression \u0002)
		{
			this.\u0001.Append(\u0002.DirectAddress);
			this.\u0001(\u0002);
		}

		// Token: 0x06000A6E RID: 2670 RVA: 0x00016924 File Offset: 0x00014B24
		public void \u0001(_IQualifiedNameExpression \u0002)
		{
			if (\u0002.Namespace != null && \u0002.Namespace != string.Empty)
			{
				this.\u0001.AppendFormat("{0}.{1}", new object[]
				{
					\u0002.Namespace,
					\u0002.Name
				});
				return;
			}
			this.\u0001.Append(\u0002.Name);
		}

		// Token: 0x06000A6F RID: 2671 RVA: 0x00016988 File Offset: 0x00014B88
		public void \u0001(_IVariableExpression \u0002)
		{
			this.\u0001.Append(\u0002.Name);
			this.\u0001(\u0002);
		}

		// Token: 0x06000A70 RID: 2672 RVA: 0x000169A4 File Offset: 0x00014BA4
		public void \u0001(_IIndexAccessExpression \u0002)
		{
			\u0002._Var.Accept(this);
			this.\u0001.Append("[");
			this.\u0001(Operator.None);
			this.\u0001(", ", \u0002._Accesses);
			this.\u0001();
			this.\u0001.Append("]");
			this.\u0001(\u0002);
		}

		// Token: 0x06000A71 RID: 2673 RVA: 0x00016A04 File Offset: 0x00014C04
		public void \u0001(_ICompoAccessExpression \u0002)
		{
			\u0002._Left.Accept(this);
			this.\u0001.Append(".");
			\u0002._Right.Accept(this);
			this.\u0001(\u0002);
		}

		// Token: 0x06000A72 RID: 2674 RVA: 0x00016A38 File Offset: 0x00014C38
		public void \u0001(_IDeRefAccessExpression \u0002)
		{
			\u0002._Base.Accept(this);
			this.\u0001.Append("^");
			this.\u0001(\u0002);
		}

		// Token: 0x06000A73 RID: 2675 RVA: 0x00016A60 File Offset: 0x00014C60
		public void \u0001(_ICopyScopeExpression \u0002)
		{
			this.\u0001.Append("__COPY.");
			\u0002._Base.Accept(this);
			this.\u0001(\u0002);
		}

		// Token: 0x06000A74 RID: 2676 RVA: 0x00016A88 File Offset: 0x00014C88
		public void \u0001(_IGlobalScopeExpression \u0002)
		{
			this.\u0001.Append(".");
			\u0002._Base.Accept(this);
			this.\u0001(\u0002);
		}

		// Token: 0x06000A75 RID: 2677 RVA: 0x00016AB0 File Offset: 0x00014CB0
		public void \u0001(_ISystemScopeExpression \u0002)
		{
			this.\u0001.Append(Scanner.GetTextOfOperator(Operator.__SystemScope) + ".");
			\u0002._Base.Accept(this);
			this.\u0001(\u0002);
		}

		// Token: 0x06000A76 RID: 2678 RVA: 0x00016AE8 File Offset: 0x00014CE8
		public void \u0001(_IPoolScopeExpression \u0002)
		{
			this.\u0001.Append(Scanner.GetTextOfOperator(Operator.__PoolScope) + ".");
			\u0002._Base.Accept(this);
			this.\u0001(\u0002);
		}

		// Token: 0x06000A77 RID: 2679 RVA: 0x00016B20 File Offset: 0x00014D20
		public void \u0001(_INamespaceAccessExpression \u0002)
		{
			\u0002._Namespace.Accept(this);
			this.\u0001.Append("#");
			\u0002._Access.Accept(this);
			this.\u0001(\u0002);
		}

		// Token: 0x06000A78 RID: 2680 RVA: 0x00016B54 File Offset: 0x00014D54
		public void \u0001(_ICurrentTaskExpression \u0002)
		{
			this.\u0001.Append(Scanner.GetTextOfOperator(Operator.__CurrentTask) + ".");
			\u0002._Base.Accept(this);
			this.\u0001(\u0002);
		}

		// Token: 0x06000A79 RID: 2681 RVA: 0x00016B8C File Offset: 0x00014D8C
		public void \u0001(_ICaseRangeExpression \u0002)
		{
			\u0002._Low.Accept(this);
			this.\u0001.Append("..");
			\u0002._High.Accept(this);
			this.\u0001(\u0002);
		}

		// Token: 0x06000A7A RID: 2682 RVA: 0x00016BC0 File Offset: 0x00014DC0
		public void \u0001(_ICaseLabelStatement \u0002)
		{
			this.\u0001.Append(this.Indent);
			this.\u0001(",", \u0002._cases);
			this.\u0001.AppendLine(":");
		}

		// Token: 0x06000A7B RID: 2683 RVA: 0x00016BF8 File Offset: 0x00014DF8
		public void \u0001(_ICaseStatement \u0002)
		{
			this.\u0001.Append(this.Indent + "CASE ");
			\u0002._Switch.Accept(this);
			this.\u0001.AppendLine(" OF");
			this.\u0001++;
			foreach (_ICase icase in \u0002._Cases)
			{
				icase._Label.Accept(this);
				this.\u0001++;
				icase._Controlled.Accept(this);
				this.\u0001--;
			}
			if (\u0002.Else != null)
			{
				this.\u0001.AppendLine(this.Indent + "ELSE");
				this.\u0001++;
				\u0002._Else.Accept(this);
				this.\u0001--;
			}
			this.\u0001--;
			this.\u0001.AppendLine(this.Indent + "END_CASE");
			this.\u0001.AppendLine();
		}

		// Token: 0x06000A7C RID: 2684 RVA: 0x00016D3C File Offset: 0x00014F3C
		public void \u0001(_IErrorExpression \u0002)
		{
			this.\u0001.Append("!!!'ERROR'!!!");
		}

		// Token: 0x06000A7D RID: 2685 RVA: 0x00016D50 File Offset: 0x00014F50
		public void \u0001(_IErrorStatement \u0002)
		{
			this.\u0001.AppendLine(this.Indent + "!!!'ERROR'!!!");
		}

		// Token: 0x06000A7E RID: 2686 RVA: 0x00016D70 File Offset: 0x00014F70
		public void \u0001(_INullExpression \u0002)
		{
			this.\u0001.Append("'null'");
		}

		// Token: 0x06000A7F RID: 2687 RVA: 0x00016D84 File Offset: 0x00014F84
		public void \u0001(_INullStatement \u0002)
		{
			this.\u0001.AppendLine(this.Indent + "'null'");
		}

		// Token: 0x06000A80 RID: 2688 RVA: 0x00016DA4 File Offset: 0x00014FA4
		public void \u0001(_IMultipleIndexInitialization \u0002)
		{
			\u0002._Number.Accept(this);
			this.\u0001.Append("(");
			\u0002._Value.Accept(this);
			this.\u0001.Append(")");
			this.\u0001(\u0002);
		}

		// Token: 0x06000A81 RID: 2689 RVA: 0x00016DF4 File Offset: 0x00014FF4
		public void \u0001(_IArrayInitialization \u0002)
		{
			IList<_IExpression> initValues = \u0002._InitValues;
			this.\u0001.Append("[");
			for (int i = 0; i < initValues.Count; i++)
			{
				initValues[i].Accept(this);
				if (i < initValues.Count - 1)
				{
					this.\u0001.Append(", ");
				}
			}
			this.\u0001.Append("]");
			this.\u0001(\u0002);
		}

		// Token: 0x06000A82 RID: 2690 RVA: 0x00016E6C File Offset: 0x0001506C
		public void \u0001(_IStructureInitialization \u0002)
		{
			IList<_IAssignmentExpression> compoInits = \u0002._CompoInits;
			this.\u0001.Append("STRUCT(");
			for (int i = 0; i < compoInits.Count; i++)
			{
				compoInits[i]._LValue.Accept(this);
				this.\u0001.Append(" := ");
				compoInits[i]._RValue.Accept(this);
				if (i < compoInits.Count - 1)
				{
					this.\u0001.Append(", ");
				}
			}
			this.\u0001.Append(")");
			this.\u0001(\u0002);
		}

		// Token: 0x06000A83 RID: 2691 RVA: 0x00016F0C File Offset: 0x0001510C
		public void \u0001(_IDefineReference \u0002)
		{
			this.\u0001.Append(\u0002.Define);
		}

		// Token: 0x06000A84 RID: 2692 RVA: 0x00016F20 File Offset: 0x00015120
		public void \u0001(_IVariableReference \u0002)
		{
			this.\u0001.Append("variable:");
			\u0002.InstancePath.Accept(this);
		}

		// Token: 0x06000A85 RID: 2693 RVA: 0x00016F40 File Offset: 0x00015140
		public void \u0001(_ITypeReference \u0002)
		{
			this.\u0001.Append("type:");
			\u0002.InstancePath.Accept(this);
		}

		// Token: 0x06000A86 RID: 2694 RVA: 0x00016F60 File Offset: 0x00015160
		public void \u0001(_IPouReference \u0002)
		{
			this.\u0001.Append("pou:");
			\u0002.InstancePath.Accept(this);
		}

		// Token: 0x06000A87 RID: 2695 RVA: 0x00016F80 File Offset: 0x00015180
		public void \u0001(_ITaskReference \u0002)
		{
			this.\u0001.Append("task:" + \u0002.TaskName);
		}

		// Token: 0x06000A88 RID: 2696 RVA: 0x00016FA0 File Offset: 0x000151A0
		public void \u0001(_IResourceReference \u0002)
		{
			this.\u0001.Append("resource:" + \u0002.ResourceName);
		}

		// Token: 0x06000A89 RID: 2697 RVA: 0x00016FC0 File Offset: 0x000151C0
		public void \u0001(_IDefinedExpression \u0002)
		{
			this.\u0001.Append("defined(");
			\u0002.ItemReference.Accept(this);
			this.\u0001.Append(")");
		}

		// Token: 0x06000A8A RID: 2698 RVA: 0x00016FF0 File Offset: 0x000151F0
		public void \u0001(_IPragmaOperatorExpression \u0002)
		{
			IList<_IExpression> operands = \u0002.Operands;
			string str = "NOP";
			PragmaOperator code = \u0002.Code;
			if (code != PragmaOperator.Or)
			{
				if (code == PragmaOperator.And)
				{
					str = "AND";
				}
			}
			else
			{
				str = "OR";
			}
			for (int i = 0; i < operands.Count; i++)
			{
				if (\u0002.Code == PragmaOperator.Not)
				{
					this.\u0001.Append("NOT ");
				}
				operands[i].Accept(this);
				if (\u0002.Code == PragmaOperator.Not)
				{
					break;
				}
				if (i < operands.Count - 1)
				{
					this.\u0001.Append(" " + str + " ");
				}
			}
		}

		// Token: 0x06000A8B RID: 2699 RVA: 0x00017094 File Offset: 0x00015294
		public void \u0001(_IPragmaAssertion \u0002)
		{
			this.\u0001.Append("{assert(");
			\u0002.Condition.Accept(this);
			this.\u0001.Append(",");
			this.\u0001.Append(\u0002.ErrorOutput);
			this.\u0001.Append(")}");
		}

		// Token: 0x06000A8C RID: 2700 RVA: 0x000170F4 File Offset: 0x000152F4
		public void \u0001(_ICompilerVersionExpression \u0002)
		{
			this.\u0001.Append("{COMPILERVERSION(");
			this.\u0001.Append(Scanner.GetTextOfOperator(\u0002.OpComparison));
			this.\u0001.Append(",");
			this.\u0001.Append(\u0002.VersionToTest);
			this.\u0001.Append(")}");
		}

		// Token: 0x06000A8D RID: 2701 RVA: 0x00017160 File Offset: 0x00015360
		public void \u0001(_IRuntimeVersionExpression \u0002)
		{
			this.\u0001.Append("{RUNTIMEVERSION(");
			this.\u0001.Append(Scanner.GetTextOfOperator(\u0002.OpComparison));
			this.\u0001.Append(",");
			this.\u0001.Append(\u0002.VersionToTest);
			this.\u0001.Append(")}");
		}

		// Token: 0x06000A8E RID: 2702 RVA: 0x000171CC File Offset: 0x000153CC
		public void \u0001(_IPragmaIfStatement \u0002)
		{
			this.\u0001.Append("{IF ");
			\u0002.Condition.Accept(this);
			this.\u0001.AppendLine("}");
			\u0002.IfThen.Accept(this);
			if (\u0002.ElseIf != null)
			{
				foreach (_IPragmaElseIf ipragmaElseIf in \u0002.ElseIf)
				{
					this.\u0001.Append("{ELSIF ");
					ipragmaElseIf.Condition.Accept(this);
					this.\u0001.AppendLine("}");
					ipragmaElseIf.Controlled.Accept(this);
				}
			}
			if (\u0002.IfElse != null)
			{
				this.\u0001.AppendLine("{ELSE}");
				\u0002.IfElse.Accept(this);
			}
			this.\u0001.AppendLine("{END_IF}");
			this.\u0001.AppendLine();
		}

		// Token: 0x06000A8F RID: 2703 RVA: 0x000172D0 File Offset: 0x000154D0
		public void \u0001(_IBreakPointStatement \u0002)
		{
			if (\u0002.SuccessorPosition == Scanner.InvalidPosition)
			{
				this.\u0001.AppendLine("{" + string.Format("p {0} bp", \u0002.BPPosition) + "}");
				return;
			}
			this.\u0001.AppendLine("{" + string.Format("p {0} bp suc {1}", \u0002.BPPosition, \u0002.SuccessorPosition) + "}");
		}

		// Token: 0x06000A90 RID: 2704 RVA: 0x00017358 File Offset: 0x00015558
		public void \u0001(_IDefineStatement \u0002)
		{
			if (\u0002.Ident == null)
			{
				return;
			}
			if (\u0002.Define)
			{
				this.\u0001.Append('{');
				if (\u0002.Value != null)
				{
					this.\u0001.Append(string.Format("define {0} '{1}'", \u0002.Ident, \u0002.Value));
				}
				else
				{
					this.\u0001.Append(string.Format("define {0}", \u0002.Ident));
				}
				this.\u0001.AppendLine("}");
				return;
			}
			this.\u0001.AppendLine("{undefine " + \u0002.Ident + "}");
		}

		// Token: 0x06000A91 RID: 2705 RVA: 0x00017400 File Offset: 0x00015600
		public void \u0001(_IXRefExpression \u0002)
		{
			this.\u0001.Append("xref(");
			\u0002.XRef.Accept(this);
			if (\u0002.XRefFrom != null)
			{
				this.\u0001.Append(" from ");
				\u0002.XRefFrom.Accept(this);
			}
			this.\u0001.Append(")");
		}

		// Token: 0x06000A92 RID: 2706 RVA: 0x00017460 File Offset: 0x00015660
		public void \u0001(_IHasTypeExpression \u0002)
		{
			this.\u0001.Append("hastype(");
			\u0002.Variable.Accept(this);
			this.\u0001.Append(", ");
			if (\u0002.ReferencedType != null)
			{
				this.\u0001.Append(\u0002.ReferencedType.ToString());
			}
			if (!\u0002.Exact)
			{
				this.\u0001.Append(", FALSE");
			}
			this.\u0001.Append(")");
		}

		// Token: 0x06000A93 RID: 2707 RVA: 0x000174E4 File Offset: 0x000156E4
		public void \u0001(_IIsEnumTypeExpression \u0002)
		{
			this.\u0001.Append("isenumtype(type:");
			this.\u0001.Append(\u0002.ReferencedType.ToString());
			this.\u0001.Append(")");
		}

		// Token: 0x06000A94 RID: 2708 RVA: 0x00017520 File Offset: 0x00015720
		public void \u0001(_IHasAttributeExpression \u0002)
		{
			this.\u0001.Append("hasattribute(");
			\u0002.ItemReference.Accept(this);
			this.\u0001.Append(", ");
			this.\u0001.Append(\u0002.Attribute);
			this.\u0001.Append(")");
		}

		// Token: 0x06000A95 RID: 2709 RVA: 0x00017580 File Offset: 0x00015780
		public void \u0001(_IHasValueExpression \u0002)
		{
			this.\u0001.Append("hasvalue(");
			this.\u0001.Append(\u0002.Define);
			this.\u0001.Append(", ");
			this.\u0001.Append(\u0002.DefineValue);
			this.\u0001.Append(")");
		}

		// Token: 0x06000A96 RID: 2710 RVA: 0x000175E4 File Offset: 0x000157E4
		public void \u0001(_IHasConstantValueExpression \u0002)
		{
			this.\u0001.Append("hasconstantvalue(");
			this.\u0001.Append(\u0002._Constant);
			this.\u0001.Append(", ");
			this.\u0001.Append(\u0002._ConstantValue);
			this.\u0001.Append(", ");
			this.\u0001.Append(Scanner.GetTextOfOperator(\u0002.OpComparison));
			this.\u0001.Append(")");
		}

		// Token: 0x06000A97 RID: 2711 RVA: 0x00017670 File Offset: 0x00015870
		public void \u0001(_IHasConstantTypeExpression \u0002)
		{
			this.\u0001.Append("hasconstanttype(");
			this.\u0001.Append(\u0002.Constant);
			this.\u0001.Append(", ");
			this.\u0001.Append(\u0002.ConstantTypeReplaced ? "TRUE" : "FALSE");
			this.\u0001.Append(")");
		}

		// Token: 0x06000A98 RID: 2712 RVA: 0x000176E4 File Offset: 0x000158E4
		public void \u0001(_IPartialAccessExpression \u0002)
		{
			\u0002._Left.Accept(this);
			this.\u0001.Append(".%");
			this.\u0001.Append(\u0084.\u0002.\u0001(\u0002.PartSize));
			this.\u0001.Append(\u0002.PartOffset);
		}

		// Token: 0x06000A99 RID: 2713 RVA: 0x00017738 File Offset: 0x00015938
		public void \u0001(_IProjectDefinedExpression \u0002)
		{
			this.\u0001.Append("project_defined(");
			_IDefineReference defineReference = \u0002.DefineReference;
			if (defineReference != null)
			{
				defineReference.Accept(this);
			}
			this.\u0001.Append(")");
		}

		// Token: 0x06000A9A RID: 2714 RVA: 0x00017770 File Offset: 0x00015970
		private static string \u0001(DirectVariableSize \u0002)
		{
			switch (\u0002)
			{
			case DirectVariableSize.X:
				return "X";
			case DirectVariableSize.B:
				return "B";
			case DirectVariableSize.W:
				return "W";
			case DirectVariableSize.D:
				return "D";
			case DirectVariableSize.L:
				return "L";
			default:
				return "?";
			}
		}

		// Token: 0x04000142 RID: 322
		private readonly bool \u0001;

		// Token: 0x04000143 RID: 323
		private readonly bool \u0002;

		// Token: 0x04000144 RID: 324
		private readonly bool \u0003;

		// Token: 0x04000145 RID: 325
		private readonly Stack<Operator> \u0001 = new Stack<Operator>();

		// Token: 0x04000146 RID: 326
		private readonly LStringBuilder \u0001 = new LStringBuilder();

		// Token: 0x04000147 RID: 327
		private int \u0001;

		// Token: 0x04000148 RID: 328
		private static readonly Dictionary<Operator, int> \u0001 = new Dictionary<Operator, int>
		{
			{
				Operator.None,
				int.MinValue
			},
			{
				Operator.Or,
				0
			},
			{
				Operator.Or_Else,
				0
			},
			{
				Operator.Xor,
				0
			},
			{
				Operator.And,
				1
			},
			{
				Operator.And_Then,
				1
			},
			{
				Operator.Equal,
				2
			},
			{
				Operator.NotEqual,
				2
			},
			{
				Operator.Less,
				2
			},
			{
				Operator.LessEqual,
				2
			},
			{
				Operator.Greater,
				2
			},
			{
				Operator.GreaterEqual,
				2
			},
			{
				Operator.Add,
				3
			},
			{
				Operator.Sub,
				3
			},
			{
				Operator.Plus,
				3
			},
			{
				Operator.Minus,
				3
			},
			{
				Operator.__vcAdd,
				3
			},
			{
				Operator.__vcSub,
				3
			},
			{
				Operator.Times,
				4
			},
			{
				Operator.Divide,
				4
			},
			{
				Operator.Mod,
				4
			},
			{
				Operator.__vcMul,
				4
			},
			{
				Operator.__vcDiv,
				4
			},
			{
				Operator.__vcDot,
				4
			}
		};
	}
}
