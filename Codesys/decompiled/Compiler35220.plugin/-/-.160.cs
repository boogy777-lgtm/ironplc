using System;
using \u0003;
using \u0006;
using \u0017;
using \u0019;
using _3S.CoDeSys.Compiler35220.PreCompile;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using \u0081;

namespace \u000E
{
	// Token: 0x020001B6 RID: 438
	internal static class \u000F
	{
		// Token: 0x06002024 RID: 8228 RVA: 0x0006CE5C File Offset: 0x0006B05C
		internal static bool \u0001(ref _IExpression \u0002, bool \u0003)
		{
			if (\u0003)
			{
				ICompiledType type = \u0002.Type;
				if (type != null && type.Class == TypeClass.Reference)
				{
					_IDeRefAccessExpression ideRefAccessExpression = global::\u0019.\u0003.Builder.CreateDeRefAccessExpression(\u0002);
					ideRefAccessExpression._CompiledType = \u0002.Type.DeRefType;
					\u0002 = ideRefAccessExpression;
					return true;
				}
			}
			return false;
		}

		// Token: 0x06002025 RID: 8229 RVA: 0x0006CEAC File Offset: 0x0006B0AC
		internal static bool \u0001(global::\u0017.\u0010 \u0002, _IExprement \u0003, ICompiledType \u0004, ICompiledType \u0005, ICommonScope \u0006, ICommonScope \u0007, ref _IExpression \u0008, out bool \u000E)
		{
			\u000E = false;
			if (\u0004 == null || \u0005 == null)
			{
				return false;
			}
			ICompiledType compiledType = (\u0005.Class == TypeClass.Reference) ? \u0005.BaseType : \u0005;
			if (compiledType.Class == TypeClass.Enum)
			{
				_ISignature isignature = \u0007.FindSignature(compiledType as IEnumType) as _ISignature;
				if (!global::\u000E.\u000F.\u0001(\u0007, \u0004, \u0005) && isignature != null && !global::\u000E.\u000F.\u0001(\u0008, isignature, \u0007))
				{
					global::\u000E.\u000F.\u0001(\u0008, MessageId.Err_StrictEnumNotAMember, new object[]
					{
						\u0008.ToString(),
						isignature.OrgName
					});
					return false;
				}
			}
			ILiteralValue literalValue = \u0006.GetLiteralValue(\u0008, true);
			if (global::\u0006.\u0011.\u0001(\u0005, \u0007) || global::\u0006.\u0011.\u0001(\u0005.DeRefType, \u0007))
			{
				int num = -1;
				if (literalValue != null && literalValue.GetInt(out num) && num == 0)
				{
					return true;
				}
			}
			bool u = false;
			if (literalValue != null)
			{
				if (!global::\u000E.\u000F.\u0001(\u0002, \u0003, \u0004, \u0005, \u0006, \u0007, ref \u0008, ref \u000E, literalValue))
				{
					return false;
				}
			}
			else
			{
				if (!global::\u0006.\u0011.\u0008(\u0004, \u0005, \u0006, \u0007))
				{
					global::\u000E.\u000F.\u0001(\u0002, \u0003, \u0004, \u0005, \u0006, ref \u0008, ref \u000E);
					return false;
				}
				if (\u0002.ApplicationGuid == Guid.Empty && global::\u0006.\u0011.\u0005(\u0004, \u0005, \u0006, \u0007))
				{
					u = global::\u000E.\u000F.\u0001(\u0002, \u0003, \u0004, \u0005);
				}
			}
			global::\u000E.\u000F.\u0001(\u0002, \u0003, \u0004, \u0005);
			TypeClass typeClass = \u0004.DeRefType.Class;
			if (\u0004.DeRefType.Class == TypeClass.Pointer)
			{
				if (TypeTable.GetSize2(TypeClass.Pointer, \u0006) == 8)
				{
					typeClass = TypeClass.LWord;
				}
				else
				{
					typeClass = TypeClass.DWord;
				}
			}
			if (global::\u000E.\u000F.\u0001(\u0004, \u0005, \u0006, \u0007, typeClass))
			{
				if (global::\u000E.\u000F.\u0001(\u0005, \u0007, ref \u0008))
				{
					return true;
				}
				bool flag = TypeTable.IsAnyType(\u0005.Class);
				global::\u000E.\u000F.\u0001(\u0002, \u0003, \u0004, \u0005, \u0006, \u0007, u, flag);
				global::\u000E.\u000F.\u0001(\u0002, \u0003, \u0004, \u0005, \u0006, \u0007);
				global::\u000E.\u000F.\u0001(\u0002, \u0005, \u0006, ref \u0008, ref \u000E, typeClass, flag);
			}
			return true;
		}

