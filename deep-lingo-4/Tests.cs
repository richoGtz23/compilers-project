/*
Luis Ricardo Gutierrez A01376121
Josep Romagosa A01374637
 */
using System;
using System.IO;

namespace DeepLingo {

	public class ScannerTest {

		public void RunTests () {

				TestFile ("./example-programs/arrays.deep", "Arrays");
				TestFile ("./example-programs/binary.deep", "Binary");
				TestFile ("./example-programs/literals.deep", "Literals");
				TestFile ("./example-programs/next_day.deep", "Next Day");
				TestFile ("./example-programs/palindrome.deep", "Palindrome");
				TestFile ("./example-programs/ultimate.deep", "Ultimate");
				TestFile ("./example-programs/factorial.deep", "Factorial");
				Console.WriteLine($"You passed 7 out of 7 tests.");
			
		}

		public void TestFile (string inputFile, string name) {
			Console.WriteLine ($"Running {name}, ==Sit tight==");
			var input = File.ReadAllText (inputFile);
			var count = 1;
			foreach (var tok in new Scanner (input).Start ()) {
				//Console.WriteLine (String.Format ("[{0}] {1}",count++, tok));
			}
			var parser = new Parser (new Scanner (input).Start ().GetEnumerator ());
            var program = parser.Program();
            Console.Write(program.ToStringTree());
            var semantic = new SemanticAnalyzer();
            semantic.Visit((dynamic)program);
            Console.WriteLine("Semantics OK.");
            Console.WriteLine();
            Console.WriteLine("Symbol Table of Functions");
            Console.WriteLine("============");
            foreach (var entry in semantic.functionsTable)
            {
                Console.WriteLine(entry);
            }
            Console.WriteLine("Symbol Table of Global Variables");
            Console.WriteLine("============");
            foreach (var entry in semantic.globalVariables)
            {
                Console.WriteLine(entry);
            }

		}

	}

}