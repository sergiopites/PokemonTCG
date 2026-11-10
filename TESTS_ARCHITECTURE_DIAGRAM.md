# 🏗️ ARQUITECTURA DE TESTS - DIAGRAMA VISUAL

## 📐 Estructura General del Proyecto

```
PokemonTCG/
├── PokemonTCG.API/
│   ├── Repositories/
│   │   ├── ICardRepository.cs ← MODIFICADO (+ método)
│   │   └── CardRepository.cs ← MODIFICADO (+ implementación)
│   │
│   └── Services/
│       └── CardService.cs ← MODIFICADO (lógica de verificación)
│
└── PokemonTCG.Test/
	├── Repositories/
	│   └── CardRepositoryTests.cs ← AGREGADOS 4 TESTS
	│
	└── Services/
		└── CardServiceTests.cs ← AGREGADOS 3 TESTS
```

---

## 🔄 Flujo de Verificación de Duplicados

```
SaveCardsAsync()
	↓
GetAllSetsAsync() ──→ Obtiene lista de sets
	↓
Para cada SET:
	↓
GetPokemonCardBySetIdAsync() ──→ Obtiene tarjetas externas
	↓
Para cada TARJETA:
	├─ CardExistsByExternalIdAsync() ──→ VERIFICA SI EXISTE ✅
	│  │
	│  ├─ SI EXISTE
	│  │  └─ Salta la tarjeta (skippedCount++)
	│  │
	│  └─ NO EXISTE
	│     └─ Guarda la tarjeta (savedCount++)
	│
	└─ SaveCardAsync()
```

---

## 🧪 Mapa de Cobertura de Tests

```
┌─────────────────────────────────────────────────────────────┐
│                   Método: SaveCardsAsync                     │
│                                                              │
│  Validación mediante 3 Tests                                │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│ 1️⃣  SaveCardsAsync_WithPokemonCards_CallsSaveCard          │
│     ├─ Verifica ejecución sin errores                       │
│     ├─ Valida logs de inicio/fin                            │
│     └─ Llama get all sets                                   │
│                                                              │
│ 2️⃣  SaveCardsAsync_WithNullLog_DoesNotThrow                │
│     ├─ Parámetro log = null                                 │
│     └─ No lanza excepción                                   │
│                                                              │
│ 3️⃣  SaveCardsAsync_VerifyCardExistenceCheck...             │
│     ├─ Mockea CardExistsByExternalIdAsync                   │
│     └─ Verifica que se usa para validar duplicados          │
│                                                              │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│             Método: CardExistsByExternalIdAsync              │
│                                                              │
│  Validación mediante 4 Tests (Repositorio)                  │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│ 1️⃣  CardExistsByExternalIdAsync_ExistingCard_ReturnsTrue   │
│     ├─ Tarjeta con ExternalId "pika-ext-1"                 │
│     └─ Retorna true ✅                                      │
│                                                              │
│ 2️⃣  CardExistsByExternalIdAsync_NonExistentCard_False      │
│     ├─ Busca ExternalId "no-such-external-id"              │
│     └─ Retorna false ✅                                     │
│                                                              │
│ 3️⃣  CardExistsByExternalIdAsync_EmptyDatabase_ReturnsFalse │
│     ├─ Base de datos sin tarjetas                           │
│     └─ Retorna false ✅                                     │
│                                                              │
│ 4️⃣  CardExistsByExternalIdAsync_MultipleCards_Finds...     │
│     ├─ 3 tarjetas: ext-1, ext-2, ext-3                     │
│     └─ Encuentra ext-2 correctamente ✅                     │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

---

## 📊 Matriz de Escenarios vs Tests

```
┌──────────────────────┬──────────────────────────────────────────────────────┐
│     ESCENARIO        │                TEST ASOCIADO                         │
├──────────────────────┼──────────────────────────────────────────────────────┤
│ Tarjeta existe       │ CardExistsByExternalIdAsync_ExistingCard_Returns...  │
│                      │ SaveCardsAsync_VerifyCardExistenceCheck_IsCalledFor..│
├──────────────────────┼──────────────────────────────────────────────────────┤
│ Tarjeta no existe    │ CardExistsByExternalIdAsync_NonExistentCard_Returns..│
│                      │ SaveCardsAsync_WithPokemonCards_CallsSaveCard        │
├──────────────────────┼──────────────────────────────────────────────────────┤
│ BD vacía             │ CardExistsByExternalIdAsync_EmptyDatabase_ReturnsFalse
├──────────────────────┼──────────────────────────────────────────────────────┤
│ Múltiples tarjetas   │ CardExistsByExternalIdAsync_MultipleCards_FindsCorrect
├──────────────────────┼──────────────────────────────────────────────────────┤
│ Log nulo             │ SaveCardsAsync_WithNullLog_DoesNotThrow              │
├──────────────────────┼──────────────────────────────────────────────────────┤
│ Ejecución completa   │ SaveCardsAsync_WithPokemonCards_CallsSaveCard        │
└──────────────────────┴──────────────────────────────────────────────────────┘
```

---

## 🔗 Dependencias de Tests

```
CardRepositoryTests
	│
	├─ CardExistsByExternalIdAsync_ExistingCard_ReturnsTrue
	│  ├─ Depende de: CreateDb(), SeedFullCard()
	│  └─ Valida: CardRepository.CardExistsByExternalIdAsync()
	│
	├─ CardExistsByExternalIdAsync_NonExistentCard_ReturnsFalse
	│  ├─ Depende de: CreateDb(), SeedFullCard()
	│  └─ Valida: CardRepository.CardExistsByExternalIdAsync()
	│
	├─ CardExistsByExternalIdAsync_EmptyDatabase_ReturnsFalse
	│  ├─ Depende de: CreateDb()
	│  └─ Valida: CardRepository.CardExistsByExternalIdAsync()
	│
	└─ CardExistsByExternalIdAsync_MultipleCards_FindsCorrectOne
	   ├─ Depende de: CreateDb(), SeedFullCard()
	   └─ Valida: CardRepository.CardExistsByExternalIdAsync()

