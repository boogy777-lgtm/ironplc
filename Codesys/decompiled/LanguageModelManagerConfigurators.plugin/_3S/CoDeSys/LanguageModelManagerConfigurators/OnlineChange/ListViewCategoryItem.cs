using System.Windows.Forms;

namespace _3S.CoDeSys.LanguageModelManagerConfigurators.OnlineChange
{
	internal class ListViewCategoryItem : ListViewItem
	{
		public AllocationPlusModel Model { get; }

		internal ListViewCategoryItem()
		{
			Model = new AllocationPlusModel();
		}

		public override string ToString()
		{
			return base.Name;
		}
	}
}
