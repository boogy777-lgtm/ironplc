using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IPropertyAdressInfoWithOffset
	{
		int OffsetValueGetter { get; set; }

		int OffsetValueSetter { get; set; }
	}
}
