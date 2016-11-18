using System.ComponentModel;

namespace ExpressionCalc
{
	public abstract class Expression
	{
		public abstract long Value { get; }

		public virtual Expression Left { get; }
		public virtual Expression Right { get; }

		protected virtual string ToJson(string opCode, bool skipEmpty, uint level)
		{
			if (skipEmpty && Right == null && Left != null)
				return Left.ToJson(true, level);

			string tabs = "";
			for (int i = 0; i < level; i++)
				tabs += "    ";

			string type = "    \"type\": \"" + this.GetType().Name + "\"";
			string left = Left?.ToJson(skipEmpty, level + 1);
			string right = Right?.ToJson(skipEmpty, level + 1);
			string value = this.Value.ToString();

			left   = (left == null ? "" : ",\n" + tabs + "    \"left\": " + left);
			opCode = (opCode == null ? "" : ",\n" + tabs + "    \"opcode\": \"" + opCode + "\"");
			right  = (right == null  ? "" : ",\n" + tabs + "    \"right\": " + right);
			value = ",\n" + tabs + "    \"value\": " + value;


			return "{\n"+tabs+type+left+opCode+right+value+"\n"+tabs+"}";
		}

		public virtual string ToJson(bool skipEmpty = false, uint level = 0) => this.ToJson(null, skipEmpty, level);
	}

	public class Logical : Expression
	{
		public Logical(Expression left, string opCode = null, Expression right = null)
		{
			OpCode = opCode?.ToEnum<OpCodeType>();
			Left = left;
			Right = right;
		}

		public enum OpCodeType
		{
			[Description("&&")]
			and,
			[Description("||")]
			or,
			[Description("^")]
			xor
		}

		public static string[] AllOpCodes => default(OpCodeType).GetDescriptions();

		public OpCodeType? OpCode { get; protected set; }
		public override Expression Left { get; }
		public override Expression Right { get; }

		public override long Value
		{
			get
			{
				if (this.OpCode == OpCodeType.and)
					return (Left.Value > 0) & ((Right?.Value ?? 1) > 0) ? 1 : 0;
				if (this.OpCode == OpCodeType.or)
					return (Left.Value > 0) | ((Right?.Value ?? 1) > 0) ? 1 : 0;
				if (this.OpCode == OpCodeType.xor)
					return (Left.Value > 0) ^ ((Right?.Value ?? 1) > 0) ? 1 : 0;
				return Left.Value;
			}
		}

		public override string ToJson(bool skipEmpty = false, uint level = 0)
			=> this.ToJson(OpCode?.Description(), skipEmpty, level);

		public override string ToString() => Left + (OpCode?.Description() ?? "") + (Right?.ToString() ?? "");
	}

	public class Relation : Expression
	{
		public Relation(Expression left, string opCode = null, Expression right = null)
		{
			OpCode = opCode?.ToEnum<OpCodeType>();
			Left = left;
			Right = right;
		}

		public enum OpCodeType
		{
			[Description("<=")]
			LessEqual,
			[Description(">=")]
			BiggerEqual,
			[Description("==")]
			Equal,
			[Description("!=")]
			NotEqual,
			[Description("<")]
			Less,
			[Description(">")]
			Bigger
		}

		public static string[] AllOpCodes => default(OpCodeType).GetDescriptions();

		public OpCodeType? OpCode { get; protected set; }
		public override Expression Left { get; }
		public override Expression Right { get; }

		public override long Value
		{
			get
			{
				if (OpCode == OpCodeType.Bigger)
					return Left.Value > (Right?.Value ?? 0) ? 1 : 0;
				if (OpCode == OpCodeType.BiggerEqual)
					return Left.Value >= (Right?.Value ?? 0) ? 1 : 0;
				if (OpCode == OpCodeType.Less)
					return Left.Value < (Right?.Value ?? 0) ? 1 : 0;
				if (OpCode == OpCodeType.LessEqual)
					return Left.Value <= (Right?.Value ?? 0) ? 1 : 0;
				if (OpCode == OpCodeType.Equal)
					return Left.Value == (Right?.Value ?? 0) ? 1 : 0;
				if (OpCode == OpCodeType.NotEqual)
					return Left.Value != (Right?.Value ?? 0) ? 1 : 0;
				return Left.Value;
			}
		}

