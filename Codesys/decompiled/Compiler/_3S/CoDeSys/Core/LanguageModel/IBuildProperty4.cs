using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Objects;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IBuildProperty4 : IBuildProperty3, IBuildProperty2, IBuildProperty, IObjectProperty, IGenericObject, IArchivable, ICloneable, IComparable
	{
		IList<string> Undefines { get; }
	}
}
