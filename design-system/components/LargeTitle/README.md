# LargeTitle

Barre de titre collante avec grand titre, surtitre de date et une action ronde.

- **Fournir** : `title` (« Aujourd'hui », « Réglages »), `eyebrow` optionnel (date longue en français, mise en capitales par le style `eyebrow`), `trailing` optionnel (un `IconButton`).
- Fond `material-bar` avec flou 20 px ; padding latéral `space-20`. Titre en `large-title`.
- Sur iOS, utiliser `.navigationTitle` + `.navigationBarTitleDisplayMode(.large)` ; ce composant sert l'admin web et les maquettes.
- Un seul grand titre par écran.
