using System;
using System.Linq;

namespace ExpressionCalc
{
	public static class Tests
	{
		public static string[] Manual =
		{
//			"0||2",
//			"2 2",
//			"(22 + 1)-3*-2 2",
//			"()",
//			")",
//			"(",
//			"",
//			"(2+(12/3)",
//			"8>=4+(1&5",
//			"12+",
//			"+12",
//			"9223372036854775807",
//			"12*(-9223372036854775807)",
//			"14112222482725050715397",
//			"79228162514264337593543950336",
//			"13:32",
//			"I am not an expression!",
//			"1!",
//			"5^\t(-2)",
//			"12 + 2 - 5b",
//			"12 + 2 - 5\n2",
//			"-11",
//			"(8 / 0)",
//			"( 6 - 10) - ((16/2-1) / (1-12&2))*((9) - 5)+6",
//			"(8 / 1)( 6 - 10)",
//			"(8 / 1)*( 6 - 10)   ((9) - 5)",
//			"(8 / 1)*( 6 - 10) - ((9) - 5)6",
//			"5*(-2)",
//			"5&(-2)",
			"-5-(-3)",
			"+5-(-3)",
			"5-(-3)",
//			"(((((-(-3))))))",
//			"-(-(3))",
//			"((3))",
//			"-",
//			"5--3",
//			"(22 + 1)-3*-2",
//			"(0)-0)",
//			"(0)-(0)-",
//			"<(1+2)",
//			"(1+2)+12-3<=",
//			"0+(1<=2)+1",
//			"9>=2&112",
			"4<<8/225&(74)or0"
		};

		///  <summary>
		///  Expression generator.
		///  © Copyright by Alexey Iskhakov
		///  </summary>
		///  <param name="amount">Amount of expressions to generate</param>
		///  <param name="lengthLimit">Approximate limit of characters per expression</param>
		///  <param name="fullyRandom">Whether the generator should produce absolutely random symbols</param>
		///  <param name="sepWithSpace">Whether to insert random spaces between symbols</param>
		/// <param name="incLog">Whether to include Logical operators</param>
		/// <param name="incRel">Whether to include Relational operators</param>
		/// <param name="incDiv">Whether to include division</param>
		/// <returns></returns>
		public static string[] Generate(int amount = 1, int lengthLimit = 30, bool fullyRandom = false,
			bool sepWithSpace = false, bool incLog = true, bool incRel = true, bool incDiv = true)
		{
			string[] result = new string[amount];

			var terms = Term.AllOpCodes;
			var facs = incDiv ? Factor.AllOpCodes : Factor.AllOpCodes.Where(x => x != "/").ToArray();
			var logs = incLog ? Logical.AllOpCodes : terms;
			var rels = incRel ? Relation.AllOpCodes : facs;
			var nums = "0 1 2 3 4 5 6 7 8 9".Split(' ');
			var alls = logs.Concat(rels.Concat(terms.Concat(facs))).ToArray();
			var rand = new Random();
			for (int i = 0; i < amount; i++)
			{
				string str = "";
				var length = rand.Next(1, lengthLimit);
				bool numPrev = false;
				bool opPrev = false;
				int bracAmount = 0;
				for (int j = 0; j < length; j++)
				{
					if (sepWithSpace && rand.Next(0, 20) == 0)
						str += ' ';

					int idx;
					if (!fullyRandom)
						switch (rand.Next(0, 14))
						{
							case 0:
								if (j == length - 1) goto default;
								if (!numPrev) break;
								if (sepWithSpace) str += ' ';
								idx = rand.Next(0, logs.Length);
								str += logs[idx];
								numPrev = false;
								opPrev = true;
								if (sepWithSpace) str += ' ';
								break;
							case 1:
								if (j == length - 1) goto default;
								if (!numPrev) break;
								if (sepWithSpace) str += ' ';
								idx = rand.Next(0, rels.Length);
								str += rels[idx];
								numPrev = false;
								opPrev = true;
								if (sepWithSpace) str += ' ';
								break;
							case 2:
								if (j == length - 1) goto default;
								if (!numPrev) break;
								if (sepWithSpace) str += ' ';
								idx = rand.Next(0, terms.Length);
								str += terms[idx];
								numPrev = false;
								opPrev = true;
								if (sepWithSpace) str += ' ';
								break;
							case 3:
								if (j == length - 1) goto default;
								if (!numPrev) break;
								if (sepWithSpace) str += ' ';
								idx = rand.Next(0, facs.Length);
								str += facs[idx];
								numPrev = false;
								opPrev = true;
								if (sepWithSpace) str += ' ';
								break;
							case 4:
								if (j == length - 1) goto default;
								if (!numPrev) break;
								if (sepWithSpace) str += ' ';
								idx = rand.Next(0, alls.Length);
								str += alls[idx];
								numPrev = false;
								opPrev = true;
								if (bracAmount > 0) goto case 5;
								if (sepWithSpace) str += ' ';
								break;
							case 5:
							case 6:
							case 7:
							case 8:
							case 9:
							case 10:
								if (bracAmount == 0 || !numPrev) break;
								str += ')';
								bracAmount--;
								str += alls[rand.Next(alls.Length)];
								numPrev = false;
								break;
							case 11:
								if (j == length - 1) goto default;
								if (numPrev) break;
								str += '(';
								bracAmount++;
								str += nums[rand.Next(nums.Length)];
								numPrev = true;
								break;

							default:
								if (rand.Next(0, nums.Length) == 0 && opPrev) break;
								idx = rand.Next(0, nums.Length);
								if (!numPrev && rand.Next(0, 2) == 0)
								{
									str += "(-" + nums[idx];
									bracAmount++;
								}
								else str += nums[idx];
								numPrev = true;
								break;
						}
					else
					{
						var all = alls.Concat(nums.Concat("( ) \t # $ % ~ ` \n ; :".Split(' '))).ToArray();
						str += all[rand.Next(all.Length)];
					}
				}
				str += nums[rand.Next(nums.Length)];
				for (int j = 0; j < bracAmount; j++)
					str += ')';
				result[i] += str;
			}
			return result;
		}

		public static void Test(this string input, bool skipEmpty = true)
			=> Test(new[] {input}, skipEmpty);

		public static void Test(this string[] input, bool skipEmpty = true)
		{
			foreach (var str in input)
			{
				try
				{
					Console.WriteLine(str);
					Expression expression = str.Parse();

					Console.WriteLine("Result: " + expression.Value);
					Console.WriteLine(expression + ":\nJSON:\n" + expression.ToJson(skipEmpty));
				}
				catch (Exception e)
				{
					Console.WriteLine(e);
				}
				finally
				{
					Console.WriteLine();
				}
			}
		}
	}
}