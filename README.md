# FizzBuzz Full Stack

API REST **ASP.NET Core (.NET 10)** et interface **Angular 22** générant des séquences FizzBuzz personnalisables, conçues en **Clean Architecture**, testées et conteneurisées.

---

## 1. Présentation fonctionnelle

Pour chaque nombre de `1` à `limit` (inclus), dans l'ordre croissant :

| Condition | Valeur retournée |
|---|---|
| multiple de `int1` uniquement | `str1` |
| multiple de `int2` uniquement | `str2` |
| multiple des deux | `str1 + str2` |
| sinon | le nombre en texte |

```http
GET /api/v1/fizzbuzz?int1=3&int2=5&limit=15&str1=Fizz&str2=Buzz
```
```json
["1","2","Fizz","4","Buzz","Fizz","7","8","Fizz","Buzz","11","Fizz","13","14","FizzBuzz"]
```

L'interface web propose un formulaire validé, affiche les résultats (défilement virtuel jusqu'à 10 000 lignes) et des messages d'erreur clairs. **Le calcul est toujours fait par le backend.**

---

## 2. Architecture

```text
            ┌────────────────────────────────────────────┐
 Navigateur │ Angular (Nginx)  ── /api ──►  FizzBuzz.Api │  Présentation : HTTP, validation de format,
            └────────────────────────────────────┬───────┘  ProblemDetails, logs, santé, rate limiting
                                                 │ IGenerateFizzBuzzUseCase
                                   ┌─────────────▼──────────────┐
                                   │   FizzBuzz.Application     │  Cas d'utilisation, validation des
                                   │   Features/GenerateFizzBuzz│  entrées, limite configurable
                                   └─────────────┬──────────────┘
                                                 │
                                   ┌─────────────▼──────────────┐
                                   │      FizzBuzz.Domain       │  FizzBuzzRule, FizzBuzzGenerator
                                   │  (aucune dépendance)       │  invariants métier, algorithme pur
                                   └────────────────────────────┘
```

Les dépendances pointent uniquement vers l'intérieur :

| Projet | Références | Dépendances externes |
|---|---|---|
| `FizzBuzz.Domain` | — | aucune |
| `FizzBuzz.Application` | Domain | `Microsoft.Extensions.Options.*` (options + validation) |
| `FizzBuzz.Api` | Application | `Microsoft.AspNetCore.OpenApi` |
| `FizzBuzz.UnitTests` | Domain, Application | xUnit |
| `FizzBuzz.IntegrationTests` | Api | xUnit, `Microsoft.AspNetCore.Mvc.Testing` |

Pas de projet `Infrastructure` : aucune dépendance externe (base de données, file, service tiers) ne le justifie.

### Responsabilités et flux de validation

