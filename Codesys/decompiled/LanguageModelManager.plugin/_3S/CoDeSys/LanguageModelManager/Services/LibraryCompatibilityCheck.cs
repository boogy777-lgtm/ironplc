using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager.Services
{
	// Token: 0x02000240 RID: 576
	[SuppressMessage("Major Code Smell", "S1200:Classes should not be coupled to too many other classes", Justification = "Will be fixed with CDS-94202")]
	internal static class LibraryCompatibilityCheck
	{
		// Token: 0x06002666 RID: 9830 RVA: 0x0005ED58 File Offset: 0x0005DD58
		[SuppressMessage("Critical Code Smell", "S3776:Cognitive Complexity of methods should not be too high", Justification = "Will be fixed with CDS-94202")]
		public static IEnumerable<IMessage> CheckLibrary(IPreCompileContext ipccOlderLibrary, IPreCompileContext ipccNewerLibrary, bool bInterfaceLibrary)
		{
			HashSet<string> hsAttributesToIgnore = new HashSet<string>
			{
				CompileAttributes.ATTRIBUTE_NO_QUERY_INTERFACE_CHECK,
				CompileAttributes.ATTRIBUTE_HIDE,
				"conditionalshow",
				"conditionalshow_all_locals",
				CompileAttributes.ATTRIBUTE_DOCUCOMMENT,
				CompileAttributes.ATTRIBUTE_COMMENT,
				CompileAttributes.ATTRIBUTE_SUPPRESS_WRN_C0410,
				CompileAttributes.ATTRIBUTE_ANALYSIS,
				CompileAttributes.ATTRIBUTE_NAMING,
				CompileAttributes.ATTRIBUTE_OBSOLETE
			};
			PreCompileContext preCompileContext = ipccOlderLibrary as PreCompileContext;
			PreCompileContext preCompileContext2 = ipccNewerLibrary as PreCompileContext;
			string b = string.Empty;
			if (!string.IsNullOrEmpty(preCompileContext.LibraryPath))
			{
				b = preCompileContext.LibraryPath;
			}
			int num = APEnvironmentFacade.Instance.PrimaryProjectHandle;
			if (!string.IsNullOrEmpty(preCompileContext2.LibraryPath))
			{
				num = APEnvironmentFacade.Instance.LanguageModelMgr.LibList.GetProjectHandle(preCompileContext2.LibraryPath);
			}
			List<IMessage> list = new List<IMessage>();
			foreach (_ISignature isignature in preCompileContext._AllSignatures)
			{
				string a = string.Empty;
				if (!string.IsNullOrEmpty(isignature.LibraryPath))
				{
					a = isignature.LibraryPath;
				}
				if (!isignature.GetFlag(SignatureFlag.SuperGlobal) && !isignature.GetFlag(SignatureFlag.TimeStampOnly) && !(a != b))
				{
					ISignature[] array = preCompileContext2.FindSignature(isignature.Name);
					_ISignature isignature2 = null;
					if (array != null)
					{
						foreach (ISignature signature in array)
						{
							if (signature.LibraryPath.ToUpperInvariant() == preCompileContext2.LibraryPath.ToUpperInvariant())
							{
								isignature2 = (signature as _ISignature);
								break;
							}
						}
					}
					Operator poutype = isignature.POUType;
					if (poutype <= Operator.Program)
					{
						if (poutype != Operator.Action && poutype - Operator.Function > 1 && poutype != Operator.Program)
						{
							continue;
						}
					}
					else
					{
						if (poutype <= Operator.VarGlobal)
						{
							if (poutype != Operator.Type)
							{
								if (poutype != Operator.VarGlobal)
								{
									continue;
								}
								if (!isignature.GetFlag(SignatureFlag.Enum))
								{
									if (LibraryCompatibilityCheck.CheckNewSignExists(isignature2, isignature, num, list))
									{
										IPrecompileScope scopeNew = preCompileContext2.CreatePrecompileScope(isignature2.ObjectGuid);
										IPrecompileScope scopeOld = preCompileContext.CreatePrecompileScope(isignature.ObjectGuid);
										using (IEnumerator<_IVariable> enumerator2 = isignature.AllVariables.GetEnumerator())
										{
											while (enumerator2.MoveNext())
											{
												_IVariable ivariable = enumerator2.Current;
												_IVariable ivariable2 = isignature2[ivariable.Name] as _IVariable;
												if (ivariable2 == null)
												{
													SourcePosition position = new SourcePosition(num, isignature2.ObjectGuid, 0L, 0, 0);
													string stError = string.Format(Strings.VariableRenamedOrDeleted, ivariable.OrgName, isignature.OrgName);
													list.Add(new CompilerMessage(position, stError, Severity.Error, MessageId.None));
												}
												else
												{
													LibraryCompatibilityCheck.CheckGVLCompatibility(bInterfaceLibrary, ivariable, ivariable2, num, isignature2, isignature, list, scopeOld, scopeNew);
												}
											}
											continue;
										}
										goto IL_2F0;
									}
									continue;
								}
							}
							LibraryCompatibilityCheck.CheckTypeCompatibility(bInterfaceLibrary, isignature2, isignature as _ISignature2, num, list, preCompileContext2, hsAttributesToIgnore);
							continue;
						}
						if (poutype != Operator.Method)
						{
							if (poutype != Operator.Interface)
							{
								continue;
							}
							goto IL_377;
						}
					}
					IL_2F0:
					bool flag = false;
					if (isignature.HasAttribute("compatibility"))
					{
						string attributeValue = isignature.GetAttributeValue("compatibility");
						if (!string.IsNullOrEmpty(attributeValue) && attributeValue.ToUpperInvariant() == "STRICT")
						{
							flag = true;
						}
					}
					if (isignature.HasAttribute(CompileAttributes.ATTRIBUTE_CPPCOMPATIBLE) || isignature.HasAttribute(CompileAttributes.ATTRIBUTE_CPPEXTERNAL))
					{
						flag = true;
					}
					if (!flag && isignature2 != null && isignature2.POUType == isignature.POUType)
					{
						LibraryCompatibilityCheck.CheckPOUCompatibility(bInterfaceLibrary, isignature, isignature2, num, list, preCompileContext, preCompileContext2);
						continue;
					}
					IL_377:
					LibraryCompatibilityCheck.CheckInterfaceCompatibility(bInterfaceLibrary, isignature2, isignature, num, list, hsAttributesToIgnore, preCompileContext, preCompileContext2);
				}
			}
			return list;
		}

		// Token: 0x06002667 RID: 9831 RVA: 0x0005F154 File Offset: 0x0005E154
		private static void CheckTypeCompatibility(bool bInterfaceLibrary, _ISignature signNew, _ISignature2 signOld, int nProjectHandleNew, List<IMessage> alMessages, PreCompileContext pccNewerLibrary, HashSet<string> hsAttributesToIgnore)
		{
			if (!LibraryCompatibilityCheck.CheckNewSignExists(signNew, signOld, nProjectHandleNew, alMessages))
			{
				return;
			}
			bool flag = false;
			IPrecompileScope precompileScope = pccNewerLibrary.CreatePrecompileScope(signNew.ObjectGuid);
			IPrecompileScope precompileScope2 = pccNewerLibrary.CreatePrecompileScope(signOld.ObjectGuid);
			if (!SignatureComparer.IsEqualPrecompile(signOld, signNew as _ISignature2, true, hsAttributesToIgnore, precompileScope2, precompileScope, ref flag))
			{
				bool flag2 = true;
				if (signOld.GetFlag(SignatureFlag.Enum))
				{
					flag2 = false;
					Dictionary<string, ILiteralValue> dictionary = new Dictionary<string, ILiteralValue>();
					Dictionary<string, ILiteralValue> dictionary2 = new Dictionary<string, ILiteralValue>();
					TypeClass typeClass;
					TypeClass typeClass2;
					if (LibraryCompatibilityCheck.CalculateEnumConstants(signOld, precompileScope2, dictionary, out typeClass) && LibraryCompatibilityCheck.CalculateEnumConstants(signNew, precompileScope, dictionary2, out typeClass2))
					{
						if (typeClass == TypeClass.None || typeClass2 == TypeClass.None || typeClass != typeClass2)
						{
							flag2 = true;
							goto IL_158;
						}
						using (Dictionary<string, ILiteralValue>.KeyCollection.Enumerator enumerator = dictionary.Keys.GetEnumerator())
						{
							while (enumerator.MoveNext())
							{
								string key = enumerator.Current;
								ILiteralValue literalValue = dictionary[key];
								if (!dictionary2.ContainsKey(key))
								{
									flag2 = true;
									break;
								}
								ILiteralValue literalValue2 = dictionary2[key];
								if (literalValue == null || literalValue2 == null || literalValue.KindOf != literalValue2.KindOf)
								{
									flag2 = true;
									break;
								}
								if (literalValue.KindOf == KindOfLiteral.SignedInteger)
								{
									if (literalValue.SignedLong != literalValue2.SignedLong)
									{
										flag2 = true;
										break;
									}
								}
								else
								{
									if (literalValue.KindOf != KindOfLiteral.UnsignedInteger)
									{
										flag2 = true;
										break;
									}
									if (literalValue.UnsignedLong != literalValue2.UnsignedLong)
									{
										flag2 = true;
										break;
									}
								}
							}
							goto IL_158;
						}
					}
					flag2 = true;
				}
				IL_158:
				if (flag2)
				{
					SourcePosition position = new SourcePosition(nProjectHandleNew, signNew.ObjectGuid, 0L, 0, 0);
					string stError = string.Format(Strings.TypeChanged, signOld.OrgName);
					alMessages.Add(new CompilerMessage(position, stError, Severity.Error, MessageId.None));
				}
			}
			if (bInterfaceLibrary && !signOld.GetFlag(SignatureFlag.Enum) && !flag)
			{
				uint checksum = signNew.Checksum;
				uint checksum2 = signOld.Checksum;
				if (checksum != checksum2)
				{
					SourcePosition position2 = new SourcePosition(nProjectHandleNew, signNew.ObjectGuid, 0L, 0, 0);
					string stError2 = string.Format(Strings.TypeChanged, signOld.OrgName);
					alMessages.Add(new CompilerMessage(position2, stError2, Severity.Error, MessageId.None));
				}
			}
		}

		// Token: 0x06002668 RID: 9832 RVA: 0x0005F35C File Offset: 0x0005E35C
		private static bool CheckForChecksum(_ISignature signNewSub, _ISignature signOldSub)
		{
			uint checksum = signNewSub.Checksum;
			uint checksum2 = signOldSub.Checksum;
			bool flag = checksum == checksum2;
			if (!flag)
			{
				flag = (signNewSub.ChecksumNoInit == signOldSub.ChecksumNoInit);
			}
			return flag;
		}

		// Token: 0x06002669 RID: 9833 RVA: 0x0005F390 File Offset: 0x0005E390
		private static void CheckInterfaceCompatibility(bool bInterfaceLibrary, _ISignature signNew, _ISignature signOld, int nProjectHandleNew, List<IMessage> alMessages, HashSet<string> hsAttributesToIgnore, PreCompileContext pccOlderLibrary, PreCompileContext pccNewerLibrary)
		{
			if (!LibraryCompatibilityCheck.CheckNewSignExists(signNew, signOld, nProjectHandleNew, alMessages))
			{
				return;
			}
			if (!SignatureComparer.IsEqualPrecompile(signOld as _ISignature2, signNew as _ISignature2, true, hsAttributesToIgnore))
			{
				SourcePosition position = new SourcePosition(nProjectHandleNew, signNew.ObjectGuid, 0L, 0, 0);
				string stError = string.Format(Strings.POUChanged, signOld.POUType.ToString(), signOld.OrgName);
				alMessages.Add(new CompilerMessage(position, stError, Severity.Error, MessageId.None));
			}
			IList<_ISignature> list = pccOlderLibrary._GetSubSignatures(signOld.ObjectGuid);
			IList<_ISignature> list2 = pccNewerLibrary._GetSubSignatures(signNew.ObjectGuid);
			if (list == null)
			{
				return;
			}
			foreach (_ISignature isignature in list)
			{
				bool flag = true;
				if (list2 != null)
				{
					foreach (_ISignature isignature2 in list2)
					{
						if (isignature.Name == isignature2.Name)
						{
							if (!SignatureComparer.IsEqualPrecompile(isignature as _ISignature2, isignature2 as _ISignature2, true, hsAttributesToIgnore))
							{
								SourcePosition position2 = new SourcePosition(nProjectHandleNew, isignature2.ObjectGuid, 0L, 0, 0);
								string stError2 = string.Format(Strings.MethodChanged, isignature.OrgName, signOld.POUType.ToString(), signOld.OrgName);
								alMessages.Add(new CompilerMessage(position2, stError2, Severity.Error, MessageId.None));
							}
							if (bInterfaceLibrary && !LibraryCompatibilityCheck.CheckForChecksum(isignature2, isignature))
							{
								SourcePosition position3 = new SourcePosition(nProjectHandleNew, isignature2.ObjectGuid, 0L, 0, 0);
								string stError3 = string.Format(Strings.MethodChanged, isignature.OrgName, signOld.POUType.ToString(), signOld.OrgName);
								alMessages.Add(new CompilerMessage(position3, stError3, Severity.Error, MessageId.None));
							}
							flag = false;
							break;
						}
					}
				}
				if (flag)
				{
					SourcePosition position4 = new SourcePosition(nProjectHandleNew, signNew.ObjectGuid, 0L, 0, 0);
					string stError4 = string.Format(Strings.MethodRenamedOrDeleted, isignature.OrgName, signOld.POUType.ToString(), signOld.OrgName);
					alMessages.Add(new CompilerMessage(position4, stError4, Severity.Error, MessageId.None));
				}
			}
			if (list2 != null)
			{
				foreach (_ISignature isignature3 in list2)
				{
					bool flag2 = true;
					foreach (_ISignature isignature4 in list)
					{
						if (isignature3.Name == isignature4.Name)
						{
							flag2 = false;
							break;
						}
					}
					if (flag2)
					{
						SourcePosition position5 = new SourcePosition(nProjectHandleNew, signNew.ObjectGuid, 0L, 0, 0);
						string stError5 = string.Format(Strings.MethodAdded, isignature3.OrgName, signOld.POUType.ToString(), signOld.OrgName);
						alMessages.Add(new CompilerMessage(position5, stError5, Severity.Error, MessageId.None));
					}
				}
			}
			if (signNew != null && bInterfaceLibrary && !LibraryCompatibilityCheck.CheckForChecksum(signNew, signOld))
			{
				SourcePosition position6 = new SourcePosition(nProjectHandleNew, signNew.ObjectGuid, 0L, 0, 0);
				string stError6 = string.Format(Strings.InterfaceChanged, signOld.OrgName);
				alMessages.Add(new CompilerMessage(position6, stError6, Severity.Error, MessageId.None));
			}
		}

		// Token: 0x0600266A RID: 9834 RVA: 0x0005F750 File Offset: 0x0005E750
		private static void CheckPOUCompatibility(bool bInterfaceLibrary, _ISignature signOld, _ISignature signNew, int nProjectHandleNew, List<IMessage> alMessages, PreCompileContext pccOlderLibrary, PreCompileContext pccNewerLibrary)
		{
			if (signOld.GetFlag(SignatureFlag.Internal) && signNew.GetFlag(SignatureFlag.Internal))
			{
				return;
			}
			if (!SignatureComparer.IsCompatible(signOld, signNew))
			{
				SourcePosition position = new SourcePosition(nProjectHandleNew, signNew.ObjectGuid, 0L, 0, 0);
				string stError = string.Format(Strings.POUChanged, signOld.POUType.ToString(), signOld.OrgName);
				alMessages.Add(new CompilerMessage(position, stError, Severity.Error, MessageId.None));
			}
			IList<_ISignature> list = pccOlderLibrary._GetSubSignatures(signOld.ObjectGuid);
			IList<_ISignature> list2 = pccNewerLibrary._GetSubSignatures(signNew.ObjectGuid);
			if (list == null)
			{
				return;
			}
			foreach (_ISignature isignature in list)
			{
				bool flag = true;
				if (list2 != null)
				{
					foreach (_ISignature isignature2 in list2)
					{
						if (isignature.Name == isignature2.Name)
						{
							if (!SignatureComparer.IsCompatible(isignature, isignature2))
							{
								SourcePosition position2 = new SourcePosition(nProjectHandleNew, isignature2.ObjectGuid, 0L, 0, 0);
								string stError2 = string.Format(Strings.MethodChanged, isignature.OrgName, signOld.POUType.ToString(), signOld.OrgName);
								alMessages.Add(new CompilerMessage(position2, stError2, Severity.Error, MessageId.None));
							}
							if (bInterfaceLibrary)
							{
								uint checksum = isignature2.Checksum;
								uint checksum2 = isignature.Checksum;
								if (checksum != checksum2)
								{
									SourcePosition position3 = new SourcePosition(nProjectHandleNew, isignature2.ObjectGuid, 0L, 0, 0);
									string stError3 = string.Format(Strings.MethodChanged, isignature.OrgName, signOld.POUType.ToString(), signOld.OrgName);
									alMessages.Add(new CompilerMessage(position3, stError3, Severity.Error, MessageId.None));
								}
							}
							flag = false;
							break;
						}
					}
				}
				if (flag)
				{
					SourcePosition position4 = new SourcePosition(nProjectHandleNew, signNew.ObjectGuid, 0L, 0, 0);
					string stError4 = string.Format(Strings.MethodRenamedOrDeleted, isignature.OrgName, signOld.POUType.ToString(), signOld.OrgName);
					alMessages.Add(new CompilerMessage(position4, stError4, Severity.Error, MessageId.None));
				}
			}
		}

		// Token: 0x0600266B RID: 9835 RVA: 0x0005F9C8 File Offset: 0x0005E9C8
		private static void CheckGVLCompatibility(bool bInterfaceLibrary, _IVariable varOld, _IVariable varNew, int nProjectHandleNew, ISignature signNew, ISignature signOld, ICollection<IMessage> alMessages, IPrecompileScope scopeOld, IPrecompileScope scopeNew)
		{
			IExpression expression = varOld.Initial;
			IExpression expression2 = varNew.Initial;
			if (!bInterfaceLibrary)
			{
				expression = Common.RemoveConversions(varOld.Initial, false);
				expression2 = Common.RemoveConversions(varNew.Initial, false);
			}
			if (expression != null && expression.IsLiteral && expression2 != null && expression2.IsLiteral)
			{
				if (!varOld.IsEqual(varNew, false))
				{
					SourcePosition position = new SourcePosition(nProjectHandleNew, signNew.ObjectGuid, 0L, 0, 0);
					string stError = string.Format(Strings.VariableDeclarationChanged, varOld.OrgName, signOld.OrgName);
					alMessages.Add(new CompilerMessage(position, stError, Severity.Error, MessageId.None));
					return;
				}
				LiteralValue literalValue = (LiteralValue)(expression as _ILiteralExpression).Literal(scopeOld);
				LiteralValue lit = (LiteralValue)(expression2 as _ILiteralExpression).Literal(scopeNew);
				if (!literalValue.IsEqual(lit))
				{
					SourcePosition position2 = new SourcePosition(nProjectHandleNew, signNew.ObjectGuid, 0L, 0, 0);
					string stError2 = string.Format(Strings.VariableDeclarationChanged, varOld.OrgName, signOld.OrgName);
					alMessages.Add(new CompilerMessage(position2, stError2, Severity.Error, MessageId.None));
					return;
				}
			}
			else if (!varOld.IsEqual(varNew, true, true, false, null))
			{
				SourcePosition position3 = new SourcePosition(nProjectHandleNew, signNew.ObjectGuid, 0L, 0, 0);
				string stError3 = string.Format(Strings.VariableDeclarationChanged, varOld.OrgName, signOld.OrgName);
				alMessages.Add(new CompilerMessage(position3, stError3, Severity.Error, MessageId.None));
			}
		}

		// Token: 0x0600266C RID: 9836 RVA: 0x0005FB28 File Offset: 0x0005EB28
		public static bool CheckNewSignExists(ISignature signNew, ISignature signOld, int nProjectHandle, List<IMessage> alMessages)
		{
			if (signNew != null && signNew.POUType == signOld.POUType)
			{
				return true;
			}
			if (signOld.POUType == Operator.VarGlobal && signOld.All.Length == 0)
			{
				return false;
			}
			SourcePosition position = null;
			if (signNew != null)
			{
				position = new SourcePosition(nProjectHandle, signNew.ObjectGuid, 0L, 0, 0);
			}
			string stError = string.Empty;
			Operator poutype = signOld.POUType;
			if (poutype != Operator.Type)
			{
				if (poutype == Operator.VarGlobal)
				{
					stError = string.Format(Strings.GVLRenamedOrDeleted, signOld.OrgName);
				}
				else
				{
					stError = string.Format(Strings.POURenamedOrDeleted, signOld.POUType.ToString(), signOld.OrgName);
				}
			}
			else
			{
				stError = string.Format(Strings.TypeRenamedOrDeleted, signOld.OrgName);
			}
			alMessages.Add(new CompilerMessage(position, stError, Severity.Error, MessageId.None));
			return false;
		}

		// Token: 0x0600266D RID: 9837 RVA: 0x0005FBE8 File Offset: 0x0005EBE8
		private static bool CalculateEnumConstants(_ISignature sign, IPrecompileScope scope, IDictionary<string, ILiteralValue> htEnums, out TypeClass tc)
		{
			tc = TypeClass.None;
			if (!sign.GetFlag(SignatureFlag.Enum))
			{
				return false;
			}
			IntegerUnion integerUnion = new IntegerUnion
			{
				m_long = 0L
			};
			foreach (_IVariable ivariable in sign.AllVariables)
			{
				_IType type = ivariable._Type;
				if (((type != null) ? type.BaseType : null) == null || htEnums.ContainsKey(ivariable.Name))
				{
					return false;
				}
				tc = ivariable._Type.BaseType.Class;
				bool flag = TypeTable.IsSigned(ivariable._Type.BaseType.Class);
				if (ivariable._Initial == null)
				{
					htEnums.Add(ivariable.Name, flag ? LanguageModelBuilder.Singleton.CreateLiteralExpression(null, integerUnion.m_long).LiteralValue : LanguageModelBuilder.Singleton.CreateLiteralExpression(null, integerUnion.m_ulong).LiteralValue);
				}
				else
				{
					ILiteralValue literalValue;
					if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351400)
					{
						bool flag2 = false;
						literalValue = ivariable._Initial.LiteralWithRecursionCheck(scope, new LDictionary<IVariable, IVariable>(), true, out flag2);
					}
					else
					{
						_ILiteralExpression iliteralExpression = ivariable._Initial as _ILiteralExpression;
						literalValue = ((iliteralExpression != null) ? iliteralExpression.LiteralValue : null);
					}
					if (literalValue == null)
					{
						return false;
					}
					KindOfLiteral kindOf = literalValue.KindOf;
					if (kindOf != KindOfLiteral.SignedInteger)
					{
						if (kindOf != KindOfLiteral.UnsignedInteger)
						{
							return false;
						}
						integerUnion.m_ulong = literalValue.UnsignedLong;
					}
					else
					{
						integerUnion.m_long = literalValue.SignedLong;
					}
					htEnums.Add(ivariable.Name, literalValue);
				}
				integerUnion.m_long += 1L;
			}
			return true;
		}
	}
}
