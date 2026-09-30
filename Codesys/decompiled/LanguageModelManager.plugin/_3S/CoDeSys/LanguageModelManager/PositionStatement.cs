using System;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000099 RID: 153
	public abstract class PositionStatement : Statement, IPositionExprement, ILengthExprement
	{
		// Token: 0x0600093F RID: 2367 RVA: 0x0001596F File Offset: 0x0001496F
		protected PositionStatement(IToken token) : base(token)
		{
		}

		// Token: 0x06000940 RID: 2368 RVA: 0x00015978 File Offset: 0x00014978
		protected PositionStatement()
		{
		}

		// Token: 0x1700021A RID: 538
		// (get) Token: 0x06000941 RID: 2369 RVA: 0x00015980 File Offset: 0x00014980
		// (set) Token: 0x06000942 RID: 2370 RVA: 0x00015988 File Offset: 0x00014988
		[Obfuscation(Feature = "rename")]
		public override IMinimalPosition PositionIntern
		{
			get
			{
				return this.m_position;
			}
			set
			{
				this.m_position = value;
			}
		}

		// Token: 0x1700021B RID: 539
		// (get) Token: 0x06000943 RID: 2371 RVA: 0x00015991 File Offset: 0x00014991
		// (set) Token: 0x06000944 RID: 2372 RVA: 0x00015999 File Offset: 0x00014999
		[DefaultSerialization("Length")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		public override short LengthIntern
		{
			get
			{
				return this.m_sLength;
			}
			set
			{
				this.m_sLength = value;
			}
		}

		// Token: 0x04000143 RID: 323
		[Obfuscation(Feature = "rename")]
		private IMinimalPosition m_position;

		// Token: 0x04000144 RID: 324
		[Obfuscation(Feature = "rename")]
		protected short m_sLength;
	}
}
