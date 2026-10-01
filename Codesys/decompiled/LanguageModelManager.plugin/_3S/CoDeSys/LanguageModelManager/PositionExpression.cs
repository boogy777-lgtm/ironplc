using System;
using System.Reflection;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000069 RID: 105
	public abstract class PositionExpression : Expression, IPositionExprement, ILengthExprement
	{
		// Token: 0x060006D8 RID: 1752 RVA: 0x00012299 File Offset: 0x00011299
		protected PositionExpression(IToken token) : base(token)
		{
		}

		// Token: 0x060006D9 RID: 1753 RVA: 0x0000C3D6 File Offset: 0x0000B3D6
		protected PositionExpression()
		{
		}

		// Token: 0x1700017F RID: 383
		// (get) Token: 0x060006DA RID: 1754 RVA: 0x000122A2 File Offset: 0x000112A2
		// (set) Token: 0x060006DB RID: 1755 RVA: 0x000122AA File Offset: 0x000112AA
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

		// Token: 0x17000180 RID: 384
		// (get) Token: 0x060006DC RID: 1756 RVA: 0x000122B3 File Offset: 0x000112B3
		// (set) Token: 0x060006DD RID: 1757 RVA: 0x000122BB File Offset: 0x000112BB
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

		// Token: 0x040000E6 RID: 230
		[Obfuscation(Feature = "rename")]
		private IMinimalPosition m_position;

		// Token: 0x040000E7 RID: 231
		[Obfuscation(Feature = "rename")]
		protected short m_sLength;
	}
}
