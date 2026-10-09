# Button

Bouton d'action : la pilule d'encre de 50 px pour l'action principale d'un écran, ou une action texte sans fond.

- **Fournir** : `children` (verbe à l'infinitif ou nom court : « Écouter le récap », « Pause »), `onClick`, éventuellement `icon` (`play`, `pause`…) et `block` pour occuper toute la largeur d'une carte.
- `variant="primary"` (défaut) : fond `accent`, texte `on-accent`, style `headline`, rayon `radius-full`, hauteur `button-height`. Une seule par écran ou par carte.
- `variant="plain"` : texte `accent` en `subhead` 600, pour une action de fin de ligne (« Activer », « Relire »).
- Ne pas teinter le bouton d'une autre couleur : l'encre est le seul accent du système.
- Web admin : même composant ; le focus clavier affiche l'anneau `focus-ring`.