		// Token: 0x06002026 RID: 8230 RVA: 0x0006D068 File Offset: 0x0006B268
		private static void \u0001(global::\u0017.\u0010 \u0002, ICompiledType \u0003, ICommonScope \u0004, ref _IExpression \u0005, ref bool \u0006, TypeClass \u0007, bool \u0008)
		{
			if (!global::\u0006.\u0011.\u0001(\u0007, \u0003.DeRefType.Class, \u0002.TreatLRealAsReal, \u0002.TreatInt64AsInt32, TypeTable.GetSize2(TypeClass.Pointer, \u0004) == 8) && !\u0008)
			{
				_IImplicitConversionExpression iimplicitConversionExpression = global::\u0019.\u0003.\u0001(\u0007, \u0003.DeRefType.Class, Token.Empty);
				iimplicitConversionExpression._Exp = \u0005;
				\u0005 = iimplicitConversionExpression;
				\u0005.Type = \u0003.DeRefType;
				if (\u0002.\u0001(iimplicitConversionExpression))
				{
					\u0006 = true;
				}
			}
		}

		// Token: 0x06002027 RID: 8231 RVA: 0x0006D0E4 File Offset: 0x0006B2E4
		private static void \u0001(global::\u0017.\u0010 \u0002, _IExprement \u0003, ICompiledType \u0004, ICompiledType \u0005, ICommonScope \u0006, ICommonScope \u0007)
		{
			if (TypeTable.IsInteger(\u0004.Class) && TypeTable.IsReal(\u0005.Class) && TypeTable.GetSize2(\u0004.Class, \u0006) >= TypeTable.GetSize2(\u0005.Class, \u0007) && !\u0002.NoConversionChecks && !\u0002.InImplicitCode)
			{
				\u0002.\u0001(\u0003, MessageId.Wrn_ImplicitIntToReal, new object[]
				{
					\u0004,
					\u0005
				});
			}
		}

		// Token: 0x06002028 RID: 8232 RVA: 0x0006D154 File Offset: 0x0006B354
		private static void \u0001(global::\u0017.\u0010 \u0002, _IExprement \u0003, ICompiledType \u0004, ICompiledType \u0005, ICommonScope \u0006, ICommonScope \u0007, bool \u0008, bool \u000E)
		{
			TypeClass tc = \u0004.DeRefType.Class;
			TypeClass @class = \u0005.DeRefType.Class;
			if (\u0004.Class == TypeClass.Enum && TypeTable.IsSigned(tc) != TypeTable.IsSigned(@class) && TypeTable.GetSize2(tc, \u0006) <= TypeTable.GetSize2(@class, \u0007))
			{
				ISignature signature = \u0006.FindSignature(\u0004 as _IEnumType);
				if (signature == null)
				{
					return;
				}
				if (signature.HasAttribute("nounsignedcheck"))
				{
					tc = @class;
				}
			}
			else if (\u0005.Class == TypeClass.Enum && TypeTable.IsSigned(tc) != TypeTable.IsSigned(@class) && TypeTable.GetSize2(tc, \u0006) <= TypeTable.GetSize2(@class, \u0007) && \u0007.FindSignature(\u0005 as _IEnumType).HasAttribute("nounsignedcheck"))
			{
				tc = @class;
			}
			if (TypeTable.IsSigned(tc) && !TypeTable.IsSigned(@class) && !\u000E)
			{
				if (!\u0008 && !\u0002.NoConversionChecks && !\u0002.InImplicitCode)
				{
					\u0002.\u0001(\u0003, MessageId.Wrn_ImplicitSignedToUnsigned, new object[]
					{
						\u0004,
						\u0005
					});
					return;
				}
			}
			else if (!TypeTable.IsSigned(tc) && TypeTable.IsSigned(@class) && !TypeTable.IsReal(@class) && TypeTable.GetSize2(tc, \u0006) == TypeTable.GetSize2(@class, \u0007))
			{
				bool flag = \u0003 is IVariableExpression && (\u0003 as IVariableExpression).Name == "LOG_STD_LOGGER" && \u0004.Class == TypeClass.Pointer;
				if (!\u0008 && !\u0002.NoConversionChecks && !\u0002.InImplicitCode && !flag)
				{
					\u0002.\u0001(\u0003, MessageId.Wrn_ImplicitUnsignedToSigned, new object[]
					{
						\u0004,
						\u0005
					});
				}
			}
		}

