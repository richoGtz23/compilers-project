using System;

namespace Test
{
    class test 
    {
        string x = "lol";
        static void Main() 
        {   
            string y,z;
            Console.WriteLine("Hello World!");
            Lol(1,2);
            y = "lol2, ";
            z = "lol3"; 
            Console.WriteLine(y.TrimEnd(new Char[]{',',' '}));
        }
        static void Lol(int b, int c){
            int a = b + c;
            Console.WriteLine(a);
        }
    }
}