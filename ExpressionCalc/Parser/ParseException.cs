using System;

namespace ExpressionCalc
{
	class ParseException : Exception
	{
		public ParseException(string input, Type place, string description = "") : base(input)
		{
			At = input;
			ErrorPlace = place;
			Message = place.Name + " parse error in \"" + input + "\" :\n\t"
			              + description + "\n";
		}

		public virtual string At { get; }
		public virtual Type ErrorPlace { get; }
		public override string Message { get; }

		public override string ToString() => this.Message;
	}

	class SyntaxException : ParseException
	{
		public SyntaxException(string input, Type place, string description = "")
			: base(input, place, "Syntax error!\n\t" + description) { }
	}

	class BracesException : SyntaxException
	{
		public BracesException(string input, Type place)
			: base(input, place, "Braces mismatch!") { }
	}

	class OperatorException : SyntaxException
	{
		public OperatorException(string input, Type place)
			: base(input, place, "Operator expected!") { }
	}

	class CalculationException : ParseException
	{
		public CalculationException(string input, Type place, string description = "")
			: base(input, place, "Calculational error!\n\t" + description) { }
	}
}