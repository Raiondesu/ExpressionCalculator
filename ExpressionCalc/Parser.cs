using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace ExpressionCalc
{
	public static class Parser
	{
		public static Expression Parse(this string input)
		{
			var m = Regex.Match(input, "[0-9]\\s+[0-9]");
			if (m.Success)
				throw new SyntaxException(m.Value, typeof(Expression), "Spaces between numbers are not allowed!");
			return ParseLogical(Regex.Replace(input, "\\s+", ""));
		}

		private static string[] ParseString(string input, string[] opcodes, Type type)
		{
			string left = input, right = null, opcode = null;

			if (type == typeof(Term) && opcodes.Any(input.StartsWith)) left = $"0{left}";
			else if (opcodes.Any(input.Contains))
			{
				for (int bracesAmount = 0, i = input.Length - 1; i >= 0 && opcode == null && bracesAmount >= 0; i--)
				{
					if (input[i] == ')')
						bracesAmount++;
					else if (input[i] == '(')
						bracesAmount--;
					else if (bracesAmount == 0)
						foreach (var c in opcodes)
						{
							if (c.Length + i - 1 >= input.Length) continue;
							string code = input.Substring(i, c.Length);
							if (code != c) continue;
							opcode = c;
							left = input.Substring(0, i);
							right = input.Substring(i + c.Length);
							if (left.Length == 0)
								throw new SyntaxException(input, typeof(Primary), "Left part of an expression is empty!");
							if (right.Length == 0)
								throw new SyntaxException(input, typeof(Primary), "Right part of an expression is empty!");
							break;
						}
				}
			}
			return new[] {left, opcode, right};
		}

		private static T ParseExpression<T>(string input, string[] opcodes, Func<string, Expression> nextParse) where T : Expression
		{
			string[] parsed = ParseString(input, opcodes, typeof(T));
			Expression left = nextParse(parsed[0]);
			var opCode = parsed[1];
			Expression right = parsed[2] == null ? null : nextParse(parsed[2]);

			return typeof(T).GetConstructor(new[] {typeof(Expression), typeof(string), typeof(Expression)})?
				.Invoke(new object[] {left, opCode, right}) as T;
		}

		private static Expression ParseLogical(string input)
			=> ParseExpression<Logical>(input, Logical.AllOpCodes, ParseRelation);

		private static Expression ParseRelation(string input)
			=> ParseExpression<Relation>(input, Relation.AllOpCodes, ParseTerm);

		private static Expression ParseTerm(string input)
			=> ParseExpression<Term>(input, Term.AllOpCodes, ParseFactor);

		private static Expression ParseFactor(string input)
			=> ParseExpression<Factor>(input, Factor.AllOpCodes, ParsePrimary);

		private static Expression ParsePrimary(string input)
		{
			bool wrapped = true, parenthesized = false;
			long bracesAmount = 0;

			while (wrapped)
			{
				foreach (char c in input)
				{
					if 		(c == '(')	bracesAmount++;
					else if (c == ')')	bracesAmount--;
					else if (bracesAmount == 0) wrapped = false;
					if (bracesAmount < 0) break;
				}
				if (!wrapped) continue;

				if (bracesAmount != 0)
					throw new BracesException(input, typeof(Primary));
				if (input.Length >= 2)
				{
					input = input.Substring(1, input.Length - 2);
					parenthesized = true;
				}
				else if (input.Length == 0)
					throw new SyntaxException(input, typeof(Primary), "Part of an expression is empty!");
				if (Integer.CheckBounds(input))
					return parenthesized ? ParseParenthesized(input) : new Integer(long.Parse(input));
			}

			var arr = Logical.AllOpCodes.Concat(Relation.AllOpCodes).Concat(Term.AllOpCodes).Concat(Factor.AllOpCodes);
			var s = arr.Aggregate(input, (current, x) => current.Replace(x, ""));
			var r = Regex.Match(input, "((.?){3}(\\)[(0-9])(.?){3}|(.?){3}([0-9)]\\()(.?){3})").Value
			        + Regex.Match(s, "[^0-9()]+").Value;
			if (r.Length > 0)
				throw new OperatorException(r, typeof(Primary));
			if (bracesAmount != 0)
				throw new BracesException(input, typeof(Primary));

			if (Integer.CheckBounds(input))
				return parenthesized ? ParseParenthesized(input) : new Integer(long.Parse(input));

			return parenthesized ? ParseParenthesized(input) : ParseLogical(input);
		}

		private static Expression ParseParenthesized(string input)
			=> new Parenthesized(ParseLogical(input));
	}
}