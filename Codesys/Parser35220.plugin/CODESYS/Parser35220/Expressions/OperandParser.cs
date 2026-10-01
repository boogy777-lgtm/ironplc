using System;
using CODESYS.Parser;
using CODESYS.Parser35220.Resources;
using CODESYS.Parser35220.Utilities;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace CODESYS.Parser35220.Expressions
{
	// Token: 0x02000046 RID: 70
	internal class OperandParser
	{
		// Token: 0x060004C2 RID: 1218 RVA: 0x00014928 File Offset: 0x00012B28
		internal OperandParser(ParserContext context)
		{
			this.Context = context;
		}

		// Token: 0x17000115 RID: 277
		// (get) Token: 0x060004C3 RID: 1219 RVA: 0x00014942 File Offset: 0x00012B42
		private ParserContext Context { get; }

		// Token: 0x17000116 RID: 278
		// (get) Token: 0x060004C4 RID: 1220 RVA: 0x0001494A File Offset: 0x00012B4A
		private ExpressionParser ExpressionParser
		{
			get
			{
				return this.Context.ExpressionParser;
			}
		}

		// Token: 0x17000117 RID: 279
		// (get) Token: 0x060004C5 RID: 1221 RVA: 0x00014957 File Offset: 0x00012B57
		private _ILanguageModelBuilder6 LMItemFactory
		{
			get
			{
				return this.Context.LMItemFactory;
			}
		}

		// Token: 0x17000118 RID: 280
		// (get) Token: 0x060004C6 RID: 1222 RVA: 0x00014964 File Offset: 0x00012B64
		private IScanner9 Scanner
		{
			get
			{
				return this.Context.Scanner;
			}
		}

		// Token: 0x060004C7 RID: 1223 RVA: 0x00014974 File Offset: 0x00012B74
		internal IExpression ParseOperand()
		{
			bool flag;
			_IExpression result = this.ParseSTOperand(out flag);
			if (flag)
			{
				return null;
			}
			return result;
		}

		// Token: 0x060004C8 RID: 1224 RVA: 0x00014990 File Offset: 0x00012B90
		internal _IExpression ParseSTOperand(out bool bError)
		{
			_IToken itoken;
			return this.ParseSTOperandWithPosition(out bError, null, out itoken);
		}

		// Token: 0x060004C9 RID: 1225 RVA: 0x000149A7 File Offset: 0x00012BA7
		private TokenType Next(out IToken token)
		{
			return this.Scanner.Next(out token);
		}

		// Token: 0x17000119 RID: 281
		// (get) Token: 0x060004CA RID: 1226 RVA: 0x000149B5 File Offset: 0x00012BB5
		private IErrorHandler ErrorHandler
		{
			get
			{
				return this.Context.ErrorHandler;
			}
		}

		// Token: 0x060004CB RID: 1227 RVA: 0x000149C4 File Offset: 0x00012BC4
		private _IExpression ParseArrayAccess(_IExpression exp, out bool bError, _IToken currentToken, ref _IToken endToken)
		{
			_IIndexAccessExpression iindexAccessExpression = this.LMItemFactory.CreateIndexAccessExpression(exp, currentToken);
			_IExpression iexpression = this.ExpressionParser.ParseAssignment(out bError);
			if (iexpression == null)
			{
				return null;
			}
			iindexAccessExpression.AddAccess(iexpression);
			IToken token;
			while (this.Next(out token) == 15)
			{
				Operator @operator = this.Scanner.GetOperator(token);
				if (@operator != 171 && @operator != 170)
				{
					this.ErrorHandler.AddErrorSTWithToken(iindexAccessExpression, token, 2, new object[]
					{
						this.Scanner.GetOperatorText(171),
						this.Scanner.GetOperatorText(170),
						this.Scanner.GetTokenText(token)
					});
					bError = true;
					return iindexAccessExpression;
				}
				if (@operator != 171)
				{
					endToken = (token as _IToken);
					return iindexAccessExpression;
				}
				iexpression = this.ExpressionParser.ParseAssignment(out bError);
				iindexAccessExpression.AddAccess(iexpression);
			}
			this.ErrorHandler.AddErrorSTWithToken(iindexAccessExpression, token, 2, new object[]
			{
				this.Scanner.GetOperatorText(171),
				this.Scanner.GetOperatorText(170),
				this.Scanner.GetTokenText(token)
			});
			bError = true;
			return iindexAccessExpression;
		}

		// Token: 0x060004CC RID: 1228 RVA: 0x00014AE8 File Offset: 0x00012CE8
		private _IExpression ParseNamespaceAccess(_IExpression exp, out bool bError, _IToken currentToken, out _IToken endToken)
		{
			_INamespaceAccessExpression inamespaceAccessExpression = this.LMItemFactory.CreateNamespaceAccessExpression(exp, null, currentToken);
			bError = false;
			IToken token;
			if (this.Next(out token) == 13)
			{
				string identifier = this.Scanner.GetIdentifier(token);
				_IVariableExpression access = this.LMItemFactory.CreateVariableExpression(identifier, token);
				inamespaceAccessExpression._Access = access;
			}
			else
			{
				inamespaceAccessExpression._Access = this.LMItemFactory.CreateNullExpression(token);
				this.ErrorHandler.AddErrorSTWithToken(inamespaceAccessExpression, token, 4, new object[]
				{
					this.Scanner.GetTokenText(token),
					exp
				});
				bError = true;
			}
			endToken = (token as _IToken);
			return inamespaceAccessExpression;
		}

		// Token: 0x060004CD RID: 1229 RVA: 0x00014B80 File Offset: 0x00012D80
		private _IExpression ParseComponentAccess(_IExpression exp, out bool bError, _IToken currentToken, out _IToken endToken)
		{
			_ICompoAccessExpression icompoAccessExpression = this.LMItemFactory.CreateCompoAccessExpression(exp, currentToken);
			bError = false;
			IToken token;
			TokenType tokenType = this.Next(out token);
			if (tokenType == 14)
			{
				ulong num;
				bool flag;
				Operator @operator;
				bool flag2;
				this.Scanner.GetInteger(token, ref num, ref flag, ref @operator, ref flag2);
				if (num >= 64UL || flag || flag2)
				{
					this.ErrorHandler.AddErrorSTWithToken(icompoAccessExpression, token, 3, new object[]
					{
						num,
						exp
					});
				}
				icompoAccessExpression._Right = this.LMItemFactory.CreateLiteralExpression((long)num, 33, token);
				endToken = (token as _IToken);
				return icompoAccessExpression;
			}
			if (tokenType == 27)
			{
				DirectVariableSize directVariableSize;
				int num2;
				bool flag3;
				((_IScanner4)this.Scanner).GetPartialAccess(token, ref directVariableSize, ref num2, ref flag3);
				_IPartialAccessExpression ipartialAccessExpression = this.LMItemFactory.CreatePartialAccessExpression(token, exp, directVariableSize, num2);
				if (flag3)
				{
					this.ErrorHandler.AddErrorST(ipartialAccessExpression, 3, new object[]
					{
						int.MaxValue,
						exp
					});
				}
				endToken = (token as _IToken);
				if (this.Context._bReportSP19Feature)
				{
					this.Context.AddUnsupportedFeatureError(ipartialAccessExpression, Strings.CompilerFeature_PartialVariableAccess, ParserContext.CompilerVersion19);
				}
				return ipartialAccessExpression;
			}
			if (tokenType == 13)
			{
				string identifier = this.Scanner.GetIdentifier(token);
				_IVariableExpression right = this.LMItemFactory.CreateVariableExpression(identifier, token);
				icompoAccessExpression._Right = right;
			}
			else
			{
				this.ErrorHandler.AddErrorSTWithToken(icompoAccessExpression, token, 4, new object[]
				{
					this.Scanner.GetTokenText(token),
					exp
				});
				bError = true;
			}
			endToken = (token as _IToken);
			return icompoAccessExpression;
		}

		// Token: 0x060004CE RID: 1230 RVA: 0x00014D00 File Offset: 0x00012F00
		internal _IExpression ParseVarAccess(_IExpression exp, out bool bError, _IToken startToken, ref _IToken endToken)
		{
			_IExpression result;
			try
			{
				this._nStackDepth++;
				if (this._nStackDepth > this._iMaxNestingDepth)
				{
					throw new MaximumNestingDepthExceededException();
				}
				bError = false;
				IToken token;
				if (this.Next(out token) != 15)
				{
					this.Scanner.SetPosition(token);
					result = exp;
				}
				else
				{
					Operator @operator = this.Scanner.GetOperator(token);
					_IExpression iexpression;
					if (@operator <= 167)
					{
						if (@operator == 162)
						{
							iexpression = this.ParseComponentAccess(exp, out bError, token as _IToken, out endToken);
							goto IL_102;
						}
						if (@operator == 167)
						{
							iexpression = FunctionCallParser.ParseFunctionCall(this.Context, exp, out bError, token as _IToken, ref endToken);
							goto IL_102;
						}
					}
					else
					{
						if (@operator == 169)
						{
							iexpression = this.ParseArrayAccess(exp, out bError, token as _IToken, ref endToken);
							goto IL_102;
						}
						if (@operator == 183)
						{
							iexpression = this.LMItemFactory.CreateDeRefAccessExpression(exp, token);
							endToken = (token as _IToken);
							goto IL_102;
						}
						if (@operator == 271)
						{
							iexpression = this.ParseNamespaceAccess(exp, out bError, token as _IToken, out endToken);
							goto IL_102;
						}
					}
					this.Scanner.SetPosition(token);
					return exp;
					IL_102:
					if (endToken == null)
					{
						endToken = startToken;
					}
					iexpression.PositionLength = Helper.CalculateLength(startToken, endToken);
					result = this.ParseVarAccess(iexpression, out bError, startToken, ref endToken);
				}
			}
			finally
			{
				this._nStackDepth--;
			}
			return result;
		}

		// Token: 0x060004CF RID: 1231 RVA: 0x00014E64 File Offset: 0x00013064
		internal _IExpression ParseSTOperandWithPosition(out bool bError, _IToken startToken, out _IToken endToken)
		{
			this._nStackDepth++;
			if (this._nStackDepth > this._iMaxNestingDepth)
			{
				throw new MaximumNestingDepthExceededException();
			}
			if (startToken == null)
			{
				IToken token;
				this.Next(out token);
				this.Scanner.SetPosition(token);
				startToken = (token as _IToken);
			}
			short num;
			_IExpression iexpression = this.ParseSTOperandHelp(out bError, startToken, out endToken, out num);
			if (endToken == null)
			{
				endToken = (_IToken)this.Scanner.CurrentToken;
			}
			iexpression.PositionLength = ((num != 0) ? num : Helper.CalculateLength(startToken, endToken));
			this._nStackDepth--;
			return iexpression;
		}

		// Token: 0x060004D0 RID: 1232 RVA: 0x00014EF8 File Offset: 0x000130F8
		internal _IExpression ParseQualifiedNameExpression(_IExprement expForError)
		{
			int sourceOffset = this.Scanner.SourceOffset;
			_IExpression iexpression = null;
			bool flag;
			bool flag2;
			IToken token = this.CheckForSpecialScopes(out flag, out flag2);
			Operator @operator = 0;
			IToken token2;
			IToken position;
			for (;;)
			{
				this.Next(out token2);
				if (token2.Type != 13)
				{
					break;
				}
				if (iexpression == null)
				{
					iexpression = this.LMItemFactory.CreateVariableExpression(this.Scanner.GetIdentifier(token2), token2);
				}
				else if (@operator == 162)
				{
					_ICompoAccessExpression icompoAccessExpression = this.LMItemFactory.CreateCompoAccessExpression(iexpression, token2);
					icompoAccessExpression._Right = this.LMItemFactory.CreateVariableExpression(this.Scanner.GetIdentifier(token2), token2);
					iexpression = icompoAccessExpression;
				}
				else if (@operator == 271)
				{
					_INamespaceAccessExpression inamespaceAccessExpression = this.LMItemFactory.CreateNamespaceAccessExpression(iexpression, null, token2);
					inamespaceAccessExpression._Access = this.LMItemFactory.CreateVariableExpression(this.Scanner.GetIdentifier(token2), token2);
					iexpression = inamespaceAccessExpression;
				}
				this.Scanner.Next(out position, false, true);
				this.Scanner.SetPosition(position);
				@operator = this.Scanner.MatchOperator(this.ErrorHandler, null, false, new Operator[]
				{
					162,
					271
				});
				if (@operator == null)
				{
					goto Block_6;
				}
			}
			if (expForError != null)
			{
				this.ErrorHandler.AddErrorSTWithToken(expForError, token2, 26, new object[]
				{
					this.Scanner.GetTokenText(token2)
				});
			}
			this.Scanner.ParseReSyncIF();
			return null;
			Block_6:
			this.Scanner.SetPosition(position);
			if (iexpression == null)
			{
				return null;
			}
			if (flag)
			{
				iexpression = this.LMItemFactory.CreateSystemScopeExpression(iexpression, token);
			}
			else if (flag2)
			{
				iexpression = this.LMItemFactory.CreatePoolScopeExpression(iexpression, token);
			}
			int sourceOffset2 = this.Scanner.SourceOffset;
			iexpression.PositionLength = (short)(sourceOffset2 - sourceOffset);
			return iexpression;
		}

		// Token: 0x060004D1 RID: 1233 RVA: 0x000150A0 File Offset: 0x000132A0
		private IToken CheckForSpecialScopes(out bool bSystemScope, out bool bPoolScope)
		{
			bSystemScope = false;
			bPoolScope = false;
			IToken token;
			this.Next(out token);
			IToken token2;
			if (token.Type == 15 && this.Scanner.GetOperator(token) == 192 && this.Next(out token2) == 15 && this.Scanner.GetOperator(token2) == 162)
			{
				bSystemScope = true;
			}
			else if (token.Type == 15 && this.Scanner.GetOperator(token) == 251 && this.Next(out token2) == 15 && this.Scanner.GetOperator(token2) == 162)
			{
				bPoolScope = true;
			}
			else
			{
				this.Scanner.SetPosition(token);
			}
			return token;
		}

		// Token: 0x060004D2 RID: 1234 RVA: 0x0001514C File Offset: 0x0001334C
		private _IExpression ParseSTOperandHelp(out bool bError, _IToken startToken, out _IToken endToken, out short lenghtOfExpWithoutParanthesis)
		{
			bError = false;
			IToken token;
			this.Next(out token);
			_IExpression iexpression = null;
			lenghtOfExpWithoutParanthesis = 0;
			endToken = (token as _IToken);
			switch (token.Type)
			{
			case 1:
			case 5:
			case 6:
			case 9:
			case 10:
			case 11:
			case 14:
			case 16:
			case 17:
			case 18:
			case 22:
			case 23:
			case 24:
			case 25:
				iexpression = this.LMItemFactory.CreateLiteralExpression(this.Context, token);
				endToken = (token as _IToken);
				break;
			case 7:
			{
				DirectVariableLocation directVariableLocation;
				DirectVariableSize directVariableSize;
				int[] array;
				bool flag;
				this.Scanner.GetDirectVariable(token, ref directVariableLocation, ref directVariableSize, ref array, ref flag);
				iexpression = this.LMItemFactory.CreateAddressExpression(this.LMItemFactory.CreateDirectVariable(directVariableLocation, directVariableSize, array), token);
				if (flag)
				{
					this.ErrorHandler.AddErrorST(iexpression, 5, new object[]
					{
						iexpression
					});
				}
				endToken = (token as _IToken);
				break;
			}
			case 13:
			{
				string identifier = this.Scanner.GetIdentifier(token);
				_IVariableExpression exp = this.LMItemFactory.CreateVariableExpression(identifier, token);
				iexpression = this.ParseVarAccess(exp, out bError, token as _IToken, ref endToken);
				break;
			}
			case 15:
			{
				Operator @operator = this.Scanner.GetOperator(token);
				iexpression = this.ExpressionParser.ParseSTPrefixOperator(out bError, @operator, token as _IToken, startToken, out endToken, out lenghtOfExpWithoutParanthesis);
				break;
			}
			}
			if (iexpression == null)
			{
				iexpression = this.LMItemFactory.CreateErrorExpression(token);
				this.ErrorHandler.AddErrorST(iexpression, 7, new object[]
				{
					this.Scanner.GetTokenText(token)
				});
				this.Scanner.SetPosition(token);
				bError = true;
			}
			return iexpression;
		}

		// Token: 0x040000B5 RID: 181
		private readonly int _iMaxNestingDepth = 2000;

		// Token: 0x040000B6 RID: 182
		private int _nStackDepth;
	}
}
