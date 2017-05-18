﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RedHttpServerNet45;
using RedHttpServerNet45.Plugins;
using RedHttpServerNet45.Plugins.Interfaces;
using RedHttpServerNet45.Response;

namespace TestServerNET45
{
    class Program
    {
        static void Main(string[] args)
        {

            // We serve static files, such as index.html from the 'public' directory
            var server = new RedHttpServer(5000, "public");
            var startTime = DateTime.UtcNow;

            // We log to terminal here
            var logger = new TerminalLogging();
            server.Plugins.Register<ILogging, TerminalLogging>(logger);

            // URL param demo
            server.Get("/:param1/:paramtwo/:somethingthird", async (req, res) =>
            {
                await res.SendString($"URL: {req.Params["param1"]} / {req.Params["paramtwo"]} / {req.Params["somethingthird"]}");
            });

            // Redirect to page on same host
            server.Get("/redirect", async (req, res) =>
            {
                await res.Redirect("/redirect/test/here");
            });



            // Save uploaded file from request body 
            Directory.CreateDirectory("./uploads");
            server.Post("/upload", async (req, res) =>
            {
                if (await req.SaveBodyToFile("./uploads"))
                {
                    await res.SendString("OK");
                    // We can use logger reference directly
                    logger.Log("UPL", "File uploaded");
                }
                else
                    await res.SendString("Error", status: 413);
            });

            server.Get("/file", async (req, res) =>
            {
                await res.SendFile("testimg.jpeg");
            });

            // Handling formdata from client
            server.Post("/formdata", async (req, res) =>
            {
                var form = await req.GetFormDataAsync();
                await res.SendString("Hello " + form["firstname"][0]);
            });

            // Using url queries to generate an answer
            server.Get("/hello", async (req, res) =>
            {
                var queries = req.Queries;
                var firstname = queries["firstname"];
                var lastname = queries["lastname"];
                await res.SendString($"Hello {firstname} {lastname}, have a nice day");
            });

            // Rendering a page for dynamic content
            server.Get("/serverstatus", async (req, res) =>
            {
                await res.RenderPage("./pages/statuspage.ecs", new RenderParams
                {
                    { "uptime", DateTime.UtcNow.Subtract(startTime).TotalHours },
                    { "versiom", RedHttpServer.Version }
                });
            });

            // WebSocket echo server
            server.WebSocket("/echo", (req, wsd) =>
            {
                // Or we can use the logger from the plugin collection 
                wsd.ServerPlugins.Use<ILogging>().Log("WS", "Echo server visited");

                wsd.SendText("Welcome to the echo test server");
                wsd.OnTextReceived += (sender, eventArgs) =>
                {
                    wsd.SendText("you sent: " + eventArgs.Text);
                };
            });


            server.Start(true);
            Console.ReadKey();
        }
    }
}
