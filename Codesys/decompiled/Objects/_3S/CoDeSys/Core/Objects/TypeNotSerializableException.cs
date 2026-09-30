using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x020000CD RID: 205
	[ReleasedClass]
	public class TypeNotSerializableException : Exception
	{
		// Token: 0x17000140 RID: 320
		// (get) Token: 0x06000341 RID: 833 RVA: 0x000056E0 File Offset: 0x000038E0
		public Guid TypeGuid
		{
			get
			{
				return TypeGuidAttribute.FromType(this.Type).Guid;
			}
		}

		// Token: 0x17000141 RID: 321
		// (get) Token: 0x06000342 RID: 834 RVA: 0x000056F2 File Offset: 0x000038F2
		// (set) Token: 0x06000343 RID: 835 RVA: 0x000056FA File Offset: 0x000038FA
		public Type Type { get; private set; }

		// Token: 0x06000344 RID: 836 RVA: 0x00005703 File Offset: 0x00003903
		public TypeNotSerializableException(Type type)
		{
			this.Type = type;
		}
	}
}
