using System;

namespace OrderTrackingExample
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Enter your Order Number");
            var number = Console.ReadLine();
            string notes = null;

            var order = new Order(number);
            printOrderState(order);

            
            if (setNote(out notes))
                order.RecordNote(notes);

            printOrderState(order);

            order.Accept();
            printOrderState(order);

            if (setNote(out notes))
                order.RecordNote(notes);

            printOrderState(order);

            order.Update();
            printOrderState(order);

            order.Update();
            printOrderState(order);

            order.Update();
            printOrderState(order);

            order.UpdateData(" here is the new data");
            printOrderState(order);
            
            order.UpdateData(" another update to the data");
            printOrderState(order);

            if (setNote(out notes))
                order.RecordNote(notes);

            order.Cancel("Saeed");
            printOrderState(order);

            if (setNote(out notes))
                order.RecordNote(notes);

            printOrderState(order);

            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("");

            Console.WriteLine("==================================");
            Console.WriteLine("Those are the states of the Order:");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("**-----**");
            foreach (var state in order.States)
            {
                Console.WriteLine(state.ToString());
                Console.WriteLine("**-----**");
                Console.WriteLine("");
            }
            Console.ReadLine();
        }


        private static bool setNote(out string notes)
        {
            Console.WriteLine("Do you have notes? if No just press Enter");
            notes = Console.ReadLine();
            return !string.IsNullOrWhiteSpace(notes);
        }


        private static void printOrderState(Order order)
        {
            Console.WriteLine("+++ Order State +++");
            Console.WriteLine(order.CurrentState);
            Console.WriteLine("+++++++++++++++++++");
            Console.WriteLine("");
            Console.WriteLine("");
        }
    }
}

