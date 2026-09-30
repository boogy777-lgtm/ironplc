using System;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Objects;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IBuildProperty2 : IBuildProperty, IObjectProperty, IGenericObject, IArchivable, ICloneable, IComparable
	{
		bool LinkAlways { get; set; }
	}
}
