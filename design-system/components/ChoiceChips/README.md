# ChoiceChips

Grille de puces pour choisir une valeur parmi une liste courte et connue (heure du récap).

- **Fournir** : `options`, `value` + `onChange` ou `defaultValue`, `columns` (3 par défaut), `label`.
- Puce 40 px de haut, rayon `radius-lg`, fond `chip-idle` ; puce choisie `chip-selected` avec texte `on-accent`. Chiffres tabulaires.
- S'ouvre sous la ligne qui l'appelle (« Heure du récap ») et se referme après le choix.
- Heures au format `7:30` dans la puce ; dans les phrases, écrire « 7 h 30 ».
