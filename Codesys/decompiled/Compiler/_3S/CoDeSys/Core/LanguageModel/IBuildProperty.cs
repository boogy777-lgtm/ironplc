using System;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Objects;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IBuildProperty : IObjectProperty, IGenericObject, IArchivable, ICloneable, IComparable
	{
		[Obsolete("'ExcludeFromBuild' is now an inherited property. Use 'ILanguageModelManager17.IsExcludedFromBuild' in order to retrieve the effective status, or 'IBuildProperty3.ExcludeFromBuildLocal' to get or set the explictly stored value for this object.")]
		bool ExcludeFromBuild { get; set; }

		bool External { get; set; }

		bool EnableSystemCall { get; set; }

		string CompilerDefines { get; set; }
	}
}
