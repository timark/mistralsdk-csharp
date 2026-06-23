.PHONY: help build test restore

SOLUTION=/home/runner/work/mistralsdk-csharp/mistralsdk-csharp/MistralSdk.slnx

help:
	@echo "Available targets:"
	@echo "  make restore        Restore .NET dependencies"
	@echo "  make build          Build the .NET 10 SDK"
	@echo "  make test           Run .NET tests"

restore:
	dotnet restore $(SOLUTION)

build: restore
	dotnet build $(SOLUTION) --configuration Release --no-restore

test: build
	dotnet test $(SOLUTION) --configuration Release --no-build
