using System;
using System.Collections;
using System.Collections.Generic;
using CODESYS.Parser35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Utilities;

namespace CODESYS.Parser35220.Scanner
{
	// Token: 0x02000014 RID: 20
	internal class OperatorTable
	{
		// Token: 0x060001A5 RID: 421 RVA: 0x000089F1 File Offset: 0x00006BF1
		private OperatorTable()
		{
			this.UpdateOperatorTable();
		}

		// Token: 0x17000031 RID: 49
		// (get) Token: 0x060001A6 RID: 422 RVA: 0x00008A2B File Offset: 0x00006C2B
		internal static OperatorTable Instance { get; } = new OperatorTable();

		// Token: 0x17000032 RID: 50
		public Operator this[string st, bool bIgnoreCase]
		{
			get
			{
				OperatorNode operatorNode = this._root;
				int num = 0;
				int length = st.Length;
				while (operatorNode != null && num < length)
				{
					int num2 = this.Compare(st[num], operatorNode.Char, bIgnoreCase);
					if (num2 < 0)
					{
						operatorNode = operatorNode.Less;
					}
					else if (num2 > 0)
					{
						operatorNode = operatorNode.Greater;
					}
					else
					{
						if (num == length - 1 && operatorNode.OperatorDesc != null)
						{
							return operatorNode.OperatorDesc.Operator;
						}
						operatorNode = operatorNode.Equal;
						num++;
					}
				}
				return 0;
			}
		}

		// Token: 0x17000033 RID: 51
		public Operator this[QuickStringBuilder qsb, bool bIgnoreCase]
		{
			get
			{
				OperatorNode operatorNode = this._root;
				int num = 0;
				int length = qsb.Length;
				while (operatorNode != null && num < length)
				{
					int num2 = this.Compare(qsb[num], operatorNode.Char, bIgnoreCase);
					if (num2 < 0)
					{
						operatorNode = operatorNode.Less;
					}
					else if (num2 > 0)
					{
						operatorNode = operatorNode.Greater;
					}
					else
					{
						if (num == length - 1 && operatorNode.OperatorDesc != null)
						{
							return operatorNode.OperatorDesc.Operator;
						}
						operatorNode = operatorNode.Equal;
						num++;
					}
				}
				return 0;
			}
		}

		// Token: 0x17000034 RID: 52
		public Operator this[char[] input, IToken token, bool bIgnoreCase]
		{
			get
			{
				OperatorNode operatorNode = this._root;
				int num = 0;
				int length = token.Length;
				int sourceOffset = token.SourceOffset;
				while (operatorNode != null && num < length)
				{
					int num2 = this.Compare(input[sourceOffset + num], operatorNode.Char, bIgnoreCase);
					if (num2 < 0)
					{
						operatorNode = operatorNode.Less;
					}
					else if (num2 > 0)
					{
						operatorNode = operatorNode.Greater;
					}
					else
					{
						if (num == length - 1 && operatorNode.OperatorDesc != null)
						{
							return operatorNode.OperatorDesc.Operator;
						}
						operatorNode = operatorNode.Equal;
						num++;
					}
				}
				return 0;
			}
		}

		// Token: 0x060001AA RID: 426 RVA: 0x00008BAF File Offset: 0x00006DAF
		private int Compare(char c1, char c2, bool bIgnoreCase)
		{
			if (bIgnoreCase)
			{
				if (c1 >= 'a' && c1 <= 'z')
				{
					c1 = c1 + 'A' - 'a';
				}
				if (c2 >= 'a' && c2 <= 'z')
				{
					c2 = c2 + 'A' - 'a';
				}
			}
			return (int)(c1 - c2);
		}

		// Token: 0x060001AB RID: 427 RVA: 0x00008BE0 File Offset: 0x00006DE0
		public Operator[] GetByFlags(OperatorFlags flags)
		{
			Hashtable hashtable = new Hashtable();
			List<OperatorDesc> all_LockRequired = this._all_LockRequired;
			lock (all_LockRequired)
			{
				foreach (OperatorDesc operatorDesc in this._all_LockRequired)
				{
					if ((operatorDesc.Flags & flags) == flags)
					{
						hashtable[operatorDesc.Operator] = null;
					}
				}
			}
			Operator[] array = new Operator[hashtable.Count];
			hashtable.Keys.CopyTo(array, 0);
			return array;
		}

		// Token: 0x060001AC RID: 428 RVA: 0x00008C94 File Offset: 0x00006E94
		public void Insert(IDictionary dictionary)
		{
			SortedList sortedList = new SortedList(dictionary);
			this.Insert(sortedList, 0, sortedList.Count - 1);
		}

		// Token: 0x060001AD RID: 429 RVA: 0x00008CB8 File Offset: 0x00006EB8
		public void FillOperatorTable(IDictionary dictionary)
		{
			this._textOfOperatorLong.Clear();
			this._textOfOperatorShort.Clear();
			foreach (object obj in dictionary)
			{
				DictionaryEntry dictionaryEntry = (DictionaryEntry)obj;
				OperatorDesc operatorDesc = (OperatorDesc)dictionaryEntry.Value;
				string value = dictionaryEntry.Key as string;
				Operator @operator = operatorDesc.Operator;
				if (@operator <= 32)
				{
					if (@operator == 31)
					{
						this._textOfOperatorLong[operatorDesc.Operator] = "DATE_AND_TIME";
						this._textOfOperatorShort[operatorDesc.Operator] = "DT";
						continue;
					}
					if (@operator == 32)
					{
						this._textOfOperatorLong[operatorDesc.Operator] = "TIME_OF_DAY";
						this._textOfOperatorShort[operatorDesc.Operator] = "TOD";
						continue;
					}
				}
				else
				{
					if (@operator == 184)
					{
						continue;
					}
					if (@operator == 275)
					{
						this._textOfOperatorLong[operatorDesc.Operator] = "LDATE_AND_TIME";
						this._textOfOperatorShort[operatorDesc.Operator] = "LDT";
						continue;
					}
					if (@operator == 276)
					{
						this._textOfOperatorLong[operatorDesc.Operator] = "LTIME_OF_DAY";
						this._textOfOperatorShort[operatorDesc.Operator] = "LTOD";
						continue;
					}
				}
				Debug.Assert(!this._textOfOperatorLong.ContainsKey(operatorDesc.Operator));
				Debug.Assert(!this._textOfOperatorShort.ContainsKey(operatorDesc.Operator));
				this._textOfOperatorLong[operatorDesc.Operator] = value;
				this._textOfOperatorShort[operatorDesc.Operator] = value;
				if ((operatorDesc.Flags & OperatorFlags.Contextual) == OperatorFlags.Contextual)
				{
					this._contextualOperators.Add(operatorDesc.Operator);
				}
			}
			this._textOfOperatorShort[223] = "CLASS";
			this._textOfOperatorShort[225] = "OVERRIDE";
			this._textOfOperatorLong[223] = "CLASS";
			this._textOfOperatorLong[225] = "OVERRIDE";
		}

