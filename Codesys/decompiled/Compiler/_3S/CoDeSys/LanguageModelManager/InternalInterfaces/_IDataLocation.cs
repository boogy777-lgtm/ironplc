using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _IDataLocation : IDataLocation2, IDataLocation
	{
		new ushort Area { get; set; }

		new int Offset { get; set; }

		new byte BitNr { get; set; }
	}
}
