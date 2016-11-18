#define PRINT_JSON
//#define TO_FILE

using System;
using System.Linq;

namespace ExpressionCalc
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			string input = "(1 + 26 - 98) / 15 + 777 >= 772";

			if (args.Length > 0)
				input = args[0];

			Test(input, false);
			Test(input);

			#region Tests
			if (args.Length == 0)
			{
				Test("0||2");
				Test("2 2");
				Test("(22 + 1)-3*-2 2");
				Test("()");
				Test(")");
				Test("(");
				Test("");
				Test("(2+(12/3)");
				Test("8>=4+(1&&5");
				Test("12+");
				Test("+12");
				Test("9223372036854775807");
				Test("12*(-9223372036854775807)");
				long t = -9223372036854775807;
				Console.WriteLine((12*t).ToString());
				Test("14112222482725050715397");
				Test("79228162514264337593543950336");
				Test("13:32");
				Test("I am not an expression!");
				Test("1!");
				Test("5^\t(-2)");
				Test("12 + 2 - 5b");
				Test("12 + 2 - 5\n2");
				Test("-11");
				Test("(8 / 0)");
				Test("( 6 - 10) - ((16/2-1) / (1-12&&2))*((9) - 5)+6");
				Test("(8 / 1)( 6 - 10)");
				Test("(8 / 1)*( 6 - 10)   ((9) - 5)");
				Test("(8 / 1)*( 6 - 10) - ((9) - 5)6");
				Test("5*(-2)");
				Test("5&&(-2)");
				Test("-5-(-3)");
				Test("+5-(-3)");
				Test("5-(-3)");
				Test("(((((-(-3))))))");
				Test("-(-(3))");
				Test("((3))");
				Test("-");
				Test("5--3");
				Test("(22 + 1)-3*-2");
				Test("(0)-0)");
				Test("(0)-(0)-");
				Test("<(1+2)");
				Test("(1+2)+12-3<=");
				Test("0+(1<=2)+1");
				Test("9>=2&&112");
				Test("4>=8/225||(74)||0");

				Test(GenerateTests(10), false);
				Test(GenerateTests(10));
				Test(GenerateTests(10, 1000, incRel: false, incLog: false, incDiv: false));
				Test(GenerateTests(10, 15, fullyRandom: true));
			}
			#endregion
		}

		private static void Test(string input, bool skipEmpty = true)
		{
			Test(new[] {input}, skipEmpty);
		}

		private static void Test(string[] input, bool skipEmpty = true)
		{
			foreach (var str in input)
			{
				try
				{
					Console.WriteLine(str);
					Expression expression = str.Parse();

					Console.WriteLine("Result: " + expression.Value);
#if PRINT_JSON
#if TO_FILE
					System.IO.File.WriteAllText($"{DateTime.Now.Ticks}.json", str + "\n" + expression.ToJson() + "\n" + "Result: " + expression.Calculate());
#else
					Console.WriteLine(expression + ":\nJSON:\n" + expression.ToJson(skipEmpty));
#endif
#endif
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

		///  <summary>
		///  © Copyright by Alexey Iskhakov
		///  Expression generator.
		///  </summary>
		///  <param name="amount">Amount of expressions to generate</param>
		///  <param name="lengthLimit">Approximate limit of characters per expression</param>
		///  <param name="fullyRandom">Whether the generator should produce absolutely random symbols</param>
		///  <param name="sepWithSpace">Whether to insert random spaces between symbols</param>
		/// <param name="incLog">Whether to include Logical operators</param>
		/// <param name="incRel">Whether to include Relational operators</param>
		/// <param name="incDiv">Whether to include division</param>
		/// <returns></returns>
		private static string[] GenerateTests(int amount = 1, int lengthLimit = 30, bool fullyRandom = false,
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
	}
}