		// Token: 0x060001AE RID: 430 RVA: 0x00008F1C File Offset: 0x0000711C
		private void Insert(ref OperatorNode current, string st, OperatorDesc opDesc)
		{
			if (st.Length == 0)
			{
				return;
			}
			if (current == null)
			{
				current = new OperatorNode();
				current.Char = st[0];
			}
			if (st[0] < current.Char)
			{
				this.Insert(ref current.Less, st, opDesc);
				return;
			}
			if (st[0] > current.Char)
			{
				this.Insert(ref current.Greater, st, opDesc);
				return;
			}
			if (st.Length == 1)
			{
				current.OperatorDesc = opDesc;
				List<OperatorDesc> all_LockRequired = this._all_LockRequired;
				lock (all_LockRequired)
				{
					this._all_LockRequired.Add(opDesc);
					return;
				}
			}
			this.Insert(ref current.Equal, st.Substring(1), opDesc);
		}

		// Token: 0x060001AF RID: 431 RVA: 0x00008FEC File Offset: 0x000071EC
		private void Insert(SortedList list, int nLeft, int nRight)
		{
			if (nLeft > nRight)
			{
				return;
			}
			int num = (nLeft + nRight) / 2;
			this.Insert(ref this._root, (string)list.GetKey(num), (OperatorDesc)list.GetByIndex(num));
			this.Insert(list, nLeft, num - 1);
			this.Insert(list, num + 1, nRight);
		}

		// Token: 0x060001B0 RID: 432 RVA: 0x0000903E File Offset: 0x0000723E
		public string GetTextOfOperatorShort(Operator op)
		{
			if (!this._textOfOperatorShort.ContainsKey(op))
			{
				return "ERROR";
			}
			return this._textOfOperatorShort[op];
		}

		// Token: 0x060001B1 RID: 433 RVA: 0x00009060 File Offset: 0x00007260
		public string GetTextOfOperatorLong(Operator op)
		{
			if (!this._textOfOperatorLong.ContainsKey(op))
			{
				return "ERROR";
			}
			return this._textOfOperatorLong[op];
		}

		// Token: 0x060001B2 RID: 434 RVA: 0x00009082 File Offset: 0x00007282
		public bool IsContextualOperator(Operator op)
		{
			return this._contextualOperators.Contains(op);
		}

		// Token: 0x060001B3 RID: 435 RVA: 0x00009090 File Offset: 0x00007290
		public string GetTextOfOperator(Operator op, bool bShort)
		{
			if (bShort)
			{
				return this.GetTextOfOperatorShort(op);
			}
			return this.GetTextOfOperatorLong(op);
		}

		// Token: 0x060001B4 RID: 436 RVA: 0x000090A4 File Offset: 0x000072A4
		public string GetTextOfOperator(Operator op)
		{
			return this.GetTextOfOperatorLong(op);
		}

		// Token: 0x060001B5 RID: 437 RVA: 0x000090AD File Offset: 0x000072AD
		public Operator GetOperatorFromText(string stOperator)
		{
			return this[stOperator, false];
		}

		// Token: 0x060001B6 RID: 438 RVA: 0x000090B8 File Offset: 0x000072B8
		public Operator[] GetKeywords(IECLanguage language)
		{
			switch (language)
			{
			case 0:
				return this.GetByFlags(OperatorFlags.Keyword | OperatorFlags.StructuredText);
			case 1:
				return this.GetByFlags(OperatorFlags.Keyword | OperatorFlags.InstructionList);
			case 2:
				return this.GetByFlags(OperatorFlags.Keyword | OperatorFlags.FunctionBlockDiagram);
			case 3:
				return this.GetByFlags(OperatorFlags.Keyword | OperatorFlags.Declaration);
			default:
				return new Operator[0];
			}
		}

		// Token: 0x060001B7 RID: 439 RVA: 0x00009114 File Offset: 0x00007314
		public Operator[] GetOperators(IECLanguage language)
		{
			switch (language)
			{
			case 0:
				return this.GetByFlags(OperatorFlags.Operator | OperatorFlags.StructuredText);
			case 1:
				return this.GetByFlags(OperatorFlags.Operator | OperatorFlags.InstructionList);
			case 2:
				return this.GetByFlags(OperatorFlags.Operator | OperatorFlags.FunctionBlockDiagram);
			case 3:
				return this.GetByFlags(OperatorFlags.Operator | OperatorFlags.Declaration);
			default:
				return new Operator[0];
			}
		}

		// Token: 0x060001B8 RID: 440 RVA: 0x00009170 File Offset: 0x00007370
		private void UpdateOperatorTable()
		{
			Dictionary<string, OperatorDesc> operators = new Dictionary<string, OperatorDesc>();
			this.AddSpecialOperators(operators);
			this.AddDataTypeNames(operators);
			this.AddOOKeywords(operators);
			this.AddStandardOperators(operators);
			this.AddKeywords(operators);
			this.AddVectorOperatos(operators);
			this.AddILOperators(operators);
			this.AddOperatorSymbols(operators);
			this.AddConversionOperators(operators);
		}

