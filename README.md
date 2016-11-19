# ExpressionCalculator
A simple extendable parser &amp; calculator of arithmetic expressions, written in C# as an assignment for "Programming Paradigms" course of Innopolis University.

## Usage example

```C#
using ExpressionCalc;

...
string input = "(2 * (-2) + (128/32) - 6) >= 0 || 1";

try
{
	Expression expression = input.Parse();
}
catch (ParseException e)
{
	Console.WriteLine(e);
}

long result = expression.Value;
string initialExpression = expression.ToString();
string json = expression.ToJson();
```
