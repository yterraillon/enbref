# ListRow

Ligne d'une section groupée : libellé à gauche, un accessoire à droite.

- **Fournir** : `title` et, au choix, `value` (pastille `fill-tertiary`, chiffres tabulaires), `action` (texte `accent`), `trailing` (un `Switch`), `chevron` (navigation), `onMoveUp` / `onMoveDown` (réordonnancement), `subtitle` (passe la ligne à 60 px).
- Hauteurs : `row-height` 52 px, `row-height-compact` 48 px, `row-height-tall` 60 px. Inset `space-18`.
- Séparateur `hairline` `separator` entre deux lignes, démarrant à l'inset (jamais bord à bord).
- `dimmed` : élément masqué, libellé à 40 % ; l'interrupteur reste lisible.
- Flèches de réordonnancement en `glyph-secondary`, désactivées à 20 % en début et fin de liste.
- Un seul accessoire principal par ligne, plus éventuellement les flèches de réordonnancement.
