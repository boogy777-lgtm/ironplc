using System;
using System.Collections.Generic;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	internal sealed class ReStructuredTextParser
	{
		private readonly string DOC_COMMENT_BLOCK_SEPARATOR = "\r\n\r\n";

		private static readonly HashSet<EReStructuredTextToken> s_hsDocCommentBlocksToIgnore;

		private static readonly Dictionary<string, string> s_htGlobalReplacements;

		internal static ReStructuredTextParser Instance { get; }

		private ReStructuredTextParser()
		{
		}

		static ReStructuredTextParser()
		{
			Instance = new ReStructuredTextParser();
			s_hsDocCommentBlocksToIgnore = new HashSet<EReStructuredTextToken>();
			s_hsDocCommentBlocksToIgnore.Add(EReStructuredTextToken.CodesysRanges);
			s_hsDocCommentBlocksToIgnore.Add(EReStructuredTextToken.Replace);
			s_hsDocCommentBlocksToIgnore.Add(EReStructuredTextToken.UnspecifiedMarkup);
			s_htGlobalReplacements = new Dictionary<string, string>();
			s_htGlobalReplacements.Add(".. note::", "note:");
			s_htGlobalReplacements.Add(":return:", "return:");
			s_htGlobalReplacements.Add(".. admonition::", string.Empty);
		}

		private bool IsSimpleTableBorder(string stText)
		{
			char[] array = stText.ToCharArray();
			int i = 0;
			int num = array.Length;
			int num2 = 0;
			bool flag = false;
			for (; i < num; i++)
			{
				if (flag)
				{
					break;
				}
				switch (array[i])
				{
				case ' ':
					num2++;
					break;
				default:
					flag = true;
					break;
				case '=':
					break;
				}
			}
			if (!flag)
			{
				flag = num2 == 0;
			}
			return !flag;
		}

		private bool IsSimpleTable(string stTimmedPart)
		{
			string text = "\r\n";
			string[] array = stTimmedPart.Split(new string[1] { text }, StringSplitOptions.None);
			if (2 <= array.Length)
			{
				if (IsSimpleTableBorder(array[0]))
				{
					return IsSimpleTableBorder(array[array.Length - 1]);
				}
				return false;
			}
			return false;
		}

		private bool ContainsMarkup(string stTimmedPart)
		{
			char[] array = stTimmedPart.ToCharArray();
			int i = 0;
			int num = array.Length;
			EMarkupScannerState eMarkupScannerState = EMarkupScannerState.Start;
			for (; i < num; i++)
			{
				if (EMarkupScannerState.End == eMarkupScannerState)
				{
					break;
				}
				if (EMarkupScannerState.Error == eMarkupScannerState)
				{
					break;
				}
				char c = array[i];
				switch (eMarkupScannerState)
				{
				case EMarkupScannerState.Start:
					eMarkupScannerState = (('.' == c) ? EMarkupScannerState.Dot1 : EMarkupScannerState.Error);
					break;
				case EMarkupScannerState.Dot1:
					eMarkupScannerState = (('.' == c) ? EMarkupScannerState.Dot2 : EMarkupScannerState.Error);
					break;
				case EMarkupScannerState.Dot2:
					eMarkupScannerState = (char.IsWhiteSpace(c) ? EMarkupScannerState.Whitespace : EMarkupScannerState.Error);
					break;
				case EMarkupScannerState.Whitespace:
					if (!char.IsWhiteSpace(c))
					{
						eMarkupScannerState = ((':' == c) ? EMarkupScannerState.Error : EMarkupScannerState.Identifier);
					}
					break;
				case EMarkupScannerState.Identifier:
					if (char.IsWhiteSpace(c))
					{
						eMarkupScannerState = EMarkupScannerState.Error;
					}
					else if (':' == c)
					{
						eMarkupScannerState = EMarkupScannerState.Colon1;
					}
					break;
				case EMarkupScannerState.Colon1:
					eMarkupScannerState = ((':' == c) ? EMarkupScannerState.Colon2 : EMarkupScannerState.Error);
					break;
				case EMarkupScannerState.Colon2:
					eMarkupScannerState = (char.IsWhiteSpace(c) ? EMarkupScannerState.End : EMarkupScannerState.Error);
					break;
				}
			}
			if (EMarkupScannerState.End != eMarkupScannerState)
			{
				return EMarkupScannerState.Colon2 == eMarkupScannerState;
			}
			return true;
		}

		private List<DocCommentBlock> SplitAndFilterBlocks(string stText)
		{
			string[] array = stText.Split(new string[1] { DOC_COMMENT_BLOCK_SEPARATOR }, StringSplitOptions.None);
			EReStructuredTextToken eReStructuredTextToken = EReStructuredTextToken.Undefined;
			EReStructuredTextToken eReStructuredTextToken2 = EReStructuredTextToken.Undefined;
			List<DocCommentBlock> list = new List<DocCommentBlock>();
			for (int i = 0; i < array.Length; i++)
			{
				bool flag = true;
				string text = array[i].Trim();
				if (text.StartsWith(".. code-block:: codesys"))
				{
					eReStructuredTextToken = EReStructuredTextToken.CodesysCodeBlock;
					flag = false;
				}
				else if (text.StartsWith(".. cds:ranges::"))
				{
					eReStructuredTextToken = EReStructuredTextToken.CodesysRanges;
					flag = false;
				}
				else if (text.Contains("replace::"))
				{
					eReStructuredTextToken = EReStructuredTextToken.Replace;
					flag = false;
				}
				else if (text.Contains(":ref:") || text.Contains(":doc:"))
				{
					eReStructuredTextToken = EReStructuredTextToken.Hyperlink;
					flag = false;
				}
				else if (text.StartsWith("+-") && text.EndsWith("-+"))
				{
					eReStructuredTextToken = EReStructuredTextToken.GridTable;
					flag = false;
				}
				else if (IsSimpleTable(text))
				{
					eReStructuredTextToken = EReStructuredTextToken.SimpleTable;
					flag = false;
				}
				else if (text.StartsWith(".. note::"))
				{
					eReStructuredTextToken = EReStructuredTextToken.Note;
				}
				else if (text.Contains(".. admonition::"))
				{
					eReStructuredTextToken = EReStructuredTextToken.Admonition;
				}
				else if (ContainsMarkup(text))
				{
					flag = false;
					eReStructuredTextToken = EReStructuredTextToken.UnspecifiedMarkup;
				}
				else
				{
					eReStructuredTextToken = EReStructuredTextToken.Text;
					flag = !s_hsDocCommentBlocksToIgnore.Contains(eReStructuredTextToken2);
					if (EReStructuredTextToken.CodesysCodeBlock == eReStructuredTextToken2)
					{
						eReStructuredTextToken = eReStructuredTextToken2;
					}
				}
				if (flag)
				{
					list.Add(new DocCommentBlock(array[i], eReStructuredTextToken));
				}
				eReStructuredTextToken2 = eReStructuredTextToken;
			}
			return list;
		}

		private void ProcessSingleBlock(DocCommentBlock block)
		{
			//IL_0073: Unknown result type (might be due to invalid IL or missing references)
			//IL_0079: Expected O, but got Unknown
			string text = "\r\n";
			string[] array = block.Text.Split(new string[1] { text }, StringSplitOptions.None);
			bool flag = EReStructuredTextToken.CodesysCodeBlock == block.KindOf || EReStructuredTextToken.Note == block.KindOf;
			for (int i = 0; i < array.Length; i++)
			{
				if (flag)
				{
					array[i] = "  " + array[i].TrimStart();
				}
				else
				{
					array[i] = array[i].Trim();
				}
			}
			LStringBuilder val = new LStringBuilder();
			bool flag2 = false;
			string[] array2 = array;
			foreach (string text2 in array2)
			{
				if (flag2)
				{
					val.Append(text);
				}
				val.Append(text2);
				flag2 = true;
			}
			block.Text = ((object)val).ToString();
		}

		internal string ConvertReStructuredTextDocuCommentToPlainText(string stText)
		{
			//IL_0073: Unknown result type (might be due to invalid IL or missing references)
			//IL_0079: Expected O, but got Unknown
			stText = stText.Replace("`", string.Empty);
			stText = stText.Replace("|", string.Empty);
			stText = stText.Replace("\r\n \r\n", DOC_COMMENT_BLOCK_SEPARATOR);
			List<DocCommentBlock> list = SplitAndFilterBlocks(stText);
			foreach (DocCommentBlock item in list)
			{
				ProcessSingleBlock(item);
			}
			LStringBuilder val = new LStringBuilder();
			bool flag = false;
			foreach (DocCommentBlock item2 in list)
			{
				if (flag)
				{
					val.Append(DOC_COMMENT_BLOCK_SEPARATOR);
				}
				val.Append(item2.Text);
				flag = true;
			}
			string text = ((object)val).ToString();
			int num = text.IndexOf(":math:");
			if (0 < num)
			{
				text = text.Substring(0, num) + "[...]";
			}
			foreach (KeyValuePair<string, string> s_htGlobalReplacement in s_htGlobalReplacements)
			{
				text = text.Replace(s_htGlobalReplacement.Key, s_htGlobalReplacement.Value);
			}
			return text;
		}
	}
}
