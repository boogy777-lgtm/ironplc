using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using CODESYS.Parser;
using CODESYS.Parser35210.Tools;

namespace CODESYS.Parser35210.Utilities
{
	internal class PragmaSourcePositionAdjuster : EmptyVisitor
	{
		private readonly long _lPosition;

		private readonly int _nPositionOffset;

		private ITokenFactory TokenFactory { get; set; }

		internal PragmaSourcePositionAdjuster(long lPosition, int nPositionOffset, ITokenFactory tokenFactory)
		{
			_lPosition = lPosition;
			_nPositionOffset = nPositionOffset;
			TokenFactory = tokenFactory;
		}

		internal void AdjustSourcePositions(_IExpression exp)
		{
			StandardTraverser ivisit = new StandardTraverser(this);
			exp.Accept(ivisit);
		}

		public override void visit(_IVariableExpression variable)
		{
			AdjustPosition(variable);
		}

		private void AdjustPosition(_IExpression expr)
		{
			ISourcePosition position = expr.Position;
			if (position != null)
			{
				_IToken position2 = TokenFactory.CreateToken(_lPosition, (short)(position.PositionOffset + (short)_nPositionOffset + 1), position.Length);
				expr.SetPosition(position2);
			}
		}
	}
}