		// Token: 0x06002029 RID: 8233 RVA: 0x0006D2E4 File Offset: 0x0006B4E4
		private static bool \u0001(ICompiledType \u0002, ICommonScope \u0003, ref _IExpression \u0004)
		{
			if (\u0004 is _ILiteralExpression)
			{
				_ILiteralExpression iliteralExpression = \u0004 as _ILiteralExpression;
				if (iliteralExpression.ConstantType == TypeClass.None || iliteralExpression.ConstantType == TypeClass.AnyInt || iliteralExpression.ConstantType == TypeClass.AnyReal)
				{
					if (iliteralExpression.ConstantType == TypeClass.AnyInt && (\u0002.DeRefType.Class == TypeClass.Real || \u0002.DeRefType.Class == TypeClass.LReal))
					{
						if (iliteralExpression.Negative)
						{
							\u0004 = global::\u0019.\u0003.\u0001((double)iliteralExpression.LongValue);
						}
						else
						{
							\u0004 = global::\u0019.\u0003.\u0001(iliteralExpression.ULongValue);
						}
					}
					if (\u0002.DeRefType.Class == TypeClass.Pointer)
					{
						if (TypeTable.GetSize2(TypeClass.Pointer, \u0003) == 8)
						{
							iliteralExpression.ConstantType = TypeClass.LWord;
						}
						else
						{
							iliteralExpression.ConstantType = TypeClass.DWord;
							if (iliteralExpression.Negative)
							{
								long longValue = iliteralExpression.LongValue;
								iliteralExpression.LongValue = (long)((ulong)new IntegerUnion
								{
									m_long = longValue
								}.m_uint0);
							}
						}
					}
					else
					{
						iliteralExpression.ConstantType = \u0002.DeRefType.Class;
					}
					\u0004.Type = \u0002.DeRefType;
					return true;
				}
			}
			return false;
		}

		// Token: 0x0600202A RID: 8234 RVA: 0x0006D3F0 File Offset: 0x0006B5F0
		private static bool \u0001(ICompiledType \u0002, ICompiledType \u0003, ICommonScope \u0004, ICommonScope \u0005, TypeClass \u0006)
		{
			return \u0006 != TypeClass.Array && \u0006 != TypeClass.String && \u0006 != TypeClass.Pointer && \u0006 != TypeClass.Userdef && !global::\u0006.\u0011.\u0001(\u0002.DeRefType, \u0003.DeRefType, \u0004, \u0005) && \u0002.DeRefType.Class != \u0003.DeRefType.Class;
		}

