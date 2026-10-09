# TextField

Ajout pour l'admin web : champ de saisie mono-ligne avec libellé, aide et erreur.

- **Fournir** : `label`, `value` / `defaultValue`, `onChange`, `placeholder`, `hint` ou `error`.
- Champ 40 px, fond `fill-tertiary`, rayon `radius-sm`, texte `body`. Libellé et aide en `footnote` `label-secondary`.
- Erreur : bordure et message en `danger` ; le message dit quoi faire (« utilisez le format 7:30 »).
- Placer dans une `ListSection` ou une carte `surface`. Sur iOS, préférer un `TextField` natif dans un `Form`.
