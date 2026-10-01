using System;
using System.Collections.Generic;
using System.Text;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Online;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[TypeGuid("{C055366B-8234-4C0E-A176-CFCB525A8A88}")]
	public class ToolTipService : IToolTipService2, IToolTipService
	{
		private ToolTipCommentSizeCallback _commentSizeCallback;

		public string CreateSymbolInfoToolTip(IPreCompileContext pcc, ISignature signature, IVariable variable)
		{
			StringBuilder stringBuilder = new StringBuilder();
			string stLocalizedString = null;
			if (variable != null)
			{
				bool flag = variable.HasAttribute(CompileAttributes.ATTRIBUTE_VARIABLE_LENGTH_ARRAY);
				if (variable.GetFlag(VarFlag.Global))
				{
					stringBuilder.Append("VAR_GLOBAL ");
				}
				else if (variable.GetFlag(VarFlag.Inout))
				{
					stringBuilder.Append("VAR_IN_OUT ");
				}
				else if (variable.GetFlag(VarFlag.Input))
				{
					stringBuilder.Append("VAR_INPUT ");
				}
				else if (variable.GetFlag(VarFlag.Output))
				{
					stringBuilder.Append("VAR_OUTPUT ");
				}
				else if (variable.GetFlag(VarFlag.Static))
				{
					stringBuilder.Append("VAR_STAT ");
				}
				else if (variable.GetFlag(VarFlag.Temp))
				{
					stringBuilder.Append("VAR_TEMP ");
				}
				else
				{
					stringBuilder.Append("VAR ");
				}
				if (variable.GetFlag(VarFlag.Constant))
				{
					stringBuilder.Append("CONSTANT ");
				}
				if (variable.GetFlag(VarFlag.Retain))
				{
					stringBuilder.Append("RETAIN ");
				}
				if (variable.GetFlag(VarFlag.Persistent) || variable.GetFlag(VarFlag.LocalPersistent))
				{
					stringBuilder.Append("PERSISTENT ");
				}
				if (signature != null)
				{
					stringBuilder.Append(signature.OrgName + ".");
				}
				stringBuilder.Append(variable.OrgName + " ");
				if (variable.Address != null)
				{
					stringBuilder.Append("AT " + variable.Address?.ToString() + " ");
				}
				if (variable.Type != null)
				{
					if (flag)
					{
						stringBuilder.Append(": ARRAY[*] OF " + ((IPointerType)variable.Type).Base);
					}
					else
					{
						stringBuilder.Append(": " + variable.Type);
					}
				}
				else
				{
					stringBuilder.Append(": ?");
				}
				if (variable.Initial != null)
				{
					string initialization = GetInitialization(variable.Initial, variable.Type.ToString());
					if (!string.IsNullOrEmpty(initialization))
					{
						stringBuilder.AppendLine();
						stringBuilder.Append(Strings.InitialValue + " : " + initialization);
					}
				}
				if (string.IsNullOrEmpty(variable.Comment) || !APEnvironmentFacade.Instance.TryGetLocalization(variable.Comment.Trim(), out stLocalizedString))
				{
					stLocalizedString = variable.Comment;
				}
			}
			else if (signature != null)
			{
				switch (signature.POUType)
				{
				case Operator.Type:
					stringBuilder.Append("TYPE ");
					break;
				case Operator.FunctionBlock:
					stringBuilder.Append("FUNCTION_BLOCK ");
					break;
				case Operator.Program:
					stringBuilder.Append("PROGRAM ");
					break;
				case Operator.Function:
					stringBuilder.Append("FUNCTION ");
					break;
				case Operator.VarGlobal:
					stringBuilder.Append("VAR_GLOBAL ");
					break;
				case Operator.VarAccess:
					stringBuilder.Append("VAR_ACCESS ");
					break;
				case Operator.VarConfig:
					stringBuilder.Append("VAR_CONFIG ");
					break;
				case Operator.Method:
					stringBuilder.Append("METHOD ");
					break;
				case Operator.Action:
					stringBuilder.Append("ACTION ");
					break;
				case Operator.Property:
					stringBuilder.Append("PROPERTY ");
					break;
				case Operator.Interface:
					stringBuilder.Append("INTERFACE ");
					break;
				default:
					return string.Empty;
				}
				if (signature.ParentObjectGuid != Guid.Empty && pcc != null)
				{
					ISignature signature2 = pcc.GetSignature(signature.ParentObjectGuid);
					if (signature2 != null)
					{
						stringBuilder.Append(signature2.OrgName + ".");
					}
				}
				stringBuilder.Append(signature.OrgName);
				if (!string.IsNullOrEmpty(signature.LibraryPath))
				{
					stringBuilder.Append(" [" + signature.LibraryPath + "]");
				}
				if (signature.HasAttribute("''DOCU__COMMENT"))
				{
					stLocalizedString = signature.GetAttributeValue("''DOCU__COMMENT");
				}
				else if (signature.HasAttribute("''NORMAL__COMMENT"))
				{
					stLocalizedString = signature.GetAttributeValue("''NORMAL__COMMENT");
				}
			}
			if (!string.IsNullOrEmpty(stLocalizedString))
			{
				stLocalizedString = stLocalizedString.Replace("<summary>", string.Empty).Replace("</summary>", string.Empty).Replace('\r', ' ')
					.Replace('\n', ' ')
					.Replace('\t', ' ');
				stLocalizedString = NormalizeAndTrimMultiWhiteSpaces(stLocalizedString);
				int? nCommentWidth = null;
				int? nCommentHeight = null;
				if (_commentSizeCallback != null)
				{
					_commentSizeCallback(out nCommentWidth, out nCommentHeight);
				}
				if (stLocalizedString.Length > (nCommentWidth.HasValue ? nCommentWidth.Value : 80))
				{
					IEnumerable<string> enumerable = stLocalizedString.Split(nCommentWidth.HasValue ? nCommentWidth.Value : 80);
					stLocalizedString = string.Empty;
					int num = 0;
					foreach (string item in enumerable)
					{
						if (num == (nCommentHeight.HasValue ? nCommentHeight.Value : 10))
						{
							break;
						}
						stLocalizedString = stLocalizedString + item + Environment.NewLine;
						num++;
					}
				}
				if (!string.IsNullOrEmpty(stLocalizedString))
				{
					stringBuilder.Append(Environment.NewLine + stLocalizedString);
				}
			}
			return stringBuilder.ToString();
		}

		public string CreateInfoToolTip(string stInfo)
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (!string.IsNullOrEmpty(stInfo))
			{
				string text = null;
				text = NormalizeAndTrimMultiWhiteSpaces(stInfo);
				int? nCommentWidth = null;
				int? nCommentHeight = null;
				if (_commentSizeCallback != null)
				{
					_commentSizeCallback(out nCommentWidth, out nCommentHeight);
				}
				if (text.Length > (nCommentWidth.HasValue ? nCommentWidth.Value : 80))
				{
					IEnumerable<string> enumerable = text.Split(nCommentWidth.HasValue ? nCommentWidth.Value : 80);
					text = string.Empty;
					int num = 0;
					foreach (string item in enumerable)
					{
						if (num == (nCommentHeight.HasValue ? nCommentHeight.Value : 10))
						{
							break;
						}
						text = text + item + Environment.NewLine;
						num++;
					}
				}
				if (!string.IsNullOrEmpty(text))
				{
					stringBuilder.Append(text);
				}
			}
			return stringBuilder.ToString();
		}

		private string NormalizeAndTrimMultiWhiteSpaces(string stInputComment)
		{
			char[] separator = new char[1] { ' ' };
			string[] array = stInputComment.Split(separator, StringSplitOptions.RemoveEmptyEntries);
			string text = "";
			string[] array2 = array;
			foreach (string text2 in array2)
			{
				text = text + text2 + " ";
			}
			return text.Trim();
		}

		public string CreateWatchBoxToolTip(IOnlineVarRef onlineVarRef, IVarRef varRef, string stShortenedValueString)
		{
			if (IsValidVarRef(onlineVarRef, varRef))
			{
				StringBuilder stringBuilder = new StringBuilder();
				if (onlineVarRef.Value is string && varRef.WatchExpression.Type.Class == TypeClass.Enum)
				{
					if (!string.IsNullOrEmpty(stShortenedValueString))
					{
						stringBuilder.Append(stShortenedValueString);
					}
					CreateEnumWatchBoxToolTip(onlineVarRef, varRef, stringBuilder);
				}
				else if (IsString(varRef.WatchExpression.Type.Class))
				{
					if (!CreateStringWatchBoxToolTipAndReplaceEscapes(onlineVarRef, stringBuilder))
					{
						stringBuilder.Append(stShortenedValueString);
					}
				}
				else if (!string.IsNullOrEmpty(stShortenedValueString))
				{
					stringBuilder.Append(stShortenedValueString);
				}
				return stringBuilder.ToString();
			}
			return string.Empty;
		}

		private static bool IsString(TypeClass tc)
		{
			if (tc == TypeClass.String || tc == TypeClass.WString)
			{
				return true;
			}
			return false;
		}

		private static bool IsValidVarRef(IOnlineVarRef onlineVarRef, IVarRef varRef)
		{
			if (onlineVarRef != null && onlineVarRef.Value != null && onlineVarRef.State == VarRefState.Good && varRef != null && varRef.WatchExpression != null && varRef.WatchExpression.Type != null && varRef.WatchExpression.Type.BaseType != null)
			{
				return true;
			}
			return false;
		}

		private static bool CreateStringWatchBoxToolTipAndReplaceEscapes(IOnlineVarRef onlineVarRef, StringBuilder sb)
		{
			string text = onlineVarRef.Value as string;
			string text2 = ReplaceEscapeSequences(text);
			if (text2 != text)
			{
				if (sb.Length > 0)
				{
					sb.Append(Environment.NewLine);
				}
				sb.Append(text2);
				return true;
			}
			return false;
		}

		private static string ReplaceEscapeSequences(string stValue)
		{
			return stValue.Replace("$\"", "\"").Replace("$N", "\n").Replace("$n", "\n")
				.Replace("$P", "\f")
				.Replace("$p", "\f")
				.Replace("$R", "\r")
				.Replace("$r", "\r")
				.Replace("$T", "\t")
				.Replace("$t", "\t")
				.Replace("$$", "$")
				.Replace("$'", "'");
		}

		private static void CreateEnumWatchBoxToolTip(IOnlineVarRef onlineVarRef, IVarRef varRef, StringBuilder sb)
		{
			ByteOrder byteOrder = GetByteOrder(varRef.ApplicationGuid);
			if (!APEnvironmentFacade.Instance.LanguageModelMgr.CanConvertRaw(onlineVarRef.RawValue, varRef.WatchExpression.Type.BaseType, varRef.ApplicationGuid, byteOrder))
			{
				return;
			}
			object obj = APEnvironmentFacade.Instance.LanguageModelMgr.ConvertRaw(onlineVarRef.RawValue, varRef.WatchExpression.Type.BaseType, varRef.ApplicationGuid, byteOrder);
			if (obj == null)
			{
				return;
			}
			string text = obj.ToString();
			if ((text.Contains("-") && long.TryParse(text, out var _)) || ulong.TryParse(text, out var _))
			{
				if (sb.Length > 0)
				{
					sb.Append(Environment.NewLine);
				}
				sb.Append(string.Format(Strings.EnumValueRepresentsRawValue, onlineVarRef.Value, text));
			}
		}

		public void SetToolTipCommentSizeCallback(ToolTipCommentSizeCallback callback)
		{
			_commentSizeCallback = callback;
		}

		public string ConvertReStructuredTextDocuCommentToPlainText(string stDocComment)
		{
			return ReStructuredTextParser.Instance.ConvertReStructuredTextDocuCommentToPlainText(stDocComment);
		}

		private static ByteOrder GetByteOrder(Guid guidApplication)
		{
			ByteOrder result = ByteOrder.Intel;
			ICompileContext referenceContextIfAvailable = APEnvironmentFacade.Instance.LanguageModelMgr.GetReferenceContextIfAvailable(guidApplication);
			if (referenceContextIfAvailable != null && referenceContextIfAvailable.Codegenerator != null)
			{
				result = (referenceContextIfAvailable.Codegenerator.MotorolaByteOrder ? ByteOrder.Motorola : ByteOrder.Intel);
			}
			return result;
		}

		private string GetInitialization(IExpression expression, string stType)
		{
			string result = string.Empty;
			if (expression != null)
			{
				if (expression is ILiteralExpression)
				{
					result = (expression as ILiteralExpression).ToString();
				}
				else if (expression is IStructureInitialization)
				{
					result = $"{stType} := (complex)";
				}
				else if (expression is IArrayInitialization)
				{
					result = $"{stType} := [complex]";
				}
				else if (expression is ICompoAccessExpression)
				{
					result = (expression as ICompoAccessExpression).ToString();
				}
			}
			return result;
		}
	}
}
