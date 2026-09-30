using System.Collections.Generic;
using System.Text;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	internal class AliasExporter
	{
		private const string STRING_BASE_TYPE = "STRING__";

		private const string WSTRING_BASE_TYPE = "WSTRING__";

		private const string ARRAY_BASE_TYPE = "ARRAY__";

		private const string POINTER_BASE_TYPE = "POINTER_TO__";

		private readonly RTSComponentExporter _rtsComponentExporter;

		private readonly HashSet<long> _hsAlreadyDeclaredStringTypes;

		private readonly HashSet<long> _hsAlreadyDeclaredWStringTypes;

		private readonly HashSet<string> _hsAlreadyDeclaredArrayTypes;

		private readonly HashSet<string> _hsAlreadyDeclaredPointerTypes;

		private StringBuilder _sb;

		private IScope2 _scope;

		internal AliasExporter(RTSComponentExporter rtsComponentExporter)
		{
			_rtsComponentExporter = rtsComponentExporter;
			_hsAlreadyDeclaredStringTypes = new HashSet<long>();
			_hsAlreadyDeclaredWStringTypes = new HashSet<long>();
			_hsAlreadyDeclaredArrayTypes = new HashSet<string>();
			_hsAlreadyDeclaredPointerTypes = new HashSet<string>();
		}

		internal void DumpAlias(StringBuilder sb, ISignature sign, string stAliasTypeName, IScope2 scope)
		{
			_sb = sb;
			_scope = scope;
			string text = string.Empty;
			if (1 == sign.All.Length && sign.All[0].Type is ICompiledType type)
			{
				text = GetBaseTypeName(type, scope, bForArrayType: false);
			}
			sb.AppendLine("typedef " + text + " " + stAliasTypeName + ";");
		}

		private string GetBaseTypeName(ICompiledType type, IScope2 scope, bool bForArrayType)
		{
			switch (type.Class)
			{
			case TypeClass.Array:
				return GetAliasArrayBaseType(type, scope);
			case TypeClass.String:
				return GetAliasStringBaseType(_hsAlreadyDeclaredStringTypes, "STRING__", "RTS_IEC_STRING", (type as IStringType).LengthExpression);
			case TypeClass.WString:
				return GetAliasStringBaseType(_hsAlreadyDeclaredWStringTypes, "WSTRING__", "RTS_IEC_WSTRING", (type as IWStringType).LengthExpression);
			case TypeClass.Pointer:
				return GetAliasPointerBaseType(type, scope, bForArrayType);
			case TypeClass.Subrange:
				return GetAliasSubrangeBaseType(type, scope, bForArrayType);
			default:
			{
				if (IsInterface(type, out var signItf))
				{
					return signItf.Name.ToLowerInvariant() + "_struct";
				}
				return _rtsComponentExporter.GetExternalType(type, type.ToString(), bForArrayType, scope);
			}
			}
		}

		private string GetAliasArrayBaseType(ICompiledType type, IScope2 scope)
		{
			string result = "ARRAY__ERROR__";
			if (!(type is IArrayType arrayType))
			{
				return result;
			}
			StringBuilder stringBuilder = new StringBuilder();
			List<int> list = new List<int>();
			stringBuilder.Append("ARRAY__");
			IArrayDimension[] dimensions = arrayType.Dimensions;
			foreach (IArrayDimension arrayDimension in dimensions)
			{
				bool bValid;
				int num = arrayDimension.LowerBorderInt(out bValid, _scope);
				if (!bValid)
				{
					return result;
				}
				int num2 = arrayDimension.UpperBorderInt(out bValid, _scope);
				if (!bValid)
				{
					return result;
				}
				int item = arrayDimension.Range(out bValid, _scope);
				if (!bValid)
				{
					return result;
				}
				list.Add(item);
				stringBuilder.Append($"{num}__{num2}__");
			}
			if (!(arrayType.Base is ICompiledType type2))
			{
				return result;
			}
			string baseTypeName = GetBaseTypeName(type2, scope, bForArrayType: true);
			stringBuilder.Append("OF__" + baseTypeName);
			string text = stringBuilder.ToString();
			DeclareArrayType(text, baseTypeName, list.ToArray());
			return text;
		}

		private string GetAliasPointerBaseType(ICompiledType type, IScope2 scope, bool bForArrayType)
		{
			string result = "POINTER_TO__ERROR__";
			if (!(type is IPointerType pointerType))
			{
				return result;
			}
			if (!(pointerType.Base is ICompiledType type2))
			{
				return result;
			}
			string baseTypeName = GetBaseTypeName(type2, scope, bForArrayType);
			string text = "POINTER_TO__" + baseTypeName;
			DeclarePointerType(text, baseTypeName);
			return text;
		}

		private string GetAliasSubrangeBaseType(ICompiledType type, IScope2 scope, bool bForArrayType)
		{
			string result = "POINTER_TO__ERROR__";
			if (!(type is ISubrangeType2 subrangeType))
			{
				return result;
			}
			ICompiledType baseType = subrangeType.BaseType;
			if (baseType == null)
			{
				return result;
			}
			return GetBaseTypeName(baseType, scope, bForArrayType);
		}

		private string GetAliasStringBaseType(HashSet<long> hsAlreadyDeclaredStringTypes, string stStringBaseType, string stRtsIecStringType, IExpression lengthExpression)
		{
			string result = stStringBaseType + "ERROR__";
			if (!(lengthExpression is _IExpression iExpression))
			{
				return result;
			}
			bool bRecursionError;
			ILiteralValue literalValue = iExpression.LiteralWithRecursionCheck(_scope, new Dictionary<IVariable, IVariable>(), bAllocatedOK: true, out bRecursionError);
			if (literalValue == null || bRecursionError)
			{
				return result;
			}
			long num;
			switch (literalValue.KindOf)
			{
			case KindOfLiteral.SignedInteger:
				num = literalValue.SignedLong;
				break;
			case KindOfLiteral.UnsignedInteger:
				num = (long)literalValue.UnsignedLong;
				break;
			default:
				return result;
			}
			DeclareStringType(num, hsAlreadyDeclaredStringTypes, stStringBaseType, stRtsIecStringType);
			return $"{stStringBaseType}{num}";
		}

		private void DeclareArrayType(string stArrayTypeName, string stIecBaseTypeName, int[] nArrayDimRanges)
		{
			if (!_hsAlreadyDeclaredArrayTypes.Contains(stArrayTypeName))
			{
				_sb.Append("typedef " + stIecBaseTypeName + " " + stArrayTypeName);
				foreach (int num in nArrayDimRanges)
				{
					_sb.Append($"[{num}]");
				}
				_sb.AppendLine(";");
				_hsAlreadyDeclaredArrayTypes.Add(stArrayTypeName);
			}
		}

		private void DeclarePointerType(string stPointerTypeName, string stIecBaseTypeName)
		{
			if (!_hsAlreadyDeclaredPointerTypes.Contains(stPointerTypeName))
			{
				_sb.AppendLine("typedef " + stIecBaseTypeName + " *" + stPointerTypeName + ";");
				_hsAlreadyDeclaredPointerTypes.Add(stPointerTypeName);
			}
		}

		private void DeclareStringType(long nStringLength, HashSet<long> hsAlreadyDeclaredStringTypes, string stStringBaseType, string stRtsIecStringType)
		{
			if (!hsAlreadyDeclaredStringTypes.Contains(nStringLength))
			{
				_sb.AppendLine($"typedef {stRtsIecStringType} {stStringBaseType}{nStringLength}[{nStringLength}];");
				hsAlreadyDeclaredStringTypes.Add(nStringLength);
			}
		}

		private bool IsInterface(ICompiledType type, out ISignature signItf)
		{
			signItf = null;
			if (TypeClass.Userdef != type.Class)
			{
				return false;
			}
			if (!(type is IUserdefType userdefType))
			{
				return false;
			}
			ISignature signature = _scope[userdefType.SignatureId];
			if (signature == null)
			{
				return false;
			}
			signItf = _rtsComponentExporter.GetSignatureIfInterfaceUnion(signature);
			if (signItf == null)
			{
				return false;
			}
			return Operator.Interface == signItf.POUType;
		}
	}
}
