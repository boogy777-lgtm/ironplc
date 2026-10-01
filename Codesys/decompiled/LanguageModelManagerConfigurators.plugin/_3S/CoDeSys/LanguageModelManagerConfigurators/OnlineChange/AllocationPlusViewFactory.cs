using System;
using System.Drawing;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Views;

namespace _3S.CoDeSys.LanguageModelManagerConfigurators.OnlineChange
{
	[TypeGuid("{E6B3B4DF-A294-46C3-97A3-507E5B40D1BD}")]
	public class AllocationPlusViewFactory : IViewFactory
	{
		public Icon LargeIcon => SmallIcon;

		public string Description => string.Empty;

		public string Name => Strings.OnlineChangeAllocPlusViewName;

		public Icon SmallIcon => null;

		public static Guid TypeGuid => ((TypeGuidAttribute)typeof(AllocationPlusViewFactory).GetCustomAttributes(typeof(TypeGuidAttribute), inherit: false)[0]).Guid;

		public IView Create()
		{
			return new OnlineChangeAllocationPlusView();
		}
	}
}
