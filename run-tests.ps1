#!/usr/bin/env pwsh

# Script para ejecutar Unit Tests del Proyecto PokemonTCG
# Ubicación: F:\PokemonTCG

param(
	[ValidateSet("all", "repository", "service", "verbose", "coverage")]
	[string]$TestType = "all",

	[switch]$ShowOutput,
	[switch]$WatchMode
)

$projectRoot = "F:\PokemonTCG"
$testProject = "PokemonTCG.Test\PokemonTCG.Test.csproj"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "PokemonTCG - Unit Tests Runner" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Cambiar al directorio del proyecto
Set-Location $projectRoot

# Compilar antes de ejecutar
Write-Host "🔨 Compilando proyecto..." -ForegroundColor Yellow
dotnet build $testProject --configuration Debug

if ($LASTEXITCODE -ne 0) {
	Write-Host "❌ Error en compilación" -ForegroundColor Red
	exit 1
}

Write-Host "✅ Compilación exitosa" -ForegroundColor Green
Write-Host ""

# Ejecutar tests según el tipo
switch ($TestType) {
	"repository" {
		Write-Host "🧪 Ejecutando tests de Repository..." -ForegroundColor Yellow
		$filter = "ClassName=PokemonTCG.Test.Repositories.CardRepositoryTests"
		dotnet test $testProject --filter $filter --verbosity normal
	}
	"service" {
		Write-Host "🧪 Ejecutando tests de Service..." -ForegroundColor Yellow
		$filter = "ClassName=PokemonTCG.Test.Services.CardServiceTests"
		dotnet test $testProject --filter $filter --verbosity normal
	}
	"verbose" {
		Write-Host "🧪 Ejecutando todos los tests (verbose)..." -ForegroundColor Yellow
		dotnet test $testProject --verbosity detailed --logger "console;verbosity=detailed"
	}
	"coverage" {
		Write-Host "🧪 Ejecutando tests con cobertura..." -ForegroundColor Yellow
		dotnet test $testProject --collect:"XPlat Code Coverage" --verbosity normal
	}
	default {
		Write-Host "🧪 Ejecutando todos los tests..." -ForegroundColor Yellow
		dotnet test $testProject --verbosity normal
	}
}

$testResult = $LASTEXITCODE

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan

if ($testResult -eq 0) {
	Write-Host "✅ TODOS LOS TESTS PASARON" -ForegroundColor Green
} else {
	Write-Host "❌ ALGUNOS TESTS FALLARON" -ForegroundColor Red
	Write-Host "Código de salida: $testResult" -ForegroundColor Red
}

Write-Host "========================================" -ForegroundColor Cyan

exit $testResult
