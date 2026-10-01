using System;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.Legacy
{
	// Token: 0x02000280 RID: 640
	internal static class ConstantFolding
	{
		// Token: 0x06002AE0 RID: 10976 RVA: 0x0006FB38 File Offset: 0x0006EB38
		internal static ILiteralValue GetLiteral(ICompiledType Type, Operator Code, ILiteralValue[] litvalOps, bool bPrecompile)
		{
			ulong num = 0UL;
			long num2 = 0L;
			bool flag = false;
			double num3 = 0.0;
			string text = null;
			KindOfLiteral kindOfLiteral;
			if (Type == null)
			{
				if ((!APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35300 && (!APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV34561 || APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35000)) || !bPrecompile)
				{
					return null;
				}
				if (litvalOps.Length == 0 || litvalOps[0].KindOf == KindOfLiteral.None)
				{
					return null;
				}
				if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35800)
				{
					if (Code - Operator.Expt <= 9)
					{
						kindOfLiteral = KindOfLiteral.Float;
					}
					else
					{
						kindOfLiteral = litvalOps[0].KindOf;
					}
				}
				else
				{
					kindOfLiteral = litvalOps[0].KindOf;
				}
			}
			else if (TypeTable.IsInteger(Type.Class))
			{
				if (TypeTable.IsSigned(Type.Class))
				{
					kindOfLiteral = KindOfLiteral.SignedInteger;
				}
				else
				{
					kindOfLiteral = KindOfLiteral.UnsignedInteger;
				}
			}
			else if (TypeTable.IsReal(Type.Class))
			{
				kindOfLiteral = KindOfLiteral.Float;
			}
			else if (Type.Class == TypeClass.Bool)
			{
				kindOfLiteral = KindOfLiteral.Bool;
			}
			else if (Type.Class == TypeClass.String && APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35800)
			{
				kindOfLiteral = KindOfLiteral.String;
			}
			else
			{
				if (Type.Class != TypeClass.WString || !APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35800)
				{
					return null;
				}
				kindOfLiteral = KindOfLiteral.String;
			}
			if (Code != Operator.Mux)
			{
				checked
				{
					if (Code != Operator.Sel)
					{
						if (Code == Operator.Move)
						{
							return litvalOps[0];
						}
						if (kindOfLiteral == KindOfLiteral.String)
						{
							return null;
						}
						ILiteralValue result;
						try
						{
							int i = 0;
							while (i < litvalOps.Length)
							{
								ulong num4 = 0UL;
								long num5 = 0L;
								bool flag2 = false;
								double num6 = 0.0;
								string text2 = null;
								ILiteralValue literalValue = litvalOps[i];
								switch (literalValue.KindOf)
								{
								case KindOfLiteral.SignedInteger:
									num5 = literalValue.SignedLong;
									if (kindOfLiteral == KindOfLiteral.UnsignedInteger)
									{
										num4 = (ulong)num5;
									}
									else if (kindOfLiteral == KindOfLiteral.Float)
									{
										num6 = (double)num5;
									}
									else if (kindOfLiteral == KindOfLiteral.Bool)
									{
										flag2 = (num5 != 0L);
									}
									break;
								case KindOfLiteral.UnsignedInteger:
									num4 = literalValue.UnsignedLong;
									if (kindOfLiteral == KindOfLiteral.SignedInteger)
									{
										num5 = (long)num4;
									}
									else if (kindOfLiteral == KindOfLiteral.Float)
									{
										num6 = num4;
									}
									else if (kindOfLiteral == KindOfLiteral.Bool && APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV33200)
									{
										flag2 = (num4 > 0UL);
									}
									break;
								case KindOfLiteral.Float:
									num6 = literalValue.Float;
									break;
								case KindOfLiteral.String:
									if (!APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV33200 || (Code != Operator.Eq && Code != Operator.Equal && Code != Operator.Ne && Code != Operator.NotEqual))
									{
										return null;
									}
									text2 = literalValue.String;
									break;
								case KindOfLiteral.Bool:
									flag2 = literalValue.Bool;
									break;
								default:
									return null;
								}
								if (Code <= Operator.NotEqual)
								{
									switch (Code)
									{
									case Operator.Abs:
										switch (kindOfLiteral)
										{
										case KindOfLiteral.SignedInteger:
											num2 = Math.Abs(num5);
											goto IL_16CD;
										case KindOfLiteral.UnsignedInteger:
											num = num4;
											goto IL_16CD;
										case KindOfLiteral.Float:
											num3 = Math.Abs(num6);
											goto IL_16CD;
										default:
											return null;
										}
										break;
									case Operator.Limit:
										if (!APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV34100)
										{
											return null;
										}
										switch (kindOfLiteral)
										{
										case KindOfLiteral.SignedInteger:
											if (i == 0)
											{
												num2 = num5;
												goto IL_16CD;
											}
											if (i == 1)
											{
												num2 = Math.Max(num2, num5);
												goto IL_16CD;
											}
											num2 = Math.Min(num2, num5);
											goto IL_16CD;
										case KindOfLiteral.UnsignedInteger:
											if (i == 0)
											{
												num = num4;
												goto IL_16CD;
											}
											if (i == 1)
											{
												num = Math.Max(num, num4);
												goto IL_16CD;
											}
											num = Math.Min(num, num4);
											goto IL_16CD;
										case KindOfLiteral.Float:
											if (i == 0)
											{
												num3 = num6;
												goto IL_16CD;
											}
											if (i == 1)
											{
												num3 = Math.Max(num3, num6);
												goto IL_16CD;
											}
											num3 = Math.Min(num3, num6);
											goto IL_16CD;
										default:
											return null;
										}
										break;
									case Operator.Min:
										switch (kindOfLiteral)
										{
										case KindOfLiteral.SignedInteger:
											if (i == 0)
											{
												num2 = num5;
												goto IL_16CD;
											}
											num2 = Math.Min(num2, num5);
											goto IL_16CD;
										case KindOfLiteral.UnsignedInteger:
											if (i == 0)
											{
												num = num4;
												goto IL_16CD;
											}
											num = Math.Min(num, num4);
											goto IL_16CD;
										case KindOfLiteral.Float:
											if (i == 0)
											{
												num3 = num6;
												goto IL_16CD;
											}
											num3 = Math.Min(num3, num6);
											goto IL_16CD;
										default:
											return null;
										}
										break;
									case Operator.Max:
										switch (kindOfLiteral)
										{
										case KindOfLiteral.SignedInteger:
											if (i == 0)
											{
												num2 = num5;
												goto IL_16CD;
											}
											num2 = Math.Max(num2, num5);
											goto IL_16CD;
										case KindOfLiteral.UnsignedInteger:
											if (i == 0)
											{
												num = num4;
												goto IL_16CD;
											}
											num = Math.Max(num, num4);
											goto IL_16CD;
										case KindOfLiteral.Float:
											if (i == 0)
											{
												num3 = num6;
												goto IL_16CD;
											}
											num3 = Math.Max(num3, num6);
											goto IL_16CD;
										default:
											return null;
										}
										break;
									case Operator.Trunc:
										num2 = (long)num6;
										goto IL_16CD;
									case Operator.Mux:
									case Operator.Sel:
										goto IL_16C8;
									case Operator.Rol:
									{
										if (Type == null)
										{
											return null;
										}
										int num7 = TypeTable.GetSize(Type.Class, null) * 8;
										if (kindOfLiteral != KindOfLiteral.SignedInteger)
										{
											if (kindOfLiteral != KindOfLiteral.UnsignedInteger)
											{
												return null;
											}
											if (i == 0)
											{
												num = num4;
												goto IL_16CD;
											}
											num4 %= (ulong)num7;
											num = (num << (int)num4 | num >> num7 - (int)num4);
											if (num7 < 64)
											{
												num = num << 64 - num7 >> 64 - num7;
												goto IL_16CD;
											}
											goto IL_16CD;
										}
										else if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35400)
										{
											if (i == 0)
											{
												num = (ulong)num5;
												goto IL_16CD;
											}
											num4 = (ulong)num5;
											num4 %= (ulong)num7;
											num = (num << (int)num4 | num >> num7 - (int)num4);
											if (num7 < 64)
											{
												num = num << 64 - num7 >> 64 - num7;
											}
											unchecked
											{
												if (num7 <= 16)
												{
													if (num7 != 8)
													{
														if (num7 != 16)
														{
															goto IL_16CD;
														}
														num2 = (long)((short)num);
														goto IL_16CD;
													}
													else
													{
														if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35600)
														{
															num2 = (long)((sbyte)num);
															goto IL_16CD;
														}
														num2 = (long)((ulong)((ushort)num));
														goto IL_16CD;
													}
												}
												else
												{
													if (num7 == 32)
													{
														num2 = (long)((int)num);
														goto IL_16CD;
													}
													if (num7 != 64)
													{
														goto IL_16CD;
													}
													if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35600)
													{
														num2 = (long)num;
														goto IL_16CD;
													}
													goto IL_16CD;
												}
											}
										}
										else
										{
											if (i == 0)
											{
												num2 = num5;
												goto IL_16CD;
											}
											num5 %= unchecked((long)num7);
											num2 = (num2 << (int)num5 | num2 >> num7 - (int)num5);
											if (num7 < 64)
											{
												num2 = num2 << 64 - num7 >> 64 - num7;
												goto IL_16CD;
											}
											goto IL_16CD;
										}
										break;
									}
									case Operator.Ror:
									{
										if (Type == null)
										{
											return null;
										}
										int num8 = TypeTable.GetSize(Type.Class, null) * 8;
										if (kindOfLiteral != KindOfLiteral.SignedInteger)
										{
											if (kindOfLiteral != KindOfLiteral.UnsignedInteger)
											{
												return null;
											}
											if (i == 0)
											{
												num = num4;
												goto IL_16CD;
											}
											num4 %= (ulong)num8;
											num = (num >> (int)num4 | num << num8 - (int)num4);
											if (num8 < 64)
											{
												num = num << 64 - num8 >> 64 - num8;
												goto IL_16CD;
											}
											goto IL_16CD;
										}
										else if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35400)
										{
											if (i == 0)
											{
												num = (ulong)num5;
												goto IL_16CD;
											}
											num4 = (ulong)num5;
											num4 %= (ulong)num8;
											num = (num >> (int)num4 | num << num8 - (int)num4);
											if (num8 < 64)
											{
												num = num << 64 - num8 >> 64 - num8;
											}
											unchecked
											{
												if (num8 <= 16)
												{
													if (num8 != 8)
													{
														if (num8 != 16)
														{
															goto IL_16CD;
														}
														num2 = (long)((short)num);
														goto IL_16CD;
													}
													else
													{
														if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35600)
														{
															num2 = (long)((sbyte)num);
															goto IL_16CD;
														}
														num2 = (long)((ulong)((ushort)num));
														goto IL_16CD;
													}
												}
												else
												{
													if (num8 == 32)
													{
														num2 = (long)((int)num);
														goto IL_16CD;
													}
													if (num8 != 64)
													{
														goto IL_16CD;
													}
													if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35600)
													{
														num2 = (long)num;
														goto IL_16CD;
													}
													goto IL_16CD;
												}
											}
										}
										else
										{
											if (i == 0)
											{
												num2 = num5;
												goto IL_16CD;
											}
											num5 %= unchecked((long)num8);
											num2 = (num2 >> (int)num5 | num2 << num8 - (int)num5);
											if (num8 < 64)
											{
												num2 = num2 << 64 - num8 >> 64 - num8;
												goto IL_16CD;
											}
											goto IL_16CD;
										}
										break;
									}
									case Operator.Shl:
									{
										if (Type == null)
										{
											return null;
										}
										int num9 = TypeTable.GetSize(Type.Class, null) * 8;
										if (kindOfLiteral != KindOfLiteral.SignedInteger)
										{
											if (kindOfLiteral != KindOfLiteral.UnsignedInteger)
											{
												return null;
											}
											if (i == 0)
											{
												num = num4;
												goto IL_16CD;
											}
											num <<= (int)num4;
											if (num9 < 64)
											{
												num = num << 64 - num9 >> 64 - num9;
												goto IL_16CD;
											}
											goto IL_16CD;
										}
										else
										{
											if (i == 0)
											{
												num2 = num5;
												goto IL_16CD;
											}
											num2 <<= (int)num5;
											if (num9 < 64)
											{
												num2 = num2 << 64 - num9 >> 64 - num9;
												goto IL_16CD;
											}
											goto IL_16CD;
										}
										break;
									}
									case Operator.Shr:
									{
										if (Type == null)
										{
											return null;
										}
										int num10 = TypeTable.GetSize(Type.Class, null) * 8;
										if (kindOfLiteral != KindOfLiteral.SignedInteger)
										{
											if (kindOfLiteral != KindOfLiteral.UnsignedInteger)
											{
												return null;
											}
											if (i == 0)
											{
												num = num4;
												goto IL_16CD;
											}
											if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35600)
											{
												num >>= (int)num4;
											}
											else
											{
												num = (ulong)((int)num >> (int)num4);
											}
											if (num10 < 64)
											{
												num = num << 64 - num10 >> 64 - num10;
												goto IL_16CD;
											}
											goto IL_16CD;
										}
										else
										{
											if (i == 0)
											{
												num2 = num5;
												goto IL_16CD;
											}
											if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35600)
											{
												num2 >>= (int)num5;
											}
											else
											{
												num2 = unchecked((long)(checked((int)num2 >> (int)num5)));
											}
											if (num10 < 64)
											{
												num2 = num2 << 64 - num10 >> 64 - num10;
												goto IL_16CD;
											}
											goto IL_16CD;
										}
										break;
									}
									case Operator.Exp:
										switch (literalValue.KindOf)
										{
										case KindOfLiteral.SignedInteger:
											num3 = Math.Exp((double)num5);
											goto IL_16CD;
										case KindOfLiteral.UnsignedInteger:
											num3 = Math.Exp(num4);
											goto IL_16CD;
										case KindOfLiteral.Float:
											num3 = Math.Exp(num6);
											goto IL_16CD;
										default:
											return null;
										}
										break;
									case Operator.Expt:
										break;
									case Operator.Sqrt:
										switch (literalValue.KindOf)
										{
										case KindOfLiteral.SignedInteger:
											num3 = Math.Sqrt((double)num5);
											goto IL_16CD;
										case KindOfLiteral.UnsignedInteger:
											num3 = Math.Sqrt(num4);
											goto IL_16CD;
										case KindOfLiteral.Float:
											num3 = Math.Sqrt(num6);
											goto IL_16CD;
										default:
											return null;
										}
										break;
									case Operator.Ln:
										switch (literalValue.KindOf)
										{
										case KindOfLiteral.SignedInteger:
											num3 = Math.Log((double)num5);
											goto IL_16CD;
										case KindOfLiteral.UnsignedInteger:
											num3 = Math.Log(num4);
											goto IL_16CD;
										case KindOfLiteral.Float:
											num3 = Math.Log(num6);
											goto IL_16CD;
										default:
											return null;
										}
										break;
									case Operator.Log:
										switch (literalValue.KindOf)
										{
										case KindOfLiteral.SignedInteger:
											num3 = Math.Log10((double)num5);
											goto IL_16CD;
										case KindOfLiteral.UnsignedInteger:
											num3 = Math.Log10(num4);
											goto IL_16CD;
										case KindOfLiteral.Float:
											num3 = Math.Log10(num6);
											goto IL_16CD;
										default:
											return null;
										}
										break;
									case Operator.Sin:
										switch (literalValue.KindOf)
										{
										case KindOfLiteral.SignedInteger:
											num3 = Math.Sin((double)num5);
											goto IL_16CD;
										case KindOfLiteral.UnsignedInteger:
											num3 = Math.Sin(num4);
											goto IL_16CD;
										case KindOfLiteral.Float:
											num3 = Math.Sin(num6);
											goto IL_16CD;
										default:
											return null;
										}
										break;
									case Operator.Cos:
										switch (literalValue.KindOf)
										{
										case KindOfLiteral.SignedInteger:
											num3 = Math.Cos((double)num5);
											goto IL_16CD;
										case KindOfLiteral.UnsignedInteger:
											num3 = Math.Cos(num4);
											goto IL_16CD;
										case KindOfLiteral.Float:
											num3 = Math.Cos(num6);
											goto IL_16CD;
										default:
											return null;
										}
										break;
									case Operator.Tan:
										switch (literalValue.KindOf)
										{
										case KindOfLiteral.SignedInteger:
											num3 = Math.Tan((double)num5);
											goto IL_16CD;
										case KindOfLiteral.UnsignedInteger:
											num3 = Math.Tan(num4);
											goto IL_16CD;
										case KindOfLiteral.Float:
											num3 = Math.Tan(num6);
											goto IL_16CD;
										default:
											return null;
										}
										break;
									case Operator.ASin:
										switch (literalValue.KindOf)
										{
										case KindOfLiteral.SignedInteger:
											num3 = Math.Asin((double)num5);
											goto IL_16CD;
										case KindOfLiteral.UnsignedInteger:
											num3 = Math.Asin(num4);
											goto IL_16CD;
										case KindOfLiteral.Float:
											num3 = Math.Asin(num6);
											goto IL_16CD;
										default:
											return null;
										}
										break;
									case Operator.ACos:
										switch (literalValue.KindOf)
										{
										case KindOfLiteral.SignedInteger:
											num3 = Math.Acos((double)num5);
											goto IL_16CD;
										case KindOfLiteral.UnsignedInteger:
											num3 = Math.Acos(num4);
											goto IL_16CD;
										case KindOfLiteral.Float:
											num3 = Math.Acos(num6);
											goto IL_16CD;
										default:
											return null;
										}
										break;
									case Operator.ATan:
										switch (literalValue.KindOf)
										{
										case KindOfLiteral.SignedInteger:
											num3 = Math.Atan((double)num5);
											goto IL_16CD;
										case KindOfLiteral.UnsignedInteger:
											num3 = Math.Atan(num4);
											goto IL_16CD;
										case KindOfLiteral.Float:
											num3 = Math.Atan(num6);
											goto IL_16CD;
										default:
											return null;
										}
										break;
									default:
										switch (Code)
										{
										case Operator.Add:
										case Operator.Plus:
											switch (kindOfLiteral)
											{
											case KindOfLiteral.SignedInteger:
												num2 += num5;
												goto IL_16CD;
											case KindOfLiteral.UnsignedInteger:
												num += num4;
												goto IL_16CD;
											case KindOfLiteral.Float:
												unchecked
												{
													num3 += num6;
													goto IL_16CD;
												}
											default:
												return null;
											}
											break;
										case Operator.Sub:
										case Operator.Minus:
											switch (kindOfLiteral)
											{
											case KindOfLiteral.SignedInteger:
												if (i == 0)
												{
													num2 = num5;
													goto IL_16CD;
												}
												num2 -= num5;
												goto IL_16CD;
											case KindOfLiteral.UnsignedInteger:
												if (i == 0)
												{
													num = num4;
													goto IL_16CD;
												}
												num -= num4;
												goto IL_16CD;
											case KindOfLiteral.Float:
												if (i == 0)
												{
													num3 = num6;
													goto IL_16CD;
												}
												unchecked
												{
													num3 -= num6;
													goto IL_16CD;
												}
											default:
												return null;
											}
											break;
										case Operator.Mul:
										case Operator.Times:
											switch (kindOfLiteral)
											{
											case KindOfLiteral.SignedInteger:
												if (i == 0)
												{
													num2 = num5;
													goto IL_16CD;
												}
												num2 *= num5;
												goto IL_16CD;
											case KindOfLiteral.UnsignedInteger:
												if (i == 0)
												{
													num = num4;
													goto IL_16CD;
												}
												num *= num4;
												goto IL_16CD;
											case KindOfLiteral.Float:
												if (i == 0)
												{
													num3 = num6;
													goto IL_16CD;
												}
												unchecked
												{
													num3 *= num6;
													goto IL_16CD;
												}
											default:
												return null;
											}
											break;
										case Operator.Div:
										case Operator.Divide:
											switch (kindOfLiteral)
											{
											case KindOfLiteral.SignedInteger:
												if (i == 0)
												{
													num2 = num5;
													goto IL_16CD;
												}
												if (num5 == 0L)
												{
													return null;
												}
												num2 /= num5;
												goto IL_16CD;
											case KindOfLiteral.UnsignedInteger:
												if (i == 0)
												{
													num = num4;
													goto IL_16CD;
												}
												if (num4 == 0UL)
												{
													return null;
												}
												num /= num4;
												goto IL_16CD;
											case KindOfLiteral.Float:
												if (i == 0)
												{
													num3 = num6;
													goto IL_16CD;
												}
												if (num6 == 0.0)
												{
													return null;
												}
												num3 /= num6;
												goto IL_16CD;
											default:
												return null;
											}
											break;
										case Operator.Mod:
											switch (kindOfLiteral)
											{
											case KindOfLiteral.SignedInteger:
												if (i == 0)
												{
													num2 = num5;
													goto IL_16CD;
												}
												if (num5 == 0L)
												{
													num2 = 0L;
													goto IL_16CD;
												}
												num2 %= num5;
												goto IL_16CD;
											case KindOfLiteral.UnsignedInteger:
												if (i == 0)
												{
													num = num4;
													goto IL_16CD;
												}
												if (num4 == 0UL)
												{
													num = 0UL;
													goto IL_16CD;
												}
												num %= num4;
												goto IL_16CD;
											case KindOfLiteral.Float:
												if (i == 0)
												{
													num3 = num6;
													goto IL_16CD;
												}
												if (num6 == 0.0)
												{
													num3 = 0.0;
													goto IL_16CD;
												}
												num3 %= num6;
												goto IL_16CD;
											default:
												return null;
											}
											break;
										case Operator.And:
										case Operator.AndN:
											switch (kindOfLiteral)
											{
											case KindOfLiteral.SignedInteger:
												if (i == 0)
												{
													num2 = num5;
													goto IL_16CD;
												}
												num2 &= num5;
												goto IL_16CD;
											case KindOfLiteral.UnsignedInteger:
												if (i == 0)
												{
													num = num4;
													goto IL_16CD;
												}
												num &= num4;
												goto IL_16CD;
											case KindOfLiteral.Bool:
												if (i == 0)
												{
													flag = flag2;
													goto IL_16CD;
												}
												flag = (flag && flag2);
												goto IL_16CD;
											}
											return null;
										case Operator.Or:
										case Operator.OrN:
											switch (kindOfLiteral)
											{
											case KindOfLiteral.SignedInteger:
												if (i == 0)
												{
													num2 = num5;
													goto IL_16CD;
												}
												num2 |= num5;
												goto IL_16CD;
											case KindOfLiteral.UnsignedInteger:
												if (i == 0)
												{
													num = num4;
													goto IL_16CD;
												}
												num |= num4;
												goto IL_16CD;
											case KindOfLiteral.Bool:
												if (i == 0)
												{
													flag = flag2;
													goto IL_16CD;
												}
												flag = (flag || flag2);
												goto IL_16CD;
											}
											return null;
										case Operator.Xor:
										case Operator.XorN:
											switch (kindOfLiteral)
											{
											case KindOfLiteral.SignedInteger:
												if (i == 0)
												{
													num2 = num5;
													goto IL_16CD;
												}
												num2 = ((num2 & ~num5) | (~num2 & num5));
												goto IL_16CD;
											case KindOfLiteral.UnsignedInteger:
												if (i == 0)
												{
													num = num4;
													goto IL_16CD;
												}
												num = ((num & ~num4) | (~num & num4));
												goto IL_16CD;
											case KindOfLiteral.Bool:
												if (i == 0)
												{
													flag = flag2;
													goto IL_16CD;
												}
												flag = ((flag && !flag2) || (!flag && flag2));
												goto IL_16CD;
											}
											return null;
										case Operator.Not:
										{
											if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351100 && Type == null)
											{
												return null;
											}
											int num11 = TypeTable.GetSize(Type.Class, null) * 8;
											switch (kindOfLiteral)
											{
											case KindOfLiteral.SignedInteger:
												num2 = ~num5;
												if (num11 < 64)
												{
													num2 = num2 << 64 - num11 >> 64 - num11;
													goto IL_16CD;
												}
												goto IL_16CD;
											case KindOfLiteral.UnsignedInteger:
												num = ~num4;
												if (num11 < 64)
												{
													num = num << 64 - num11 >> 64 - num11;
													goto IL_16CD;
												}
												goto IL_16CD;
											case KindOfLiteral.Bool:
												flag = !flag2;
												goto IL_16CD;
											}
											return null;
										}
										case Operator.Eq:
											break;
										case Operator.Ne:
											goto IL_1110;
										case Operator.Ge:
											goto IL_F3A;
										case Operator.Gt:
											goto IL_FB4;
										case Operator.Le:
											goto IL_1025;
										case Operator.Lt:
											goto IL_109F;
										case Operator.Cal:
										case Operator.CalC:
										case Operator.CalCN:
										case Operator.Jmp:
										case Operator.JmpC:
										case Operator.JmpCN:
										case Operator.Ret:
										case Operator.RetC:
										case Operator.RetCN:
										case Operator.Ld:
										case Operator.LdN:
										case Operator.St:
										case Operator.StN:
										case Operator.Move:
										case Operator.TestAndSet:
										case Operator.R:
										case Operator.S:
											goto IL_16C8;
										case Operator.Power:
											goto IL_B69;
										default:
											switch (Code)
											{
											case Operator.Less:
												goto IL_109F;
											case Operator.Greater:
												goto IL_FB4;
											case Operator.LessEqual:
												goto IL_1025;
											case Operator.GreaterEqual:
												goto IL_F3A;
											case Operator.Equal:
												break;
											case Operator.NotEqual:
												goto IL_1110;
											default:
												goto IL_16C8;
											}
											break;
										}
										switch (literalValue.KindOf)
										{
										case KindOfLiteral.SignedInteger:
											if (i == 0)
											{
												num3 = (double)num5;
												goto IL_16CD;
											}
											flag = (num3 == (double)num5);
											goto IL_16CD;
										case KindOfLiteral.UnsignedInteger:
											if (i == 0)
											{
												num3 = num4;
												goto IL_16CD;
											}
											flag = (num3 == num4);
											goto IL_16CD;
										case KindOfLiteral.Float:
											if (i == 0)
											{
												num3 = num6;
												goto IL_16CD;
											}
											flag = (num3 == num6);
											goto IL_16CD;
										case KindOfLiteral.String:
											if (!APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV33200)
											{
												return null;
											}
											if (i == 0)
											{
												text = text2;
												goto IL_16CD;
											}
											flag = (text != null && text2 != null && text == text2);
											goto IL_16CD;
										case KindOfLiteral.Bool:
										{
											if (!APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV33200)
											{
												return null;
											}
											double num12 = flag2 ? 1.0 : 0.0;
											if (i == 0)
											{
												num3 = num12;
												goto IL_16CD;
											}
											flag = (num3 == num12);
											goto IL_16CD;
										}
										default:
											return null;
										}
										IL_F3A:
										switch (literalValue.KindOf)
										{
										case KindOfLiteral.SignedInteger:
											if (i == 0)
											{
												num3 = (double)num5;
												goto IL_16CD;
											}
											flag = (num3 >= (double)num5);
											goto IL_16CD;
										case KindOfLiteral.UnsignedInteger:
											if (i == 0)
											{
												num3 = num4;
												goto IL_16CD;
											}
											flag = (num3 >= num4);
											goto IL_16CD;
										case KindOfLiteral.Float:
											if (i == 0)
											{
												num3 = num6;
												goto IL_16CD;
											}
											flag = (num3 >= num6);
											goto IL_16CD;
										default:
											return null;
										}
										IL_FB4:
										switch (literalValue.KindOf)
										{
										case KindOfLiteral.SignedInteger:
											if (i == 0)
											{
												num3 = (double)num5;
												goto IL_16CD;
											}
											flag = (num3 > (double)num5);
											goto IL_16CD;
										case KindOfLiteral.UnsignedInteger:
											if (i == 0)
											{
												num3 = num4;
												goto IL_16CD;
											}
											flag = (num3 > num4);
											goto IL_16CD;
										case KindOfLiteral.Float:
											if (i == 0)
											{
												num3 = num6;
												goto IL_16CD;
											}
											flag = (num3 > num6);
											goto IL_16CD;
										default:
											return null;
										}
										IL_1025:
										switch (literalValue.KindOf)
										{
										case KindOfLiteral.SignedInteger:
											if (i == 0)
											{
												num3 = (double)num5;
												goto IL_16CD;
											}
											flag = (num3 <= (double)num5);
											goto IL_16CD;
										case KindOfLiteral.UnsignedInteger:
											if (i == 0)
											{
												num3 = num4;
												goto IL_16CD;
											}
											flag = (num3 <= num4);
											goto IL_16CD;
										case KindOfLiteral.Float:
											if (i == 0)
											{
												num3 = num6;
												goto IL_16CD;
											}
											flag = (num3 <= num6);
											goto IL_16CD;
										default:
											return null;
										}
										IL_109F:
										switch (literalValue.KindOf)
										{
										case KindOfLiteral.SignedInteger:
											if (i == 0)
											{
												num3 = (double)num5;
												goto IL_16CD;
											}
											flag = (num3 < (double)num5);
											goto IL_16CD;
										case KindOfLiteral.UnsignedInteger:
											if (i == 0)
											{
												num3 = num4;
												goto IL_16CD;
											}
											flag = (num3 < num4);
											goto IL_16CD;
										case KindOfLiteral.Float:
											if (i == 0)
											{
												num3 = num6;
												goto IL_16CD;
											}
											flag = (num3 < num6);
											goto IL_16CD;
										default:
											return null;
										}
										IL_1110:
										switch (literalValue.KindOf)
										{
										case KindOfLiteral.SignedInteger:
											if (i == 0)
											{
												num3 = (double)num5;
												goto IL_16CD;
											}
											flag = (num3 != (double)num5);
											goto IL_16CD;
										case KindOfLiteral.UnsignedInteger:
											if (i == 0)
											{
												num3 = num4;
												goto IL_16CD;
											}
											flag = (num3 != num4);
											goto IL_16CD;
										case KindOfLiteral.Float:
											if (i == 0)
											{
												num3 = num6;
												goto IL_16CD;
											}
											flag = (num3 != num6);
											goto IL_16CD;
										case KindOfLiteral.String:
											if (!APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV33200)
											{
												return null;
											}
											if (i == 0)
											{
												text = text2;
												goto IL_16CD;
											}
											flag = (text != null && text2 != null && text != text2);
											goto IL_16CD;
										case KindOfLiteral.Bool:
										{
											if (!APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV33200)
											{
												return null;
											}
											double num13 = flag2 ? 1.0 : 0.0;
											if (i == 0)
											{
												num3 = num13;
												goto IL_16CD;
											}
											flag = (num3 != num13);
											goto IL_16CD;
										}
										default:
											return null;
										}
										break;
									}
									IL_B69:
									switch (literalValue.KindOf)
									{
									case KindOfLiteral.SignedInteger:
										if (i == 0)
										{
											num3 = (double)num5;
										}
										else
										{
											num3 = Math.Pow(num3, (double)num5);
										}
										break;
									case KindOfLiteral.UnsignedInteger:
										if (i == 0)
										{
											num3 = num4;
										}
										else
										{
											num3 = Math.Pow(num3, num4);
										}
										break;
									case KindOfLiteral.Float:
										if (i == 0)
										{
											num3 = num6;
										}
										else
										{
											num3 = Math.Pow(num3, num6);
										}
										break;
									default:
										return null;
									}
								}
								else if (Code <= Operator.And_Then)
								{
									if (Code != Operator.TruncInt)
									{
										if (Code != Operator.And_Then)
										{
											goto IL_16C8;
										}
										if (kindOfLiteral != KindOfLiteral.Bool)
										{
											return null;
										}
										if (i == 0)
										{
											flag = flag2;
										}
										else
										{
											flag = (flag && flag2);
										}
									}
									else
									{
										num2 = unchecked((long)(checked((short)num6)));
									}
								}
								else if (Code != Operator.Or_Else)
								{
									if (unchecked(Code - Operator.__XAdd) > 1)
									{
										goto IL_16C8;
									}
									return null;
								}
								else
								{
									if (kindOfLiteral != KindOfLiteral.Bool)
									{
										return null;
									}
									if (i == 0)
									{
										flag = flag2;
									}
									else
									{
										flag = (flag || flag2);
									}
								}
								IL_16CD:
								i++;
								continue;
								IL_16C8:
								return null;
							}
							LiteralValue empty = LiteralValue.Empty;
							switch (kindOfLiteral)
							{
							case KindOfLiteral.SignedInteger:
								empty = new LiteralValue(num2);
								break;
							case KindOfLiteral.UnsignedInteger:
								empty = new LiteralValue(num);
								break;
							case KindOfLiteral.Float:
								empty = new LiteralValue(num3);
								break;
							case KindOfLiteral.Bool:
								empty = new LiteralValue(flag);
								break;
							}
							result = empty;
						}
						catch
						{
							result = null;
						}
						return result;
					}
					else
					{
						bool flag3;
						bool boolV = litvalOps[0].GetBoolV(out flag3);
						if (!flag3 || litvalOps.Length != 3)
						{
							return null;
						}
						if (boolV)
						{
							return litvalOps[2];
						}
						return litvalOps[1];
					}
				}
			}
			else
			{
				bool flag4;
				int num14 = litvalOps[0].GetInt(out flag4);
				if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV32120)
				{
					num14++;
				}
				if (!flag4 || num14 < 0 || num14 >= litvalOps.Length)
				{
					return null;
				}
				return litvalOps[num14];
			}
		}
	}
}
