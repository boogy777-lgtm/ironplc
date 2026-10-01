using System.Collections;
using System.Collections.Generic;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Utilities;
using CODESYS.Parser35210.Tools;

namespace CODESYS.Parser35210.Scanner
{
	internal class OperatorTable
	{
		private OperatorNode _root;

		private readonly List<OperatorDesc> _all_LockRequired = new List<OperatorDesc>();

		private readonly Dictionary<Operator, string> _textOfOperatorLong = new Dictionary<Operator, string>();

		private readonly Dictionary<Operator, string> _textOfOperatorShort = new Dictionary<Operator, string>();

		private readonly LHashSet<Operator> _contextualOperators = new LHashSet<Operator>();

		internal static OperatorTable Instance { get; } = new OperatorTable();


		public Operator this[string st, bool bIgnoreCase]
		{
			get
			{
				OperatorNode operatorNode = _root;
				int num = 0;
				int length = st.Length;
				while (operatorNode != null && num < length)
				{
					int num2 = Compare(st[num], operatorNode.Char, bIgnoreCase);
					if (num2 < 0)
					{
						operatorNode = operatorNode.Less;
						continue;
					}
					if (num2 > 0)
					{
						operatorNode = operatorNode.Greater;
						continue;
					}
					if (num == length - 1 && operatorNode.OperatorDesc != null)
					{
						return operatorNode.OperatorDesc.Operator;
					}
					operatorNode = operatorNode.Equal;
					num++;
				}
				return Operator.None;
			}
		}

		public Operator this[QuickStringBuilder qsb, bool bIgnoreCase]
		{
			get
			{
				OperatorNode operatorNode = _root;
				int num = 0;
				int length = qsb.Length;
				while (operatorNode != null && num < length)
				{
					int num2 = Compare(qsb[num], operatorNode.Char, bIgnoreCase);
					if (num2 < 0)
					{
						operatorNode = operatorNode.Less;
						continue;
					}
					if (num2 > 0)
					{
						operatorNode = operatorNode.Greater;
						continue;
					}
					if (num == length - 1 && operatorNode.OperatorDesc != null)
					{
						return operatorNode.OperatorDesc.Operator;
					}
					operatorNode = operatorNode.Equal;
					num++;
				}
				return Operator.None;
			}
		}

		public Operator this[char[] input, IToken token, bool bIgnoreCase]
		{
			get
			{
				OperatorNode operatorNode = _root;
				int num = 0;
				int length = token.Length;
				int sourceOffset = token.SourceOffset;
				while (operatorNode != null && num < length)
				{
					int num2 = Compare(input[sourceOffset + num], operatorNode.Char, bIgnoreCase);
					if (num2 < 0)
					{
						operatorNode = operatorNode.Less;
						continue;
					}
					if (num2 > 0)
					{
						operatorNode = operatorNode.Greater;
						continue;
					}
					if (num == length - 1 && operatorNode.OperatorDesc != null)
					{
						return operatorNode.OperatorDesc.Operator;
					}
					operatorNode = operatorNode.Equal;
					num++;
				}
				return Operator.None;
			}
		}

		private OperatorTable()
		{
			UpdateOperatorTable();
		}

		private int Compare(char c1, char c2, bool bIgnoreCase)
		{
			if (bIgnoreCase)
			{
				if (c1 >= 'a' && c1 <= 'z')
				{
					c1 = (char)(c1 + 65 - 97);
				}
				if (c2 >= 'a' && c2 <= 'z')
				{
					c2 = (char)(c2 + 65 - 97);
				}
			}
			return c1 - c2;
		}

		public Operator[] GetByFlags(OperatorFlags flags)
		{
			Hashtable hashtable = new Hashtable();
			lock (_all_LockRequired)
			{
				foreach (OperatorDesc item in _all_LockRequired)
				{
					if ((item.Flags & flags) == flags)
					{
						hashtable[item.Operator] = null;
					}
				}
			}
			Operator[] array = new Operator[hashtable.Count];
			hashtable.Keys.CopyTo(array, 0);
			return array;
		}

		public void Insert(IDictionary dictionary)
		{
			SortedList sortedList = new SortedList(dictionary);
			Insert(sortedList, 0, sortedList.Count - 1);
		}

