using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedClass]
	public class AddLanguageModelEventArgs3 : AddLanguageModelEventArgs2
	{
		public bool OnlineChange { get; private set; }

		public AddLanguageModelEventArgs3(Guid guidApplication, ILanguageModelList languagemodellist, List<ILanguageModel> structuredlanguagemodellist, ILanguageModelBuilder lmbuilder, bool bOnlineChange)
			: base(guidApplication, languagemodellist, structuredlanguagemodellist, lmbuilder)
		{
			OnlineChange = bOnlineChange;
		}
	}
}
