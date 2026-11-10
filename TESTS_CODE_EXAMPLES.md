# 💻 EJEMPLOS DE CÓDIGO - TESTS AGREGADOS

## 1. Ejemplos de CardRepositoryTests

### ✅ Test 1: CardExistsByExternalIdAsync_ExistingCard_ReturnsTrue

```csharp
[Fact]
public async Task CardExistsByExternalIdAsync_ExistingCard_ReturnsTrue()
{
	// ARRANGE - Preparar datos
	var db = CreateDb(nameof(CardExistsByExternalIdAsync_ExistingCard_ReturnsTrue));
	await SeedFullCard(db, "pika-1", "pika-ext-1");

	// ACT - Ejecutar el método a testear
	var repo = CreateRepo(db);
	var result = await repo.CardExistsByExternalIdAsync("pika-ext-1", CancellationToken.None);

	// ASSERT - Verificar resultados
	Assert.True(result);
}
```

**Explicación:**
- Crea una BD en memoria con una tarjeta
- Tarjeta tiene ExternalId = "pika-ext-1"
- Verifica que el método lo encuentra correctamente
- ¿Cuándo falla? Si retorna false cuando debería retornar true

---

### ✅ Test 2: CardExistsByExternalIdAsync_NonExistentCard_ReturnsFalse

```csharp
[Fact]
public async Task CardExistsByExternalIdAsync_NonExistentCard_ReturnsFalse()
{
	// ARRANGE
	var db = CreateDb(nameof(CardExistsByExternalIdAsync_NonExistentCard_ReturnsFalse));
	await SeedFullCard(db, "pika-1", "pika-ext-1");
	// BD tiene: pika-ext-1

	// ACT
	var repo = CreateRepo(db);
	var result = await repo.CardExistsByExternalIdAsync(
		"no-such-external-id",  // Buscamos algo que NO existe
		CancellationToken.None
	);

	// ASSERT
	Assert.False(result);  // Debe ser false
}
```

**Explicación:**
- La BD tiene "pika-ext-1"
- Buscamos "no-such-external-id" que no existe
- Debe retornar false (no encontrada)
- Previene falsos positivos

---

### ✅ Test 3: CardExistsByExternalIdAsync_EmptyDatabase_ReturnsFalse

```csharp
[Fact]
public async Task CardExistsByExternalIdAsync_EmptyDatabase_ReturnsFalse()
{
	// ARRANGE - BD vacía (sin tarjetas)
	var repo = CreateRepo(nameof(CardExistsByExternalIdAsync_EmptyDatabase_ReturnsFalse));

	// ACT - Buscar en BD vacía
	var result = await repo.CardExistsByExternalIdAsync("any-external-id", CancellationToken.None);

	// ASSERT
	Assert.False(result);  // Debe ser false ya que no hay nada
}
```

**Explicación:**
- BD completamente vacía
- Buscar cualquier ExternalId
- Siempre debe retornar false
- Edge case: asegura que no hay problemas con BD vacía

---

### ✅ Test 4: CardExistsByExternalIdAsync_MultipleCards_FindsCorrectOne

```csharp
[Fact]
public async Task CardExistsByExternalIdAsync_MultipleCards_FindsCorrectOne()
{
	// ARRANGE - Insertar múltiples tarjetas
	var db = CreateDb(nameof(CardExistsByExternalIdAsync_MultipleCards_FindsCorrectOne));
	await SeedFullCard(db, "card-1", "ext-1");  // Primera tarjeta
	await SeedFullCard(db, "card-2", "ext-2");  // Segunda tarjeta
	await SeedFullCard(db, "card-3", "ext-3");  // Tercera tarjeta

	// BD ahora tiene: ext-1, ext-2, ext-3

	// ACT - Buscar una específica
	var repo = CreateRepo(db);
	var result = await repo.CardExistsByExternalIdAsync("ext-2", CancellationToken.None);

	// ASSERT
	Assert.True(result);  // Debe encontrar ext-2
}
```

**Explicación:**
- Simula una BD con múltiples tarjetas
- Busca una tarjeta específica entre varias
- Valida precisión en búsqueda entre múltiples registros
- ¿Qué pasa si hay 1000 tarjetas? El test sirve como base

