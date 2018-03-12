using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TspLibNet.TSP;

namespace TabuSearch
{
    class Program
    {
        static void Main(string[] args)
        {
            var parser = new DataLoader();
            Ts solver;
            Data data = null;
            var timeLimit = new int();
            while (true)
            {
                Console.Clear();
                Console.WriteLine(
                    $"\n1.Wczytanie danych z pliku\n2.Wprowadzenie kryterium stopu\n3.Uruchom algorytm");
                var decision = Convert.ToInt32(Console.ReadLine());
                Console.Clear();
                switch (decision)
                {
                    case 1:
                        Console.WriteLine($"Podaj nazwe pliku: ");
                        var fileName = Console.ReadLine();
                        data = new Data();
                        parser.LoadData(out data.TspArray, out data.optimal, out data.Cities, fileName);
                        Console.Clear();
                        break;
                    case 2:
                        Console.WriteLine($"Ile czasu ma dzialac algorytm [s]: ");
                        var time = Convert.ToInt32(Console.ReadLine());
                        timeLimit = time;
                        Console.Clear();
                        break;
                    case 3:
                        solver = new Ts(data);
                        solver.TimeLimit = timeLimit;
                        solver.Solve();
                        Console.ReadKey();
                        break;
                    
                    default:
                        Console.WriteLine($"Nie ma takiej opcji");
                        Console.ReadKey();
                        break;

                }
            }

            
        }
    }
}