		// Token: 0x060001B9 RID: 441 RVA: 0x000091C4 File Offset: 0x000073C4
		private void AddOperatorSymbols(IDictionary<string, OperatorDesc> operators)
		{
			operators["+"] = new OperatorDesc(157, OperatorFlags.Operator | OperatorFlags.StructuredText | OperatorFlags.Declaration);
			operators["-"] = new OperatorDesc(158, OperatorFlags.Operator | OperatorFlags.StructuredText | OperatorFlags.Declaration);
			operators["*"] = new OperatorDesc(159, OperatorFlags.Operator | OperatorFlags.StructuredText | OperatorFlags.Declaration);
			operators["**"] = new OperatorDesc(160, OperatorFlags.Operator | OperatorFlags.Declaration | OperatorFlags.Internal);
			operators["/"] = new OperatorDesc(161, OperatorFlags.Operator | OperatorFlags.StructuredText | OperatorFlags.Declaration);
			operators["."] = new OperatorDesc(162, OperatorFlags.Operator | OperatorFlags.StructuredText | OperatorFlags.InstructionList | OperatorFlags.Declaration);
			operators["#"] = new OperatorDesc(271, OperatorFlags.Operator | OperatorFlags.StructuredText | OperatorFlags.InstructionList | OperatorFlags.Declaration);
			operators[":"] = new OperatorDesc(163, OperatorFlags.Operator | OperatorFlags.StructuredText | OperatorFlags.InstructionList | OperatorFlags.Declaration);
			operators[":="] = new OperatorDesc(164, OperatorFlags.Operator | OperatorFlags.StructuredText | OperatorFlags.InstructionList | OperatorFlags.Declaration);
			operators["=:"] = new OperatorDesc(189, OperatorFlags.Internal);
			operators["S="] = new OperatorDesc(165, OperatorFlags.Operator | OperatorFlags.StructuredText);
			operators["R="] = new OperatorDesc(166, OperatorFlags.Operator | OperatorFlags.StructuredText);
			operators["REF="] = new OperatorDesc(185, OperatorFlags.Operator | OperatorFlags.StructuredText);
			operators["("] = new OperatorDesc(167, OperatorFlags.Operator | OperatorFlags.StructuredText | OperatorFlags.InstructionList | OperatorFlags.Declaration);
			operators[")"] = new OperatorDesc(168, OperatorFlags.Operator | OperatorFlags.StructuredText | OperatorFlags.InstructionList | OperatorFlags.Declaration);
			operators["["] = new OperatorDesc(169, OperatorFlags.Operator | OperatorFlags.StructuredText | OperatorFlags.InstructionList | OperatorFlags.Declaration);
			operators["]"] = new OperatorDesc(170, OperatorFlags.Operator | OperatorFlags.StructuredText | OperatorFlags.InstructionList | OperatorFlags.Declaration);
			operators[","] = new OperatorDesc(171, OperatorFlags.Operator | OperatorFlags.StructuredText | OperatorFlags.InstructionList | OperatorFlags.Declaration);
			operators[";"] = new OperatorDesc(172, OperatorFlags.Operator | OperatorFlags.StructuredText | OperatorFlags.Declaration);
			operators[".."] = new OperatorDesc(173, OperatorFlags.Operator | OperatorFlags.StructuredText | OperatorFlags.Declaration);
			operators["=>"] = new OperatorDesc(174, OperatorFlags.Operator | OperatorFlags.StructuredText | OperatorFlags.InstructionList);
			operators["<"] = new OperatorDesc(175, OperatorFlags.Operator | OperatorFlags.StructuredText);
			operators[">"] = new OperatorDesc(176, OperatorFlags.Operator | OperatorFlags.StructuredText);
			operators["<="] = new OperatorDesc(177, OperatorFlags.Operator | OperatorFlags.StructuredText);
			operators[">="] = new OperatorDesc(178, OperatorFlags.Operator | OperatorFlags.StructuredText);
			operators["="] = new OperatorDesc(179, OperatorFlags.Operator | OperatorFlags.StructuredText);
			operators["<>"] = new OperatorDesc(180, OperatorFlags.Operator | OperatorFlags.StructuredText);
			operators["&"] = new OperatorDesc(181, OperatorFlags.Operator | OperatorFlags.StructuredText);
			operators["|"] = new OperatorDesc(182, OperatorFlags.Operator | OperatorFlags.StructuredText);
			operators["^"] = new OperatorDesc(183, OperatorFlags.Operator | OperatorFlags.StructuredText | OperatorFlags.InstructionList | OperatorFlags.Declaration);
		}

		// Token: 0x060001BA RID: 442 RVA: 0x000094E0 File Offset: 0x000076E0
		private void AddILOperators(IDictionary<string, OperatorDesc> operators)
		{
			operators["XOR"] = new OperatorDesc(131, OperatorFlags.Operator | OperatorFlags.StructuredText | OperatorFlags.InstructionList | OperatorFlags.FunctionBlockDiagram);
			operators["XORN"] = new OperatorDesc(132, OperatorFlags.Operator | OperatorFlags.InstructionList);
			operators["NOT"] = new OperatorDesc(133, OperatorFlags.Operator | OperatorFlags.StructuredText | OperatorFlags.InstructionList | OperatorFlags.FunctionBlockDiagram);
			operators["EQ"] = new OperatorDesc(134, OperatorFlags.Operator | OperatorFlags.InstructionList | OperatorFlags.FunctionBlockDiagram);
			operators["NE"] = new OperatorDesc(135, OperatorFlags.Operator | OperatorFlags.InstructionList | OperatorFlags.FunctionBlockDiagram);
			operators["GE"] = new OperatorDesc(136, OperatorFlags.Operator | OperatorFlags.InstructionList | OperatorFlags.FunctionBlockDiagram);
			operators["GT"] = new OperatorDesc(137, OperatorFlags.Operator | OperatorFlags.InstructionList | OperatorFlags.FunctionBlockDiagram);
			operators["LE"] = new OperatorDesc(138, OperatorFlags.Operator | OperatorFlags.InstructionList | OperatorFlags.FunctionBlockDiagram);
			operators["LT"] = new OperatorDesc(139, OperatorFlags.Operator | OperatorFlags.InstructionList | OperatorFlags.FunctionBlockDiagram);
			operators["CAL"] = new OperatorDesc(140, OperatorFlags.Operator | OperatorFlags.InstructionList);
			operators["CALC"] = new OperatorDesc(141, OperatorFlags.Operator | OperatorFlags.InstructionList);
			operators["CALCN"] = new OperatorDesc(142, OperatorFlags.Operator | OperatorFlags.InstructionList);
			operators["JMP"] = new OperatorDesc(143, OperatorFlags.Operator | OperatorFlags.InstructionList);
			operators["JMPC"] = new OperatorDesc(144, OperatorFlags.Operator | OperatorFlags.InstructionList);
			operators["JMPCN"] = new OperatorDesc(145, OperatorFlags.Operator | OperatorFlags.InstructionList);
			operators["RET"] = new OperatorDesc(146, OperatorFlags.Operator | OperatorFlags.InstructionList);
			operators["RETC"] = new OperatorDesc(147, OperatorFlags.Operator | OperatorFlags.InstructionList);
			operators["RETCN"] = new OperatorDesc(148, OperatorFlags.Operator | OperatorFlags.InstructionList);
			operators["LD"] = new OperatorDesc(149, OperatorFlags.Operator | OperatorFlags.InstructionList);
			operators["LDN"] = new OperatorDesc(150, OperatorFlags.Operator | OperatorFlags.InstructionList);
			operators["ST"] = new OperatorDesc(151, OperatorFlags.Operator | OperatorFlags.InstructionList);
			operators["STN"] = new OperatorDesc(152, OperatorFlags.Operator | OperatorFlags.InstructionList);
			operators["MOVE"] = new OperatorDesc(153, (OperatorFlags)4294901762U);
			operators["TEST_AND_SET"] = new OperatorDesc(154, OperatorFlags.Operator | OperatorFlags.StructuredText | OperatorFlags.InstructionList | OperatorFlags.FunctionBlockDiagram);
			operators["R"] = new OperatorDesc(155, OperatorFlags.Operator | OperatorFlags.InstructionList);
			operators["S"] = new OperatorDesc(156, OperatorFlags.Operator | OperatorFlags.InstructionList);
		}

