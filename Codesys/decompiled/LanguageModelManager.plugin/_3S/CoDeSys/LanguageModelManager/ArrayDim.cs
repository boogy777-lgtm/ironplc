using System;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x0200019E RID: 414
	[TypeGuid("{d4b6feaa-e9d6-4356-aaf6-400ae9d83e05}")]
	[StorageVersion("3.3.0.0")]
	public class ArrayDim : GenericObject2, _IArrayDimension, IArrayDimension2, IArrayDimension
	{
		// Token: 0x06001DE8 RID: 7656 RVA: 0x0000AC39 File Offset: 0x00009C39
		public ArrayDim()
		{
		}

		// Token: 0x06001DE9 RID: 7657 RVA: 0x00052255 File Offset: 0x00051255
		internal ArrayDim(_IExpression expLower, _IExpression expUpper)
		{
			this.m_expLower = expLower;
			this.m_expUpper = expUpper;
		}

		// Token: 0x170007DA RID: 2010
		// (get) Token: 0x06001DEA RID: 7658 RVA: 0x0005226B File Offset: 0x0005126B
		public IExpression LowerBorder
		{
			get
			{
				return this.m_expLower;
			}
		}

		// Token: 0x170007DB RID: 2011
		// (get) Token: 0x06001DEB RID: 7659 RVA: 0x0005226B File Offset: 0x0005126B
		// (set) Token: 0x06001DEC RID: 7660 RVA: 0x00052273 File Offset: 0x00051273
		public _IExpression _LowerBorder
		{
			get
			{
				return this.m_expLower;
			}
			set
			{
				this.m_expLower = value;
			}
		}

		// Token: 0x170007DC RID: 2012
		// (get) Token: 0x06001DED RID: 7661 RVA: 0x0005227C File Offset: 0x0005127C
		public IExpression UpperBorder
		{
			get
			{
				return this.m_expUpper;
			}
		}

		// Token: 0x170007DD RID: 2013
		// (get) Token: 0x06001DEE RID: 7662 RVA: 0x0005227C File Offset: 0x0005127C
		// (set) Token: 0x06001DEF RID: 7663 RVA: 0x00052284 File Offset: 0x00051284
		public _IExpression _UpperBorder
		{
			get
			{
				return this.m_expUpper;
			}
			set
			{
				this.m_expUpper = value;
			}
		}

		// Token: 0x170007DE RID: 2014
		// (get) Token: 0x06001DF0 RID: 7664 RVA: 0x0005228D File Offset: 0x0005128D
		public ArrayDim Duplicate
		{
			get
			{
				return new ArrayDim(this.m_expLower.Duplicate() as _IExpression, this.m_expUpper.Duplicate() as _IExpression);
			}
		}

		// Token: 0x06001DF1 RID: 7665 RVA: 0x000522B4 File Offset: 0x000512B4
		public int LowerBorderInt(out bool bValid, IScope scope, IRecursionGuard recursionGuard)
		{
			bValid = false;
			int @int = TypeHelper.GetInt(this.LowerBorder, scope, APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35200, out bValid);
			if (!bValid)
			{
				return -1;
			}
			return @int;
		}

		// Token: 0x06001DF2 RID: 7666 RVA: 0x000522E8 File Offset: 0x000512E8
		public int LowerBorderInt(out bool bValid, IScope scope)
		{
			bValid = false;
			int @int = TypeHelper.GetInt(this.LowerBorder, scope, APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35200, out bValid);
			if (!bValid)
			{
				return -1;
			}
			return @int;
		}

		// Token: 0x06001DF3 RID: 7667 RVA: 0x0005231C File Offset: 0x0005131C
		public int UpperBorderInt(out bool bValid, IScope scope, IRecursionGuard recursionGuard)
		{
			int @int = TypeHelper.GetInt(this.m_expUpper, scope, APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35200, recursionGuard, out bValid);
			if (!bValid)
			{
				return -1;
			}
			return @int;
		}

		// Token: 0x06001DF4 RID: 7668 RVA: 0x00052350 File Offset: 0x00051350
		public int UpperBorderInt(out bool bValid, IScope scope)
		{
			int @int = TypeHelper.GetInt(this.m_expUpper, scope, APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35200, out bValid);
			if (!bValid)
			{
				return -1;
			}
			return @int;
		}

		// Token: 0x06001DF5 RID: 7669 RVA: 0x00052384 File Offset: 0x00051384
		public int Range(out bool bValid, IScope scope, IRecursionGuard recursionGuard)
		{
			bValid = false;
			int num = this.LowerBorderInt(out bValid, scope, recursionGuard);
			if (!bValid)
			{
				return -1;
			}
			int num2 = this.UpperBorderInt(out bValid, scope, recursionGuard);
			if (!bValid)
			{
				return -1;
			}
			int num3 = num2 - num + 1;
			if (num3 < 0)
			{
				bValid = false;
				return -1;
			}
			return num3;
		}

		// Token: 0x06001DF6 RID: 7670 RVA: 0x000523C8 File Offset: 0x000513C8
		public int Range(out bool bValid, IScope scope)
		{
			bValid = false;
			int num = this.LowerBorderInt(out bValid, scope);
			if (!bValid)
			{
				return -1;
			}
			int num2 = this.UpperBorderInt(out bValid, scope);
			if (!bValid)
			{
				return -1;
			}
			int num3 = num2 - num + 1;
			if (num3 < 0)
			{
				bValid = false;
				return -1;
			}
			return num3;
		}

		// Token: 0x06001DF7 RID: 7671 RVA: 0x00052408 File Offset: 0x00051408
		public int LowerBorderInt(out bool bValid, IPrecompileScope scope)
		{
			bValid = false;
			int @int = TypeHelper.GetInt(this.LowerBorder, scope, out bValid);
			if (!bValid)
			{
				return -1;
			}
			return @int;
		}

		// Token: 0x06001DF8 RID: 7672 RVA: 0x00052430 File Offset: 0x00051430
		public int UpperBorderInt(out bool bValid, IPrecompileScope scope)
		{
			int @int = TypeHelper.GetInt(this.m_expUpper, scope, out bValid);
			if (!bValid)
			{
				return -1;
			}
			return @int;
		}

		// Token: 0x06001DF9 RID: 7673 RVA: 0x00052454 File Offset: 0x00051454
		public int Range(out bool bValid, IPrecompileScope scope)
		{
			bValid = false;
			int num = this.LowerBorderInt(out bValid, scope);
			if (!bValid)
			{
				return -1;
			}
			int num2 = this.UpperBorderInt(out bValid, scope);
			if (!bValid)
			{
				return -1;
			}
			int num3 = num2 - num + 1;
			if (num3 < 0)
			{
				bValid = false;
				return -1;
			}
			return num3;
		}

		// Token: 0x040005F5 RID: 1525
		[DefaultSerialization("Lower")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private _IExpression m_expLower;

		// Token: 0x040005F6 RID: 1526
		[DefaultSerialization("Upper")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private _IExpression m_expUpper;
	}
}