		// Token: 0x0600202B RID: 8235 RVA: 0x0006D448 File Offset: 0x0006B648
		private static void \u0001(global::\u0017.\u0010 \u0002, _IExprement \u0003, ICompiledType \u0004, ICompiledType \u0005)
		{
			if (\u0004.Class == \u0005.Class && \u0005.Class == TypeClass.Enum)
			{
				_IEnumType ienumType = (\u0004.Class == TypeClass.Enum) ? (\u0004 as _IEnumType) : (\u0004.DeRefType as _IEnumType);
				_IEnumType ienumType2 = (\u0005.Class == TypeClass.Enum) ? (\u0005 as _IEnumType) : (\u0005.DeRefType as _IEnumType);
				if (ienumType != null && ienumType2 != null && ienumType.SignatureId != ienumType2.SignatureId)
				{
					\u0002.\u0001(\u0003, MessageId.Wrn_ImplicitEnumConversion, new object[]
					{
						ienumType,
						ienumType2
					});
				}
			}
		}

		// Token: 0x0600202C RID: 8236 RVA: 0x0006D4D8 File Offset: 0x0006B6D8
		private static bool \u0001(global::\u0017.\u0010 \u0002, _IExprement \u0003, ICompiledType \u0004, ICompiledType \u0005)
		{
			bool flag = true;
			if (TypeTable.IsResolvedXType(\u0005))
			{
				flag = false;
			}
			if (TypeTable.IsResolvedXType(\u0004))
			{
				flag = false;
			}
			if (flag)
			{
				global::\u000E.\u000F.\u0001(\u0002, \u0003, \u0004, \u0005);
			}
			return flag;
		}

		// Token: 0x0600202D RID: 8237 RVA: 0x0006D508 File Offset: 0x0006B708
		private static void \u0001(global::\u0017.\u0010 \u0002, _IExprement \u0003, ICompiledType \u0004, ICompiledType \u0005, ICommonScope \u0006, ref _IExpression \u0007, ref bool \u0008)
		{
			if (\u0002.ConvertAllTypeMismatches)
			{
				_IImplicitConversionExpression iimplicitConversionExpression = global::\u0019.\u0003.\u0001(\u0004.DeRefType.Class, \u0005.DeRefType.Class, Token.Empty);
				iimplicitConversionExpression._Exp = \u0007;
				\u0007 = iimplicitConversionExpression;
				\u0007.Type = \u0005.DeRefType;
				if (\u0002.\u0001(iimplicitConversionExpression))
				{
					\u0008 = true;
					return;
				}
			}
			else
			{
				global::\u000E.\u000F.\u0001(\u0002, \u0006, \u0003, \u0004, \u0005);
			}
		}

