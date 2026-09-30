using System;
using System.Collections.Generic;
using \u0015;
using \u0017;
using \u0019;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0005
{
	// Token: 0x020001A7 RID: 423
	internal sealed class \u0004 : ITypeVisitor2, ITypeVisitor
	{
		// Token: 0x06001E78 RID: 7800 RVA: 0x000627AC File Offset: 0x000609AC
		internal \u0004()
		{
			this.\u0001 = \u0019.\u0003.Builder;
		}

		// Token: 0x06001E79 RID: 7801 RVA: 0x000627CC File Offset: 0x000609CC
		private \u0004(string \u0082\u0002) : this()
		{
			this.\u0001 = \u0082\u0002;
		}

		// Token: 0x06001E7A RID: 7802 RVA: 0x000627DC File Offset: 0x000609DC
		public IType \u0001(IType \u0002, ILMPreCompileSet \u0003, ILMPreCompileSet \u0004)
		{
			IPreCompileContext9 preCompileContext = \u0004 as IPreCompileContext9;
			if (preCompileContext != null)
			{
				ILibraryTable4 libraryTable = preCompileContext.LibraryTable as ILibraryTable4;
				if (libraryTable != null)
				{
					_IType itype = \u0002 as _IType;
					if (itype != null)
					{
						this.\u0001 = libraryTable.GetLocalLibraryNamespaceRecursive(preCompileContext, \u0003.LibraryPath);
						if (!string.IsNullOrEmpty(this.\u0001))
						{
							itype.Accept(this);
							return this.\u0001.Peek();
						}
						return \u0002;
					}
				}
			}
			return null;
		}

		// Token: 0x06001E7B RID: 7803 RVA: 0x00062844 File Offset: 0x00060A44
		public static _IType \u0001(\u0015.\u0002 \u0002, \u0015.\u0002 \u0003, _IType \u0004)
		{
			if (\u0004 == null)
			{
				return null;
			}
			if (\u0003 == null)
			{
				return \u0004;
			}
			string text = (\u0002 != null) ? \u0002.\u0001(\u0003.\u0001()) : null;
			if (!string.IsNullOrEmpty(text))
			{
				global::\u0005.\u0004 u = new global::\u0005.\u0004(text);
				\u0004.Accept(u);
				return u.\u0001.Peek();
			}
			return \u0004;
		}

		// Token: 0x06001E7C RID: 7804 RVA: 0x00062894 File Offset: 0x00060A94
		private _IExpression \u0001(_IExpression \u0002)
		{
			if (\u0002 == null)
			{
				return null;
			}
			string stExpression = \u0017.\u000F.\u0001(\u0002, this.\u0001);
			return this.\u0001.ParseExpression(stExpression) as _IExpression;
		}

		// Token: 0x06001E7D RID: 7805 RVA: 0x000628C4 File Offset: 0x00060AC4
		public void \u0001(_IUserdefType \u0002)
		{
			_IExpression expname = this.\u0001.ParseExpression(this.\u0001 + "." + \u0002.NameExpression.ToString()) as _IExpression;
			_IUserdefType item = this.\u0001.CreateUserdefType(expname);
			this.\u0001.Push(item);
		}

		// Token: 0x06001E7E RID: 7806 RVA: 0x00062918 File Offset: 0x00060B18
		public void \u0001(_IReferenceType \u0002)
		{
			\u0002._Base.Accept(this);
			_IReferenceType item = this.\u0001.CreateReferenceType(this.\u0001.Pop());
			this.\u0001.Push(item);
		}

		// Token: 0x06001E7F RID: 7807 RVA: 0x00062954 File Offset: 0x00060B54
		public void \u0001(_IParamsType \u0002)
		{
			\u0002._Base.Accept(this);
			_IParamsType item = this.\u0001.CreateParamsType(this.\u0001.Pop(), \u0002.Count);
			this.\u0001.Push(item);
		}

		// Token: 0x06001E80 RID: 7808 RVA: 0x00062998 File Offset: 0x00060B98
		public void \u0001(_IStringType \u0002)
		{
			_IStringType istringType = \u0002._Duplicate(true) as _IStringType;
			istringType.Length = this.\u0001(\u0002.Length);
			this.\u0001.Push(istringType);
		}

		// Token: 0x06001E81 RID: 7809 RVA: 0x000629D0 File Offset: 0x00060BD0
		public void \u0001(_IVariableLengthArrayType \u0002)
		{
			this.\u0001.Push(\u0002._Duplicate(true));
		}

		// Token: 0x06001E82 RID: 7810 RVA: 0x000629E4 File Offset: 0x00060BE4
		public void \u0001(_IWStringType \u0002)
		{
			_IWStringType iwstringType = \u0002._Duplicate(true) as _IWStringType;
			iwstringType.Length = this.\u0001(\u0002.Length);
			this.\u0001.Push(iwstringType);
		}

		// Token: 0x06001E83 RID: 7811 RVA: 0x00062A1C File Offset: 0x00060C1C
		public void \u0001(_IArrayType \u0002)
		{
			\u0002._Base.Accept(this);
			_IArrayType iarrayType = \u0002._Duplicate(true) as _IArrayType;
			iarrayType._Base = this.\u0001.Pop();
			foreach (_IArrayDimension iarrayDimension in iarrayType._Dimensions)
			{
				iarrayDimension._LowerBorder = this.\u0001(iarrayDimension._LowerBorder);
				iarrayDimension._UpperBorder = this.\u0001(iarrayDimension._UpperBorder);
			}
			this.\u0001.Push(iarrayType);
		}

		// Token: 0x06001E84 RID: 7812 RVA: 0x00062ABC File Offset: 0x00060CBC
		public void \u0001(_IVectorType \u0002)
		{
			\u0002._Base.Accept(this);
			_IVectorType ivectorType = \u0002._Duplicate(true) as _IVectorType;
			ivectorType._Base = this.\u0001.Pop();
			ivectorType._Dimension = this.\u0001(ivectorType._Dimension);
			this.\u0001.Push(ivectorType);
		}

		// Token: 0x06001E85 RID: 7813 RVA: 0x00062B14 File Offset: 0x00060D14
		public void \u0001(_ISubrangeType \u0002)
		{
			\u0002._Base.Accept(this);
			_ISubrangeType isubrangeType = \u0002._Duplicate(true) as _ISubrangeType;
			isubrangeType._Base = this.\u0001.Pop();
			isubrangeType._LowerBorder = this.\u0001(isubrangeType._LowerBorder);
			isubrangeType._UpperBorder = this.\u0001(isubrangeType._UpperBorder);
			this.\u0001.Push(isubrangeType);
		}

		// Token: 0x06001E86 RID: 7814 RVA: 0x00062B7C File Offset: 0x00060D7C
		public void \u0001(_IPointerType \u0002)
		{
			\u0002._Base.Accept(this);
			_IPointerType item = this.\u0001.CreatePointerType(this.\u0001.Pop());
			this.\u0001.Push(item);
		}

		// Token: 0x06001E87 RID: 7815 RVA: 0x00062BB8 File Offset: 0x00060DB8
		public void \u0001(_IBoolType \u0002)
		{
			this.\u0001.Push(\u0002);
		}

		// Token: 0x06001E88 RID: 7816 RVA: 0x00062BC8 File Offset: 0x00060DC8
		public void \u0001(_ISIntType \u0002)
		{
			this.\u0001.Push(\u0002);
		}

		// Token: 0x06001E89 RID: 7817 RVA: 0x00062BD8 File Offset: 0x00060DD8
		public void \u0001(_IIntType \u0002)
		{
			this.\u0001.Push(\u0002);
		}

		// Token: 0x06001E8A RID: 7818 RVA: 0x00062BE8 File Offset: 0x00060DE8
		public void \u0001(_IWordType \u0002)
		{
			this.\u0001.Push(\u0002);
		}

		// Token: 0x06001E8B RID: 7819 RVA: 0x00062BF8 File Offset: 0x00060DF8
		public void \u0001(_IUDIntType \u0002)
		{
			this.\u0001.Push(\u0002);
		}

		// Token: 0x06001E8C RID: 7820 RVA: 0x00062C08 File Offset: 0x00060E08
		public void \u0001(_ILIntType \u0002)
		{
			this.\u0001.Push(\u0002);
		}

		// Token: 0x06001E8D RID: 7821 RVA: 0x00062C18 File Offset: 0x00060E18
		public void \u0001(_ILWordType \u0002)
		{
			this.\u0001.Push(\u0002);
		}

		// Token: 0x06001E8E RID: 7822 RVA: 0x00062C28 File Offset: 0x00060E28
		public void \u0001(_ILRealType \u0002)
		{
			this.\u0001.Push(\u0002);
		}

		// Token: 0x06001E8F RID: 7823 RVA: 0x00062C38 File Offset: 0x00060E38
		public void \u0001(_IEnumType \u0002)
		{
			this.\u0001.Push(\u0002);
		}

		// Token: 0x06001E90 RID: 7824 RVA: 0x00062C48 File Offset: 0x00060E48
		public void \u0001(_IAnyType \u0002)
		{
			this.\u0001.Push(\u0002);
		}

		// Token: 0x06001E91 RID: 7825 RVA: 0x00062C58 File Offset: 0x00060E58
		public void \u0001(_IAnyIntType \u0002)
		{
			this.\u0001.Push(\u0002);
		}

		// Token: 0x06001E92 RID: 7826 RVA: 0x00062C68 File Offset: 0x00060E68
		public void \u0001(_IAnyBitType \u0002)
		{
			this.\u0001.Push(\u0002);
		}

		// Token: 0x06001E93 RID: 7827 RVA: 0x00062C78 File Offset: 0x00060E78
		public void \u0001(_IAnyBitButBoolIsPreferred \u0002)
		{
			this.\u0001.Push(\u0002);
		}

		// Token: 0x06001E94 RID: 7828 RVA: 0x00062C88 File Offset: 0x00060E88
		public void \u0001(_ITimeOfDayType \u0002)
		{
			this.\u0001.Push(\u0002);
		}

		// Token: 0x06001E95 RID: 7829 RVA: 0x00062C98 File Offset: 0x00060E98
		public void \u0001(_ILTimeOfDayType \u0002)
		{
			this.\u0001.Push(\u0002);
		}

		// Token: 0x06001E96 RID: 7830 RVA: 0x00062CA8 File Offset: 0x00060EA8
		public void \u0001(_ITimeType \u0002)
		{
			this.\u0001.Push(\u0002);
		}

		// Token: 0x06001E97 RID: 7831 RVA: 0x00062CB8 File Offset: 0x00060EB8
		public void \u0001(_IXIntType \u0002)
		{
			this.\u0001.Push(\u0002);
		}

		// Token: 0x06001E98 RID: 7832 RVA: 0x00062CC8 File Offset: 0x00060EC8
		public void \u0001(_IXUDIntType \u0002)
		{
			this.\u0001.Push(\u0002);
		}

		// Token: 0x06001E99 RID: 7833 RVA: 0x00062CD8 File Offset: 0x00060ED8
		public void \u0001(_IXLIntType \u0002)
		{
			this.\u0001.Push(\u0002);
		}

		// Token: 0x06001E9A RID: 7834 RVA: 0x00062CE8 File Offset: 0x00060EE8
		public void \u0001(_IXStringType \u0002)
		{
			this.\u0001.Push(\u0002);
		}

		// Token: 0x06001E9B RID: 7835 RVA: 0x00062CF8 File Offset: 0x00060EF8
		public void \u0001(_IAnyStringType \u0002)
		{
			this.\u0001.Push(\u0002);
		}

		// Token: 0x06001E9C RID: 7836 RVA: 0x00062D08 File Offset: 0x00060F08
		public void \u0001(_IUXIntType \u0002)
		{
			this.\u0001.Push(\u0002);
		}

		// Token: 0x06001E9D RID: 7837 RVA: 0x00062D18 File Offset: 0x00060F18
		public void \u0001(_IXULIntType \u0002)
		{
			this.\u0001.Push(\u0002);
		}

		// Token: 0x06001E9E RID: 7838 RVA: 0x00062D28 File Offset: 0x00060F28
		public void \u0001(_IXWordType \u0002)
		{
			this.\u0001.Push(\u0002);
		}

		// Token: 0x06001E9F RID: 7839 RVA: 0x00062D38 File Offset: 0x00060F38
		public void \u0001(_ILTimeType \u0002)
		{
			this.\u0001.Push(\u0002);
		}

		// Token: 0x06001EA0 RID: 7840 RVA: 0x00062D48 File Offset: 0x00060F48
		public void \u0001(_IDateAndTimeType \u0002)
		{
			this.\u0001.Push(\u0002);
		}

		// Token: 0x06001EA1 RID: 7841 RVA: 0x00062D58 File Offset: 0x00060F58
		public void \u0001(_ILDateAndTimeType \u0002)
		{
			this.\u0001.Push(\u0002);
		}

		// Token: 0x06001EA2 RID: 7842 RVA: 0x00062D68 File Offset: 0x00060F68
		public void \u0001(_IDateType \u0002)
		{
			this.\u0001.Push(\u0002);
		}

		// Token: 0x06001EA3 RID: 7843 RVA: 0x00062D78 File Offset: 0x00060F78
		public void \u0001(_ILDateType \u0002)
		{
			this.\u0001.Push(\u0002);
		}

		// Token: 0x06001EA4 RID: 7844 RVA: 0x00062D88 File Offset: 0x00060F88
		public void \u0001(_IAnyDateType \u0002)
		{
			this.\u0001.Push(\u0002);
		}

		// Token: 0x06001EA5 RID: 7845 RVA: 0x00062D98 File Offset: 0x00060F98
		public void \u0001(_IAnyNumType \u0002)
		{
			this.\u0001.Push(\u0002);
		}

		// Token: 0x06001EA6 RID: 7846 RVA: 0x00062DA8 File Offset: 0x00060FA8
		public void \u0001(_IAnyRealType \u0002)
		{
			this.\u0001.Push(\u0002);
		}

		// Token: 0x06001EA7 RID: 7847 RVA: 0x00062DB8 File Offset: 0x00060FB8
		public void \u0001(IImplicitEnumerationType \u0002)
		{
			this.\u0001.Push(\u0002);
		}

		// Token: 0x06001EA8 RID: 7848 RVA: 0x00062DC8 File Offset: 0x00060FC8
		public void \u0001(_ILazyType \u0002)
		{
			this.\u0001.Push(\u0002);
		}

		// Token: 0x06001EA9 RID: 7849 RVA: 0x00062DD8 File Offset: 0x00060FD8
		public void \u0001(_IRealType \u0002)
		{
			this.\u0001.Push(\u0002);
		}

		// Token: 0x06001EAA RID: 7850 RVA: 0x00062DE8 File Offset: 0x00060FE8
		public void \u0001(_IULIntType \u0002)
		{
			this.\u0001.Push(\u0002);
		}

		// Token: 0x06001EAB RID: 7851 RVA: 0x00062DF8 File Offset: 0x00060FF8
		public void \u0001(_IDWordType \u0002)
		{
			this.\u0001.Push(\u0002);
		}

		// Token: 0x06001EAC RID: 7852 RVA: 0x00062E08 File Offset: 0x00061008
		public void \u0001(_IDIntType \u0002)
		{
			this.\u0001.Push(\u0002);
		}

		// Token: 0x06001EAD RID: 7853 RVA: 0x00062E18 File Offset: 0x00061018
		public void \u0001(_IUIntType \u0002)
		{
			this.\u0001.Push(\u0002);
		}

		// Token: 0x06001EAE RID: 7854 RVA: 0x00062E28 File Offset: 0x00061028
		public void \u0001(_IUSIntType \u0002)
		{
			this.\u0001.Push(\u0002);
		}

		// Token: 0x06001EAF RID: 7855 RVA: 0x00062E38 File Offset: 0x00061038
		public void \u0001(_IByteType \u0002)
		{
			this.\u0001.Push(\u0002);
		}

		// Token: 0x06001EB0 RID: 7856 RVA: 0x00062E48 File Offset: 0x00061048
		public void \u0001(_IBitType \u0002)
		{
			this.\u0001.Push(\u0002);
		}

		// Token: 0x06001EB1 RID: 7857 RVA: 0x00062E58 File Offset: 0x00061058
		public void \u0001(_IBitConstType \u0002)
		{
			this.\u0001.Push(\u0002);
		}

		// Token: 0x040004EB RID: 1259
		private readonly Stack<_IType> \u0001 = new Stack<_IType>();

		// Token: 0x040004EC RID: 1260
		private readonly _ILanguageModelBuilder \u0001;

		// Token: 0x040004ED RID: 1261
		private string \u0001;
	}
}
