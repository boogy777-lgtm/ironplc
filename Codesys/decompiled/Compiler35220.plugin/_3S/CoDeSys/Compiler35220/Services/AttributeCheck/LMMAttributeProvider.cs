using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using \u0003;
using \u0007;
using \u001B;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;
using \u0081;

namespace _3S.CoDeSys.Compiler35220.Services.AttributeCheck
{
	// Token: 0x0200013B RID: 315
	public class LMMAttributeProvider : ILMAttributeProvider
	{
		// Token: 0x060015DD RID: 5597 RVA: 0x0004019C File Offset: 0x0003E39C
		public LMMAttributeProvider()
		{
			this.\u0008();
			this.\u0001(LMMSlotAttributes.SLOT_ATTRIBUTES);
			this.\u0001(LMMSlotAttributes.SLOT_ATTRIBUTES_WITH_PARAMETERS);
			this.\u0001<CompileAttributes>();
			this.\u0001.Add(new GenericEnumAttribute("monitoring_encoding", "", new Version(3, 5, 18, 0), AttributeScope.Variable, new string[]
			{
				"UTF-8",
				"UnicodeCharacter"
			}));
			this.\u0007();
			this.\u0006();
			this.\u0005();
			this.\u0004();
			this.\u0003();
			this.\u0002();
			this.\u0001();
			this.\u0001.Add(new \u001B.\u0003("dummy-var", "", null, AttributeScope.Variable, 0L, 100L));
			this.\u0001.Add(new \u001B.\u0003("dummy-sign", "", null, AttributeScope.Signature, 0L, 100L));
		}

		// Token: 0x060015DE RID: 5598 RVA: 0x00040638 File Offset: 0x0003E838
		public static string CheckValidUsageOfImplicitParameter(string value, ISignature sign, IVariable var)
		{
			LMMAttributeProvider.\u0001 u = new LMMAttributeProvider.\u0001();
			u.\u0001 = value;
			string[] array = new string[]
			{
				"pouname",
				"position",
				"instance-path"
			};
			if (!string.IsNullOrEmpty(u.\u0001) && !array.Any(new Func<string, bool>(u.\u0001)))
			{
				return string.Format(\u0081.\u0001.Error_Check_ImplicitParameter_UnknownKind, u.\u0001, string.Join(", ", array));
			}
			if (!var.HasFlag(VarFlag.Input))
			{
				return \u0081.\u0001.Error_Check_ImplicitParameter_WrongVarKind;
			}
			if (sign.POUType != Operator.Method && sign.POUType != Operator.Function)
			{
				return \u0081.\u0001.Error_Check_ImplicitParameter_WrongPouType;
			}
			if (var.Type != null && !TypeTable.IsString(var.Type.Class))
			{
				IPointerType pointerType = var.Type as IPointerType;
				if (pointerType == null || !TypeTable.IsString(pointerType.Base.Class))
				{
					return \u0081.\u0001.Error_Check_ImplicitParameter_WrongVarType;
				}
			}
			if (var.Initial == null)
			{
				return \u0081.\u0001.Error_Check_ImplicitParameter_MissingDefault;
			}
			return null;
		}

		// Token: 0x060015DF RID: 5599 RVA: 0x00040728 File Offset: 0x0003E928
		private void \u0001()
		{
			this.\u0001.Add(new global::\u0003.\u0005("implicit-parameter", "", new Version(3, 5, 22, 0), AttributeScope.Variable, new Func<string, AttributeScope, ISignature, IVariable, string>(LMMAttributeProvider.<>c.<>9.\u0001)));
		}

		// Token: 0x060015E0 RID: 5600 RVA: 0x0004077C File Offset: 0x0003E97C
		private void \u0002()
		{
			this.\u0001.Add(new global::\u0003.\u0005(CompileAttributes.ATTRIBUTE_THREAD_SAFE, "", new Version(3, 5, 12, 0), AttributeScope.Signature, new Func<string, AttributeScope, ISignature, IVariable, string>(LMMAttributeProvider.<>c.<>9.\u0002)));
		}

		// Token: 0x060015E1 RID: 5601 RVA: 0x000407D0 File Offset: 0x0003E9D0
		private void \u0003()
		{
			this.\u0001.Add(new global::\u0003.\u0005(CompileAttributes.ATTRIBUTE_TO_STRING, "", new Version(3, 5, 11, 0), AttributeScope.Signature, new Func<string, AttributeScope, ISignature, IVariable, string>(LMMAttributeProvider.<>c.<>9.\u0003)));
		}

