using System.Threading.Tasks.Dataflow;
using SynchronizationDemo.Domain;

var semaforo = new SemaphoreExperiment();
var ProducersConsumers = new ProducerConsumerExperiment(); 

bool termina = false;

while (!termina)
{
    Console.WriteLine();
    Console.WriteLine("=== SYNCHRONIZATION DEMO ===");
    Console.WriteLine("1. Semaforo");
    Console.WriteLine("2. Producers Consumers");
    Console.WriteLine("0. Esci");
    Console.Write("Scelta: ");

    switch (Console.ReadLine())
    {
        case "1":
            semaforo.Run();
            break;

        case "2":
            ProducersConsumers.Run();
            break;

        case "3":
            break;

        case "0":
            termina = true;
            break;

        default:
            Console.WriteLine("Scelta non valida.");
            break;
    }
}

Console.WriteLine("SynchronizationDemo terminato.");