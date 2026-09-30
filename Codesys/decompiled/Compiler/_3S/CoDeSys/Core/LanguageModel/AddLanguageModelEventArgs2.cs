using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedClass]
	public class AddLanguageModelEventArgs2 : AddLanguageModelEventArgs
	{
		private List<ILanguageModel> _structuredlanguagemodellist;

		private ILanguageModelBuilder _lmbuilder;

		public List<ILanguageModel> StructuredLanguageModelList => _structuredlanguagemodellist;

		public ILanguageModelBuilder LanguageModelBuilder => _lmbuilder;

		public AddLanguageModelEventArgs2(Guid guidApplication, ILanguageModelList languagemodellist, List<ILanguageModel> structuredlanguagemodellist, ILanguageModelBuilder lmbuilder)
			: base(guidApplication, languagemodellist)
		{
			_structuredlanguagemodellist = structuredlanguagemodellist;
			_lmbuilder = lmbuilder;
		}
	}
}
