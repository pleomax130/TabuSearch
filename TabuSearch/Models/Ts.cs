using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TabuSearch
{
    class Ts
    {
        public Data Data { get; private set; }
        public List<int> BestSolution { get; set; } // Najlepsze rozwiązanie
        public List<int> CurrentSolution { get; set; }  // Aktualne rozwiązanie
        public List<TabuElement> TabuList { get; set; } // Lista tabu
        public int Cadency { get; set; } = 10;  //Kadencja
        public int TimeLimit { get; set; } = 60;    // Warunek zakonczenia [s]
        public int Cost { get; set; }
        public int Cost5 { get; set; }
        public int Cost15 { get; set; }
        public int Cost30 { get; set; }
        public Stopwatch Sw { get; set; }
        private List<int> SetFirstSolution()    // Funkcja generująca pierwsze rozwiązanie
        {
            var list = new List<int> {0};
            var list1 = new List<int>();
            for (int i = 1; i < Data.TspArray.Length; i++)
            {
                list1.Add(i);
            }
            var r = new Random();
            for (int i = 0; i < Data.Cities-1; i++)
            {
                var z = r.Next(list1.Count);
                list.Add(list1[z]);
                list1.RemoveAt(z);
            }
            return list;
        }

        private int CalculateCost(List<int> list)
        {
            var cost = 0;
            for (int i = 0; i < list.Count - 1; i++)
                cost += Data.TspArray[list[i]][list[i + 1]];
            cost += Data.TspArray[list[list.Count - 1]][list[0]];
            return cost;
        }

        private bool Aspiration(Edge edge)  // Funkcja sprawdzająca kryterium aspiracji
        {
            var better = false;
            if (TabuList.Count == 0) return false;  // Jesli lista tabu jest pusta to nie ma potrzzeby sprawdzania
            var x = TabuList[0];
            var min = Data.TspArray[x.Edge.Start][x.Edge.End];
            foreach (var tabuElement in TabuList)   // Szukanie krawedzi o najmniejszym koszcie
            {
                if((edge.Start!=tabuElement.Edge.Start && edge.End!=tabuElement.Edge.End) && (edge.Start != tabuElement.Edge.End && edge.End != tabuElement.Edge.Start))    // Upewnienie sie ze krawedzie ze soba nie sasiaduja
                    if (Data.TspArray[tabuElement.Edge.Start][tabuElement.Edge.End] < min)
                    {
                        x = tabuElement;
                        min = Data.TspArray[x.Edge.Start][x.Edge.End];
                    }

            }
            if ((edge.Start == x.Edge.Start || edge.End == x.Edge.End) || (edge.Start == x.Edge.End || edge.End == x.Edge.Start)) return false; // Upewnienie sie ze krawedzie ze soba nie sasiaduja
            if (CompareEdges(edge, x.Edge)) // Sprawdzenie czy zamiana krawedzi podanej w argumencie z ta z listy tabu poprawi rozwiazanie
            better = true;
            return better;
        }
        private void SwapEdges()    // Funkcja wybierajaca krawedzie do zamiany
        {
            
            Edge firstEdge = null, secondEdge = null;
            var r = new Random();
            while(Sw.ElapsedMilliseconds/1000<TimeLimit)    // Warunek zakonczenia algorytmu
            {
                var diff = false;
                while (!diff)   // Petla ktora wykonuje sie dopoki nie uzyskamy krawedzi ze soba nie sasiadujacych 
                {
                    var neigh = true;
                    var z = r.Next(0, Data.Cities - 1);
                    firstEdge = z == Data.Cities - 1 ? new Edge(z, 0) : new Edge(z, z + 1); // Wybranie pierwszej krawedzi
                    if (!Aspiration(firstEdge)) // Sprawdzenie czy kryterium aspiracji dla tej krawedzi, jezeli nie spelnione wybierz druga krawedz w taki sam sposob jak pierwsza
                    {
                        z = r.Next(0, Data.Cities - 1);
                        secondEdge = z == Data.Cities - 1 ? new Edge(z, 0) : new Edge(z, z + 1);    // Wybranie drugiej krawedzi
                        if (firstEdge.Start == secondEdge.Start || firstEdge.Start == secondEdge.End) neigh = false;
                        if (firstEdge.End == secondEdge.Start || firstEdge.End == secondEdge.End) neigh = false;    // Jezeli krawedzie ze soba sasiaduja wykonaj petle jeszcze raz
                        foreach (var tabuElement in TabuList)   // Sprawdzenie czy ktoras z krawedzi nie znajduje sie na tabu liscie
                        {
                            if (tabuElement.Edge == firstEdge || tabuElement.Edge == secondEdge)
                                neigh = false;
                        }
                        if (neigh) diff = true;
                    }
                }
                CompareEdges(firstEdge, secondEdge);    // Wywolanie funkcji sprawdzajacej czy zamiana tych krawedzi polepszy rozwiazanie
            }
        }

        private bool CompareEdges(Edge firstEdge, Edge secondEdge)  // Funkcja sprawdzajaca czy zamiana dwoch krawedzi polepszy rozwiazanie 
        {
            var cost = CalculateCost(BestSolution); // Zapamietanie kosztu dotychczas najlepszego rozwiazania
            var list = CurrentSolution.ToList();    // Zapamietanie obecnego rozwiazania
            var edge = new Edge(list[firstEdge.Start], list[firstEdge.End]);    // Zapamietanie krawedzi ktora jest pod tymi indeksami, aby pozniej mozna bylo ja zamienic
            list[firstEdge.Start] = list[secondEdge.Start]; // Zamiana krawedzi
            list[firstEdge.End] = list[secondEdge.End];
            list[secondEdge.Start] = edge.Start;
            list[secondEdge.End] = edge.End;
            var cost1 = CalculateCost(list);    // Obliczenie kosztu otrzymanego rozwiazania
            var list1 = CurrentSolution.ToList();   // Zapamietanie obecnego rozwiazania
            var edge1 = new Edge(list1[firstEdge.Start], list1[firstEdge.End]); // Inna mozliwozc zamiany krawedzi
            list1[firstEdge.Start] = list1[secondEdge.End];
            list1[firstEdge.End] = list1[secondEdge.Start];
            list1[secondEdge.Start] = edge.End;
            list1[secondEdge.End] = edge.Start;
            var cost2 = CalculateCost(list1);   // Obliczenie kosztu otrzymanego rozwiazania
            if (cost1 < cost2)  // Jezeli pierwsze otrzymane rozwiazanie jest lepsze od drugiego
            {
                if (cost1 < cost)   // Jezeli rozwiazanie jest lepsze od dotychczas najlepszego
                {
                    if (TabuList.Count != 0)    // Jezeli tabu lista nie jest pusta zmniejsz kadencje kazdego elementu
                    {
                        for (int k = TabuList.Count - 1; k >= 0; k--)
                        {
                            TabuList[k].Cadency--;
                            if (TabuList[k].Cadency == 0)
                                TabuList.RemoveAt(k);
                        }
                    }
                        var tabu = new TabuElement(Cadency, edge);  // Dodanie pierwszej krawedzi z ruchu do tabu listy
                        TabuList.Add(tabu);
                    
                    CurrentSolution = list; // Zapisanie otzrymanego rozwiazania jako najlepsze i aktualne
                    BestSolution.Clear();
                    foreach (var j in CurrentSolution)
                    {
                        BestSolution.Add(j);
                    }
                    return true;
                }
            }
            else    // Jezeli drugie otrzymane rozwiazanie jest lepsze od pierwszego
            {
                if (cost2 < cost)   // Jezeli rozwiazanie jest lepsze od dotychczas najlepszego
                {
                    if (TabuList.Count != 0)    // Jezeli tabu lista nie jest pusta zmniejsz kadencje kazdego elementu
                    {
                        for (int k = TabuList.Count - 1; k >= 0; k--)
                        {
                            TabuList[k].Cadency--;
                            if (TabuList[k].Cadency == 0)
                                TabuList.RemoveAt(k);
                        }
                    }
                    
                        var tabu = new TabuElement(Cadency, edge1); // Dodanie pierwszej krawedzi z ruchu do tabu listy
                        TabuList.Add(tabu);
                    
                    CurrentSolution = list1;    // Zapisanie otzrymanego rozwiazania jako najlepsze i aktualne
                    BestSolution.Clear();
                    foreach (var j in CurrentSolution)
                    {
                        BestSolution.Add(j);
                    }
                    return true;
                }
            }
            return false;
        }
        public void Solve()
        {
            BestSolution = new List<int>();
            CurrentSolution = new List<int>();
            TabuList = new List<TabuElement>();
            Sw = new Stopwatch();
            Sw.Start();

            BestSolution = SetFirstSolution();  // Wygenerowanie pierwszego rozwiazania 
            foreach (var i in BestSolution)
            {
                CurrentSolution.Add(i);
            }
            SwapEdges();
            Cost = CalculateCost(BestSolution);
            Console.WriteLine($"\nkoszt{Cost}");
            foreach (var i in BestSolution)
            {
                Console.Write($"{i} ");
            }
            Sw.Stop();
            Console.WriteLine($"\nCzas: {Sw.Elapsed}");
        }

        public Ts(Data data)
        {
           
            Data = data;
        }
    }
}
