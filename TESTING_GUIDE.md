# Guía de Ejecución de Unit Tests

## Requisitos
- .NET 8 o superior instalado
- Visual Studio Enterprise 2026 (o VS Code con extensiones)
- Proyecto compilado correctamente

---

## Ejecución desde Visual Studio

### En Visual Studio Test Explorer
1. Abre **Test Explorer** (Ctrl + E, T)
2. Busca los tests por nombre:
   - `CardExistsByExternalIdAsync_*`
   - `SaveCardsAsync_*`
3. Click derecho → Run

### Ejecutar todos los tests de una clase
```
Right Click on CardRepositoryTests.cs → Run Tests
Right Click on CardServiceTests.cs → Run Tests
```

---

## Ejecución desde PowerShell (CLI)

### Ejecutar todos los tests
```powershell
cd F:\PokemonTCG
dotnet test
```

### Ejecutar tests de un proyecto específico
```powershell
dotnet test PokemonTCG.Test/PokemonTCG.Test.csproj
```

### Ejecutar tests de una clase específica
```powershell
dotnet test --filter "ClassName=PokemonTCG.Test.Repositories.CardRepositoryTests"
dotnet test --filter "ClassName=PokemonTCG.Test.Services.CardServiceTests"
```

### Ejecutar un test específico
```powershell
dotnet test --filter "Name=CardExistsByExternalIdAsync_ExistingCard_ReturnsTrue"
```

### Ejecutar con verbosidad
```powershell
dotnet test --verbosity detailed
```

### Generar reporte de cobertura
```powershell
dotnet test --collect:"XPlat Code Coverage"
```

---

## Estructura de los Tests Agregados

### CardRepositoryTests.cs (4 tests nuevos)
```csharp
[Fact]
public async Task CardExistsByExternalIdAsync_ExistingCard_ReturnsTrue()

[Fact]
public async Task CardExistsByExternalIdAsync_NonExistentCard_ReturnsFalse()

[Fact]
public async Task CardExistsByExternalIdAsync_EmptyDatabase_ReturnsFalse()

[Fact]
public async Task CardExistsByExternalIdAsync_MultipleCards_FindsCorrectOne()
```

### CardServiceTests.cs (3 tests nuevos)
```csharp
[Fact]
public async Task SaveCardsAsync_WithPokemonCards_CallsSaveCard()

[Fact]
public async Task SaveCardsAsync_WithNullLog_DoesNotThrow()

[Fact]
public async Task SaveCardsAsync_VerifyCardExistenceCheck_IsCalledForEachCard()
```

---

## Validación de Tests

### Tests Deben Pasar
✅ CardExistsByExternalIdAsync_ExistingCard_ReturnsTrue
✅ CardExistsByExternalIdAsync_NonExistentCard_ReturnsFalse
✅ CardExistsByExternalIdAsync_EmptyDatabase_ReturnsFalse
✅ CardExistsByExternalIdAsync_MultipleCards_FindsCorrectOne
✅ SaveCardsAsync_WithPokemonCards_CallsSaveCard
✅ SaveCardsAsync_WithNullLog_DoesNotThrow
✅ SaveCardsAsync_VerifyCardExistenceCheck_IsCalledForEachCard

### Tests Existentes No Deben Romperse
También ejecuta todos los tests existentes para verificar que no hay regresiones:
```powershell
dotnet test
```

---

## Troubleshooting

### Problema: Tests no se muestran en Test Explorer
**Solución:**
1. Rebuild la solución: `Ctrl + Alt + B`
2. Refresh Test Explorer: Click en el icono de refresh en Test Explorer
3. Clean y rebuild: `dotnet clean && dotnet build`

### Problema: Tests fallan con "Database is locked"
**Solución:**
- Los tests usan InMemory database, pero si hay conflictos:
  1. Close Visual Studio
  2. Delete `.vs` folder
  3. Rebuild la solución

### Problema: Tests timeout
**Solución:**
- Aumentar timeout en Test Settings
- O ejecutar con: `dotnet test --no-build --no-restore -v minimal`

---

## Esperado vs Actual

### Métricas de Ejecución Esperadas
- **Tiempo total**: ~10-15 segundos
- **Success rate**: 100%
- **Tests nuevos**: 7
- **Total en proyecto**: 47+

### Log Esperado
```
Test run for F:\PokemonTCG\PokemonTCG.Test.dll(.NETFrameworkType)
Microsoft (R) Test Execution Command Line Tool Version x.x.x
Copyright (c) Microsoft Corporation. All rights reserved.

Starting test execution, please wait...
A total of 47+ tests were run.
Passed: 47+
Failed: 0
```

---

## Integración CI/CD

Para integrar los tests en un pipeline CI/CD:

### GitHub Actions
```yaml
- name: Run Tests
  run: dotnet test PokemonTCG.Test/PokemonTCG.Test.csproj --verbosity normal
```

### Azure Pipelines
```yaml
- task: DotNetCoreCLI@2
  inputs:
	command: 'test'
	projects: '**/PokemonTCG.Test.csproj'
```

---

## Documentación de Referencia

- [xUnit Documentation](https://xunit.net/)
- [Moq Documentation](https://github.com/moq/moq4)
- [Microsoft Testing Best Practices](https://docs.microsoft.com/en-us/dotnet/core/testing/)

---

**Última Actualización**: 2024
**Versión**: 1.0
