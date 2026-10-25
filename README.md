# Garage Camion

Application console C# de gestion d'un garage, réalisée avec Entity Framework Core et PostgreSQL.

Le programme importe les clients et voitures des fichiers CSV, les enregistre en base de données puis permet de consulter le parc, gérer les ventes et calculer le chiffre d'affaires hebdomadaire.

## Fonctionnalités

- import idempotent de `client.csv` et `voiture.csv` ;
- relation plusieurs-à-plusieurs entre les clients et les voitures ;
- consultation des listes de clients et de voitures ;
- fiches détaillées d'un client et d'une voiture ;
- vente d'une voiture disponible à un client ;
- échange de la voiture vendue d'un client contre une voiture disponible ;
- calcul du chiffre d'affaires HT de la semaine en cours ;
- migrations Entity Framework Core appliquées automatiquement au lancement.

## Règles métier

- Une voiture disponible peut être vendue à un client seulement une fois.
- Un client ne peut pas posséder plus de deux voitures.
- Lors d'un échange, le client quitte son ancienne voiture et reçoit une voiture disponible. Si l'ancienne voiture n'a plus de client, elle redevient disponible.
- Le chiffre d'affaires correspond à la somme HT des voitures vendues depuis le lundi de la semaine en cours jusqu'au moment de la consultation.
- Les CSV indiquent quelles voitures sont vendues, mais pas leur propriétaire. À l'import, les voitures vendues sont donc attribuées de manière déterministe : voitures triées par marque/modèle, clients triés par e-mail, sans dépasser deux voitures par client.

## Prérequis

- .NET SDK 10 ;
- Docker Desktop avec Docker Compose ;
- PostgreSQL lancé avec le fichier `compose.yaml` fourni.

## Démarrage sous Windows PowerShell

Depuis la racine du dépôt :

```powershell
Copy-Item .env.example .env
# Modifier ensuite POSTGRES_PASSWORD dans .env.
docker compose up -d

$env:ConnectionStrings__Garage = "Host=localhost;Port=5432;Database=camion;Username=camion;Password=VOTRE_MOT_DE_PASSE"
dotnet run --project .\Camion\Camion\Camion.csproj
```

Au premier lancement, les migrations sont appliquées et les CSV sont importés. Les lancements suivants ne créent pas de doublons.

Pour arrêter PostgreSQL :

```powershell
docker compose down
```

Les mots de passe restent dans `.env` ou dans la variable d'environnement `ConnectionStrings__Garage` : ils ne doivent jamais être ajoutés au dépôt.

## Menu

```text
1. Afficher les clients
2. Afficher les voitures
3. Consulter la fiche d'un client
4. Consulter la fiche d'une voiture
5. Vendre une voiture
6. Échanger une voiture
7. Afficher le chiffre d'affaires hebdomadaire
0. Quitter
```

Les opérations sur une voiture utilisent son identifiant, affiché dans la liste des voitures. La recherche d'un client utilise son e-mail, qui est unique en base.

## Tests

```powershell
dotnet test .\Camion\Camion.slnx --configuration Release
```

Les tests couvrent notamment l'import CSV, le contexte Entity Framework Core, le menu, les ventes, les échanges, la limite de deux voitures et le calcul du chiffre d'affaires.

## Organisation

```text
Camion/
├── Camion/                 # Application console
│   ├── Data/               # CSV, contexte EF Core et migrations
│   ├── Models/             # Entités métier
│   └── Services/           # Import, menu, ventes et statistiques
└── Camion.Tests/           # Tests automatisés xUnit
```
