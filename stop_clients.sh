#!/bin/bash

echo "Stopping all RPA Clients..."

# Kill all dotnet processes running RpaClient
pkill -f "dotnet run --project RpaClient"

echo "All RPA clients stopped."
