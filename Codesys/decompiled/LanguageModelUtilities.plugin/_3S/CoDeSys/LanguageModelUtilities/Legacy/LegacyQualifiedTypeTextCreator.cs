using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelUtilities.Legacy
{
	[Obsolete("Use _3S.CoDeSys.Core.LanguageModel.ILMQualifierService.GetQualifiedType instead")]
	[SuppressMessage("Info Code Smell", "S1133:Deprecated code should be removed", Justification = "This deprecated code cannot be removed because it will be used in case of older compiler versions")]
	internal sealed class LegacyQualifiedTypeTextCreator
	{
		private IPreCompileContext3 PrecompileContextSource { get; }

		private string LibraryNamespace { get; }

		private IPrecompileScope3 CreateSourceScope()
		{
			if (PrecompileContextSource == null)
			{
				return null;
			}
			return PrecompileContextSource.CreatePrecompileScope(Guid.Empty) as IPrecompileScope3;
		}

		private LegacyQualifiedTypeTextCreator(IPreCompileContext3 precomSource, string stLibraryNamespace)
		{
			PrecompileContextSource = precomSource;
			LibraryNamespace = stLibraryNamespace;
		}

		internal static string GetQualifiedTypeText(IType type, IPreCompileContext3 precomSource, IPreCompileContext3 precomDest)
		{
			if (precomDest is IPreCompileContext9 preCompileContext && preCompileContext.LibraryTable is ILibraryTable4 libraryTable)
			{
				string localLibraryNamespaceRecursive = libraryTable.GetLocalLibraryNamespaceRecursive(precomDest, precomSource.LibraryPath);
				return GetQualifiedTypeText(type, precomSource, localLibraryNamespaceRecursive);
			}
			string namespaceOfLibrary = precomDest.GetNamespaceOfLibrary(precomSource, null);
			return GetQualifiedTypeText(type, precomSource, namespaceOfLibrary);
		}

		internal static string GetQualifiedTypeText(IType type, IPreCompileContext3 precomSource, string stLibraryNamespace)
		{
			return new LegacyQualifiedTypeTextCreator(precomSource, stLibraryNamespace).GetQualifiedTypeText(type);
		}

		private string GetQualifiedTypeText(IType type)
		{
			switch (type.Class)
			{
			case TypeClass.Array:
				return WriteArrayType((IArrayType)type);
			case TypeClass.Pointer:
				return WritePointerType((IPointerType)type);
			case TypeClass.Userdef:
				return WriteUserdefinedType((IUserdefType2)type);
			case TypeClass.Reference:
				return WriteReferenceType((IReferenceType)type);
			case TypeClass.Subrange:
				return WriteSubrangeType((ISubrangeType2)type);
			default:
				return type.ToString();
			}
		}

		private string WriteUserdefinedType(IUserdefType2 type)
		{
			string text = type.ToString();
			if (text.ToUpperInvariant() == "__SYSTEM.ANYTYPE")
			{
				return "ANY";
			}
			IPrecompileScope3 precompileScope = CreateSourceScope();
			if (precompileScope != null)
			{
				string stNamespace;
				ISignature signature = precompileScope.FindSignatureGlobal(type.NameExpression, out stNamespace);
				if (LibraryNamespace != null && LibraryNamespace != string.Empty)
				{
					text = LibraryNamespace + "." + (object)type.NameExpression;
				}
				else if (stNamespace != null && stNamespace != string.Empty)
				{
					text = stNamespace + "." + signature.OrgName;
				}
			}
			return text;
		}

		private string WriteArrayType(IArrayType type)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("ARRAY [");
			bool flag = true;
			IArrayDimension[] dimensions = type.Dimensions;
			foreach (IArrayDimension dimension in dimensions)
			{
				if (flag)
				{
					flag = false;
				}
				else
				{
					stringBuilder.Append(",");
				}
				stringBuilder.Append(WriteArrayDimension(dimension));
			}
			stringBuilder.Append("] OF ");
			stringBuilder.Append(GetQualifiedTypeText(type.Base));
			return stringBuilder.ToString();
		}

		private string WriteArrayDimension(IArrayDimension dimension)
		{
			string text = LegacyQualifiedExpressionTextVisitor.DumpQualifiedExpressionText(dimension.LowerBorder, LibraryNamespace);
			string text2 = LegacyQualifiedExpressionTextVisitor.DumpQualifiedExpressionText(dimension.UpperBorder, LibraryNamespace);
			return text + ".." + text2;
		}

		private string WriteSubrangeType(ISubrangeType2 type)
		{
			string qualifiedTypeText = GetQualifiedTypeText(type.BaseType);
			string text = LegacyQualifiedExpressionTextVisitor.DumpQualifiedExpressionText(type.LowerBorder, LibraryNamespace);
			string text2 = LegacyQualifiedExpressionTextVisitor.DumpQualifiedExpressionText(type.UpperBorder, LibraryNamespace);
			return qualifiedTypeText + "(" + text + ".." + text2 + ")";
		}

		private string WriteReferenceType(IReferenceType type)
		{
			string qualifiedTypeText = GetQualifiedTypeText(type.Base);
			return "REFERENCE TO " + qualifiedTypeText;
		}

		private string WritePointerType(IPointerType type)
		{
			string qualifiedTypeText = GetQualifiedTypeText(type.Base);
			return "POINTER TO " + qualifiedTypeText;
		}
	}
}
