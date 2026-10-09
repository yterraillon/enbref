# CheckCircle

Pastille ronde à cocher de 22 px suivie d'un libellé : « Marquer comme lu ».

- **Fournir** : `children` (libellé), `checked` + `onChange` ou `defaultChecked`.
- Non coché : contour 1,5 px `control-border`. Coché : disque `accent` et coche `on-accent`.
- Toujours accompagnée de son libellé (le contour seul n'atteint pas 3:1 en thème clair).
- La ligne entière (50 px) est la cible tactile.
