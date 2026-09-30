using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedClass]
	public abstract class XmlConstants
	{
		public const string TAG_LANGUAGE_MODEL_LIST = "language-model-list";

		public const string TAG_LANGUAGE_MODEL = "language-model";

		public const string TAG_POU = "pou";

		public const string TAG_APPLICATION = "application";

		public const string TAG_CLONE = "application-clone";

		public const string TAG_DEVICE = "device";

		public const string TAG_METHOD = "method";

		public const string TAG_ACTION = "action";

		public const string TAG_DATA_TYPE = "data-type";

		public const string TAG_GLOBAL_INTERFACE = "global-interface";

		public const string TAG_INTERFACE = "interface";

		public const string TAG_TARGETID = "device-identification";

		public const string ATTR_TARGETTYPE = "type";

		public const string ATTR_TARGETNAME = "target-id";

		public const string ATTR_TARGETVERSION = "version";

		public const string ATTR_STRING_TABLE_REF = "string-table-reference";

		public const string TAG_BODY = "body";

		public const string ATTR_OBJECT_ID = "object-id";

		public const string ATTR_ID = "id";

		public const string ATTR_TASK_ID = "task-id";

		public const string ATTR_LIBRARY_ID = "library-id";

		public const string ATTR_LIBREF_RESOLUTION_GUID = "library_resolution_id";

		public const string ATTR_DEFAULT_LIBRARY = "default-library";

		public const string ATTR_POU_ID = "pou-id";

		public const string ATTR_NAME = "name";

		public const string ATTR_APPLICATION = "application";

		public const string ATTR_SIMULATION_APPLICATION = "simulation-application";

		public const string ATTR_APPLICATION_DYNAMIC_MEMORY = "dynamic_memory_size";

		public const string ATTR_APPLICATION_GENERATE_CONTENT = "generate_content";

		public const string ATTR_SLOT = "slot";

		public const string ATTR_DOWNLOAD_SLOT = "download_slot";

		public const string ATTR_ONLINE_CHANGE_SLOT = "online_change_slot";

		public const string ATTR_PLCLOGIC = "plclogic";

		public const string ATTR_PLCLOGIC_ID = "plclogic-id";

		public const string ATTR_APPLICATION_ID = "application-id";

		public const string ATTR_PARENT_APPLICATION_ID = "parent-application-id";

		public const string ATTR_MEMORYSETTINGS_PROVIDER_ID = "memory-settings-provider-id";

		public const string ATTR_EXTERNAL = "external";

		public const string ATTR_ENABLE_SYSTEM_CALL = "enable-system-call";

		public const string ATTR_INHIBIT_ONLCHANGE = "inhibit-online-change";

		public const string ATTR_GENERATED_INTELLISENSE_ITEM = "generated_intellisense_item";

		public const string TAG_LIBRARYLIST = "library-list";

		public const string TAG_LIBRARY = "library";

		public const string TAG_LIB_PLACEHOLDER = "placeholder";

		public const string ATTR_NAMESPACE = "namespace";

		public const string ATTR_DEFAULT_NAMESPACE = "default-namespace";

		public const string ATTR_PUBLISH_SYMBOLS_IN_CONTAINER = "publish-symbols-in-container";

		public const string ATTR_LIB_LINK_ALL_CONTENT = "link-all-content";

		public const string ATTR_LIB_SYSTEM_APPLICATION = "system-application";

		public const string ATTR_QUALIFIED_ONLY = "qualified-access-only";

		public const string ATTR_SYSTEMLIBRARY = "system-library";

		public const string TAG_PARAMETER = "parameter";

		public const string TAG_PARAMETER_LIST = "parameter-list";

		public const string ATTR_PARAMETER_NAME = "name";

		public const string ATTR_PARAMETER_VALUE = "value";

		public const string TAG_TASKLIST = "task-list";

		public const string TAG_TASK = "task";

		public const string ATTR_PARENTSYNCHTASK = "parent-synch-task";

		public const string TAG_AUXILIARY_DATA_LIST = "auxiliary-data-list";

		public const string TAG_AUXILIARY_DATA = "auxiliary-data";

		public const string ATTR_AUX_DATA_ID = "id";

		public const string ATTR_AUX_DATA_TYPE = "type";
	}
}
