using System;
using System.Collections;

namespace _3S.CoDeSys.LanguageModelManagerConfigurators.OnlineChange
{
	internal class SizeColumnComparer : IComparer
	{
		private readonly int _iColumn = -1;

		private readonly bool _bAscending = true;

		public SizeColumnComparer(int iColumnIndex, bool bAscending)
		{
			_iColumn = iColumnIndex;
			_bAscending = bAscending;
		}

		public int Compare(object o1, object o2)
		{
			if (o1 == o2)
			{
				return 0;
			}
			ModelNode modelNode = o1 as ModelNode;
			ModelNode modelNode2 = o2 as ModelNode;
			if (modelNode == null || modelNode2 == null)
			{
				return -1;
			}
			object value = modelNode.GetValue(_iColumn);
			object value2 = modelNode2.GetValue(_iColumn);
			int num = -1;
			num = ((modelNode.Compiled && modelNode2.Compiled) ? Convert.ToInt32(value).CompareTo(Convert.ToInt32(value2)) : ((modelNode.Compiled || modelNode2.Compiled) ? ((modelNode.Compiled || !modelNode2.Compiled) ? 1 : (-1)) : 0));
			if (!_bAscending)
			{
				num *= -1;
			}
			return num;
		}
	}
}
