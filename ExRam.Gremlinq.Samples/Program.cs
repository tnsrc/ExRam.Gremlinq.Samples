﻿using System;
using System.Linq;
using System.Threading.Tasks;
using ExRam.Gremlinq.Core;
using ExRam.Gremlinq.Providers;
using ExRam.Gremlinq.Providers.WebSocket;
using static ExRam.Gremlinq.Core.GremlinQuerySource;

namespace ExRam.Gremlinq.Samples
{
    class Program
    {
        private readonly IConfigurableGremlinQuerySource _g;

        public Program()
        {
            _g = g
                //Since the Vertex and Edge classes contained in this sample implement IVertex resp. IEdge,
                //setting a model is actually not required as long as these classes are discoverable (i.e. they reside
                //in a currently loaded assembly). We explicitly set a model here anyway.
                .WithModel(GraphModel.FromBaseTypes<Vertex, Edge>(x => x.Id, x => x.Id))
                //Configure Gremlinq to work on a locally running instance of Gremlin server.
                .WithRemote("localhost", GraphsonVersion.V3);

            //Uncomment below and comment above to configure Gremlinq to work on CosmosDB!
            //_g = g
            //    .WithCosmosDbRemote(hostname, database, graphName, authKey);
        }

        public async Task CreateGraph()
        {
            // Uncomment to delete the whole graph on every run.
            //await _g.V().Drop().ToArray();

            var marko = await _g
                .AddV(new Person { Name = "Marko", Age = 29 })
                .First();

            var vadas = await _g
                .AddV(new Person { Name = "Vadas", Age = 27 })
                .First();
            
            var josh = await _g
                .AddV(new Person { Name = "Josh", Age = 32 })
                .First();

            var peter = await _g
                .AddV(new Person { Name = "Peter", Age = 29 })
                .First();

            var lop = await _g
                .AddV(new Software { Name = "Lop", Language = ProgrammingLanguage.Java })
                .First();

            var ripple = await _g
                .AddV(new Software { Name = "Ripple", Language = ProgrammingLanguage.Java })
                .First();

            await _g
                .V(marko.Id)
                .AddE<Knows>()
                .To(__ => __.V(vadas.Id))
                .First();

            await _g
                .V(marko.Id)
                .AddE<Knows>()
                .To(__ => __.V(josh.Id))
                .First();

            await _g
                .V(marko.Id)
                .AddE<Created>()
                .To(__ => __.V(lop.Id))
                .First();

            await _g
                .V(josh.Id)
                .AddE<Created>()
                .To(__ => __.V(ripple.Id))
                .First();

            await _g
                .V(josh.Id)
                .AddE<Created>()
                .To(__ => __.V(lop.Id))
                .First();

            await _g
                .V(peter.Id)
                .AddE<Created>()
                .To(__ => __.V(lop.Id))
                .First();
        }

        public async Task CreateKnowsRelationInOneQuery()
        {
            await _g
                .AddV(new Person { Name = "Bob", Age = 36 })
                .AddE<Knows>()
                .To(__ => __
                    .AddV(new Person { Name = "Jeff", Age = 27 }))
                .First();
        }

        public async Task WhoDoesMarkoKnow()
        {
            var knownPersonsToMarko = await _g
                .V<Person>()
                .Where(x => x.Name == "Marko")
                .Out<Knows>()
                .OfType<Person>()
                .OrderBy(x => x.Name)
                .Values(x => x.Name)
                .ToArray();

            Console.WriteLine("Who does Marko know?");

            foreach (var person in knownPersonsToMarko)
            {
                Console.WriteLine($" Marko knows {person}.");
            }

            Console.WriteLine();
        }

        public async Task WhoIsOlderThan30()
        {
            var personsOlderThan30 = await _g
                .V<Person>()
                .Where(x => x.Age > 30)
                .ToArray();

            Console.WriteLine("Who is older than 30?");

            foreach (var person in personsOlderThan30)
            {
                Console.WriteLine($" {person.Name} is older than 30.");
            }

            Console.WriteLine();
        }

        public async Task WhoseNameStartsWithB()
        {
            var nameStartsWithB = await _g
                .V<Person>()
                .Where(x => x.Name.StartsWith("B"))
                .ToArray();

            Console.WriteLine("Whose name starts with 'B'?");

            foreach (var person in nameStartsWithB)
            {
                Console.WriteLine($" {person.Name}'s name starts with a 'B'.");
            }

            Console.WriteLine();
        }

        public async Task WhoKnowsWho()
        {
            var friendTuples = await _g
                .V<Person>()
                .As((__, person) => __
                    .Out<Knows>()
                    .OfType<Person>()
                    .As((___, friend) => ___
                        .Select(person, friend)))
                .ToArray();

            Console.WriteLine("Who knows who?");

            foreach (var tuples in friendTuples)
            {
                Console.WriteLine($" {tuples.Item1.Name} knows {tuples.Item2.Name}.");
            }

            Console.WriteLine();
        }

        static async Task Main()
        {
            var program = new Program();

            await program.CreateGraph();
            await program.CreateKnowsRelationInOneQuery();
            await program.WhoDoesMarkoKnow();
            await program.WhoIsOlderThan30();
            await program.WhoseNameStartsWithB();
            await program.WhoKnowsWho();

            Console.Write("Press any key...");
            Console.ReadLine();
        }
    }
}
