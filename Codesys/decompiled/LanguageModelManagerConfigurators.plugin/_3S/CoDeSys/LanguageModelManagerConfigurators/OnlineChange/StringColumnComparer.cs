using System.Collections;

namespace _3S.CoDeSys.LanguageModelManagerConfigurators.OnlineChange
{
	internal class StringColumnComparer : IComparer
	{
		private readonly int _iColumn = -1;

		private readonly bool _bAscending = true;

		public StringColumnComparer(int iColumnIndex, bool bAscending)
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
			if (o1 == null || o2 == null)
			{
				return -1;
			}
			int num = modelNode.GetValue(_iColumn).ToString().CompareTo(modelNode2.GetValue(_iColumn).ToString());
			if (_bAscending)
			{
				num *= -1;
			}
			return num;
		}
	}
}
