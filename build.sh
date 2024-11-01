#!/bin/bash

# Exit on any error
set -e

# Clean the solution
dotnet clean

# Restore the solution
dotnet restore

# Build the solution
dotnet build

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Generate the coverage report
reportgenerator -reports:./**/coverage.cobertura.xml -targetdir:coverage-report -reporttypes:Html

# Open the coverage report (optional)
xdg-open coverage-report/index.html