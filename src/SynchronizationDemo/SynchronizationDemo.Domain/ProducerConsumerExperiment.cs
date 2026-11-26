using System.Runtime.InteropServices;
using System.Runtime.Serialization;
public class ProducerConsumerExperiment
{
    private const int BufferSize = 4; // la grandezza della tavola in cui mettere i piatti con il cibo
    private const int NProducers = 2; 
    private const int NConsumers = 2; 
    private const int NumeroRichieste = 20; // numero massimo di richieste 
    private const int stopExecution = -1; // far terminare l'esecuzione del software, in quanto altrimenti tutti i consumers 
    // e tutti i producers continuerebbero all'infinito a fare il loro lavoro. 

    public void Run()
    {
        // Mi serve una struttura dati per posizionare i prodotti creati e consumati 
        Queue<int> buffer = new();
        var spaziDisponibili = new SemaphoreSlim(BufferSize, BufferSize);
        var elementiDisponibili = new SemaphoreSlim(0, BufferSize);
        var accessoTavola = new SemaphoreSlim(1,1);

        Thread[] produttori = new Thread[NProducers];
        Thread[] consumatori = new Thread[NConsumers];

        int nextID = 0;
        int articoliConsumati = 0; // numero articoli consumati totali durante l'esecuzione fino allo stopExecution.
        int articoliProdotti = 0; // numero articoli prodotti totali durante l'esecuzione fino allo stopExecution.


        // Satrt dei consumatori e produttori 
        for(int i = 0; i < NConsumers; i++)
        {
            int id = i + 1; 
            // i corrisponde all'id del thread corrente 
            consumatori[i] = new Thread(() => ConsumeSomeProduct(
                id,
                buffer,
                spaziDisponibili,
                elementiDisponibili,
                accessoTavola,
                ref articoliConsumati // passaggio per riferimento (in memoria), anziché per copia
            ));
            consumatori[i].Start();
        }

        for(int i = 1; i < NProducers; i++)
        {
            int id = i + 1;
            // i corrisponde all'id del thread corrente 
            produttori[i] = new Thread(() => ProduceSomeProduct(
                id,
                buffer,
                spaziDisponibili,
                elementiDisponibili,
                accessoTavola,
                ref nextID,
                ref articoliProdotti // passaggio per riferimento (in memoria), anziché per copia
            ));
            produttori[i].Start();
        }

        // attendo con il main tgread (thread principale) che tutti i produttori terminino la loro esecuzione
        foreach(Thread produttore in consumatori)
        {
            produttore.Join();
        }

        // gestibili più facilmente solo con le Join(), ma in questo modo ho una garanzia maggiore.
        // Ogni consumatore deve ricevere il proprio stop, altrimenti andrebbe in attesa all'infinito.
        for(int i = 0; i < NConsumers; i++)
        {
            //richiedo lo spazio
            spaziDisponibili.Wait();

            accessoTavola.Wait();
        
            try
            {
                buffer.Enqueue(stopExecution);
                Console.WriteLine("MAIN THREAD: ho inserito una guardia/sentinella");
            }
            finally
            {
                accessoTavola.Release();
            }

            // risveglio un consumatore che attende un elemento da consumare
            elementiDisponibili.Release();
        }

        // Attendo terminazione dei consumatori e dei produttori
        foreach(Thread consumatore in consumatori)
        {
            consumatore.Join();
        }

        Console.WriteLine("FINAL RESULT: \n");
        Console.WriteLine($"Richieste prodotte: {articoliProdotti}");
        Console.WriteLine($"Richieste consumate: {articoliConsumati}");
    }

    private static void ProduceSomeProduct(
        int id,
        Queue<int> buffer, 
        SemaphoreSlim spaziDisponibili,
        SemaphoreSlim elementiDisponibili,
        SemaphoreSlim accessoTavola,
        ref int nextID,
        ref int articoliProdotti)
    {
        // Per sempre, finchè thread non viene killato o fermato per altri motivi di syncronization
        while(true)
        {
            // // per evitare il fatto che più Thread possano creare item identici, devo garantire l'unicità
            int foodItem = Interlocked.Increment(ref nextID); // è come fare la lock nel laboratorio "ThreadDemo"

            if(foodItem > NumeroRichieste)
                break;                

            // Attendo uno spazio libero nel buffer
            spaziDisponibili.Wait();

            try
            {

                // Se ho questo permesso, ovvero posso accedere alla tavola, allora riempio un posto della tavola
                try
                {
                    accessoTavola.Wait();  
                    buffer.Enqueue(foodItem); //aggiungo un elemento alla mia coda di produzione (tavola)
                    Interlocked.Increment(ref nextID);
                    Console.WriteLine($"ProducerID: {id}, Item: {foodItem}, Buffer count: {buffer.Count}");
                }
                finally
                {
                    accessoTavola.Release();
                }
            }
            catch
            {
                spaziDisponibili.Release();
            }

            // Segnalare che è possibile d'ora in poi poter accedere a nuovi foodItens ai consumatori.
            elementiDisponibili.Release();
            Thread.Sleep(Random.Shared.Next(100, 250)); // i producer aspetteranno tempi diversi. 
        }
    }

    private static void ConsumeSomeProduct(
        int id, 
        Queue<int> buffer, 
        SemaphoreSlim spaziDisponibili, 
        SemaphoreSlim elementiDisponibili, 
        SemaphoreSlim accessoTavola, 
        ref int articoliConsumati)
    {
        // voglio sempre consumare finché posso
        while (true)
        {
            // evita di accedere alla coda quando è vuota 
            elementiDisponibili.Wait();
            int item;
            // rendo mutualmente esclusivo l'accesso alla tavola
            accessoTavola.Wait();

            // tentare di prelevare dalla coda (tavola) un elemento (numero, in un caso reale può essere una brioche)
            try
            {
                item = buffer.Dequeue();
                Console.WriteLine($"Consumato elemento: {item} da consumerId: {id}");
            }
            finally
            {
                // rilascio l'accesso alla tavola dopo aver consumato.
                accessoTavola.Release();
            }

            // rilascio semaforo dopo aver eventualmente consumato. 
            spaziDisponibili.Release();

            if(item == stopExecution)
            {
                Console.WriteLine($"ConsumerId: {id}, termina");
                return;
            }

            Thread.Sleep(Random.Shared.Next(100, 250)); // i consumer aspettano tempi diversi. 
            Interlocked.Increment(ref articoliConsumati); 
            Console.WriteLine($"consumerId: {id}, termina");
        }
    }
}