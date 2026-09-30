using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ISignatureMemberHierachyInfo
	{
		string MemberName { get; set; }

		ISignature Signature { get; }

		int Level { get; }

		object Tag { get; set; }

		bool EmptyLevel { get; set; }
	}
}
