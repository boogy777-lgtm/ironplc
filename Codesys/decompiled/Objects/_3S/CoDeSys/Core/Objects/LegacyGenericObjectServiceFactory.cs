using System;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x0200003D RID: 61
	internal sealed class LegacyGenericObjectServiceFactory : IGenericObjectServiceFactory
	{
		// Token: 0x0600010A RID: 266 RVA: 0x00003984 File Offset: 0x00001B84
		public IGenericObjectService Create(Type type)
		{
			return new LegacyGenericObjectService(type);
		}
	}
}