		// Token: 0x060015E2 RID: 5602 RVA: 0x00040824 File Offset: 0x0003EA24
		private void \u0004()
		{
			this.\u0001.Add(new global::\u0003.\u0005(CompileAttributes.ATTRIBUTE_TO_WSTRING_FUNCTION, "", new Version(3, 5, 11, 0), AttributeScope.Signature, new Func<string, AttributeScope, ISignature, IVariable, string>(LMMAttributeProvider.<>c.<>9.\u0004)));
		}

		// Token: 0x060015E3 RID: 5603 RVA: 0x00040878 File Offset: 0x0003EA78
		private void \u0005()
		{
			this.\u0001.Add(new global::\u0003.\u0005(CompileAttributes.ATTRIBUTE_TO_STRING_FUNCTION, "", new Version(3, 5, 11, 0), AttributeScope.Signature, new Func<string, AttributeScope, ISignature, IVariable, string>(LMMAttributeProvider.<>c.<>9.\u0005)));
		}

		// Token: 0x060015E4 RID: 5604 RVA: 0x000408CC File Offset: 0x0003EACC
		private void \u0006()
		{
			this.\u0001.Add(new global::\u0003.\u0005(CompileAttributes.ATTRIBUTE_CALL_ON_TYPE_CHANGE, "", new Version(3, 5, 8, 0), AttributeScope.Signature, new Func<string, AttributeScope, ISignature, IVariable, string>(LMMAttributeProvider.<>c.<>9.\u0006)));
		}

		// Token: 0x060015E5 RID: 5605 RVA: 0x0004091C File Offset: 0x0003EB1C
		private void \u0007()
		{
			this.\u0001.Add(new global::\u0003.\u0005(CompileAttributes.ATTRIBUTE_C_CALLING_CONVENTION, "", new Version(3, 5, 6, 0), AttributeScope.Signature, new Func<string, AttributeScope, ISignature, IVariable, string>(LMMAttributeProvider.<>c.<>9.\u0007)));
		}

		// Token: 0x060015E6 RID: 5606 RVA: 0x0004096C File Offset: 0x0003EB6C
		private void \u0008()
		{
			foreach (string u0089_u in this.\u0001)
			{
				this.\u0001.Add(new global::\u0007.\u0004(u0089_u));
			}
		}

		// Token: 0x060015E7 RID: 5607 RVA: 0x000409A4 File Offset: 0x0003EBA4
		private void \u0001(string[] \u0002)
		{
			foreach (string u0089_u in \u0002)
			{
				this.\u0001.Add(new \u001B.\u0003(u0089_u, string.Empty, null, AttributeScope.Signature, -2147483648L, 2147483647L));
			}
		}

		// Token: 0x060015E8 RID: 5608 RVA: 0x000409EC File Offset: 0x0003EBEC
		private void \u0001<\u0001>()
		{
			foreach (FieldInfo fieldInfo in typeof(\u0001).GetFields(BindingFlags.Static | BindingFlags.Public))
			{
				if (fieldInfo.FieldType == typeof(string) && !fieldInfo.GetCustomAttributes(typeof(ObsoleteAttribute), false).Any<object>())
				{
					string u0089_u = fieldInfo.GetValue(null) as string;
					this.\u0001.Add(new global::\u0007.\u0004(u0089_u));
				}
			}
		}

		// Token: 0x060015E9 RID: 5609 RVA: 0x00040A6C File Offset: 0x0003EC6C
		public IEnumerable<IAttribute> GetProvidedAttributes()
		{
			return this.\u0001;
		}

		// Token: 0x040003D0 RID: 976
		private readonly string[] \u0001 = new string[]
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

		// Token: 0x040003D1 RID: 977
		private static readonly string[] \u0002 = new string[]
		{
			"true",
			"false",
			"local_data_only"
		};

		// Token: 0x040003D2 RID: 978
		private readonly LList<IAttribute> \u0001 = new LList<IAttribute>();

		// Token: 0x0200013C RID: 316
		[CompilerGenerated]
		private sealed class \u0001
		{
			// Token: 0x060015EC RID: 5612 RVA: 0x00040AA4 File Offset: 0x0003ECA4
			internal bool \u0001(string \u0002)
			{
				return \u0002.Equals(this.\u0001, StringComparison.OrdinalIgnoreCase);
			}

			// Token: 0x040003D3 RID: 979
			public string \u0001;
		}
	}
}