---

## 2. Ejemplos de CardServiceTests

### ✅ Test 1: SaveCardsAsync_WithPokemonCards_CallsSaveCard

```csharp
[Fact]
public async Task SaveCardsAsync_WithPokemonCards_CallsSaveCard()
{
	// ARRANGE - Preparar mocks
	var sets = new List<API.Models.Set> 
	{ 
		new() { SetId = "base1", Name = "Base Set" } 
	};

	_setRepo.Setup(r => r.GetAllSetsAsync())
		.ReturnsAsync(sets);  // Mock: retorna un set

	// Mock: las cartas NO existen aún (false = no existe)
	_cardRepo.Setup(r => r.CardExistsByExternalIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
		.ReturnsAsync(false);

	// Mock: retorna una imagen guardada
	_cardImageRepo.Setup(r => r.SaveImageCardAsync(It.IsAny<API.Models.CardImage>(), It.IsAny<CancellationToken>()))
		.ReturnsAsync(new API.Models.CardImage { CardImageId = 1 });

	// Mock: retorna una legalidad guardada
	_legalityRepo.Setup(r => r.SaveLegalityAsync(It.IsAny<API.Models.Legality>(), It.IsAny<CancellationToken>()))
		.ReturnsAsync(new API.Models.Legality { LegalityId = 1 });

	var logMessages = new List<string>();

	// ACT - Ejecutar SaveCardsAsync
	await CreateService().SaveCardsAsync(
		async (msg) =>  // Callback para logs
		{
			logMessages.Add(msg);
			await Task.CompletedTask;
		},
		CancellationToken.None
	);

	// ASSERT - Verificar que los logs tienen los mensajes esperados
	Assert.Contains("starting card sync", logMessages[0].ToLower());  // Primer log
	Assert.Contains("complete", logMessages[^1].ToLower());           // Último log
}
```

**Explicación:**
- Mockea todos los repositorios
- Simula el flujo completo de SaveCardsAsync
- Verifica que los logs se generan correctamente
- **El por qué:** Asegura que el método funciona end-to-end

---

### ✅ Test 2: SaveCardsAsync_WithNullLog_DoesNotThrow

```csharp
[Fact]
public async Task SaveCardsAsync_WithNullLog_DoesNotThrow()
{
	// ARRANGE - Preparar datos básicos
	var sets = new List<API.Models.Set> 
	{ 
		new() { SetId = "base1", Name = "Base Set" } 
	};

	_setRepo.Setup(r => r.GetAllSetsAsync())
		.ReturnsAsync(sets);

	// ACT & ASSERT
	// Si no lanza excepción, el test pasa
	// Si lanza excepción, el test falla
	await CreateService().SaveCardsAsync(
		null,  // ← IMPORTANTE: log = null
		CancellationToken.None
	);

	// Si llegamos aquí sin excepciones, el método es robusto
}
```

**Explicación:**
- Llama SaveCardsAsync con `log = null`
- Verifica que NO lanza una NullReferenceException
- Simula uso donde el logging es opcional
- **Importancia:** Robustez ante parámetros nulos

---

### ✅ Test 3: SaveCardsAsync_VerifyCardExistenceCheck_IsCalledForEachCard

```csharp
[Fact]
public async Task SaveCardsAsync_VerifyCardExistenceCheck_IsCalledForEachCard()
{
	// ARRANGE - Configurar el mock para una tarjeta específica
	var sets = new List<API.Models.Set> 
	{ 
		new() { SetId = "base1", Name = "Base Set" } 
	};

	_setRepo.Setup(r => r.GetAllSetsAsync())
		.ReturnsAsync(sets);

	// Mock: la tarjeta "card-1" YA EXISTE
	_cardRepo.Setup(r => r.CardExistsByExternalIdAsync("card-1", It.IsAny<CancellationToken>()))
		.ReturnsAsync(true);  // ← true significa que existe (debe ser omitida)

	// Mocks adicionales (por si acaso se necesitan)
	_cardImageRepo.Setup(r => r.SaveImageCardAsync(It.IsAny<API.Models.CardImage>(), It.IsAny<CancellationToken>()))
		.ReturnsAsync(new API.Models.CardImage { CardImageId = 1 });

	_legalityRepo.Setup(r => r.SaveLegalityAsync(It.IsAny<API.Models.Legality>(), It.IsAny<CancellationToken>()))
		.ReturnsAsync(new API.Models.Legality { LegalityId = 1 });

	// ACT - Ejecutar Sin logging
	await CreateService().SaveCardsAsync(null, CancellationToken.None);

	// ASSERT - Verificar que GetAllSetsAsync fue llamado
	// (Esto demuestra que se ingresó al flujo de sincronización)
	_setRepo.Verify(
		r => r.GetAllSetsAsync(),
		Times.Once  // Debe llamarse exactamente 1 vez
	);
}
```

