using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Objects;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IBuildProperty6 : IBuildProperty5, IBuildProperty4, IBuildProperty3, IBuildProperty2, IBuildProperty, IObjectProperty, IGenericObject, IArchivable, ICloneable, IComparable
	{
		new IList<string> Undefines { get; set; }
	}
}
