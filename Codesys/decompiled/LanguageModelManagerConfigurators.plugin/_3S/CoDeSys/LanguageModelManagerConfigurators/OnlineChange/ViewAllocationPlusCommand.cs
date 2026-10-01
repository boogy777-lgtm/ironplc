using System;
using System.Drawing;
using _3S.CoDeSys.Core.Commands;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.OnlineHelp;
using _3S.CoDeSys.Core.Views;

namespace _3S.CoDeSys.LanguageModelManagerConfigurators.OnlineChange
{
	[AssociatedOnlineHelpTopic("codesys.chm::/_cds_cmd_memory_reserve_online_change.htm")]
	[TypeGuid("{CA5B4F8A-6BE8-4ED3-9494-E1D99E716C8B}")]
	public class ViewAllocationPlusCommand : IStandardCommand, ICommand
	{
		private static readonly Guid GUID_VIEWCOMMANDCATEGORY = new Guid("{7550CC92-8A35-4635-A6A8-7B1882215928}");

		private static readonly string[] BATCH_COMMAND = new string[2] { "view", "allocationplus" };

		public string ToolTipText => Name;

		public Icon LargeIcon => SmallIcon;

		public string Description => string.Empty;

		public string Name => Strings.OnlineChangeAllocPlusCmdName;

		public bool Enabled => true;

		public Icon SmallIcon => null;

		public Guid Category => GUID_VIEWCOMMANDCATEGORY;

		public string[] BatchCommand => BATCH_COMMAND;

		public string[] CreateBatchArguments()
		{
			return new string[0];
		}

		public bool IsVisible(bool bContextMenu)
		{
			return !bContextMenu;
		}

		public void ExecuteBatch(string[] arguments)
		{
			if (arguments == null)
			{
				throw new ArgumentNullException("arguments");
			}
			if (arguments.Length != 0)
			{
				throw new BatchTooManyArgumentsException(BatchCommand, arguments.Length, 0);
			}
			if (APEnvironment.Engine.Frame == null)
			{
				throw new BatchInteractiveException(BatchCommand);
			}
			IView view = APEnvironment.Engine.Frame.OpenView(AllocationPlusViewFactory.TypeGuid, null);
			if (view != null)
			{
				APEnvironment.Engine.Frame.ActiveView = view;
			}
		}

		public void AddedToUI()
		{
		}

		public void RemovedFromUI()
		{
		}
	}
}
