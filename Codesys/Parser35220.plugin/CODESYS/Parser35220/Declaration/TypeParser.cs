using System;
using System.Collections.Generic;
using System.Linq;
using CODESYS.Parser;
using CODESYS.Parser35220.Expressions;
using CODESYS.Parser35220.Resources;
using CODESYS.Parser35220.Utilities;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace CODESYS.Parser35220.Declaration
{
	// Token: 0x0200005C RID: 92
	internal class TypeParser
	{
		// Token: 0x060005BC RID: 1468 RVA: 0x00018266 File Offset: 0x00016466
		internal TypeParser(ParserContext context)
		{
			this.Context = context;
		}

		// Token: 0x17000165 RID: 357
		// (get) Token: 0x060005BD RID: 1469 RVA: 0x00018275 File Offset: 0x00016475
		private ParserContext Context { get; }

		// Token: 0x17000166 RID: 358
		// (get) Token: 0x060005BE RID: 1470 RVA: 0x0001827D File Offset: 0x0001647D
		private ExpressionParser ExpressionParser
		{
			get
			{
				return this.Context.ExpressionParser;
			}
		}

		// Token: 0x17000167 RID: 359
		// (get) Token: 0x060005BF RID: 1471 RVA: 0x0001828A File Offset: 0x0001648A
		private _ILanguageModelBuilder6 LMItemFactory
		{
			get
			{
				return this.Context.LMItemFactory;
			}
		}

		// Token: 0x17000168 RID: 360
		// (get) Token: 0x060005C0 RID: 1472 RVA: 0x00018297 File Offset: 0x00016497
		private ITypeTable3 TypeTable
		{
			get
			{
				return this.Context.TypeTable;
			}
		}

		// Token: 0x17000169 RID: 361
		// (get) Token: 0x060005C1 RID: 1473 RVA: 0x000182A4 File Offset: 0x000164A4
		private ITokenFactory TokenFactory
		{
			get
			{
				return this.Context.TokenFactory;
			}
		}

		// Token: 0x1700016A RID: 362
		// (get) Token: 0x060005C2 RID: 1474 RVA: 0x000182B1 File Offset: 0x000164B1
		private IScanner9 Scanner
		{
			get
			{
				return this.Context.Scanner;
			}
		}

		// Token: 0x060005C3 RID: 1475 RVA: 0x000182BE File Offset: 0x000164BE
		private TokenType Next(out IToken token)
		{
			return this.Scanner.Next(out token);
		}

		// Token: 0x060005C4 RID: 1476 RVA: 0x000182CC File Offset: 0x000164CC
		private void Next(out IToken token, bool bWithPragma, bool bWithComment)
		{
			this.Scanner.Next(out token, bWithPragma, bWithComment);
		}

		// Token: 0x060005C5 RID: 1477 RVA: 0x000182DD File Offset: 0x000164DD
		private void ParseReSyncIF()
		{
			this.Scanner.ParseReSyncIF();
		}

		// Token: 0x1700016B RID: 363
		// (get) Token: 0x060005C6 RID: 1478 RVA: 0x000182EA File Offset: 0x000164EA
		private IErrorHandler ErrorHandler
		{
			get
			{
				return this.Context.ErrorHandler;
			}
		}

		// Token: 0x060005C7 RID: 1479 RVA: 0x000182F7 File Offset: 0x000164F7
		private Operator MatchOperator(params Operator[] ops)
		{
			return this.Scanner.MatchOperator(this.ErrorHandler, null, true, ops);
		}

		// Token: 0x060005C8 RID: 1480 RVA: 0x0001830D File Offset: 0x0001650D
		private Operator MatchOperator(bool bGenerateError, params Operator[] ops)
		{
			return this.Scanner.MatchOperator(this.ErrorHandler, null, bGenerateError, ops);
		}

		// Token: 0x060005C9 RID: 1481 RVA: 0x00018323 File Offset: 0x00016523
		private void AddErrorIF(IToken tokenPos, MessageId ErrorId, params object[] args)
		{
			this.ErrorHandler.AddError(tokenPos, ErrorId, args);
		}

		// Token: 0x060005CA RID: 1482 RVA: 0x00018334 File Offset: 0x00016534
		private _IType ParseSubrangeType(_IType iectypebase)
		{
			IToken token;
			this.Next(out token, true, true);
			if (token.Type == 15 && this.Scanner.GetOperator(token) == 167)
			{
				bool flag;
				_IExpression iexpression = this.ExpressionParser.ParseAssignment(out flag);
				this.MatchOperator(new Operator[]
				{
					173
				});
				_IExpression iexpression2 = this.ExpressionParser.ParseAssignment(out flag);
				this.MatchOperator(new Operator[]
				{
					168
				});
				_ISubrangeType isubrangeType = this.LMItemFactory.CreateSubrangeType(iexpression, iexpression2);
				isubrangeType._Base = iectypebase;
				return isubrangeType;
			}
			this.Scanner.SetPosition(token);
			return iectypebase;
		}

		// Token: 0x060005CB RID: 1483 RVA: 0x000183D0 File Offset: 0x000165D0
		public _IType ParseType()
		{
			return this.ParseType(true, false, this.TokenFactory.Empty());
		}

		// Token: 0x060005CC RID: 1484 RVA: 0x000183E5 File Offset: 0x000165E5
		public _IType TryParseType()
		{
			return this.ParseType(true, true, this.TokenFactory.Empty());
		}

		// Token: 0x060005CD RID: 1485 RVA: 0x000183FA File Offset: 0x000165FA
		private _IType ParseType(bool bTop, bool bTry)
		{
			return this.ParseType(bTop, bTry, this.TokenFactory.Empty());
		}

		// Token: 0x060005CE RID: 1486 RVA: 0x00018410 File Offset: 0x00016610
		public _IType ParseType(bool bTop, bool bTry, IToken tokenError)
		{
			IToken token;
			TokenType tokenType = this.Next(out token);
			if (tokenType == 13)
			{
				this.Scanner.SetPosition(token);
				_IType itype = this.ParseUserdefType();
				return itype ?? this.HandleTryCase(bTry, token);
			}
			if (tokenType != 15)
			{
				return this.HandleTryCase(bTry, token);
			}
			Operator @operator = this.Scanner.GetOperator(token);
			_IType itype2 = this.HandleOperatorCase(@operator, bTry, bTop, token, tokenError);
			if (itype2 != null)
			{
				return itype2;
			}
			return null;
		}

		// Token: 0x060005CF RID: 1487 RVA: 0x0001847C File Offset: 0x0001667C
		private _IType HandleOperatorCase(Operator opGlobal, bool bTry, bool bTop, IToken token, IToken tokenError)
		{
			HashSet<Operator> hashSet = new HashSet<Operator>();
			hashSet.Add(232);
			hashSet.Add(231);
			hashSet.Add(237);
			hashSet.Add(3);
			hashSet.Add(11);
			hashSet.Add(12);
			hashSet.Add(16);
			hashSet.Add(20);
			hashSet.Add(17);
			hashSet.Add(21);
			hashSet.Add(13);
			hashSet.Add(18);
			hashSet.Add(22);
			hashSet.Add(14);
			hashSet.Add(19);
			hashSet.Add(23);
			hashSet.Add(15);
			hashSet.Add(24);
			hashSet.Add(25);
			hashSet.Add(28);
			hashSet.Add(29);
			hashSet.Add(30);
			hashSet.Add(274);
			hashSet.Add(31);
			hashSet.Add(275);
			hashSet.Add(32);
			hashSet.Add(276);
			hashSet.Add(10);
			hashSet.Add(206);
			hashSet.Add(207);
			hashSet.Add(209);
			hashSet.Add(208);
			hashSet.Add(210);
			hashSet.Add(212);
			hashSet.Add(211);
			hashSet.Add(213);
			hashSet.Add(216);
			hashSet.Add(214);
			hashSet.Add(217);
			hashSet.Add(219);
			hashSet.Add(218);
			hashSet.Add(215);
			hashSet.Add(272);
			hashSet.Add(273);
			HashSet<Operator> hashSet2 = new HashSet<Operator>
			{
				4,
				5,
				6,
				7,
				8,
				9,
				250
			};
			if (!hashSet.Contains(opGlobal) && !hashSet2.Contains(opGlobal))
			{
				if (opGlobal <= 99)
				{
					if (opGlobal <= 61)
					{
						if (opGlobal == 26)
						{
							return this.ParseStringType();
						}
						if (opGlobal == 27)
						{
							return this.ParseWStringType();
						}
						if (opGlobal == 61)
						{
							return this.ParseArrayType(bTop, token, tokenError);
						}
					}
					else
					{
						if (opGlobal == 62)
						{
							if (!bTop)
							{
								this.AddErrorIF(token, 9, new object[]
								{
									this.Scanner.GetOperatorText(62)
								});
							}
							this.AddErrorIF(token, 9, new object[]
							{
								this.Scanner.GetOperatorText(62)
							});
							this.MatchOperator(new Operator[]
							{
								167
							});
							bool flag;
							_IExpression iexpression = this.ExpressionParser.ParseAssignment(out flag);
							this.MatchOperator(new Operator[]
							{
								168
							});
							this.MatchOperator(new Operator[]
							{
								90
							});
							_IType itype = this.ParseType(false, false);
							return this.LMItemFactory.CreateParamsType(itype, iexpression);
						}
						if (opGlobal == 92)
						{
							return this.ParsePointerType(token, tokenError);
						}
						if (opGlobal != 99)
						{
						}
					}
				}
				else
				{
					if (opGlobal <= 192)
					{
						if (opGlobal == 167)
						{
							_IEnumDeclarationListStatement ienumDeclarationListStatement = this.ParseEnumList(token, null);
							return this.LMItemFactory.CreateImplicitEnumerationType(ienumDeclarationListStatement, "__IMPLICIT__ENUM");
						}
						if (opGlobal == 186)
						{
							return this.ParseReferenceType(bTop, token, tokenError);
						}
						if (opGlobal != 192)
						{
							goto IL_473;
						}
					}
					else
					{
						if (opGlobal == 243)
						{
							return this.ParseXStringType();
						}
						if (opGlobal != 251)
						{
							if (opGlobal != 257)
							{
								goto IL_473;
							}
							return this.ParseVectorType();
						}
					}
					IToken token2;
					if (this.Next(out token2) == 15 && this.Scanner.GetOperator(token2) == 162)
					{
						_IExpression iexpression2 = this.ExpressionParser.ParseQualifiedNameExpression(null);
						_IExpression iexpression4;
						if (opGlobal != 251)
						{
							_IExpression iexpression3 = this.LMItemFactory.CreateSystemScopeExpression(iexpression2, token);
							iexpression4 = iexpression3;
						}
						else
						{
							_IExpression iexpression3 = this.LMItemFactory.CreatePoolScopeExpression(iexpression2, token);
							iexpression4 = iexpression3;
						}
						_IExpression iexpression5 = iexpression4;
						return this.LMItemFactory.CreateUserdefType(iexpression5);
					}
				}
				IL_473:
				return this.HandleTryCase(bTry, token);
			}
			_IType itype2 = this.TypeTable.Get(opGlobal);
			if (!this.TypeTable.IsInteger(itype2.Class))
			{
				return itype2;
			}
			if (hashSet2.Contains(opGlobal) && !bTop)
			{
				this.AddErrorIF(token, 311, new object[]
				{
					itype2.ToString()
				});
			}
			return this.ParseSubrangeType(itype2);
		}

		// Token: 0x060005D0 RID: 1488 RVA: 0x00018905 File Offset: 0x00016B05
		private _IType HandleTryCase(bool bTry, IToken token)
		{
			if (!bTry)
			{
				this.AddErrorIF(token, 31, new object[]
				{
					this.Scanner.GetTokenText(token)
				});
				this.ParseReSyncIF();
			}
			return null;
		}

		// Token: 0x060005D1 RID: 1489 RVA: 0x00018930 File Offset: 0x00016B30
		public _IType ParseUserdefType()
		{
			_IExpression iexpression = this.ExpressionParser.ParseQualifiedNameExpression(null);
			if (iexpression == null)
			{
				return null;
			}
			if (!this.Scanner.CheckOptionalOperator(175))
			{
				return this.LMItemFactory.CreateUserdefType(iexpression);
			}
			return this.ParseGenericUserdefType(iexpression);
		}

		// Token: 0x060005D2 RID: 1490 RVA: 0x00018978 File Offset: 0x00016B78
		private _IType ParseGenericUserdefType(_IExpression qne)
		{
			IGenericUserdefType genericUserdefType = this.LMItemFactory.CreateGenericUserdefType(qne);
			Operator @operator;
			do
			{
				_IExpression iexpression;
				if (this.Scanner.CheckOptionalOperator(167))
				{
					iexpression = (this.ExpressionParser.ParseExpression() as _IExpression);
					this.MatchOperator(new Operator[]
					{
						168
					});
				}
				else
				{
					iexpression = (this.ExpressionParser.ParseOperand() as _IExpression);
				}
				if (iexpression != null)
				{
					genericUserdefType.AddGenericConstantInitialization(iexpression);
				}
				@operator = this.MatchOperator(new Operator[]
				{
					171,
					176
				});
			}
			while (@operator == 171);
			if (this.Context._bReportSP18Feature)
			{
				_IExpression iexpression2 = genericUserdefType.GenericConstantsInitializations.FirstOrDefault<_IExpression>();
				if (iexpression2 != null)
				{
					this.Context.AddUnsupportedFeatureError(iexpression2, Strings.CompilerFeature_GenericConstantVariable, ParserContext.CompilerVersion18);
				}
			}
			return genericUserdefType;
		}

		// Token: 0x060005D3 RID: 1491 RVA: 0x00018A40 File Offset: 0x00016C40
		private _IType ParseVectorType()
		{
			this.MatchOperator(new Operator[]
			{
				169
			});
			bool flag;
			_IExpression iexpression = this.ExpressionParser.ParseAssignment(out flag);
			if (flag)
			{
				this.AddErrorIF(this.Scanner.CurrentToken, 500, Array.Empty<object>());
				iexpression = null;
			}
			this.MatchOperator(new Operator[]
			{
				170
			});
			this.MatchOperator(new Operator[]
			{
				90
			});
			_IType itype = this.ParseType(false, false);
			if (itype == null || iexpression == null)
			{
				return null;
			}
			return this.LMItemFactory.CreateVectorType(itype, iexpression);
		}

		// Token: 0x060005D4 RID: 1492 RVA: 0x00018AD8 File Offset: 0x00016CD8
		private _IType ParseXStringType()
		{
			IToken token;
			this.Next(out token);
			_IXStringType ixstringType = this.LMItemFactory.CreateXStringtype();
			this.Scanner.SetPosition(token);
			if (this.MatchOperator(false, new Operator[]
			{
				167
			}) == null)
			{
				this.Scanner.SetPosition(token);
				return ixstringType;
			}
			bool flag;
			_IExpression length = this.ExpressionParser.ParseAssignment(out flag);
			if (flag)
			{
				this.AddErrorIF(token, 27, Array.Empty<object>());
				return ixstringType;
			}
			ixstringType.Length = length;
			this.MatchOperator(new Operator[]
			{
				168
			});
			return ixstringType;
		}

		// Token: 0x060005D5 RID: 1493 RVA: 0x00018B6C File Offset: 0x00016D6C
		private _IType ParseWStringType()
		{
			IToken token;
			this.Next(out token);
			_IWStringType iwstringType = this.LMItemFactory.CreateWStringType();
			this.Scanner.SetPosition(token);
			if (this.MatchOperator(false, new Operator[]
			{
				167
			}) == null)
			{
				this.Scanner.SetPosition(token);
				return iwstringType;
			}
			bool flag;
			_IExpression length = this.ExpressionParser.ParseAssignment(out flag);
			if (flag)
			{
				this.AddErrorIF(token, 27, Array.Empty<object>());
				return iwstringType;
			}
			iwstringType.Length = length;
			this.MatchOperator(new Operator[]
			{
				168
			});
			return iwstringType;
		}

		// Token: 0x060005D6 RID: 1494 RVA: 0x00018C00 File Offset: 0x00016E00
		private _IType ParseStringType()
		{
			IToken token;
			this.Next(out token, false, true);
			_IStringType istringType = this.LMItemFactory.CreateStringType();
			this.Scanner.SetPosition(token);
			if (this.MatchOperator(false, new Operator[]
			{
				167,
				169
			}) == null)
			{
				this.Scanner.SetPosition(token);
				return istringType;
			}
			bool flag;
			_IExpression length = this.ExpressionParser.ParseAssignment(out flag);
			if (flag)
			{
				this.AddErrorIF(token, 27, Array.Empty<object>());
				return istringType;
			}
			istringType.Length = length;
			this.MatchOperator(new Operator[]
			{
				168,
				170
			});
			return istringType;
		}

		// Token: 0x060005D7 RID: 1495 RVA: 0x00018CA4 File Offset: 0x00016EA4
		private _IType ParseArrayType(bool bTop, IToken token, IToken tokenError)
		{
			this.MatchOperator(new Operator[]
			{
				169
			});
			IToken token2;
			if (this.Scanner.GetNext(ref token2) == 15 && this.Scanner.GetOperator(token2) == 159)
			{
				if (!bTop)
				{
					this.AddErrorIF(token2, 386, Array.Empty<object>());
				}
				int num = 1;
				_IVariableLengthArrayType ivariableLengthArrayType = this.LMItemFactory.CreateVariableLengthArrayType();
				while (this.MatchOperator(new Operator[]
				{
					171,
					170
				}) == 171)
				{
					num++;
					this.MatchOperator(new Operator[]
					{
						159
					});
				}
				ivariableLengthArrayType.Dimensions = num;
				this.MatchOperator(new Operator[]
				{
					90
				});
				ivariableLengthArrayType._Base = this.ParseType(false, false);
				if (ivariableLengthArrayType._Base == null)
				{
					return null;
				}
				return ivariableLengthArrayType;
			}
			else
			{
				this.Scanner.SetPosition(token2);
				_IArrayType iarrayType = this.LMItemFactory.CreateArrayType();
				do
				{
					bool flag;
					_IExpression iexpression = this.ExpressionParser.ParseAssignment(out flag);
					this.MatchOperator(new Operator[]
					{
						173
					});
					_IExpression iexpression2 = this.ExpressionParser.ParseAssignment(out flag);
					iarrayType.AddDimension(iexpression, iexpression2);
				}
				while (this.MatchOperator(new Operator[]
				{
					170,
					171
				}) == 171);
				this.MatchOperator(new Operator[]
				{
					90
				});
				_IType itype = this.ParseType(false, false);
				iarrayType._Base = itype;
				if (iarrayType._Base == null)
				{
					return null;
				}
				if (itype.Class == 1)
				{
					this.AddErrorIF((tokenError.Length == 0) ? token : tokenError, 206, Array.Empty<object>());
				}
				return iarrayType;
			}
		}

		// Token: 0x060005D8 RID: 1496 RVA: 0x00018E54 File Offset: 0x00017054
		private _IType ParseReferenceType(bool bTop, IToken token, IToken tokenError)
		{
			this.MatchOperator(new Operator[]
			{
				102
			});
			_IReferenceType ireferenceType = this.LMItemFactory.CreateReferenceType();
			_IType itype = this.ParseType(false, false);
			if (itype == null)
			{
				return null;
			}
			if (itype.Class == 1)
			{
				this.AddErrorIF((tokenError.Length == 0) ? token : tokenError, 272, Array.Empty<object>());
			}
			ireferenceType._Base = itype;
			if (!bTop)
			{
				this.AddErrorIF(token, 261, Array.Empty<object>());
			}
			return ireferenceType;
		}

		// Token: 0x060005D9 RID: 1497 RVA: 0x00018ED0 File Offset: 0x000170D0
		private _IType ParsePointerType(IToken token, IToken tokenError)
		{
			this.MatchOperator(new Operator[]
			{
				102
			});
			_IPointerType ipointerType = this.LMItemFactory.CreatePointerType();
			_IType itype = this.ParseType(false, false);
			if (itype == null)
			{
				return null;
			}
			if (itype.Class == 1)
			{
				this.AddErrorIF((tokenError.Length == 0) ? token : tokenError, 205, Array.Empty<object>());
			}
			ipointerType._Base = itype;
			return ipointerType;
		}

		// Token: 0x060005DA RID: 1498 RVA: 0x00018F36 File Offset: 0x00017136
		private _IEnumDeclarationListStatement ParseEnumList(IToken tokenParenthesis, string stEnumName)
		{
			return EnumListParser.ParseEnumList(this.Context, tokenParenthesis, stEnumName);
		}
	}
}
