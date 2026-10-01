using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedClass]
	public static class LMMSlotAttributes
	{
		public static string[] SLOT_ATTRIBUTES = new string[2] { "call_before_global_exit_slot", "call_after_online_change_slot" };

		public static string[] SLOT_ATTRIBUTES_WITH_PARAMETERS = new string[3] { "call_after_global_init_slot", "call_before_online_change_concurrent_slot", "call_after_online_change_concurrent_slot" };
	}
}
