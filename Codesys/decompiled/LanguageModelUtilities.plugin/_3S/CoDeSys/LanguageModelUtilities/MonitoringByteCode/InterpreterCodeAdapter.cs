using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelUtilities.MonitoringByteCode
{
	internal class InterpreterCodeAdapter
	{
		internal bool IsLogicOperator(Operator code)
		{
			return ByteProgramCreator.IsLogicOperator(code);
		}

		internal bool IsArithmeticOperator(Operator code)
		{
			return ByteProgramCreator.IsArithmeticOperator(code);
		}

		internal bool IsAddressOperator(Operator code)
		{
			if (code == Operator.Adr || code == Operator.DeRef)
			{
				return true;
			}
			return false;
		}

		internal bool NeedsExternalFunctionCall(IConversionExpression conv, out string stFunctionName, out TypeClass tcWithType)
		{
			TypeClass from = conv.From;
			TypeClass to = conv.To;
			stFunctionName = string.Empty;
			tcWithType = TypeClass.None;
			switch (from)
			{
			case TypeClass.String:
				switch (to)
				{
				case TypeClass.Real:
					stFunctionName = CGConstants.string_to_real32;
					return true;
				case TypeClass.LReal:
					stFunctionName = CGConstants.string_to_real64;
					return true;
				case TypeClass.WString:
					stFunctionName = CGConstants.string_to_wstring;
					return true;
				case TypeClass.LWord:
				case TypeClass.LInt:
				case TypeClass.ULInt:
				case TypeClass.LTime:
					tcWithType = to;
					stFunctionName = CGConstants.string_to_any64;
					return true;
				default:
					tcWithType = to;
					stFunctionName = CGConstants.string_to_any32;
					return true;
				}
			case TypeClass.WString:
				switch (to)
				{
				case TypeClass.Real:
					stFunctionName = CGConstants.wstring_to_real32;
					return true;
				case TypeClass.LReal:
					stFunctionName = CGConstants.wstring_to_real64;
					return true;
				case TypeClass.String:
					stFunctionName = CGConstants.wstring_to_string;
					return true;
				case TypeClass.LWord:
				case TypeClass.LInt:
				case TypeClass.ULInt:
				case TypeClass.LTime:
					tcWithType = to;
					stFunctionName = CGConstants.wstring_to_any64;
					return true;
				default:
					tcWithType = to;
					stFunctionName = CGConstants.wstring_to_any32;
					return true;
				}
			default:
				switch (to)
				{
				case TypeClass.String:
					switch (from)
					{
					case TypeClass.Real:
						tcWithType = from;
						stFunctionName = CGConstants.new_real32_to_string;
						return true;
					case TypeClass.LReal:
						tcWithType = from;
						stFunctionName = CGConstants.real64_to_string;
						return true;
					case TypeClass.LWord:
					case TypeClass.LInt:
					case TypeClass.ULInt:
					case TypeClass.LTime:
						tcWithType = from;
						stFunctionName = CGConstants.any64_to_string;
						return true;
					default:
						tcWithType = from;
						stFunctionName = CGConstants.any32_to_string;
						return true;
					}
				case TypeClass.WString:
					switch (from)
					{
					case TypeClass.Real:
						tcWithType = from;
						stFunctionName = CGConstants.new_real32_to_wstring;
						return true;
					case TypeClass.LReal:
						tcWithType = from;
						stFunctionName = CGConstants.real64_to_wstring;
						return true;
					case TypeClass.LWord:
					case TypeClass.LInt:
					case TypeClass.ULInt:
					case TypeClass.LTime:
						tcWithType = from;
						stFunctionName = CGConstants.any64_to_wstring;
						return true;
					default:
						tcWithType = from;
						stFunctionName = CGConstants.any32_to_wstring;
						return true;
					}
				default:
					switch (from)
					{
					case TypeClass.LReal:
						switch (to)
						{
						case TypeClass.LWord:
						case TypeClass.LInt:
						case TypeClass.ULInt:
						case TypeClass.LTime:
							if (to == TypeClass.LTime)
							{
								tcWithType = TypeClass.LWord;
							}
							else
							{
								tcWithType = to;
							}
							stFunctionName = CGConstants.real64_to_any64;
							return true;
						case TypeClass.Real:
							return false;
						default:
							tcWithType = to;
							stFunctionName = CGConstants.real64_to_any32;
							return true;
						}
					case TypeClass.Real:
						switch (to)
						{
						case TypeClass.LWord:
						case TypeClass.LInt:
						case TypeClass.ULInt:
						case TypeClass.LTime:
							if (to == TypeClass.LTime)
							{
								tcWithType = TypeClass.LWord;
							}
							else
							{
								tcWithType = to;
							}
							stFunctionName = CGConstants.real32_to_any64;
							return true;
						case TypeClass.LReal:
							return false;
						default:
							tcWithType = to;
							stFunctionName = CGConstants.real32_to_any32;
							return true;
						}
					default:
						switch (to)
						{
						case TypeClass.LReal:
							switch (from)
							{
							case TypeClass.LWord:
							case TypeClass.LInt:
							case TypeClass.ULInt:
							case TypeClass.LTime:
								if (from == TypeClass.LTime)
								{
									tcWithType = TypeClass.LWord;
								}
								else
								{
									tcWithType = from;
								}
								stFunctionName = CGConstants.any64_to_real64;
								return true;
							case TypeClass.Int:
							case TypeClass.Real:
								return false;
							default:
								tcWithType = from;
								stFunctionName = CGConstants.any32_to_real64;
								return true;
							}
						case TypeClass.Real:
							switch (from)
							{
							case TypeClass.LWord:
							case TypeClass.LInt:
							case TypeClass.ULInt:
							case TypeClass.LTime:
								if (from == TypeClass.LTime)
								{
									tcWithType = TypeClass.LWord;
								}
								else
								{
									tcWithType = from;
								}
								stFunctionName = CGConstants.any64_to_real32;
								return true;
							case TypeClass.Int:
							case TypeClass.LReal:
								return false;
							default:
								tcWithType = from;
								stFunctionName = CGConstants.any32_to_real32;
								return true;
							}
						default:
							switch (from)
							{
							case TypeClass.LWord:
							case TypeClass.LInt:
							case TypeClass.ULInt:
							case TypeClass.LTime:
								if (to == TypeClass.TimeOfDay)
								{
									tcWithType = to;
									stFunctionName = CGConstants.int64_to_any32;
									return true;
								}
								break;
							}
							return false;
						}
					}
				}
			}
		}
	}
}
