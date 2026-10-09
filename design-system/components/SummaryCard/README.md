# SummaryCard

Carte d'en-tête du récap : progression de lecture, durée, bouton d'écoute.

- **Fournir** : `read`, `total`, `minutes`, `segments`, `playing`, `onPlay` ; `playLabel` pour remplacer « Écouter le récap » / « Pause ».
- Ligne du haut : « 2 sur 7 rubriques lues » en `headline`, durée en `subhead` `label-secondary`.
- Contient un `ProgressSegments` puis un `Button` primaire pleine largeur.
- Toujours la première carte de l'écran Aujourd'hui, juste sous le grand titre.
