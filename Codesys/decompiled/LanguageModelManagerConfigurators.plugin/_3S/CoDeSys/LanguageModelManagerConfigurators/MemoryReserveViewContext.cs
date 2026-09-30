using _3S.CoDeSys.Controls.Controls;
using _3S.CoDeSys.LanguageModelManagerConfigurators.OnlineChange;

namespace _3S.CoDeSys.LanguageModelManagerConfigurators
{
	internal class MemoryReserveViewContext
	{
		internal AllocationPlusModel AllFBsModel { get; set; }

		internal ITreeTableModel ActiveModel { get; set; }

		internal bool ChangingMemoryReserveSizeEnabled { get; set; }

		internal bool UpToDate { get; set; }
	}
}
