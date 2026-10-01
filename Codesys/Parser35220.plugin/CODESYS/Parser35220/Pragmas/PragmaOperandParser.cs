using System;
using CODESYS.Parser;
using CODESYS.Parser35220.Resources;
using CODESYS.Parser35220.Utilities;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace CODESYS.Parser35220.Pragmas
{
	// Token: 0x02000038 RID: 56
	internal readonly struct PragmaOperandParser
	{
		// Token: 0x060003E7 RID: 999 RVA: 0x0001198B File Offset: 0x0000FB8B
		private PragmaOperandParser(ParserContext context)
		{
			this.Context = context;
		}

		// Token: 0x060003E8 RID: 1000 RVA: 0x00011994 File Offset: 0x0000FB94
		internal static _IExpression ParsePragmaORExp(ParserContext context, out bool bError, IToken tokenPragma)
		{
			PragmaOperandParser pragmaOperandParser = new PragmaOperandParser(context);
			return pragmaOperandParser.ParsePragmaORExp(out bError, tokenPragma);
		}

		// Token: 0x170000CC RID: 204
		// (get) Token: 0x060003E9 RID: 1001 RVA: 0x000119B2 File Offset: 0x0000FBB2
		private ParserContext Context { get; }

		// Token: 0x170000CD RID: 205
		// (get) Token: 0x060003EA RID: 1002 RVA: 0x000119BA File Offset: 0x0000FBBA
		private _ILanguageModelBuilder6 LMItemFactory
		{
			get
			{
				return this.Context.LMItemFactory;
			}
		}

		// Token: 0x170000CE RID: 206
		// (get) Token: 0x060003EB RID: 1003 RVA: 0x000119C7 File Offset: 0x0000FBC7
		private PragmaStatementParser PragmaParser
		{
			get
			{
				return this.Context.PragmaStatementParser;
			}
		}

		// Token: 0x170000CF RID: 207
		// (get) Token: 0x060003EC RID: 1004 RVA: 0x000119D4 File Offset: 0x0000FBD4
		private IPragmaScanner PragmaScanner
		{
			get
			{
				return this.PragmaParser.PragmaScanner;
			}
		}

		// Token: 0x170000D0 RID: 208
		// (get) Token: 0x060003ED RID: 1005 RVA: 0x000119E1 File Offset: 0x0000FBE1
		private IScanner9 Scanner
		{
			get
			{
				return this.Context.Scanner;
			}
		}

		// Token: 0x170000D1 RID: 209
		// (get) Token: 0x060003EE RID: 1006 RVA: 0x000119EE File Offset: 0x0000FBEE
		private IErrorHandler ErrorHandler
		{
			get
			{
				return this.Context.ErrorHandler;
			}
		}

		// Token: 0x060003EF RID: 1007 RVA: 0x000119FB File Offset: 0x0000FBFB
		private void AddErrorST(_IExprement exp, MessageId nErrorId, params object[] args)
		{
			this.ErrorHandler.AddErrorST(exp, nErrorId, args);
		}

		// Token: 0x060003F0 RID: 1008 RVA: 0x00011A0B File Offset: 0x0000FC0B
		private _IExpression ParseVersionPragmaOperand(_IVersionComparisonSupportingExpression verexpr, bool bUseWarningInsteadOfError)
		{
			return PragmaVersionOperandParser.ParsePragmaVersionOperand(this.Context, verexpr, bUseWarningInsteadOfError);
		}

		// Token: 0x060003F1 RID: 1009 RVA: 0x00011A1C File Offset: 0x0000FC1C
		private _IExpression ParsePragmaOperand(out bool bError, IToken tokenPragma)
		{
			bError = false;
			IPragmaToken pragmaToken;
			int next = this.PragmaScanner.GetNext(ref pragmaToken);
			_IExpression iexpression = null;
			if (next != 3)
			{
				return null;
			}
			PragmaOperator @operator = pragmaToken.Operator;
			if (@operator <= 40)
			{
				if (@operator != 16)
				{
					switch (@operator)
					{
					case 20:
						iexpression = this.ParseXRefPragma(out bError, tokenPragma);
						break;
					case 21:
					case 22:
					case 23:
						break;
					case 24:
						iexpression = this.ParseHasTypePragma(out bError, tokenPragma);
						break;
					case 25:
						iexpression = this.ParseHasAttributePragma(out bError, tokenPragma);
						break;
					case 26:
						iexpression = this.ParseHasValuePragma(tokenPragma);
						break;
					case 27:
						iexpression = this.ParseHasConstantValuePragma(tokenPragma);
						break;
					default:
						if (@operator == 40)
						{
							iexpression = (this.ParsePragmaORExp(out bError, tokenPragma) ?? this.LMItemFactory.CreateErrorExpression(tokenPragma));
							if (this.PragmaScanner.GetNext(ref pragmaToken) != 3 || pragmaToken.Operator != 41)
							{
								this.AddErrorST(iexpression, 6, new object[]
								{
									this.Scanner.GetOperatorText(168),
									this.PragmaScanner.GetTokenText(pragmaToken)
								});
							}
						}
						break;
					}
				}
				else
				{
					iexpression = this.ParseDefinedPragma(out bError, tokenPragma);
				}
			}
			else if (@operator <= 51)
			{
				if (@operator != 45)
				{
					if (@operator == 51)
					{
						_ICompilerVersionExpression verexpr = this.LMItemFactory.CreateCompilerVersionExpression(tokenPragma);
						return this.ParseVersionPragmaOperand(verexpr, true);
					}
				}
				else
				{
					_IExpression iexpression2 = this.ParsePragmaOperand(out bError, tokenPragma) ?? this.LMItemFactory.CreateErrorExpression(tokenPragma);
					_IPragmaOperatorExpression ipragmaOperatorExpression = this.LMItemFactory.CreatePragmaOperatorExpression(45, tokenPragma);
					ipragmaOperatorExpression.AddOperand(iexpression2);
					iexpression = ipragmaOperatorExpression;
				}
			}
			else if (@operator != 52)
			{
				switch (@operator)
				{
				case 59:
				{
					_IRuntimeVersionExpression verexpr2 = this.LMItemFactory.CreateRuntimeVersionExpression(tokenPragma);
					return this.ParseVersionPragmaOperand(verexpr2, false);
				}
				case 60:
					iexpression = this.ParseHasConstantTypePragma(tokenPragma);
					break;
				case 61:
					iexpression = this.ParseProjectDefinedPragma(out bError, tokenPragma);
					if (!bError && this.Context._bReportSP20Feature)
					{
						_IErrorExpression ierrorExpression = this.LMItemFactory.CreateErrorExpression();
						ierrorExpression.SetPositionIntern(iexpression.PositionIntern);
						ierrorExpression.LengthIntern = iexpression.LengthIntern;
						iexpression = ierrorExpression;
						this.Context.AddUnsupportedFeatureError(iexpression, Strings.CompilerFeature_ProjectDefines, ParserContext.CompilerVersion20);
					}
					break;
				}
			}
			else
			{
				iexpression = this.ParseIsEnumTypePragma(tokenPragma);
			}
			return iexpression;
		}

		// Token: 0x060003F2 RID: 1010 RVA: 0x00011C44 File Offset: 0x0000FE44
		private _IExpression ParseDefinedPragma(out bool bError, IToken tokenPragma)
		{
			return DefinedPragmaOperandParser.ParseDefinedPragma(this.Context, out bError, tokenPragma);
		}

		// Token: 0x060003F3 RID: 1011 RVA: 0x00011C53 File Offset: 0x0000FE53
		private _IExpression ParseProjectDefinedPragma(out bool bError, IToken tokenPragma)
		{
			return DefinedPragmaOperandParser.ParseProjectDefinedPragma(this.Context, out bError, tokenPragma);
		}

		// Token: 0x060003F4 RID: 1012 RVA: 0x00011C62 File Offset: 0x0000FE62
		private _IExpression ParseXRefPragma(out bool bError, IToken tokenPragma)
		{
			return XRefPragmaOperandParser.ParseXRefPragma(this.Context, out bError, tokenPragma);
		}

		// Token: 0x060003F5 RID: 1013 RVA: 0x00011C71 File Offset: 0x0000FE71
		private _IExpression ParseHasTypePragma(out bool bError, IToken tokenPragma)
		{
			return HasTypePragmaParser.ParseHasTypePragma(this.Context, out bError, tokenPragma);
		}

		// Token: 0x060003F6 RID: 1014 RVA: 0x00011C80 File Offset: 0x0000FE80
		private _IExpression ParseIsEnumTypePragma(IToken tokenPragma)
		{
			return HasTypePragmaParser.ParseIsEnumTypePragma(this.Context, tokenPragma);
		}

		// Token: 0x060003F7 RID: 1015 RVA: 0x00011C8E File Offset: 0x0000FE8E
		private _IExpression ParseHasAttributePragma(out bool bError, IToken tokenPragma)
		{
			return HasAttributePragmaParser.ParseHasAttributePragma(this.Context, out bError, tokenPragma);
		}

		// Token: 0x060003F8 RID: 1016 RVA: 0x00011C9D File Offset: 0x0000FE9D
		private _IExpression ParseHasValuePragma(IToken tokenPragma)
		{
			return HasValuePragmaParser.ParseHasValuePragma(this.Context, tokenPragma);
		}

		// Token: 0x060003F9 RID: 1017 RVA: 0x00011CAB File Offset: 0x0000FEAB
		private _IExpression ParseHasConstantValuePragma(IToken tokenPragma)
		{
			return HasConstantValueOrTypePragmaParser.ParseHasConstantValuePragma(this.Context, tokenPragma);
		}

		// Token: 0x060003FA RID: 1018 RVA: 0x00011CB9 File Offset: 0x0000FEB9
		private _IExpression ParseHasConstantTypePragma(IToken tokenPragma)
		{
			return HasConstantValueOrTypePragmaParser.ParseHasConstantTypePragma(this.Context, tokenPragma);
		}

		// Token: 0x060003FB RID: 1019 RVA: 0x00011CC8 File Offset: 0x0000FEC8
		private _IExpression ParsePragmaANDExp(out bool bError, IToken tokenPragma)
		{
			_IPragmaOperatorExpression ipragmaOperatorExpression = null;
			_IExpression iexpression = this.ParsePragmaOperand(out bError, tokenPragma);
			if (iexpression == null)
			{
				return null;
			}
			IPragmaToken pragmaToken;
			PragmaTokenType next = this.PragmaScanner.GetNext(ref pragmaToken);
			while (next == 3 && pragmaToken.Operator == 44)
			{
				_IExpression iexpression2 = this.ParsePragmaOperand(out bError, tokenPragma);
				if (iexpression2 == null)
				{
					return null;
				}
				this.AddPragmaOperandHelp(ref ipragmaOperatorExpression, iexpression, iexpression2, pragmaToken.Operator, tokenPragma);
				next = this.PragmaScanner.GetNext(ref pragmaToken);
			}
			this.PragmaScanner.SetPosition(pragmaToken);
			if (ipragmaOperatorExpression == null)
			{
				return iexpression;
			}
			return ipragmaOperatorExpression;
		}

		// Token: 0x060003FC RID: 1020 RVA: 0x00011D48 File Offset: 0x0000FF48
		private _IExpression ParsePragmaORExp(out bool bError, IToken tokenPragma)
		{
			_IPragmaOperatorExpression ipragmaOperatorExpression = null;
			_IExpression iexpression = this.ParsePragmaANDExp(out bError, tokenPragma);
			if (iexpression == null)
			{
				return null;
			}
			IPragmaToken pragmaToken;
			PragmaTokenType next = this.PragmaScanner.GetNext(ref pragmaToken);
			while (next == 3 && pragmaToken.Operator == 43)
			{
				_IExpression iexpression2 = this.ParsePragmaANDExp(out bError, tokenPragma);
				if (iexpression2 == null)
				{
					return null;
				}
				this.AddPragmaOperandHelp(ref ipragmaOperatorExpression, iexpression, iexpression2, pragmaToken.Operator, tokenPragma);
				next = this.PragmaScanner.GetNext(ref pragmaToken);
			}
			this.PragmaScanner.SetPosition(pragmaToken);
			if (ipragmaOperatorExpression == null)
			{
				return iexpression;
			}
			return ipragmaOperatorExpression;
		}

		// Token: 0x060003FD RID: 1021 RVA: 0x00011DC8 File Offset: 0x0000FFC8
		private void AddPragmaOperandHelp(ref _IPragmaOperatorExpression extop, _IExpression exp1, _IExpression exp2, PragmaOperator op, IToken tokenPragma)
		{
			if (exp2 == null)
			{
				return;
			}
			if (extop == null)
			{
				extop = this.LMItemFactory.CreatePragmaOperatorExpression(op, tokenPragma);
				extop.AddOperand(exp1);
			}
			else if (extop.Code != op)
			{
				_IPragmaOperatorExpression ipragmaOperatorExpression = this.LMItemFactory.CreatePragmaOperatorExpression(op, tokenPragma);
				ipragmaOperatorExpression.AddOperand(extop);
				extop = ipragmaOperatorExpression;
			}
			extop.AddOperand(exp2);
		}
	}
}
