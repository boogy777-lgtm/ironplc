using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using \u0007;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;
using \u0084;

namespace \u0003
{
	// Token: 0x020000E0 RID: 224
	internal static class \u0002
	{
		// Token: 0x06000FC1 RID: 4033 RVA: 0x0002BE48 File Offset: 0x0002A048
		public static void \u0001(BinaryWriter \u0002, string \u0003)
		{
			Encoding ascii = Encoding.ASCII;
			char[] chars = new char[1];
			\u0002.Write(ascii.GetBytes(\u0003));
			\u0002.Write(ascii.GetBytes(chars));
		}

		// Token: 0x06000FC2 RID: 4034 RVA: 0x0002BE7C File Offset: 0x0002A07C
		private static bool \u0001(ISignature \u0002, LStack<ISignature> \u0003)
		{
			using (LStack<ISignature>.Enumerator enumerator = \u0003.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.Id == \u0002.Id)
					{
						return false;
					}
				}
			}
			return true;
		}

		// Token: 0x06000FC3 RID: 4035 RVA: 0x0002BED8 File Offset: 0x0002A0D8
		internal static bool \u0001(BinaryWriter \u0002, ICompiledType \u0003, ICompileContext \u0004, IScope2 \u0005)
		{
			\u0002.Write((int)\u0003.Class);
			switch (\u0003.Class)
			{
			case TypeClass.String:
			{
				IStringType stringType = \u0003 as IStringType;
				if (stringType.LengthExpression != null)
				{
					ILiteralValue literalValue = stringType.LengthExpression.Literal(\u0005);
					if (literalValue == null)
					{
						return false;
					}
					\u0002.Write(Helper.\u0001(literalValue));
				}
				break;
			}
			case TypeClass.WString:
			{
				IWStringType iwstringType = \u0003 as IWStringType;
				if (iwstringType.LengthExpression != null)
				{
					ILiteralValue literalValue2 = iwstringType.LengthExpression.Literal(\u0005);
					if (literalValue2 == null)
					{
						return false;
					}
					\u0002.Write(Helper.\u0001(literalValue2));
				}
				break;
			}
			case TypeClass.Pointer:
				return global::\u0003.\u0002.\u0001(\u0002, \u0003.BaseType, \u0004, \u0005);
			case TypeClass.Reference:
			case TypeClass.Subrange:
				return global::\u0003.\u0002.\u0001(\u0002, \u0003.DeRefType, \u0004, \u0005);
			case TypeClass.Array:
			{
				IArrayType arrayType = \u0003 as IArrayType;
				foreach (IArrayDimension arrayDimension in arrayType.Dimensions)
				{
					ILiteralValue literalValue3 = arrayDimension.LowerBorder.Literal(\u0005);
					if (literalValue3 != null)
					{
						\u0002.Write(Helper.\u0001(literalValue3));
						literalValue3 = arrayDimension.UpperBorder.Literal(\u0005);
						if (literalValue3 != null)
						{
							\u0002.Write(Helper.\u0001(literalValue3));
						}
					}
				}
				return global::\u0003.\u0002.\u0001(\u0002, arrayType.Base as ICompiledType, \u0004, \u0005);
			}
			case TypeClass.Userdef:
			{
				IUserdefType2 userdefType = \u0003 as IUserdefType2;
				ISignature signature = \u0005[userdefType.SignatureId];
				if (signature == null)
				{
					return false;
				}
				uint value = 0U;
				if (signature.HasAttribute(CompileAttributes.ATTRIBUTE_SIGNATURE_CRC))
				{
					string attributeValue = signature.GetAttributeValue(CompileAttributes.ATTRIBUTE_SIGNATURE_CRC);
					try
					{
						value = uint.Parse(attributeValue);
					}
					catch
					{
						return false;
					}
					\u0002.Write(value);
					break;
				}
				\u0002.Write(signature.Name);
				break;
			}
			}
			return true;
		}

		// Token: 0x06000FC4 RID: 4036 RVA: 0x0002C0C0 File Offset: 0x0002A2C0
		public static bool \u0001(BinaryWriter \u0002, LStack<ISignature> \u0003, ICompiledType \u0004, ICompileContext \u0005, IScope2 \u0006, bool \u0007)
		{
			if (\u0007 && TypeTable.IsResolvedXType(\u0004))
			{
				\u0002.Write((int)TypeTable.GetEquivalent64BitTypeOfResolvedXType(\u0004));
			}
			else
			{
				\u0002.Write((int)\u0004.Class);
			}
			switch (\u0004.Class)
			{
			case TypeClass.String:
			{
				IStringType stringType = \u0004 as IStringType;
				if (stringType.LengthExpression != null)
				{
					ILiteralValue literalValue = stringType.LengthExpression.Literal(\u0006);
					if (literalValue == null)
					{
						return false;
					}
					\u0002.Write(Helper.\u0001(literalValue));
				}
				break;
			}
			case TypeClass.WString:
			{
				IWStringType iwstringType = \u0004 as IWStringType;
				if (iwstringType.LengthExpression != null)
				{
					ILiteralValue literalValue2 = iwstringType.LengthExpression.Literal(\u0006);
					if (literalValue2 == null)
					{
						return false;
					}
					\u0002.Write(Helper.\u0001(literalValue2));
				}
				break;
			}
			case TypeClass.Pointer:
				return global::\u0003.\u0002.\u0001(\u0002, \u0003, \u0004.BaseType, \u0005, \u0006, \u0007);
			case TypeClass.Reference:
			case TypeClass.Subrange:
				return global::\u0003.\u0002.\u0001(\u0002, \u0003, \u0004.DeRefType, \u0005, \u0006, \u0007);
			case TypeClass.Array:
			{
				IArrayType arrayType = \u0004 as IArrayType;
				foreach (IArrayDimension arrayDimension in arrayType.Dimensions)
				{
					ILiteralValue literalValue3 = arrayDimension.LowerBorder.Literal(\u0006);
					if (literalValue3 != null)
					{
						\u0002.Write(Helper.\u0001(literalValue3));
						literalValue3 = arrayDimension.UpperBorder.Literal(\u0006);
						if (literalValue3 != null)
						{
							\u0002.Write(Helper.\u0001(literalValue3));
						}
					}
				}
				return global::\u0003.\u0002.\u0001(\u0002, \u0003, arrayType.Base as ICompiledType, \u0005, \u0006, \u0007);
			}
			case TypeClass.Userdef:
			{
				IUserdefType2 userdefType = \u0004 as IUserdefType2;
				_ISignature isignature = \u0006[userdefType.SignatureId] as _ISignature;
				if (isignature == null)
				{
					return false;
				}
				uint value = 0U;
				string stAttribute = CompileAttributes.ATTRIBUTE_SIGNATURE_CRC;
				if (\u0007)
				{
					stAttribute = CompileAttributes.ATTRIBUTE_SIGNATURE_CRC_64;
				}
				if (isignature.HasAttribute(stAttribute))
				{
					string attributeValue = isignature.GetAttributeValue(stAttribute);
					try
					{
						value = uint.Parse(attributeValue);
					}
					catch
					{
						return false;
					}
					\u0002.Write(value);
					break;
				}
				if (global::\u0003.\u0002.\u0001(isignature, \u0003))
				{
					uint value2;
					if (global::\u0003.\u0002.\u0001(isignature, \u0003, \u0005, out value2, \u0007))
					{
						isignature.AddAttribute(stAttribute, value2.ToString());
					}
					\u0002.Write(value2);
				}
				else
				{
					\u0002.Write(isignature.Name);
				}
				break;
			}
			}
			return true;
		}

		// Token: 0x06000FC5 RID: 4037 RVA: 0x0002C314 File Offset: 0x0002A514
		public static bool \u0001(_ISignature \u0002, uint \u0003, ICompileContext \u0004, out uint \u0005)
		{
			\u0005 = 0U;
			IScope2 u = \u0004.CreateIScope(\u0002.Id) as IScope2;
			ChecksumStream checksumStream = new MyChecksumStream(true);
			BinaryWriter binaryWriter = new BinaryWriter(checksumStream);
			global::\u0003.\u0002.\u0001(binaryWriter, \u0002.Name);
			binaryWriter.Write((int)\u0002.POUType);
			LStack<ISignature> lstack = new LStack<ISignature>();
			lstack.Push(\u0002);
			foreach (_IVariable ivariable in \u0002.AllVariables)
			{
				if (ivariable.DataLocation != null && (long)ivariable.DataLocation.Offset >= (long)((ulong)\u0003))
				{
					break;
				}
				if (!global::\u0003.\u0002.\u0001(\u0004, ivariable, binaryWriter, lstack, u))
				{
					return false;
				}
			}
			binaryWriter.Flush();
			checksumStream.Close();
			\u0005 = checksumStream.Checksum;
			return true;
		}

		// Token: 0x06000FC6 RID: 4038 RVA: 0x0002C3F0 File Offset: 0x0002A5F0
		public static IEnumerable<uint> \u0001(_ISignature \u0002, ICompileContext \u0003)
		{
			LList<uint> llist = new LList<uint>(\u0002.AllVariables.Count);
			IScope2 u = \u0003.CreateIScope(\u0002.Id) as IScope2;
			MyChecksumStream myChecksumStream = new MyChecksumStream(true);
			BinaryWriter binaryWriter = new BinaryWriter(myChecksumStream);
			global::\u0003.\u0002.\u0001(binaryWriter, \u0002.Name);
			binaryWriter.Write((int)\u0002.POUType);
			LStack<ISignature> lstack = new LStack<ISignature>();
			lstack.Push(\u0002);
			foreach (_IVariable ivariable in \u0002.AllVariables)
			{
				if (ivariable.DataLocation == null)
				{
					return Array.Empty<uint>();
				}
				if (!global::\u0003.\u0002.\u0001(\u0003, ivariable, binaryWriter, lstack, u))
				{
					return Array.Empty<uint>();
				}
				binaryWriter.Flush();
				llist.Add(myChecksumStream.\u0001());
			}
			myChecksumStream.Close();
			return llist;
		}

		// Token: 0x06000FC7 RID: 4039 RVA: 0x0002C4DC File Offset: 0x0002A6DC
		private static void \u0001(_ISignature \u0002, IScope \u0003, LList<LList<ISignature>> \u0004, LDictionary<int, LList<ISignature>> \u0005, LStack<ISignature> \u0006, LDictionary<int, int> \u0007)
		{
			if (\u0007.ContainsKey(\u0002.Id))
			{
				return;
			}
			\u0007.Add(\u0002.Id, \u0002.Id);
			\u0006.Push(\u0002);
			foreach (_IVariable ivariable in \u0002.AllVariables)
			{
				ICompiledType compiledType = ivariable.CompiledType.DeRefType;
				if (compiledType.Class == TypeClass.Pointer)
				{
					compiledType = (ICompiledType)\u0084.\u0004.\u0001(compiledType);
				}
				else if (compiledType.Class == TypeClass.Array)
				{
					compiledType = \u0084.\u0004.\u0001(compiledType);
				}
				if (compiledType.Class == TypeClass.Userdef)
				{
					_ISignature isignature = (compiledType as _IUserdefType).GetSignature(\u0003) as _ISignature;
					if (isignature != \u0002)
					{
						ISignature signature = null;
						if (\u0006.Contains(isignature))
						{
							signature = isignature;
						}
						if (\u0005.ContainsKey(isignature.Id) && !\u0006.Contains(isignature))
						{
							foreach (ISignature signature2 in \u0006)
							{
								if (\u0005.ContainsKey(signature2.Id) && \u0005[signature2.Id] == \u0005[isignature.Id])
								{
									signature = signature2;
									break;
								}
							}
						}
						if (\u0006.Contains(signature))
						{
							if (\u0005.ContainsKey(signature.Id))
							{
								LList<ISignature> llist = \u0005[signature.Id];
								using (LStack<ISignature>.Enumerator enumerator2 = \u0006.GetEnumerator())
								{
									while (enumerator2.MoveNext())
									{
										ISignature signature3 = enumerator2.Current;
										if (!\u0005.ContainsKey(signature3.Id))
										{
											llist.Add(signature3);
											\u0005[signature3.Id] = llist;
										}
										if (signature3 == signature)
										{
											break;
										}
									}
									continue;
								}
							}
							LList<ISignature> llist2 = new LList<ISignature>();
							\u0004.Add(llist2);
							using (LStack<ISignature>.Enumerator enumerator2 = \u0006.GetEnumerator())
							{
								while (enumerator2.MoveNext())
								{
									ISignature signature4 = enumerator2.Current;
									llist2.Add(signature4);
									\u0005[signature4.Id] = llist2;
									if (signature4 == isignature)
									{
										break;
									}
								}
								continue;
							}
						}
						global::\u0003.\u0002.\u0001(isignature, \u0003, \u0004, \u0005, \u0006, \u0007);
					}
				}
			}
			\u0006.Pop();
		}

		// Token: 0x06000FC8 RID: 4040 RVA: 0x0002C77C File Offset: 0x0002A97C
		private static void \u0001(IList<ISignature4> \u0002, IScope \u0003, out LList<LList<ISignature>> \u0004)
		{
			\u0004 = new LList<LList<ISignature>>();
			LStack<ISignature> u = new LStack<ISignature>();
			LDictionary<int, int> u2 = new LDictionary<int, int>();
			LDictionary<int, LList<ISignature>> u3 = new LDictionary<int, LList<ISignature>>();
			foreach (ISignature4 signature in \u0002)
			{
				global::\u0003.\u0002.\u0001((_ISignature)signature, \u0003, \u0004, u3, u, u2);
			}
		}

		// Token: 0x06000FC9 RID: 4041 RVA: 0x0002C7E8 File Offset: 0x0002A9E8
		public static void \u0001(_ICompileContext \u0002)
		{
			global::\u0003.\u0002.\u0002(\u0002);
		}

		// Token: 0x06000FCA RID: 4042 RVA: 0x0002C7F0 File Offset: 0x0002A9F0
		private static void \u0002(_ICompileContext \u0002)
		{
			global::\u0003.\u0002.\u0001(\u0002, false);
			if (\u0002.ApplicationGuid == Guid.Empty)
			{
				global::\u0003.\u0002.\u0001(\u0002, true);
			}
		}

		// Token: 0x06000FCB RID: 4043 RVA: 0x0002C814 File Offset: 0x0002AA14
		private static void \u0001(_ICompileContext \u0002, bool \u0003)
		{
			LStack<ISignature> u = new LStack<ISignature>();
			IList<ISignature4> allSignaturesFlatEx = \u0002.GetAllSignaturesFlatEx();
			string u2 = CompileAttributes.ATTRIBUTE_SIGNATURE_CRC;
			if (\u0003)
			{
				u2 = CompileAttributes.ATTRIBUTE_SIGNATURE_CRC_64;
			}
			IScope u3 = global::\u0007.\u0005.\u0001(\u0002);
			LList<LList<ISignature>> llist;
			global::\u0003.\u0002.\u0001(allSignaturesFlatEx, u3, out llist);
			global::\u0003.\u0002.\u0001 u4 = new global::\u0003.\u0002.\u0001();
			foreach (LList<ISignature> llist2 in llist)
			{
				llist2.Sort(u4);
				global::\u0003.\u0002.\u0001(\u0002, \u0003, llist2, u2, u);
			}
			global::\u0003.\u0002.\u0001(\u0002, \u0003, allSignaturesFlatEx, u2, u);
		}

		// Token: 0x06000FCC RID: 4044 RVA: 0x0002C8B0 File Offset: 0x0002AAB0
		private static void \u0001(_ICompileContext \u0002, bool \u0003, IEnumerable<ISignature> \u0004, string \u0005, LStack<ISignature> \u0006)
		{
			foreach (ISignature signature in \u0004)
			{
				_ISignature isignature = (_ISignature)signature;
				uint num;
				if (!isignature.HasAttribute(\u0005) && global::\u0003.\u0002.\u0001(isignature, \u0006, \u0002, out num, \u0003))
				{
					isignature.AddAttribute(\u0005, num.ToString());
				}
			}
		}

		// Token: 0x06000FCD RID: 4045 RVA: 0x0002C91C File Offset: 0x0002AB1C
		public static bool \u0001(_ISignature \u0002, LStack<ISignature> \u0003, ICompileContext \u0004, out uint \u0005)
		{
			return global::\u0003.\u0002.\u0001(\u0002, \u0003, \u0004, out \u0005, false);
		}

		// Token: 0x06000FCE RID: 4046 RVA: 0x0002C928 File Offset: 0x0002AB28
		private static bool \u0001(_ISignature \u0002, LStack<ISignature> \u0003, ICompileContext \u0004, out uint \u0005, bool \u0006)
		{
			\u0005 = 0U;
			IScope2 u = \u0004.CreateIScope(\u0002.Id) as IScope2;
			ChecksumStream checksumStream = new MyChecksumStream(true);
			BinaryWriter binaryWriter = new BinaryWriter(checksumStream);
			if (\u0002.HasAttribute(CompileAttributes.ATTRIBUTE_EXTERNAL_NAME))
			{
				string u2 = \u0002.GetAttributeValue(CompileAttributes.ATTRIBUTE_EXTERNAL_NAME).ToUpperInvariant();
				global::\u0003.\u0002.\u0001(binaryWriter, u2);
			}
			else
			{
				global::\u0003.\u0002.\u0001(binaryWriter, \u0002.Name);
			}
			binaryWriter.Write((int)\u0002.POUType);
			\u0003.Push(\u0002);
			bool flag = true;
			foreach (_IVariable u3 in \u0002.AllVariables)
			{
				flag = global::\u0003.\u0002.\u0001(\u0002, \u0003, \u0004, \u0006, u3, binaryWriter, u);
				if (!flag)
				{
					break;
				}
			}
			\u0003.Pop();
			if (!flag)
			{
				return false;
			}
			binaryWriter.Flush();
			checksumStream.Close();
			\u0005 = checksumStream.Checksum;
			return true;
		}

		// Token: 0x06000FCF RID: 4047 RVA: 0x0002CA18 File Offset: 0x0002AC18
		private static bool \u0001(_ISignature \u0002, LStack<ISignature> \u0003, ICompileContext \u0004, bool \u0005, _IVariable \u0006, BinaryWriter \u0007, IScope2 \u0008)
		{
			bool result = true;
			string text = \u0006.Name;
			if (\u0006.HasAttribute("Checksum_Name"))
			{
				text = \u0006.GetAttributeValue("Checksum_Name");
			}
			if ((\u0002.POUType == Operator.Function || \u0002.POUType == Operator.Method) && \u0002.HasAttribute(CompileAttributes.ATTRIBUTE_EXTERNAL_NAME) && \u0002.Name == text)
			{
				text = \u0002.GetAttributeValue(CompileAttributes.ATTRIBUTE_EXTERNAL_NAME).ToUpperInvariant();
			}
			if (\u0006.Name.StartsWith("__INTERFACEPOINTER__"))
			{
				text = "__INTERFACEPOINTER__";
			}
			global::\u0003.\u0002.\u0001(\u0007, text);
			if (\u0006.CompiledType == null)
			{
				result = false;
			}
			else if (\u0006.HasAttribute("Checksum_Type"))
			{
				global::\u0003.\u0002.\u0001(\u0007, \u0006.GetAttributeValue("Checksum_Type"));
			}
			else if (!global::\u0003.\u0002.\u0001(\u0007, \u0003, \u0006.CompiledType, \u0004, \u0008, \u0005))
			{
				result = false;
			}
			return result;
		}

		// Token: 0x06000FD0 RID: 4048 RVA: 0x0002CAF4 File Offset: 0x0002ACF4
		private static bool \u0001(ICompileContext \u0002, _IVariable \u0003, BinaryWriter \u0004, LStack<ISignature> \u0005, IScope2 \u0006)
		{
			if (\u0003.HasAttribute("Checksum_Name"))
			{
				global::\u0003.\u0002.\u0001(\u0004, \u0003.GetAttributeValue("Checksum_Name"));
			}
			else
			{
				global::\u0003.\u0002.\u0001(\u0004, \u0003.Name);
			}
			if (\u0003.CompiledType == null)
			{
				return false;
			}
			if (\u0003.HasAttribute("Checksum_Type"))
			{
				global::\u0003.\u0002.\u0001(\u0004, \u0003.GetAttributeValue("Checksum_Type"));
			}
			else if (!global::\u0003.\u0002.\u0001(\u0004, \u0005, \u0003.CompiledType, \u0002, \u0006, false))
			{
				return false;
			}
			return true;
		}

		// Token: 0x020000E1 RID: 225
		internal sealed class \u0001 : IComparer<ISignature>
		{
			// Token: 0x06000FD1 RID: 4049 RVA: 0x0002CB70 File Offset: 0x0002AD70
			public int \u0001(ISignature \u0002, ISignature \u0003)
			{
				string searchName = (\u0002 as _ISignature).GetSearchName(null);
				string searchName2 = (\u0003 as _ISignature).GetSearchName(null);
				return string.Compare(searchName, searchName2, StringComparison.Ordinal);
			}
		}
	}
}
