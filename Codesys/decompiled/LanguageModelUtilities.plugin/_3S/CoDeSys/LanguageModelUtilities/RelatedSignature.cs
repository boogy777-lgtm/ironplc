using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	internal class RelatedSignature : IRelatedSignature2, IRelatedSignature
	{
		public ISignature Signature { get; private set; }

		public string Reason { get; private set; }

		public ESignatureRelationship SignatureRelationship { get; private set; }

		public RelatedSignature(ISignature2 signature, string stReason, ESignatureRelationship eSignatureRelationship)
		{
			Signature = signature;
			Reason = stReason;
			SignatureRelationship = eSignatureRelationship;
		}
	}
}
