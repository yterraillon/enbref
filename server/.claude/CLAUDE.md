# CLAUDE.md — server/

Guidance pour Claude Code sur le serveur .NET d'EnBref. Voir `README.md` pour la structure, les
commandes et la configuration ; le présent fichier ne contient que ce qui oriente le travail.

## État du code

Le contenu de `src/` est une **reprise iso** du module `Modules/EnBref` du monolithe myfanwy
(septembre 2026), historique git non conservé. Rien n'a été refactoré au passage : la structure
BuildingBlocks, les noms et les choix techniques sont ceux de myfanwy.

Conséquence directe pour toute tâche ici : **ne pas prendre le code existant comme référence de
style ou de vocabulaire.** Il est la base de départ du refactoring, pas son modèle.

## Vocabulaire

`docs/ubiquitous-language.md` fait autorité et **prime sur le code**. Le code importé le contredit
sur plusieurs points (voir § 9 du glossaire) : `Section` là où le glossaire dit `Category` et
`Brief`, `RecapSectionMetric` pour ce que le glossaire appelle l'historique, `latest-recap.json` là
où l'artefact cible est `latest.json`.

Du code ou de la doc **neufs** qui reproduisent ces écarts sont bloquants. Les corriger dans le code
importé relève du chantier de refactoring, pas d'une tâche de passage.

## Architecture

Clean Architecture + CQRS via MediatR, un module par domaine :

- `Modules/<Nom>/<Nom>.Application` — modèles, contrats (`Contracts/`), features. Une feature est
  une classe statique contenant `Request`, `Response` et `Handler` (voir
  `Features/GenerateDailyRecap.cs`).
- `Modules/<Nom>/<Nom>.Infrastructure` — implémentations des contrats, jobs Quartz, accès externes.
- Chaque projet expose un `DependencyInjection.cs` avec son `Add<Nom><Couche>()`, appelé depuis
  `src/Api/App/DependencyInjection.cs`.
- `BuildingBlocks/Application` ne contient que des abstractions ; l'implémentation vit dans
  `BuildingBlocks/Infrastructure`.

Les quatre projets sont en `TreatWarningsAsErrors` : un warning casse la build.

## Pièges connus

- Le namespace `Api.EnBref` (contrôleur) masque le namespace racine `EnBref` : dans `src/Api`,
  qualifier en `global::EnBref.…`.
- `LoadModules(isDevelopment:)` dans `src/Api/App/DependencyInjection.cs` est **mal nommé** — la
  valeur passée est `!IsDevelopment()`, c'est-à-dire « tourne en conteneur ». Iso myfanwy.
- `GET /api/enbref/en-bref` déclenche une génération **et publie** sur le CDN de production. Ce
  n'est pas un endpoint de lecture ; ne jamais l'appeler pour « voir le récap ».
- Les versions NuGet sont centralisées : ajouter un `<PackageVersion>` dans
  `Directory.Packages.props`, jamais de version dans un `.csproj`.
- AutoMapper reste en 14.0.0 (dernière version MIT) ; l'avis de sécurité est supprimé
  explicitement dans `Directory.Build.props`.