		// Token: 0x0600202E RID: 8238 RVA: 0x0006D574 File Offset: 0x0006B774
		private static bool \u0001(global::\u0017.\u0010 \u0002, _IExprement \u0003, ICompiledType \u0004, ICompiledType \u0005, ICommonScope \u0006, ICommonScope \u0007, ref _IExpression \u0008, ref bool \u000E, ILiteralValue \u000F)
		{
			int size = \u0007.GetSize(\u0005.DeRefType);
			if (!global::\u000E.\u000F.\u0001(\u0008, \u0005, size) && !global::\u0006.\u0011.\u0001(\u000F, \u0004, \u0005, \u0006, \u0007))
			{
				if (\u0002.ConvertAllTypeMismatches)
				{
					_IImplicitConversionExpression iimplicitConversionExpression = global::\u0019.\u0003.\u0001(\u0004.DeRefType.Class, \u0005.DeRefType.Class, Token.Empty);
					iimplicitConversionExpression._Exp = \u0008;
					\u0008 = iimplicitConversionExpression;
					\u0008.Type = \u0005.DeRefType;
					if (\u0002.\u0001(iimplicitConversionExpression))
					{
						\u000E = true;
					}
				}
				else if (\u0005.Class == TypeClass.Subrange)
				{
					\u0002.AddError(\u0008, MessageId.Err_TypeMismatch, new object[]
					{
						\u0008,
						\u0005
					});
				}
				else if (global::\u0006.\u0011.\u0008(\u0004, \u0005, \u0006, \u0007))
				{
					if (TypeTable.IsSigned(\u0004.Class) && !TypeTable.IsSigned(\u0005.Class))
					{
						if (!\u0002.NoConversionChecks && !\u0002.InImplicitCode)
						{
							\u0002.\u0001(\u0003, MessageId.Wrn_ImplicitSignedToUnsigned, new object[]
							{
								\u0004,
								\u0005
							});
						}
					}
					else if (!TypeTable.IsSigned(\u0004.Class) && TypeTable.IsSigned(\u0005.Class) && !TypeTable.IsReal(\u0005.Class) && TypeTable.GetSize2(\u0004.Class, \u0006) == TypeTable.GetSize2(\u0005.Class, \u0007) && !\u0002.NoConversionChecks && !\u0002.InImplicitCode)
					{
						\u0002.\u0001(\u0003, MessageId.Wrn_ImplicitUnsignedToSigned, new object[]
						{
							\u0004,
							\u0005
						});
					}
					_IImplicitConversionExpression iimplicitConversionExpression2;
					if (\u0005.DeRefType.Class == TypeClass.Pointer)
					{
						TypeClass u = TypeClass.DWord;
						if (TypeTable.GetSize2(TypeClass.Pointer, \u0007) == 8)
						{
							u = TypeClass.LWord;
						}
						iimplicitConversionExpression2 = global::\u0019.\u0003.\u0001(\u0004.DeRefType.Class, u, Token.Empty);
					}
					else
					{
						iimplicitConversionExpression2 = global::\u0019.\u0003.\u0001(\u0004.DeRefType.Class, \u0005.DeRefType.Class, Token.Empty);
					}
					iimplicitConversionExpression2._Exp = \u0008;
					\u0008 = iimplicitConversionExpression2;
					\u0008.Type = \u0005.DeRefType;
				}
				else
				{
					global::\u000E.\u000F.\u0001(\u0002, \u0006, \u0003, \u0004, \u0005);
				}
				return false;
			}
			return true;
		}

		// Token: 0x0600202F RID: 8239 RVA: 0x0006D780 File Offset: 0x0006B980
		internal static ICaseInsensitiveDictionary<IExpression> \u0001(_ICallExpression \u0002, ISignature \u0003)
		{
			ICaseInsensitiveDictionary<IExpression> caseInsensitiveDictionary = new CaseInsensitiveDictionary<IExpression>();
			ICaseInsensitiveDictionary<object> caseInsensitiveDictionary2 = new CaseInsensitiveDictionary<object>();
			foreach (IVariable variable in \u0003.AllInputs)
			{
				if (!variable.GetFlag(VarFlag.Implicit))
				{
					if (variable.Initial == null)
					{
						caseInsensitiveDictionary2[variable.Name] = null;
					}
					else
					{
						caseInsensitiveDictionary[variable.Name] = variable.Initial;
					}
				}
			}
			IAssignmentExpression[] inputAssigns = \u0002.InputAssigns;
			for (int i = 0; i < inputAssigns.Length; i++)
			{
				_IVariableExpression ivariableExpression = ((_IAssignmentExpression)inputAssigns[i]).LValue as _IVariableExpression;
				if (ivariableExpression != null && ivariableExpression.Name != null)
				{
					caseInsensitiveDictionary.Remove(ivariableExpression.Name);
					caseInsensitiveDictionary2.Remove(ivariableExpression.Name);
				}
			}
			if (0 < caseInsensitiveDictionary2.Count)
			{
				caseInsensitiveDictionary.Clear();
			}
			return caseInsensitiveDictionary;
		}

		// Token: 0x06002030 RID: 8240 RVA: 0x0006D854 File Offset: 0x0006BA54
		internal static bool \u0001(ICommonScope \u0002, ICompiledType \u0003, ICompiledType \u0004)
		{
			ISignature signature = global::\u000E.\u000F.\u0001(\u0002, \u0004);
			ISignature signature2 = global::\u000E.\u000F.\u0001(\u0002, \u0003);
			return (signature != null && signature == signature2) || \u0003.IsEqual(\u0004);
		}

