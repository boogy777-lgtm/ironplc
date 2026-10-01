using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler.Serialization
{
	[ReleasedInterface]
	public interface IDataManagerSerializable
	{
		IList<_IDataSegment> _DataSegments { get; set; }

		IList<IArea> _Areas { get; set; }

		int FirstArea { get; set; }

		void AfterDeserialize();
	}
}