		// Token: 0x060001BB RID: 443 RVA: 0x00009794 File Offset: 0x00007994
		private void AddVectorOperatos(IDictionary<string, OperatorDesc> operators)
		{
			operators["__VCLOAD_REAL"] = new OperatorDesc(268, (OperatorFlags)4294901762U);
			operators["__VCLOAD_LREAL"] = new OperatorDesc(269, (OperatorFlags)4294901762U);
			operators["__VCSTORE"] = new OperatorDesc(270, (OperatorFlags)4294901762U);
			operators["__VCSET_REAL"] = new OperatorDesc(266, (OperatorFlags)4294901762U);
			operators["__VCSET_LREAL"] = new OperatorDesc(267, (OperatorFlags)4294901762U);
			operators["__VCADD"] = new OperatorDesc(258, (OperatorFlags)4294901762U);
			operators["__VCSUB"] = new OperatorDesc(259, (OperatorFlags)4294901762U);
			operators["__VCMUL"] = new OperatorDesc(260, (OperatorFlags)4294901762U);
			operators["__VCDIV"] = new OperatorDesc(261, (OperatorFlags)4294901762U);
			operators["__VCDOT"] = new OperatorDesc(262, (OperatorFlags)4294901762U);
			operators["__VCSQRT"] = new OperatorDesc(263, (OperatorFlags)4294901762U);
			operators["__VCMAX"] = new OperatorDesc(265, (OperatorFlags)4294901762U);
			operators["__VCMIN"] = new OperatorDesc(264, (OperatorFlags)4294901762U);
		}

