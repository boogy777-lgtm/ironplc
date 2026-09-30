using System.Collections.ObjectModel;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	internal class SubelementItemCollection : KeyedCollection<string, SubelementItem>
	{
		protected override string GetKeyForItem(SubelementItem item)
		{
			return item.Name;
		}
	}
}
