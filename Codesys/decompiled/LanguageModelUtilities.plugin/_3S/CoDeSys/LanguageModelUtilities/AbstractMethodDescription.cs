using System.Collections.Generic;
using System.Diagnostics;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[DebuggerDisplay("{ParentSignature.Name}.{Signature.Name}")]
	public class AbstractMethodDescription : IAbstractMethodDescription
	{
		public ISignature Signature { get; private set; }

		public ISignature ParentSignature { get; private set; }

		public IPreCompileContext PreCompileContext { get; private set; }

		internal AbstractMethodDescription(ISignature signature, ISignature parentSignature, IPreCompileContext preCompileContext)
		{
			Signature = signature;
			ParentSignature = parentSignature;
			PreCompileContext = preCompileContext;
		}

		public override bool Equals(object obj)
		{
			if (!(obj is AbstractMethodDescription abstractMethodDescription))
			{
				return false;
			}
			if (Signature.Name.Equals(abstractMethodDescription.Signature.Name) && ParentSignature.Name.Equals(abstractMethodDescription.ParentSignature.Name))
			{
				return PreCompileContext.Equals(abstractMethodDescription.PreCompileContext);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return ((838576303 * -1448569836 + EqualityComparer<string>.Default.GetHashCode(Signature.Name)) * -1448569836 + EqualityComparer<string>.Default.GetHashCode(ParentSignature.Name)) * -1448569836 + PreCompileContext.GetHashCode();
		}
	}
}
