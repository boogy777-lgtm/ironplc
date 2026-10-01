using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x020001AB RID: 427
	[TypeGuid("{2BEA228F-A16D-4567-9D25-71A45C96366E}")]
	[StorageVersion("3.5.9.0")]
	public class RangeAwareAnyIntType : AnyIntType, _IRangeAwareAnyIntType, _IType, ICompiledType5, ICompiledType4, ICompiledType3, ICompiledType2, ICompiledType, IType, IArchivable
	{
		// Token: 0x17000807 RID: 2055
		// (get) Token: 0x06001EC5 RID: 7877 RVA: 0x00054CB6 File Offset: 0x00053CB6
		// (set) Token: 0x06001EC6 RID: 7878 RVA: 0x00054CBE File Offset: 0x00053CBE
		public long MaxValue { get; set; }

		// Token: 0x17000808 RID: 2056
		// (get) Token: 0x06001EC7 RID: 7879 RVA: 0x00054CC7 File Offset: 0x00053CC7
		// (set) Token: 0x06001EC8 RID: 7880 RVA: 0x00054CCF File Offset: 0x00053CCF
		public long MinValue { get; set; }

		// Token: 0x06001EC9 RID: 7881 RVA: 0x00054CD8 File Offset: 0x00053CD8
		public _IType ResolveIntegerType(_ICompileContext comcon)
		{
			int num;
			if (comcon != null && comcon.Codegenerator != null && comcon.Codegenerator is ICodegenerator4 && (comcon.Codegenerator as ICodegenerator4).RegisterSize == 2)
			{
				num = 0;
			}
			else
			{
				num = 1;
			}
			TypeClass typeClass = TypeClass.AnyInt;
			for (int i = num; i < RangeAwareAnyIntType._integerTypes.Length; i++)
			{
				TypeClass typeClass2 = RangeAwareAnyIntType._integerTypes[i];
				if (i == RangeAwareAnyIntType._integerTypes.Length - 1)
				{
					typeClass = typeClass2;
					break;
				}
				bool flag = this.MaxValue <= (long)TypeTable.GetTypeRangeHigh(typeClass2);
				bool flag2 = this.MinValue >= TypeTable.GetTypeRangeLow(typeClass2);
				if (flag && flag2)
				{
					typeClass = typeClass2;
					break;
				}
			}
			Debug.Assert(typeClass != TypeClass.AnyInt);
			return TypeTable.Get(typeClass);
		}

		// Token: 0x06001ECB RID: 7883 RVA: 0x00054D8B File Offset: 0x00053D8B
		// Note: this type is marked as 'beforefieldinit'.
		static RangeAwareAnyIntType()
		{
			TypeClass[] array = new TypeClass[3];
			RuntimeHelpers.InitializeArray(array, fieldof(<PrivateImplementationDetails>.BCBC01A5036673E493422616677A83718EDFE475D3E938B1A879903FFB2A05A0).FieldHandle);
			RangeAwareAnyIntType._integerTypes = array;
		}

		// Token: 0x04000606 RID: 1542
		private static readonly TypeClass[] _integerTypes;

		// Token: 0x04000607 RID: 1543
		private const int IntIndex = 0;

		// Token: 0x04000608 RID: 1544
		private const int DIntIndex = 1;
	}
}
