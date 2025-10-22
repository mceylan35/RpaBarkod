#!/bin/bash

# Raboid RPA System - Stop All Components Script

echo "=== Stopping Raboid RPA System ==="

# Find and kill dotnet processes
echo "Stopping RPA API and Scheduler..."
pkill -f "dotnet.*RpaApi"
pkill -f "dotnet.*RpaScheduler"

# Stop MongoDB
echo "Stopping MongoDB..."
docker-compose down

echo "=== All components stopped ==="