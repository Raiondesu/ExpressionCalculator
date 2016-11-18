using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

		private static Expression ParseExpression(string input, Type T, Func<string, Expression> nextParse)
		{
			string[] opcodes = T.GetProperty("AllOpCodes").GetValue(null) as string[] ?? new string[]{};
			string leftStr = input, rightStr = null, opCode = null;

			if (T == typeof(Term) && opcodes.Any(input.StartsWith)) leftStr = $"0{leftStr}";
			else if (opcodes.Any(input.Contains))
			{
				for (int bracesAmount = 0, i = input.Length - 1; i >= 0 && opCode == null && bracesAmount >= 0; i--)
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
							opCode = c;
							leftStr = input.Substring(0, i);
							rightStr = input.Substring(i + c.Length);
							if (leftStr.Length == 0)
								throw new SyntaxException(input, typeof(Unary), "Left part of an expression is empty!");
							if (rightStr.Length == 0)
								throw new SyntaxException(input, typeof(Unary), "Right part of an expression is empty!");
							break;
						}
				}
			}
			Expression left = nextParse(leftStr);
			Expression right = rightStr == null ? null : nextParse(rightStr);

			return T.GetConstructor(new[] {typeof(Expression), typeof(string), typeof(Expression)})?
				.Invoke(new object[] {left, opCode, right}) as Expression;
		}

		private static Expression ParseLogical(string input)
			=> ParseExpression(input, typeof(Logical), ParseRelation);

		private static Expression ParseRelation(string input)
			=> ParseExpression(input, typeof(Relation), ParseTerm);

		private static Expression ParseTerm(string input)
			=> ParseExpression(input, typeof(Term), ParseFactor);

		private static Expression ParseFactor(string input)
			=> ParseExpression(input, typeof(Factor), ParsePrimary);

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
					throw new BracesException(input, typeof(Unary));
				if (input.Length >= 2)
				{
					input = input.Substring(1, input.Length - 2);
					parenthesized = true;
				}
				else if (input.Length == 0)
					throw new SyntaxException(input, typeof(Unary), "Part of an expression is empty!");
				if (Integer.CheckBounds(input))
					return parenthesized ? ParseParenthesized(input) : new Integer(long.Parse(input));
			}

			var arr = Logical.AllOpCodes.Concat(Relation.AllOpCodes).Concat(Term.AllOpCodes).Concat(Factor.AllOpCodes);
			var s = arr.Aggregate(input, (current, x) => current.Replace(x, ""));
			var r = Regex.Match(input, "((.?){3}(\\)[(0-9])(.?){3}|(.?){3}([0-9)]\\()(.?){3})").Value
			        + Regex.Match(s, "[^0-9()]+").Value;
			if (r.Length > 0)
				throw new OperatorException(r, typeof(Unary));
			if (bracesAmount != 0)
				throw new BracesException(input, typeof(Unary));

			if (Integer.CheckBounds(input))
				return parenthesized ? ParseParenthesized(input) : new Integer(long.Parse(input));

			return parenthesized ? ParseParenthesized(input) : ParseLogical(input);
		}

		private static Expression ParseParenthesized(string input)
			=> new Parenthesized(ParseLogical(input));
	}
}