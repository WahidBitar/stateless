using System;

namespace OrderTrackingExample
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Enter your Order Number");
            var number = Console.ReadLine();
            var order = new Order(number);
            Console.WriteLine("Do you have notes? if No just press Enter");
            var notes = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(notes))
                order.RecordNote(notes);

            order.Accept();
            order.Complete("soso");

            Console.ReadLine();
        }
    }
}

