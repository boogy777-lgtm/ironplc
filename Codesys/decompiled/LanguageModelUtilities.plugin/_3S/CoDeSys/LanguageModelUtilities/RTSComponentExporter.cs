using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using _3S.CoDeSys.BuildCommands;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[TypeGuid("{003AF92B-6C4C-4687-B3B0-40AE7CD601FE}")]
	[SuppressMessage("Major Code Smell", "S1200:Classes should not be coupled to too many other classes", Justification = "Will be fixed with CDS-77832")]
	public class RTSComponentExporter : IRTSComponentExporter6, IRTSComponentExporter5, IRTSComponentExporter4, IRTSComponentExporter3, IRTSComponentExporter2, IRTSComponentExporter
	{
		[DebuggerDisplay("{Signature}.{Variable}")]
		private class VariableAndSignature
		{
			internal IVariable Variable { get; private set; }

			internal ISignature Signature { get; private set; }

			internal VariableAndSignature(IVariable var, ISignature sign)
			{
				Variable = var;
				Signature = sign;
			}
		}

		private static string ATTRIBUTE_M4EXPORT_NOSIGNATURE = "m4export_nosignature";

		private static string ATTRIBUTE_M4EXPORT_32BIT_NOSIGNATURE = "m4export_32bit_nosignature";

		private static string ATTRIBUTE_M4EXPORT_64BIT_NOSIGNATURE = "m4export_64bit_nosignature";

		private static string ATTRIBUTE_M4EXPORT_ENUM_AS_C = "m4export_enum-as-C";

		private static string ATTRIBUTE_M4EXPORT_ENUM_AS_C_OLD = "enum-as-C";

		internal const string ATTRIBUTE_M4EXPORT_ENUM_NO_PREFIX = "m4export_enum-no-prefix";

		private static string ATTRIBUTE_M4EXPORT_STRINGLEN_AS_VALUE = "m4export_stringlen-as-value";

		private static string ATTRIBUTE_M4EXPORT_STRINGLEN_AS_IS = "m4export_stringlen-as-is";

		private static string ATTRIBUTE_M4EXPORT_ARRAYLEN_AS_VALUE = "m4export_arraylen-as-value";

		private static string ATTRIBUTE_M4EXPORT_ARRAYLEN_AS_IS = "m4export_arraylen-as-is";

		private readonly LList<string> _additionalIncludes = new LList<string>();

		private ICompileContext _comcon;

		private AliasExporter _aliasExporter;

		private static string[] s_sTypeNameSubstringsToUseOriginalType = new string[4] { "PFSYS_TASK_FUNCTION", "PFSYS_TASK_EXCEPTIONHANDLER", "PFTIMERCALLBACK", "PFTIMEREXCEPTIONHANDLER" };

		private static string[] s_sWellKnownPointerTypes = new string[5] { "RTS_IEC_HANDLE", "PFSYS_TASK_FUNCTION", "PFSYS_TASK_EXCEPTIONHANDLER", "PFTIMERCALLBACK", "PFTIMEREXCEPTIONHANDLER" };

		private static string[] s_sWellKnownTypesForSpecialProcessing = new string[7] { "RTS_IEC_RESULT", "RTS_IEC_HANDLE", "XWORD", "PFSYS_TASK_FUNCTION", "PFSYS_TASK_EXCEPTIONHANDLER", "PFTIMERCALLBACK", "PFTIMEREXCEPTIONHANDLER" };

		private const string TAG_DESCRIPTION = " * <description>\r\n{0} * </description>";

		private const string TAG_ELEMENT = " * <element {0}>{1}</element>";

		private const string TAG_CATEGORY = " * <category>{0}</category>";

		private const string ATTRIBUTES_ELEMENT = "name=\"{0}\" type={1}";

		private const string ELEMENTTYPE_INOUT = "INOUT";

		private const string ELEMENTTYPE_IN = "IN";

		private const string ELEMENTTYPE_OUT = "OUT";

		private const string ELEMENTTYPE_LOCAL = "LOCAL";

		private const string CATETORY_STATIC_DEFINES = "Static defines";

		public string ProjectDirectory { get; set; }

		public string ProjectName { get; set; }

		public string Namespace { get; set; }

		public string Placeholder { get; set; }

		public bool GenerateM4 { get; set; }

		public bool GenerateC { get; set; }

		public bool GenerateTypeHeader { get; set; }

		public bool UseCAAGuidelines { get; set; }

		public bool ExportTopLevelPOUs { get; set; }

		public bool TypeLengthAsValue { get; set; }

		public bool UseOriginalTypeNames { get; set; }

		public bool ExportLibTypes { get; set; }

		public uint LibVersion { get; set; }

		public IMessageCategory MessageCategory { get; set; }

		private IList<string> AdditionalIncludes => (IList<string>)_additionalIncludes;

		public RTSComponentExporter()
		{
			ProjectDirectory = string.Empty;
			ProjectName = string.Empty;
			Namespace = string.Empty;
			Placeholder = string.Empty;
			GenerateM4 = true;
			GenerateC = false;
			GenerateTypeHeader = false;
			UseCAAGuidelines = false;
			TypeLengthAsValue = false;
			LibVersion = 0u;
		}

		public void SetAdditionalIncludes(IList<string> list)
		{
			_additionalIncludes.AddRange((IEnumerable<string>)list);
		}

		public IList<ISignature> FilterSignatures(ISignature[] signatures)
		{
			LList<ISignature> val = new LList<ISignature>();
			foreach (ISignature signature in signatures)
			{
				if (!IsSignatureToIgnore(signature))
				{
					val.Add(signature);
				}
			}
			return (IList<ISignature>)val;
		}

		private bool IsSignatureToIgnore(ISignature sign)
		{
			if (sign.LibraryPath != string.Empty && !ExportLibTypes)
			{
				return true;
			}
			if (sign.OrgName.Contains("__") && !sign.OrgName.Contains("__ARRAY__DIM__INFO"))
			{
				return true;
			}
			if (sign.GetFlag(SignatureFlag.SuperGlobal))
			{
				return true;
			}
			if (sign.GetFlag(SignatureFlag.TopLevel) && sign.POUType != Operator.Interface && sign.POUType != Operator.Function)
			{
				return true;
			}
			if (sign.HasAttribute(RTSExportAttributes.ATTRIBUTE_M4EXPORT_HIDE))
			{
				return true;
			}
			if (sign.Name.StartsWith("RTS_IEC_") && !string.IsNullOrEmpty(sign.LibraryPath))
			{
				return true;
			}
			return false;
		}

		[SuppressMessage("Critical Code Smell", "S3776:Cognitive Complexity of methods should not be too high", Justification = "Will be fixed with CDS-77832")]
		public void Run(ICompileContext comcon, ISignature[] signatures)
		{
			StreamWriter swM = null;
			StreamWriter swC = null;
			AuthFileStream fsM = null;
			AuthFileStream fsC = null;
			StringBuilder stringBuilder = new StringBuilder();
			StreamReader srOriginalM = null;
			AuthFileStream fsOriginalM = null;
			_comcon = comcon;
			_aliasExporter = new AliasExporter(this);
			LList<ISignature> alSigns = new LList<ISignature>((IEnumerable<ISignature>)signatures);
			alSigns = new ExportSignatureCollector().SortByDependencies((IList<ISignature>)alSigns, comcon, ExportLibTypes);
			signatures = alSigns.ToArray();
			if (GenerateM4)
			{
				CreateM4File(ProjectDirectory, ProjectName, out fsM, out swM, out fsOriginalM, out srOriginalM);
			}
			else if (GenerateTypeHeader)
			{
				CreateHFile(ProjectDirectory, ProjectName, out fsM, out swM);
			}
			if (GenerateC)
			{
				CreateCFile(ProjectDirectory, ProjectName, out fsC, out swC);
			}
			ISignature[] array = signatures;
			foreach (ISignature signature in array)
			{
				if (signature.POUType == Operator.VarGlobal && (signature.GetFlag(SignatureFlag.External) || signature.HasAttribute(RTSExportAttributes.ATTRIBUTE_M4EXPORT)) && !signature.GetFlag(SignatureFlag.Enum))
				{
					DumpConstants(swM, signature);
				}
			}
			array = signatures;
			foreach (ISignature signature2 in array)
			{
				if (!signature2.GetFlag(SignatureFlag.Structure) && !signature2.GetFlag(SignatureFlag.Union) && !signature2.GetFlag(SignatureFlag.Enum) && signature2.POUType == Operator.Interface)
				{
					if (IsInterfaceWrapperStructToCreate(signature2))
					{
						DumpInterfaceWrapperStruct(swM, signature2);
					}
					ISignature[] subSignatures = signature2.SubSignatures;
					foreach (ISignature sign in subSignatures)
					{
						DumpSignature(swM, swC, comcon, sign, signature2, UseCAAGuidelines, Namespace);
					}
				}
			}
			array = signatures;
			foreach (ISignature signature3 in array)
			{
				if (signature3.GetFlag(SignatureFlag.Enum))
				{
					DumpEnums(swM, comcon, signature3, null);
				}
			}
			array = signatures;
			foreach (ISignature signature4 in array)
			{
				if ((!signature4.GetFlag(SignatureFlag.Structure) && !signature4.GetFlag(SignatureFlag.Union)) || signature4.GetFlag(SignatureFlag.Enum))
				{
					continue;
				}
				DumpUserDefs(swM, swC, comcon, signature4, null);
				if (signature4.GetFlag(SignatureFlag.External))
				{
					ISignature[] subSignatures = signature4.SubSignatures;
					foreach (ISignature sign2 in subSignatures)
					{
						DumpSignature(swM, swC, comcon, sign2, signature4);
					}
				}
			}
			array = signatures;
			foreach (ISignature signature5 in array)
			{
				if (signature5.GetFlag(SignatureFlag.Structure) || signature5.GetFlag(SignatureFlag.Union) || signature5.POUType == Operator.VarGlobal || signature5.POUType == Operator.Interface)
				{
					continue;
				}
				DumpSignature(swM, swC, comcon, signature5, null);
				ISignature[] subSignatures = signature5.SubSignatures;
				foreach (ISignature signature6 in subSignatures)
				{
					if (signature6.GetFlag(SignatureFlag.External))
					{
						DumpSignature(swM, swC, comcon, signature6, signature5);
					}
				}
			}
			if (GenerateM4)
			{
				stringBuilder.Remove(0, stringBuilder.Length);
				stringBuilder.Append("#ifdef __cplusplus");
				stringBuilder.AppendLine();
				stringBuilder.Append("}");
				stringBuilder.AppendLine();
				stringBuilder.Append("#endif");
				stringBuilder.AppendLine();
				swM.Write(stringBuilder);
				if (srOriginalM != null)
				{
					swM.WriteLine();
					bool flag = false;
					while (!srOriginalM.EndOfStream)
					{
						string text = srOriginalM.ReadLine();
						if (text == "/** EXTERN LIB SECTION END **/")
						{
							swM.WriteLine(text);
							flag = true;
							break;
						}
					}
					while (flag && !srOriginalM.EndOfStream)
					{
						string text = srOriginalM.ReadLine();
						swM.WriteLine(text);
					}
					swM.Flush();
					swM.Close();
					srOriginalM.Close();
					AuthFile.Replace(((FileStream)(object)fsM).Name.ToString(), ((FileStream)(object)fsOriginalM).Name.ToString(), (string)null);
					IMessage message = new GenerateFilesMessage(string.Format(Strings.GenerateExtLibM4_Result, ((FileStream)(object)fsOriginalM).Name.ToString()), Guid.Empty, Severity.Text);
					APEnvironmentFacade.Instance.AddMessage(MessageCategory, message);
					if (AuthFile.Exists(((FileStream)(object)fsM).Name.ToString()))
					{
						AuthFile.Delete(((FileStream)(object)fsM).Name.ToString());
					}
				}
				else
				{
					swM.WriteLine();
					swM.WriteLine("/** EXTERN LIB SECTION END **/");
					swM.WriteLine();
					swM.Flush();
					swM.Close();
					IMessage message2 = new GenerateFilesMessage(string.Format(Strings.GenerateExtLibM4_Result, ((FileStream)(object)fsM).Name.ToString()), Guid.Empty, Severity.Text);
					APEnvironmentFacade.Instance.AddMessage(MessageCategory, message2);
				}
			}
			else if (GenerateTypeHeader)
			{
				stringBuilder.Remove(0, stringBuilder.Length);
				stringBuilder.Append("#ifdef __cplusplus");
				stringBuilder.AppendLine();
				stringBuilder.Append("}");
				stringBuilder.AppendLine();
				stringBuilder.Append("#endif");
				stringBuilder.AppendLine();
				swM.Write(stringBuilder);
				swM.WriteLine();
				swM.WriteLine("/** EXTERN LIB SECTION END **/");
				swM.WriteLine();
				swM.Flush();
				swM.Close();
			}
			if (GenerateC)
			{
				swC.Flush();
				swC.Close();
				IMessage message3 = new GenerateFilesMessage(string.Format(Strings.GenerateExtLibC_Result, ((FileStream)(object)fsC).Name.ToString()), Guid.Empty, Severity.Information);
				APEnvironmentFacade.Instance.AddMessage(MessageCategory, message3);
			}
		}

		private bool IsInterfaceWrapperStructToCreate(ISignature sign)
		{
			if (Operator.Interface != sign.POUType)
			{
				return false;
			}
			if (sign.GetFlag(SignatureFlag.External) || sign.GetFlag(SignatureFlag.TopLevel))
			{
				return true;
			}
			if (Array.Exists(sign.SubSignatures, (ISignature signSub) => signSub.GetFlag(SignatureFlag.External)))
			{
				return true;
			}
			if (sign.SubSignatures.Select((ISignature signSub) => _comcon.GetCompiledPOU(signSub.ObjectGuid)).Any((ICompiledPOU cpou) => cpou?.GetFlag(CompiledPOUFlags.TopLevel) ?? false))
			{
				return true;
			}
			return false;
		}

		internal string MapCAATypes(IVariable2 var)
		{
			string text = var.OriginalType.ToString();
			if (var.Type.Class == TypeClass.Pointer)
			{
				text = text.Replace("POINTER TO ", "");
			}
			if (text.StartsWith("CAA."))
			{
				return text.Replace('.', '_');
			}
			if (text == "BOOL")
			{
				return "CAA_BOOL";
			}
			if (text == "ERROR")
			{
				return "CAA_ERROR";
			}
			if (var.Type.Class == TypeClass.Enum)
			{
				return "CAA_ENUM";
			}
			if (IsXType(var.CompiledType))
			{
				return "RTS_IEC_" + GetXType(var.CompiledType);
			}
			return "RTS_IEC_" + text;
		}

		private void DumpInterfaceWrapperStruct(StreamWriter swM4, ISignature sign)
		{
			if (swM4 != null)
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.AppendLine("typedef struct");
				stringBuilder.AppendLine("{");
				stringBuilder.AppendLine("\tRTS_IEC_UXINT instance;");
				stringBuilder.AppendLine("} " + GetExternalUserDefType(sign, null) + ";");
				stringBuilder.AppendLine();
				stringBuilder.AppendLine();
				swM4.Write(stringBuilder);
			}
		}

		[SuppressMessage("Critical Code Smell", "S3776:Cognitive Complexity of methods should not be too high", Justification = "Will be fixed with CDS-77832")]
		private void DumpSignatureInterface(StreamWriter swM4, StreamWriter swC, ICompileContext comcon, ISignature sign, ISignature signParent, uint uiSignatureId32bit, uint uiSignatureId64bit, IScope2 scope)
		{
			StringBuilder stringBuilder = new StringBuilder();
			ICompiledPOU compiledPOU = comcon.GetCompiledPOU(sign.ObjectGuid);
			bool flag = compiledPOU != null && !compiledPOU.GetFlag(CompiledPOUFlags.TopLevel);
			if (swM4 != null && !GenerateTypeHeader)
			{
				string text = GetSignatureIDAsString(uiSignatureId32bit, uiSignatureId64bit).ToString();
				stringBuilder.Remove(0, stringBuilder.Length);
				Guid objectGuid;
				string stMsg;
				if (signParent != null && sign.POUType == Operator.Method)
				{
					string text2 = GetSignatureOrgName(signParent).ToLowerInvariant() + "__" + sign.Name.ToLowerInvariant().Replace("__", "");
					string argName = GetSignatureOrgName(signParent).ToLowerInvariant() + "_" + sign.Name.ToLowerInvariant().Replace("__", "") + "_struct";
					ExportApi(stringBuilder, flag, text2, argName, text);
					objectGuid = signParent.ObjectGuid;
					stMsg = string.Format(Strings.GenerateExtLib_Export, text2, text, LibVersion);
				}
				else
				{
					if (sign.POUType == Operator.FunctionBlock || sign.POUType == Operator.Type)
					{
						return;
					}
					string name = ((!sign.GetFlag(SignatureFlag.Structure)) ? GetSignatureOrgName(sign).ToLowerInvariant() : (GetSignatureOrgName(sign).ToLowerInvariant() + "__fb_init"));
					string argName2 = GetSignatureOrgName(sign).ToLowerInvariant() + "_struct";
					ExportApi(stringBuilder, flag, name, argName2, text);
					objectGuid = Guid.Empty;
					stMsg = string.Format(Strings.GenerateExtLib_Export, sign.Name.ToLowerInvariant(), text, LibVersion);
				}
				stringBuilder.AppendLine();
				stringBuilder.AppendLine();
				swM4.Write(stringBuilder);
				IMessage message = new GenerateFilesMessage(stMsg, objectGuid, Severity.Information);
				APEnvironmentFacade.Instance.AddMessage(MessageCategory, message);
			}
			if (!(swC != null && flag))
			{
				return;
			}
			stringBuilder.Remove(0, stringBuilder.Length);
			if (signParent != null && sign.POUType == Operator.Method)
			{
				stringBuilder.AppendFormat("void CDECL CDECL_EXT {0}__{1}({0}_{1}_struct *p)", GetSignatureOrgName(signParent).ToLowerInvariant(), GetSignatureName(sign).ToLowerInvariant().Replace("__", ""));
			}
			else
			{
				if (sign.POUType == Operator.FunctionBlock || sign.POUType == Operator.Type)
				{
					return;
				}
				stringBuilder.AppendFormat("void CDECL CDECL_EXT {0}({0}_struct *p)", GetSignatureOrgName(sign).ToLowerInvariant());
			}
			stringBuilder.AppendLine();
			swC.Write(stringBuilder);
			stringBuilder.Remove(0, stringBuilder.Length);
			stringBuilder.Append("{");
			if (UseCAAGuidelines)
			{
				stringBuilder.AppendLine();
				stringBuilder.Append("\t");
				swC.Write(stringBuilder);
				DumpCAACall(swC, sign, signParent, scope);
				stringBuilder.Remove(0, stringBuilder.Length);
				stringBuilder.AppendLine();
			}
			else
			{
				IEnumerable<IGenerateExtCodeProvider> enumerable = APEnvironmentFacade.Instance.CreateGenerateExtCodeProviders();
				bool flag2 = false;
				foreach (IGenerateExtCodeProvider item in enumerable)
				{
					if (item.HasToGenerateCodeForSignature(sign))
					{
						stringBuilder.AppendLine();
						swC.Write(stringBuilder);
						string value = item.GenerateCodeForSignature(sign);
						swC.WriteLine(value);
						stringBuilder.Remove(0, stringBuilder.Length);
						flag2 = true;
					}
				}
				if (!enumerable.Any() || !flag2)
				{
					stringBuilder.AppendLine();
					stringBuilder.AppendLine();
				}
			}
			stringBuilder.Append("}");
			stringBuilder.AppendLine();
			stringBuilder.AppendLine();
			swC.Write(stringBuilder);
		}

		private void ExportApi(StringBuilder st, bool exportDef, string name, string argName, string signatureIdAsString)
		{
			if (exportDef)
			{
				st.AppendFormat("DEF_API(`void',`CDECL',`{0}',`({1} *p)',1,{2},0x{3:X8})", name, argName, signatureIdAsString, LibVersion);
			}
		}

		private string GetSignatureOrgName(ISignature sign)
		{
			sign = GetSignatureIfInterfaceUnion(sign);
			if (sign.HasAttribute(CompileAttributes.ATTRIBUTE_EXTERNAL_NAME))
			{
				CheckExternalNameAttributeValue(sign);
				return sign.GetAttributeValue(CompileAttributes.ATTRIBUTE_EXTERNAL_NAME);
			}
			return sign.OrgName;
		}

		private string GetSignatureName(ISignature sign)
		{
			sign = GetSignatureIfInterfaceUnion(sign);
			if (sign.HasAttribute(CompileAttributes.ATTRIBUTE_EXTERNAL_NAME))
			{
				CheckExternalNameAttributeValue(sign);
				return sign.GetAttributeValue(CompileAttributes.ATTRIBUTE_EXTERNAL_NAME);
			}
			return sign.Name;
		}

		private void CheckExternalNameAttributeValue(ISignature sign)
		{
			if (sign.HasAttribute(CompileAttributes.ATTRIBUTE_EXTERNAL_NAME) && sign.GetAttributeValue(CompileAttributes.ATTRIBUTE_EXTERNAL_NAME).Contains("__"))
			{
				IMessage message = new GenerateFilesMessage(Strings.WrnNoDoubleUnderscoresForAttributeExternalName, sign.MessageGuid, Severity.Warning);
				APEnvironmentFacade.Instance.AddMessage(MessageCategory, message);
			}
		}

		internal ISignature GetSignatureIfInterfaceUnion(ISignature sign)
		{
			if (sign.GetFlag(SignatureFlag.ImplicitInterfaceUnion))
			{
				IVariable variable = sign["__Interface"];
				if (variable != null && variable.CompiledType.BaseType is IUserdefType userdefType)
				{
					sign = _comcon.CreateGlobalIScope()[userdefType.SignatureId];
				}
			}
			return sign;
		}

		private StringBuilder GetSignatureIDAsString(uint uiSignatureId32bit, uint uiSignatureId64bit)
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (uiSignatureId32bit == uiSignatureId64bit)
			{
				if (uiSignatureId32bit == 0)
				{
					stringBuilder.Append("0");
				}
				else
				{
					stringBuilder.AppendFormat("0x{0:X8}", uiSignatureId32bit);
				}
			}
			else if (uiSignatureId32bit == 0)
			{
				stringBuilder.AppendFormat("RTSITF_GET_SIGNATURE(0, 0x{0:X8})", uiSignatureId64bit);
			}
			else if (uiSignatureId64bit == 0)
			{
				stringBuilder.AppendFormat("RTSITF_GET_SIGNATURE(0x{0:X8}, 0)", uiSignatureId32bit);
			}
			else
			{
				stringBuilder.AppendFormat("RTSITF_GET_SIGNATURE(0x{0:X8}, 0x{1:X8})", uiSignatureId32bit, uiSignatureId64bit);
			}
			return stringBuilder;
		}

		private void DumpInterface(StreamWriter swM4, ISignature sign, ISignature signParent, uint uiSignatureId32bit, uint uiSignatureId64bit, IScope2 scope)
		{
			if (swM4 != null && !sign.GetFlag(SignatureFlag.External))
			{
				StringBuilder stringBuilder = new StringBuilder();
				string stMsg = string.Format(Strings.GenerateExtLib_Export, GetSignatureName(sign).ToString(), GetSignatureIDAsString(uiSignatureId32bit, uiSignatureId64bit), LibVersion);
				stringBuilder.AppendFormat("DEF_ITF_API(`void',`CDECL',`{0}',`({1}_{2}_struct *p)')", GetSignatureOrgName(sign).ToString(), GetSignatureName(signParent).ToLowerInvariant(), GetSignatureName(sign).ToLowerInvariant());
				IMessage message = new GenerateFilesMessage(stMsg, sign.ObjectGuid, Severity.Information);
				APEnvironmentFacade.Instance.AddMessage(MessageCategory, message);
				stringBuilder.AppendLine();
				stringBuilder.AppendLine();
				swM4.Write(stringBuilder);
			}
		}

		private void CreateM4File(string stPath, string stName, out AuthFileStream fsM4, out StreamWriter swM4, out AuthFileStream fsOriginalM4, out StreamReader srOriginalM4)
		{
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Expected O, but got Unknown
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			//IL_006b: Expected O, but got Unknown
			//IL_00db: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e1: Expected O, but got Unknown
			fsOriginalM4 = null;
			srOriginalM4 = null;
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendFormat("{0}\\{1}Itf.m4", stPath, stName);
			if (AuthFile.Exists(stringBuilder.ToString()))
			{
				fsM4 = new AuthFileStream(stringBuilder.ToString(), FileMode.Open, FileAccess.Read);
				fsOriginalM4 = fsM4;
				stringBuilder.Remove(0, stringBuilder.Length);
				stringBuilder.AppendFormat("{0}\\{1}Itf_.m4", stPath, stName);
				fsM4 = new AuthFileStream(stringBuilder.ToString(), FileMode.Create, FileAccess.Write);
				srOriginalM4 = new StreamReader((Stream)(object)fsOriginalM4);
				swM4 = new StreamWriter((Stream)(object)fsM4);
				while (!srOriginalM4.EndOfStream)
				{
					string text = srOriginalM4.ReadLine();
					swM4.WriteLine(text);
					if (text == "/** EXTERN LIB SECTION BEGIN **/")
					{
						swM4.WriteLine("/*  Comments are ignored for m4 compiler so restructured text can be used. changecom(`/*', `*/') */");
						swM4.WriteLine();
						DumpAdditionalIncludes(swM4);
						break;
					}
				}
			}
			else
			{
				fsM4 = new AuthFileStream(stringBuilder.ToString(), FileMode.Create);
				swM4 = new StreamWriter((Stream)(object)fsM4);
				stringBuilder.Remove(0, stringBuilder.Length);
				stringBuilder.Append("/**");
				stringBuilder.AppendLine();
				stringBuilder.AppendFormat(" * <interfacename>{0}</interfacename>", stName);
				stringBuilder.AppendLine();
				stringBuilder.Append(" * <description></description>");
				stringBuilder.AppendLine();
				stringBuilder.Append(" *");
				stringBuilder.AppendLine();
				stringBuilder.Append(" * <copyright></copyright>");
				stringBuilder.AppendLine();
				stringBuilder.Append(" */");
				stringBuilder.AppendLine();
				stringBuilder.AppendLine();
				swM4.Write(stringBuilder);
				stringBuilder.Remove(0, stringBuilder.Length);
				stringBuilder.AppendFormat("SET_INTERFACE_NAME(`{0}')", stName);
				if (!string.IsNullOrEmpty(Placeholder))
				{
					stringBuilder.AppendLine();
					stringBuilder.AppendFormat("SET_PLACEHOLDER_NAME(`{0}')", Placeholder);
				}
				stringBuilder.AppendLine();
				stringBuilder.AppendLine();
				stringBuilder.AppendLine("/** EXTERN LIB SECTION BEGIN **/");
				stringBuilder.AppendLine("/*  Comments are ignored for m4 compiler so restructured text can be used. changecom(`/*', `*/') */");
				stringBuilder.AppendLine();
				swM4.Write(stringBuilder);
				DumpAdditionalIncludes(swM4);
			}
			stringBuilder.Remove(0, stringBuilder.Length);
			stringBuilder.Append("#ifdef __cplusplus");
			stringBuilder.AppendLine();
			stringBuilder.Append("extern \"C\" {");
			stringBuilder.AppendLine();
			stringBuilder.Append("#endif");
			stringBuilder.AppendLine();
			stringBuilder.AppendLine();
			swM4.Write(stringBuilder);
		}

		private void CreateHFile(string stPath, string stName, out AuthFileStream fsM4, out StreamWriter swM4)
		{
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Expected O, but got Unknown
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendFormat("{0}\\{1}Itf.h", stPath, stName);
			fsM4 = new AuthFileStream(stringBuilder.ToString(), FileMode.Create);
			swM4 = new StreamWriter((Stream)(object)fsM4);
			stringBuilder.Remove(0, stringBuilder.Length);
			stringBuilder.Append("/**");
			stringBuilder.AppendLine();
			stringBuilder.AppendFormat(" * <interfacename>{0}</interfacename>", stName);
			stringBuilder.AppendLine();
			stringBuilder.Append(" * <description></description>");
			stringBuilder.AppendLine();
			stringBuilder.Append(" *");
			stringBuilder.AppendLine();
			stringBuilder.Append(" * <copyright></copyright>");
			stringBuilder.AppendLine();
			stringBuilder.Append(" */");
			stringBuilder.AppendLine();
			stringBuilder.AppendLine();
			swM4.Write(stringBuilder);
			stringBuilder.Remove(0, stringBuilder.Length);
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("/** EXTERN LIB SECTION BEGIN **/");
			stringBuilder.AppendLine();
			swM4.Write(stringBuilder);
			stringBuilder.Remove(0, stringBuilder.Length);
			stringBuilder.Append("#ifdef __cplusplus");
			stringBuilder.AppendLine();
			stringBuilder.Append("extern \"C\" {");
			stringBuilder.AppendLine();
			stringBuilder.Append("#endif");
			stringBuilder.AppendLine();
			stringBuilder.AppendLine();
			swM4.Write(stringBuilder);
		}

		private void DumpAdditionalIncludes(StreamWriter swM4)
		{
			foreach (string additionalInclude in AdditionalIncludes)
			{
				swM4.WriteLine("#include \"{0}\"", additionalInclude);
			}
		}

		private void CreateCFile(string stPath, string stName, out AuthFileStream fsC, out StreamWriter swC)
		{
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Expected O, but got Unknown
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Expected O, but got Unknown
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_0067: Expected O, but got Unknown
			fsC = null;
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendFormat("{0}\\{1}.c", stPath, stName);
			try
			{
				fsC = new AuthFileStream(stringBuilder.ToString(), FileMode.Open, FileAccess.Read);
			}
			catch
			{
			}
			if (fsC != null)
			{
				((Stream)(object)fsC).Close();
				fsC = new AuthFileStream(stringBuilder.ToString(), FileMode.Append, FileAccess.Write);
				swC = new StreamWriter((Stream)(object)fsC);
				swC.WriteLine();
				return;
			}
			fsC = new AuthFileStream(stringBuilder.ToString(), FileMode.Create, FileAccess.Write);
			swC = new StreamWriter((Stream)(object)fsC);
			stringBuilder.Remove(0, stringBuilder.Length);
			stringBuilder.Append("#include \"CmpStd.h\"");
			stringBuilder.AppendLine();
			stringBuilder.Append("#include \"CmpErrors.h\"");
			stringBuilder.AppendLine();
			stringBuilder.Append("#include \"CmpItf.h\"");
			stringBuilder.AppendLine();
			stringBuilder.AppendFormat("#include \"{0}Dep.h\"", stName);
			stringBuilder.AppendLine();
			stringBuilder.AppendLine();
			swC.Write(stringBuilder);
		}

		private bool IsVariableConstant(IVariable var, ISignature sign)
		{
			if (var.HasAttribute(RTSExportAttributes.ATTRIBUTE_M4EXPORT_HIDE))
			{
				return false;
			}
			if (!var.HasFlag(VarFlag.ReplacedConstant | VarFlag.Constant))
			{
				return false;
			}
			return true;
		}

		private void DumpConstants(StreamWriter swM4, ISignature sign)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("/**");
			stringBuilder.AppendLine();
			stringBuilder.Append(FormatCategory("Static defines"));
			stringBuilder.AppendLine();
			stringBuilder.Append(FormatDescription(GetSignatureComment(sign, null, bGlobal: true)));
			stringBuilder.AppendLine();
			string value = FormatElements(sign, bUpperCase: false, string.Empty, IsVariableConstant);
			if (!string.IsNullOrEmpty(value))
			{
				stringBuilder.Append(value);
			}
			stringBuilder.Append(" */");
			stringBuilder.AppendLine();
			IVariable[] vars = GetVars(sign);
			for (int i = 0; i < vars.Length; i++)
			{
				IVariable2 variable = (IVariable2)vars[i];
				if (IsVariableConstant(variable, sign))
				{
					StringBuilder initialValue = GetInitialValue(variable);
					stringBuilder.AppendFormat("#define {0}\t\t{1}", variable.OrgName.ToString().ToUpper(), initialValue.ToString());
					stringBuilder.AppendLine();
				}
			}
			stringBuilder.AppendLine();
			swM4.Write(stringBuilder);
		}

		private void DumpEnums(StreamWriter swM4, ICompileContext comcon, ISignature sign, ISignature signParent)
		{
			DumpEnums(swM4, comcon, sign, signParent, bUseCAAGuidelines: false, "");
		}

		private void DumpEnums(StreamWriter swM4, ICompileContext comcon, ISignature sign, ISignature signParent, bool bUseCAAGuidelines, string stNamespace)
		{
			IScope2 scope = null;
			scope = ((signParent == null) ? (comcon.CreateIScope(sign.Id) as IScope2) : (comcon.CreateIScope(signParent.Id, sign.Id) as IScope2));
			if (!sign.OrgName.Contains("__") && !(sign.Name.ToString() == "VERSION") && !sign.HasAttribute(RTSExportAttributes.ATTRIBUTE_C_SOURCE_EXPORT))
			{
				DumpTypes(swM4, sign, signParent, scope, bGlobal: true, bUseCAAGuidelines, stNamespace);
			}
		}

		private void DumpUserDefs(StreamWriter swM4, StreamWriter swC, ICompileContext comcon, ISignature sign, ISignature signParent)
		{
			IScope2 scope = null;
			scope = ((signParent == null) ? (comcon.CreateIScope(sign.Id) as IScope2) : (comcon.CreateIScope(signParent.Id, sign.Id) as IScope2));
			if (sign.OrgName.Contains("__"))
			{
				if (sign.OrgName == "__ARRAY__DIM__INFO")
				{
					DumpInternalType(swM4, sign, scope, UseCAAGuidelines, Namespace);
				}
			}
			else if (!(sign.Name.ToString() == "VERSION") && !sign.HasAttribute(RTSExportAttributes.ATTRIBUTE_C_SOURCE_EXPORT))
			{
				DumpTypes(swM4, sign, signParent, scope, bGlobal: true);
				if (sign.GetFlag(SignatureFlag.External) || sign.HasAttribute(RTSExportAttributes.ATTRIBUTE_M4EXPORT))
				{
					DumpTypes(swM4, sign, signParent, scope, bGlobal: false, UseCAAGuidelines, Namespace);
				}
			}
		}

		private string GetElementType(ISignature sign, IVariable var)
		{
			if (IsEnumeration(sign) || IsConstant(sign, var))
			{
				return "OUT";
			}
			if (IsType(sign))
			{
				return "INOUT";
			}
			if (var.GetFlag(VarFlag.Input))
			{
				return "IN";
			}
			if (var.GetFlag(VarFlag.Output))
			{
				return "OUT";
			}
			if (var.GetFlag(VarFlag.Inout))
			{
				return "INOUT";
			}
			if (var.GetFlag(VarFlag.Local))
			{
				return "LOCAL";
			}
			return string.Empty;
		}

		private void DumpStructureComment(StreamWriter swM4, ISignature sign, ISignature signParent, bool bGlobal)
		{
			if (swM4 != null)
			{
				string signatureComment = GetSignatureComment(sign, signParent, bGlobal);
				string value = FormatDescription(signatureComment);
				string value2 = FormatElements(sign, bUpperCase: false, string.Empty, IncludeVariable);
				swM4.Write("/**");
				swM4.Write(Environment.NewLine);
				swM4.Write(value);
				swM4.Write(Environment.NewLine);
				if (!string.IsNullOrEmpty(value2))
				{
					swM4.Write(value2);
				}
				swM4.Write("*/" + Environment.NewLine);
			}
		}

		private void DumpEnumerationComment(StreamWriter swM4, ISignature sign, ISignature signParent, bool bGlobal)
		{
			if (swM4 != null)
			{
				string signatureComment = GetSignatureComment(sign, signParent, bGlobal);
				string value = FormatDescription(signatureComment);
				string stPrefix = string.Empty;
				if (!sign.HasAttribute("m4export_enum-no-prefix"))
				{
					stPrefix = GetSignatureOrgName(sign).ToUpper();
				}
				string value2 = FormatElements(sign, bUpperCase: true, stPrefix, IncludeVariable);
				swM4.Write("/**");
				swM4.Write(Environment.NewLine);
				swM4.Write(value);
				swM4.Write(Environment.NewLine);
				if (!string.IsNullOrEmpty(value2))
				{
					swM4.Write(value2);
				}
				swM4.Write("*/" + Environment.NewLine);
			}
		}

		private bool IncludeVariable(IVariable var, ISignature sign)
		{
			if (var.HasFlag(VarFlag.Implicit) && var.Name == "__VFTABLEPOINTER")
			{
				return false;
			}
			if (var.HasFlag(VarFlag.Implicit) && var.Name.StartsWith("__INTERFACEPOINTER__"))
			{
				return false;
			}
			if (sign.POUType == Operator.Function && var.GetFlag(VarFlag.Local))
			{
				return false;
			}
			if (var.GetFlag(VarFlag.Static))
			{
				return false;
			}
			return true;
		}

		private string FormatElements(ISignature sign, bool bUpperCase, string stPrefix, Func<IVariable, ISignature, bool> Select)
		{
			string text = string.Empty;
			IVariable[] all = sign.All;
			foreach (IVariable variable in all)
			{
				if (Select == null || Select(variable, sign))
				{
					string text2 = string.Empty;
					if (!string.IsNullOrEmpty(stPrefix))
					{
						text2 = stPrefix + "_";
					}
					text2 += variable.OrgName;
					string elementType = GetElementType(sign, variable);
					string arg = $"name=\"{text2}\" type={elementType}";
					text = text + $" * <element {arg}>{GetVariableComment(variable)}</element>" + Environment.NewLine;
				}
			}
			return text;
		}

		private string GetSignatureComment(ISignature sign, ISignature signParent, bool bGlobal)
		{
			string text = sign.GetAttributeValue("''DOCU__COMMENT");
			if (text == null || text == string.Empty)
			{
				text = sign.GetAttributeValue("''NORMAL__COMMENT");
			}
			if (text == null || text == string.Empty)
			{
				text = GetDefaultComment(sign, signParent, bGlobal);
			}
			return text;
		}

		private string GetVariableComment(IVariable var)
		{
			if (var.Comment != null)
			{
				return var.Comment.Trim('\t', ' ');
			}
			return string.Empty;
		}

		private string FormatDescription(string stComment)
		{
			_ = string.Empty;
			StringReader stringReader = new StringReader(stComment);
			StringBuilder stringBuilder = new StringBuilder();
			for (string text = stringReader.ReadLine(); text != null; text = stringReader.ReadLine())
			{
				stringBuilder.AppendFormat(" * {0}", text.Trim(' '));
				stringBuilder.AppendLine();
			}
			return $" * <description>\r\n{stringBuilder} * </description>";
		}

		private string FormatCategory(string stCategory)
		{
			return $" * <category>{stCategory}</category>";
		}

		private bool IsStructure(ISignature sign)
		{
			return sign.GetFlag(SignatureFlag.Structure);
		}

		private bool IsEnumeration(ISignature sign)
		{
			return sign.GetFlag(SignatureFlag.Enum);
		}

		private bool IsUnion(ISignature sign)
		{
			return sign.GetFlag(SignatureFlag.Union);
		}

		private bool IsType(ISignature sign)
		{
			if (!IsStructure(sign) && !IsEnumeration(sign))
			{
				return IsUnion(sign);
			}
			return true;
		}

		private bool IsConstant(ISignature sign, IVariable var)
		{
			if (sign.POUType == Operator.VarGlobal && !sign.GetFlag(SignatureFlag.Enum))
			{
				return IsVariableConstant(var, sign);
			}
			return false;
		}

		private bool DumpComment(StreamWriter swM4, ISignature sign, ISignature signParent, bool bGlobal)
		{
			if (IsEnumeration(sign))
			{
				DumpEnumerationComment(swM4, sign, signParent, bGlobal);
				return true;
			}
			if (IsStructure(sign) || IsUnion(sign) || sign.POUType == Operator.Function)
			{
				DumpStructureComment(swM4, sign, signParent, bGlobal);
				return true;
			}
			if (swM4 != null)
			{
				StringBuilder stringBuilder = new StringBuilder();
				string attributeValue = sign.GetAttributeValue("''DOCU__COMMENT");
				if (attributeValue == null || attributeValue == string.Empty)
				{
					attributeValue = sign.GetAttributeValue("''NORMAL__COMMENT");
				}
				if (attributeValue != null && attributeValue != string.Empty)
				{
					stringBuilder.Remove(0, stringBuilder.Length);
					stringBuilder.Append("/**");
					stringBuilder.AppendLine();
					StringReader stringReader = new StringReader(attributeValue);
					for (string text = stringReader.ReadLine(); text != null; text = stringReader.ReadLine())
					{
						stringBuilder.AppendFormat(" *{0}", text);
						stringBuilder.AppendLine();
					}
					stringBuilder.Append(" */");
					stringBuilder.AppendLine();
					swM4.Write(stringBuilder);
					return true;
				}
			}
			return false;
		}

		private void DumpSignature(StreamWriter swM4, StreamWriter swC, ICompileContext comcon, ISignature sign, ISignature signParent)
		{
			DumpSignature(swM4, swC, comcon, sign, signParent, bUseCAAGuidlines: false, "");
		}

		private void DumpSignature(StreamWriter swM4, StreamWriter swC, ICompileContext comcon, ISignature sign, ISignature signParent, bool bUseCAAGuidlines, string stNamespace)
		{
			IScope2 scope = null;
			scope = ((signParent == null) ? (comcon.CreateIScope(sign.Id) as IScope2) : (comcon.CreateIScope(signParent.Id, sign.Id) as IScope2));
			if (sign.OrgName.Contains("__") && !sign.OrgName.ToLower().Contains("__main") && !sign.OrgName.ToLower().Contains("__get") && !sign.OrgName.ToLower().Contains("__set"))
			{
				return;
			}
			string attributeValue = sign.GetAttributeValue(CompileAttributes.ATTRIBUTE_SIGNATURE_CRC);
			uint uiSignatureId32bit = 0u;
			if (attributeValue != null)
			{
				uiSignatureId32bit = uint.Parse(attributeValue);
			}
			attributeValue = sign.GetAttributeValue(CompileAttributes.ATTRIBUTE_SIGNATURE_CRC_64);
			uint uiSignatureId64bit = 0u;
			if (attributeValue != null)
			{
				uiSignatureId64bit = uint.Parse(attributeValue);
			}
			ISignature signature = sign;
			if (sign.OrgName.ToLower().Contains("__main") && signParent != null && signParent.POUType == Operator.FunctionBlock)
			{
				signature = signParent;
			}
			if (signature.HasAttribute(ATTRIBUTE_M4EXPORT_NOSIGNATURE))
			{
				uiSignatureId32bit = 0u;
				uiSignatureId64bit = 0u;
			}
			if (signature.HasAttribute(ATTRIBUTE_M4EXPORT_32BIT_NOSIGNATURE))
			{
				uiSignatureId32bit = 0u;
			}
			if (signature.HasAttribute(ATTRIBUTE_M4EXPORT_64BIT_NOSIGNATURE))
			{
				uiSignatureId64bit = 0u;
			}
			if (signParent != null && signParent.POUType == Operator.Interface)
			{
				ICompiledPOU compiledPOU = _comcon.GetCompiledPOU(sign.ObjectGuid);
				bool flag = false;
				if (compiledPOU != null)
				{
					flag = compiledPOU.GetFlag(CompiledPOUFlags.TopLevel);
				}
				if (sign.GetFlag(SignatureFlag.External) || flag)
				{
					DumpTypes(swM4, sign, signParent, scope, bGlobal: false, bUseCAAGuidlines, stNamespace);
					if (!sign.HasAttribute(RTSExportAttributes.ATTRIBUTE_M4EXPORT_HIDE))
					{
						DumpInterface(swM4, sign, signParent, uiSignatureId32bit, uiSignatureId64bit, scope);
					}
				}
			}
			else
			{
				DumpTypes(swM4, sign, signParent, scope, bGlobal: false, bUseCAAGuidlines, stNamespace);
				DumpSignatureInterface(swM4, swC, comcon, sign, signParent, uiSignatureId32bit, uiSignatureId64bit, scope);
			}
		}

		private IVariable[] HelpCollectInputs(ISignature sign)
		{
			IVariable[] allInputs = sign.AllInputs;
			IVariable[] array = new IVariable[allInputs.Length];
			IVariable variable = sign["__INSTANCEPOINTER"];
			if (variable == null)
			{
				return allInputs;
			}
			array[0] = variable;
			int num = 0;
			int num2 = 1;
			while (num2 < array.Length)
			{
				if (allInputs[num] == variable)
				{
					num++;
				}
				array[num2] = allInputs[num];
				num2++;
				num++;
			}
			return array;
		}

		private void DumpCAACall(StreamWriter swC, ISignature sign, ISignature signParent, IScope2 scope)
		{
			if (swC == null)
			{
				return;
			}
			StringBuilder stringBuilder = new StringBuilder();
			IVariable[] array2;
			if (sign.POUType == Operator.Function || sign.POUType == Operator.Method)
			{
				IVariable[] array = HelpCollectInputs(sign);
				array2 = new IVariable[array.Length];
				array.CopyTo(array2, 0);
				stringBuilder.AppendFormat("p->{0} = ", sign.Outputs[0].OrgName);
			}
			else
			{
				array2 = sign.All;
			}
			stringBuilder.AppendFormat("CAL_{0}_{1}(", Namespace, sign.OrgName);
			swC.Write(stringBuilder);
			bool flag = true;
			IVariable[] array3 = array2;
			for (int i = 0; i < array3.Length; i++)
			{
				IVariable2 var = (IVariable2)array3[i];
				if (flag)
				{
					flag = false;
				}
				else
				{
					stringBuilder.Remove(0, stringBuilder.Length);
					stringBuilder.Append(", ");
					swC.Write(stringBuilder);
				}
				DumpCAACallElement(swC, var, sign, scope);
			}
			stringBuilder.Remove(0, stringBuilder.Length);
			stringBuilder.Append(");");
			swC.Write(stringBuilder);
		}

		private void DumpCAACallElement(StreamWriter swC, IVariable2 var, ISignature sign, IScope2 scope)
		{
			if (swC == null || (sign.POUType == Operator.Function && var.GetFlag(VarFlag.Local)))
			{
				return;
			}
			ICompiledType compiledType = var.CompiledType;
			StringBuilder stringBuilder = new StringBuilder();
			StringBuilder stringBuilder2 = new StringBuilder();
			ICompiledType compiledType2 = compiledType;
			while (compiledType2.Class == TypeClass.Pointer || compiledType2.Class == TypeClass.Reference)
			{
				stringBuilder.Append("&");
				compiledType2 = compiledType2.BaseType;
			}
			if (stringBuilder.ToString() == "" && var.GetFlag(VarFlag.Inout))
			{
				stringBuilder.Append("&");
			}
			if (compiledType2.ToString().Contains("RTS_IEC_"))
			{
				if (stringBuilder.ToString() != "")
				{
					stringBuilder2.AppendFormat("{0}(p->{1})", stringBuilder, var.OrgName);
				}
				else
				{
					stringBuilder2.AppendFormat("p->{0}", var.OrgName);
				}
			}
			else if (stringBuilder.ToString() != "")
			{
				stringBuilder2.AppendFormat("{0}(p->{1})", stringBuilder, var.OrgName);
			}
			else
			{
				stringBuilder2.AppendFormat("p->{0}", var.OrgName);
			}
			swC.Write(stringBuilder2);
		}

		private void DumpTypes(StreamWriter swM4, ISignature sign, ISignature signParent, IScope2 scope, bool bGlobal)
		{
			DumpTypes(swM4, sign, signParent, scope, bGlobal, bUseCAAGuidelines: false, "");
		}

		private static IEnumerable<string> GetImplicitInterfacePointerVariableNames(ISignature sign, ref int iIdxItfPointerVar)
		{
			List<string> list = new List<string>();
			IVariable[] all = sign.All;
			foreach (IVariable variable in all)
			{
				if (variable.HasFlag(VarFlag.Implicit) && variable.Name.StartsWith("__INTERFACEPOINTER__") && !variable.HasAttribute(CompileAttributes.ATTRIBUTE_USELOCATION) && !string.IsNullOrEmpty(variable.Comment))
				{
					list.Add($"ITF{iIdxItfPointerVar}");
					iIdxItfPointerVar++;
				}
			}
			return list;
		}

		private string GetBaseStructMember(ISignature sign)
		{
			ISignature signatureById = _comcon.GetSignatureById(sign.BaseSignatureId);
			if (signatureById != null)
			{
				return "\t" + GetSignatureOrgName(signatureById).ToLowerInvariant() + "_struct  __BASE;";
			}
			return string.Empty;
		}

		private void DumpInstanceMember(ISignature sign, StreamWriter swM4, IScope2 scope, bool bUseCAAGuidelines, string stNamespace, ref int iIdxItfPointerVar)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(GetBaseStructMember(sign));
			stringBuilder.AppendLine();
			stringBuilder.AppendFormat("\t/* Member variables of {0} */", GetSignatureOrgName(sign));
			stringBuilder.AppendLine();
			try
			{
				foreach (string implicitInterfacePointerVariableName in GetImplicitInterfacePointerVariableNames(sign, ref iIdxItfPointerVar))
				{
					stringBuilder.AppendFormat("\tRTS_IEC_BYTE *__p{0};", implicitInterfacePointerVariableName);
					AppendTabs(stringBuilder);
					stringBuilder.AppendLine("/* Pointer to interface */");
				}
				stringBuilder.AppendLine();
				swM4.Write(stringBuilder);
			}
			catch
			{
			}
			DumpTypeElements(sign, swM4, scope, bUseCAAGuidelines, stNamespace);
		}

		private void DumpTypeElementEnum(StringBuilder st, ISignature sign, IVariable2 var, StreamWriter swM4, bool bUseCAAGuidelines, string stNamespace, ref int iEnumLines)
		{
			if (iEnumLines > 0)
			{
				st.Remove(0, st.Length);
				st.Append(",");
				st.AppendLine();
				swM4.Write(st);
			}
			if (bUseCAAGuidelines)
			{
				DumpCAAEnumElement(swM4, var, sign, stNamespace);
			}
			else
			{
				DumpEnumElement(swM4, var, sign);
			}
			iEnumLines++;
		}

		private int DumpTypeElements(ISignature sign, StreamWriter swM4, IScope2 scope, bool bUseCAAGuidelines, string stNamespace)
		{
			StringBuilder stringBuilder = new StringBuilder();
			int iEnumLines = 0;
			VariableAndSignature[] array = (from var in GetVarsWithDeclaringSignature(sign)
				where var.Variable is IVariable2 && IncludeVariable(var.Variable, sign)
				select var).ToArray();
			HashSet<string> hashSet = new HashSet<string>();
			Enumerable.AddRange<string>((ICollection<string>)hashSet, from varAndSign in array
				select varAndSign.Variable into var
				where var.HasAttribute("anytypeclass")
				select var.Name);
			if (Operator.Type == sign.POUType)
			{
				string baseStructMember = GetBaseStructMember(sign);
				if (!string.IsNullOrEmpty(baseStructMember))
				{
					swM4.Write(baseStructMember);
					swM4.Write("\r\n");
				}
			}
			for (int i = 0; i < array.Length; i++)
			{
				IVariable2 var2 = array[i].Variable as IVariable2;
				if (sign.GetFlag(SignatureFlag.Enum))
				{
					DumpTypeElementEnum(stringBuilder, sign, var2, swM4, bUseCAAGuidelines, stNamespace, ref iEnumLines);
				}
				else if (!IsImplicitAnyTypeVar(hashSet, var2))
				{
					if (bUseCAAGuidelines)
					{
						DumpCAAElement(swM4, var2, sign, scope, stNamespace);
					}
					else
					{
						DumpElement(swM4, var2, sign, scope);
					}
				}
			}
			if (sign.POUType == Operator.Function && array.Length < 1)
			{
				stringBuilder.AppendLine("\tRTS_IEC_WORD dummy; /* void function(void) */");
				swM4.Write(stringBuilder);
			}
			return iEnumLines;
		}

		private bool IsImplicitAnyTypeVar(HashSet<string> hsAnyVariables, IVariable2 var)
		{
			if (hsAnyVariables.Count == 0)
			{
				return false;
			}
			if (!var.OrgName.EndsWith("__pValue") && !var.OrgName.EndsWith("__typeClass") && !var.OrgName.EndsWith("__sizeOf"))
			{
				return false;
			}
			int length = var.Name.LastIndexOf("__");
			string item = var.Name.Substring(0, length);
			return hsAnyVariables.Contains(item);
		}

		private IVariable[] GetVars(ISignature sign)
		{
			return (from vas in GetVarsWithDeclaringSignature(sign)
				select vas.Variable).ToArray();
		}

		private VariableAndSignature[] GetVarsWithDeclaringSignature(ISignature sign)
		{
			VariableAndSignature[] array;
			if (sign.POUType != Operator.Function && sign.POUType != Operator.Method)
			{
				array = ((sign.POUType != Operator.Type) ? sign.All.Select((IVariable var) => new VariableAndSignature(var, sign)).ToArray() : GetVariablesWithDeclaringSignature(sign).ToArray());
			}
			else
			{
				VariableAndSignature[] array2 = (from var in HelpCollectInputs(sign)
					select new VariableAndSignature(var, sign)).ToArray();
				VariableAndSignature[] array3 = sign.Outputs.Select((IVariable var) => new VariableAndSignature(var, sign)).ToArray();
				array = new VariableAndSignature[array2.Length + array3.Length];
				array2.CopyTo(array, 0);
				array3.CopyTo(array, array2.Length);
			}
			return array;
		}

		private IEnumerable<VariableAndSignature> GetVariablesWithDeclaringSignature(ISignature sign)
		{
			LList<VariableAndSignature> obj = new LList<VariableAndSignature>();
			obj.InsertRange(0, (IEnumerable<VariableAndSignature>)sign.All.Select((IVariable var) => new VariableAndSignature(var, sign)).ToArray());
			return (IEnumerable<VariableAndSignature>)obj;
		}

		private string GetTypeName(ISignature sign, ISignature signParent, bool bGlobal)
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (signParent != null && sign.POUType == Operator.Method)
			{
				stringBuilder.AppendFormat("{0}_{1}_struct", GetSignatureOrgName(signParent).ToLowerInvariant(), GetSignatureName(sign).ToLowerInvariant());
			}
			else if (sign.POUType == Operator.FunctionBlock)
			{
				stringBuilder.AppendFormat("{0}_struct", GetSignatureOrgName(sign).ToLowerInvariant());
			}
			else if (bGlobal || sign.GetFlag(SignatureFlag.Alias))
			{
				stringBuilder.AppendFormat("{0}", GetSignatureOrgName(sign));
			}
			else
			{
				stringBuilder.AppendFormat("{0}_struct", GetSignatureOrgName(sign).ToLowerInvariant());
			}
			return stringBuilder.ToString().Replace("__", "");
		}

		private string GetDefaultComment(ISignature sign, ISignature signParent, bool bGlobal)
		{
			string typeName = GetTypeName(sign, signParent, bGlobal);
			StringBuilder stringBuilder = new StringBuilder();
			if (bGlobal)
			{
				if (IsEnumeration(sign))
				{
					stringBuilder.AppendFormat("Enumeration: {0}", typeName);
				}
				else if (IsUnion(sign))
				{
					stringBuilder.AppendFormat("Union: {0}", typeName);
				}
				else if (IsStructure(sign))
				{
					stringBuilder.AppendFormat("Structure: {0}", typeName);
				}
				else
				{
					stringBuilder.AppendFormat("{0}", typeName);
				}
			}
			else if (signParent != null && sign.POUType == Operator.Method)
			{
				if (signParent.POUType == Operator.Interface)
				{
					stringBuilder.AppendFormat("{0}::{1}", GetSignatureOrgName(signParent).ToString(), GetSignatureOrgName(sign).ToString());
				}
				else
				{
					stringBuilder.AppendFormat("{0}_{1}", GetSignatureOrgName(signParent).ToLowerInvariant(), GetSignatureName(sign).ToLowerInvariant());
				}
			}
			else if (sign.POUType == Operator.FunctionBlock)
			{
				stringBuilder.AppendFormat("{0}__main", GetSignatureOrgName(sign).ToLowerInvariant());
			}
			else
			{
				stringBuilder.AppendFormat("{0}", GetSignatureOrgName(sign).ToLowerInvariant());
			}
			return stringBuilder.ToString();
		}

		[SuppressMessage("Critical Code Smell", "S3776:Cognitive Complexity of methods should not be too high", Justification = "Will be fixed with CDS-77832")]
		private void DumpTypes(StreamWriter swM4, ISignature sign, ISignature signParent, IScope2 scope, bool bGlobal, bool bUseCAAGuidelines, string stNamespace)
		{
			if (swM4 == null)
			{
				return;
			}
			string typeName = GetTypeName(sign, signParent, bGlobal);
			StringBuilder stringBuilder = new StringBuilder();
			if (!DumpComment(swM4, sign, signParent, bGlobal))
			{
				stringBuilder.Remove(0, stringBuilder.Length);
				stringBuilder.Append("/**");
				stringBuilder.AppendLine();
				if (bGlobal)
				{
					if (sign.GetFlag(SignatureFlag.Enum))
					{
						stringBuilder.AppendFormat(" * <description>Enum: {0}</description>", typeName);
					}
					else
					{
						stringBuilder.AppendFormat(" * <description>{0}</description>", typeName);
					}
				}
				else if (signParent != null && sign.POUType == Operator.Method)
				{
					if (signParent.POUType == Operator.Interface)
					{
						stringBuilder.AppendFormat(" * <description>{0}::{1}</description>", GetSignatureOrgName(signParent).ToString(), GetSignatureOrgName(sign).ToString().Replace("__", ""));
					}
					else
					{
						stringBuilder.AppendFormat(" * <description>{0}_{1}</description>", GetSignatureOrgName(signParent).ToLowerInvariant(), GetSignatureName(sign).ToLowerInvariant().Replace("__", ""));
					}
				}
				else if (sign.POUType == Operator.FunctionBlock)
				{
					stringBuilder.AppendFormat(" * <description>{0}__main</description>", GetSignatureOrgName(sign).ToLowerInvariant());
				}
				else
				{
					stringBuilder.AppendFormat(" * <description>{0}</description>", GetSignatureOrgName(sign).ToLowerInvariant().Replace("__", ""));
				}
				stringBuilder.AppendLine();
				stringBuilder.Append(" */");
				stringBuilder.AppendLine();
				swM4.Write(stringBuilder);
			}
			stringBuilder.Remove(0, stringBuilder.Length);
			if (sign.GetFlag(SignatureFlag.Enum))
			{
				IVariable[] vars = GetVars(sign);
				if ((!sign.HasAttribute(ATTRIBUTE_M4EXPORT_ENUM_AS_C) && !sign.HasAttribute(ATTRIBUTE_M4EXPORT_ENUM_AS_C_OLD)) || vars.Length == 0 || vars[0].CompiledType.DeRefType.Class != TypeClass.DInt)
				{
					ICompiledType compiledType = null;
					IVariable[] array = vars;
					for (int i = 0; i < array.Length; i++)
					{
						IVariable2 variable = (IVariable2)array[i];
						StringBuilder initialValue = GetInitialValue(variable);
						if (sign.HasAttribute("m4export_enum-no-prefix"))
						{
							stringBuilder.AppendFormat("#define {0}    {1}", variable.OrgName.ToString().ToUpper(), initialValue.ToString());
						}
						else
						{
							stringBuilder.AppendFormat("#define {0}_{1}    {2}", GetSignatureOrgName(sign).ToUpper(), variable.OrgName.ToString().ToUpper(), initialValue.ToString());
						}
						stringBuilder.AppendLine();
						compiledType = variable.CompiledType;
					}
					if (compiledType != null)
					{
						stringBuilder.Append("/* Typed enum definition */");
						stringBuilder.AppendLine();
						stringBuilder.AppendFormat("#define {0}    RTS_IEC_{1}", GetSignatureOrgName(sign).ToUpper(), compiledType.BaseType.ToString());
					}
					stringBuilder.AppendLine();
					stringBuilder.AppendLine();
					swM4.Write(stringBuilder);
					return;
				}
				stringBuilder.Append("typedef enum");
				stringBuilder.AppendLine();
				stringBuilder.Append("{");
			}
			else if (sign.GetFlag(SignatureFlag.Union))
			{
				stringBuilder.Append("typedef union");
				stringBuilder.AppendLine();
				stringBuilder.Append("{");
			}
			else if (sign.GetFlag(SignatureFlag.Alias))
			{
				_aliasExporter.DumpAlias(stringBuilder, sign, typeName, scope);
			}
			else
			{
				stringBuilder.AppendFormat("typedef struct tag{0}", typeName);
				stringBuilder.AppendLine();
				stringBuilder.Append("{");
			}
			stringBuilder.AppendLine();
			swM4.Write(stringBuilder);
			int num = 0;
			bool flag = _comcon.GetSignatureById(sign.BaseSignatureId) != null;
			if (sign.POUType == Operator.FunctionBlock)
			{
				stringBuilder.Remove(0, stringBuilder.Length);
				if (!flag)
				{
					stringBuilder.Append("\tvoid* __VFTABLEPOINTER;");
					AppendTabs(stringBuilder);
					stringBuilder.Append("/* Pointer to virtual function table */");
					stringBuilder.AppendLine();
					swM4.Write(stringBuilder);
				}
				try
				{
					int iIdxItfPointerVar = 0;
					DumpInstanceMember(sign, swM4, scope, bUseCAAGuidelines, stNamespace, ref iIdxItfPointerVar);
				}
				catch
				{
					stringBuilder.Remove(0, stringBuilder.Length);
					stringBuilder.AppendLine();
					swM4.Write(stringBuilder);
					num = DumpTypeElements(sign, swM4, scope, bUseCAAGuidelines, stNamespace);
				}
			}
			else if (!sign.GetFlag(SignatureFlag.Alias))
			{
				num = DumpTypeElements(sign, swM4, scope, bUseCAAGuidelines, stNamespace);
			}
			stringBuilder.Remove(0, stringBuilder.Length);
			if (num > 0)
			{
				stringBuilder.AppendLine();
			}
			if (bUseCAAGuidelines)
			{
				stringBuilder.AppendFormat("}} {0};", typeName);
			}
			else if (!sign.GetFlag(SignatureFlag.Alias))
			{
				stringBuilder.AppendFormat("}} {0};", typeName);
			}
			stringBuilder.AppendLine();
			stringBuilder.AppendLine();
			swM4.Write(stringBuilder);
		}

		private void DumpInternalType(StreamWriter swM4, ISignature sign, IScope2 scope, bool bUseCAAGuidelines, string stNamespace)
		{
			if (swM4 != null)
			{
				string arg = GetSignatureOrgName(sign).ToUpperInvariant();
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Remove(0, stringBuilder.Length);
				stringBuilder.Append("/**");
				stringBuilder.AppendLine();
				stringBuilder.AppendFormat(" * <description>{0}</description>", arg);
				stringBuilder.AppendLine();
				stringBuilder.Append(" */");
				stringBuilder.AppendLine();
				swM4.Write(stringBuilder);
				stringBuilder.Remove(0, stringBuilder.Length);
				stringBuilder.AppendFormat("typedef struct tag{0}", arg);
				stringBuilder.AppendLine();
				stringBuilder.Append("{");
				stringBuilder.AppendLine();
				swM4.Write(stringBuilder);
				DumpTypeElements(sign, swM4, scope, bUseCAAGuidelines, stNamespace);
				stringBuilder.Remove(0, stringBuilder.Length);
				stringBuilder.AppendFormat("}} {0};", arg);
				stringBuilder.AppendLine();
				stringBuilder.AppendLine();
				swM4.Write(stringBuilder);
			}
		}

		private void DumpCAAEnumElement(StreamWriter swM4, IVariable2 var, ISignature sign, string stNamespace)
		{
			if (swM4 != null && (sign.POUType != Operator.Function || !var.GetFlag(VarFlag.Local)) && var.CompiledType.Class == TypeClass.Enum)
			{
				StringBuilder stringBuilder = new StringBuilder();
				StringBuilder initialValue = GetInitialValue(var);
				stringBuilder.AppendFormat("\t{0}", var.OrgName);
				if (initialValue.ToString() != "")
				{
					stringBuilder.AppendFormat(" = {0}", initialValue);
				}
				swM4.Write(stringBuilder);
			}
		}

		public StringBuilder GetInitialValue(IVariable2 var)
		{
			StringBuilder stringBuilder = new StringBuilder();
			IScope5 scope = _comcon.CreateGlobalIScope() as IScope5;
			ILiteralValue literalValue = ((_IExpression)var.Initial).Literal(scope, bAllocatedOK: true);
			if (literalValue != null)
			{
				switch (literalValue.KindOf)
				{
				case KindOfLiteral.Bool:
					stringBuilder.Append(literalValue.Bool.ToString().ToUpperInvariant());
					break;
				case KindOfLiteral.Float:
					stringBuilder.Append(literalValue.Float.ToString(CultureInfo.InvariantCulture));
					break;
				case KindOfLiteral.SignedInteger:
					stringBuilder.AppendFormat("0x{0:X}", literalValue.SignedLong);
					break;
				case KindOfLiteral.UnsignedInteger:
					stringBuilder.AppendFormat("0x{0:X}", literalValue.UnsignedLong);
					break;
				case KindOfLiteral.String:
				{
					string text = ((TypeClass.WString == var.Type.Class) ? "RTS_UTF16STRING_TEXT" : "RTS_UTF8STRING_TEXT");
					stringBuilder.Append(text + "(\"" + literalValue.String + "\")");
					break;
				}
				}
				DecorateLiteralWithTypeHint(var, stringBuilder);
			}
			return stringBuilder;
		}

		private void DecorateLiteralWithTypeHint(IVariable2 var, StringBuilder st)
		{
			if (!string.IsNullOrWhiteSpace(st.ToString()) && var.Initial.Type != null)
			{
				TypeClass @class = var.Initial.Type.Class;
				if (@class == TypeClass.Bool || (uint)(@class - 2) <= 13u || (uint)(@class - 39) <= 2u)
				{
					st.Insert(0, $"RTS_IEC_{var.Initial.Type.ToString().ToUpperInvariant()}_C(");
					st.Append(")");
				}
			}
		}

		private void DumpEnumElement(StreamWriter swM4, IVariable2 var, ISignature sign)
		{
			if (swM4 != null && (sign.POUType != Operator.Function || !var.GetFlag(VarFlag.Local)) && var.CompiledType.Class == TypeClass.Enum)
			{
				StringBuilder stringBuilder = new StringBuilder();
				StringBuilder initialValue = GetInitialValue(var);
				stringBuilder.AppendFormat("\t{0}", var.OrgName);
				if (initialValue.ToString() != "")
				{
					stringBuilder.AppendFormat(" = {0}", initialValue);
				}
				swM4.Write(stringBuilder);
			}
		}

		private void AppendTabs(StringBuilder st)
		{
			int num = 1;
			if (st.Length < 36)
			{
				num = (36 - st.Length) / 4 + 1;
			}
			for (int i = 0; i < num; i++)
			{
				st.Append("\t");
			}
		}

		private void DumpCAAElement(StreamWriter swM4, IVariable2 var, ISignature sign, IScope2 scope, string stNamespace)
		{
			if (swM4 == null)
			{
				return;
			}
			ICompiledType compiledType = var.CompiledType;
			StringBuilder stringBuilder = new StringBuilder();
			StringBuilder stringBuilder2 = new StringBuilder();
			ICompiledType compiledType2 = compiledType;
			while (compiledType2.Class == TypeClass.Pointer || compiledType2.Class == TypeClass.Reference)
			{
				stringBuilder.Append("*");
				compiledType2 = compiledType2.BaseType;
			}
			if (stringBuilder.ToString() == "" && var.GetFlag(VarFlag.Inout))
			{
				stringBuilder.Append("*");
			}
			compiledType = compiledType2;
			if (compiledType.ToString().Contains("RTS_IEC_"))
			{
				string text = compiledType.ToString();
				text = text.Substring(text.IndexOf("RTS_IEC_"));
				stringBuilder2.AppendFormat("\t{0} {1}{2};", text.ToUpperInvariant(), stringBuilder, GetVarOrgName(var, sign));
			}
			else if (compiledType.Class == TypeClass.Array)
			{
				if (stringBuilder.ToString() != "")
				{
					stringBuilder2.AppendFormat("\t{0} *{1}{2}", compiledType.BaseType.ToString(), stringBuilder, GetVarOrgName(var, sign));
					stringBuilder2.Append(";");
				}
				else
				{
					stringBuilder2.AppendFormat("\t{0} {1}", compiledType.BaseType.ToString(), GetVarOrgName(var, sign));
					IArrayDimension[] dimensions = (compiledType as IArrayType).Dimensions;
					foreach (IArrayDimension arrayDimension in dimensions)
					{
						stringBuilder2.AppendFormat("[{0}]", uint.Parse(arrayDimension.UpperBorder.ToString()) - uint.Parse(arrayDimension.LowerBorder.ToString()) + 1);
					}
					stringBuilder2.Append(";");
				}
			}
			else if (compiledType.Class == TypeClass.Enum)
			{
				stringBuilder2.AppendFormat("\t{0} {1}{2};", MapCAATypes(var), stringBuilder, GetVarOrgName(var, sign));
			}
			else if (compiledType.Class == TypeClass.Userdef)
			{
				if (var.OrgName == "__INSTANCEPOINTER")
				{
					stringBuilder2.AppendFormat("\t{0}_struct *pInstance;", GetExternalType(compiledType.BaseType, var.Type.ToString(), bSimpleType: false, scope).ToLowerInvariant().Replace("_struct", ""));
				}
				else
				{
					IUserdefType2 userdefType = ((compiledType.Class != TypeClass.Userdef) ? (compiledType.BaseType as IUserdefType2) : (compiledType as IUserdefType2));
					if (scope[userdefType.SignatureId].GetFlag(SignatureFlag.ImplicitInterfaceUnion))
					{
						stringBuilder.Append("*");
					}
					if (compiledType.ToString() == sign.OrgName)
					{
						stringBuilder2.AppendFormat("\tstruct tag{0} {1}{2};", compiledType.ToString(), stringBuilder, GetVarOrgName(var, sign));
					}
					else if (compiledType.ToString().Contains("."))
					{
						string text2 = compiledType.ToString().Substring(compiledType.ToString().LastIndexOf("."));
						stringBuilder2.AppendFormat("\t{0} {1}{2};", text2.Replace(".", ""), stringBuilder, GetVarOrgName(var, sign));
					}
					else
					{
						stringBuilder2.AppendFormat("\t{0} {1}{2};", compiledType.ToString(), stringBuilder, GetVarOrgName(var, sign));
					}
				}
			}
			else
			{
				stringBuilder2.AppendFormat("\t{0} {1}{2};", MapCAATypes(var), stringBuilder, GetVarOrgName(var, sign));
			}
			if (sign.POUType == Operator.FunctionBlock || sign.POUType == Operator.Method || sign.POUType == Operator.Function)
			{
				AppendTabs(stringBuilder2);
				if (var.GetFlag(VarFlag.Input))
				{
					stringBuilder2.AppendFormat("/* VAR_INPUT */\t{0}", GetComment(var));
				}
				else if (var.GetFlag(VarFlag.Output))
				{
					stringBuilder2.AppendFormat("/* VAR_OUTPUT */\t{0}", GetComment(var));
				}
				else if (var.GetFlag(VarFlag.Inout))
				{
					stringBuilder2.AppendFormat("/* VAR_IN_OUT */\t{0}", GetComment(var));
				}
				else if (var.GetFlag(VarFlag.Local))
				{
					stringBuilder2.AppendFormat("/* Local variable */\t{0}", GetComment(var));
				}
			}
			stringBuilder2.AppendLine();
			swM4.Write(stringBuilder2);
		}

		private string GetExternalUserDefType(ISignature sign, ICompiledType type)
		{
			if (sign.POUType == Operator.FunctionBlock || sign.POUType == Operator.Interface)
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.AppendFormat("{0}_struct", GetSignatureOrgName(sign).ToLowerInvariant().Replace("__", ""));
				return stringBuilder.ToString();
			}
			if (sign.HasAttribute(RTSExportAttributes.ATTRIBUTE_C_SOURCE_EXPORT))
			{
				return sign.GetAttributeValue(RTSExportAttributes.ATTRIBUTE_C_SOURCE_EXPORT);
			}
			if (type != null)
			{
				return GetSignatureOrgName(sign);
			}
			return null;
		}

		private bool IsXType(ICompiledType type)
		{
			if (APEnvironmentFacade.Instance.LanguageModelMgr.TypeInfo is ITypeInfo4 typeInfo && typeInfo.IsResolvedXType(type))
			{
				return true;
			}
			return false;
		}

		private string GetXType(ICompiledType type)
		{
			if (APEnvironmentFacade.Instance.LanguageModelMgr.TypeInfo is ITypeInfo4 typeInfo && typeInfo.IsResolvedXType(type))
			{
				switch (type.Class)
				{
				case TypeClass.DInt:
				case TypeClass.LInt:
					return "XINT";
				case TypeClass.DWord:
				case TypeClass.LWord:
					return "XWORD";
				case TypeClass.UDInt:
				case TypeClass.ULInt:
					return "UXINT";
				}
			}
			return null;
		}

		private bool IsOriginalTypeToUse(string sOrgType)
		{
			if (sOrgType == null)
			{
				return false;
			}
			string[] array = s_sTypeNameSubstringsToUseOriginalType;
			foreach (string value in array)
			{
				if (sOrgType.Contains(value))
				{
					return true;
				}
			}
			return false;
		}

		[SuppressMessage("Critical Code Smell", "S3776:Cognitive Complexity of methods should not be too high", Justification = "Will be fixed with CDS-77832")]
		internal string GetExternalType(ICompiledType type, string sOrgType, bool bSimpleType, IScope2 scope)
		{
			sOrgType = sOrgType.Replace("POINTER TO ", "");
			if (sOrgType.Contains("RTS_IEC_"))
			{
				sOrgType = sOrgType.Replace("SysTypes.", "");
				return sOrgType;
			}
			if (sOrgType.Contains("XWORD"))
			{
				sOrgType = sOrgType.Replace("SysTypes.", "");
				sOrgType = sOrgType.Replace("__", "");
				return "RTS_IEC_" + sOrgType;
			}
			if (IsOriginalTypeToUse(sOrgType))
			{
				return sOrgType;
			}
			if (IsXType(type))
			{
				return "RTS_IEC_" + GetXType(type);
			}
			switch (type.Class)
			{
			case TypeClass.Enum:
				if (type is IEnumType2 enumType)
				{
					ISignature signature = scope[enumType.SignatureId];
					if (signature != null && UseOriginalTypeNames && !string.IsNullOrEmpty(signature.LibraryPath) && !ExportLibTypes)
					{
						return signature.OrgName;
					}
				}
				return type.ToString();
			case TypeClass.Userdef:
			{
				IUserdefType2 userdefType = type as IUserdefType2;
				ISignature sign = scope[userdefType.SignatureId];
				return GetExternalUserDefType(sign, type);
			}
			case TypeClass.Array:
				if (bSimpleType)
				{
					return GetExternalType(type.BaseType, sOrgType, bSimpleType, scope);
				}
				type = type.BaseType;
				if (type.Class != TypeClass.Array && type.Class != TypeClass.Enum && type.Class != TypeClass.Userdef)
				{
					return "RTS_IEC_" + type.ToString().ToUpperInvariant();
				}
				return type.ToString();
			case TypeClass.String:
			case TypeClass.WString:
			{
				string[] array = type.ToString().Split('(', ')');
				return "RTS_IEC_" + array[0];
			}
			case TypeClass.Bit:
				return "RTS_IEC_USINT";
			default:
			{
				string text = "";
				while (type.Class == TypeClass.Pointer || type.Class == TypeClass.Array)
				{
					if (type.Class != TypeClass.Userdef)
					{
						text += "*";
					}
					type = type.BaseType;
				}
				if (type.Class == TypeClass.Userdef)
				{
					return GetExternalType(type, type.ToString().ToUpperInvariant(), bSimpleType, scope) + text;
				}
				return "RTS_IEC_" + type.ToString().ToUpperInvariant() + text;
			}
			}
		}

		[SuppressMessage("Critical Code Smell", "S3776:Cognitive Complexity of methods should not be too high", Justification = "Will be fixed with CDS-77832")]
		private string GetArrayDims(IVariable var, ICompiledType type, IScope2 scope)
		{
			StringBuilder stringBuilder = new StringBuilder();
			while (type.Class == TypeClass.Array)
			{
				IArrayDimension[] dimensions = (type as IArrayType).Dimensions;
				foreach (IArrayDimension arrayDimension in dimensions)
				{
					int num = -1;
					int num2 = -1;
					bool flag = true;
					bool flag2 = true;
					try
					{
						num = int.Parse(arrayDimension.LowerBorder.ToString());
					}
					catch
					{
						ILiteralValue literalValue = arrayDimension.LowerBorder.Literal(scope);
						if (literalValue != null)
						{
							flag2 = false;
							num = literalValue.GetInt(out bool bValid);
							flag = flag && bValid;
						}
						else
						{
							flag = false;
						}
					}
					try
					{
						num2 = int.Parse(arrayDimension.UpperBorder.ToString());
					}
					catch
					{
						ILiteralValue literalValue2 = arrayDimension.UpperBorder.Literal(scope);
						if (literalValue2 != null)
						{
							flag2 = false;
							num2 = literalValue2.GetInt(out bool bValid2);
							flag = flag && bValid2;
						}
						else
						{
							flag = false;
						}
					}
					if ((var.HasAttribute(ATTRIBUTE_M4EXPORT_ARRAYLEN_AS_VALUE) || flag2 || TypeLengthAsValue) && flag)
					{
						stringBuilder.AppendFormat("[{0}]", num2 - num + 1);
					}
					else if (var.HasAttribute(ATTRIBUTE_M4EXPORT_ARRAYLEN_AS_IS))
					{
						string arg = new ConstantNumericExpressionDumper(_comcon, bUseExpressionsAsIs: true).DumpConstantExpression(arrayDimension.UpperBorder);
						if (num == 0)
						{
							stringBuilder.AppendFormat("[{0}]", arg);
						}
						else
						{
							stringBuilder.AppendFormat("[{0} - {1}]", arg, num);
						}
					}
					else
					{
						string arg2 = new ConstantNumericExpressionDumper(_comcon, bUseExpressionsAsIs: false).DumpConstantExpression(arrayDimension.UpperBorder);
						if (num == 0)
						{
							stringBuilder.AppendFormat("[{0} + 1]", arg2);
						}
						else
						{
							stringBuilder.AppendFormat("[{0} - {1}]", arg2, num - 1);
						}
					}
				}
				type = type.BaseType;
			}
			StringBuilder stringBuilder2 = null;
			if (type.Class == TypeClass.String || type.Class == TypeClass.WString)
			{
				stringBuilder2 = new StringBuilder();
				string[] array = type.ToString().Split('(', ')');
				if (array.Length > 1 && array[1] != "")
				{
					try
					{
						stringBuilder2.AppendFormat("[{0}]", int.Parse(array[1]) + 1);
					}
					catch (Exception)
					{
						if (var.HasAttribute(ATTRIBUTE_M4EXPORT_STRINGLEN_AS_VALUE) || TypeLengthAsValue)
						{
							IPreCompileUtilities preCompileUtils = APEnvironmentFacade.Instance.LanguageModelUtilities.PreCompileUtils;
							preCompileUtils.CreateConstantEvaluator(preCompileUtils.CreateContext(Guid.Empty, Guid.Empty)).Evaluate(array[1]).GetInt(out int value);
							stringBuilder2.AppendFormat("[{0}]", ++value);
						}
						else
						{
							string[] array2 = Common.SplitAtDot(array[1].ToUpper());
							if (var.HasAttribute(ATTRIBUTE_M4EXPORT_STRINGLEN_AS_IS))
							{
								stringBuilder2.AppendFormat("[{0}]", array2[array2.Length - 1].Replace("(", "").Replace(")", ""));
							}
							else
							{
								stringBuilder2.AppendFormat("[{0} + 1]", array2[array2.Length - 1].Replace("(", "").Replace(")", ""));
							}
						}
					}
				}
				else
				{
					stringBuilder2.Append("[81]");
				}
			}
			if (stringBuilder2 != null)
			{
				stringBuilder.Append(stringBuilder2);
			}
			return stringBuilder.ToString();
		}

		private string GetVarOrgName(IVariable2 var, ISignature sign)
		{
			if (var.OrgName == sign.OrgName)
			{
				return GetSignatureOrgName(sign);
			}
			return var.OrgName;
		}

		private string GetComment(IVariable2 var)
		{
			if (var.Comment != null)
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.AppendFormat("/* {0} */", var.Comment.Trim('\t', ' ').ToString());
				return stringBuilder.ToString();
			}
			return "";
		}

		private bool IsWellKnownPointerType(IVariable2 var)
		{
			if (var == null)
			{
				return false;
			}
			string[] array = s_sWellKnownPointerTypes;
			foreach (string value in array)
			{
				if (var.Type.ToString().Contains(value) || var.OriginalType.ToString().Contains(value))
				{
					return true;
				}
			}
			return false;
		}

		private IType GetWellKnownTypeForSpecialProcessing(IVariable2 var)
		{
			if (var == null)
			{
				return null;
			}
			IType type = null;
			string[] array = s_sWellKnownTypesForSpecialProcessing;
			foreach (string value in array)
			{
				if (var.Type.ToString().Contains(value))
				{
					type = var.Type;
					break;
				}
				if (var.OriginalType.ToString().Contains(value))
				{
					type = var.OriginalType;
					break;
				}
			}
			if (type is IReferenceType2 referenceType)
			{
				type = referenceType.OriginalBase;
			}
			return type;
		}

		[SuppressMessage("Critical Code Smell", "S3776:Cognitive Complexity of methods should not be too high", Justification = "Will be fixed with CDS-77832")]
		private void DumpElement(StreamWriter swM4, IVariable2 var, ISignature sign, IScope2 scope)
		{
			if (swM4 == null || var.HasAttribute(CompileAttributes.ATTRIBUTE_PROPERTY))
			{
				return;
			}
			ICompiledType compiledType = var.CompiledType;
			ICompiledType originalType = var.OriginalType;
			StringBuilder stringBuilder = new StringBuilder();
			StringBuilder stringBuilder2 = new StringBuilder();
			IType wellKnownTypeForSpecialProcessing = GetWellKnownTypeForSpecialProcessing(var);
			bool flag = IsWellKnownPointerType(var);
			ICompiledType compiledType2 = compiledType;
			if (UseOriginalTypeNames || (originalType != null && wellKnownTypeForSpecialProcessing == null && !flag && originalType.ToString().ToUpper().StartsWith("RTS_IEC_")))
			{
				compiledType2 = originalType;
			}
			bool flag2 = false;
			while (compiledType2.Class == TypeClass.Pointer || compiledType2.Class == TypeClass.Reference)
			{
				flag2 = true;
				stringBuilder.Append("*");
				compiledType2 = compiledType2.BaseType;
			}
			if (stringBuilder.ToString() == "" && var.GetFlag(VarFlag.Inout))
			{
				stringBuilder.Append("*");
			}
			if (!UseOriginalTypeNames && flag)
			{
				stringBuilder = stringBuilder.Replace("*", "", 0, 1);
			}
			compiledType = compiledType2;
			if (wellKnownTypeForSpecialProcessing != null)
			{
				stringBuilder2.AppendFormat("\t{0} {1}{2};", GetExternalType(compiledType, wellKnownTypeForSpecialProcessing.ToString(), bSimpleType: false, scope), stringBuilder, GetVarOrgName(var, sign));
			}
			else if (compiledType.Class == TypeClass.Array)
			{
				if (stringBuilder.ToString() != "")
				{
					stringBuilder2.AppendFormat("\t{0} *{1}{2}", GetExternalType(compiledType, var.Type.ToString(), bSimpleType: true, scope), stringBuilder, GetVarOrgName(var, sign));
					stringBuilder2.Append(";");
				}
				else
				{
					stringBuilder2.AppendFormat("\t{0} {1}{2};", GetExternalType(compiledType, var.Type.ToString(), bSimpleType: true, scope), GetVarOrgName(var, sign), GetArrayDims(var, compiledType, scope));
				}
			}
			else if (compiledType.Class == TypeClass.Enum)
			{
				string externalType = GetExternalType(UseOriginalTypeNames ? compiledType : compiledType.DeRefType, var.Type.ToString(), bSimpleType: false, scope);
				string varOrgName = GetVarOrgName(var, sign);
				stringBuilder2.Append($"\t{externalType} {stringBuilder}{varOrgName};");
			}
			else if (compiledType.Class == TypeClass.Userdef)
			{
				if (var.OrgName == "__INSTANCEPOINTER")
				{
					GenerateInstancePointer(var, scope, compiledType, stringBuilder2);
				}
				else
				{
					IUserdefType2 userdefType = ((compiledType.Class != TypeClass.Userdef) ? (compiledType.BaseType as IUserdefType2) : (compiledType as IUserdefType2));
					ISignature signature = scope[userdefType.SignatureId];
					if (signature.GetFlag(SignatureFlag.ImplicitInterfaceUnion))
					{
						stringBuilder.Append("*");
						string externalUserDefType = GetExternalUserDefType(GetSignatureIfInterfaceUnion(signature), null);
						stringBuilder2.Append($"\t{externalUserDefType} {stringBuilder}{GetVarOrgName(var, sign)};");
					}
					else if (compiledType.ToString() == sign.OrgName)
					{
						stringBuilder2.AppendFormat("\tstruct tag{0} {1}{2};", GetExternalType(compiledType, var.Type.ToString(), bSimpleType: false, scope), stringBuilder, GetVarOrgName(var, sign));
					}
					else if (compiledType.ToString().Contains("."))
					{
						string text = compiledType.ToString().Substring(compiledType.ToString().LastIndexOf("."));
						stringBuilder2.AppendFormat("\t{0} {1}{2};", text.Replace(".", ""), stringBuilder, GetVarOrgName(var, sign));
					}
					else
					{
						stringBuilder2.AppendFormat("\t{0} {1}{2};", GetExternalType(compiledType, var.Type.ToString(), bSimpleType: false, scope), stringBuilder, GetVarOrgName(var, sign), stringBuilder, GetVarOrgName(var, sign));
					}
				}
			}
			else if (compiledType.Class == TypeClass.String || compiledType.Class == TypeClass.WString)
			{
				StringBuilder stringBuilder3 = new StringBuilder();
				if (!flag2)
				{
					string[] array = compiledType.ToString().Split('(', ')');
					if (array.Length > 1 && array[1] != "")
					{
						try
						{
							stringBuilder3.AppendFormat("[{0}]", int.Parse(array[1]) + 1);
						}
						catch (Exception)
						{
							if (var.HasAttribute(ATTRIBUTE_M4EXPORT_STRINGLEN_AS_VALUE) || TypeLengthAsValue)
							{
								IPreCompileUtilities preCompileUtils = APEnvironmentFacade.Instance.LanguageModelUtilities.PreCompileUtils;
								preCompileUtils.CreateConstantEvaluator(preCompileUtils.CreateContext(Guid.Empty, Guid.Empty)).Evaluate(array[1]).GetInt(out int value);
								stringBuilder3.AppendFormat("[{0}]", ++value);
							}
							else
							{
								string[] array2 = Common.SplitAtDot(array[1].ToUpper());
								if (var.HasAttribute(ATTRIBUTE_M4EXPORT_STRINGLEN_AS_IS))
								{
									stringBuilder3.AppendFormat("[{0}]", array2[array2.Length - 1].Replace("(", "").Replace(")", ""));
								}
								else
								{
									stringBuilder3.AppendFormat("[{0} + 1]", array2[array2.Length - 1].Replace("(", "").Replace(")", ""));
								}
							}
						}
					}
					else
					{
						stringBuilder3.Append("[81]");
					}
				}
				stringBuilder2.AppendFormat("\t{0} {1}{2}{3};", GetExternalType(compiledType, var.Type.ToString(), bSimpleType: false, scope), stringBuilder, GetVarOrgName(var, sign), stringBuilder3);
			}
			else if (compiledType.Class == TypeClass.Bit)
			{
				stringBuilder2.AppendFormat("\t{0} {1} : 1;", GetExternalType(compiledType, var.Type.ToString(), bSimpleType: false, scope), GetVarOrgName(var, sign));
			}
			else
			{
				stringBuilder2.AppendFormat("\t{0} {1}{2};", GetExternalType(compiledType, var.Type.ToString(), bSimpleType: false, scope), stringBuilder, GetVarOrgName(var, sign));
			}
			if (sign.POUType == Operator.FunctionBlock || sign.POUType == Operator.Method)
			{
				AppendTabs(stringBuilder2);
				if (var.GetFlag(VarFlag.Input))
				{
					if (var.Type.Class == TypeClass.Enum)
					{
						stringBuilder2.AppendFormat("/* VAR_INPUT, Enum: {0} */", GetExternalType(var.CompiledType, var.Type.ToString(), bSimpleType: false, scope));
					}
					else
					{
						stringBuilder2.AppendFormat("/* VAR_INPUT */\t{0}", GetComment(var));
					}
				}
				else if (var.GetFlag(VarFlag.Output))
				{
					if (var.Type.Class == TypeClass.Enum)
					{
						stringBuilder2.AppendFormat("/* VAR_OUTPUT, Enum: {0} */", GetExternalType(var.CompiledType, var.Type.ToString(), bSimpleType: false, scope));
					}
					else
					{
						stringBuilder2.AppendFormat("/* VAR_OUTPUT */\t{0}", GetComment(var));
					}
				}
				else if (var.GetFlag(VarFlag.Inout))
				{
					stringBuilder2.AppendFormat("/* VAR_IN_OUT */\t{0}", GetComment(var));
				}
				else if (var.GetFlag(VarFlag.Local))
				{
					stringBuilder2.AppendFormat("/* Local variable */\t{0}", GetComment(var));
				}
			}
			else if (!sign.GetFlag(SignatureFlag.Structure) && sign.POUType != Operator.Function)
			{
				stringBuilder2.AppendFormat("\t\t{0}", GetComment(var));
			}
			stringBuilder2.AppendLine();
			swM4.Write(stringBuilder2);
		}

		private void GenerateInstancePointer(IVariable2 var, IScope2 scope, ICompiledType type, StringBuilder st)
		{
			string text = GetExternalType(type.BaseType, var.Type.ToString(), bSimpleType: false, scope);
			string arg;
			if (TypeClass.Userdef == type.BaseType.Class)
			{
				IUserdefType2 userdefType = type.BaseType as IUserdefType2;
				ISignature signature = scope[userdefType.SignatureId];
				arg = ((!signature.HasAttribute(CompileAttributes.ATTRIBUTE_EXTERNAL_NAME)) ? text.ToLowerInvariant() : text);
				if (Operator.FunctionBlock == signature.POUType || Operator.Interface == signature.POUType)
				{
					text = text.Replace("_struct", "");
				}
				if (!signature.HasAttribute(CompileAttributes.ATTRIBUTE_EXTERNAL_NAME))
				{
					arg = text.ToLowerInvariant() + "_struct";
				}
			}
			else
			{
				arg = text.ToLowerInvariant() + "_struct";
			}
			st.AppendFormat("\t{0} *pInstance;", arg);
		}
	}
}
