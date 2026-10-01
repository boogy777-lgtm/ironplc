using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	internal class CrossReferenceIdentifierInfo : IIdentifierInfo2, IIdentifierInfo
	{
		public ISignature ContainingSignature { get; private set; }

		public string Name { get; private set; }

		public string Comment { get; private set; }

		public IdentifierInfoFlag Flags { get; private set; }

		public IType Type { get; private set; }

		public IVariable Variable { get; internal set; }

		public ISignature Signature { get; internal set; }

		public IScope Scope { get; private set; }

		public CrossReferenceIdentifierInfo(ISignature containingSignature, string stName, string stComment, IdentifierInfoFlag flags, IType type, IVariable variable, ISignature signature, IScope scope)
		{
			ContainingSignature = containingSignature;
			Name = stName;
			Comment = stComment;
			Flags = flags;
			Variable = variable;
			Type = type;
			Signature = signature;
			Scope = scope;
		}
	}
}
