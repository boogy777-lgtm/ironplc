using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelUtilities.RetainsUpdatedInCycleReporter
{
	internal struct InstanceInformation
	{
		public string Instance { get; set; }

		public IVariable Variable { get; set; }

		public ISignature Signature { get; set; }
	}
}
