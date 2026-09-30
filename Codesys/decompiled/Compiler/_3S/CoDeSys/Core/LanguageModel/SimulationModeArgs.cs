using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedClass]
	public class SimulationModeArgs : EventArgs
	{
		private bool _bSimulation;

		private Guid _guidPlcLogicObject = Guid.Empty;

		public bool Simulation => _bSimulation;

		public Guid PlCLogicObjectGuid => _guidPlcLogicObject;

		public SimulationModeArgs(bool bSimulation, Guid guidPlcLogicObject)
		{
			_bSimulation = bSimulation;
			_guidPlcLogicObject = guidPlcLogicObject;
		}
	}
}
