using System;

namespace Test
{
    class test 
    {
        static void Main() 
        {   
            int a = 5;
            int b = 6;
            if(a >= b){
                Lol(a,b);
            }

        }
        static void Lol(int b, int c){
            int a = b + c;
            Console.WriteLine(a);
        }
    }
}