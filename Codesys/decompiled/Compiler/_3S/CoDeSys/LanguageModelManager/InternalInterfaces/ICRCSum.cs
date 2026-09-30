using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface ICRCSum
	{
		uint CRC32Finish(byte[] bBuffer, int iSize);

		void CRC32Update(byte[] bBuffer, int iSize);
	}
}