		// Token: 0x06002031 RID: 8241 RVA: 0x0006D884 File Offset: 0x0006BA84
		private static ISignature \u0001(ICommonScope \u0002, ICompiledType \u0003)
		{
			if (\u0003 is _IEnumType)
			{
				return \u0002.FindSignature(\u0003 as _IEnumType);
			}
			if (\u0003 is _IReferenceType)
			{
				if (\u0003.BaseType is _IEnumType)
				{
					return \u0002.FindSignature(\u0003.BaseType as _IEnumType);
				}
				return null;
			}
			else
			{
				if (\u0003 is _IUserdefType)
				{
					return \u0002.FindSignature(\u0003 as _IUserdefType);
				}
				return null;
			}
		}

		// Token: 0x06002032 RID: 8242 RVA: 0x0006D8E8 File Offset: 0x0006BAE8
		internal static bool \u0001(_IExpression \u0002, _ISignature \u0003, ICommonScope \u0004)
		{
			if (!\u0003.HasAttribute("strict"))
			{
				return true;
			}
			IMultipleIndexInitialization multipleIndexInitialization = \u0002 as IMultipleIndexInitialization;
			if (multipleIndexInitialization != null)
			{
				\u0002 = (multipleIndexInitialization.Value as _IExpression);
			}
			ILiteralValue literalValue = \u0004.GetLiteralValue(\u0002, true);
			if (literalValue == null)
			{
				return global::\u000E.\u000F.\u0001(\u0002, \u0004);
			}
			bool flag;
			long num = literalValue.GetSignedLong(out flag);
			if (!flag)
			{
				num = (long)literalValue.GetUnsignedLong(out flag);
			}
			long num2 = 0L;
			bool result = false;
			foreach (IVariable variable in \u0003.Constant)
			{
				ILiteralValue2 literalValue2 = (variable.Initial == null) ? null : (\u0004.GetLiteralValue(variable.Initial, true) as ILiteralValue2);
				if (literalValue2 != null)
				{
					if (literalValue2.IsValueEqual(literalValue))
					{
						result = true;
						break;
					}
					bool flag2;
					num2 = literalValue2.GetSignedLong(out flag2);
					if (!flag2)
					{
						num2 = (long)literalValue2.GetUnsignedLong(out flag2);
						if (!flag2)
						{
							return false;
						}
					}
				}
				else if (flag && num == num2)
				{
					result = true;
					break;
				}
				num2 += 1L;
			}
			return result;
		}

		// Token: 0x06002033 RID: 8243 RVA: 0x0006D9E0 File Offset: 0x0006BBE0
		internal static bool \u0001(_IExpression \u0002, ICommonScope \u0003)
		{
			IVariable variable = null;
			if (\u0003 is IPrecompileScope)
			{
				variable = \u0002.GetVariable(\u0003 as IPrecompileScope);
			}
			else if (\u0003 is IScope)
			{
				variable = \u0002.GetVariable(\u0003 as IScope);
			}
			return variable != null && variable.HasAttribute("suspend_strict");
		}

		// Token: 0x06002034 RID: 8244 RVA: 0x0006DA30 File Offset: 0x0006BC30
		private static bool \u0001(_IExpression \u0002, ICompiledType \u0003, int \u0004)
		{
			bool result = false;
			_ILiteralExpression iliteralExpression = \u0002 as _ILiteralExpression;
			if (iliteralExpression != null && \u0002.Type != null)
			{
				int length = iliteralExpression.StringValue.Length;
				if ((\u0002.Type.Class == TypeClass.String && \u0003.Class == TypeClass.String && length > \u0004 - 1) || (\u0002.Type.Class == TypeClass.WString && \u0003.Class == TypeClass.WString && length * 2 > \u0004 - 2))
				{
					int num = (TypeClass.String == \u0003.Class) ? (\u0004 - 1) : ((\u0004 - 2) / 2);
					string text = \u0002.ToString();
					if (length < 1000)
					{
						if (num >= 3)
						{
							num -= 3;
						}
						text = text.Substring(0, num) + "...";
					}
					else
					{
						int num2 = length - num;
						string str = string.Format(\u0081.\u0001.TooLongStringLiteralHint, num2);
						int num3 = num / 2;
						text = text.Substring(0, num3) + str + text.Substring(num3 + num2);
					}
					global::\u0003.\u0006.\u0001(\u0002, Severity.Warning, MessageId.Wrn_StringConstantTooLong, new object[]
					{
						text,
						\u0003.ToString()
					});
					result = true;
				}
			}
			return result;
		}

