using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedClass]
	public abstract class CompileAttributes
	{
		public static string ATTRIBUTE_DOCUCOMMENT = "''DOCU__COMMENT";

		public static string ATTRIBUTE_COMMENT = "''NORMAL__COMMENT";

		public static string ATTRIBUTE_NOINIT = "noinit";

		public static string ATTRIBUTE_NO_INIT1 = "no-init";

		public static string ATTRIBUTE_NO_INIT2 = "no_init";

		public static string ATTRIBUTE_NO_INIT_CALL = "no_init_call";

		public static string ATTRIBUTE_PACK_MODE = "pack_mode";

		public static string ATTRIBUTE_LOCATE_DATA_SLOT = "locate_data_slot";

		public static string ATTRIBUTE_GLOBAL_INIT_SLOT = "global_init_slot";

		public static string ATTRIBUTE_LOCATION = "location";

		public static string ATTRIBUTE_OFFSET = "offset";

		public static string ATTRIBUTE_COMPATIBILITY_ID = "compatibility_id";

		public static string ATTRIBUTE_SIGNATURE_FLAG = "signature_flag";

		public static string ATTRIBUTE_POU_FLAG = "pou_flag";

		public static string ATTRIBUTE_EXTERNAL_NAME = "external_name";

		public static string ATTRIBUTE_FUNCTION = "function";

		public static string ATTRIBUTE_PROPERTY = "property";

		public static string ATTRIBUTE_TRANSITION = "transition";

		public static string ATTRIBUTE_MONITORING = "monitoring";

		public static string ATTRIBUTE_USELOCATION = "uselocation";

		public static string ATTRIBUTE_INITIALIZE_ON_CALL = "initialize_on_call";

		public static string ATTRIBUTE_HIDE = "hide";

		public const string ATTRIBUTE_CONDITIONAL_SHOW = "conditionalshow";

		public static string ATTRIBUTE_SHOW = "show";

		public static string ATTRIBUTE_HIDE_FROM_TASK_CONFIGURATION = "hide-from-task-configuration";

		public static string ATTRIBUTE_TASKCYCLE = "task_cycle_pou";

		public static string ATTRIBUTE_REFLECTION = "reflection";

		public static string ATTRIBUTE_INSTANCE_PATH = "instance-path";

		public static string ATTRIBUTE_NO_COPY = "no_copy";

		public static string ATTRIBUTE_NO_ONLINE_CHANGE = "no_online_change";

		public static string ATTRIBUTE_NO_EXIT = "no-exit";

		public static string ATTRIBUTE_INIT_ON_ONLCHANGE = "init_on_onlchange";

		public static string ATTRIBUTE_INIT_NAMESPACE = "init_namespace";

		public const string ATTRIBUTE_INIT_NAMESPACE_LOWERCASE = "init_namespace_lowercase";

		public static string ATTRIBUTE_QUALIFIED_ACCESS_ONLY = "qualified-access-only";

		public static string ATTRIBUTE_QUALIFIED_ONLY = "qualified_only";

		public static string ATTRIBUTE_IGNORE_DUPLICATE = "ignore-duplicate";

		public static string ATTRIBUTE_HIDE_ALL_LOCALS = "hide_all_locals";

		public const string ATTRIBUTE_CONDITIONALSHOW_ALL_LOCALS = "conditionalshow_all_locals";

		public static string ATTRIBUTE_SIGNATURE_CRC = "''crc";

		public static string ATTRIBUTE_SIGNATURE_CRC_64 = "''crc_64";

		public static string ATTRIBUTE_CPPCOMPATIBLE = "c++_compatible";

		public static string ATTRIBUTE_CPPEXTERNAL = "c++_external";

		public static string ATTRIBUTE_MINIMAL_INPUT_SIZE = "minimal_input_size";

		public static string ATTRIBUTE_GENERATE_EXCEPTIONINFO = "generate_exceptioninfo";

		public static string ATTRIBUTE_OBSOLETE = "obsolete";

		public static string ATTRIBUTE_RELATIVE_OFFSET = "relative_offset";

		public static string ATTRIBUTE_USESINSTANCELAZIES = "use_instance_lazy";

		public static string ATTRIBUTE_MESSAGE_GUID = "message_guid";

		public static string ATTRIBUTE_CHECK_BOUNDS = "check_bounds";

		public static string ATTRIBUTE_CHECK_POINTER = "check_pointer";

		public static string ATTRIBUTE_CHECK_RANGE_UNSIGNED = "check_range_unsigned";

		public static string ATTRIBUTE_CHECK_RANGE_SIGNED = "check_range_signed";

		public static string ATTRIBUTE_CHECK_LRANGE_UNSIGNED = "check_lrange_unsigned";

		public static string ATTRIBUTE_CHECK_LRANGE_SIGNED = "check_lrange_signed";

		public static string ATTRIBUTE_CHECK_DIV_REAL64 = "check_div_real64";

		public static string ATTRIBUTE_CHECK_DIV_REAL32 = "check_div_real32";

		public static string ATTRIBUTE_CHECK_DIV_INT64 = "check_div_int64";

		public static string ATTRIBUTE_CHECK_DIV_INT32 = "check_div_int32";

		public static string ATTRIBUTEVALUE_VARIABLE = "variable";

		public static string ATTRIBUTEVALUE_CALL = "call";

		public static string ATTRIBUTE_NO_CHECK = "no_check";

		public static string DO_CHECKS_FOR_IMPLICIT_CODE = "do_checks_for_implicit_code";

		public static string ATTRIBUTE_READ_ONLY = "read_only";

		public static string ATTRIBUTE_IMPLICIT_CODE = "implicit_code";

		public static string ATTRIBUTE_LINK_ID = "''link_id";

		public static string ATTRIBUTE_OBJECT_NAME = "object_name";

		public static string ATTRIBUTE_LINK_ALWAYS = "linkalways";

		public static string ATTRIBUTE_CONSTANT_REPLACED = "const_replaced";

		public static string ATTRIBUTE_CONSTANT_NON_REPLACED = "const_non_replaced";

		public static string ATTRIBUTE_PARAMETERLIST = "parameterlist";

		public static string ATTRIBUTE_IS_CONNECTED = "is_connected";

		public static string ATTRIBUTE_SIGNATURE_OVERLOAD = "signatureoverload";

		public static string ATTRIBUTE_SIGNATURE_OVERLOAD_LIBRARY = "signatureoverloadlibrary";

		public static string ATTRIBUTE_SIGNATURE_OVERLOAD_NAMESPACE = "signatureoverloadnamespace";

		public static string ATTRIBUTE_CALL_AFTER_INIT = "call_after_init";

		public static string GET_ACCESS = "get_access";

		public static string SET_ACCESS = "set_access";

		public static string GET_BITACCESS = "get_bitaccess";

		public static string SET_BITACCESS = "set_bitaccess";

		public static string DEVICE_PARAMETER = "device_parameter";

		public static string ATTRIBUTE_SET = "set";

		public static string ATTRIBUTE_GET = "get";

		public static string ATTRIBUTE_ALLOW_INITIAL_VALUE_CHANGES = "allow_initial_value_changes";

		public const string ATTRIBUTE_ALLOW_ADD_OR_REMOVE_SIGNATURE = "allow_add_or_remove_signature";

		public static string ATTRIBUTE_MONITORING_INSTEAD = "monitoring_instead";

		public const string ATTRIBUTE_MONITORING_DISPLAY = "monitoring_display";

		public const string ATTRIBUTE_MONITORING_ENCODING = "monitoring_encoding";

		public static string ATTRIBUTE_NOCONST = "noconst";

		public static string ATTRIBUTE_CHECKSUPERGLOBAL = "checksuperglobal";

		public static string ATTRIBUTE_IOCONFIG_POU = "ioconfig_pou";

		public static string ATTRIBUTE_NOWATCH = "nowatch";

		public static string ATTRIBUTE_DIRECTCALL = "direct-call";

		public static string ATTRIBUTE_HASANYTYPE = "hasanytype";

		public static string ATTRIBUTE_ANYTYPECLASS = "anytypeclass";

		public static string ATTRIBUTE_GEN_IMPLICIT_INIT_FUN = "generate_implicit_init_function";

		public static string ATTRIBUTE_NO_QUERY_INTERFACE_CHECK = "no-query-interface-check";

		public static string ATTRIBUTE_GENERATE_BP = "generate_bp";

		public static string ATTRIBUTE_BLOBINIT = "blobinit";

		public static string ATTRIBUTE_BLOBINITCONST = "blobinitconst";

		public static string ATTRIBUTE_NO_OPTIMIZED_ARRAY_INIT = "no_optimized_array_init";

		public static string ATTRIBUTE_BLOBINITCONST_ONLINECHANGESUPPORT = "blobinitconst_onlinechangesupport";

		public const string ATTRIBUTE_DELAYED_LANGUAGEMODEL_PROVISION = "delayed_languagemodel_provision";

		public const string ATTRIBUTE_OMIT_UPTODATE_CHECK = "omit_uptodate_check";

		public const string ATTRIBUTE_UPTODATE_CHECK_CHECKSUM = "uptodate_check_checksum";

		public const string ATTRIBUTE_INPUTCONSTANT = "input_constant";

		public const string ATTRIBUTE_COMPATIBILITY = "compatibility";

		public const string ATTRIBUTE_NO_UNICODE_SUPPORT = "NO_UNICODE_SUPPORT";

		public const string ATTRIBUTE_GRANULARITY = "granularity";

		public const string ATTRIBUTE_MAP_TO = "map_to";

		public const string ATTRIBUTE_NO_INSTANCE_IN_RETAIN = "no_instance_in_retain";

		public static string ATTRIBUTE_NO_PRECOMPILE_CHECKS = "no_precompile_checks";

		public static string ATTRIBUTE_FORCE_PRECOMPILE_CHECKS = "force_precompile_checks";

		public static string ATTRIBUTE_RECREATED_ON_ONLINE_CHANGE = "recreated_on_online_change";

		public static string ATTRIBUTE_IGNORE_FOR_FAST_ONLINE_CHANGE_TEST = "ignore_for_fast_online_change_test";

		public static string ATTRIBUTE_POOL_IGNORE_TOPLEVEL_POUS = "pool_ignore_toplevel_pous";

		public static string ATTRIBUTE_ESTIMATED_STACK_USAGE = "estimated-stack-usage";

		public static string ATTRIBUTE_ALLOW_DISASSEMBLY = "allow-disassembly";

		public static string ATTRIBUTE_C_CALLING_CONVENTION = "c-calling-convention";

		public static string ATTRIBUTE_SKIP_IMPLEMENTED_ITF_CHECK = "skip-implemented-itf-check";

		public static string ATTRIBUTE_CHECKS_IN_LIBS = "checks_in_libs";

		public static string ATTRIBUTE_EXPLICIT_INIT_EXIT_HANDLING = "explicit-init-exit-handling";

		public static string ATTRIBUTE_CALL_WITHIN_GLOBAL_INIT_EXIT_SLOT = "call_within_global_init_exit_slot";

		public const string ATTRIBUTE_PREVENT_FAST_ONLINECHANGE = "prevent-fastonlinechange";

		public const string ATTRIBUTE_PREVENT_FAST_ONLINECHANGE_ONCODECHANGES = "prevent-fastonlinechange-oncodechanges";

		public const string ATTRIBUTE_ATOMIC_READ_WRITE = "atomic-read-write";

		public const string ATTRIBUTE_STRICT = "strict";

		public const string ATTRIBUTE_SUSPEND_STRICT = "suspend_strict";

		public const string ATTRIBUTE_LTICK = "LTICK";

		public const string ATTRIBUTE_CONTAINS_IMPLICIT_ENUM = "contains_implicit_enum";

		public static string ATTRIBUTE_REDUCED_BP_SET = "reduced_bp_set";

		public static string ATTRIBUTE_SUPPRESS_MESSAGES = "suppress_messages";

		public static string ATTRIBUTE_PARAMS_MINIMAL_NUMBER = "minimal_number";

		public static string ATTRIBUTE_PARAMS_DEFAULT_NUMBER = "default_number";

		public static string ATTRIBUTE_VARIABLE_LENGTH_ARRAY = "variable_length_array";

		public const string ATTRIBUTE_VARIABLE_LENGTH_ARRAY_ORIGINAL_SCOPE = "variable_length_array_original_scope";

		public static string ATTRIBUTE_IMPLICIT_INPUT = "implicit_input";

		public static string ATTRIBUTE_CALL_ON_TYPE_CHANGE = "call_on_type_change";

		public static string ATTRIBUTE_ALLOW_EXTERNAL_VAR_IN_OUT_ACCESS = "allow_external_var_in_out_access";

		public static string ATTRIBUTE_NO_RELINK_CODE = "no_relink_code";

		public static string ATTRIBUTE_ENABLE_DYNAMIC_CREATION = "enable_dynamic_creation";

		public static string ATTRIBUTE_IMPLICIT_REFERENCE_TYPE = "implicit_reference_type";

		public static string ATTRIBUTE_IGNORE_ERROR_361 = "ignore_error_361";

		public static string ATTRIBUTE_IMPLICIT_ENUM_TYPE = "implicit_enum_type";

		public static string ATTRIBUTE_SUPPRESS_WRN_C0410 = "suppress_wrn_C0410";

		public static string ATTRIBUTE_TO_STRING_FUNCTION = "to_string_function";

		public static string ATTRIBUTE_TO_WSTRING_FUNCTION = "to_wstring_function";

		public static string ATTRIBUTE_TO_STRING = "to_string";

		public static string ATTRIBUTE_FB_ALLOC_PLUS = "allocation_plus_fb";

		public static string ATTRIBUTE_THREAD_SAFE = "thread_safe";

		public static string ATTRIBUTE_TASKLOCALGVL = "task_local_gvl";

		public static string ATTRIBUTE_NOASSIGN = "no_assign";

		public static string ATTRIBUTE_NOASSIGN_WARNING = "no_assign_warning";

		public static string ATTRIBUTE_IMPLICIT_INIT_FUN = "implicit_init_function";

		public static string ATTRIBUTE_ANALYSIS = "analysis";

		public static string ATTRIBUTE_NAMING = "naming";

		public static string ATTRIBUTE_IMPLICIT_INST_VAR = "implicit_inst_var";

		public const string ATTRIBUTE_NO_EXPLICIT_CALL = "no_explicit_call";

		public const string ATTRIBUTE_NO_JUMPINSTRUCTION_OPTIMIZATION = "no_jumpinstruction_optimization";

		public const string ATTRIBUTE_SUPPRESS_WARNING = "suppress_warning";

		public const string ATTRIBUTE_NO_VIRTUAL_ACTIONS = "no_virtual_actions";

		public const string ATTRIBUTE_ABSTRACT = "abstract";

		public const string ATTRIBUTE_PROPERTYOBJECT_GUID = "propertyobject-guid";

		public const string ATTRIBUTE_PROHIBIT_MULTIPLE_INSTANCE_CALLS = "prohibit-multiple-instance-calls";

		public const string ATTRIBUTE_SUBSEQUENT = "subsequent";

		public const string MANGLED_NAME = "mangled_name";

		public const string OVERLOADS = "overloads";

		public const string OVERLOADED = "overloaded";

		public const string ATTRIBUTE_EXTERNAL = "external";

		public const string ATTRIBUTE_TASK = "task";

		public const string ATTRIBUTE_NO_FUNCTION_POINTER = "no-function-pointer";

		public const string ATTRIBUTE_INIT_RELATED_CODE = "init_related_code";

		public const string ATTRIBUTE_MEMSETINIT = "memsetinit";

		public const string ATTRIBUTE_OLD_INPUT_ASSIGNMENTS = "old_input_assignments";

		public const string ATTRIBUTE_INIT_INPUTS_ON_ONLCHANGE = "init_inputs_on_onlchange";

		public const string ATTRIBUTE_NO_FAST_ONLINE_CHANGE = "no_fast_online_change";

		public const string ATTRIBUTE_NO_DEFAULT_INITIALIZATION = "no_default_initialisation";

		public const string ATTRIBUTE_NO_MISALIGNMENT_CHECK = "no_misalignment_check";

		public const string ATTRIBUTE_SINGLETON = "singleton";

		public const string ATTRIBUTE_NOUNSIGNEDCHECK = "nounsignedcheck";

		public const string ATTRIBUTE_NO_DEFAULT_INIT = "no_default_init";

		public const string ATTRIBUTE_INSTANCE_VAR = "instancevar";

		public const string ATTRIBUTE_FRIEND_INSTANCE = "friend_instance";

		public const string ATTRIBUTE_IGNORE_IN_INTERFACE = "ignore_in_interface";

		public const string ATTRIBUTE_GENERATED_INTELLISENSE_ITEM = "generated_intellisense_item";

		public const string ATTRIBUTE_IMPLICIT_PARAMETER = "implicit-parameter";

		public const string ATTRIBUTE_ALLOW_UNQUALIFIED_ACCESS = "allow_unqualified_access";
	}
}
