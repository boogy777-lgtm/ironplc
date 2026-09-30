using System;
using _3S.CoDeSys.Controls.Controls;

namespace _3S.CoDeSys.LanguageModelManagerConfigurators.OnlineChange
{
	internal class ModelNode : ITreeTableNode2, ITreeTableNode
	{
		internal const int COLUMN_NAME = 0;

		internal const int COLUMN_SIZE = 1;

		internal const int COLUMN_INST_COUNT = 2;

		internal const int COLUMN_RESERVE = 3;

		internal const int COLUMN_RESERVE_SIZE_ALL_INST = 4;

		internal const int COLUMN_REMAINING_RESERVE_SIZE = 5;

		private readonly int _nProjectHandle;

		private readonly int _size;

		private readonly int _numInstances;

		private readonly int _remainingMemReserveSize;

		private readonly MemoryReserveViewContext _viewContext;

		public string PouName { get; set; }

		public int MemoryReserveSize { get; private set; }

		public int MemoryReserveSizeAllInstances { get; private set; }

		internal bool Compiled => _size > 0;

		internal Guid ObjectGuid { get; }

		public int ChildCount => 0;

		public bool HasChildren => false;

		public ITreeTableNode Parent => null;

		internal ModelNode(int nProjectHandle, Guid objGuid, string pouName, int size, int memoryReserveSize, int numInstances, int remainingMemReserveSize, MemoryReserveViewContext viewContext)
		{
			_nProjectHandle = nProjectHandle;
			ObjectGuid = objGuid;
			PouName = pouName;
			MemoryReserveSize = memoryReserveSize;
			_viewContext = viewContext;
			_size = size;
			_numInstances = numInstances;
			_remainingMemReserveSize = remainingMemReserveSize;
			UpdateMemoryReserveSizeForAllInstances();
		}

		public ITreeTableNode GetChild(int nIndex)
		{
			return null;
		}

		public int GetIndex(ITreeTableNode node)
		{
			return -1;
		}

		public string GetToolTipText(int nColumnIndex)
		{
			return string.Empty;
		}

		public object GetValue(int nColumnIndex)
		{
			switch (nColumnIndex)
			{
			case 0:
				return PouName;
			case 1:
				if (Compiled)
				{
					return _size;
				}
				return string.Empty;
			case 2:
				if (Compiled)
				{
					return _numInstances;
				}
				return string.Empty;
			case 3:
				if (Utilities.MemoryReserveSupported)
				{
					return MemoryReserveSize;
				}
				return string.Empty;
			case 4:
				if (Compiled && Utilities.MemoryReserveSupported)
				{
					return MemoryReserveSizeAllInstances;
				}
				return string.Empty;
			case 5:
				if (Compiled && Utilities.MemoryReserveSupported)
				{
					return _remainingMemReserveSize;
				}
				return string.Empty;
			default:
				return null;
			}
		}

		public bool IsEditable(int nColumnIndex)
		{
			if (nColumnIndex == 3)
			{
				return _viewContext.ChangingMemoryReserveSizeEnabled;
			}
			return false;
		}

		public void SetValue(int nColumnIndex, object value)
		{
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Expected O, but got Unknown
			if (nColumnIndex == 3 && int.TryParse(value.ToString(), out var result) && Utilities.StoreNewReserveValueInObject(_nProjectHandle, ObjectGuid, result))
			{
				MemoryReserveSize = result;
				UpdateMemoryReserveSizeForAllInstances();
				_viewContext.UpToDate = false;
				_viewContext.ActiveModel.RaiseChanged(new TreeTableModelEventArgs((ITreeTableNode)null, -1, (ITreeTableNode)(object)this));
			}
		}

		private void UpdateMemoryReserveSizeForAllInstances()
		{
			MemoryReserveSizeAllInstances = _numInstances * MemoryReserveSize;
		}

		public void SwapChildren(int nIndex1, int nIndex2)
		{
		}
	}
}