		// Token: 0x06002035 RID: 8245 RVA: 0x0006DB50 File Offset: 0x0006BD50
		internal static void \u0001(global::\u0017.\u0010 \u0002, ICommonScope \u0003, _IExprement \u0004, ICompiledType \u0005, ICompiledType \u0006)
		{
			string text = global::\u000E.\u000F.\u0001(\u0003, \u0005);
			string text2 = global::\u000E.\u000F.\u0001(\u0003, \u0006);
			\u0002.AddError(\u0004, MessageId.Err_TypeMismatch, new object[]
			{
				text,
				text2
			});
		}

		// Token: 0x06002036 RID: 8246 RVA: 0x0006DB88 File Offset: 0x0006BD88
		private static void \u0001(global::\u0017.\u0010 \u0002, _IExprement \u0003, IType \u0004, IType \u0005)
		{
			\u0002.\u0001(\u0003, MessageId.Wrn_PointerMisatch, new object[]
			{
				\u0004,
				\u0005
			});
		}

		// Token: 0x06002037 RID: 8247 RVA: 0x0006DBA4 File Offset: 0x0006BDA4
		internal static string \u0001(ICommonScope \u0002, ICompiledType \u0003)
		{
			string text = \u0003.ToString();
			if (\u0002 != null && \u0003.DeRefType.Class == TypeClass.Userdef)
			{
				ISignature signature = \u0002.FindSignature(\u0003.DeRefType as _IUserdefType);
				if (signature != null && !string.IsNullOrEmpty(signature.LibraryPath))
				{
					text = text + "(" + signature.LibraryPath + ")";
				}
			}
			return text;
		}

		// Token: 0x06002038 RID: 8248 RVA: 0x0006DC04 File Offset: 0x0006BE04
		private static void \u0001(_IExprement \u0002, MessageId \u0003, params object[] \u0004)
		{
			string format = global::\u000E.\u0018.\u0001(\u0003);
			\u0002.AddError(string.Format(format, \u0004), \u0003);
		}

		// Token: 0x06002039 RID: 8249 RVA: 0x0006DC28 File Offset: 0x0006BE28
		internal static bool \u0001(ICompiledType \u0002, ICompiledType \u0003, ref IExpression \u0004)
		{
			try
			{
				if (\u0002 == null || \u0003 == null)
				{
					return false;
				}
				if (\u0002.DeRefType.Class == TypeClass.Userdef || \u0003.DeRefType.Class == TypeClass.Userdef)
				{
					return false;
				}
				if (!global::\u0006.\u0011.\u0002(\u0002, \u0003, null))
				{
					return false;
				}
				if (\u0002.DeRefType.Class != TypeClass.Array && \u0002.DeRefType.Class != TypeClass.String && \u0002.DeRefType.Class != TypeClass.Pointer && \u0002.DeRefType.Class != TypeClass.Userdef && !global::\u0006.\u0011.\u0001(\u0002.DeRefType, \u0003.DeRefType, null) && \u0002.DeRefType.Class != \u0003.DeRefType.Class && !global::\u0006.\u0011.\u0001(\u0002.DeRefType.Class, \u0003.DeRefType.Class, false, false, false))
				{
					_IImplicitConversionExpression iimplicitConversionExpression = global::\u0019.\u0003.\u0001(\u0002.DeRefType.Class, \u0003.DeRefType.Class, Token.Empty);
					iimplicitConversionExpression._Exp = (\u0004 as _IExpression);
					\u0004 = iimplicitConversionExpression;
					(\u0004 as _IExpression).Type = \u0003.DeRefType;
				}
			}
			catch
			{
				return false;
			}
			return true;
		}
	}
}
