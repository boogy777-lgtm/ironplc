using System.Collections.Generic;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace CODESYS.Parser35210.Declaration
{
	public class POUSyntax : IPOUSyntax
	{
		private readonly IList<POUSyntax> subpoulist = new List<POUSyntax>();

		public _ISequenceStatement Declaration { get; set; }

		public _ISequenceStatement Implementation { get; set; }

		public IEnumerable<IPOUSyntax> SubPOUs => subpoulist;

		internal void AddSubpou(POUSyntax subpou)
		{
			subpoulist.Add(subpou);
		}
	}
}
