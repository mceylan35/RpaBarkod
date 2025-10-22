#!/bin/bash

# Raboid RPA System - Start All Components Script

echo "=== Starting Raboid RPA System ==="

# Start MongoDB
echo "Starting MongoDB..."
docker-compose up -d

# Wait for MongoDB to be ready
echo "Waiting for MongoDB to start..."
sleep 10

# Start RPA API
echo "Starting RPA API..."
cd RpaApi
dotnet run & API_PID=$!
cd ..

# Wait for API to start
echo "Waiting for API to start..."
sleep 10

# Start RPA Scheduler
echo "Starting RPA Scheduler..."
cd RpaScheduler
dotnet run & SCHEDULER_PID=$!
cd ..

# Wait for Scheduler to start
echo "Waiting for Scheduler to start..."
sleep 5

echo "=== All components started ==="
echo "API PID: $API_PID"
echo "Scheduler PID: $SCHEDULER_PID"
echo ""
echo "To stop all components, run: ./stop_all.sh"

# Keep script running
wait $API_PID $SCHEDULER_PID