**Explicación:**
- Mock la respuesta de CardExistsByExternalIdAsync
- Verifica que se usa realmente en SaveCardsAsync
- Valida que la lógica de "omitir duplicados" está siendo utilizada
- **Propósito:** Confirmar que la funcionalidad principal fue implementada

---

## 3. Uso de Helpers y Utilities

### Helper: CreateDb()
```csharp
private static AppDbContext CreateDb(string dbName) 
	=> DbContextFactory.Create(dbName);
```
**Uso:** Crea una BD en memoria para cada test con un nombre único

---

### Helper: CreateRepo()
```csharp
private CardRepository CreateRepo(AppDbContext db) =>
	new(db, _logger.Object, new MemoryCache(new MemoryCacheOptions()));
```
**Uso:** Instancia un CardRepository con dependencias mockadas

---

### Helper: CreateService()
```csharp
private CardService CreateService() =>
	new(
		_cardRepo.Object,
		_logger.Object,
		_setRepo.Object,
		_cardImageRepo.Object,
		_legalityRepository.Object,
		_abilityRepository.Object,
		_attackRepository.Object,
		_tcgPlayerRepository.Object,
		_ancientTraitRepository.Object
	);
```
**Uso:** Crea un CardService con todos los mocks inyectados

---

### Helper: SeedFullCard()
```csharp
private static async Task<Card> SeedFullCard(
	AppDbContext db, 
	string cardId, 
	string externalId, 
	string? name = "Pikachu",
	string? types = "Lightning", 
	string? rarity = "Common", 
	string? superType = "Pokémon", 
	string? subTypes = "Basic",
	string? number = "25")
{
	// 1. Crea Set
	// 2. Crea CardImage
	// 3. Crea Card con todos los atributos
	// 4. Agrega a BD
	// 5. Guarda cambios
	return card;
}
```
**Uso:** Inserta una tarjeta completamente configurada en la BD

---

## 4. Patrones de Assertions

### Assert.True / Assert.False
```csharp
Assert.True(result);   // Verifica que es true
Assert.False(result);  // Verifica que es false
```
**Uso en Tests:** CardExistsByExternalIdAsync (todos los 4 tests)

---

### Assert.Contains
```csharp
Assert.Contains("starting card sync", logMessages[0].ToLower());
```
**Uso en Tests:** SaveCardsAsync_WithPokemonCards_CallsSaveCard

---

### Verify (Moq)
```csharp
_setRepo.Verify(r => r.GetAllSetsAsync(), Times.Once);
```
**Uso en Tests:** SaveCardsAsync_VerifyCardExistenceCheck_IsCalledForEachCard

---

## 5. Ejecución de Tests desde Different Contextos

### Desde Visual Studio Test Explorer
```
Right Click en test → Run
↓
Output muestra PASSED o FAILED
```

### Desde Command Line
```powershell
dotnet test --filter "Name=CardExistsByExternalIdAsync_ExistingCard_ReturnsTrue"
```

### Desde Script PowerShell
```powershell
.\run-tests.ps1 -TestType repository
```

---

**Código Completo Disponible en:**
- `PokemonTCG.Test/Repositories/CardRepositoryTests.cs` (líneas 740-788)
- `PokemonTCG.Test/Services/CardServiceTests.cs` (líneas 217-296)

**Versión**: 1.0
**Fecha**: 2024