CardServiceTests
	│
	├─ SaveCardsAsync_WithPokemonCards_CallsSaveCard
	│  ├─ Depende de: CreateService(), Mock repositories
	│  └─ Valida: CardService.SaveCardsAsync()
	│
	├─ SaveCardsAsync_WithNullLog_DoesNotThrow
	│  ├─ Depende de: CreateService(), Mock repositories
	│  └─ Valida: CardService.SaveCardsAsync() con log = null
	│
	└─ SaveCardsAsync_VerifyCardExistenceCheck_IsCalledForEachCard
	   ├─ Depende de: CreateService(), Mock repositories
	   └─ Valida: Uso de CardExistsByExternalIdAsync en SaveCardsAsync()
```

---

## 🎯 Integración con el Sistema Existente

```
┌─────────────────────────────────────────────────────────────────┐
│                  Tests Existentes: 40                            │
│              CardRepositoryTests: 29                             │
│              CardServiceTests: 11                                │
└─────────────────────────────────────────────────────────────────┘
							↓
				[AGREGAR 7 TESTS NUEVOS]
							↓
┌─────────────────────────────────────────────────────────────────┐
│                  Tests Totales: 47+                              │
│              CardRepositoryTests: 33 (+4) ✅                    │
│              CardServiceTests: 14 (+3) ✅                       │
└─────────────────────────────────────────────────────────────────┘
```

---

## 🔍 Detalle de Cada Test - Estructura AAA

### Tipo 1: CardRepositoryTests (Tests del Repositorio)

```
┌─ ARRANGE ────────────────────────────────┐
│ 1. CreateDb(dbName)  → Crea BD InMemory │
│ 2. SeedFullCard()    → Inserta tarjeta   │
└──────────────────────────────────────────┘
		 ↓
┌─ ACT ────────────────────────────────────┐
│ Llama CardRepository.CardExists...Async()│
└──────────────────────────────────────────┘
		 ↓
┌─ ASSERT ─────────────────────────────────┐
│ Assert.True() o Assert.False()           │
└──────────────────────────────────────────┘
```

### Tipo 2: CardServiceTests (Tests del Servicio)

```
┌─ ARRANGE ────────────────────────────────────────┐
│ 1. CreateService()                               │
│ 2. Setup() mocks para:                           │
│    - ICardRepository                             │
│    - ISetRepository                              │
│    - ICardImageRepository                        │
│    - ILegalityRepository                         │
└──────────────────────────────────────────────────┘
		 ↓
┌─ ACT ────────────────────────────────────────────┐
│ Llama CardService.SaveCardsAsync(log, token)     │
└──────────────────────────────────────────────────┘
		 ↓
┌─ ASSERT ─────────────────────────────────────────┐
│ 1. LogMessages.Contains(...)                     │
│ 2. _cardRepo.Verify(...)                         │
│ 3. No exceptions thrown                          │
└──────────────────────────────────────────────────┘
```

---

## 📈 Progresión de Compilación

```
1. Compilar PokemonTCG.API  ✅
   └─ CardRepository.cs (+ método implementado)
   └─ ICardRepository.cs (+ firma del método)
   └─ CardService.cs (+ lógica de verificación)

2. Compilar PokemonTCG.Test ✅
   └─ CardRepositoryTests.cs (+ 4 tests)
   └─ CardServiceTests.cs (+ 3 tests)

3. Ejecutar Tests ✅
   └─ 47+ tests deben pasar
   └─ 0 fallos esperados
```

---

## 🎪 Tabla Resumen Final

```
╔════════════════════════════╦═════════════╦═══════════╗
║      Test File             ║   Nuevos    ║   Total   ║
╠════════════════════════════╬═════════════╬═══════════╣
║ CardRepositoryTests.cs     ║     +4      ║     33    ║
║ CardServiceTests.cs        ║     +3      ║     14    ║
╠════════════════════════════╬═════════════╬═══════════╣
║ TOTALES                    ║     +7      ║     47+   ║
╚════════════════════════════╩═════════════╩═══════════╝
```

---

**Diagrama Generado**: 2024
**Versión**: 1.0
**Estado**: ✅ COMPLETE
