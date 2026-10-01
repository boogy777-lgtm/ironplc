using System;
using System.Collections.Generic;
using _3S.CoDeSys.Controls.Controls;

namespace _3S.CoDeSys.LanguageModelManagerConfigurators
{
	internal class WarningNode : ITreeTableNode
	{
		public const int COLIDX_NAME = 0;

		public const int COLIDX_CHECKSTATE = 1;

		private readonly WarningModel _model;

		private readonly Warning _warning;

		private readonly List<WarningNode> _alChildNodes = new List<WarningNode>();

		private readonly WarningNode _parentNode;

		private SeveralCheckedStates _state;

		public ITreeTableNode Parent => (ITreeTableNode)(object)_parentNode;

		public bool HasChildren => _alChildNodes.Count != 0;

		public int ChildCount => _alChildNodes.Count;

		internal List<WarningNode> ChildList => _alChildNodes;

		internal SeveralCheckedStates State
		{
			get
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				return _state;
			}
			set
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				//IL_0002: Unknown result type (might be due to invalid IL or missing references)
				//IL_0008: Unknown result type (might be due to invalid IL or missing references)
				//IL_000d: Unknown result type (might be due to invalid IL or missing references)
				//IL_000e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0030: Expected I4, but got Unknown
				_state = value;
				SeveralCheckedStates state = _state;
				switch ((int)state)
				{
				case 0:
					_warning.Active = false;
					_warning.AsError = false;
					break;
				case 2:
				case 6:
					_warning.Active = true;
					_warning.AsError = false;
					break;
				case 5:
					_warning.AsError = true;
					break;
				case 1:
				case 3:
				case 4:
					break;
				}
			}
		}

		internal WarningNode(WarningModel model, Warning warning, WarningNode parentNode)
		{
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			_model = model;
			_warning = warning;
			_parentNode = parentNode;
			_state = (SeveralCheckedStates)0;
			if (_warning.Active)
			{
				_state = (SeveralCheckedStates)6;
			}
			if (_warning.AsError)
			{
				_state = (SeveralCheckedStates)5;
			}
		}

		public object GetValue(int nColumnIndex)
		{
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			switch (nColumnIndex)
			{
			case 0:
				if (IsRoot())
				{
					return Strings.Warnings;
				}
				return _warning.IdString + ": " + _warning.Message;
			case 1:
				return _state;
			default:
				throw new ArgumentOutOfRangeException("nColumnIndex");
			}
		}

		public void SetValue(int nColumnIndex, object value)
		{
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Expected O, but got Unknown
			switch (nColumnIndex)
			{
			case 1:
				State = (SeveralCheckedStates)value;
				((AbstractTreeTableModel)_model).RaiseChanged(new TreeTableModelEventArgs((ITreeTableNode)null, -1, (ITreeTableNode)(object)this));
				StateChanged();
				_model.UpdateState();
				break;
			default:
				throw new InvalidOperationException("Cannot modify the cells of this node.");
			case 0:
				break;
			}
		}

		public bool IsRoot()
		{
			return _parentNode == null;
		}

		public ITreeTableNode GetChild(int nIndex)
		{
			return (ITreeTableNode)(object)_alChildNodes[nIndex];
		}

		public int GetIndex(ITreeTableNode node)
		{
			for (int i = 0; i < _alChildNodes.Count; i++)
			{
				if (_alChildNodes[i] == node)
				{
					return i;
				}
			}
			return -1;
		}

		public bool IsEditable(int nColumnIndex)
		{
			return nColumnIndex == 1;
		}

		public void SwapChildren(int nIndex1, int nIndex2)
		{
			WarningNode value = _alChildNodes[nIndex1];
			_alChildNodes[nIndex1] = _alChildNodes[nIndex2];
			_alChildNodes[nIndex2] = value;
		}

		internal void Refill(IEnumerable<Warning> alWarnings)
		{
			foreach (Warning alWarning in alWarnings)
			{
				WarningNode item = new WarningNode(_model, alWarning, this);
				_alChildNodes.Add(item);
			}
		}

		internal void RaiseChanged()
		{
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Expected O, but got Unknown
			((AbstractTreeTableModel)_model).RaiseChanged(new TreeTableModelEventArgs((ITreeTableNode)null, -1, (ITreeTableNode)(object)this));
		}

		private void StateChanged()
		{
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Expected O, but got Unknown
			foreach (WarningNode alChildNode in _alChildNodes)
			{
				alChildNode.State = _state;
				((AbstractTreeTableModel)_model).RaiseChanged(new TreeTableModelEventArgs((ITreeTableNode)null, -1, (ITreeTableNode)(object)alChildNode));
				alChildNode.StateChanged();
			}
		}

		public Warning GetWarning()
		{
			return _warning;
		}

		internal void Save(HashSet<int> hsDisabledWarningIds, HashSet<int> hsWarningsAsErrorsIds)
		{
			foreach (WarningNode alChildNode in _alChildNodes)
			{
				Warning warning = alChildNode.GetWarning();
				if (!warning.Active && !hsDisabledWarningIds.Contains(warning.Id))
				{
					hsDisabledWarningIds.Add(warning.Id);
				}
				if (warning.AsError)
				{
					hsWarningsAsErrorsIds.Add(warning.Id);
				}
			}
		}
	}
}