1. **Api – `FizzBuzzRequest`** : vérifie uniquement la *présence* et le *format* (paramètre manquant, `abc`, `1.5`, dépassement d'`int`). Erreur → 400 automatique via `[ApiController]`.
2. **Application – `GenerateFizzBuzzQueryValidator`** : vérifie les *règles* (diviseurs > 0, `limit` entre 1 et la limite configurée, chaînes non vides ≤ 50 caractères) et **retourne toutes les erreurs** dans un `Result<T>` (pas d'exception pour un cas attendu).
3. **Domain – `FizzBuzzRule` / `FizzBuzzGenerator`** : garantit les invariants par des clauses de garde (`ArgumentException`). Si elles se déclenchent, c'est un bug → 500 générique.

Le contrôleur convertit les erreurs applicatives (`Int1`) en noms de paramètres HTTP (`int1`) : toutes les erreurs 400 ont le même format `ValidationProblemDetails`.

### Choix techniques justifiés

| Choix | Justification |
|---|---|
| `FizzBuzzGenerator` statique et pur | Fonction déterministe sans état ni dépendance : rien à injecter ni à simuler (KISS). |
| `Result<T>` plutôt qu'exception de validation | Une saisie invalide est un cas nominal ; flux explicite et sans coût d'exception. |
| Pas de MediatR / AutoMapper / FluentValidation | Un seul cas d'utilisation : la valeur ajoutée ne compense pas la complexité (YAGNI). |
| Longueur des chaînes en **points de code Unicode** | `"😀"` compte 1 caractère (2 unités UTF-16). Même règle côté Angular (`Array.from`). |
| Limite métier `10 000` (Domain) + `FizzBuzz:MaxLimit` (config) | La configuration peut **réduire** la limite ; `[Range]` + `ValidateOnStart` refusent au démarrage toute valeur hors `[1 ; 10 000]`. |
| Mécanismes natifs ASP.NET Core | `IExceptionHandler`, `ProblemDetails`, `HttpLogging`, `RateLimiter`, `HealthChecks`, `OpenApi`. |
| Proxy (Angular dev server / Nginx) | Front et API sur la même origine : pas de CORS en production. |

### Durées de vie (Dependency Injection)

| Service | Durée de vie | Raison |
|---|---|---|
| `IGenerateFizzBuzzUseCase` → `GenerateFizzBuzzUseCase` | Singleton | Sans état, thread-safe ; ne dépend que de singletons. |
| `GenerateFizzBuzzQueryValidator` | Singleton | Sans état ; lit `IOptions<FizzBuzzOptions>` (singleton) une fois. |
| `IOptions<FizzBuzzOptions>` / `IOptions<RateLimitingOptions>` | Singleton | Fournis par le framework. |
| `GlobalExceptionHandler` | Singleton | Imposé par `AddExceptionHandler`. |
| `FizzBuzzReadinessCheck` | Transient | Défaut de `AddCheck<T>` ; dépend d'un singleton (pas de dépendance captive). |

Aucun service *Scoped* : aucun état par requête n'est nécessaire.

---

## 3. Structure du dépôt

```text
FizzBuzz
├── FizzBuzz.Domain/                 FizzBuzzRule, FizzBuzzGenerator
├── FizzBuzz.Application/
│   ├── Common/Result.cs
│   ├── Features/GenerateFizzBuzz/   Query, IGenerateFizzBuzzUseCase, UseCase, QueryValidator
│   ├── FizzBuzzOptions.cs
│   └── DependencyInjection.cs       AddApplication()
├── FizzBuzz.Api/
│   ├── Contracts/FizzBuzzRequest.cs
│   ├── Controllers/FizzBuzzController.cs
│   ├── ErrorHandling/  HealthChecks/  RateLimiting/
│   ├── Program.cs                   composition root
│   └── appsettings*.json
├── FizzBuzz.UnitTests/              Domain + Application
├── FizzBuzz.IntegrationTests/       WebApplicationFactory
├── FizzBuzz.Web/                    Angular 22
│   ├── FizzBuzz.Web.esproj          projet Visual Studio (SDK JavaScript)
│   ├── src/app/core/                codec HTTP, jeton API_BASE_URL
│   ├── src/app/features/fizzbuzz/   page, formulaire, résultats, service, modèles
│   ├── src/environments/
│   ├── nginx/                       configuration Nginx + en-têtes de sécurité
│   └── proxy.conf.json              proxy /api pour ng serve
├── Directory.Build.props            nullable, analyseurs, warnings = erreurs
├── Directory.Packages.props         versions NuGet centralisées
├── Dockerfile.api  Dockerfile.web  compose.yaml  .dockerignore
└── FizzBuzz.slnx
```

---

## 4. Prérequis

| Outil | Version |
|---|---|
| .NET SDK | 10.0.100 ou supérieur (`global.json`, roll-forward sur la feature band) |
| Node.js | **^22.22.3 ou ^24.15.0** (exigence d'Angular CLI 22) |
| npm | 10+ |
| Docker / Docker Compose | Docker Engine 24+ avec Compose v2 (optionnel) |

> Avec nvm-windows : `nvm install 24` puis `nvm use 24`.

---

## 5. Installation et lancement

### Backend

```bash
dotnet restore
dotnet run --project FizzBuzz.Api --launch-profile http
```

API disponible sur `http://localhost:5001` (profil `https` : `https://localhost:7075`).
En développement, le document OpenAPI est servi sur `http://localhost:5001/openapi/v1.json` et [FizzBuzz.Api.http](FizzBuzz.Api/FizzBuzz.Api.http) contient des requêtes prêtes à l'emploi.

### Frontend Angular

```bash
cd FizzBuzz.Web
npm ci
npm start
```

Application sur `http://localhost:4200`. Le serveur de développement relaie `/api` vers `http://localhost:5001` ([proxy.conf.json](FizzBuzz.Web/proxy.conf.json)) : lancer l'API avant.

Build de production : `npm run build` (sortie dans `FizzBuzz.Web/dist/fizzbuzz-web/browser`).

### Depuis Visual Studio

`FizzBuzz.Web.esproj` rattache le frontend à la solution (dossier `src`). Pour lancer les deux ensemble :
clic droit sur la solution → **Configurer les projets de démarrage** → **Plusieurs projets de démarrage** →
`FizzBuzz.Api` et `FizzBuzz.Web` sur **Démarrer**. Exécuter `npm ci` une première fois dans `FizzBuzz.Web`.

L'installation npm et le build Angular sont volontairement exclus de MSBuild (`ShouldRunNpmInstall` /
`ShouldRunBuildScript` à `false`) : `dotnet build` et `dotnet test` sur la solution ne nécessitent pas Node.js.

---

## 6. Tests

```bash
# Backend : tests unitaires + intégration
dotnet test

# Frontend : Vitest + jsdom (aucun navigateur requis)
cd FizzBuzz.Web
npm test -- --watch=false
```

| Suite | Contenu |
|---|---|
| **Unitaires** (70) | Séquence nominale, multiples de `int1` / `int2` / des deux, nombres non remplacés, ordre de concaténation, diviseurs identiques, diviseurs = 1, limites (0, négative, 10 001, bornes), chaînes vides/blanches/trop longues, Unicode (50 emojis acceptés), use case (succès, agrégation des erreurs, limite configurée), `Result<T>`. |
| **Intégration** (26) | 200 + JSON exact, encodage `C++ & co` / `日本😀`, `limit=10000`, 400 par paramètre invalide (11 cas), paramètres manquants, format `ValidationProblemDetails` (`application/problem+json`, `traceId`), limite configurée, échec au démarrage si configuration hors bornes, 500 sans fuite d'information, 429 + `Retry-After`, health checks live/ready, en-tête `X-Trace-Id`, 404 ProblemDetails. |
| **Angular** (46) | Codec d'encodage, service HTTP (URL et encodage), traduction des erreurs (0, 400, 429, 5xx), validateurs, formulaire (valeurs initiales, messages accessibles, focus, réinitialisation, état de chargement), résultats (défilement virtuel sur 10 000 lignes), page (chargement → résultats, erreurs API, serveur injoignable, annulation à la réinitialisation). |

Le build échoue sur tout avertissement compilateur ou analyseur (`TreatWarningsAsErrors`, `AnalysisLevel=latest-recommended`, `EnforceCodeStyleInBuild`).

---

## 7. Docker

```bash
docker compose up --build        # http://localhost:8080
docker compose ps                # état des health checks
docker compose logs -f api       # logs JSON structurés
docker compose down
```

| Image | Build | Exécution |
|---|---|---|
| `Dockerfile.api` | `dotnet/sdk:10.0-alpine`, restauration en couche dédiée (cache) | `dotnet/aspnet:10.0-alpine`, utilisateur non-root `$APP_UID`, port 8080, `HEALTHCHECK` sur `/health/live` |
| `Dockerfile.web` | `node:24.22.2-alpine`, `npm ci` puis build de production | `nginx-unprivileged:1.28-alpine` (non-root, port 8080), fallback SPA, proxy `/api` → `api:8080`, cache long des bundles hashés, CSP et en-têtes de sécurité |

`compose.yaml` : l'API n'est **pas publiée** sur l'hôte (seul Nginx l'atteint) ; `web` attend que l'API soit *ready* (`depends_on: condition: service_healthy`) ; systèmes de fichiers en lecture seule, `no-new-privileges`.

---

## 8. Configuration

Toutes les clés sont surchargeables par variable d'environnement (`:` → `__`).

| Clé | Défaut | Description |
|---|---|---|
| `FizzBuzz:MaxLimit` | `10000` | Limite maximale de `limit`, dans `[1 ; 10000]` (vérifiée au démarrage). |
| `RateLimiting:PermitLimit` | `100` | Requêtes autorisées par IP et par fenêtre. |
| `RateLimiting:WindowSeconds` | `10` | Durée de la fenêtre fixe. |
| `Cors:AllowedOrigins` | `[]` (`http://localhost:4200` en Development) | Origines autorisées, inutile derrière le proxy. |
| `Https:RedirectionEnabled` | `true` | `false` dans le conteneur : TLS terminé par le reverse proxy. |
| `Logging:LogLevel:*` | voir `appsettings*.json` | Console JSON en Production, simple en Development. |
| `ASPNETCORE_FORWARDEDHEADERS_ENABLED` | `true` (compose) | Prise en compte de `X-Forwarded-For` (IP réelle pour le rate limiting). |

Variables de `compose.yaml` : `WEB_PORT`, `FIZZBUZZ_MAX_LIMIT`, `RATE_LIMIT_PERMITS`, `RATE_LIMIT_WINDOW_SECONDS`, `API_LOG_LEVEL`.

Aucun secret n'est requis ni stocké dans le dépôt.

---

## 9. Endpoints

| Méthode | Route | Réponses |
|---|---|---|
| `GET` | `/api/v1/fizzbuzz?int1&int2&limit&str1&str2` | `200` tableau JSON · `400` ValidationProblemDetails · `429` ProblemDetails · `500` ProblemDetails |
| `GET` | `/health/live` | `200` processus vivant · `503` sinon |
| `GET` | `/health/ready` | `200` génération opérationnelle (vérifiée sur une entrée connue) · `503` sinon |
| `GET` | `/openapi/v1.json` | Document OpenAPI 3.1 (Development uniquement) |

### Paramètres

| Nom | Type | Règle |
|---|---|---|
| `int1`, `int2` | entier | `> 0`, peuvent être égaux |
| `limit` | entier | `1` à `FizzBuzz:MaxLimit` (≤ 10 000) |
| `str1`, `str2` | texte | obligatoire, non blanc, ≤ 50 caractères Unicode, à encoder en URL (`+` → `%2B`) |

### Exemples

**Paramètres invalides**
```http
GET /api/v1/fizzbuzz?int1=0&int2=5&limit=20000&str1=Fizz&str2=Buzz
```
```json
HTTP/1.1 400 Bad Request
Content-Type: application/problem+json

{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "int1": ["Must be greater than 0."],
    "limit": ["Must be between 1 and 10000."]
  },
  "traceId": "00-46833dade26451e898dcee752c2dccd8-631fc4f46775fca0-00"
}
```

**Paramètre manquant ou non numérique**
```json
{ "status": 400, "errors": { "int2": ["This parameter must be a valid integer."], "str1": ["This parameter is required."] } }
```
Les erreurs de format sont renvoyées avant la validation métier (comportement standard de `[ApiController]`).

**Erreur interne** : `500` avec `"title": "An unexpected error occurred."` et un `traceId`, sans message ni pile d'exception (journalisés côté serveur uniquement).

---

## 10. Observabilité

- **Logs structurés** : `LoggerMessage` (source generator), console JSON en Production avec scopes `TraceId`/`SpanId`.
- **Durée des requêtes** : `HttpLogging` (méthode, chemin, statut, durée) ; ni query string ni corps ne sont journalisés ; sondes `/health` exclues.
- **Corrélation** : en-tête `traceparent` (W3C) accepté ; l'identifiant de trace est renvoyé dans `X-Trace-Id` et dans le `traceId` des ProblemDetails, puis affiché par l'interface en cas d'erreur.

---

## 11. Limites connues et perspectives

- **Validation en deux temps** : une requête avec un paramètre mal formé ne reçoit pas encore les erreurs métier des autres paramètres.
- **Unicode** : longueur comptée en points de code ; un graphème composé (ex. 👨‍👩‍👧) compte plusieurs caractères.
- **Rate limiting en mémoire** : par instance ; un déploiement multi-instances nécessiterait une limite au niveau de la passerelle ou un stockage distribué.
- **CSP** : `style-src 'unsafe-inline'` est requis par les styles de composants Angular ; un nonce (`ngCspNonce`) injecté par Nginx permettrait de le retirer.
- **Perspectives** : statistiques des requêtes les plus fréquentes (ajouterait un vrai besoin de persistance, donc un projet Infrastructure), OpenTelemetry (traces/métriques exportées), pipeline CI (build, tests, scan des images), tests end-to-end (Playwright), interface d'exploration OpenAPI (Scalar) en développement.
