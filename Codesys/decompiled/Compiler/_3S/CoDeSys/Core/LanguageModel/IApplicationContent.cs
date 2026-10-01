using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IApplicationContent
	{
		IPOUInfoStruct[] POUs { get; }

		IDUTInfoStruct[] DUTs { get; }

		IGVLInfoStruct[] GVLs { get; }

		IFBInfoStruct[] FBs { get; }

		IMethodInfoStruct[] Methods { get; }

		string[] Libs { get; }
	}
}
