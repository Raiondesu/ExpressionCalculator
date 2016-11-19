using System.ComponentModel;

namespace ExpressionCalc
{
	public abstract class Expression
	{
		public abstract double Value { get; }

		public abstract string ToJson(bool skipEmpty = false, uint level = 0);
	}

	public abstract class Binary : Expression
	{
		protected Binary(Expression left, Expression right = null)
		{
			Left = left;
			Right = right;
		}

		public enum OpCodeType{}

		public static string[] AllOpCodes => default(OpCodeType).GetDescriptions();

		public abstract string OpCode { get; }
		public Expression Left { get; }
		public Expression Right { get; }

		public override string ToJson(bool skipEmpty = false, uint level = 0)
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
			string opCode = (OpCode == null ? "" : ",\n" + tabs + "    \"opcode\": \"" + OpCode + "\"");

			left   = (left == null ? "" : ",\n" + tabs + "    \"left\": " + left);
			right  = (right == null  ? "" : ",\n" + tabs + "    \"right\": " + right);
			value = ",\n" + tabs + "    \"value\": " + value;

			return "{\n"+tabs+type+left+opCode+right+value+"\n"+tabs+"}";
		}

		public override string ToString() => Left + (OpCode ?? "") + (Right?.ToString() ?? "");
	}

	public class Logical : Binary
	{
		public Logical(Expression left, string opCode = null, Expression right = null)
			: base(left, right)
		{
			this.OperationCode = opCode?.ToEnum<OpCodeType>();
		}

		public new enum OpCodeType
		{
			[Description("and")]
			and,
			[Description("or")]
			or,
			[Description("xor")]
			xor
		}

		public new static string[] AllOpCodes => default(OpCodeType).GetDescriptions();

		private OpCodeType? OperationCode { get; }
		public override string OpCode => OperationCode?.Description();

		public override double Value
		{
			get
			{
				switch (this.OperationCode)
				{
					case OpCodeType.and:
						return (Left.Value > 0) & ((Right?.Value ?? 1) > 0) ? 1 : 0;
					case OpCodeType.or:
						return (Left.Value > 0) | ((Right?.Value ?? 1) > 0) ? 1 : 0;
					case OpCodeType.xor:
						return (Left.Value > 0) ^ ((Right?.Value ?? 1) > 0) ? 1 : 0;
					default:
						return Left.Value;
				}
			}
		}
	}

	public class Bitwise : Binary
	{
		public Bitwise(Expression left, string opCode = null, Expression right = null) : base(left, right)
		{
			OperationCode = opCode?.ToEnum<OpCodeType>();
		}

		public new enum OpCodeType
		{
			[Description("&")]
			band,
			[Description("|")]
			bor,
			[Description("^")]
			bxor,
			[Description("<<")]
			bshiftl,
			[Description(">>")]
			bshiftr
		}

		public new static string[] AllOpCodes => default(OpCodeType).GetDescriptions();

		private OpCodeType? OperationCode { get; }
		public override string OpCode => OperationCode?.Description();

		public override double Value
		{
			get
			{
				switch (OperationCode)
				{
						case OpCodeType.band:
							return (int) Left.Value & (int) (Right?.Value ?? 1);
						case OpCodeType.bor:
							return (int) Left.Value | (int) (Right?.Value ?? 0);
						case OpCodeType.bxor:
							return (int) Left.Value ^ (int) (Right?.Value ?? 0);
						case OpCodeType.bshiftl:
							return (int) Left.Value << (int)(Right?.Value ?? 0);
						case OpCodeType.bshiftr:
							return (int) Left.Value >> (int)(Right?.Value ?? 0);
					default:
							return Left.Value;
				}
			}
		}
	}

	public class Relation : Binary
	{
		public Relation(Expression left, string opCode = null, Expression right = null)
			: base(left, right)
		{
			OperationCode = opCode?.ToEnum<OpCodeType>();
		}

		public new enum OpCodeType
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

		public new static string[] AllOpCodes => default(OpCodeType).GetDescriptions();

		private OpCodeType? OperationCode { get; }
		public override string OpCode => OperationCode?.Description();

		public override double Value
		{
			get
			{
				switch (OperationCode)
				{
					case OpCodeType.Bigger:
						return Left.Value > (Right?.Value ?? 0) ? 1 : 0;
					case OpCodeType.BiggerEqual:
						return Left.Value >= (Right?.Value ?? 0) ? 1 : 0;
					case OpCodeType.Less:
						return Left.Value < (Right?.Value ?? 0) ? 1 : 0;
					case OpCodeType.LessEqual:
						return Left.Value <= (Right?.Value ?? 0) ? 1 : 0;
					case OpCodeType.Equal:
						return Left.Value == (Right?.Value ?? 0) ? 1 : 0;
					case OpCodeType.NotEqual:
						return Left.Value != (Right?.Value ?? 0) ? 1 : 0;
					default:
						return Left.Value;
				}
			}
		}
	}

	public class Term : Binary
	{
		public Term(Expression left, string opCode = null, Expression right = null)
			: base(left, right)
		{
			OperationCode = opCode?.ToEnum<OpCodeType>();
		}

		public new enum OpCodeType
		{
			[Description("+")]
			Plus,
			[Description("-")]
			Minus
		}

		public new static string[] AllOpCodes => default(OpCodeType).GetDescriptions();

		private OpCodeType? OperationCode { get; }
		public override string OpCode => OperationCode?.Description();

		public override double Value
		{
			get
			{
				switch (OperationCode)
				{
					case OpCodeType.Minus:
						return Left.Value - (Right?.Value ?? 0);
					case OpCodeType.Plus:
						return Left.Value + (Right?.Value ?? 0);
					default:
						return Left.Value;
				}
			}
		}
	}

	public class Factor : Binary
	{
		public Factor(Expression left, string opCode = null, Expression right = null)
			: base(left, right)
		{
			OperationCode = opCode?.ToEnum<OpCodeType>();
		}

		public new enum OpCodeType
		{
			[Description("*")]
			Product,
			[Description("/")]
			Division,
			[Description("%")]
			Modulo
		}

		public new static string[] AllOpCodes => default(OpCodeType).GetDescriptions();

		private OpCodeType? OperationCode { get; }
		public override string OpCode => OperationCode?.Description();

		public override double Value
		{
			get
			{
				switch (OperationCode)
				{
					case OpCodeType.Division:
						if (Right?.Value == 0)
							throw new CalculationException(Left.GetLast() + "/" + Right, this.GetType(), "Division by 0!");
						return Left.Value / (Right?.Value ?? 1);
					case OpCodeType.Product:
						return Left.Value * (Right?.Value ?? 1);
					default:
						return Left.Value;
				}
			}
		}
	}

	public abstract class Primary : Expression
	{
		public abstract Expression Expression { get; }

		public override string ToJson(bool skipEmpty = false, uint level = 0)
		{
			string tabs = "";
			for (int i = 0; i < level; i++)
				tabs += "    ";

			string type = "    \"type\": \"" + this.GetType().Name + "\"";
			string expression = Expression?.ToJson(skipEmpty, level + 1);
			string value = this.Value.ToString();

			expression   = (expression == null ? "" : ",\n" + tabs + "    \"expression\": " + expression);
			value = ",\n" + tabs + "    \"value\": " + value;

			return "{\n"+tabs+type+expression+value+"\n"+tabs+"}";
		}
	}

	public sealed class Integer : Primary
	{
		public Integer(double value)
		{
			this.Value = value;
		}

		public override double Value { get; }

		public static bool CheckBounds(string input)
		{
			double res;
			if (double.TryParse(input, out res)) return true;
			if (System.Text.RegularExpressions.Regex.IsMatch(input, "^-?[0-9]+$"))
				throw new CalculationException(input, typeof(Integer), "Number is out of bounds!");
			return false;
		}

		public override Expression Expression => null;

		public override string ToString() => Value.ToString();
	}

	public sealed class Parenthesized : Primary
	{
		public Parenthesized(Expression expression)
		{
			Expression = expression;
		}

		public override Expression Expression { get; }
		public override double Value => Expression.Value;

		public override string ToString() => $"({Expression})";
	}
}