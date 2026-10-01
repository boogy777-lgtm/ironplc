using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager.Legacy
{
	// Token: 0x02000275 RID: 629
	public class LMMAttributeProvider
	{
		// Token: 0x06002A49 RID: 10825 RVA: 0x0006B608 File Offset: 0x0006A608
		[SuppressMessage("Critical Code Smell", "S3776:Cognitive Complexity of methods should not be too high", Justification = "No more refactoring in legacy code")]
		public LMMAttributeProvider()
		{
			this.AddStringAttributes();
			this.AddSlotAttributes(LMMSlotAttributes.SLOT_ATTRIBUTES);
			this.AddSlotAttributes(LMMSlotAttributes.SLOT_ATTRIBUTES_WITH_PARAMETERS);
			this.AddAttributesForClass<CompileAttributes>();
			this._attributes.Add(new GenericEnumAttribute("monitoring_encoding", "", new Version(3, 5, 18, 0), AttributeScope.Variable, new string[]
			{
				"UTF-8",
				"UnicodeCharacter"
			}));
			this._attributes.Add(new GenericCheckedAttribute(CompileAttributes.ATTRIBUTE_C_CALLING_CONVENTION, "", new Version(3, 5, 6, 0), AttributeScope.Signature, delegate(string value, AttributeScope scope, ISignature sign, IVariable var)
			{
				if (!string.IsNullOrEmpty(value))
				{
					return string.Format(Strings.AttributeMustBeEmpty, value, CompileAttributes.ATTRIBUTE_C_CALLING_CONVENTION);
				}
				if (sign.POUType != Operator.FunctionBlock && sign.POUType != Operator.Method && sign.POUType != Operator.Function)
				{
					return string.Format(Strings.AttributeNotInFunctionFBOrMethod, CompileAttributes.ATTRIBUTE_C_CALLING_CONVENTION);
				}
				return string.Empty;
			}));
			this._attributes.Add(new GenericCheckedAttribute(CompileAttributes.ATTRIBUTE_CALL_ON_TYPE_CHANGE, "", new Version(3, 5, 8, 0), AttributeScope.Signature, delegate(string value, AttributeScope scope, ISignature sign, IVariable var)
			{
				if (string.IsNullOrEmpty(value))
				{
					return string.Format(Strings.AttributeMustNotBeEmpty, CompileAttributes.ATTRIBUTE_CALL_ON_TYPE_CHANGE);
				}
				if (sign.POUType != Operator.Method)
				{
					return string.Format(Strings.AttributeNotInMethod, CompileAttributes.ATTRIBUTE_CALL_ON_TYPE_CHANGE);
				}
				return string.Empty;
			}));
			this._attributes.Add(new GenericCheckedAttribute(CompileAttributes.ATTRIBUTE_TO_STRING_FUNCTION, "", new Version(3, 5, 11, 0), AttributeScope.Signature, delegate(string value, AttributeScope scope, ISignature sign, IVariable var)
			{
				if (string.IsNullOrWhiteSpace(value))
				{
					return string.Format(Strings.AttributeMustNotBeEmpty, CompileAttributes.ATTRIBUTE_TO_STRING_FUNCTION);
				}
				Operator poutype = sign.POUType;
				if (poutype != Operator.Type && poutype != Operator.FunctionBlock && poutype != Operator.VarGlobal)
				{
					return string.Format(Strings.AttributeToStringFunctionWrongPOUType, CompileAttributes.ATTRIBUTE_TO_STRING_FUNCTION);
				}
				return string.Empty;
			}));
			this._attributes.Add(new GenericCheckedAttribute(CompileAttributes.ATTRIBUTE_TO_WSTRING_FUNCTION, "", new Version(3, 5, 11, 0), AttributeScope.Signature, delegate(string value, AttributeScope scope, ISignature sign, IVariable var)
			{
				if (string.IsNullOrWhiteSpace(value))
				{
					return string.Format(Strings.AttributeMustNotBeEmpty, CompileAttributes.ATTRIBUTE_TO_WSTRING_FUNCTION);
				}
				Operator poutype = sign.POUType;
				if (poutype != Operator.Type && poutype != Operator.FunctionBlock && poutype != Operator.VarGlobal)
				{
					return string.Format(Strings.AttributeToStringFunctionWrongPOUType, CompileAttributes.ATTRIBUTE_TO_WSTRING_FUNCTION);
				}
				return string.Empty;
			}));
			this._attributes.Add(new GenericCheckedAttribute(CompileAttributes.ATTRIBUTE_TO_STRING, "", new Version(3, 5, 11, 0), AttributeScope.Signature, delegate(string value, AttributeScope scope, ISignature sign, IVariable var)
			{
				if (!string.IsNullOrEmpty(value))
				{
					return string.Format(Strings.AttributeMustBeEmpty, value, CompileAttributes.ATTRIBUTE_TO_WSTRING_FUNCTION);
				}
				bool flag = sign.POUType == Operator.VarGlobal && sign.GetFlag(SignatureFlag.Enum);
				if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV352000)
				{
					flag |= (sign.POUType == Operator.Type && sign.GetFlag(SignatureFlag.Enum));
				}
				if (!flag)
				{
					return string.Format(Strings.AttributeToStringWrongType, CompileAttributes.ATTRIBUTE_TO_STRING);
				}
				return string.Empty;
			}));
			this._attributes.Add(new GenericCheckedAttribute(CompileAttributes.ATTRIBUTE_THREAD_SAFE, "", new Version(3, 5, 12, 0), AttributeScope.Signature, delegate(string value, AttributeScope scope, ISignature sign, IVariable var)
			{
				if (Array.IndexOf<string>(LMMAttributeProvider.ThreadSafeValues, value) != -1)
				{
					return string.Empty;
				}
				IEnumerable<string> values = from v in LMMAttributeProvider.ThreadSafeValues
				select "'" + v + "'";
				return string.Format(Strings.AttributeValueMustBeOneOf, value, CompileAttributes.ATTRIBUTE_THREAD_SAFE, string.Join(", ", values));
			}));
			this._attributes.Add(new GenericIntegerAttribute("dummy-var", "", null, AttributeScope.Variable, 0L, 100L));
			this._attributes.Add(new GenericIntegerAttribute("dummy-sign", "", null, AttributeScope.Signature, 0L, 100L));
		}

		// Token: 0x06002A4A RID: 10826 RVA: 0x0006BC10 File Offset: 0x0006AC10
		private void AddStringAttributes()
		{
			foreach (string name in this.STRING_ATTRIBUTES)
			{
				this._attributes.Add(new GenericAttribute(name));
			}
		}

		// Token: 0x06002A4B RID: 10827 RVA: 0x0006BC48 File Offset: 0x0006AC48
		private void AddSlotAttributes(string[] attributes)
		{
			foreach (string name in attributes)
			{
				this._attributes.Add(new GenericIntegerAttribute(name, string.Empty, null, AttributeScope.Signature, -2147483648L, 2147483647L));
			}
		}

		// Token: 0x06002A4C RID: 10828 RVA: 0x0006BC90 File Offset: 0x0006AC90
		private void AddAttributesForClass<T>()
		{
			Type typeFromHandle = typeof(T);
			foreach (FieldInfo fieldInfo in typeFromHandle.GetFields(BindingFlags.Static | BindingFlags.Public))
			{
				if (fieldInfo.FieldType == typeof(string) && !fieldInfo.GetCustomAttributes(typeof(ObsoleteAttribute), false).Any<object>())
				{
					string text = fieldInfo.GetValue(null) as string;
					Debug.Assert(text != null, string.Format("Failed to get string value from {0}.{1}", typeFromHandle.Name, fieldInfo.Name));
					this._attributes.Add(new GenericAttribute(text));
				}
			}
		}

		// Token: 0x17000BDF RID: 3039
		// (get) Token: 0x06002A4D RID: 10829 RVA: 0x0006BD32 File Offset: 0x0006AD32
		public IEnumerable<IAttribute> ProvidedAttributes
		{
			get
			{
				return this._attributes;
			}
		}

		// Token: 0x04000809 RID: 2057
		private readonly string[] STRING_ATTRIBUTES = new string[]
		{
			"@callattribute",
			"minimal_number",
			"default_number",
			"Checksum_Name",
			"Checksum_Type",
			"DoGenerateBP",
			"DoLink",
			"GLOBAL__EXIT__COPY",
			"ReallyReallyRemoveMe",
			"RemovedAfterDownload",
			"__SYSTEM__CALL",
			"analyzation",
			"authentification",
			"blobinit",
			"blobinitconst",
			"checksum_override",
			"contains_blobinitconst",
			"contains_no_copy",
			"direct-call",
			"displaymode",
			"do_retain",
			"enum-as-C",
			"friend_instance",
			"hide",
			"ignore_link_all",
			"implicit_state_chart_struct",
			"inferredtype",
			"init_inputs_on_onlchange",
			"init_on_onl_change",
			"init_related_code",
			"initial_value_crc",
			"instancevar",
			"ioconfig_pou",
			"is_connected",
			"mapping_unchanged",
			"memsetinit",
			"no-function-pointer",
			"no_default_init",
			"no_default_initialisation",
			"no_explicit_call",
			"no_misalignment_check",
			"no_virtual_actions",
			"nounsignedcheck",
			"property",
			"qualified_only",
			"relocate_special",
			"show-toplevel",
			"singleton",
			"subsequent",
			"suppress_warning_0",
			"system_variable",
			"task",
			"test",
			"testassignactivefor",
			"testassignvaluefor",
			"testcasecount",
			"testcaseendexpression",
			"testcasegroup",
			"testcasegrouptimeout",
			"testcaseindex",
			"testcasename",
			"testcasestartexpression",
			"testcasetimeout",
			"testcategory",
			"testcomparetypefor",
			"testcustomconditions",
			"testcustomstatements",
			"testdescription",
			"testdocall",
			"testees",
			"testexecuteinherited",
			"testexecuteserialized",
			"testexpressionindex",
			"testfailuremode",
			"testfailuremodefor",
			"testforces",
			"testignorevalue",
			"testinvariants",
			"testparametersource",
			"testparametertable",
			"testreferenceprecision",
			"testreferenceuppervalue",
			"testreferencevalue",
			"teststatementindex",
			"teststepdescription",
			"teststepduration",
			"teststepdurationcondition",
			"teststepdurationcount",
			"teststepdurationfailuremode",
			"teststepdurationmaxcount",
			"teststepdurationmaxtime",
			"teststepdurationmincount",
			"teststepdurationmintime",
			"teststepdurationtime",
			"teststepexecutionendexpression",
			"teststepexecutionstartexpression",
			"vfinitonly",
			"vtable_order",
			"prohibit-multiple-instance-calls",
			"no-analysis",
			"do-analysis",
			"suppress_messages",
			"abstract",
			"propertyobject-guid",
			"allow_add_or_remove_signature"
		};

		// Token: 0x0400080A RID: 2058
		private static readonly string[] ThreadSafeValues = new string[]
		{
			"true",
			"false",
			"local_data_only"
		};

		// Token: 0x0400080B RID: 2059
		private readonly LList<IAttribute> _attributes = new LList<IAttribute>();
	}
}
