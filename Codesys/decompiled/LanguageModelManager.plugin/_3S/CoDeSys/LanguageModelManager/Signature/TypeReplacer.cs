using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.Signature
{
	// Token: 0x02000261 RID: 609
	public static class TypeReplacer
	{
		// Token: 0x060029A5 RID: 10661 RVA: 0x00069DBC File Offset: 0x00068DBC
		public static _IType ReplaceTypes(_IType type, SpecialFeatures sf)
		{
			if (sf == SpecialFeatures.NoByteSupport)
			{
				TypeClass @class = type.Class;
				if (@class == TypeClass.Bool)
				{
					return TypeTable.Bool16;
				}
				switch (@class)
				{
				case TypeClass.Pointer:
					TypeReplacer.ReplacePointerType(type, sf);
					break;
				case TypeClass.Reference:
				{
					ReferenceType referenceType = type as ReferenceType;
					if (referenceType != null)
					{
						referenceType._Base = TypeReplacer.ReplaceTypes(referenceType._Base, sf);
					}
					break;
				}
				case TypeClass.Array:
				{
					ArrayType arrayType = type as ArrayType;
					if (arrayType != null)
					{
						arrayType._Base = TypeReplacer.ReplaceTypes(arrayType._Base, sf);
					}
					break;
				}
				}
			}
			return type;
		}

		// Token: 0x060029A6 RID: 10662 RVA: 0x00069E40 File Offset: 0x00068E40
		private static void ReplacePointerType(_IType type, SpecialFeatures sf)
		{
			PointerType pointerType = type as PointerType;
			if (pointerType != null)
			{
				pointerType._Base = TypeReplacer.ReplaceTypes(pointerType._Base, sf);
				if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV34421)
				{
					TypeClass @class = pointerType._Base.Class;
					if (@class <= TypeClass.SInt)
					{
						if (@class - TypeClass.Bit > 1)
						{
							if (@class != TypeClass.SInt)
							{
								return;
							}
							pointerType._Base = TypeTable.Int;
							return;
						}
					}
					else if (@class != TypeClass.USInt && @class != TypeClass.String)
					{
						return;
					}
					pointerType._Base = TypeTable.Word;
					return;
				}
			}
		}
	}
}
