using System;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedClass]
	public static class IdentifierConstants
	{
		public static string GlobalImplicitFunctionPointers => "IMPLICIT_FUNCTION_POINTERS";

		public static string GlobalImplicitSignature => "IMPLICIT__DEFINITIONS";

		public static string CodeInitName => "CODE__INIT";

		public static string VFTableName => "CODE__INIT__VFTABLES";

		public static string GlobalInitName => "GLOBAL__INIT";

		public static string GlobalExitName => "GLOBAL__EXIT";

		public static string SignTryCatchAddressTemplate => "SIGNATURE__{0}__TRYCATCH__ADDRESS__{1}";

		public static string SignFPAddressTemplate => "SIGNATURE__{0}__FP__ADDRESS";

		public static string POUStartAddressTemplate => "POU__{0}__START__ADDRESS";

		public static string TryCatchCodeAddressTemplate => "POU__{0}__TRYCATCH__ADDRESS__{1}";

		public static string CopyFunctionNameTemplate => "COPY__FB__{0}__{1}";

		public static string PartialInitFunctionNameTemplate => "PARTIALINIT__FB__{0}__{1}";

		public static string MemoryRegionTemplate => "MEMREG__{0}";

		public static string VFTableTemplate => "VFTABLE__{0}";

		public static string VFTablePointer => "__VFTABLEPOINTER";

		public static string CurrentTaskInfoPointer => "__PCURRENTTASKINFO";

		public static string ImplicitIndexVariable => "__Index__{0}";

		public static string InstancePointer => "__INSTANCEPOINTER";

		public static string InterfacePointerTemplate => "__INTERFACEPOINTER__{0}";

		public static string InterfaceStructureTemplate => "__INTERFACESTRUCTURE__{0}";

		public static string ImplicitMethodInstVarNameTemplate => "__{0}__{1}";

		public static string ImplicitMethodInstVarNameTemplate35110 => "__{0}__{1}__{2}";

		public static string InitMethodName => "FB_INIT";

		public static string ReInitMethodName => "FB_REINIT";

		public static string PartialInitMethodName => "__FB_PARTIALINIT";

		public static string ExitMethodName => "FB_EXIT";

		public static string VFInitMethodName => "__VFINIT";

		public static string MainSignatureName => "__MAIN";

		public static string RelocateCodeName => "__RELOCATE__CODE";

		public static string RelocateOffsetTable => "__RELOCATE__OFFSET_TABLE";

		public static string RelocateOffsetTableVar => "__RELOCATE__OFFSET_TABLE_VAR";

		public static string DownloadPOUName => "__download__code";

		public static string OnlineChangePOUName => "__online__change__code";

		public static string OnlineChange1ConcurrentPOUName => "__online__change__concurrent";

		public static string OnlineChange2RepeatablePOUName => "__online__change__repeatable";

		public static string GetCopyFunctionIdentification => "__TaskLocalVars_GetCopy__";

		public static string PutCopyFunctionIdentification => "__TaskLocalVars_PutCopy__";

		public static string WatchVarsName => "__WatchVars";

		public static string GetPOUStartAddressVarName(int nId)
		{
			return string.Format(POUStartAddressTemplate, nId);
		}

		public static string GetFPAddressVarName(int nId)
		{
			return string.Format(SignFPAddressTemplate, nId);
		}

		public static string GetMemoryRegionVarName(int nIndex)
		{
			return string.Format(MemoryRegionTemplate, nIndex);
		}

		public static string GetVFTableVarName(int nId)
		{
			return string.Format(VFTableTemplate, nId);
		}

		public static string GetCopyFunctionName(_ISignature sign)
		{
			return string.Format(CopyFunctionNameTemplate, sign.Name, sign.Id);
		}

		public static string GetImplicitIndexVariable(int n)
		{
			return string.Format(ImplicitIndexVariable, n);
		}

		public static string GetInterfacePointerName(int nId)
		{
			return string.Format(InterfacePointerTemplate, nId);
		}

		public static string GetInterfaceStructureName(int nId)
		{
			return string.Format(InterfaceStructureTemplate, nId);
		}

		public static string GetImplicitMethodInstVarName(string method, string var)
		{
			return string.Format(ImplicitMethodInstVarNameTemplate, method, var);
		}

		public static string GetImplicitMethodInstVarName351100(string fb, string method, string var)
		{
			return string.Format(ImplicitMethodInstVarNameTemplate35110, fb, method, var);
		}

		public static string GetImplicitMethodInstVarName351800(ISignature sign, ISignature method, IVariable var)
		{
			return $"{sign.OrgName}_{sign.Id}__{method.OrgName}_{method.Id}__{var.OrgName}";
		}

		public static string GetImplicitMethodInstVarName352000(_ISignature sign, ISignature method, IVariable var)
		{
			string text = sign.OrgName;
			if (sign.GetFlagInternal(SignatureFlagInternal.ContainsGenericConstants))
			{
				int num = text.IndexOf('<');
				if (num > 0)
				{
					int num2 = text.LastIndexOf('>');
					if (num2 > 0)
					{
						text = text.Remove(num, num2 - num + 1);
					}
				}
			}
			return $"{text}_{sign.Id}__{method.OrgName}_{method.Id}__{var.OrgName}";
		}

		public static bool IsImplicitInitFunction35100(string stPOUName)
		{
			return stPOUName.ToUpperInvariant().EndsWith("__GVL__INIT");
		}

		public static bool IsImplicitInitFunction(string stPOUName)
		{
			return stPOUName.ToUpperInvariant().EndsWith("__INIT");
		}

		public static string InterfaceUnion(string stName)
		{
			string arg = stName.Replace(".", "__");
			return $"{arg}__Union";
		}

		public static string InterfaceStructure(_ISignature sign)
		{
			string arg = sign.OrgName.Replace(".", "__");
			return $"{arg}__Struct__{sign.Id}";
		}

		public static string GetInterfaceSubstitute(string varOrgName)
		{
			return $"{varOrgName}.__vfTablePointer";
		}

		public static string GetCycleCode(string stTaskName)
		{
			return $"__cycle__code__{stTaskName}";
		}

		public static string GetOnlineChangeConcurrentPOUName(bool bBefore)
		{
			if (bBefore)
			{
				return "__online__change__concurrent_before";
			}
			return "__online__change__concurrent_after";
		}

		public static string GetExplicitInitPOUName(string key)
		{
			return "__explicit__globalinit__" + key;
		}

		public static string GetExplicitExitPOUName(string key)
		{
			return "__explicit__globalexit__" + key;
		}

		public static string GetFBExitInterface()
		{
			return "METHOD FB_Exit: BOOL\r\nVAR_INPUT\r\nbInCopyCode : BOOL;\r\nEND_VAR";
		}

		public static string CreateGetterName(string stVariablename)
		{
			return $"__get{stVariablename}";
		}

		public static string CreateSetterName(string stVariablename)
		{
			return $"__set{stVariablename}";
		}

		public static string GetPropertyName(string stSetterOrGetterFunction)
		{
			if (stSetterOrGetterFunction.StartsWith("__set", StringComparison.InvariantCultureIgnoreCase) || stSetterOrGetterFunction.StartsWith("__get", StringComparison.InvariantCultureIgnoreCase))
			{
				return stSetterOrGetterFunction.Substring(5);
			}
			return string.Empty;
		}

		public static string GetTaskLocalVariablesGVLName(_ISignature signGVL)
		{
			return signGVL.GetAttributeValue("impl_task_local_gvl_name");
		}

		public static string GetTaskLocalVariablesComponentName(_IVariable var)
		{
			return "__" + var.OrgName;
		}

		public static string GetTaskLocalVariablesArrayName(_ISignature signGVL)
		{
			return signGVL.GetAttributeValue("impl_task_local_array_name");
		}
	}
}
