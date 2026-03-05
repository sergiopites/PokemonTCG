# PokemonTCG - Guía de Instalación y Configuración Local

Este documento proporciona instrucciones paso a paso para descargar, configurar y ejecutar el proyecto PokemonTCG localmente con Docker y base de datos SQL Server.

## ?? Requisitos Previos

Antes de comenzar, asegúrate de tener instalado lo siguiente en tu sistema:

- [Git](https://git-scm.com/downloads) (versión 2.0 o superior)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (versión 4.0 o superior)
- [.NET SDK 9.0](https://dotnet.microsoft.com/download/dotnet/9.0) (solo si deseas ejecutar sin Docker)
- [Node.js](https://nodejs.org/) (versión 18 o superior) y npm (solo si deseas ejecutar sin Docker)

## ?? Paso 1: Clonar el Repositorio

Abre una terminal o línea de comandos y ejecuta:

```bash
git clone https://github.com/sergiopites/PokemonTCG.git
cd PokemonTCG
```

Cambia a la rama de desarrollo:

```bash
git checkout development
```

## ?? Paso 2: Ejecutar con Docker (Recomendado)

### 2.1. Iniciar los Contenedores

Desde el directorio raíz del proyecto, ejecuta:

```bash
docker-compose up --build
```

Este comando construirá y levantará tres contenedores:
- **pokemontcg_db**: SQL Server 2022 (puerto 1433)
- **pokemontcg_api**: API .NET 9 (puerto 8080)
- **pokemontcg_client**: Cliente React con Vite (puerto 3000)

### 2.2. Aplicar las Migraciones de Base de Datos

Una vez que los contenedores estén en ejecución, abre una nueva terminal y ejecuta:

```bash
docker exec -it pokemontcg_api dotnet ef database update
```

Si prefieres ejecutar las migraciones desde tu máquina local (requiere .NET SDK 9.0):

```bash
cd PokemonTCG.API
dotnet ef database update
```

### 2.3. Verificar que los Servicios están Funcionando

- **API**: Abre tu navegador en [http://localhost:8080/swagger](http://localhost:8080/swagger)
- **Cliente Web**: Abre tu navegador en [http://localhost:3000](http://localhost:3000)
- **Base de Datos**: Conéctate con SQL Server Management Studio o Azure Data Studio:
  - Server: `localhost,1433`
  - User: `sa`
  - Password: `Admin2025!`
  - Database: `PokemonTCG`

### 2.4. Detener los Contenedores

Para detener los contenedores:

```bash
docker-compose down
```

Para detener y eliminar los volúmenes (¡esto eliminará los datos de la base de datos!):

```bash
docker-compose down -v
```

## ??? Paso 3: Ejecutar sin Docker (Alternativa)

### 3.1. Configurar SQL Server

Instala SQL Server localmente o usa una instancia existente. Luego, actualiza la cadena de conexión en `PokemonTCG.API\appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=PokemonTCG;User=sa;Password=TuPassword;TrustServerCertificate=True"
  }
}
```

### 3.2. Aplicar las Migraciones

```bash
cd PokemonTCG.API
dotnet ef database update
```

Si no tienes instalado `dotnet-ef`, instálalo primero:

```bash
dotnet tool install --global dotnet-ef
```

### 3.3. Ejecutar la API

```bash
cd PokemonTCG.API
dotnet run
```

La API estará disponible en [http://localhost:5000](http://localhost:5000) o el puerto configurado.

### 3.4. Ejecutar el Cliente React

En una nueva terminal:

```bash
cd PokemonTCG.Web.Client
npm install
npm run dev
```

El cliente estará disponible en [http://localhost:5173](http://localhost:5173) (puerto por defecto de Vite).

## ?? Estructura del Proyecto

```
PokemonTCG/
??? PokemonTCG.API/          # API REST en .NET 9
?   ??? Controllers/         # Controladores de la API
?   ??? Data/                # DbContext y configuración EF Core
?   ??? Migrations/          # Migraciones de base de datos
?   ??? Repositories/        # Capa de datos
?   ??? Services/            # Lógica de negocio
?   ??? appsettings.json     # Configuración de la API
??? PokemonTCG.Web.Client/   # Cliente React con Vite
?   ??? src/
?   ?   ??? pages/           # Páginas de la aplicación
?   ?   ??? App.jsx          # Componente principal
?   ??? package.json
??? PokemonTCG.SDK/          # SDK para consumir la API de PokemonTCG
??? PokemonTCG.PDFPrinter/   # Generador de PDFs para cartas
??? docker-compose.yml       # Configuración de Docker Compose
??? Dockerfile.api           # Dockerfile para la API
```

## ?? Comandos Útiles

### Migraciones de Entity Framework

Crear una nueva migración:
```bash
cd PokemonTCG.API
dotnet ef migrations add NombreDeLaMigracion
```

Aplicar migraciones:
```bash
dotnet ef database update
```

Revertir a una migración anterior:
```bash
dotnet ef database update NombreDeLaMigracionAnterior
```

Eliminar la última migración:
```bash
dotnet ef migrations remove
```

### Docker

Ver logs de un contenedor específico:
```bash
docker logs pokemontcg_api
docker logs pokemontcg_client
docker logs pokemontcg_db
```

Reconstruir un contenedor específico:
```bash
docker-compose up --build api
```

Acceder al shell de un contenedor:
```bash
docker exec -it pokemontcg_api /bin/bash
```

## ?? Solución de Problemas

### La base de datos no se crea automáticamente

Verifica que las migraciones se hayan aplicado correctamente:
```bash
docker exec -it pokemontcg_api dotnet ef database update
```

### Error de conexión a la base de datos

- Verifica que el contenedor de SQL Server esté en ejecución: `docker ps`
- Asegúrate de que la contraseña cumple con los requisitos de complejidad de SQL Server
- Espera unos segundos después de iniciar Docker Compose para que SQL Server termine de inicializarse

### Puerto ya en uso

Si algún puerto (1433, 8080, 3000) está ocupado, puedes modificar los puertos en `docker-compose.yml`:

```yaml
ports:
  - "TU_PUERTO:PUERTO_INTERNO"
```

### El cliente React no se conecta a la API

Verifica la configuración de CORS en la API y la URL base en el cliente React.

## ?? Recursos Adicionales

- [Documentación de la API](http://localhost:8080/swagger)
- [Repositorio en GitHub](https://github.com/sergiopites/PokemonTCG)
- [Documentación de Docker](https://docs.docker.com/)
- [Documentación de Entity Framework Core](https://docs.microsoft.com/ef/core/)

## ?? Contribuir

Si deseas contribuir al proyecto:

1. Haz un fork del repositorio
2. Crea una rama para tu feature (`git checkout -b feature/nueva-funcionalidad`)
3. Haz commit de tus cambios (`git commit -am 'Agrega nueva funcionalidad'`)
4. Haz push a la rama (`git push origin feature/nueva-funcionalidad`)
5. Abre un Pull Request

## ?? Licencia

Este proyecto es de código abierto y está disponible bajo la licencia especificada en el repositorio.
