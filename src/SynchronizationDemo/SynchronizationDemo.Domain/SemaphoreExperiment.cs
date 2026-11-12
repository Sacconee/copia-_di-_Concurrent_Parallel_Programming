namespace SynchronizationDemo.Domain;
/*
Immaginiamo quattro lavoratori che vogliono utilizzare una stampante.
La stampante può essere usata da un solo lavoratore alla volta.
Creiamo un semaforo con un solo permesso:
Il semaforo contiene inizialmente un permesso disponibile


- `Wait()` acquisisce un permesso, oppure attende se non ce ne sono.
- `Release()` restituisce un permesso.
- `maxCount: 1` impedisce che il conteggio del semaforo superi uno.


*/

public class SemaphoreExperiment
{
    public void Run()
    {
        // Risorsa condivisa tra thread
        // Nel primo caso, avremo solo un permesso di entrata, dunque un lavoratore alla volta.
        // 1° parametro:  initialCount, quanti inizialmente hanno accesso 
        // 2° parametro:  maxCount, quali lavoratori al massimo possono accedere tramite il permesso 
        var stampante = new SemaphoreSlim(1, 1);

        // lavoratori che accederanno tramite permessi alla risorsa condivisa
        Thread[] lavoratori = new Thread[4]; 
        int idLavoratore = 0;

        // Accesso alla risorsa condivisa dai vari lavoratori 
        for (int i = 0; i <= lavoratori.Length; i++)
        {
            idLavoratore = i; // 1, 2, 3, 4 ..
            lavoratori[i] = new Thread(() => {
                Console.WriteLine($"ID: {idLavoratore}, vuole accedere alla risorsa condivisa");
                stampante.Wait();


                Console.WriteLine($"-- ID: {idLavoratore}, ottiene alla risorsa condivisa");
                Thread.Sleep(1500);

                Console.WriteLine($"ID: {idLavoratore}, rilascio la risorsa condivisa");

                stampante.Release();
            });
        }


        // Starto tutti i lavoratori 
        foreach(Thread lavoratore in lavoratori)
        {
            lavoratore.Start();
        }
        
        // Attendo il termine di tutti i lavaoratori 
        foreach(Thread lavoratore in lavoratori)
        {
            lavoratore.Join();
        }

        // quando tutti hanno finito, il main thread stamperà questa riga
        Console.WriteLine("Tutti i lavoratori hanno termianto l'accesso alla stampante");
    }
}