		// Token: 0x060001BC RID: 444 RVA: 0x000098F4 File Offset: 0x00007AF4
		private void AddKeywords(IDictionary<string, OperatorDesc> operators)
		{
			operators["ACTION"] = new OperatorDesc(60, OperatorFlags.Keyword | OperatorFlags.Declaration);
			operators["ARRAY"] = new OperatorDesc(61, OperatorFlags.Keyword | OperatorFlags.Declaration);
			operators["__VECTOR"] = new OperatorDesc(257, OperatorFlags.Keyword | OperatorFlags.Declaration);
			operators["AT"] = new OperatorDesc(63, OperatorFlags.Keyword | OperatorFlags.Declaration);
			operators["BY"] = new OperatorDesc(64, OperatorFlags.Keyword | OperatorFlags.StructuredText);
			operators["CASE"] = new OperatorDesc(65, OperatorFlags.Keyword | OperatorFlags.StructuredText);
			operators["CONSTANT"] = new OperatorDesc(66, OperatorFlags.Keyword | OperatorFlags.Declaration);
			operators["DO"] = new OperatorDesc(67, OperatorFlags.Keyword | OperatorFlags.StructuredText);
			operators["ELSE"] = new OperatorDesc(68, OperatorFlags.Keyword | OperatorFlags.StructuredText);
			operators["ELSIF"] = new OperatorDesc(69, OperatorFlags.Keyword | OperatorFlags.StructuredText);
			operators["END_ACTION"] = new OperatorDesc(70, OperatorFlags.Keyword | OperatorFlags.Internal);
			operators["END_CASE"] = new OperatorDesc(71, OperatorFlags.Keyword | OperatorFlags.StructuredText);
			operators["END_FOR"] = new OperatorDesc(72, OperatorFlags.Keyword | OperatorFlags.StructuredText);
			operators["END_FUNCTION"] = new OperatorDesc(73, OperatorFlags.Keyword | OperatorFlags.Internal);
			operators["END_FUNCTION_BLOCK"] = new OperatorDesc(74, OperatorFlags.Keyword | OperatorFlags.Internal);
			operators["END_IF"] = new OperatorDesc(75, OperatorFlags.Keyword | OperatorFlags.StructuredText);
			operators["END_PROGRAM"] = new OperatorDesc(76, OperatorFlags.Keyword | OperatorFlags.Internal);
			operators["END_REPEAT"] = new OperatorDesc(77, OperatorFlags.Keyword | OperatorFlags.StructuredText);
			operators["END_STRUCT"] = new OperatorDesc(78, OperatorFlags.Keyword | OperatorFlags.Declaration);
			operators["END_UNION"] = new OperatorDesc(79, OperatorFlags.Keyword | OperatorFlags.Declaration);
			operators["END_TRANSITION"] = new OperatorDesc(291, OperatorFlags.Keyword | OperatorFlags.Internal);
			operators["END_TYPE"] = new OperatorDesc(80, OperatorFlags.Keyword | OperatorFlags.Declaration);
			operators["END_VAR"] = new OperatorDesc(81, OperatorFlags.Keyword | OperatorFlags.Declaration);
			operators["END_WHILE"] = new OperatorDesc(82, OperatorFlags.Keyword | OperatorFlags.StructuredText);
			operators["EXIT"] = new OperatorDesc(83, OperatorFlags.Keyword | OperatorFlags.StructuredText);
			operators["CONTINUE"] = new OperatorDesc(84, OperatorFlags.Keyword | OperatorFlags.StructuredText);
			operators["FOR"] = new OperatorDesc(85, OperatorFlags.Keyword | OperatorFlags.StructuredText);
			operators["FROM"] = new OperatorDesc(86, OperatorFlags.Keyword | OperatorFlags.Internal);
			operators["FUNCTION"] = new OperatorDesc(87, OperatorFlags.Keyword | OperatorFlags.Declaration);
			operators["FUNCTION_BLOCK"] = new OperatorDesc(88, OperatorFlags.Keyword | OperatorFlags.Declaration);
			operators["IF"] = new OperatorDesc(89, OperatorFlags.Keyword | OperatorFlags.StructuredText);
			operators["OF"] = new OperatorDesc(90, OperatorFlags.Keyword | OperatorFlags.Declaration);
			operators["PARAMS"] = new OperatorDesc(62, OperatorFlags.Keyword | OperatorFlags.Declaration);
			operators["PERSISTENT"] = new OperatorDesc(91, OperatorFlags.Keyword | OperatorFlags.Declaration);
			operators["POINTER"] = new OperatorDesc(92, OperatorFlags.Keyword | OperatorFlags.Declaration);
			operators["REFERENCE"] = new OperatorDesc(186, OperatorFlags.Keyword | OperatorFlags.Declaration);
			operators["PROGRAM"] = new OperatorDesc(93, OperatorFlags.Keyword | OperatorFlags.Declaration);
			operators["READ_ONLY"] = new OperatorDesc(94, OperatorFlags.Keyword | OperatorFlags.Internal);
			operators["READ_WRITE"] = new OperatorDesc(95, OperatorFlags.Keyword | OperatorFlags.Internal);
			operators["REPEAT"] = new OperatorDesc(96, OperatorFlags.Keyword | OperatorFlags.StructuredText);
			operators["RETAIN"] = new OperatorDesc(97, OperatorFlags.Keyword | OperatorFlags.Declaration);
			operators["RETURN"] = new OperatorDesc(98, OperatorFlags.Keyword | OperatorFlags.StructuredText);
			operators["STRUCT"] = new OperatorDesc(99, OperatorFlags.Keyword | OperatorFlags.Declaration);
			operators["UNION"] = new OperatorDesc(100, OperatorFlags.Keyword | OperatorFlags.Declaration);
			operators["THEN"] = new OperatorDesc(101, OperatorFlags.Keyword | OperatorFlags.StructuredText);
			operators["TO"] = new OperatorDesc(102, OperatorFlags.Keyword | OperatorFlags.StructuredText | OperatorFlags.Declaration);
			operators["TRANSITION"] = new OperatorDesc(290, OperatorFlags.Keyword | OperatorFlags.Contextual | OperatorFlags.Internal);
			operators["TYPE"] = new OperatorDesc(103, OperatorFlags.Keyword | OperatorFlags.Declaration);
			operators["UNTIL"] = new OperatorDesc(104, OperatorFlags.Keyword | OperatorFlags.StructuredText);
			operators["VAR"] = new OperatorDesc(105, OperatorFlags.Keyword | OperatorFlags.Declaration);
			operators["VAR_ACCESS"] = new OperatorDesc(106, OperatorFlags.Keyword | OperatorFlags.Declaration);
			operators["VAR_CONFIG"] = new OperatorDesc(107, OperatorFlags.Keyword | OperatorFlags.Declaration);
			operators["VAR_EXTERNAL"] = new OperatorDesc(108, OperatorFlags.Keyword | OperatorFlags.Declaration);
			operators["VAR_GLOBAL"] = new OperatorDesc(109, OperatorFlags.Keyword | OperatorFlags.Declaration);
			operators["VAR_INPUT"] = new OperatorDesc(110, OperatorFlags.Keyword | OperatorFlags.Declaration);
			operators["VAR_IN_OUT"] = new OperatorDesc(111, OperatorFlags.Keyword | OperatorFlags.Declaration);
			operators["VAR_OUTPUT"] = new OperatorDesc(112, OperatorFlags.Keyword | OperatorFlags.Declaration);
			operators["VAR_TEMP"] = new OperatorDesc(113, OperatorFlags.Keyword | OperatorFlags.Declaration);
			operators["VAR_STAT"] = new OperatorDesc(114, OperatorFlags.Keyword | OperatorFlags.Declaration);
			operators["WHILE"] = new OperatorDesc(115, OperatorFlags.Keyword | OperatorFlags.StructuredText);
			operators["EXTENDS"] = new OperatorDesc(116, OperatorFlags.Keyword | OperatorFlags.Declaration);
			operators["IMPLEMENTS"] = new OperatorDesc(117, OperatorFlags.Keyword | OperatorFlags.Declaration);
			operators["METHOD"] = new OperatorDesc(118, OperatorFlags.Keyword | OperatorFlags.Declaration);
			operators["END_METHOD"] = new OperatorDesc(281, OperatorFlags.Keyword | OperatorFlags.Internal);
			operators["PROPERTY"] = new OperatorDesc(187, OperatorFlags.Keyword | OperatorFlags.Declaration);
			operators["END_PROPERTY"] = new OperatorDesc(282, OperatorFlags.Keyword | OperatorFlags.Internal);
			operators["PROPERTY_SET"] = new OperatorDesc(284, OperatorFlags.Keyword | OperatorFlags.Internal);
			operators["PROPERTY_GET"] = new OperatorDesc(285, OperatorFlags.Keyword | OperatorFlags.Internal);
			operators["INTERFACE"] = new OperatorDesc(119, OperatorFlags.Keyword | OperatorFlags.Declaration);
			operators["END_INTERFACE"] = new OperatorDesc(283, OperatorFlags.Keyword | OperatorFlags.Internal);
			operators["THIS"] = new OperatorDesc(120, (OperatorFlags)4294901764U);
			operators["SUPER"] = new OperatorDesc(121, OperatorFlags.Keyword | OperatorFlags.StructuredText);
			operators["VAR_INST"] = new OperatorDesc(244, OperatorFlags.Keyword | OperatorFlags.Declaration);
			operators["VAR_GENERIC"] = new OperatorDesc(280, OperatorFlags.Keyword | OperatorFlags.Declaration);
			operators["__BEGIN_IMPLEMENTATION"] = new OperatorDesc(286, OperatorFlags.Keyword | OperatorFlags.StructuredText | OperatorFlags.Internal);
			operators["NAMESPACE"] = new OperatorDesc(287, OperatorFlags.Keyword | OperatorFlags.Contextual | OperatorFlags.StructuredText | OperatorFlags.Internal);
			operators["END_NAMESPACE"] = new OperatorDesc(288, OperatorFlags.Keyword | OperatorFlags.StructuredText | OperatorFlags.Internal);
		}

