# ProgressSegments

Barre de progression découpée en un segment par rubrique.

- **Fournir** : `segments` (un état par rubrique visible, dans l'ordre : `done`, `current`, `todo`) et `label` (« 2 sur 7 rubriques lues »).
- Segments de 4 px (`progress-height`), écart `space-4`, rayon `radius-xs`. Couleurs `progress-done`, `progress-current`, `progress-track`. Transition 0,3 s.
- Le nombre de segments suit les rubriques visibles : une rubrique masquée disparaît de la barre.
