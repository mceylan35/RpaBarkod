#!/bin/bash

echo "Starting 50 RPA Clients..."

# Create logs directory if it doesn't exist
mkdir -p logs

# Start 50 RPA clients in parallel
for i in {1..50}
do
    echo "Starting client $i..."
    dotnet run --project RpaClient test-client-$i > logs/client-$i.log 2>&1 &
done

echo "All 50 clients started. Check logs/ directory for individual client logs."
echo "To stop all clients, run: ./stop_clients.sh"
