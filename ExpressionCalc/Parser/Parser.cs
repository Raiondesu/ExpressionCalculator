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
			return ParseBinary(Regex.Replace(input, "\\s+", ""), typeof(Binary).GetAllSubclasses());
		}

		private static Expression ParseBinary(string input, Type[] T, int idx = 0)
		{
			if (input == null) return null;
			string[] opcodes = T[idx].GetProperty("AllOpCodes").GetValue(null) as string[] ?? new string[]{};
			string leftStr = input, rightStr = null, opCode = null;

			if (T[idx] == typeof(Term) && opcodes.Any(input.StartsWith)) leftStr = $"0{leftStr}";
			else if (opcodes.Any(x => Regex.IsMatch(input, $"[0-9)]{Regex.Escape(x)}[(0-9]")))
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
								throw new SyntaxException(input, typeof(Primary), "Left part of an expression is empty!");
							if (rightStr.Length == 0)
								throw new SyntaxException(input, typeof(Primary), "Right part of an expression is empty!");
							break;
						}
				}
			}
			Expression left = idx < T.Length - 1 ?
				ParseBinary(leftStr, T, idx + 1) : ParsePrimary(leftStr, T);
			Expression right = idx < T.Length - 1 ?
				ParseBinary(rightStr, T, idx + 1): ParsePrimary(rightStr, T);

			return T[idx].GetConstructor(new[] {typeof(Expression), typeof(string), typeof(Expression)})?
				.Invoke(new object[] {left, opCode, right}) as Expression;
		}

		private static Expression ParsePrimary(string input, Type[] T)
		{
			if (input == null) return null;
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
				if (Integer.CheckBounds(input))
					return parenthesized ? new Parenthesized(ParseBinary(input, T)) : new Integer(long.Parse(input)) as Expression;

				if (bracesAmount != 0)
					throw new BracesException(input, typeof(Primary));
				if (input.Length >= 2 && wrapped)
				{
					input = input.Substring(1, input.Length - 2);
					parenthesized = true;
				}
				else if (input.Length == 0)
					throw new SyntaxException(input, typeof(Primary), "Part of an expression is empty!");
			}

			var t = T.Aggregate(new string[T.Length], (current, subclass) => current.Concat(
				subclass.GetProperty("AllOpCodes").GetValue(null) as string[] ?? new string[] {}
			).ToArray());
			var s = t.Aggregate(input, (current, x) => x != null ? current.Replace(x, "") : current);
			var r = Regex.Match(input, "((.?){3}(\\)[(0-9])(.?){3}|(.?){3}([0-9)]\\()(.?){3})").Value;
			var m = Regex.Match(s, "[^0-9()]+").Value;
			if (r.Length + m.Length > 0)
				throw new OperatorException(r + m, typeof(Primary));

			return parenthesized ? new Parenthesized(ParseBinary(input, T)) : ParseBinary(input, T);
		}
	}
}