		// Token: 0x060001BD RID: 445 RVA: 0x0000A01C File Offset: 0x0000821C
		private void AddStandardOperators(IDictionary<string, OperatorDesc> operators)
		{
			operators["LOWER_BOUND"] = new OperatorDesc(248, (OperatorFlags)4294901762U);
			operators["UPPER_BOUND"] = new OperatorDesc(249, (OperatorFlags)4294901762U);
			operators["ADR"] = new OperatorDesc(33, (OperatorFlags)4294901762U);
			operators["BITADR"] = new OperatorDesc(34, (OperatorFlags)4294901762U);
			operators["INDEXOF"] = new OperatorDesc(35, OperatorFlags.Operator | OperatorFlags.Internal);
			operators["SIZEOF"] = new OperatorDesc(36, (OperatorFlags)4294901762U);
			operators["XSIZEOF"] = new OperatorDesc(277, (OperatorFlags)4294901762U);
			operators["INI"] = new OperatorDesc(37, OperatorFlags.Operator | OperatorFlags.Internal);
			operators["ABS"] = new OperatorDesc(38, (OperatorFlags)4294901762U);
			operators["LIMIT"] = new OperatorDesc(39, (OperatorFlags)4294901762U);
			operators["MIN"] = new OperatorDesc(40, (OperatorFlags)4294901762U);
			operators["MAX"] = new OperatorDesc(41, (OperatorFlags)4294901762U);
			operators["TRUNC"] = new OperatorDesc(42, (OperatorFlags)4294901762U);
			operators["TRUNC_INT"] = new OperatorDesc(188, (OperatorFlags)4294901762U);
			operators["MUX"] = new OperatorDesc(43, (OperatorFlags)4294901762U);
			operators["SEL"] = new OperatorDesc(44, (OperatorFlags)4294901762U);
			operators["ROL"] = new OperatorDesc(45, (OperatorFlags)4294901762U);
			operators["ROR"] = new OperatorDesc(46, (OperatorFlags)4294901762U);
			operators["SHL"] = new OperatorDesc(47, (OperatorFlags)4294901762U);
			operators["SHR"] = new OperatorDesc(48, (OperatorFlags)4294901762U);
			operators["EXP"] = new OperatorDesc(49, (OperatorFlags)4294901762U);
			operators["EXPT"] = new OperatorDesc(50, (OperatorFlags)4294901762U);
			operators["SQRT"] = new OperatorDesc(51, (OperatorFlags)4294901762U);
			operators["LN"] = new OperatorDesc(52, (OperatorFlags)4294901762U);
			operators["LOG"] = new OperatorDesc(53, (OperatorFlags)4294901762U);
			operators["SIN"] = new OperatorDesc(54, (OperatorFlags)4294901762U);
			operators["COS"] = new OperatorDesc(55, (OperatorFlags)4294901762U);
			operators["TAN"] = new OperatorDesc(56, (OperatorFlags)4294901762U);
			operators["ASIN"] = new OperatorDesc(57, (OperatorFlags)4294901762U);
			operators["ACOS"] = new OperatorDesc(58, (OperatorFlags)4294901762U);
			operators["ATAN"] = new OperatorDesc(59, (OperatorFlags)4294901762U);
			operators["ADD"] = new OperatorDesc(122, OperatorFlags.Operator | OperatorFlags.InstructionList | OperatorFlags.FunctionBlockDiagram);
			operators["SUB"] = new OperatorDesc(123, OperatorFlags.Operator | OperatorFlags.InstructionList | OperatorFlags.FunctionBlockDiagram);
			operators["MUL"] = new OperatorDesc(124, OperatorFlags.Operator | OperatorFlags.InstructionList | OperatorFlags.FunctionBlockDiagram);
			operators["DIV"] = new OperatorDesc(125, OperatorFlags.Operator | OperatorFlags.InstructionList | OperatorFlags.FunctionBlockDiagram);
			operators["MOD"] = new OperatorDesc(126, OperatorFlags.Operator | OperatorFlags.StructuredText | OperatorFlags.InstructionList | OperatorFlags.FunctionBlockDiagram);
			operators["AND"] = new OperatorDesc(127, OperatorFlags.Operator | OperatorFlags.StructuredText | OperatorFlags.InstructionList | OperatorFlags.FunctionBlockDiagram);
			operators["ANDN"] = new OperatorDesc(128, OperatorFlags.Operator | OperatorFlags.InstructionList);
			operators["OR"] = new OperatorDesc(129, OperatorFlags.Operator | OperatorFlags.StructuredText | OperatorFlags.InstructionList | OperatorFlags.FunctionBlockDiagram);
			operators["ORN"] = new OperatorDesc(130, OperatorFlags.Operator | OperatorFlags.InstructionList);
		}

		// Token: 0x060001BE RID: 446 RVA: 0x0000A3D8 File Offset: 0x000085D8
		private void AddOOKeywords(IDictionary<string, OperatorDesc> operators)
		{
			operators["ABSTRACT"] = new OperatorDesc(224, OperatorFlags.Keyword | OperatorFlags.Declaration);
			operators["PUBLIC"] = new OperatorDesc(226, OperatorFlags.Keyword | OperatorFlags.Declaration);
			operators["PRIVATE"] = new OperatorDesc(227, OperatorFlags.Keyword | OperatorFlags.Declaration);
			operators["PROTECTED"] = new OperatorDesc(228, OperatorFlags.Keyword | OperatorFlags.Declaration);
			operators["INTERNAL"] = new OperatorDesc(229, OperatorFlags.Keyword | OperatorFlags.Declaration);
			operators["FINAL"] = new OperatorDesc(230, OperatorFlags.Keyword | OperatorFlags.Declaration);
			operators["OVERRIDE"] = new OperatorDesc(225, OperatorFlags.Keyword | OperatorFlags.Contextual | OperatorFlags.Declaration);
			operators["OVERLOAD"] = new OperatorDesc(289, OperatorFlags.Keyword | OperatorFlags.Contextual | OperatorFlags.Declaration);
		}

