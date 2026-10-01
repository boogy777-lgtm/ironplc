using System;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.TargetSettings;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ISecondLevelPlaceholderResolution
	{
		string ResolvePlaceholder(ITargetSettings tarset, Guid typeGuidFirstLevelResolver, string stPlaceholderName, string stDefaultLibrary, out string stDefaultNamespace);
	}
}