		public void FillOperatorTable(IDictionary dictionary)
		{
			_textOfOperatorLong.Clear();
			_textOfOperatorShort.Clear();
			foreach (DictionaryEntry item in dictionary)
			{
				OperatorDesc operatorDesc = (OperatorDesc)item.Value;
				string value = item.Key as string;
				switch (operatorDesc.Operator)
				{
				case Operator.TimeOfDay:
					_textOfOperatorLong[operatorDesc.Operator] = "TIME_OF_DAY";
					_textOfOperatorShort[operatorDesc.Operator] = "TOD";
					continue;
				case Operator.LTimeOfDay:
					_textOfOperatorLong[operatorDesc.Operator] = "LTIME_OF_DAY";
					_textOfOperatorShort[operatorDesc.Operator] = "LTOD";
					continue;
				case Operator.DateAndTime:
					_textOfOperatorLong[operatorDesc.Operator] = "DATE_AND_TIME";
					_textOfOperatorShort[operatorDesc.Operator] = "DT";
					continue;
				case Operator.LDateAndTime:
					_textOfOperatorLong[operatorDesc.Operator] = "LDATE_AND_TIME";
					_textOfOperatorShort[operatorDesc.Operator] = "LDT";
					continue;
				case Operator.Conversion:
					continue;
				}
				Debug.Assert(!_textOfOperatorLong.ContainsKey(operatorDesc.Operator));
				Debug.Assert(!_textOfOperatorShort.ContainsKey(operatorDesc.Operator));
				_textOfOperatorLong[operatorDesc.Operator] = value;
				_textOfOperatorShort[operatorDesc.Operator] = value;
				if ((operatorDesc.Flags & OperatorFlags.Contextual) == OperatorFlags.Contextual)
				{
					_contextualOperators.Add(operatorDesc.Operator);
				}
			}
			_textOfOperatorShort[Operator.Class] = "CLASS";
			_textOfOperatorShort[Operator.Override] = "OVERRIDE";
			_textOfOperatorLong[Operator.Class] = "CLASS";
			_textOfOperatorLong[Operator.Override] = "OVERRIDE";
		}

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
				Insert(ref current.Less, st, opDesc);
				return;
			}
			if (st[0] > current.Char)
			{
				Insert(ref current.Greater, st, opDesc);
				return;
			}
			if (st.Length == 1)
			{
				current.OperatorDesc = opDesc;
				lock (_all_LockRequired)
				{
					_all_LockRequired.Add(opDesc);
					return;
				}
			}
			Insert(ref current.Equal, st.Substring(1), opDesc);
		}

		private void Insert(SortedList list, int nLeft, int nRight)
		{
			if (nLeft <= nRight)
			{
				int num = (nLeft + nRight) / 2;
				Insert(ref _root, (string)list.GetKey(num), (OperatorDesc)list.GetByIndex(num));
				Insert(list, nLeft, num - 1);
				Insert(list, num + 1, nRight);
			}
		}

		public string GetTextOfOperatorShort(Operator op)
		{
			if (!_textOfOperatorShort.ContainsKey(op))
			{
				return "ERROR";
			}
			return _textOfOperatorShort[op];
		}

		public string GetTextOfOperatorLong(Operator op)
		{
			if (!_textOfOperatorLong.ContainsKey(op))
			{
				return "ERROR";
			}
			return _textOfOperatorLong[op];
		}

		public bool IsContextualOperator(Operator op)
		{
			return _contextualOperators.Contains(op);
		}

		public string GetTextOfOperator(Operator op, bool bShort)
		{
			if (bShort)
			{
				return GetTextOfOperatorShort(op);
			}
			return GetTextOfOperatorLong(op);
		}

		public string GetTextOfOperator(Operator op)
		{
			return GetTextOfOperatorLong(op);
		}

		public Operator GetOperatorFromText(string stOperator)
		{
			return this[stOperator, false];
		}

		public Operator[] GetKeywords(IECLanguage language)
		{
			switch (language)
			{
			case IECLanguage.Declaration:
				return GetByFlags(OperatorFlags.Keyword | OperatorFlags.Declaration);
			case IECLanguage.FunctionBlockDiagram:
				return GetByFlags(OperatorFlags.Keyword | OperatorFlags.FunctionBlockDiagram);
			case IECLanguage.InstructionList:
				return GetByFlags(OperatorFlags.Keyword | OperatorFlags.InstructionList);
			case IECLanguage.StructuredText:
				return GetByFlags(OperatorFlags.Keyword | OperatorFlags.StructuredText);
			default:
				return new Operator[0];
			}
		}

		public Operator[] GetOperators(IECLanguage language)
		{
			switch (language)
			{
			case IECLanguage.Declaration:
				return GetByFlags(OperatorFlags.Operator | OperatorFlags.Declaration);
			case IECLanguage.FunctionBlockDiagram:
				return GetByFlags(OperatorFlags.Operator | OperatorFlags.FunctionBlockDiagram);
			case IECLanguage.InstructionList:
				return GetByFlags(OperatorFlags.Operator | OperatorFlags.InstructionList);
			case IECLanguage.StructuredText:
				return GetByFlags(OperatorFlags.Operator | OperatorFlags.StructuredText);
			default:
				return new Operator[0];
			}
		}

		private void UpdateOperatorTable()
		{
			Dictionary<string, OperatorDesc> operators = new Dictionary<string, OperatorDesc>();
			AddSpecialOperators(operators);
			AddDataTypeNames(operators);
			AddOOKeywords(operators);
			AddStandardOperators(operators);
			AddKeywords(operators);
			AddVectorOperatos(operators);
			AddILOperators(operators);
			AddOperatorSymbols(operators);
			AddConversionOperators(operators);
		}

		private void AddOperatorSymbols(IDictionary<string, OperatorDesc> operators)
		{
			operators["+"] = new OperatorDesc(Operator.Plus, OperatorFlags.Operator | OperatorFlags.StructuredText | OperatorFlags.Declaration);
			operators["-"] = new OperatorDesc(Operator.Minus, OperatorFlags.Operator | OperatorFlags.StructuredText | OperatorFlags.Declaration);
			operators["*"] = new OperatorDesc(Operator.Times, OperatorFlags.Operator | OperatorFlags.StructuredText | OperatorFlags.Declaration);
			operators["**"] = new OperatorDesc(Operator.Power, OperatorFlags.Operator | OperatorFlags.Declaration | OperatorFlags.Internal);
			operators["/"] = new OperatorDesc(Operator.Divide, OperatorFlags.Operator | OperatorFlags.StructuredText | OperatorFlags.Declaration);
			operators["."] = new OperatorDesc(Operator.Period, OperatorFlags.Operator | OperatorFlags.StructuredText | OperatorFlags.InstructionList | OperatorFlags.Declaration);
			operators["#"] = new OperatorDesc(Operator.Hash, OperatorFlags.Operator | OperatorFlags.StructuredText | OperatorFlags.InstructionList | OperatorFlags.Declaration);
			operators[":"] = new OperatorDesc(Operator.Colon, OperatorFlags.Operator | OperatorFlags.StructuredText | OperatorFlags.InstructionList | OperatorFlags.Declaration);
			operators[":="] = new OperatorDesc(Operator.Assign, OperatorFlags.Operator | OperatorFlags.StructuredText | OperatorFlags.InstructionList | OperatorFlags.Declaration);
			operators["=:"] = new OperatorDesc(Operator.FupAssign, OperatorFlags.Internal);
			operators["S="] = new OperatorDesc(Operator.SetAssign, OperatorFlags.Operator | OperatorFlags.StructuredText);
			operators["R="] = new OperatorDesc(Operator.ResetAssign, OperatorFlags.Operator | OperatorFlags.StructuredText);
			operators["REF="] = new OperatorDesc(Operator.RefAssign, OperatorFlags.Operator | OperatorFlags.StructuredText);
			operators["("] = new OperatorDesc(Operator.LeftParenthesis, OperatorFlags.Operator | OperatorFlags.StructuredText | OperatorFlags.InstructionList | OperatorFlags.Declaration);
			operators[")"] = new OperatorDesc(Operator.RightParenthesis, OperatorFlags.Operator | OperatorFlags.StructuredText | OperatorFlags.InstructionList | OperatorFlags.Declaration);
			operators["["] = new OperatorDesc(Operator.LeftBracket, OperatorFlags.Operator | OperatorFlags.StructuredText | OperatorFlags.InstructionList | OperatorFlags.Declaration);
			operators["]"] = new OperatorDesc(Operator.RightBracket, OperatorFlags.Operator | OperatorFlags.StructuredText | OperatorFlags.InstructionList | OperatorFlags.Declaration);
			operators[","] = new OperatorDesc(Operator.Comma, OperatorFlags.Operator | OperatorFlags.StructuredText | OperatorFlags.InstructionList | OperatorFlags.Declaration);
			operators[";"] = new OperatorDesc(Operator.Semicolon, OperatorFlags.Operator | OperatorFlags.StructuredText | OperatorFlags.Declaration);
			operators[".."] = new OperatorDesc(Operator.Range, OperatorFlags.Operator | OperatorFlags.StructuredText | OperatorFlags.Declaration);
			operators["=>"] = new OperatorDesc(Operator.AssignOut, OperatorFlags.Operator | OperatorFlags.StructuredText | OperatorFlags.InstructionList);
			operators["<"] = new OperatorDesc(Operator.Less, OperatorFlags.Operator | OperatorFlags.StructuredText);
			operators[">"] = new OperatorDesc(Operator.Greater, OperatorFlags.Operator | OperatorFlags.StructuredText);
			operators["<="] = new OperatorDesc(Operator.LessEqual, OperatorFlags.Operator | OperatorFlags.StructuredText);
			operators[">="] = new OperatorDesc(Operator.GreaterEqual, OperatorFlags.Operator | OperatorFlags.StructuredText);
			operators["="] = new OperatorDesc(Operator.Equal, OperatorFlags.Operator | OperatorFlags.StructuredText);
			operators["<>"] = new OperatorDesc(Operator.NotEqual, OperatorFlags.Operator | OperatorFlags.StructuredText);
			operators["&"] = new OperatorDesc(Operator.Ampersand, OperatorFlags.Operator | OperatorFlags.StructuredText);
			operators["|"] = new OperatorDesc(Operator.VerticalLine, OperatorFlags.Operator | OperatorFlags.StructuredText);
			operators["^"] = new OperatorDesc(Operator.DeRef, OperatorFlags.Operator | OperatorFlags.StructuredText | OperatorFlags.InstructionList | OperatorFlags.Declaration);
		}

		private void AddILOperators(IDictionary<string, OperatorDesc> operators)
		{
			operators["XOR"] = new OperatorDesc(Operator.Xor, OperatorFlags.Operator | OperatorFlags.StructuredText | OperatorFlags.InstructionList | OperatorFlags.FunctionBlockDiagram);
			operators["XORN"] = new OperatorDesc(Operator.XorN, OperatorFlags.Operator | OperatorFlags.InstructionList);
			operators["NOT"] = new OperatorDesc(Operator.Not, OperatorFlags.Operator | OperatorFlags.StructuredText | OperatorFlags.InstructionList | OperatorFlags.FunctionBlockDiagram);
			operators["EQ"] = new OperatorDesc(Operator.Eq, OperatorFlags.Operator | OperatorFlags.InstructionList | OperatorFlags.FunctionBlockDiagram);
			operators["NE"] = new OperatorDesc(Operator.Ne, OperatorFlags.Operator | OperatorFlags.InstructionList | OperatorFlags.FunctionBlockDiagram);
			operators["GE"] = new OperatorDesc(Operator.Ge, OperatorFlags.Operator | OperatorFlags.InstructionList | OperatorFlags.FunctionBlockDiagram);
			operators["GT"] = new OperatorDesc(Operator.Gt, OperatorFlags.Operator | OperatorFlags.InstructionList | OperatorFlags.FunctionBlockDiagram);
			operators["LE"] = new OperatorDesc(Operator.Le, OperatorFlags.Operator | OperatorFlags.InstructionList | OperatorFlags.FunctionBlockDiagram);
			operators["LT"] = new OperatorDesc(Operator.Lt, OperatorFlags.Operator | OperatorFlags.InstructionList | OperatorFlags.FunctionBlockDiagram);
			operators["CAL"] = new OperatorDesc(Operator.Cal, OperatorFlags.Operator | OperatorFlags.InstructionList);
			operators["CALC"] = new OperatorDesc(Operator.CalC, OperatorFlags.Operator | OperatorFlags.InstructionList);
			operators["CALCN"] = new OperatorDesc(Operator.CalCN, OperatorFlags.Operator | OperatorFlags.InstructionList);
			operators["JMP"] = new OperatorDesc(Operator.Jmp, OperatorFlags.Operator | OperatorFlags.InstructionList);
			operators["JMPC"] = new OperatorDesc(Operator.JmpC, OperatorFlags.Operator | OperatorFlags.InstructionList);
			operators["JMPCN"] = new OperatorDesc(Operator.JmpCN, OperatorFlags.Operator | OperatorFlags.InstructionList);
			operators["RET"] = new OperatorDesc(Operator.Ret, OperatorFlags.Operator | OperatorFlags.InstructionList);
			operators["RETC"] = new OperatorDesc(Operator.RetC, OperatorFlags.Operator | OperatorFlags.InstructionList);
			operators["RETCN"] = new OperatorDesc(Operator.RetCN, OperatorFlags.Operator | OperatorFlags.InstructionList);
			operators["LD"] = new OperatorDesc(Operator.Ld, OperatorFlags.Operator | OperatorFlags.InstructionList);
			operators["LDN"] = new OperatorDesc(Operator.LdN, OperatorFlags.Operator | OperatorFlags.InstructionList);
			operators["ST"] = new OperatorDesc(Operator.St, OperatorFlags.Operator | OperatorFlags.InstructionList);
			operators["STN"] = new OperatorDesc(Operator.StN, OperatorFlags.Operator | OperatorFlags.InstructionList);
			operators["MOVE"] = new OperatorDesc(Operator.Move, OperatorFlags.AllLanguages | OperatorFlags.Operator);
			operators["TEST_AND_SET"] = new OperatorDesc(Operator.TestAndSet, OperatorFlags.Operator | OperatorFlags.StructuredText | OperatorFlags.InstructionList | OperatorFlags.FunctionBlockDiagram);
			operators["R"] = new OperatorDesc(Operator.R, OperatorFlags.Operator | OperatorFlags.InstructionList);
			operators["S"] = new OperatorDesc(Operator.S, OperatorFlags.Operator | OperatorFlags.InstructionList);
		}

		private void AddVectorOperatos(IDictionary<string, OperatorDesc> operators)
		{
			operators["__VCLOAD_REAL"] = new OperatorDesc(Operator.__vcLoadReal, OperatorFlags.AllLanguages | OperatorFlags.Operator);
			operators["__VCLOAD_LREAL"] = new OperatorDesc(Operator.__vcLoadLReal, OperatorFlags.AllLanguages | OperatorFlags.Operator);
			operators["__VCSTORE"] = new OperatorDesc(Operator.__vcStore, OperatorFlags.AllLanguages | OperatorFlags.Operator);
			operators["__VCSET_REAL"] = new OperatorDesc(Operator.__vcSetReal, OperatorFlags.AllLanguages | OperatorFlags.Operator);
			operators["__VCSET_LREAL"] = new OperatorDesc(Operator.__vcSetLReal, OperatorFlags.AllLanguages | OperatorFlags.Operator);
			operators["__VCADD"] = new OperatorDesc(Operator.__vcAdd, OperatorFlags.AllLanguages | OperatorFlags.Operator);
			operators["__VCSUB"] = new OperatorDesc(Operator.__vcSub, OperatorFlags.AllLanguages | OperatorFlags.Operator);
			operators["__VCMUL"] = new OperatorDesc(Operator.__vcMul, OperatorFlags.AllLanguages | OperatorFlags.Operator);
			operators["__VCDIV"] = new OperatorDesc(Operator.__vcDiv, OperatorFlags.AllLanguages | OperatorFlags.Operator);
			operators["__VCDOT"] = new OperatorDesc(Operator.__vcDot, OperatorFlags.AllLanguages | OperatorFlags.Operator);
			operators["__VCSQRT"] = new OperatorDesc(Operator.__vcSqrt, OperatorFlags.AllLanguages | OperatorFlags.Operator);
			operators["__VCMAX"] = new OperatorDesc(Operator.__vcMax, OperatorFlags.AllLanguages | OperatorFlags.Operator);
			operators["__VCMIN"] = new OperatorDesc(Operator.__vcMin, OperatorFlags.AllLanguages | OperatorFlags.Operator);
		}

		private void AddKeywords(IDictionary<string, OperatorDesc> operators)
		{
			operators["ACTION"] = new OperatorDesc(Operator.Action, OperatorFlags.Keyword | OperatorFlags.Declaration);
			operators["ARRAY"] = new OperatorDesc(Operator.Array, OperatorFlags.Keyword | OperatorFlags.Declaration);
			operators["__VECTOR"] = new OperatorDesc(Operator.__Vector, OperatorFlags.Keyword | OperatorFlags.Declaration);
			operators["AT"] = new OperatorDesc(Operator.At, OperatorFlags.Keyword | OperatorFlags.Declaration);
			operators["BY"] = new OperatorDesc(Operator.By, OperatorFlags.Keyword | OperatorFlags.StructuredText);
			operators["CASE"] = new OperatorDesc(Operator.Case, OperatorFlags.Keyword | OperatorFlags.StructuredText);
			operators["CONSTANT"] = new OperatorDesc(Operator.Constant, OperatorFlags.Keyword | OperatorFlags.Declaration);
			operators["DO"] = new OperatorDesc(Operator.Do, OperatorFlags.Keyword | OperatorFlags.StructuredText);
			operators["ELSE"] = new OperatorDesc(Operator.Else, OperatorFlags.Keyword | OperatorFlags.StructuredText);
			operators["ELSIF"] = new OperatorDesc(Operator.Elsif, OperatorFlags.Keyword | OperatorFlags.StructuredText);
			operators["END_ACTION"] = new OperatorDesc(Operator.EndAction, OperatorFlags.Keyword | OperatorFlags.Internal);
			operators["END_CASE"] = new OperatorDesc(Operator.EndCase, OperatorFlags.Keyword | OperatorFlags.StructuredText);
			operators["END_FOR"] = new OperatorDesc(Operator.EndFor, OperatorFlags.Keyword | OperatorFlags.StructuredText);
			operators["END_FUNCTION"] = new OperatorDesc(Operator.EndFunction, OperatorFlags.Keyword | OperatorFlags.Internal);
			operators["END_FUNCTION_BLOCK"] = new OperatorDesc(Operator.EndFunctionBlock, OperatorFlags.Keyword | OperatorFlags.Internal);
			operators["END_IF"] = new OperatorDesc(Operator.EndIf, OperatorFlags.Keyword | OperatorFlags.StructuredText);
			operators["END_PROGRAM"] = new OperatorDesc(Operator.EndProgram, OperatorFlags.Keyword | OperatorFlags.Internal);
			operators["END_REPEAT"] = new OperatorDesc(Operator.EndRepeat, OperatorFlags.Keyword | OperatorFlags.StructuredText);
			operators["END_STRUCT"] = new OperatorDesc(Operator.EndStruct, OperatorFlags.Keyword | OperatorFlags.Declaration);
			operators["END_UNION"] = new OperatorDesc(Operator.EndUnion, OperatorFlags.Keyword | OperatorFlags.Declaration);
			operators["END_TYPE"] = new OperatorDesc(Operator.EndType, OperatorFlags.Keyword | OperatorFlags.Declaration);
			operators["END_VAR"] = new OperatorDesc(Operator.EndVar, OperatorFlags.Keyword | OperatorFlags.Declaration);
			operators["END_WHILE"] = new OperatorDesc(Operator.EndWhile, OperatorFlags.Keyword | OperatorFlags.StructuredText);
			operators["EXIT"] = new OperatorDesc(Operator.Exit, OperatorFlags.Keyword | OperatorFlags.StructuredText);
			operators["CONTINUE"] = new OperatorDesc(Operator.Continue, OperatorFlags.Keyword | OperatorFlags.StructuredText);
			operators["FOR"] = new OperatorDesc(Operator.For, OperatorFlags.Keyword | OperatorFlags.StructuredText);
			operators["FROM"] = new OperatorDesc(Operator.From, OperatorFlags.Keyword | OperatorFlags.Internal);
			operators["FUNCTION"] = new OperatorDesc(Operator.Function, OperatorFlags.Keyword | OperatorFlags.Declaration);
			operators["FUNCTION_BLOCK"] = new OperatorDesc(Operator.FunctionBlock, OperatorFlags.Keyword | OperatorFlags.Declaration);
			operators["IF"] = new OperatorDesc(Operator.If, OperatorFlags.Keyword | OperatorFlags.StructuredText);
			operators["OF"] = new OperatorDesc(Operator.Of, OperatorFlags.Keyword | OperatorFlags.Declaration);
			operators["PARAMS"] = new OperatorDesc(Operator.Params, OperatorFlags.Keyword | OperatorFlags.Declaration);
			operators["PERSISTENT"] = new OperatorDesc(Operator.Persistent, OperatorFlags.Keyword | OperatorFlags.Declaration);
			operators["POINTER"] = new OperatorDesc(Operator.Pointer, OperatorFlags.Keyword | OperatorFlags.Declaration);
			operators["REFERENCE"] = new OperatorDesc(Operator.Reference, OperatorFlags.Keyword | OperatorFlags.Declaration);
			operators["PROGRAM"] = new OperatorDesc(Operator.Program, OperatorFlags.Keyword | OperatorFlags.Declaration);
			operators["READ_ONLY"] = new OperatorDesc(Operator.ReadOnly, OperatorFlags.Keyword | OperatorFlags.Internal);
			operators["READ_WRITE"] = new OperatorDesc(Operator.ReadWrite, OperatorFlags.Keyword | OperatorFlags.Internal);
			operators["REPEAT"] = new OperatorDesc(Operator.Repeat, OperatorFlags.Keyword | OperatorFlags.StructuredText);
			operators["RETAIN"] = new OperatorDesc(Operator.Retain, OperatorFlags.Keyword | OperatorFlags.Declaration);
			operators["RETURN"] = new OperatorDesc(Operator.Return, OperatorFlags.Keyword | OperatorFlags.StructuredText);
			operators["STRUCT"] = new OperatorDesc(Operator.Struct, OperatorFlags.Keyword | OperatorFlags.Declaration);
			operators["UNION"] = new OperatorDesc(Operator.Union, OperatorFlags.Keyword | OperatorFlags.Declaration);
			operators["THEN"] = new OperatorDesc(Operator.Then, OperatorFlags.Keyword | OperatorFlags.StructuredText);
			operators["TO"] = new OperatorDesc(Operator.To, OperatorFlags.Keyword | OperatorFlags.StructuredText | OperatorFlags.Declaration);
			operators["TYPE"] = new OperatorDesc(Operator.Type, OperatorFlags.Keyword | OperatorFlags.Declaration);
			operators["UNTIL"] = new OperatorDesc(Operator.Until, OperatorFlags.Keyword | OperatorFlags.StructuredText);
			operators["VAR"] = new OperatorDesc(Operator.Var, OperatorFlags.Keyword | OperatorFlags.Declaration);
			operators["VAR_ACCESS"] = new OperatorDesc(Operator.VarAccess, OperatorFlags.Keyword | OperatorFlags.Declaration);
			operators["VAR_CONFIG"] = new OperatorDesc(Operator.VarConfig, OperatorFlags.Keyword | OperatorFlags.Declaration);
			operators["VAR_EXTERNAL"] = new OperatorDesc(Operator.VarExternal, OperatorFlags.Keyword | OperatorFlags.Declaration);
			operators["VAR_GLOBAL"] = new OperatorDesc(Operator.VarGlobal, OperatorFlags.Keyword | OperatorFlags.Declaration);
			operators["VAR_INPUT"] = new OperatorDesc(Operator.VarInput, OperatorFlags.Keyword | OperatorFlags.Declaration);
			operators["VAR_IN_OUT"] = new OperatorDesc(Operator.VarInOut, OperatorFlags.Keyword | OperatorFlags.Declaration);
			operators["VAR_OUTPUT"] = new OperatorDesc(Operator.VarOutput, OperatorFlags.Keyword | OperatorFlags.Declaration);
			operators["VAR_TEMP"] = new OperatorDesc(Operator.VarTemp, OperatorFlags.Keyword | OperatorFlags.Declaration);
			operators["VAR_STAT"] = new OperatorDesc(Operator.VarStat, OperatorFlags.Keyword | OperatorFlags.Declaration);
			operators["WHILE"] = new OperatorDesc(Operator.While, OperatorFlags.Keyword | OperatorFlags.StructuredText);
			operators["EXTENDS"] = new OperatorDesc(Operator.Extends, OperatorFlags.Keyword | OperatorFlags.Declaration);
			operators["IMPLEMENTS"] = new OperatorDesc(Operator.Implements, OperatorFlags.Keyword | OperatorFlags.Declaration);
			operators["METHOD"] = new OperatorDesc(Operator.Method, OperatorFlags.Keyword | OperatorFlags.Declaration);
			operators["END_METHOD"] = new OperatorDesc(Operator.EndMethod, OperatorFlags.Keyword | OperatorFlags.Internal);
			operators["PROPERTY"] = new OperatorDesc(Operator.Property, OperatorFlags.Keyword | OperatorFlags.Declaration);
			operators["END_PROPERTY"] = new OperatorDesc(Operator.EndProperty, OperatorFlags.Keyword | OperatorFlags.Internal);
			operators["PROPERTY_SET"] = new OperatorDesc(Operator.PropertySet, OperatorFlags.Keyword | OperatorFlags.Internal);
			operators["PROPERTY_GET"] = new OperatorDesc(Operator.PropertyGet, OperatorFlags.Keyword | OperatorFlags.Internal);
			operators["INTERFACE"] = new OperatorDesc(Operator.Interface, OperatorFlags.Keyword | OperatorFlags.Declaration);
			operators["END_INTERFACE"] = new OperatorDesc(Operator.EndInterface, OperatorFlags.Keyword | OperatorFlags.Internal);
			operators["THIS"] = new OperatorDesc(Operator.This, OperatorFlags.AllLanguages | OperatorFlags.Keyword);
			operators["SUPER"] = new OperatorDesc(Operator.Super, OperatorFlags.Keyword | OperatorFlags.StructuredText);
			operators["VAR_INST"] = new OperatorDesc(Operator.VarInst, OperatorFlags.Keyword | OperatorFlags.Declaration);
			operators["VAR_GENERIC"] = new OperatorDesc(Operator.VarGeneric, OperatorFlags.Keyword | OperatorFlags.Declaration);
		}

		private void AddStandardOperators(IDictionary<string, OperatorDesc> operators)
		{
			operators["LOWER_BOUND"] = new OperatorDesc(Operator.LowerBound, OperatorFlags.AllLanguages | OperatorFlags.Operator);
			operators["UPPER_BOUND"] = new OperatorDesc(Operator.UpperBound, OperatorFlags.AllLanguages | OperatorFlags.Operator);
			operators["ADR"] = new OperatorDesc(Operator.Adr, OperatorFlags.AllLanguages | OperatorFlags.Operator);
			operators["BITADR"] = new OperatorDesc(Operator.BitAdr, OperatorFlags.AllLanguages | OperatorFlags.Operator);
			operators["INDEXOF"] = new OperatorDesc(Operator.IndexOf, OperatorFlags.Operator | OperatorFlags.Internal);
			operators["SIZEOF"] = new OperatorDesc(Operator.SizeOf, OperatorFlags.AllLanguages | OperatorFlags.Operator);
			operators["XSIZEOF"] = new OperatorDesc(Operator.XSizeOf, OperatorFlags.AllLanguages | OperatorFlags.Operator);
			operators["INI"] = new OperatorDesc(Operator.Ini, OperatorFlags.Operator | OperatorFlags.Internal);
			operators["ABS"] = new OperatorDesc(Operator.Abs, OperatorFlags.AllLanguages | OperatorFlags.Operator);
			operators["LIMIT"] = new OperatorDesc(Operator.Limit, OperatorFlags.AllLanguages | OperatorFlags.Operator);
			operators["MIN"] = new OperatorDesc(Operator.Min, OperatorFlags.AllLanguages | OperatorFlags.Operator);
			operators["MAX"] = new OperatorDesc(Operator.Max, OperatorFlags.AllLanguages | OperatorFlags.Operator);
			operators["TRUNC"] = new OperatorDesc(Operator.Trunc, OperatorFlags.AllLanguages | OperatorFlags.Operator);
			operators["TRUNC_INT"] = new OperatorDesc(Operator.TruncInt, OperatorFlags.AllLanguages | OperatorFlags.Operator);
			operators["MUX"] = new OperatorDesc(Operator.Mux, OperatorFlags.AllLanguages | OperatorFlags.Operator);
			operators["SEL"] = new OperatorDesc(Operator.Sel, OperatorFlags.AllLanguages | OperatorFlags.Operator);
			operators["ROL"] = new OperatorDesc(Operator.Rol, OperatorFlags.AllLanguages | OperatorFlags.Operator);
			operators["ROR"] = new OperatorDesc(Operator.Ror, OperatorFlags.AllLanguages | OperatorFlags.Operator);
			operators["SHL"] = new OperatorDesc(Operator.Shl, OperatorFlags.AllLanguages | OperatorFlags.Operator);
			operators["SHR"] = new OperatorDesc(Operator.Shr, OperatorFlags.AllLanguages | OperatorFlags.Operator);
			operators["EXP"] = new OperatorDesc(Operator.Exp, OperatorFlags.AllLanguages | OperatorFlags.Operator);
			operators["EXPT"] = new OperatorDesc(Operator.Expt, OperatorFlags.AllLanguages | OperatorFlags.Operator);
			operators["SQRT"] = new OperatorDesc(Operator.Sqrt, OperatorFlags.AllLanguages | OperatorFlags.Operator);
			operators["LN"] = new OperatorDesc(Operator.Ln, OperatorFlags.AllLanguages | OperatorFlags.Operator);
			operators["LOG"] = new OperatorDesc(Operator.Log, OperatorFlags.AllLanguages | OperatorFlags.Operator);
			operators["SIN"] = new OperatorDesc(Operator.Sin, OperatorFlags.AllLanguages | OperatorFlags.Operator);
			operators["COS"] = new OperatorDesc(Operator.Cos, OperatorFlags.AllLanguages | OperatorFlags.Operator);
			operators["TAN"] = new OperatorDesc(Operator.Tan, OperatorFlags.AllLanguages | OperatorFlags.Operator);
			operators["ASIN"] = new OperatorDesc(Operator.ASin, OperatorFlags.AllLanguages | OperatorFlags.Operator);
			operators["ACOS"] = new OperatorDesc(Operator.ACos, OperatorFlags.AllLanguages | OperatorFlags.Operator);
			operators["ATAN"] = new OperatorDesc(Operator.ATan, OperatorFlags.AllLanguages | OperatorFlags.Operator);
			operators["ADD"] = new OperatorDesc(Operator.Add, OperatorFlags.Operator | OperatorFlags.InstructionList | OperatorFlags.FunctionBlockDiagram);
			operators["SUB"] = new OperatorDesc(Operator.Sub, OperatorFlags.Operator | OperatorFlags.InstructionList | OperatorFlags.FunctionBlockDiagram);
			operators["MUL"] = new OperatorDesc(Operator.Mul, OperatorFlags.Operator | OperatorFlags.InstructionList | OperatorFlags.FunctionBlockDiagram);
			operators["DIV"] = new OperatorDesc(Operator.Div, OperatorFlags.Operator | OperatorFlags.InstructionList | OperatorFlags.FunctionBlockDiagram);
			operators["MOD"] = new OperatorDesc(Operator.Mod, OperatorFlags.Operator | OperatorFlags.StructuredText | OperatorFlags.InstructionList | OperatorFlags.FunctionBlockDiagram);
			operators["AND"] = new OperatorDesc(Operator.And, OperatorFlags.Operator | OperatorFlags.StructuredText | OperatorFlags.InstructionList | OperatorFlags.FunctionBlockDiagram);
			operators["ANDN"] = new OperatorDesc(Operator.AndN, OperatorFlags.Operator | OperatorFlags.InstructionList);
			operators["OR"] = new OperatorDesc(Operator.Or, OperatorFlags.Operator | OperatorFlags.StructuredText | OperatorFlags.InstructionList | OperatorFlags.FunctionBlockDiagram);
			operators["ORN"] = new OperatorDesc(Operator.OrN, OperatorFlags.Operator | OperatorFlags.InstructionList);
		}

		private void AddOOKeywords(IDictionary<string, OperatorDesc> operators)
		{
			operators["ABSTRACT"] = new OperatorDesc(Operator.Abstract, OperatorFlags.Keyword | OperatorFlags.Declaration);
			operators["PUBLIC"] = new OperatorDesc(Operator.Public, OperatorFlags.Keyword | OperatorFlags.Declaration);
			operators["PRIVATE"] = new OperatorDesc(Operator.Private, OperatorFlags.Keyword | OperatorFlags.Declaration);
			operators["PROTECTED"] = new OperatorDesc(Operator.Protected, OperatorFlags.Keyword | OperatorFlags.Declaration);
			operators["INTERNAL"] = new OperatorDesc(Operator.Internal, OperatorFlags.Keyword | OperatorFlags.Declaration);
			operators["FINAL"] = new OperatorDesc(Operator.Final, OperatorFlags.Keyword | OperatorFlags.Declaration);
		}

		private void AddDataTypeNames(IDictionary<string, OperatorDesc> operators)
		{
			operators["ANY"] = new OperatorDesc(Operator.Any, OperatorFlags.DataType | OperatorFlags.Internal);
			operators["ANY_BIT"] = new OperatorDesc(Operator.AnyBit, OperatorFlags.DataType | OperatorFlags.Internal);
			operators["ANY_DATE"] = new OperatorDesc(Operator.AnyDate, OperatorFlags.DataType | OperatorFlags.Internal);
			operators["ANY_INT"] = new OperatorDesc(Operator.AnyInt, OperatorFlags.DataType | OperatorFlags.Internal);
			operators["ANY_NUM"] = new OperatorDesc(Operator.AnyNum, OperatorFlags.DataType | OperatorFlags.Internal);
			operators["ANY_REAL"] = new OperatorDesc(Operator.AnyReal, OperatorFlags.DataType | OperatorFlags.Internal);
			operators["ANY_STRING"] = new OperatorDesc(Operator.AnyString, OperatorFlags.DataType | OperatorFlags.Internal);
			operators["SAFEBOOL"] = new OperatorDesc(Operator.SafeBool, OperatorFlags.AllLanguages | OperatorFlags.SafetyDataType);
			operators["SAFEBYTE"] = new OperatorDesc(Operator.SafeByte, OperatorFlags.AllLanguages | OperatorFlags.SafetyDataType);
			operators["SAFESINT"] = new OperatorDesc(Operator.SafeSInt, OperatorFlags.AllLanguages | OperatorFlags.SafetyDataType);
			operators["SAFEUSINT"] = new OperatorDesc(Operator.SafeUSInt, OperatorFlags.AllLanguages | OperatorFlags.SafetyDataType);
			operators["SAFEWORD"] = new OperatorDesc(Operator.SafeWord, OperatorFlags.AllLanguages | OperatorFlags.SafetyDataType);
			operators["SAFEINT"] = new OperatorDesc(Operator.SafeInt, OperatorFlags.AllLanguages | OperatorFlags.SafetyDataType);
			operators["SAFEUINT"] = new OperatorDesc(Operator.SafeUInt, OperatorFlags.AllLanguages | OperatorFlags.SafetyDataType);
			operators["SAFEDWORD"] = new OperatorDesc(Operator.SafeDWord, OperatorFlags.AllLanguages | OperatorFlags.SafetyDataType);
			operators["SAFEDINT"] = new OperatorDesc(Operator.SafeDInt, OperatorFlags.AllLanguages | OperatorFlags.SafetyDataType);
			operators["SAFEUDINT"] = new OperatorDesc(Operator.SafeUDInt, OperatorFlags.AllLanguages | OperatorFlags.SafetyDataType);
			operators["SAFELWORD"] = new OperatorDesc(Operator.SafeLWord, OperatorFlags.AllLanguages | OperatorFlags.SafetyDataType);
			operators["SAFELINT"] = new OperatorDesc(Operator.SafeLInt, OperatorFlags.AllLanguages | OperatorFlags.SafetyDataType);
			operators["SAFEULINT"] = new OperatorDesc(Operator.SafeULInt, OperatorFlags.AllLanguages | OperatorFlags.SafetyDataType);
			operators["SAFETIME"] = new OperatorDesc(Operator.SafeTime, OperatorFlags.AllLanguages | OperatorFlags.SafetyDataType);
			operators["SAFEREAL"] = new OperatorDesc(Operator.SafeReal, OperatorFlags.AllLanguages | OperatorFlags.SafetyDataType);
			operators["SAFELREAL"] = new OperatorDesc(Operator.SafeLReal, OperatorFlags.AllLanguages | OperatorFlags.SafetyDataType);
			operators["BOOL"] = new OperatorDesc(Operator.Bool, OperatorFlags.AllLanguages | OperatorFlags.DataType);
			operators["BIT"] = new OperatorDesc(Operator.Bit, OperatorFlags.AllLanguages | OperatorFlags.DataType);
			operators["BYTE"] = new OperatorDesc(Operator.Byte, OperatorFlags.AllLanguages | OperatorFlags.DataType | OperatorFlags.NumericDataType);
			operators["WORD"] = new OperatorDesc(Operator.Word, OperatorFlags.AllLanguages | OperatorFlags.DataType | OperatorFlags.NumericDataType);
			operators["DWORD"] = new OperatorDesc(Operator.DWord, OperatorFlags.AllLanguages | OperatorFlags.DataType | OperatorFlags.NumericDataType);
			operators["LWORD"] = new OperatorDesc(Operator.LWord, OperatorFlags.AllLanguages | OperatorFlags.DataType | OperatorFlags.NumericDataType);
			operators["SINT"] = new OperatorDesc(Operator.SInt, OperatorFlags.AllLanguages | OperatorFlags.DataType | OperatorFlags.NumericDataType);
			operators["INT"] = new OperatorDesc(Operator.Int, OperatorFlags.AllLanguages | OperatorFlags.DataType | OperatorFlags.NumericDataType);
			operators["DINT"] = new OperatorDesc(Operator.DInt, OperatorFlags.AllLanguages | OperatorFlags.DataType | OperatorFlags.NumericDataType);
			operators["LINT"] = new OperatorDesc(Operator.LInt, OperatorFlags.AllLanguages | OperatorFlags.DataType | OperatorFlags.NumericDataType);
			operators["USINT"] = new OperatorDesc(Operator.USInt, OperatorFlags.AllLanguages | OperatorFlags.DataType | OperatorFlags.NumericDataType);
			operators["UINT"] = new OperatorDesc(Operator.UInt, OperatorFlags.AllLanguages | OperatorFlags.DataType | OperatorFlags.NumericDataType);
			operators["UDINT"] = new OperatorDesc(Operator.UDInt, OperatorFlags.AllLanguages | OperatorFlags.DataType | OperatorFlags.NumericDataType);
			operators["ULINT"] = new OperatorDesc(Operator.ULInt, OperatorFlags.AllLanguages | OperatorFlags.DataType | OperatorFlags.NumericDataType);
			operators["__XWORD"] = new OperatorDesc(Operator.__XWord, OperatorFlags.AllLanguages | OperatorFlags.DataType | OperatorFlags.NumericDataType);
			operators["__UXINT"] = new OperatorDesc(Operator.__UXInt, OperatorFlags.AllLanguages | OperatorFlags.DataType | OperatorFlags.NumericDataType);
			operators["__XINT"] = new OperatorDesc(Operator.__XInt, OperatorFlags.AllLanguages | OperatorFlags.DataType | OperatorFlags.NumericDataType);
			operators["REAL"] = new OperatorDesc(Operator.Real, OperatorFlags.AllLanguages | OperatorFlags.DataType | OperatorFlags.NumericDataType);
			operators["LREAL"] = new OperatorDesc(Operator.LReal, OperatorFlags.AllLanguages | OperatorFlags.DataType | OperatorFlags.NumericDataType);
			operators["WSTRING"] = new OperatorDesc(Operator.WString, OperatorFlags.AllLanguages | OperatorFlags.DataType);
			operators["STRING"] = new OperatorDesc(Operator.String, OperatorFlags.AllLanguages | OperatorFlags.DataType);
			operators["__XSTRING"] = new OperatorDesc(Operator.__XString, OperatorFlags.DataType | OperatorFlags.Internal);
			operators["TIME"] = new OperatorDesc(Operator.Time, OperatorFlags.AllLanguages | OperatorFlags.DataType);
			operators["LTIME"] = new OperatorDesc(Operator.LTime, OperatorFlags.AllLanguages | OperatorFlags.DataType);
			operators["DATE"] = new OperatorDesc(Operator.Date, OperatorFlags.AllLanguages | OperatorFlags.DataType);
			operators["LDATE"] = new OperatorDesc(Operator.LDate, OperatorFlags.AllLanguages | OperatorFlags.DataType);
			operators["DATE_AND_TIME"] = new OperatorDesc(Operator.DateAndTime, OperatorFlags.AllLanguages | OperatorFlags.DataType);
			operators["LDATE_AND_TIME"] = new OperatorDesc(Operator.LDateAndTime, OperatorFlags.AllLanguages | OperatorFlags.DataType);
			operators["DT"] = new OperatorDesc(Operator.DateAndTime, OperatorFlags.AllLanguages | OperatorFlags.DataType);
			operators["LDT"] = new OperatorDesc(Operator.LDateAndTime, OperatorFlags.AllLanguages | OperatorFlags.DataType);
			operators["TIME_OF_DAY"] = new OperatorDesc(Operator.TimeOfDay, OperatorFlags.AllLanguages | OperatorFlags.DataType);
			operators["LTIME_OF_DAY"] = new OperatorDesc(Operator.LTimeOfDay, OperatorFlags.AllLanguages | OperatorFlags.DataType);
			operators["TOD"] = new OperatorDesc(Operator.TimeOfDay, OperatorFlags.AllLanguages | OperatorFlags.DataType);
			operators["LTOD"] = new OperatorDesc(Operator.LTimeOfDay, OperatorFlags.AllLanguages | OperatorFlags.DataType);
		}

		private void AddSpecialOperators(IDictionary<string, OperatorDesc> operators)
		{
			operators["__COPY"] = new OperatorDesc(Operator.__Copy, OperatorFlags.Operator | OperatorFlags.Internal);
			operators["__RELOC"] = new OperatorDesc(Operator.__Reloc, OperatorFlags.Operator | OperatorFlags.Internal);
			operators["__LAZY"] = new OperatorDesc(Operator.__Lazy, OperatorFlags.DataType | OperatorFlags.Internal);
			operators["__CRC"] = new OperatorDesc(Operator.__CRC, OperatorFlags.Operator | OperatorFlags.Internal);
			operators["__MAXOFFSET"] = new OperatorDesc(Operator.__MaxOffset, OperatorFlags.Operator | OperatorFlags.Internal);
			operators["__LOCALOFFSET"] = new OperatorDesc(Operator.__LocalOffset, OperatorFlags.Operator | OperatorFlags.Internal);
			operators["__TYPEOF"] = new OperatorDesc(Operator.__TypeOf, OperatorFlags.Operator | OperatorFlags.Internal);
			operators["__VARINFO"] = new OperatorDesc(Operator.__VarInfo, OperatorFlags.Operator | OperatorFlags.Internal);
			operators["__SYSTEM"] = new OperatorDesc(Operator.__SystemScope, OperatorFlags.AllLanguages | OperatorFlags.Operator);
			operators["__POOL"] = new OperatorDesc(Operator.__PoolScope, OperatorFlags.AllLanguages | OperatorFlags.Operator);
			operators["__INIT"] = new OperatorDesc(Operator.__Init, OperatorFlags.Operator | OperatorFlags.Internal);
			operators["__CAST"] = new OperatorDesc(Operator.__Cast, OperatorFlags.Operator | OperatorFlags.Internal);
			operators["__ISVALIDREF"] = new OperatorDesc(Operator.__IsValidRef, OperatorFlags.AllLanguages | OperatorFlags.Operator);
			operators["__FCALL"] = new OperatorDesc(Operator.__FCall, OperatorFlags.Operator | OperatorFlags.Internal);
			operators["__PROPERTYINFO"] = new OperatorDesc(Operator.__PropertyInfo, OperatorFlags.Operator | OperatorFlags.Internal);
			operators["__ADRINST"] = new OperatorDesc(Operator.__AdrInst, OperatorFlags.Operator | OperatorFlags.Internal);
			operators["__REFADR"] = new OperatorDesc(Operator.__RefAdr, OperatorFlags.Operator | OperatorFlags.Internal);
			operators["__MEMORYSET"] = new OperatorDesc(Operator.__MemorySet, OperatorFlags.Operator | OperatorFlags.Internal);
			operators["__GETLTICK"] = new OperatorDesc(Operator.__GetLTick, OperatorFlags.Operator | OperatorFlags.Internal);
			operators["__QUERYINTERFACE"] = new OperatorDesc(Operator.__QueryInterface, OperatorFlags.AllLanguages | OperatorFlags.Operator);
			operators["__QUERYPOINTER"] = new OperatorDesc(Operator.__QueryPointer, OperatorFlags.AllLanguages | OperatorFlags.Operator);
			operators["__NEW"] = new OperatorDesc(Operator.__New, OperatorFlags.AllLanguages | OperatorFlags.Operator);
			operators["__DELETE"] = new OperatorDesc(Operator.__Delete, OperatorFlags.AllLanguages | OperatorFlags.Operator);
			operators["__WAIT"] = new OperatorDesc(Operator.__Wait, OperatorFlags.Operator | OperatorFlags.Internal);
			operators["__TRY"] = new OperatorDesc(Operator.__Try, OperatorFlags.Keyword | OperatorFlags.StructuredText);
			operators["__ENDTRY"] = new OperatorDesc(Operator.__EndTry, OperatorFlags.Keyword | OperatorFlags.StructuredText);
			operators["__CATCH"] = new OperatorDesc(Operator.__Catch, OperatorFlags.Keyword | OperatorFlags.StructuredText);
			operators["__FINALLY"] = new OperatorDesc(Operator.__Finally, OperatorFlags.Keyword | OperatorFlags.StructuredText);
			operators["__THROW"] = new OperatorDesc(Operator.__Throw, OperatorFlags.Operator | OperatorFlags.StructuredText | OperatorFlags.Internal);
			operators["__BITOFFSET"] = new OperatorDesc(Operator.__BitOffset, OperatorFlags.Operator | OperatorFlags.Internal);
			operators["__XADD"] = new OperatorDesc(Operator.__XAdd, OperatorFlags.AllLanguages | OperatorFlags.Operator);
			operators["__COMPARE_AND_SWAP"] = new OperatorDesc(Operator.__CompareAndSwap, OperatorFlags.AllLanguages | OperatorFlags.Operator);
			operators["__MEMORYBARRIER"] = new OperatorDesc(Operator.__MemoryBarrier, OperatorFlags.AllLanguages | OperatorFlags.Operator);
			operators["__CURRENTTASK"] = new OperatorDesc(Operator.__CurrentTask, OperatorFlags.AllLanguages | OperatorFlags.Operator);
			operators["__CHECKLICENSE"] = new OperatorDesc(Operator.__CheckLicense, OperatorFlags.AllLanguages | OperatorFlags.Operator);
			operators["__CHECKLICENSEBIT"] = new OperatorDesc(Operator.__CheckLicenseBit, OperatorFlags.AllLanguages | OperatorFlags.Operator);
			operators["__CALLINITFUNCTION"] = new OperatorDesc(Operator.__CallInitFunction, OperatorFlags.Operator | OperatorFlags.Internal);
			operators["__LATECOMPILEDEXPR"] = new OperatorDesc(Operator.__LateCompiledExpr, OperatorFlags.Operator | OperatorFlags.Internal);
			operators["__POUNAME"] = new OperatorDesc(Operator.__PouName, OperatorFlags.AllLanguages | OperatorFlags.Operator);
			operators["__POSITION"] = new OperatorDesc(Operator.__Position, OperatorFlags.AllLanguages | OperatorFlags.Operator);
			operators["AND_THEN"] = new OperatorDesc(Operator.And_Then, OperatorFlags.Operator | OperatorFlags.StructuredText | OperatorFlags.InstructionList | OperatorFlags.FunctionBlockDiagram);
			operators["OR_ELSE"] = new OperatorDesc(Operator.Or_Else, OperatorFlags.Operator | OperatorFlags.StructuredText | OperatorFlags.InstructionList | OperatorFlags.FunctionBlockDiagram);
		}

		private void AddConversionOperators(Dictionary<string, OperatorDesc> operators)
		{
			ArrayList arrayList = new ArrayList();
			List<OperatorDesc> list = new List<OperatorDesc>();
			foreach (string key4 in operators.Keys)
			{
				OperatorDesc operatorDesc = operators[key4];
				if ((operatorDesc.Flags & OperatorFlags.DataType) != 0)
				{
					arrayList.Add(operatorDesc.Operator);
				}
				if ((operatorDesc.Flags & OperatorFlags.SafetyDataType) != 0)
				{
					arrayList.Add(operatorDesc.Operator);
				}
				if ((operatorDesc.Flags & OperatorFlags.DataType) != 0 && (operatorDesc.Flags & OperatorFlags.NumericDataType) != 0)
				{
					list.Add(operatorDesc);
				}
			}
			FillOperatorTable(operators);
			AddOverloadedConversions(operators, arrayList);
			string textOfOperator = GetTextOfOperator(Operator.Reference);
			string textOfOperator2 = GetTextOfOperator(Operator.Pointer);
			string key = textOfOperator + "_TO_" + textOfOperator2;
			operators[key] = new OperatorDesc(Operator.Conversion, OperatorFlags.AllLanguages | OperatorFlags.Operator);
			foreach (OperatorDesc item in list)
			{
				string textOfOperator3 = GetTextOfOperator(item.Operator, bShort: true);
				string key2 = "ANY_NUM_TO_" + textOfOperator3;
				operators[key2] = new OperatorDesc(Operator.Conversion, OperatorFlags.AllLanguages | OperatorFlags.Operator);
			}
			foreach (Operator item2 in arrayList)
			{
				string textOfOperator4 = GetTextOfOperator(item2, bShort: true);
				string key3 = "ANY_TO_" + textOfOperator4;
				operators[key3] = new OperatorDesc(Operator.Conversion, OperatorFlags.AllLanguages | OperatorFlags.Operator);
			}
			Insert(operators);
		}

		private void AddOverloadedConversions(Dictionary<string, OperatorDesc> operators, ArrayList dataTypes)
		{
			for (int i = 0; i < dataTypes.Count; i++)
			{
				string textOfOperator = GetTextOfOperator((Operator)dataTypes[i], bShort: true);
				string key = "TO_" + textOfOperator;
				operators[key] = new OperatorDesc(Operator.Conversion, OperatorFlags.AllLanguages | OperatorFlags.Operator);
				for (int j = 0; j < dataTypes.Count; j++)
				{
					if (i != j)
					{
						string textOfOperator2 = GetTextOfOperator((Operator)dataTypes[j], bShort: true);
						key = textOfOperator + "_TO_" + textOfOperator2;
						operators[key] = new OperatorDesc(Operator.Conversion, OperatorFlags.AllLanguages | OperatorFlags.Operator);
					}
				}
			}
		}
	}
}