		// Token: 0x060001BF RID: 447 RVA: 0x0000A4B8 File Offset: 0x000086B8
		private void AddDataTypeNames(IDictionary<string, OperatorDesc> operators)
		{
			operators["ANY"] = new OperatorDesc(4, OperatorFlags.DataType | OperatorFlags.Internal);
			operators["ANY_BIT"] = new OperatorDesc(5, OperatorFlags.DataType | OperatorFlags.Internal);
			operators["ANY_DATE"] = new OperatorDesc(6, OperatorFlags.DataType | OperatorFlags.Internal);
			operators["ANY_INT"] = new OperatorDesc(7, OperatorFlags.DataType | OperatorFlags.Internal);
			operators["ANY_NUM"] = new OperatorDesc(8, OperatorFlags.DataType | OperatorFlags.Internal);
			operators["ANY_REAL"] = new OperatorDesc(9, OperatorFlags.DataType | OperatorFlags.Internal);
			operators["ANY_STRING"] = new OperatorDesc(250, OperatorFlags.DataType | OperatorFlags.Internal);
			operators["SAFEBOOL"] = new OperatorDesc(206, (OperatorFlags)4294901776U);
			operators["SAFEBYTE"] = new OperatorDesc(207, (OperatorFlags)4294901776U);
			operators["SAFESINT"] = new OperatorDesc(209, (OperatorFlags)4294901776U);
			operators["SAFEUSINT"] = new OperatorDesc(208, (OperatorFlags)4294901776U);
			operators["SAFEWORD"] = new OperatorDesc(210, (OperatorFlags)4294901776U);
			operators["SAFEINT"] = new OperatorDesc(212, (OperatorFlags)4294901776U);
			operators["SAFEUINT"] = new OperatorDesc(211, (OperatorFlags)4294901776U);
			operators["SAFEDWORD"] = new OperatorDesc(213, (OperatorFlags)4294901776U);
			operators["SAFEDINT"] = new OperatorDesc(216, (OperatorFlags)4294901776U);
			operators["SAFEUDINT"] = new OperatorDesc(214, (OperatorFlags)4294901776U);
			operators["SAFELWORD"] = new OperatorDesc(217, (OperatorFlags)4294901776U);
			operators["SAFELINT"] = new OperatorDesc(219, (OperatorFlags)4294901776U);
			operators["SAFEULINT"] = new OperatorDesc(218, (OperatorFlags)4294901776U);
			operators["SAFETIME"] = new OperatorDesc(215, (OperatorFlags)4294901776U);
			operators["SAFEREAL"] = new OperatorDesc(272, (OperatorFlags)4294901776U);
			operators["SAFELREAL"] = new OperatorDesc(273, (OperatorFlags)4294901776U);
			operators["BOOL"] = new OperatorDesc(11, (OperatorFlags)4294901761U);
			operators["BIT"] = new OperatorDesc(10, (OperatorFlags)4294901761U);
			operators["BYTE"] = new OperatorDesc(12, (OperatorFlags)4294901769U);
			operators["WORD"] = new OperatorDesc(13, (OperatorFlags)4294901769U);
			operators["DWORD"] = new OperatorDesc(14, (OperatorFlags)4294901769U);
			operators["LWORD"] = new OperatorDesc(15, (OperatorFlags)4294901769U);
			operators["SINT"] = new OperatorDesc(16, (OperatorFlags)4294901769U);
			operators["INT"] = new OperatorDesc(17, (OperatorFlags)4294901769U);
			operators["DINT"] = new OperatorDesc(18, (OperatorFlags)4294901769U);
			operators["LINT"] = new OperatorDesc(19, (OperatorFlags)4294901769U);
			operators["USINT"] = new OperatorDesc(20, (OperatorFlags)4294901769U);
			operators["UINT"] = new OperatorDesc(21, (OperatorFlags)4294901769U);
			operators["UDINT"] = new OperatorDesc(22, (OperatorFlags)4294901769U);
			operators["ULINT"] = new OperatorDesc(23, (OperatorFlags)4294901769U);
			operators["__XWORD"] = new OperatorDesc(231, (OperatorFlags)4294901769U);
			operators["__UXINT"] = new OperatorDesc(232, (OperatorFlags)4294901769U);
			operators["__XINT"] = new OperatorDesc(237, (OperatorFlags)4294901769U);
			operators["REAL"] = new OperatorDesc(24, (OperatorFlags)4294901769U);
			operators["LREAL"] = new OperatorDesc(25, (OperatorFlags)4294901769U);
			operators["WSTRING"] = new OperatorDesc(27, (OperatorFlags)4294901761U);
			operators["STRING"] = new OperatorDesc(26, (OperatorFlags)4294901761U);
			operators["__XSTRING"] = new OperatorDesc(243, OperatorFlags.DataType | OperatorFlags.Internal);
			operators["TIME"] = new OperatorDesc(28, (OperatorFlags)4294901761U);
			operators["LTIME"] = new OperatorDesc(29, (OperatorFlags)4294901761U);
			operators["DATE"] = new OperatorDesc(30, (OperatorFlags)4294901761U);
			operators["LDATE"] = new OperatorDesc(274, (OperatorFlags)4294901761U);
			operators["DATE_AND_TIME"] = new OperatorDesc(31, (OperatorFlags)4294901761U);
			operators["LDATE_AND_TIME"] = new OperatorDesc(275, (OperatorFlags)4294901761U);
			operators["DT"] = new OperatorDesc(31, (OperatorFlags)4294901761U);
			operators["LDT"] = new OperatorDesc(275, (OperatorFlags)4294901761U);
			operators["TIME_OF_DAY"] = new OperatorDesc(32, (OperatorFlags)4294901761U);
			operators["LTIME_OF_DAY"] = new OperatorDesc(276, (OperatorFlags)4294901761U);
			operators["TOD"] = new OperatorDesc(32, (OperatorFlags)4294901761U);
			operators["LTOD"] = new OperatorDesc(276, (OperatorFlags)4294901761U);
		}

