namespace ExpressionCalc
{
	internal static class Program
	{
		private static void Main(string[] args)
		{
			string input = "(1 + 26 - 98) / 15 + 777 >= 772";

			if (args.Length > 0)
				input = args[0];
			

			input.Test(false);
			input.Test();

			#region Tests
			if (args.Length == 0)
			{
				Tests.Manual.Test();
//				Test(GenerateTests(10), false);
//				Tests.Generate(10).Test();
//				Test(GenerateTests(10, 1000, incRel: false, incLog: false, incDiv: false));
//				Test(GenerateTests(10, 15, fullyRandom: true));
			}
			#endregion
		}
	}
}