		public override string ToJson(bool skipEmpty = false, uint level = 0)
			=> this.ToJson(OpCode?.Description(), skipEmpty, level);

		public override string ToString() => Left + (OpCode?.Description() ?? "") + (Right?.ToString() ?? "");
	}

	public class Term : Expression
	{
		public Term(Expression left, string opCode = null, Expression right = null)
		{
			OpCode = opCode?.ToEnum<OpCodeType>();
			Left = left;
			Right = right;
		}

		public enum OpCodeType
		{
			[Description("+")]
			Plus,
			[Description("-")]
			Minus
		}

		public static string[] AllOpCodes => default(OpCodeType).GetDescriptions();

		public OpCodeType? OpCode { get; }
		public override Expression Left { get; }
		public override Expression Right { get; }

		public override long Value
		{
			get
			{
				if (OpCode == OpCodeType.Minus)
					return Left.Value - (Right?.Value ?? 0);
				if (OpCode == OpCodeType.Plus)
					return Left.Value + (Right?.Value ?? 0);
				return Left.Value;
			}
		}

		public override string ToJson(bool skipEmpty = false, uint level = 0)
			=> this.ToJson(OpCode?.Description(), skipEmpty, level);

		public override string ToString() => Left + (OpCode?.Description() ?? "") + (Right?.ToString() ?? "");
	}

	public class Factor : Expression
	{
		public Factor(Expression left, string opCode = null, Expression right = null)
		{
			OpCode = opCode?.ToEnum<OpCodeType>();
			Left = left;
			Right = right;
		}

		public enum OpCodeType
		{
			[Description("*")]
			Product,
			[Description("/")]
			Division
		}

		public static string[] AllOpCodes => default(OpCodeType).GetDescriptions();

		public OpCodeType? OpCode { get; }
		public override Expression Left { get; }
		public override Expression Right { get; }

		public override long Value
		{
			get
			{
				if (OpCode == OpCodeType.Division)
				{
					if (Right?.Value == 0)
						throw new CalculationException(Left.ToString().GetLast() + "/" + Right, this.GetType(), "Division by 0!");
					return Left.Value / (Right?.Value ?? 1);
				}
				if (OpCode == OpCodeType.Product)
					return Left.Value * (Right?.Value ?? 1);
				return Left.Value;
			}
		}

		public override string ToJson(bool skipEmpty = false, uint level = 0)
			=> this.ToJson(OpCode?.Description(), skipEmpty, level);

		public override string ToString() => Left + (OpCode?.Description() ?? "") + (Right?.ToString() ?? "");
	}

	public abstract class Primary : Expression {}

	public class Integer : Primary
	{
		public Integer(long value)
		{
			this.Value = value;
		}

		public static bool CheckBounds(string input)
		{
			long res;
			if (long.TryParse(input, out res)) return true;
			if (System.Text.RegularExpressions.Regex.IsMatch(input, "^-?[0-9]+$"))
				throw new CalculationException(input, typeof(Integer), "Number is out of bounds!");
			return false;
		}

		public override long Value { get; }

		public override string ToJson(bool skipEmpty = false, uint level = 0)
			=> this.ToJson(null, skipEmpty, level);

		public override string ToString() => Value.ToString();
	}

	public class Parenthesized : Primary
	{
		public Parenthesized(Expression expression)
		{
			Left = expression;
		}

		public override Expression Left { get; }

		public override long Value => Left.Value;

		protected override string ToJson(string opCode, bool skipEmpty, uint level)
		{
			string tabs = "";
			for (int i = 0; i < level; i++)
				tabs += "    ";

			string type = "    \"type\": \"" + this.GetType().Name + "\"";
			string expression = Left?.ToJson(skipEmpty, level + 1);
			string value = this.Value.ToString();

			expression   = (expression == null ? "" : ",\n" + tabs + "    \"expression\": " + expression);
			value = ",\n" + tabs + "    \"value\": " + value;

			return "{\n"+tabs+type+expression+value+"\n"+tabs+"}";
		}

		public override string ToJson(bool skipEmpty = false, uint level = 0)
			=> this.ToJson(null, skipEmpty, level);

		public override string ToString() => $"({Left})";
	}
}