		// Token: 0x060001C0 RID: 448 RVA: 0x0000AA30 File Offset: 0x00008C30
		private void AddSpecialOperators(IDictionary<string, OperatorDesc> operators)
		{
			operators["__COPY"] = new OperatorDesc(2, OperatorFlags.Operator | OperatorFlags.Internal);
			operators["__RELOC"] = new OperatorDesc(1, OperatorFlags.Operator | OperatorFlags.Internal);
			operators["__LAZY"] = new OperatorDesc(3, OperatorFlags.DataType | OperatorFlags.Internal);
			operators["__CRC"] = new OperatorDesc(194, OperatorFlags.Operator | OperatorFlags.Internal);
			operators["__MAXOFFSET"] = new OperatorDesc(195, OperatorFlags.Operator | OperatorFlags.Internal);
			operators["__LOCALOFFSET"] = new OperatorDesc(190, OperatorFlags.Operator | OperatorFlags.Internal);
			operators["__TYPEOF"] = new OperatorDesc(193, OperatorFlags.Operator | OperatorFlags.Internal);
			operators["__VARINFO"] = new OperatorDesc(191, OperatorFlags.Operator | OperatorFlags.Internal);
			operators["__SYSTEM"] = new OperatorDesc(192, (OperatorFlags)4294901762U);
			operators["__POOL"] = new OperatorDesc(251, (OperatorFlags)4294901762U);
			operators["__INIT"] = new OperatorDesc(196, OperatorFlags.Operator | OperatorFlags.Internal);
			operators["__CAST"] = new OperatorDesc(202, OperatorFlags.Operator | OperatorFlags.Internal);
			operators["__ISVALIDREF"] = new OperatorDesc(197, (OperatorFlags)4294901762U);
			operators["__FCALL"] = new OperatorDesc(221, OperatorFlags.Operator | OperatorFlags.Internal);
			operators["__PROPERTYINFO"] = new OperatorDesc(222, OperatorFlags.Operator | OperatorFlags.Internal);
			operators["__ADRINST"] = new OperatorDesc(203, OperatorFlags.Operator | OperatorFlags.Internal);
			operators["__REFADR"] = new OperatorDesc(204, OperatorFlags.Operator | OperatorFlags.Internal);
			operators["__MEMORYSET"] = new OperatorDesc(233, OperatorFlags.Operator | OperatorFlags.Internal);
			operators["__GETLTICK"] = new OperatorDesc(236, OperatorFlags.Operator | OperatorFlags.Internal);
			operators["__QUERYINTERFACE"] = new OperatorDesc(198, (OperatorFlags)4294901762U);
			operators["__QUERYPOINTER"] = new OperatorDesc(199, (OperatorFlags)4294901762U);
			operators["__NEW"] = new OperatorDesc(200, (OperatorFlags)4294901762U);
			operators["__DELETE"] = new OperatorDesc(201, (OperatorFlags)4294901762U);
			operators["__WAIT"] = new OperatorDesc(205, OperatorFlags.Operator | OperatorFlags.Internal);
			operators["__TRY"] = new OperatorDesc(238, OperatorFlags.Keyword | OperatorFlags.StructuredText);
			operators["__ENDTRY"] = new OperatorDesc(239, OperatorFlags.Keyword | OperatorFlags.StructuredText);
			operators["__CATCH"] = new OperatorDesc(240, OperatorFlags.Keyword | OperatorFlags.StructuredText);
			operators["__FINALLY"] = new OperatorDesc(241, OperatorFlags.Keyword | OperatorFlags.StructuredText);
			operators["__THROW"] = new OperatorDesc(242, OperatorFlags.Operator | OperatorFlags.StructuredText | OperatorFlags.Internal);
			operators["__BITOFFSET"] = new OperatorDesc(220, OperatorFlags.Operator | OperatorFlags.Internal);
			operators["__XADD"] = new OperatorDesc(253, (OperatorFlags)4294901762U);
			operators["__COMPARE_AND_SWAP"] = new OperatorDesc(256, (OperatorFlags)4294901762U);
			operators["__MEMORYBARRIER"] = new OperatorDesc(254, (OperatorFlags)4294901762U);
			operators["__CURRENTTASK"] = new OperatorDesc(255, (OperatorFlags)4294901762U);
			operators["__CHECKLICENSE"] = new OperatorDesc(245, (OperatorFlags)4294901762U);
			operators["__CHECKLICENSEBIT"] = new OperatorDesc(252, (OperatorFlags)4294901762U);
			operators["__CALLINITFUNCTION"] = new OperatorDesc(246, OperatorFlags.Operator | OperatorFlags.Internal);
			operators["__LATECOMPILEDEXPR"] = new OperatorDesc(247, OperatorFlags.Operator | OperatorFlags.Internal);
			operators["__POUNAME"] = new OperatorDesc(278, (OperatorFlags)4294901762U);
			operators["__POSITION"] = new OperatorDesc(279, (OperatorFlags)4294901762U);
			operators["AND_THEN"] = new OperatorDesc(234, OperatorFlags.Operator | OperatorFlags.StructuredText | OperatorFlags.InstructionList | OperatorFlags.FunctionBlockDiagram);
			operators["OR_ELSE"] = new OperatorDesc(235, OperatorFlags.Operator | OperatorFlags.StructuredText | OperatorFlags.InstructionList | OperatorFlags.FunctionBlockDiagram);
		}

		// Token: 0x060001C1 RID: 449 RVA: 0x0000AE78 File Offset: 0x00009078
		private void AddConversionOperators(Dictionary<string, OperatorDesc> operators)
		{
			ArrayList arrayList = new ArrayList();
			List<OperatorDesc> list = new List<OperatorDesc>();
			foreach (string key in operators.Keys)
			{
				OperatorDesc operatorDesc = operators[key];
				if ((operatorDesc.Flags & OperatorFlags.DataType) != (OperatorFlags)0U)
				{
					arrayList.Add(operatorDesc.Operator);
				}
				if ((operatorDesc.Flags & OperatorFlags.SafetyDataType) != (OperatorFlags)0U)
				{
					arrayList.Add(operatorDesc.Operator);
				}
				if ((operatorDesc.Flags & OperatorFlags.DataType) != (OperatorFlags)0U && (operatorDesc.Flags & OperatorFlags.NumericDataType) != (OperatorFlags)0U)
				{
					list.Add(operatorDesc);
				}
			}
			this.FillOperatorTable(operators);
			this.AddOverloadedConversions(operators, arrayList);
			string textOfOperator = this.GetTextOfOperator(186);
			string textOfOperator2 = this.GetTextOfOperator(92);
			string key2 = textOfOperator + "_TO_" + textOfOperator2;
			operators[key2] = new OperatorDesc(184, (OperatorFlags)4294901762U);
			foreach (OperatorDesc operatorDesc2 in list)
			{
				string textOfOperator3 = this.GetTextOfOperator(operatorDesc2.Operator, true);
				string key3 = "ANY_NUM_TO_" + textOfOperator3;
				operators[key3] = new OperatorDesc(184, (OperatorFlags)4294901762U);
			}
			foreach (object obj in arrayList)
			{
				Operator op = (Operator)obj;
				string textOfOperator4 = this.GetTextOfOperator(op, true);
				string key4 = "ANY_TO_" + textOfOperator4;
				operators[key4] = new OperatorDesc(184, (OperatorFlags)4294901762U);
			}
			this.Insert(operators);
		}

		// Token: 0x060001C2 RID: 450 RVA: 0x0000B064 File Offset: 0x00009264
		private void AddOverloadedConversions(Dictionary<string, OperatorDesc> operators, ArrayList dataTypes)
		{
			for (int i = 0; i < dataTypes.Count; i++)
			{
				string textOfOperator = this.GetTextOfOperator((Operator)dataTypes[i], true);
				string key = "TO_" + textOfOperator;
				operators[key] = new OperatorDesc(184, (OperatorFlags)4294901762U);
				for (int j = 0; j < dataTypes.Count; j++)
				{
					if (i != j)
					{
						string textOfOperator2 = this.GetTextOfOperator((Operator)dataTypes[j], true);
						key = textOfOperator + "_TO_" + textOfOperator2;
						operators[key] = new OperatorDesc(184, (OperatorFlags)4294901762U);
					}
				}
			}
		}

		// Token: 0x04000048 RID: 72
		private OperatorNode _root;

		// Token: 0x04000049 RID: 73
		private readonly List<OperatorDesc> _all_LockRequired = new List<OperatorDesc>();

		// Token: 0x0400004A RID: 74
		private readonly Dictionary<Operator, string> _textOfOperatorLong = new Dictionary<Operator, string>();

		// Token: 0x0400004B RID: 75
		private readonly Dictionary<Operator, string> _textOfOperatorShort = new Dictionary<Operator, string>();

		// Token: 0x0400004C RID: 76
		private readonly LHashSet<Operator> _contextualOperators = new LHashSet<Operator>();
	}
}
