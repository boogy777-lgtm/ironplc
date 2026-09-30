using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedClass]
	public class IsHiddenVariableEventArgs : EventArgs
	{
		private IVariable _variable;

		private GUIHidingFlags _flagsToConsider;

		private bool _bHide;

		public IVariable Variable => _variable;

		public GUIHidingFlags Flags => _flagsToConsider;

		public bool Hide
		{
			get
			{
				return _bHide;
			}
			set
			{
				_bHide = value;
			}
		}

		public IsHiddenVariableEventArgs(IVariable variable, GUIHidingFlags flagsToConsider)
		{
			_variable = variable;
			_flagsToConsider = flagsToConsider;
		}
	}
}
