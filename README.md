# durable

Proof of concept project, trying to map an existing data correction process flow with long-running database backend task to an Azure Durable Functions backend.
Front end is not yet in scope, but will need to be designed as well. Options are Angular or ASP.NET.

The frontend is now simulated as a powershell script (runCorrection.ps1)

Prerequisites:
- dotnet sdk 6+ 
- azure function core tools 4+
- VSCode with Azure functions and Azurite extension

Usage of this POC:
- Make sure Azurite is running to simulate storage account.
- Run the function app.
- Run the runCorrection.ps1 script to simulate an operation.
