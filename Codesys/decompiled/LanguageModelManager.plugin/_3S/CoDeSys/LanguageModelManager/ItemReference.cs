using System;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000060 RID: 96
	[TypeGuid("{c68a5803-1e1f-41ac-b315-cc271e861d57}")]
	[StorageVersion("3.3.0.0")]
	public abstract class ItemReference : PragmaExpression, _IItemReference, _IPragmaExpression, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression
	{
		// Token: 0x060005C2 RID: 1474 RVA: 0x0000C6EA File Offset: 0x0000B6EA
		protected ItemReference()
		{
		}

		// Token: 0x060005C3 RID: 1475 RVA: 0x0000C6F2 File Offset: 0x0000B6F2
		protected ItemReference(IToken token) : base(token)
		{
		}

		// Token: 0x060005C4 RID: 1476 RVA: 0x00004E6B File Offset: 0x00003E6B
		public virtual bool HasAttribute(string stAttribute, IScope scope)
		{
			return false;
		}

		// Token: 0x060005C5 RID: 1477 RVA: 0x00004E6B File Offset: 0x00003E6B
		public virtual bool HasAttribute(string stAttribute, IPrecompileScope2 scope)
		{
			return false;
		}

		// Token: 0x17000140 RID: 320
		// (get) Token: 0x060005C6 RID: 1478 RVA: 0x0000FEAC File Offset: 0x0000EEAC
		// (set) Token: 0x060005C7 RID: 1479 RVA: 0x0000FEB4 File Offset: 0x0000EEB4
		[DefaultSerialization("Type")]
		[StorageVersion("3.3.0.10")]
		[StorageIgnorable]
		[Obfuscation(Feature = "rename")]
		public override ICompiledType _CompiledType
		{
			get
			{
				return this.m_ctype;
			}
			set
			{
				this.m_ctype = value;
			}
		}

		// Token: 0x040000CC RID: 204
		[Obfuscation(Feature = "rename")]
		[DefaultSerialization("compiledtype")]
		[StorageVersion("3.3.0.0")]
		private ICompiledType m_ctype;
	}
}
