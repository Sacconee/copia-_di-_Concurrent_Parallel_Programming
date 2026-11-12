using ThreadDemo.Domain;

var experiments = new ThreadExperiments();

while (true)
{
    Console.WriteLine();
    Console.WriteLine("THREAD DEMO");
    Console.WriteLine("1. Esecuzione sequenziale");
    Console.WriteLine("2. Due thread concorrenti");
    Console.WriteLine("3. Proprietà del thread corrente");
    Console.WriteLine("4. Contatore senza lock");
    Console.WriteLine("5. Contatore con lock");
    Console.WriteLine("6. Thread in background");
    Console.WriteLine("7. Thread in foreground");
    Console.WriteLine("0. Esci");
    Console.Write("Scelta: ");

    string? choice = Console.ReadLine();

    switch (choice)
    {
        case "1":
            experiments.RunSequential();
        break;
        
        case "2":
        experiments.RunThreads();
        break;

        case "3":
        experiments.ShowThreadProperties();
        break;

        case "6":
            experiments.RunBackgroundThreadExperiment();
            break;

        case "7":
            experiments.RunForegroundThreadExperiment();
            break;

         case "4":
         case "5":
            var result = experiments.RunCounter(useLock: choice == "5");

             Console.WriteLine($"Atteso: {result.Expected}");
             Console.WriteLine($"Ottenuto: {result.Actual}");
             break;

        case "0":
            return;

        default:
            Console.WriteLine("Scelta non valida.");
            break;
    }

    Console.WriteLine("Premi INVIO per continuare...");
    Console.ReadLine();
}
